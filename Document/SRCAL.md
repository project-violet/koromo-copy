# SRCAL Script CDL

## 0. 스크립트의 필요성

하나의 웹 사이트 다운로더를 개발하는데엔 많은 개발시간과 디버깅 시간이 필요합니다.
다운로더의 개발 환경이 잘 갖추어져 있더라도 개발시간은 단축시킬 수 있을테지만, 디버깅 시간을 단축시키는데엔 한계가 있습니다.
`Koromo Copy`는 보다 나은 개발환경을 지원하고, 유지 보수가 편리한 다운로더를 만들기 위해 이 스크립트를 제작하게 되었습니다.

크롤러 제작에 대한 방법론에 관심이 있으신 분들은 `XPath`기반의 명령 구문인 `CAL`을 참고해보시면 도움이 될 것입니다.

## 1. 소개

`Simple Robust CAL Script Crawler Description Language` 이하 `SRCAL-CDL`은 `Koromo Copy`에서 크롤러의 동작을 정의하기 위한 스크립트 언어입니다.
최대한 간단한 방법으로 크롤러를 정의하기위해 언어를 최대한 단순화 시켰습니다.
이 언어를 사용하면 거의 모든 종류의 크롤러의 동작을 정의할 수 있습니다.

## 2. 설계 및 문법

```
EBNF: SRCAL-CDL

script    -> block

comment   -> ##.*?
line      -> comment
           | expr
           | expr comment
           | e

expr      -> func
           | var = index
           | runnable
          
block     -> [ block ]
          -> line block
          -> e
         
name      -> [_a-zA-Z]\w*
           | $name            ; Inernal functions

number    -> [0-9]+
string    -> "([^\\"]|\\")*"
const     -> number
           | string
         
var       -> name

index     -> variable
           | variable [ variable ]
variable  -> var
           | function    ; function call
           | const
           | procedure   ; function variable or lambda function

argument  -> index
           | index, argument
function  -> name ( )
           | name ( argument )

procedure -> ( argument ) block

runnable  -> loop (var = index "to" index) block
           | foreach (var : index)         block
           | if (index)                    block
           | if (index)                    block else block
```

