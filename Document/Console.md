# Console 명령 모음

이 명령들은 모두 런타임 명령어로, 실행 즉시 프로그램에 반영됩니다.
잘못사용시 치명적인 오류가 발생할 수 있으므로 주의해주세요.

## 1. 작가 추천의 시작 지점을 변경하기

세 번째 줄의 마지막 `1`을 원하는 시작 지점으로 고치면 된다.

```
internal --call MainWindow.RecommendTab.get_Content "" -RA
internal --set <latest>.latest_load_count 1
```

## 2. 어떤 작가의 작가 추천 순위를 알고싶은 경우

```
hitomi -rank > grep michiking
```

## 3. 강제로 업데이트 다시 확인

```
internal --call searchspace.CheckUpdate ""
```

## 4. 현재 세팅 정보 가져오기

```
internal --get setting
```