# Koromo Copy 개발자 문서

## 목표

Emacs와 같이 다재 다능한 유틸리티를 만드는 것을 목표로하고 있습니다. 첫 번째 릴리즈가 끝나면 플러그인 기능을 개발할 계획입니다.

## Commit Prefix

`Koromo Copy`의 커밋은 다음 리스트 중 하나의 `Prefix`를 가집니다. 이 리스트는 삭제는 되지 않으나, 추가될 수 있습니다. 또한 여기에 없는 `Prefix`는 `hitomi`, `exh`와 같이 직관적으로 이해할 수 있는 범주들이라 보면됩니다.

```
kc: 이하 해당하지 않는 모든 커밋
tidy: 파일이동, 삭제 관련
pys: 파이썬 스크립트
kcp: 프로그램 기반
net: 네트워크 통신
fs: 파일시스템 관련
ui: User Interface관련
db: 데이터베이스 관련
update: Version 업데이트
doc: 문서 업데이트
```

---

# 콘솔 (Console)

`Koromo Copy`를 `Debug`모드로 실행하면 자동으로 `Console`이 실행됩니다. `Console`은 프로그램의 모든 IO를 캡쳐하며, 작업, 오류를 보여줍니다. 또한 미리 정의된 명령을 이용해 사용자 정의 작업을 수행할 수 있습니다. `Koromo Copy`는 하나의 `Console`만을 지원하며, 두 개 이상의 `Console`창을 열 수 없습니다.

## 콘솔 구조

`Koromo Copy`의 `Console`은 `CommandLineParser`를 가지고 있습니다. 이 파서는 사용자 입력을 분석하여 각 최상위 명령 클래스로 인자들을 보냅니다. 첫 번재 인자는 최상위 명령 클래스의 요소 이름이여야하며, 두 번째 부터 각 최상위 명령 클래스가 필요로하는 인자들을 포함해야합니다. 각 최상위 명령 클래스는 `--help` 인자를 기본으로 가집니다. 또한 하나의 파이프라인 명령을 포함하고 있습니다. 파이프라인 명령은 `>` 명령 하나이며, `A > B`와 같이 표현할 수 있습니다. `A > B` 명령어는 `A`의 Output을 `B`의 인자로 보내란 명령입니다. 또한 루프 파이프라인 명령 `|>`로 new line으로 구분된 다중 결과를 처리할 수 있습니다.

## Prompt Task

콘솔의 `Prompt`는 하나의 `Task`로 관리됩니다. 이는 콘솔의 입력 대기 상태를 빠져나오기 위함입니다.

## Command Line Parser

`CommandLineParser`는 리다이렉션된 최상위 명령 클래스에서 사용됩니다. 각 최상위 명령 클래스는 옵션 클래스를 가지고있으며, `CommandLineParser`는 이 옵션 클래스를 이용해 명령어를 분석하게 됩니다.

콘솔에서 입력받은 사용자 명령은 다음과 같은 과정을 거쳐 처리됩니다.

```
Console입력 => 각 최상위 명령 클래스 => Command Line Parser(+ Option)
    => 명령어 실행 => 파이프라인 연결 ...
```

## 명령어

다음은 `Koromo Copy Console`에서 사용가능한 명령어를 정리한 것입니다. 모든 명령어는 특정 최상위 명령 클래스와 연결됩니다. 이 리다이렉션은 `Console.cs::Loop()` 함수의 `Dictionary`에 정의되어있습니다.

| 명령어 | 설명 |
| ------|-------|
|hitomi|히토미와 관련된 모든 명령을 포함합니다.|
|exh|익헨과 관련된 모든 명령을 포함합니다.|
|pixiv|픽시브와 관련된 모든 명령을 포함합니다.|
|grep|일반문자열이나 정규표현식으로 검색가능한 grep 도구입니다.|
|out|파일 쓰기에 관련한 명령을 포함합니다.|
|in|파일 읽기(텍스트 파일)를 위한 명령입니다.|
|scan|파일 열거에 관련한 명령을 포함합니다.|
|run|특정 유틸리티 실행 명령입니다.|
|down|입력받은 Url이나, 배열의 모든 요소를 입력받은 경로 규칙에 맞게 다운로드합니다. (추가예정)|
|internal|프로그램의 인스턴스를 제어할 수 있는 명령을 포함합니다.|
|test|기능 테스트에 사용되는 명령집합입니다.|
|ux|UX/UI 테스트에 사용되는 명령집합입니다.|

기본적으로 명령어의 규칙은 `옵션`, `인자`, `지정` 이렇게 세가지로 구성됩니다.

`옵션`은 `True`나 `False` 중 하나의 상태를 가지며, 커맨드라인에 `옵션`이 포함되면 해당 옵션을 `True`로 지정합니다. `Koromo Copy`에선 일반적으로 `-a`, `-b`, `-c` 처럼 `-`와 한 글자의 알파벳이 합쳐진 형태로 선언되어있습니다. 또한 일부 최상위 명령을 제외하면, 접두사로 `-`가 하나만 존재하는 경우엔 `옵션`으로 보며, `-abc`처럼 되어있으면, `-a -b -c`로 인식하는 최상위 명령어들이 존재합니다.