```
   1:         S' -> script
   2:     script -> block
   3:       line -> expr
   4:      lines -> expr
   5:      lines -> expr lines
   6:       expr -> function
   7:       expr -> name equal index
   8:       expr -> runnable
   9:      block -> pp_open iblock pp_close
  10:      block -> line
  11:     iblock -> block
  12:     iblock -> lines
  13:     iblock ->
  14:      index -> variable
  15:      index -> variable pp_open variable pp_close
  16:   variable -> name
  17:   variable -> function
  18:   variable -> const
  19:   argument -> index
  20:   argument -> argument comma index
  21:   function -> name op_open op_close
  22:   function -> name op_open argument op_close
  23:   runnable -> loop op_open name equal index to index op_close block
  24:   runnable -> foreach op_open name scolon index op_close block
  25:   runnable -> if op_open index op_close block
  26:   runnable -> if op_open index op_close block else block

+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+
|          |     name |    const |     loop |  op_open | op_close |  pp_open | pp_close |    equal |       to |   scolon |    comma |  foreach |       if |     else |        $ |   script |     line |    lines |     expr |    block |   iblock |    index | variable | argument | function | runnable |
+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+
|       0  |       s7 |          |       s9 |          |          |       s3 |          |          |          |          |          |      s10 |      s11 |          |          |        1 |        4 |          |        5 |        2 |          |          |          |          |        6 |        8 |
|       1  |          |          |          |          |          |          |          |          |          |          |          |          |          |          |      acc |          |          |          |          |          |          |          |          |          |          |          |
|       2  |          |          |          |          |          |          |          |          |          |          |          |          |          |          |       r1 |          |          |          |          |          |          |          |          |          |          |          |
|       3  |       s7 |          |       s9 |          |          |       s3 |      r12 |          |          |          |          |      s10 |      s11 |          |          |          |        4 |       14 |       15 |       13 |       12 |          |          |          |        6 |        8 |
|       4  |       r9 |          |       r9 |          |          |          |       r9 |          |          |          |          |       r9 |       r9 |       r9 |       r9 |          |          |          |          |          |          |          |          |          |          |          |
|       5  |       r2 |          |       r2 |          |          |          |       r2 |          |          |          |          |       r2 |       r2 |       r2 |       r2 |          |          |          |          |          |          |          |          |          |          |          |
|       6  |       r5 |          |       r5 |          |          |          |       r5 |          |          |          |          |       r5 |       r5 |       r5 |       r5 |          |          |          |          |          |          |          |          |          |          |          |
|       7  |          |          |          |      s17 |          |          |          |      s16 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|       8  |       r7 |          |       r7 |          |          |          |       r7 |          |          |          |          |       r7 |       r7 |       r7 |       r7 |          |          |          |          |          |          |          |          |          |          |          |
|       9  |          |          |          |      s18 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      10  |          |          |          |      s19 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      11  |          |          |          |      s20 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      12  |          |          |          |          |          |          |      s21 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      13  |          |          |          |          |          |          |      r10 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      14  |          |          |          |          |          |          |      r11 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      15  |       s7 |          |       s9 |          |          |          |       r3 |          |          |          |          |      s10 |      s11 |          |          |          |          |       22 |       23 |          |          |          |          |          |        6 |        8 |
|      16  |      s26 |      s28 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |       24 |       25 |          |       27 |          |
|      17  |      s26 |      s28 |          |          |      s29 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |       31 |       25 |       30 |       27 |          |
|      18  |      s32 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      19  |      s33 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      20  |      s26 |      s28 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |       34 |       25 |          |       27 |          |
|      21  |       r8 |          |       r8 |          |          |          |       r8 |          |          |          |          |       r8 |       r8 |       r8 |       r8 |          |          |          |          |          |          |          |          |          |          |          |
|      22  |          |          |          |          |          |          |       r4 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      23  |       s7 |          |       s9 |          |          |          |       r3 |          |          |          |          |      s10 |      s11 |          |          |          |          |       22 |       23 |          |          |          |          |          |        6 |        8 |
|      24  |       r6 |          |       r6 |          |          |          |       r6 |          |          |          |          |       r6 |       r6 |       r6 |       r6 |          |          |          |          |          |          |          |          |          |          |          |
|      25  |      r13 |          |      r13 |          |      r13 |      s35 |      r13 |          |      r13 |          |      r13 |      r13 |      r13 |      r13 |      r13 |          |          |          |          |          |          |          |          |          |          |          |
|      26  |      r15 |          |      r15 |      s17 |      r15 |      r15 |      r15 |          |      r15 |          |      r15 |      r15 |      r15 |      r15 |      r15 |          |          |          |          |          |          |          |          |          |          |          |
|      27  |      r16 |          |      r16 |          |      r16 |      r16 |      r16 |          |      r16 |          |      r16 |      r16 |      r16 |      r16 |      r16 |          |          |          |          |          |          |          |          |          |          |          |
|      28  |      r17 |          |      r17 |          |      r17 |      r17 |      r17 |          |      r17 |          |      r17 |      r17 |      r17 |      r17 |      r17 |          |          |          |          |          |          |          |          |          |          |          |
|      29  |      r20 |          |      r20 |          |      r20 |      r20 |      r20 |          |      r20 |          |      r20 |      r20 |      r20 |      r20 |      r20 |          |          |          |          |          |          |          |          |          |          |          |
|      30  |          |          |          |          |      s36 |          |          |          |          |          |      s37 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      31  |          |          |          |          |      r18 |          |          |          |          |          |      r18 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      32  |          |          |          |          |          |          |          |      s38 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      33  |          |          |          |          |          |          |          |          |          |      s39 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      34  |          |          |          |          |      s40 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      35  |      s26 |      s28 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |       41 |          |       27 |          |
|      36  |      r21 |          |      r21 |          |      r21 |      r21 |      r21 |          |      r21 |          |      r21 |      r21 |      r21 |      r21 |      r21 |          |          |          |          |          |          |          |          |          |          |          |
|      37  |      s26 |      s28 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |       42 |       25 |          |       27 |          |
|      38  |      s26 |      s28 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |       43 |       25 |          |       27 |          |
|      39  |      s26 |      s28 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |       44 |       25 |          |       27 |          |
|      40  |       s7 |          |       s9 |          |          |       s3 |          |          |          |          |          |      s10 |      s11 |          |          |          |        4 |          |        5 |       45 |          |          |          |          |        6 |        8 |
|      41  |          |          |          |          |          |          |      s46 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      42  |          |          |          |          |      r19 |          |          |          |          |          |      r19 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      43  |          |          |          |          |          |          |          |          |      s47 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      44  |          |          |          |          |      s48 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      45  |      r24 |          |      r24 |          |          |          |      r24 |          |          |          |          |      r24 |      r24 |      s49 |      r24 |          |          |          |          |          |          |          |          |          |          |          |
|      46  |      r14 |          |      r14 |          |      r14 |          |      r14 |          |      r14 |          |      r14 |      r14 |      r14 |      r14 |      r14 |          |          |          |          |          |          |          |          |          |          |          |
|      47  |      s26 |      s28 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |       50 |       25 |          |       27 |          |
|      48  |       s7 |          |       s9 |          |          |       s3 |          |          |          |          |          |      s10 |      s11 |          |          |          |        4 |          |        5 |       51 |          |          |          |          |        6 |        8 |
|      49  |       s7 |          |       s9 |          |          |       s3 |          |          |          |          |          |      s10 |      s11 |          |          |          |        4 |          |        5 |       52 |          |          |          |          |        6 |        8 |
|      50  |          |          |          |          |      s53 |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |          |
|      51  |      r23 |          |      r23 |          |          |          |      r23 |          |          |          |          |      r23 |      r23 |      r23 |      r23 |          |          |          |          |          |          |          |          |          |          |          |
|      52  |      r25 |          |      r25 |          |          |          |      r25 |          |          |          |          |      r25 |      r25 |      r25 |      r25 |          |          |          |          |          |          |          |          |          |          |          |
|      53  |       s7 |          |       s9 |          |          |       s3 |          |          |          |          |          |      s10 |      s11 |          |          |          |        4 |          |        5 |       54 |          |          |          |          |        6 |        8 |
|      54  |      r22 |          |      r22 |          |          |          |      r22 |          |          |          |          |      r22 |      r22 |      r22 |      r22 |          |          |          |          |          |          |          |          |          |          |          |
+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+----------+
```

