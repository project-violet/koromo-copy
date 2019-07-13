# Koromo Copy Development Manual

기존의 개발자문서가 오래되어
더이상 참고할 수 없는 상태이므로 새로운 개발자 문서를 다시 작성합니다.

`Koromo Copy`에 기여하고 싶으신 분들, 소스코드에 관심이 있으신 분들,
소스 코드의 일부 기능을 자신의 프로젝트로 이식하고 싶은 분들에게 
이 문서를 읽어보시는 것을 추천드립니다.

## Koromo Copy 란?

`ENM Software`의 `Hi Downloader`를 사용하다가 사용에 불편함을 느껴
새로운 다운로더를 만들어 보고자 시작한 프로젝트입니다.
강력한 다운로더를 만드는 것을 목표로 시작했으며,
지금은 강력한 파일관리 기능을 목표로 개발 중 입니다.

## Koromo Copy 프로젝트 구조

`Koromo Copy`는 `Koromo Copy Core(Koromo Copy)`와 `Koromo Copy UX` 두 개의 
프로젝트로 개발되고 있습니다.
`Koromo Copy Core` 프로젝트는 모든 알고리즘 및 핵심기능을 구현해놓은 프로젝트 이며,
`Koromo Copy UX` 프로젝트는 `Koromo Copy` 프로그램의 `GUI`를 구현한 프로젝트입니다.
두 개의 프로젝트로 분리한 이유는 너무 많은 파일들로 인해 프로젝트 관리가 힘들고,
외부 라이브러리인 `Costura.Fody`의 바이너리 압축 기능을 이용하기 위함입니다.

### 프로그램 초기화

`Koromo Copy`가 실행되면 다음과 같은 절차를 걸쳐 프로그램을 초기화합니다.

```
Koromo Copy UX/MainWindow.cs - 생성자
1. 효율적인 메모리 관리를 위해 GC 성능을 최대로 올립니다.
2. 실시간으로 메모리 사용량을 보여주는 타이머를 초기화합니다.
3. 서비스포인트 연결 제한을 무제한(또는 사용자 설정)으로 설정합니다.
4. 핸들링되지 않은 모든 에러를 잡기위한 Callback 함수를 등록합니다.

Koromo Copy UX/SearchSpace.cs - Loaded Event
1. Koromo Copy 이용에 필요한 데이터 베이스를 다운로드하고, 로딩합니다.
2. 새로운 업데이트를 확인합니다.
3. 자동완성에 필요한 클래스를 로딩합니다.

위 두 과정을 거치면 초기화 과정이 끝납니다.
```

### 자동완성 프로세싱

자동완성에 필요한 모든 태그데이터는 데이터 베이스 로딩시 캐시한 데이터를 사용합니다.
이 과정은 데이터 베이스 로딩시 `HitomiIndex.Instance.RebuildTagData` 함수에 의해 실행됩니다.

`Koromo Copy UX/SearchSpace.cs`에서 `'Search Helper' region`으로 둘러쌓인 부분이
자동완성을 구현한 부분입니다.

`AutoCompleteLogic` 클래스는 `Koromo Copy UX` 프로젝트의 여러부분에서 
쓰이는 자동완성 기능의 코드 중복을 최소화하고자 구현한 클래스입니다.
이 클래스에 이벤트를 등록하면, 손쉽게 자동완성 기능을 구현할 수 있습니다.
`AutoCompleteLogic`과 `AutoCompleteBase` 두 개 모두 `Koromo Copy`에 종속적인 클래스라
외부 프로젝트에서 이용하려면 약간의 수정이 필요합니다.

```
SearchText_KeyUp 함수를 수정
Koromo Copy UX/Domain/AutomiCompleteLogic.cs의 120줄 ~ 204줄을 수정해야 합니다.
HitomiTagdat 클래스를 직접 구현하여 사용 프로젝트에 알맞게 수정하세요.

퍼지기반 자동완성을 사용하려면, 270줄의 Strings.GetLevenshteinDistance함수를 Koromo Copy Core에서 가져와야합니다.
```

### 검색 프로세싱

`Koromo Copy`는 일반검색과 고급검색 두 가지 검색옵션이 있습니다.

