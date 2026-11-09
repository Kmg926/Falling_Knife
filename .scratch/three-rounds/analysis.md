# 실행 흐름 분석: 데이터·규칙·상태 (10주차 3단계)

> 날짜: 2026-11-09 · 대상: 커밋 `406b1cf` 시점 코드(= `candle-deck-turn/draft/05`와 동일 확인), GameScene Inspector 값(MCP로 읽음)
> 요청: 10주차 프롬프트 (a) "[3라운드 진행]과 연결된 기존 기능의 실행 흐름". **이 단계에서는 코드를 수정하지 않았다.**
> 표시: ✅ 코드·Inspector에서 확인한 사실 / 🔶 실행해서 확인해야 할 부분

## 1. 데이터 (Inspector·에셋에 보관하는 값)

| 어디 | 값 (지금) | 3라운드에서 |
|---|---|---|
| `GameManager.startingBalance` | 2 ✅ | 라운드 표 1줄의 "잔고"로 옮김 |
| `GameManager.revealSeconds` | 1 ✅ | 그대로 |
| `CandleDeck.downCount / upCount` | 2 / 3 ✅ | 라운드 표로 옮김 (덱에서 칸 삭제) |
| `CandleDeck.fixedOrder` | 빈칸 ✅ | 라운드 표 줄마다 |
| `CandleDeck.useFixedOrder` | 꺼짐 ✅ | 덱에 남김 |
| `FallingKnife.inputactions` | Select / Execute / Restart ✅ | 그대로 (Space·R 재사용) |
| `UIManager.statusText`, `Galmuri11 SDF` | 연결됨 ✅ | 그대로 |

**Inspector 값만 바꿔서 해결되나?** 아니다. 라운드 값 자체는 데이터지만, "클리어 → 다음 라운드", "3라운드면 최종 승리", "패배 → 1라운드부터"는 새 **규칙**과 **상태**(라운드 번호)가 필요하다.

## 2. 규칙 (판단·계산하는 코드)

| 스크립트.메서드 | 하는 일 | 3라운드 영향 |
|---|---|---|
| `GameManager.Start` → `StartRound()` | 참가자·레버리지·선택·결과 줄을 새로 만들고 `deck.Fill()` → `ui.Refresh` | **라운드 번호를 받도록** 바꿔서 재사용(Q11) |
| `GameManager.Select(action)` | 공개 중이거나 `IsRoundOver`면 무시 | 그대로. 끝난 화면 3종 모두에서 무시(U4) |
| `GameManager.Execute()` | 공개 중이거나 `IsRoundOver`면 무시 → 빈 덱이면 무시 → `RevealRoutine` | **바뀜:** 라운드 클리어 화면에서는 "다음 라운드" |
| `GameManager.Restart()` | 공개 중이 아니고 `IsRoundOver`일 때만 `StartRound()` | **바뀜:** 항상 1라운드부터 |
| `GameManager.RevealRoutine(action)` | 1초 대기 → `Draw` → `ResolveHold/Sell` → `!IsRoundOver && deck.IsEmpty`면 `Reload` | 처리 순서는 그대로(U2) |
| `ResolveHold / ResolveSell / PassTurn` | 피해·레버리지·차례 | 그대로 |
| `GameManager.Reload()` | `deck.Fill()` + 재장전 문구(개수는 덱에서 읽음) | 그대로. 덱이 **넘겨받은 구성**을 쓰면 U3 충족 |
| `IsRoundOver` / `PlayerWon` (파생 값) | 둘 중 하나 파산 / 광대 파산 | **나뉨:** 패배 / 라운드 클리어 / 최종 승리 |
| `CandleDeck.Fill / FillShuffled / TryFillFixedOrder` | 자기 칸(`downCount` 등)으로 채우고 섞기 | **바뀜:** 넘겨받은 구성으로 |
| `CandleDeck.Draw / Recount` | 맨 위 꺼내기, 남은 개수 | 그대로 |
| `Participant.TakeDamage / IsBankrupt`, `LeverageCounter.Increase / Reset` | 0에서 멈춤, 0~3 | 그대로(U5) |
| `UIManager.Refresh(game)` | 6줄 + 끝난 화면 문구 2종(승리/패배) | **바뀜:** `라운드 N / 3` 줄, 문구 3종 |
| `PlayerInputHandler.OnSelect / OnExecute / OnRestart` | 입력 → GameManager 전달만 | 그대로 |

## 3. 상태 (실행 중 변하는 값과, 그 값을 만들고·바꾸고·초기화하는 곳)