## 3. 스크립트 상호작용

### 3.1. CAL

`cal` 함수는 `SRCAL`의 핵심 함수로 `XPath`가 혼합된 일련의 문자열입니다.
자세한 내용은 `Custom Crawler` 문서를 통해 확인해주세요.

https://github.com/dc-koromo/koromo-copy/blob/master/Document/CustomCrawler.md#2-cal-%EB%AC%B8%EB%B2%95

### 3.2. 상호작용 API

이 API들은 `Koromo Copy`의 스크립트 엔진이 제공하는 상호작용 함수입니다.
이 함수들은 `$~~~`와 같이 `$`가 접두어로 붙은 함수들입니다.
필요하다면 새로운 함수가 언제든지 추가될 수 있습니다.

#### 1. 일반 제어 함수

크롤러를 제어하는데 필요한 가장 기본적인 요소들 입니다.

|  이름 | 내용 |
|------|-----|
|$LoadPage(url)|특정 URL을 다운로드하고, 현재 분석하는 문서를 다운로드한 문서로 바꿉니다. 이전 페이지는 저장되지 않기때문에, 필요한 문자열은 미리 저장해두어야 합니다.|
|$AppendImage(url, filename)|다운로드할 이미지와 이미지의 저장 경로를 저장합니다. 다운로드의 대상이 이미지가 아니여도 상관없습니다. Referer은 현재 URL로 지정됩니다. 저장 경로는 사용자설정에 대한 상대주소입니다.|
|$RequestDownload()|모든 요청을 끝내고 저장된 URL을 모두 다운로드합니다.|
|$ExitLoop()|특정 loop나 foreach를 탈출 합니다.|
|$ClearImagesCount()|이미지 카운트를 0으로 합니다.|
|$ConsolePrint(str)|콘솔에 str를 출력합니다.|
|$ConsolePrintln(str)|콘솔에 str를 출력하고, 다음줄로 옮깁니다.|
|$MonitorPrint(str)|Monitor를 통해 str을 출력합니다. 이는 로그에 기록됩니다.|