```
Koromo Copy UX/SearchSpace.xaml.cs - AppendAsync 함수
/ 및 ? 토큰 제거

 -- 일반 검색 --
HitomiDataParser.SearchAsync 호출 <= 검색 시작
Koromo Copy/Component/Hitomi/HitomiDataParser.cs
24~120줄 각각의 토큰을 분석해 쿼리를 만듬
검색 토큰에 recent가 있다면 HitomiDataSearch.GetSubsetOf 호출
없다면 HitomiDataSearch.Search3 호출
이후 완전탐색을 이용해 모든 자료 검색

 -- 고급 검색 --
HitomiDataSearchAdvanced.Search 호출 <= 검색 시작
주어진 쿼리문을 분석해 파스트리 생성
파스트리를 직렬화시켜 배열로 바꿈
검색을 시작하고, 각 작품마다 다음 과정을 따름
모든 Leaf노드에 필요한 정보들을 채우고, Root 까지 역으로 Attributed를 채운다
만약 Root가 True라면 검색 결과에 추가하고, 그렇지 않으면 추가하지 않는다.

제거했던 / 및 ? 토큰 정보를 바탕으로 결과 개수 조정
```

### 썸네일 로딩

### 작가 추천 프로세싱

### 파일 다운로더

### 콘솔 창

## Koromo Copy의 모든 기능들

아래 내용은 .NET Framework 또는 다른 라이브러리에서 제공하지 않는 
독자적으로 만든 모든 기능들을 나열한 것입니다.

### -- Koromo Copy Core --

|이름|위치|
|--|--|
|개발자 전용 Console|Console/Console.cs|
|커맨드 라인 파서|Console/CommandLineParser.cs|
|고성능 Download Queue|Net/Emilia*.cs|
|Koromo Copy 전용 Crawling Script Engine|Script/SRCAL/SRCALEngine.cs|
|인스턴스 레이지 (ILazy)|Interface/ILazy.cs|
|문자열 유사도 비교 배열 추출|Strings.cs|
|Updatable Heap 자료구조|Algorithm/Heap.cs|
|런타임 인스턴스 및 메서드 정보추출 및 시험도구|Internal.cs|
|Simple Regular Expression|LP/ScannerGenerator.cs|
|Scanner Generator|LP/ScannerGenerator.cs|
|LR 기반 Parser Generator|LP/ParserGenerator.cs|
|Elo System|Elo/EloSystem.cs|
|Graph Viewer|Utility/RelatedTags/GraphViewer.cs|
|File Indexor|Fs/FileIndexor.cs|
|한타 영타 상호 변환도구|LP/LPKor.cs|
|Log 관리 클래스|Monitor.cs|

### -- Koromo Copy UX --

|이름|위치|
|--|--|
|자동완성 기능|Domain/AutoCompleteBase.cs|
|HTML 분석기|Utility/CustomCrawler.xaml|
|Zip Viewer|Utility/ZipViewer.xaml|
|Zip Listing|Utlity/ZipListing.xaml|
|Zip Artists|Utility/ZipArtists/ZipARtists.xaml|
|Gallery Explorer|Utlity/GalleryExplorer/GalleryExplorer.xaml|
|다국어 지원|Language/Lang*|
|동적 판넬|Utility/FallsPanel.cs|
|Data Grid Sorter|Domain/DataGridSorter.cs|

## Koromo Copy 기능들에 대한 상세한 설명

## 도구 및 유틸리티

## 기타 세부적인 기능들

### Koromo Copy로 데이터 베이스를 동기화하는 방법

데이터 베이스 동기화 기능을 사용하려면 먼저 `Koromo Copy`를 `Debug`상태로 빌드해야합니다.

```
1. 프로그램이 실행되면 'Ctrl + T'를 눌러 콘솔창을 띄웁니다.
2. run gbt 명령을 입력해 'GalleryBlock Tester for Hidden Galleries' 창을 띄웁니다.
3. 시작과 끝을 적절하게 조정하고, 시작 버튼을 누릅니다.
4. 3과정이 끝나면 아래 합치기 버튼을 누르고 해당 창을 종료합니다.
5. hitomi -test 8 명령을 입력해 데이터 동기화를 수행합니다.
6. 프로그램을 재시작합니다.
```

## 앞으로의 개발 계획

