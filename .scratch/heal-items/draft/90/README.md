# draft 90: 광대도 위기에 회복 아이템을 쓴다

> 2026-11-23. 코드는 AI가 Scripts에 직접(heal-items Grilling Q8, CLAUDE.md 4장 예외). `*.before.cs` = 바꾸기 전.

## 줄별 설명 (면담 대비)
### ClownAI.cs
- `Decide`에 `maxBalance`(회복 상한 = 시작 잔고) 칸 추가.
- 새 규칙 ③-1(차트 분석 ①·거래 정지 ② 다음, 복권 ③ 앞): 잔고 ≤ 기준(3) **그리고** 가득이 아니면 → 2 이상 비었으면 구제 금융 먼저·없으면 배당금, 1만 비었으면 배당금 먼저(구제 금융을 아낌)·없으면 구제 금융. 회복이 운(복권)보다 확실해서 앞에 둠.
- 아이템 하나를 쓰면 GameManager가 처음부터 다시 물어보므로(①부터), 회복 뒤 잔고가 기준을 넘으면 다음 회복 아이템은 안 씀.

### GameManager.cs
- `ClownAI.Decide(…, Clown.StartingBalance)`.
- `UseClownItem`: 배당금·구제 금융 → `UseHeal(Clown, item)`(89와 같은 회복, 상한에서 자름).
- `ItemLine`: `UseDividend`·`UseBailout`, `ItemResultLine`: 둘 다 `Healed`.

### ClownLine.cs · ClownSpeechView.cs
- enum 뒤에 `UseDividend`·`UseBailout`·`Healed`(앞 번호 그대로 → Scene에 저장된 대사 표가 안 깨짐). 표 기본값 3줄: "**배당금**을 받지." / "**구제 금융**을 쓰지." / "숨 좀 돌리지.".

## Scene (MCP)
- `ClownSpeech` 대사 표: **지금 표의 글은 그대로 두고**(사용자가 고쳤을 수 있어서) 없는 3줄만 코드 기본값에서 덧붙임 → 22줄.

## 확인 (2026-11-23, MCP)
| | 결과 |
|---|---|
| 판단(직접 호출 7가지) | 잔고 3 [배당금·구제] → 구제 / [배당금] → 배당금 / [복권·배당금] → 배당금(복권보다 먼저) / 잔고 4 → 안 씀 / [거래 정지·구제] → 거래 정지 먼저 / 1만 빔 → 배당금 먼저 / 가득 → 안 씀 |
| 실제 흐름 | 광대 잔고 3 + 구제·배당금 → "**구제 금융**을 쓰지." → 잔고 5 → "숨 좀 돌리지."(`clown-healed.png`) → 잔고 5 > 3이라 배당금은 안 씀 |
| 퇴행 | T65~T68·T143 통과, Console 오류 0 |

- 퇴행 테스트 주의(새로 알게 됨): 광대 차례 도중에 `StartGame()`을 부르면 앞 판의 광대 행동(코루틴)이 새 판에 섞임 → 첫 T65가 무효였음. Play를 새로 켜서 다시 돌림. 실제 게임은 끝난 뒤에만 새 판이라 게임 버그는 아님. `.scratch/regression/README.md`에 적음.