#### 2. 드라이버 제어 함수

`$UsingDriver`가 `1`일때 사용가능합니다. 이 함수들은 셀레니움 웹 드라이버를 제어합니다.

| 이름 | 내용 |
|-----|-----|
|$DriverNew()|기존 드라이버를 닫고, 새로운 드라이버를 실행합니다.|
|$DriverLoadPage(url)|특정 URL로 Navigate합니다.|
|$DriverClickByXPath(xpath)|XPath를 이용해 특정 버튼을 클릭합니다.|
|$DriverClickByName(name)|Name을 이용해 특정 버튼을 클릭합니다.|
|$DriverSendKey(id, text)|특정 id 요소에 text를 씁니다.|
|$DriverGetScrollHeight()|스크롤의 높이를 가져옵니다. 반환값은 정수입니다.|
|$DriverScrollTo(offset)|스크롤의 높이를 설정합니다. offset은 정수입니다.|
|$DriverScrollBottom()|스크롤을 최하단으로 내립니다.|

#### 3. 메인창 메세지 제어 함수

| 이름 | 내용 |
|-----|-----|
|$MessageFadeOn(progress, text)|메인창에 메세지를 나타냅니다. progress는 boolean 값이며 진행애니메이션의 출력여부입니다.|
|$MessageText(text)|메세지의 내용을 바꿉니다. progress 여부는 바뀌지 않습니다.|
|$MessageFadeOff(progress, text)|메인창에서 메세지를 사라지게합니다. 이 함수는 $MessageFadeOn이 호출된 다음에 호출하는 것이 좋습니다.|
|$MessageFadeInFadeOut(progress, text)|일시적으로 나타났다 사라지는 메세지를 출력합니다.|

### 3.3. 상호작용 상수

`SRCAL`은 반드시 설정해야하는 상수인 `Attributes`가 있습니다.
이는 스크립트를 식별하기 위한 상수이므로 반드시 설정되어야합니다.

| 이름 | 내용 |
|------|-----|
|$ScriptName|스크립트의 이름입니다.|
|$ScriptVersion|스크립트의 버전입니다.|
|$ScriptAuthor|스크립트의 작성자입니다.|
|$ScriptFolderName|스크립트의 기본 생성 폴더 이름입니다.|
|$ScriptRequestName|다운로드 상태에 표시될 접두사입니다.|
|$URLSpecifier|스크립트 식별용 URL입니다. 이 URL이 포함된 URL이라면 해당 스크립트로 식별됩니다.|
|$UsingDriver|Selenium Driver를 사용할지에 대한 여부입니다. 유일하게 boolean 타입을 갖습니다.|

다음은 스크립트 엔진이 제공하는. 크롤러를 정의하는데 유용한 상수들입니다.

