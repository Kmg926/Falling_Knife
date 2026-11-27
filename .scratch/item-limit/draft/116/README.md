# 116 줄별 설명 — 거래 정지·회복템은 라운드마다 한 번만 나온다

> 2026-11-27, 사용자 "거래 정지는 한 판에 한 번씩만 나오게 해. 체력템도 마찬가지야" → AI 질문 → "라운드마다 한 번", "사람마다, 회복템은 묶어서". AI가 Scripts에 직접 씀.

## Participant.cs
| 줄 | 무엇을 | 왜 |
|---|---|---|
| `GotHalt`·`GotHeal` | 이번 라운드에 거래 정지·회복템(배당금이든 구제 금융이든)을 받았나 | Participant는 라운드마다 새로 만들어져서(`StartRound`의 `new Participant`) 따로 지우지 않아도 라운드가 바뀌면 false |
| `CanReceive(item)` | 이미 받은 제한 아이템이면 false | 무작위 고르기에서 뺄 때 씀 |
| `MarkGiven(item)` | 실제로 받았을 때 표시 | 슬롯이 가득 차 버려진 건 "나온" 게 아니므로 표시하지 않음 |

## GameManager.cs
| 줄 | 무엇을 | 왜 |
|---|---|---|
| `Give(who, item)` | `AddItem` + 받았으면 `MarkGiven` | 지급·복권 아이템 모두 같은 길로 |
| `GiveItems()` | `Give(Player, NextItem(Player))`(광대도) | 한 번에 2개 줄 때 첫 개가 거래 정지면 둘째는 거래 정지가 안 나옴(표시가 먼저 됨) |
| 복권 아이템 당첨 | `NextItem(user)` + `Give` | 복권으로 받는 것도 같은 제한 |
| `NextItem(who)` | 고정 지급(시험용)은 그대로, 무작위만 제한 | 시험 절차를 바꾸지 않게 |
| `RandomItem(who)` | 받을 수 없는 종류는 무게 0, 나머지 비율 그대로 | 나머지 아이템이 조금씩 더 자주 나옴(비율은 유지) |
튜토리얼은 각본(`tutorialPlayerItems`)이라 `AddItem` 그대로.

## 확인(MCP, Play 중 리플렉션)
새 Participant로 라운드 2000번 × 20번 받기(받자마자 씀): 한 라운드 최대 거래 정지 1·회복템 1.