`인자`는 하나 이상의 인자를 필요로합니다. 일부 최상위 명령에는 `기본 인자`가 존재하며, `기본 인자`는 생략되어도, 필요한 인자가 존재한다면 자동으로 인식됩니다. 인자는 반드시 `인자`바로뒤에 와야하며, `-`나, `--`로 시작할 수 없습니다.

`지정`은 미리 정해진 옵션들 중 사용자가 선택할 수 있게끔 선택지를 제공합니다. `-type=small`, `-type="big"`처럼 쓰이며, `-`나 `--`옵션뒤에 `=`를 붙히고, 특정 옵션을 지정하면됩니다.

명령어들을 리스팅하려면 `help` 커맨드를 입력하세요. 각 명령의 인스턴스를 확인하려면, `internal --get console.redirections -R`를 통해 원하는 명령의 인스턴스 정보를 가져온뒤, `internal -eEPS <latest>`를 통해 인스턴스 함수를 나열하세요.

## hitomi 명령 예제

```
1. 특정 작품의 정보를 얻고 싶을 경우
hitomi -article 1234

2. 특정 작품의 이미지 정보를 얻고 싶을 경우
hitomi -image 1234

3. 특정 작품의 썸네일 정보를 얻고 싶을 경우
hitomi -image 1234 -type=small
hitomi -image 1234 -type=big

4. 메타데이터/히든데이터를 다운로드하고 싶을 경우
hitomi -downloadmetadata
hitomi -downloadhidden
* 히든데이터 다운로드는 단독으로 불가능하며, 메타데이터가 로드된 상태여야함

5. 메타데이터/히든데이터를 로드하고 싶을 경우
hitomi -loadmetadata
hitomi -loadhidden
hitomi -load
* 히든데이터 로드는 단독으로 불가능하며, 메타데이터가 로드된 상태여야함

6. 데이터를 동기화하고 싶을 경우
hitomi -sync

7. 검색하고 싶을 경우
hitomi -search "artist:hisasi lang:korean"

8. 자세한 검색결과를 얻고싶을 경우
hitomi -search "artist:hisasi lang:korean" -all

9. 특정 검색 토큰을 고정시키고 싶을 경우
hitomi -setsearch "lang:korean"
hitomi -search "artist:hisasi" -all

10. 특정 작품을 다운로드하고 싶을 경우
hitomi -image 1234 > down --out "C:\Hitomi"
```

## exh 명령 예제

```
1. 특정 작품의 정보를 가져오고 싶을 경우
exh -article https://...

2. 히토미 아티클 정보로 Url을 가져오고 싶을 경우
hitomi -article 1234 > exh -addr
```

## pixiv 명령 예제

```
1. 픽시브 로그인 (필수)
pixiv -login id pw

2. 픽시브 유저목록을 가져오고 싶을 경우
pixiv -user 4

3. 픽시브 유저의 이미지 목록을 가져오고 싶을 경우
pixiv -image 4
```

## internal 명령 예제

```
1. 열린 창을 보고싶을 경우
internal -e

2. 모든 인스턴스를 보고싶을 경우
internal -eI

3. 어떤 인스턴스의 모든 함수를 열거하고 싶을 경우
internal -eEP console

4. 어떤 인스턴스의 어떤 함수를 호출하고 싶을 경우
internal --call console.Write "asdf"
```

---

# 소스코드 구조

## ILazy

`Koromo Copy` 구현에 주로 쓰인 인스턴스 레이지(`ILazy`)는 상속된 클래스의 레이지 인스턴스를 생성합니다. 이렇게 생성된 레이지 인스턴스는 외부에서 쉽게 접근할 수 있습니다.

## Internal

`Internal` 클래스는 `Koromo Copy`에 구현된 `Memory Model`를 쉽게 다룰 수 있는 메서드 집합입니다. 변수의 상태, 함수 목록, 함수 호출, 반환값 수정 등 여러가지 기능이 구현되어있습니다.

## Monitor

`Hitomi Copy`에서 모든 동작을 기록하기 위해 만들었던 도구입니다. `Koromo Copy`에서 `Monitor`는 모든 동작을 기록하며, 콘솔을 제어합니다.

## Version

`Version` 클래스는 `Koromo Copy`의 버전관리 클래스입니다. 이 클래스는 최신 업데이트를 체크하며, 웹에 등록된 정보(공지사항 등)의 정보를 가져옵니다.

## 콘솔

콘솔은 `Koromo Copy`의 모든 기능을 시험하고, 검사하기 위해 만들어진 도구입니다. `Monitor`에서 콘솔을 동적으로 로드할 수 있습니다.

## 플러그인

