# Grilling: UI 개편 (덱 공개 연출 · 아이템 유사 3D · 정보 위계 · 작은 반응)

> 2026-11-16. 계기: 정식 UI(formal-ui 24~29) 완료 뒤 사용자 "UI 다 개편해야 할 것 같아. 처음에 공탄, 실탄 나오는 것도 애니매이션 추가하고 싶고. 아이템도 자세히 보기시 그것만 유사 3d처럼 떴으면 좋겠어. 이런 거는 너가 좋은 UI란 무엇인가 서치해서 다 적용시켜봐".
> AI가 웹 조사 → 추천과 함께 질문, 사용자가 **Q1~Q8 모두 채택**.

## 조사 (2026-11-16, AI가 웹 검색 3건. 대부분 실무 블로그·커뮤니티 글 → 정답이 아니라 출발점)
| 원칙 | 내용 | 출처 |
|---|---|---|
| 정보 위계 | 중요한 정보를 눈이 먼저 가는 곳에, 글자 크기·대비·색맹 확인 | [Virtuall](https://virtuall.pro/blog/game-ui-design), [Made Good Designs](https://madegooddesigns.com/?p=5485) |
| 디에제틱 UI | 정보를 게임 세계 안 물건에 담으면 몰입↑·화면 정리↑, 대신 읽기 어려워질 수 있음 → 분위기에 맞게 섞음 | [Wayline](https://www.wayline.io/blog/diegetic-interfaces-game-design) |
| 피드백("주스") | 흔들림·입자·잠깐 멈춤 같은 작은 반응 = 행동 결과를 알리는 주된 방법. **구조가 안정된 뒤에** 넣기. 흔들림 줄이기 옵션 | [Outlook Respawn](https://respawn.outlookindia.com/gaming/gaming-guides/ui-and-ux-in-games-building-menus-huds-and-feedback-systems) |
| 움직임 원칙 | 예비 동작·천천히 시작/멈춤(easing, 직선 속도 피함)·차례로 나타나기, 버튼 눌림 = 찌그러짐, 짧은 반응 0.1~0.2초 | [IxDF](https://ixdf.org/literature/article/ui-animation-how-to-apply-disney-s-12-principles-of-animation-to-ui-design), [Bugnet](https://bugnet.io/blog/animation-principles-every-game-developer-should-know) |
| 유사 3D | 스프라이트 겹쳐 쌓기 / 기울이기 셰이더 / **여러 각도를 미리 그리기**(도트에서 가장 안정적이라는 의견), Point 필터·1:1 | [Unity Discussions](https://discussions.unity.com/t/how-can-i-fake-3d-sprite-rotation-in-2d-ex-with-skewing/591026), [80.lv](https://80.lv/articles/how-to-make-your-2d-sprites-look-like-3d-in-unity), [Game Developer](https://gamedeveloper.com/design/making-2d-games-with-unity) |

## 지금 화면에서 본 문제 (캡처 `.scratch/art/captures/26~29`)
- 단말기 `GE코인`과 덱 구성 글자가 붙어 보임, 상자(틀)가 많아 무엇이 중요한지 흐림
- 잔고·라운드·턴이 떠 있는 상자(디에제틱 아님)
- 반응이 피해 깜빡임 하나, 패널은 켜고 끄기만(움직임 없음)
- 확인 패널 아이콘이 평면

## 1라운드 (모두 채택)
| Q | 질문 | 결정 | 이유 |
|---|---|---|---|
| Q1 | 순서 | ① 정보 위계·겹침 정리 → ② 덱 채우기 공개 연출 → ③ 아이템 유사 3D → ④ 작은 반응(버튼·턴 전환·흔들림) | 조사: 반응은 구조가 안정된 뒤 |
| Q2 | 기준 디자인 | 스케치 v2 배치 유지, 떠 있는 상자를 줄이고 **테이블 위 물건 쪽으로**(잔고 = 동전 더미, 턴 = 조명) | GDD와 맞고 몰입↑ |
| Q3 | 덱 공개 연출("공탄/실탄") | 라운드 시작·재장전 때 하락/상승 캔들을 테이블 위에 한 장씩 늘어놓아 보여 줌(약 1.5초) → 뒤집어 섞여 단말기로. 그동안 입력 막음, **Space로 건너뛰기**. 벅샷 룰렛 모습을 따라 하지 않고 캔들·단말기 소재로 | 개수 공개·순서 숨김 규칙을 눈으로 |
| Q4 | 아이템 유사 3D | 아이콘을 두께 있는 도트 물체로 보고 **회전 각도 8장을 코드로 미리 그림**(팔레트 유지), 확인 패널에서 천천히 돌고 위아래로 살짝 뜸 + 그림자 | 도트에서 가장 안정적, 그림은 AI 코드 생성(#42) |
| Q5 | 작은 반응 | 버튼 눌림(0.1초), 턴 바뀔 때 띠 문구, 피해 2도트 흔들림, 동전 떨어짐. 흔들림 끄기는 Inspector 칸 | 조사 원칙 적용, 설정 화면은 범위 밖 |
| Q6 | 읽기 쉬움 | 글자 겹침 0, 최소 글자 크기 9, 색 역할 고정(노랑 = 선택·돈, 빨강 = 위험), 긴장 색 대비 재확인 | 위계 원칙 + 기존 남은 일 |
| Q7 | 시간 | 캔들 공개 0.5초·광대 생각 1초 그대로, 새 연출은 모두 건너뛸 수 있게 | GDD 한 판 3~5분 |
| Q8 | 진행 | `.scratch/ui-revamp/` Grilling → Spec → Ticket, 코드는 AI가 작성(4장 예외 기록) | 지금까지와 같은 방식 |