| 이름 | 내용 |
|------|-----|
|$RequestURL|현재 분석하고있는 URL을 가져옵니다.|
|$RequestHtml|현재 분석하고있는 Html을 가져옵니다.|
|$Infinity|무한대입니다. 기본적으로 int.Max값을 가집니다.|
|$LatestImagesCount|이미지 카운트를 가져옵니다.|

필요하다면 새로운 상수가 언제든지 추가될 수 있습니다.

### 3.4. 어노테이션

어노테이션은 반복문을 병렬화하기 위해 사용합니다.

### 3.5. 함수

| 이름 | 내용 |
|-----|-----|
|cal(text)|CAL 문법을 이용해 계산합니다. 반환값은 string-list입니다.|
|equal(var, var)|두 값이 같은지 확인합니다. 반환값은 boolean입니다.|
|split(tar, src)|src를 기준으로 문자열을 자릅니다. 반환값은 string-list입니다.|
|count(list)|List의 요소 개수를 가져옵니다. 반환값은 정수입니다.|
|add(x,y)|두 값을 더합니다.|
|mul(x,y)|두 값을 곱합니다.|
|mod(x,y)|x mod y의 값을 구합니다.|
|div(x,y)|x / y의 값을 구합니다.|
|concat(str,str,...)|여러 문자열을 하나로 합칩니다.|
|url_parameter(url,param,value)|URL의 특정 매개변수를 value로 설정합니다. value는 정수도 올 수 있습니다.|
|url_parameter_tidy(url,param)|URL의 특정 매개변수를 삭제합니다.|
|int(txt)|불리안값이나 정수형, 문자열을 정수형으로 바꿉니다.|
|string(tar)|불리안값이나 정수형, 문자열을 문자열형으로 바꿉니다.|
|regex_exists(pattern, tar)|tar에 pattern과 매칭되는 항목이 존재하는지의 여부를 가져옵니다.|
|regex_match(pattern, tar)|tar에 pattern과 매칭되는 항목의 첫 번째 요소를 가져옵니다. 반환값은 string입니다.|
|regex_matches(pattern, tar, index=0)|tar에 pattern과 매칭되는 모든 항목을 가져옵니다. index는 각 매칭된 항목 그룹의 offset입니다. 반환값은 string-list입니다.|
|type(obj)|인자의 타입을 문자열형태로 가져옵니다.|
|to_json(str)|문자열을 json형태로 가져옵니다.|
|get_json(json, str)|json의 특정 항목을 가져옵니다. 반환값은 json입니다.|
|get_json_string(json, str)|json의 특정 항목을 문자열 형태로 가져옵니다.|
|base64decode(str)|base64 포맷을 디코딩합니다.|
|htmldecode(str)|html 포맷을 디코딩합니다. 가령, `&amp;`와 같은 문자열이 치환됩니다.|
|urldecode(str)|url 포맷을 디코딩합니다.|

## 4. 스크립트 자동 생성 도구

`Koromo Copy`는 `SRCAL-CDL`의 정형화된 몇 가지 틀을 제공합니다.
제공되는 틀은 다음과 같습니다.

```
1. 한 페이지에서 여러개의 이미지를 다운로드할 경우
2. 한 페이지에서 다른 페이지들을 불러오고, 불러온 페이지에서 여러개의 이미지를 다운로드할 경우
3. 한 페이지에서 다른 페이지들을 불러오고, 불러온 페이지에서 또 다른 하위 페이지들을 불러온 뒤, 불러온 하위 페이지에서 여러개의 이미지를 다운로드할 경우
4. 구글 블로그 (blogspot)의 동의 여부가 필요할 경우

위 네 가지의 크롤링 유형외엔 다른 유형은 제공되지 않습니다.
```

## 5. 예제

### 5.1. 단부루 다운로더