`Koromo Copy`에선 오픈소스 라이브러리인 `Sps` (https://www.codeproject.com/Articles/182970/A-Simple-Plug-In-Library-For-NET)를 사용하여 플러그인 시스템을 구현하였습니다. 플러그인의 유형은 `None`, `Download`, `Utility`, `Console`, `Helper`로 총 다섯 가지 입니다.

플러그인의 각 유형을 Prefix로하는 `XXXPlugin` 클래스가 있습니다. 이 클래스들은 `Koromo Copy`와 상호작용할 때 유용합니다. 메인 레포지토리의 `Plugins/`를 참고하여 새로운 플러그인을 제작해보세요.

플러그인 개발자는 `Koromo Copy Base.dll` 및 `Koromo Copy Plugin.dll`를 참조하여 플러그인을 개발해야 합니다. 다만, 플러그인 배포시에는 참조한 파일은 같이 배포하지 않아도됩니다.

## 다운로드

`Net/DownloadQueue.cs` 코드가 파일다운로드를 총괄하는 클래스입니다. 이 클래스는 지정된 개수의 태스크를 생성해 다운로드를 진행합니다. 다운로드 큐는 일시적인 선점이 가능합니다. 선점 기능을 이용하면 일시적으로 모든 바이트블록 갱신을 멈추고 재활성화될 때까지 기다립니다.

`Net/EmiliaQueue.cs` 코드는 `DownloadQueue`와는 달리 `Thread`를 사용하여 구현한 클래스입니다. 사용방법은 `DownloadQueue`와 동일합니다.

`Net/DownloadGroup.cs`는 일괄 다운로드 클래스입니다. `Net/DownloadQueue.cs`를 이용하여 구현되었습니다. 이 클래스를 이용하면 여러 파일을 그룹화하여 다운로드시킬 수 있습니다.

## 고급 검색 (Advanced Search)

`Advanced Search` 기능이 `Koromo Copy 0.4` 버전부터 추가됩니다. 이 검색 방법은 집합연산을 사용합니다. 가령, `artist:michiking - (male:shota lang:japanes)`를 검색하면, `michiking` 작가의 작품 중 `male:shota`태그가 없고, 일본어가 아닌 작품들을 모두 가져옵니다. 집합 연산 중 차집합 연산은 사칙연산이 아니기 때문에 다음과 같은 해석의 오류가 있을 수 있습니다. 가령, `artist:michiking - (male:shota - lang:korean)`를 검색할 때 이 검색어를  `artist:michiking - male:shota + lang:korean`와 동치로 보면 안됩니다. 정확한 해석은 `michiking` 작가의 작품 중 `male:shota` 태그가 없거나, 언어가 한국어인 작품을 가져오는 것입니다.

중첩가능한 최대 토큰 수는 30개 입니다. 이를 수정하려면 `to_linear` 함수에서 `querys` 배열 크기를 늘리세요.

---

# 프로젝트 분리

`Koromo Copy`는 프로그램 크기를 줄이기 위해 `Costura`를 사용합니다. `Costura`는 `Nuget Packages`뿐만 아니라 같은 솔루션의 프로젝트도 압축시켜 바이너리를 생성하므로 Core와 UI/UX를 나누어 제작하면 `Koromo Copy`에 포함된 상당수의 코드를 `Costura`로 압축시킬 수 있습니다. 따라서 별도의 프로젝트로 분리하여 설계하였습니다.

---

# UI/UX

`WPF`와 `WPF`기반 라이브러리들을 이용하여 UI/UX를 제작할 예정입니다.

## WPF 기술



## 사용중인 라이브러리

|이름|용도|주소|
|----|--|---|
|ControlzEx|간단한 컨트롤 확장들을 지원해줌|https://github.com/ControlzEx/ControlzEx|
|Material Design In XAML Toolkit|실험중인 디자인|https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit|
|Modern Chrome|실험중인 디자인, Visual Studio 테마|https://github.com/ChristianIvicevic/ModernChrome|

## Material 아이콘

https://materialdesignicons.com/

---

# 외부 라이브러리

|이름|용도|주소|
|----|---|---|
|Costura.Fody|Assembly(.dll) 리소스파일 자동바운딩|https://github.com/Fody/Costura|
|Html Agility Pack|XML, HTML 문서 분석|https://html-agility-pack.net|
|Newtonsoft.Json|JSon 파일 분석|https://www.newtonsoft.com/json|
|PhantomJS|스크립트 실행할 수 있는 드라이버|http://phantomjs.org|
|Selenium.WebDriver|셀레니움 드라이버|https://www.seleniumhq.org|
|Media Devices|MTP 디바이스 연결용 드라이버|https://github.com/Bassman2/MediaDevices|
|Message Pack|JSon 파일 분석|https://github.com/neuecc/MessagePack-CSharp/|
|Zstd Net|빠르고 가벼운 압축 알고리즘|https://github.com/skbkontur/ZstdNet|
|Puppeteer sharp|스크립트 실행할 수 있는 웹 드라이버|https://github.com/kblok/puppeteer-sharp|