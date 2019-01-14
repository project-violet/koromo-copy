# Custom Crawler 사용방법

`Custom Crawler`는 `Koromo Copy` 사용자가 쉽게 크롤러를 작성할 수 있도록 도와주는 도구입니다.
이 도구를 사용하면 누구나 손쉽게 새로운 크롤러를 작성할 수 있습니다.

## 0. 선행 지식

`HTML`의 간단한 문법, `XPath` 읽는 방법, 문자열 `Split`함수, 이 세 가지를 알고 있으면 더 쉽게 크롤러를 제작할 수 있습니다.

`도구 탭->도구 및 유틸리티->커스텀 크롤러`를 통해 커스텀 크롤러를 실행할 수 있습니다.

### 0.1. HTML 구조 및 문법

![hitomi main](Images/cc-html-tree.png)

네이버(http://www.naver.com)의 `HTML` 파일 구조

위 사진과 같이 `HTML`은 `<html> ~ </html>`, `<head> ~ </head>`, `<body> ~ </body>` 처럼 `<> ~ </>`에 둘러싸여 있습니다.
이런 계층 구조 덕분에 우리는 `HTML` 파일을 쉽게 분석할 수 있습니다.

우리가 `HTML`을 분석함에 있어서 꼭 알아야 할 문법은 딱 두 가지 입니다.

``` html
<a href="https://google.com">링크</a>
<img src="https://www.google.com/images/branding/googlelogo/2x/googlelogo_color_272x92dp.png">
```

바탕화면에 `a.html` 파일을 만들고 위 `HTML`을 복사한 후 브라우져로 실행해 보세요.

`<a> ~ </a>` 태그는 `<a> ~ </a>`로 감싼 텍스트(또는 사진 등)에 `href` 하이퍼링크 속성을 걸어주는 태그입니다.
`href`는 감싼 텍스트를 클릭할 때 이동할 링크입니다.

`<img src="~">` 태그는 `src` 링크에 있는 이미지를 가져와 보여줍니다. `<img>`태그는 닫는 태그 `</img>`가 없습니다.

### 0.2. XPath

### 0.3. 문자열 함수

## 1. 도구

![hitomi main](Images/custom-crawler.png)

`URL` 입력 후 `Load` 버튼을 누르면, 해당 `URL`을 다운로드하고, 분석하여 리스트로 보여줍니다.
`a,img` 태그이외에 다른 태그를 보려면, `Load` 버튼 하단의 `이름`에서 리스트에 포함할 태그들을 선택하고, `Load` 버튼을 누르세요.

리스트의 요소들엔 태그들의 깊이와 특징항목(예를 들어 `a` 태그는 `href` 속성값, `img` 태그는 `src` 속성값)이 있습니다.
인덱스는 같은 깊이에서 순서대로 부여됩니다.
리스트의 요소를 더블클릭하면 `img`의 경우엔 이미지를 보여주며, `a`나 기타 태그의 경우엔 해당 링크를 실행합니다.

또한 드래그를 통해 요소들을 선택할 수 있으며, `Ctrl`키를 누른채 클릭하면 특정항목 선택/선택취소를, `Shift`키를 누른채 클릭하면 범위 선택을 할 수 있습니다.

하단 `FILTER`를 통해 원하는 특정항목을 필터링할 수 있습니다.

### 1.1. LCA (Lowest Common Ancestor)

![hitomi main](Images/cc-lca.png)

`LCA`는 선택된 태그들의 최소 공통 조상을 찾습니다.
이 기능은 개발용도로 넣어둔 것이니 `XPath` 대신 사용하시길 바랍니다.

### 1.2. XPath

![hitomi main](Images/cc-xpath.png)

선택된 태그들의 최소 공통 조상과 선택된 태그들의 `XPath`를 보여줍니다.
`LCA XPath`와 함께 보여주며, 절대경로를 구하려면 `LCA XPath`와 각 항목을 합쳐야합니다.

가령 `[0000] /aside[1]/div[3]/div[2]/div[1]/a[2]`의 절대 경로는 `/html[1]/body[1]/aside[1]/div[3]/div[2]/div[1]/a[2]`입니다.

![hitomi main](Images/cc-xpath-pattern.png)

`Pattern`은 각 `XPath`의 규칙을 분석한 일반화된 `XPath`입니다.
이 패턴은 각 항목 모두를 검증하진 않기 때문에 반드시 수동 검증을 해야합니다.

### 1.3. Attributes

![hitomi main](Images/cc-attributes.png)

선택된 태그의 속성들을 보여줍니다.

### 1.4. Tree/LCA Tree

![hitomi main](Images/cc-tree.png)

### 1.5. CAL

`CAL` 문법에 관해선 `2. CAL 문법`을 참고하세요.

![hitomi main](Images/cc-cal.gif)

실시간으로 `CAL` 문법을 해석하고, 결과를 보여줍니다.

## 2. CAL 문법

## 3. 제작

준비물 : `FireFox`, `Chrome` 또는 개발자 도구가 지원되는 웹 브라우져

### 3.1. 예제) 디시인사이드 게시글

### 3.2. 예제) 망가쇼미 작품

https://mangashow.me/bbs/page.php?hid=manga_detail&manga_name=%ED%9E%88%ED%86%A0%EB%A6%AC+%EB%B4%87%EC%B9%98%EC%9D%98+OO%EC%83%9D%ED%99%9C

```
제목: /html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]
부 제목: /html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[2]/div[2]/div[1]/div[1]/div[{1+i*1}]/a[1], #text
작품들 링크: /html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[2]/div[2]/div[1]/div[1]/div[{1+i*1}]/a[1], #attr[href]
```

https://mangashow.me/bbs/board.php?bo_table=msm_manga&wr_id=417295

```
제목: /html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[2]/div[1]/div[1]/h1[1]
이미지: /html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/section[1]/div[1]/form[1]/div[1]/div[{1+i*1}]/div[1], #attr[style], #regex[https://[^\)]*]
```

망가쇼미 스크립트 전문

``` json
{
  "ScriptName": "Mangashowme",
  "ScriptVersion": "1.0",
  "ScriptAuthor": "dc-koromo",
  "ScriptFolderName": "mangashowme",
  "ScriptRequestName": "mangashow-me",
  "PerDelay": 1000,
  "UsingDriver": false,
  "URLSpecifier": "https://mangashow.me/",
  "TitleCAL": "/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]",
  "ImagesCAL": "",
  "FileNameCAL": "",
  "UsingSub": true,
  "SubURLCAL": "/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[2]/div[2]/div[1]/div[1]/div[{1+i*1}]/a[1], #attr[href]",
  "SubURLTitleCAL": "/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[2]/div[2]/div[1]/div[1]/div[{1+i*1}]/a[1], #text",
  "SubTitleCAL": "",
  "SubImagesCAL": "/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/section[1]/div[1]/form[1]/div[1]/div[{1+i*1}]/div[1], #attr[style], #regex[https://[^\\\\)]*]",
  "SubFileNameCAL": "/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/section[1]/div[1]/form[1]/div[1]/div[{1+i*1}]/div[1], #attr[style], #regex[https://[^\\\\)]*], #split[/,-1]"
}
```