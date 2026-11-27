# 121 줄별 설명 — 구제 금융 → 위험 채권(반반 도박)

> 2026-11-27, Grilling `../../grilling.md`(사용자 "위험 채권 채택. 반영해."). AI가 Scripts에 직접 씀. 각 파일 `*.before.cs` → `*.cs`.
> enum 이름 `ItemType.Bailout`·파일 이름 `item-bailout*.png`·에셋 `Bailout.asset`은 그대로(Scene·에셋 연결을 지키려고). 보이는 이름·효과만 바뀜.

## GameManager.cs
| 부분 | 무엇을 | 왜 |
|---|---|---|
| `UseConfirmedItem` | `Bailout`을 배당금(회복)과 떼어 `UseRiskBond(Player)` — 가득이어도 막지 않음 | Q3 |
| `UseClownItem` | `Bailout` → `UseRiskBond(Clown)` | 광대도 같은 효과 |
| `HealAmount` | 배당금만 1, 나머지 0 | 위험 채권은 회복템이 아님 → "가득이면 못 씀" 검사에서 빠짐 |
| `bondWinChance 0.5`·`bondWin 2`·`bondLose 1` | 숫자 칸(Inspector에서 조정) | |
| `UseRiskBond(user)` | `Random.value < 0.5`면 `Heal(2)`(시작 잔고까지, 가득이면 "+0 (가득)"), 아니면 `TakeDamage(1)` + 피해 소리·깜빡임 + `LastDamage`(결과 화면 원인·결정타) | Q2·Q4 — 잔고 0이 되면 `IsGameOver`/`IsRoundOver`가 저절로 참(파산 판정은 잔고로 계산) |
| `LastBondWon`·`LastBondText`·`PlayerBondCount` | 결과 신호(판정엔 안 씀) | 결과 판·광대 대사 |
| `ItemResultLine` | 위험 채권: 이기면 `LotteryHeal`("운이 좋군."), 지면 `LotteryNothing`("…꽝이군.") | 도박이라 복권 대사를 같이 씀 |

## ClownAI.cs
③-1 회복은 배당금만(가득이면 안 씀). ③-2 새 줄: 잔고 ≤ 기준값(3) **그리고 ≥ 2**이고 위험 채권이 있으면 사용 — 1이면 지는 순간 파산이라 안 씀(Q7).

## LotteryPopupView.cs
`PlayerBondCount`가 바뀌면 `ShowBond()`: 제목 "위험 채권", 결과 한 줄, 성공 금색·높은 소리 / 실패 회색·낮은 소리, 내 잔고 줄(MyBalanceArea = 전광판 아래 줄)을 금색 틀로 반짝임. 복권·작전과 같은 판(Q5).

## 글
`ClownSpeechView` "위험 채권에 걸겠다."(코드 기본값 + GameScene 값), `TutorialGuide` #58, `TitleMenu.itemShort[5]` "반반 도박: 잔고 +2 또는 −1."(코드 + TitleScene 값), 주석(`ItemType`·`ClownLine`·`Participant`). 아이템 데이터(MCP): 이름 "위험 채권", 분류 "도박"(복권과 같음), 설명 "50% 확률로 잔고를 2 얻는다. 아니면 잔고를 1 잃는다."

## 그림
`tools/make_art_hd_items.py riskbond`: `icon_riskbond` — 크림색 증서(테·글 줄·도장 자리) + 빨강·검정 대각선 위험 띠 + "+"·"−". `Sprites/HUD/item-bailout.png`(42×32)·`Sprites/Closeup/item-bailout-big.png`(68×52) 덮어씀.

## 시험(MCP, Play 중에만 값 바꿈)
| 경우 | 결과 |
|---|---|
| 잔고 4/7, 성공 | 6/7, 결과 판 "잔고 +2", 전광판 ▲2·내 줄 금색 틀 |
| 잔고 1, 실패 | 0 → GAME OVER, 원인 "R1, 나 위험 채권, 피해 1" |
| 잔고 5/5, 성공 | "잔고 +0 (가득)", 아이템 씀(사용 1회) |
| 광대 판단 | 잔고 3·2 → 사용, 1·5 → 사용 안 함 |
| 광대 사용·실패 | 5 → 4, 대사 UseBailout → LotteryNothing |
| 폰 상세 | 이름·설명·새 아이콘 |
