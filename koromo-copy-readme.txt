
 --------------------------------------------------------------------------
 
                * Koromo Copy 사용자 분들에게 드리는 말씀 *
 
     2019년 08월 31일 이후로 Koromo Copy 프로젝트를 완전히 중단합니다.
 위 날짜 이후엔 다음 두 가지 항목에 대한 지원을 더이상 받을 수 없습니다.
 
   1. Koromo Copy 프로그램 업데이트
   2. 최신 데이터 동기화
 
 최신 데이터 동기화 기능은 hitomi.la 웹사이트의 구조가 변경되지 않는 한 
 수동으로 동기화할 수 있습니다. 아래 적어두었으니 참고바랍니다.
 
 이는 제 개인적인 이유 때문에 더이상 개발을 지속할 수 없어 내린 결정입니다.
 
 --------------------------------------------------------------------------
 
  * 수동으로 데이터를 동기화하는 방법
 
 https://github.com/dc-koromo/e-archive 여기서 hiddendata.json과 
 metadata.json을 다운로드하고, Koromo Copy가 있는 디렉토리에 넣습니다.
 Koromo Copy를 실행한뒤 Ctrl + T를 눌러 콘솔창을 띄우고, 다음 명령을 
 입력합니다.
 
   run gbt
 
 위 명령을 입력하면 GalleryBlock Tester for Hidden Galleries가 실행됩니다.
 그 다음 시작 버튼을 클릭합니다. 테스팅 과정이 끝나면 우측하단 합치기 버튼을
 클릭합니다. 콘솔창으로 다시 돌아가서 아래 명령을 입력합니다.
 
  hitomi -test 8
 
 Koromo Copy를 재시작하면 최신 데이터를 로딩할 수 있습니다.
 
 
  * 원래 제목 동기화 방법
 
 https://github.com/dc-koromo/e-archive 여기서 ex-hentai-archive.json를
 다운로드하고, Koromo Copy가 있는 디렉토리에 넣습니다. Koromo Copy를 
 실행한뒤 Ctrl + T를 눌러 콘솔창을 띄우고 다음 명령을 입력합니다.
 
  hitomi -test 10
 
 위 명령은 ex-hentai의 모든 게시글 목록을 최신기준 50페이지까지 읽어옵니다.
 모든 과정이 완료되고 콘솔창이 새로운 명령을 입력받을 준비가 되었을 경우
 Koromo Copy를 재시작합니다. ex-hentai-archive3.json을 ex-hentai-archive
 .json로 바꾸면 지속적으로 새로운 원래 제목을 동기화할 수 있습니다.
 
 
  * 추천 설정
 
 최근들어 다운로드 상태가 0 KB/S 떨어지고 다시 올라가는 현상을 겪고 있는 분
 들이 많이 계신것 같습니다. 이 문제를 어느정도 해결하려면 setting.json 파일
 을 열고 다음 항목을 변경해보시길 바랍니다.
 
  TimeoutInfinite: false
  TimeoutMillisecond: 15
  Thread: 12
 
 위 세 가지 항목 정도만 바꿔준다면 0 KB/S 문제는 개선될 겁니다.
 
 --------------------------------------------------------------------------
 
  * 최종버전까지의 개발 정보 요약
 
 https://gall.dcinside.com/board/view/?id=programming&no=1062475
 
 
  * 개발자 정보
 
 이메일: koromo.software@gmail.com
 깃허브: https://github.com/dc-koromo
 
 --------------------------------------------------------------------------
 
 Hitomi Copy 및 Koromo Copy를 이용해 주신 모든 분들께 감사의 말씀 드립니다.
 향후 기회가 된다면 새로운 프로젝트로 찾아뵙겠습니다.

