# Grilling: 광대 폰은 깨지고 불안정하게 + 시작 스토리 길게 눌러 건너뛰기 (clown-phone)

> 2026-11-22. 계기: 사용자 "광대 폰은 깨져있고, 뭔가 불안정하게 만들어줘. 광대의 것이라는 걸 유저가 인식했으면 좋겠거든. 그리고 광대의 핸드폰을 클릭했을 때도 나오는 화면도 휴대폰의 겉모습과 어울리게 해줘. 그리고 시네마틱 skip 기능 만들어줘(space바 꾹 눌러서 스킵) 이런거. UI는 너가 서치해서 최적의 아트 워크로 만들어".
> 확인한 사실: 광대 폰 확대 화면 = 내 폰과 똑같음(`phone-open.png` 남색·금색, 제목만 "광대 아이템"). 탁자 위 두 폰은 같은 모양(item-phone 83). 시작 스토리: Space·Enter·클릭 = 누르는 순간 다음 줄, Esc = 즉시 건너뛰기(intro-story 77).
> 조사(2026-11-22): 길게 누르기는 진행률 하나로 링/막대를 채우는 방식이 흔함(Motion hold-to-confirm, Construct 3 hold-to-skip), 정보는 필요할 때만(300Mind), 오래 누르기는 덜 힘든 대안을 함께(Xbox 접근성 지침 107·116 — "Hold to skip" 예시 화면). 누르는 시간의 공식 기준은 못 찾음.
> 사용자 **Q1~Q7 모두 채택**.

| Q | 질문 | 결정 | 이유 |
|---|---|---|---|
| Q1 | 탁자 위 광대 폰 겉모습 | 빨간 몸통(광대 옷 색) + 금 간 바깥 화면 + 깨진 모서리 + 테이프 한 줄, 숫자는 금을 피해 잘 읽히게 | 색이 "광대 것" 가장 강한 신호 |
| Q2 | 불안정한 움직임 | 2~4초마다 0.1~0.2초 지지직(가로줄 노이즈·숫자 1도트 떨림·잠깐 꺼짐), 소리 없음 | 아이템 진동과 헷갈리지 않게 |
| Q3 | 확대 화면 | 같은 폴더폰 모양 + 빨강 몸통·키패드(몇 키 금·빠짐) + 화면 금 간 유리(가장자리 위주) + 2~4초마다 0.1초 글리치(가로줄 밀림·색 번짐), 글자·칸 배치 그대로 | 겉모습과 어울리게, 읽기는 유지 |
| Q4 | 건너뛰기 표시 | 오른쪽 아래(대사 상자 위) 픽셀 키캡 `SPACE` + "길게 눌러 건너뛰기", 평소 흐리게 40%, 누르는 동안 밝게 + 키 안이 왼→오 금색으로 참, 다 차면 바로 1라운드 | Space는 가로로 긴 키라 링보다 키 안 채움이 자연스러움 |
| Q5 | 시간·짧게 누르기 | 길게 1.0초 = 건너뛰기 / 0.25초 안에 뗌 = 다음 줄(뗄 때) / 차다 뗌 = 0.3초에 빠르게 비움 / Enter·클릭 = 지금처럼 누를 때 다음 줄 / Esc = 즉시 건너뛰기(대안) | 길게 누르려다 줄이 먼저 넘어가지 않게, 접근성 |
| Q6 | 선택 중 | Space 짧게 = 결정, 길게 = 건너뛰기("예") | |
| Q7 | 진행 | `.scratch/clown-phone/` Ticket 84 탁자 광대 폰·85 확대 화면 + intro-story Ticket 86 길게 눌러 건너뛰기, 코드·Scene·그림 AI, gdd-changes·asset-rights | 지금까지와 같음 |

## 범위 밖
- 광대 폰 소리(지지직 소리), 내 폰 변경, 건너뛰기 설정 메뉴(자동 넘김 등)

Sources: Motion hold-to-confirm (motion.dev/tutorials/js-hold-to-confirm), 300Mind radial menus (300mind.studio/blog/radial-menus-in-game-design/), Construct 3 hold-to-skip (jefawk.itch.io), Xbox Accessibility Guidelines 107·116 (learn.microsoft.com/gaming/accessibility/xbox-accessibility-guidelines), gameaccessibilityguidelines.com.