```
##
## Koromo Copy SRCAL Script
##
## Danbooru Downloader
##

##
## Attributes
##
$ScriptName = "danbooru-pages"
$ScriptVersion = "0.2"
$ScriptAuthor = "dc-koromo"
$ScriptFolderName = "danbooru"
$ScriptRequestName = "danbooru"
$URLSpecifier = "https://danbooru.donmai.us/"
$UsingDriver = 0

##
## Procedure
##
request_url = $RequestURL
## request_url = url_parameter_tidy(request_url, "page")
max_page = $Infinity

loop (i = 1 to max_page) [
    ## $LoadPage(concat(request_url, "&page=", i))
    $LoadPage(url_parameter(request_url, "page", i))
    sub_urls = cal("/html[1]/body[1]/div[1]/div[3]/div[1]/section[1]/div[3]/div[1]/article[{1+i*1}]/a[1], #attr[href], #front[https://danbooru.donmai.us]")

    foreach (sub_url : sub_urls) [
        $LoadPage(sub_url)
        image_url = ""
        if (equal(cal("/html[1]/body[1]/div[1]/div[3]/div[1]/section[1]/div[1]/span[1]/a[1], #attr[id]")[0], "image-resize-link")) [
            image_url = cal("/html[1]/body[1]/div[1]/div[3]/div[1]/section[1]/div[1]/span[1]/a[1], #attr[href]")[0]
        ] else if (equal(cal("/html[1]/body[1]/div[1]/div[3]/div[1]/section[1]/div[2]/span[1]/a[1], #attr[id]")[0], "image-resize-link")) [
            image_url = cal("/html[1]/body[1]/div[1]/div[3]/div[1]/section[1]/div[2]/span[1]/a[1], #attr[href]")[0]
        ] else [
            image_url = cal("/html[1]/body[1]/div[1]/div[3]/div[1]/section[1]/section[1]/img[1], #attr[src]")[0]
        ]
        file_name = split(image_url, "/")[-1]
        $AppendImage(image_url, file_name)
    ]

    if (equal($LatestImagesCount, 0)) [ 
        $ExitLoop()
    ]

    $ClearImagesCount()
]

$RequestDownload()
```

### 5.2. 망가쇼미 시리즈 다운로더

```
##
## Koromo Copy SRCAL Script
##
## Mangashowme Series Downloader
##

##
## Attributes
##
$ScriptName = "mangashowme-series"
$ScriptVersion = "0.2"
$ScriptAuthor = "dc-koromo"
$ScriptFolderName = "mangashowme"
$ScriptRequestName = "mangashowme"
$URLSpecifier = "https://mangashow.me/bbs/page.php"
$UsingDriver = 0

##
## Procedure
##
request_url = $RequestURL

title = cal("/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]")[0]
data_wrids = cal("/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[2]/div[2]/div[1]/div[1]/div[{1+i*1}], #attr[data-wrid]")
sub_titles = cal("/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[2]/div[2]/div[1]/div[1]/div[{1+i*1}]/a[1]/div[1], #htext")

$MessageFadeOn(true, concat(title, "...[0/", count(data_wrids), "]"))

loop (i = 0 to add(count(data_wrids), -1)) [
    $LoadPage(concat("https://mangashow.me/bbs/board.php?bo_table=msm_manga&wr_id=", data_wrids[i]))
    $MessageText(concat(title, "...[", add(i,1), "/", count(data_wrids), "]"))
    images = cal("/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/section[1]/div[1]/form[1]/div[1]/div[{1+i*1}]/div[1], #attr[style], #regex[https://[^\\)]*]")
    foreach (image : images) [
        filename = split(image, "/")[-1]
        $AppendImage(image, concat(title, "/", sub_titles[i], "/", filename))
    ]
]

$MessageFadeOff(true, concat(title, "...complete"))

$RequestDownload()
```

### 5.3. 디시인사이드 게시글 이미지 다운로더