| 상태 | 만든다 | 바꾼다 | 초기화 |
|---|---|---|---|
| `Player`, `Clown` (잔고) | `StartRound` (`new`) | `TakeDamage` | `StartRound` |
| `Current` | `StartRound` = Player | `PassTurn` | `StartRound` |
| `Leverage.Value` | `StartRound` (`new`, 0) | `Increase`, `Reset` | `StartRound` |
| `Selected` | `StartRound` = Hold | `Select`, `PassTurn` | `StartRound` |
| `IsRevealing` | `StartRound` = false | `RevealRoutine` 시작·끝 | `StartRound` |
| `LastResult` | `StartRound` = "" | `Resolve*`, `Reload`(+=) | `StartRound` |
| `CandleDeck.cards`, `RemainingDown/Up` | `Fill` | `Draw` | `Fill` (라운드 시작·재장전) |
| **(새) 라운드 번호** | — | 다음 라운드 | R·패배 후 재시작 시 1라운드 |

- **여러 기능이 같이 쓰는 상태 ✅:** `IsRoundOver`를 `Select`·`Execute`·`Restart`·`RevealRoutine`(재장전 조건)·`UIManager`가 모두 본다. 3라운드에서 이 하나를 "패배 / 클리어 / 최종 승리"로 나눌 때 **다섯 곳이 모두 영향을 받는다**. 특히 재장전 조건은 세 경우 모두 "재장전 안 함"이어야 한다(U2).
- **새 상태를 기존 상태와 분리할 수 있나?** 가능하다. 라운드 번호는 `StartRound`가 새로 만드는 값들과 달리 **라운드를 넘어 유지**되므로, `StartRound` 안에서 만들지 말고 밖(다음 라운드·재시작)에서 바꿔야 한다. 그러지 않으면 "다음 라운드가 늘 1라운드"인 버그가 생긴다.
- **덱 구성의 주인이 바뀜 ✅:** 지금은 덱이 구성을 갖고 있어서 `Reload`가 자연스럽게 같은 구성을 쓴다. 구성을 넘겨받게 되면, 덱이 **받은 구성을 저장해 두어야** 재장전이 그 라운드 구성을 쓴다(U3). 저장하지 않으면 재장전 때 구성이 0/0이 될 수 있다.

## 4. 결과 재현에 영향을 주는 것

| 요소 | 어디 | 영향 |
|---|---|---|
| 무작위 | `FillShuffled`의 `Random.Range` (시드 없음) ✅ | 매번 순서가 다름 → 줄마다 고정 순서로 재현(Q6) |
| 시간 | `WaitForSeconds(revealSeconds)` ✅ | `Time.timeScale`의 영향을 받음. 지금은 일시정지 기능이 없음 |
| 코루틴과 재시작 | `StartRound`는 진행 중인 코루틴을 멈추지 않음 ✅ | 지금은 공개 중 R을 막아서 안전. **다음 라운드(Space)도 공개 중에는 막아야 한다** 🔶 |
| 입력 콜백 | `performed`에서 바로 GameManager 호출 ✅ | 한 번 누름 = 한 번 처리. 중복 등록 없음(`OnEnable/OnDisable` 짝) |

## 5. 코드 확인 vs 실행 확인

| 항목 | 상태 |
|---|---|
| 위 1~4의 메서드·상태·호출 관계 | ✅ 코드에서 확인 |
| 덱에서 칸을 지우고 라운드 표에 새 칸을 만들면, Scene에 저장된 덱 값은 사라지고 새 칸은 **코드에 적은 기본값**으로 채워지는지 | 🔶 06 반영 후 Inspector로 확인 |
| 새 한글 글자(클리어·최종·라운드)가 Galmuri11 SDF 동적 아틀라스에 자동으로 추가되는지 | 🔶 07 Play로 확인 (지금까지는 정상 추가됨) |
| 라운드가 넘어갈 때 공개 중 입력 무시·재장전 조건이 그대로인지 | 🔶 C3·C4·C8·C9로 확인 |

## 6. Ticket별로 손댈 곳 (요약)

| Ticket | 데이터 | 규칙 | 상태 |
|---|---|---|---|
| 06 | 라운드 표 추가, 덱 칸 삭제 | 덱이 구성을 받는 함수, `StartRound`가 1번째 줄 사용, UI 라운드 줄 | 라운드 번호(항상 1) |
| 07 | — | 클리어 판정, `Execute`의 다음 라운드 분기, 최종 승리, UI 문구 | 라운드 번호 증가 |
| 08 | — | 패배 문구, `Restart` = 1라운드부터 | 라운드 번호 1로 |
