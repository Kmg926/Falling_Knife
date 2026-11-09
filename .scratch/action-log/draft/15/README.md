# Ticket 15 초안: 지나간 행동은 화면에 기록으로 남지 않는다 (로그 제거)

> 스크립트는 **직접 수정**(지우기). Scene `Log Text` 삭제는 AI가 MCP로(코드 반영 **뒤**).
> 결과 파일은 Ticket 13 완료 시점(커밋 `2ede120`)의 `GameManager.cs`·`UIManager.cs`와 **똑같다** → 같은 폴더 전문과 비교하면 된다.

## 1. `GameManager.cs` — 지울 것
| 위치 | 지울 줄 |
|---|---|
| 맨 위 | `using System.Collections.Generic;` |
| `LastResult` 아래 | 주석 1줄 + `readonly List<string> log = …;` + `public IReadOnlyList<string> Log => log;` (+ 빈 줄) |
| `StartRound()` | `AddLog($"— 라운드 {RoundNumber} —");` |
| `Restart()` | `log.Clear();` |
| `RevealRoutine()` | `AddLog(LastResult);` / 주석 `// 라운드가 끝났으면…` + `if (IsRoundCleared) AddLog(…)` 3줄 (+ 빈 줄) |
| `Reload()` | `string line = …;` + `LastResult += "\n" + line;` → **한 줄로 되돌림**: `LastResult += $"\n덱 재장전 (하락{deck.RemainingDown} / 상승{deck.RemainingUp})";` |
| `PassTurn()` 위 | 주석 `// 기록 맨 아래에…` + `void AddLog(string line) { log.Add(line); }` 함수 전체 |

**남기는 것(Ticket 13):** `Participant actor = Current;`, `LastResult = $"{actor.Name}: {LastResult}";`, 보유 상승 문구.

## 2. `UIManager.cs` — 지울 것
| 위치 | 지울 줄 |
|---|---|
| 맨 위 | `using System.Collections.Generic;` |
| 필드 | `[Header("행동 로그 (왼쪽 아래)")]`, `logText`, `logLines` (+ 빈 줄) |
| `Refresh()` 끝 | `logText.text = RecentLog(game.Log);` (+ 앞 빈 줄) |
| 아래 | `RecentLog()` 함수 전체 |

## 3. 순서 (에러 없이)
1. **UIManager 먼저** 지우고 저장 (UIManager가 `game.Log`를 안 쓰게 됨)
2. GameManager 지우고 저장 (반대 순서면 잠깐 CS1061 `Log` 없음)
3. Unity 창 클릭 → 컴파일 → Console 에러 0
4. "반영했어" → AI가 Scene 저장 전 변경 확인 → `Canvas/Log Text` 삭제(MCP) → 저장 → 테스트

- `Log Text` 오브젝트를 코드보다 먼저 지우면, 반영 전 Play 때 빈 칸 참조로 NullReferenceException이 난다 → 그래서 코드 뒤에 지운다.
- 코드를 다 지운 뒤에도 Scene에는 UIManager의 옛 `Log Text` 칸 값이 남아 있을 수 있다(쓰이지 않는 데이터). 오브젝트 삭제 후 Scene을 저장하면 정리된다.

## 4. 테스트
| # | 방법 | 기대 |
|---|---|---|
| T90 | Play·캡처 | 왼쪽 아래 비어 있음, Hierarchy에 `Log Text` 없음 |
| T91 | `D D U U U`(잔고 2, Play 중에만) | 결과 줄 문구 13과 같음, 재장전 순간 `덱 재장전 (하락2 / 상승3)` |
| T92 | 광대 턴 시각 | 생각 1초 → 공개 0.5초 |
| T93 | `clown-ai` T65~T68 | 판정·차례 같음, Console 오류 0 |