```
##
## Koromo Copy SRCAL Script
##
## DCInside Article Image Downloader
##

##
## Attributes
##
$ScriptName = "dcinside-article"
$ScriptVersion = "0.1"
$ScriptAuthor = "dc-koromo"
$ScriptFolderName = "dcinside"
$ScriptRequestName = "dcinside"
$URLSpecifier = "http://gall.dcinside.com/board/view/"
$UsingDriver = 0

##
## Procedure
##
request_url = $RequestURL

gallery_name = cal("/html[1]/body[1]/div[2]/div[2]/main[1]/section[1]/header[1]/div[1]/div[1]/h2[1]/a[1]")
title = cal("/html[1]/body[1]/div[2]/div[2]/main[1]/section[1]/article[2]/div[1]/header[1]/div[1]/h3[1]/span[2]")
images = cal("/html[1]/body[1]/div[2]/div[2]/main[1]/section[1]/article[2]/div[1]/div[1]/div[6]/ul[1]/li[{1+i*1}]/a[1], #attr[href]")
filenames = cal("/html[1]/body[1]/div[2]/div[2]/main[1]/section[1]/article[2]/div[1]/div[1]/div[6]/ul[1]/li[{1+i*1}]/a[1]")

loop (i = 0 to add(count(images), -1))
    $AppendImage(images[i], concat(gallery_name, "/", title, "/", filenames[i]))

$RequestDownload()
```

### 5.4. Nozomi.la 다운로더

```
##
## Koromo Copy SRCAL Script
##
## Nozomi.la Downloader
##

##
## Attributes
##
$ScriptName = "nozomi"
$ScriptVersion = "0.1"
$ScriptAuthor = "dc-koromo"
$ScriptFolderName = "nozomi"
$ScriptRequestName = "nozomi"
$URLSpecifier = "https://nozomi.la/"
$UsingDriver = 1

##
## Procedure
##
request_url = $RequestURL
max_page = int(cal("/html[1]/body[1]/div[1]/div[2]/div[1]/ul[1]/li[5]/a[1]")[0])

prefix = split(request_url, "-")[0]

loop (i = 1 to max_page) [
    $DriverLoadPage(concat(prefix, "-", i, ".html"))
    images = cal("/html[1]/body[1]/div[1]/div[2]/div[2]/div[{1+i*1}]/a[1]/img[1], #attr[src]")

    foreach (image : images) [
       image = concat("https:", image)
       filename = split(image, "/")[-1]
       $AppendImage(image, filename)
    ]
]

$RequestDownload()
```

### 5.5. 구구단

```
##
## Koromo Copy SRCAL Script
##
## Dovedan Printer
##

##
## Attributes
##
$ScriptName = "googoodan"
$ScriptVersion = "1.0"
$ScriptAuthor = ""
$ScriptFolderName = ""
$ScriptRequestName = ""
$URLSpecifier = "https://google.com"
$UsingDriver = 0

##
## Procedure
##
request_url = $RequestURL

$MessageFadeOn(true, "Start collecting...")

loop (i = 1 to 10) [
    loop (j = 1 to 10) [
        multiple = mul(i,j)
        $ConsolePrint(concat(i, " * ", j, " = "))
        $ConsolePrintln(string(multiple))
    ]
]

$MessageFadeOff(true, "Complete collect all images!")
```

### 5.6. 소수

```
##
## Koromo Copy SRCAL Script
##
## Prime Numbers Printer
##

##
## Attributes
##
$ScriptName = "prime-number"
$ScriptVersion = "1.0"
$ScriptAuthor = ""
$ScriptFolderName = ""
$ScriptRequestName = ""
$URLSpecifier = "https://google.com"
$UsingDriver = 0

##
## Procedure
##
request_url = $RequestURL

$MessageFadeOn(true, "Start collecting...")

loop (i = 2 to 100) [
    is_prime = true
    loop (j = 2 to add(i,-1)) [
        if (equal(mod(i,j), 0)) [
            is_prime = false
            $ExitLoop()
        ]
    ]
    if (is_prime)
        $ConsolePrintln(string(i))
]

$MessageFadeOff(true, "Complete collect all images!")
```

### 5.7. 겔부루

