# Koromo Copy 1.8 업데이트 내용

이번 업데이트는 내용이 많아 웹 페이지로 대체합니다.

## 1. 추가된 기능

### Zip Artists 도구 추가

다운로드된 작품을 관리해주는 `Zip Artists` 도구가 추가됩니다.
이 도구를 사용하면 정리되지 않은 수 많은 작품들을 손쉽게 정리할 수 있으며,
독립된 검색환경과 카테고리를 통해 원하는 작가를 탐색할 수 있습니다.

https://github.com/dc-koromo/koromo-copy/blob/master/Document/ZipArtists.md

개별 작품들을 관리해주는 `Zip Listing`과 연동시킬 계획이 있습니다.

### 작업표시줄에 다운로드 현황 표시

이제부터 작업표시줄에 다운로드 작업 현황이 표시됩니다.

## 2. 삭제된 기능

### 플러그인 기능 삭제

`Koromo Copy` 초기에 적용되었던 플러그인 기능을 완전 삭제했습니다.
기존 플러그인 기능들은 스크립트나 설정으로 대체됩니다.

## 3. 이모저모

### 개발 계획

앞으로 다운로더의 기능 개선보단 `Zip Artists`와 같은 도구들을 위주로 개발해나갈 계획입니다.

### 안드로이드 앱 개발

`Koromo Copy`의 안드로이드 어플리케이션 개발을 준비중입니다.
개발 언어는 `Kotlin` 이며, 알파 버전이 나오면 소스코드와 함께 릴리즈하겠습니다.

### 메인 검색창에 페이저 추가여부

`Hitomi Copy` 때부터 지적받아온 검색환경의 문제는 모든 검색결과를 한 번에 보여준다는 것 입니다.
임시 방편으로 `/ ?` 검색토큰을 추가했지만, 잊어버리기 쉽고 효율적인 방법이 아니라서 새로운 방법이 필요하다는 의견이 많았습니다. 그래서 추천받은 기능들 중 하나가 페이저(Pagination, 검색결과를 특정 개수 별로 나누어 페이지로 보여주는 방식) 기능이며 이 기능은 `Zip Listing`에 적용한 바 있습니다.

메인 검색창에도 적용을 시도해보았으나 기존 `MetroUI`에 어울릴만한 디자인이 도저히 나오질 않아서 적용시키지 못했습니다 ㅠㅠ. 아래 브랜치는 페이저 기능을 적용시킨 테스트 버전 중 하나입니다.

https://github.com/dc-koromo/koromo-copy/tree/ux-dev

좋은 방법이나 좋은 디자인이 있다면 조언해주셨으면 좋겠습니다.

## 4. 자주 묻는 질문

### 데이터 다운로드/동기화가 안됩니다

초기 데이터 다운로드가 안되거나, 예전 데이터가 검색이 안되는 상황이라면, 다음과 같은 절차를 따라주세요.

https://github.com/dc-koromo/e-archive/releases/download/metadata/metadata.compress

https://github.com/dc-koromo/e-archive/blob/master/hiddendata.compress

이곳에서 `metadata.compress`와 `hiddendata.compress` 파일을 다운로드받아 `Koromo Copy`가 있는 폴더로 이동시킵니다.

`Koromo Copy`를 실행하고, `Ctrl + T`를 눌러 콘솔창을 실행한 뒤 다음 명령어를 순서대로 입력합니다.

```
hitomi -test 5
hitomi -test 6
```

명령어 실행이 완료되었으면 프로그램을 재시작해주세요.