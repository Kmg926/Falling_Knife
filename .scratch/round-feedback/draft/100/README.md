# draft 100: 내 복권 결과 연출

> 2026-11-23. 코드는 AI가 Scripts에 직접. `GameManager.before.cs` = 바꾸기 전, `LotteryPopupView.cs`는 새 파일.

## 줄별 설명 — GameManager.cs
- `UseLottery` 끝: 쓴 사람이 나면 `PlayerLotteryCount++`. 화면은 이 번호가 바뀌었나만 봄(광대 폰 반짝임·말풍선과 같은 "신호 번호" 방식). 판정·결과 계산은 그대로(`LastLotteryOutcome` = 실제로 된 것, `LastLotteryDetail` = 결과 한 줄).

## 줄별 설명 — LotteryPopupView.cs (새 파일)
- `Awake`: 판을 끄고 금색 틀 선 4개를 만듦(튜토리얼 강조 틀과 같은 방법 — 그림 없는 단색 Image).
- `Start`: 시작할 때의 번호를 기억(처음부터 뜨지 않게). `Update`: 번호가 바뀌면 `Show()`.
- `Show()`: 당첨이면(`LastLotteryOutcome`이 꽝이 아니면) 금색 + 높은 소리, 꽝이면 회색 + 낮은 소리(`AudioManager.PlayVoice` = 클릭음을 높낮이만 바꿔 냄, 효과음 볼륨을 따름). 글은 결과 한 줄, 레버리지만 "(현재 N)"을 뺀 "레버리지 +1". 판 크기 0.6 → 1(살짝 지나쳤다 돌아오는 `EaseOutBack`) 0.15초.
- 바뀐 곳: 잔고 +1 → 내 잔고, 레버리지 +1 → 레버리지 상자, 아이템 +1 → 내 폰. 그 자리·크기 + 여백 2로 틀을 놓고 남은 시간 동안 6번 켜고 끔(세 번 깜빡). 꽝이면 틀 없이 기다림.
- `OnDisable`: 다른 Scene으로 가거나 꺼질 때 판·틀을 끄고 코루틴 정지.

## Scene (MCP, GameScene `HUD/ShakeRoot/LotteryPopup`)
| 대상 | 값 |
|---|---|
| Root/Panel | 가운데 (0, 14) 120×34, 창 테두리 그림 |
| Label / Result | "복권" 9 cream0 / 결과 12(자동 8~12) |
| Frame | 금색 틀(선은 코드가 붙임) |
| 연결 | 내 잔고·레버리지 자리 = 튜토리얼의 `MyBalanceArea`·`LeverageArea`, 폰 = `PhoneClosed` |

## 확인 (2026-11-23, MCP Play — 복권 결과 고정은 Play 중에만)
| 결과 | 화면 |
|---|---|
| 레버리지 | "레버리지 +1" 금색, 레버리지 상자 틀 |
| 잔고(가득) | "꽝 (잔고가 이미 가득)" 회색, 틀 없음 |
| 아이템 | "아이템 +1 (찌라시)" 금색, 내 폰 틀 |