```
##
## Koromo Copy SRCAL Script
##
## Gelbooru Image Downloader
##

##
## Attributes
##
$ScriptName = "gelbooru-images"
$ScriptVersion = "0.1"
$ScriptAuthor = "dc-koromo"
$ScriptFolderName = "gelbooru"
$ScriptRequestName = "gelbooru"
$URLSpecifier = "https://gelbooru.com/"
$UsingDriver = 0

##
## Procedure
##
request_url = $RequestURL

tags = url_parameter(request_url, "tags")

$MessageFadeOn(true, "Start collecting...")

loop (i = 0 to $Infinity) [
    $LoadPage(concat("https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=100&tags=", tags, "&pid=", i))
    
    $MessageText(concat(tags, "...[", i, "]"))
    foreach (image : cal("/posts[1]/post[{1+i*1}], #attr[file_url]"))
        $AppendImage(image, concat(tags, "/", split(image, "/")[-1]))
    
    if (equal($LatestImagesCount, 0))
        $ExitLoop()
    
    $ClearImagesCount()
]

$MessageFadeOff(true, "Complete collect all images!")
$RequestDownload()
```

### 5.8. imgur

```
##
## Koromo Copy SRCAL Script
##
## imgur Images Downloader
##

##
## Attributes
##
$ScriptName = "imgur-images"
$ScriptVersion = "0.1"
$ScriptAuthor = "dc-koromo"
$ScriptFolderName = "imgur"
$ScriptRequestName = "imgur"
$URLSpecifier = "https://imgur.com/"
$UsingDriver = 0

##
## Procedure
##
request_url = $RequestURL

$MessageFadeOn(true, "Start collecting...")

title = split(request_url, "/")[-1]

json = to_json(regex_matches("item: ({.*})", $RequestHtml, 1)[0])
json = get_json(json, "album_images")
json = get_json(json, "images")

foreach (j : json) [
    hash = get_json_string(j, "hash")
    ext = get_json_string(j, "ext")
    filename = concat(hash, ext)
    image = concat("https://i.imgur.com/", filename)
    $AppendImage(image, concat(title, "/", filename))
]
    
$MessageFadeOff(true, "Complete collect all images!")
$RequestDownload()
```

### 5.9. jmana

```
##
## Koromo Copy SRCAL Script
##
## JMana Downloader
##

##
## Attributes
##
$ScriptName = "jmana"
$ScriptVersion = "0.1"
$ScriptAuthor = "dc-koromo"
$ScriptFolderName = "jmana"
$ScriptRequestName = "jmana"
$URLSpecifier = "https://www.jmana2.com/book/"
$UsingDriver = 0

##
## Procedure
##
request_url = $RequestURL

$MessageFadeOn(true, "Start collecting...")

title = cal("/html[1]/body[1]/div[2]/div[1]/ul[1]/li[1]")[0]
sub_urls = cal("/html[1]/body[1]/div[2]/div[2]/div[2]/ul[1]/li[{1+i*1}]/a[1], #attr[href]")
sub_titles = cal("/html[1]/body[1]/div[2]/div[2]/div[2]/ul[1]/li[{1+i*1}]/a[1]")

max_page = add(count(sub_urls),-1)
loop (i = 0 to max_page) [
    $LoadPage(concat("https://www.jmana2.com", sub_urls[i]))
    $MessageText(concat(title, "...[", i, "/", max_page, "]"))
    
    images = cal("/html[1]/body[1]/div[2]/div[1]/div[3]/ul[1]/li[{1+i*1}]/div[1]/img[1], #attr[src]")
    
    foreach (image : images) [
        tidy = split(image, "amp;")
        image = concat(tidy[0], tidy[1])
        filename = concat(split(image, "=")[-1], ".jpg")
        $AppendImage(image, concat(title, "/", sub_titles[i], "/", filename))
    ]
]

$MessageFadeOff(true, "Complete collect all images!")
$RequestDownload()
```