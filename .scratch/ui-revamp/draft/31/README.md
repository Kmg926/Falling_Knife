# ui-revamp Ticket 31 초안: 덱 공개 연출

> AI가 Scripts에 직접 쓴다(Grilling Q8, CLAUDE.md 4장 예외). 면담 대비로 읽어 둔다.

## 바꾼 것
| 파일 | 할 일 |
|---|---|
| `CandleDeck.cs` | `FillCount`(채운 횟수) +1 |
| `GameManager.cs` | `IsPresenting`·`SkipRequested`·`LastFillWasRoundStart`, `BeginPresentation()`·`EndPresentation()`, 입력 함수에 `IsPresenting`, 연출 중 Space = 건너뛰기, 광대 차례는 연출 끝까지 기다림, `CanPlayerAct`에 `!IsPresenting` |
| `DeckRevealView.cs` | **새 파일** — 놓기 → 잠깐 → 뒤집기·`섞는 중…` → 모으기 |
| `UIManager.cs` | 칸 1개 + `Refresh` 한 줄 |

## 줄별 핵심
| 부분 | 왜 |
|---|---|
| `FillCount` | `JustFilled`는 "켜져 있는 상태"라 재장전이 이어지면 변화를 놓칠 수 있다. 횟수가 바뀌었는지 보면 **채울 때마다 정확히 한 번** 연출 |
| `lastFillCount = fill`을 먼저 | `BeginPresentation()`이 화면을 다시 그리면서 이 `Refresh`를 또 부른다 → 먼저 바꿔 두면 두 번 시작하지 않음 |
| `IsPresenting` | 판정 코루틴은 그대로. 연출 동안 플레이어 입력(Select·SelectItem)과 **광대 생각 시작**만 미룬다(`while (IsPresenting) yield return null;`) → 판정 시간·순서 U1·U2 그대로 |
| Execute의 `SkipRequested` | 연출 중 Space = 건너뛰기(Q3). 화면이 매 프레임 이 값을 보고 바로 끝낸다 |
| `LastFillWasRoundStart` | 라운드 시작이면 느리게(약 1.5초), 재장전이면 빠르게(약 1.0초, 제안 2) |
| 하락 먼저 놓기 | 개수 세기 쉽게(제안 3). 실제 순서가 아님을 `섞는 중…`으로 |
| `Tween.MoveAnchored(… EaseOutBack)` | 위에서 살짝 떨어지며 튀는 등장(예비 동작·튀기, 조사 원칙) |
| `Wait()` | 기다리는 중에도 건너뛰기를 보고 바로 빠져나온다 |
| `Finish()` | 어떤 길로 끝나든 한 곳에서 정리(캔들 지우기·화면 끄기·`EndPresentation`) |

## 테스트 중 고친 것 (2026-11-17)
| 문제 | 고침 |
|---|---|
| 캔들을 놓는 사이 대기(`WaitForSeconds`)에서는 Space를 보지 않아 건너뛰기가 최대 0.08초 늦음(T191 ❌) | 그 대기도 `Wait()`로 바꿔 매 프레임 건너뛰기 확인 |
| 연출 도중 새 판이 시작되면 이미 지운 캔들을 `Tween`이 계속 움직이려다 `MissingReferenceException` 2번 | `DeckRevealView.Refresh`에서 `StopAllCoroutines()`로 앞 연출·캔들 움직임을 함께 멈춤 + `Tween`이 대상이 지워졌으면 조용히 끝(`target == null`) |
