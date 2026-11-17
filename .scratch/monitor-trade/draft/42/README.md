# monitor-trade Ticket 42 초안: 매수라고 부르고, 숫자 대신 캔들 색으로 본다

> AI가 Scripts에 직접 쓴다(monitor-trade Grilling Q8 "코드는 AI가 작성", CLAUDE.md 4장 예외). 면담 대비로 읽어 둔다.
> 근거: `../../spec.md` (C4·C5·C6·C8), Grilling Q2~Q4, gdd-changes #57·#59·#60.

## 바꾼 것
| 파일 | 할 일 |
|---|---|
| `GameManager.cs` | `enum PlayerAction { Hold, Sell }` → `{ Buy, Sell }`, `ResolveHold` → `ResolveBuy`, 결과 문장 `보유 → …` → `매수 → …`, 결과 화면 원인 `… 보유` → `… 매수`, 복권 결과 문장 `복권 사용`만 + 숨은 `LastLotteryDetail` |
| `ClownAI.cs`·`PlayerInputHandler.cs`·`ActionButtonsView.cs`·`UIManager.cs`(F1 개발용 글자)·`LeverageCounter.cs`(주석) | `PlayerAction.Hold` → `Buy`, 주석·글자 "보유" → "매수" |
| `TerminalView.cs` | 모니터 `남은 n`/`하락n 상승m` 글자 칸·코드 없앰 |
| `DeckRevealView.cs` | `하락 n · 상승 m`, `섞는 중…` 글자 칸·코드 없앰(캔들 색만) |
| Scene (AI가 MCP로) | 버튼 글자 `보유` → `매수`, `RemainingText`·`DeckReveal/Caption` 끔 |
| `GLOSSARY.md` | "보유" 항목 → "매수"(옛 이름은 _Avoid_로) |

## 줄별 핵심
| 부분 | 왜 |
|---|---|
| enum 값 이름만 바꿈(`Hold` → `Buy`, 순서 그대로) | Unity는 enum을 **번호(0, 1)** 로 저장한다. 0번이 그대로 "나에게 거는 행동"이라 Scene·저장값이 바뀌지 않는다 |
| `ActionButtonsView`의 칸 이름 `holdButton`·`holdImage`는 **그대로** | `[SerializeField]` 칸 이름을 바꾸면 Inspector 연결이 끊긴다. 버튼은 Ticket 43에서 없앨 예정이라 이름만 남겨 둠 |
| `LastLotteryDetail` | 당첨 내용은 화면에 안 쓰지만(#60) 테스트에서 "효과가 그대로인가"를 확인하려면 필요 → 화면에 연결하지 않는 공개 속성으로 남김 |
| `remainingText`·`caption` 칸을 **지움** | 숫자로 개수를 알리지 않기로(#59) → 쓰지 않는 칸을 남기면 다시 보이게 될 위험. Scene의 글자 오브젝트도 끔 |
| 덱 공개 연출 동작(놓기 → 뒤집기 → 모으기)은 그대로 | "캔들 색만 보여줘"(사용자) → 색으로 늘어놓는 장면은 유지, 글자만 없앰 |
| F1 개발용 글자에는 개수가 남음 | 개발용(#41, 최종 빌드 전 제거)이라 그대로. 플레이 화면에는 안 보임 |

## 테스트 (Ticket 42)
| # | 방법 | 기대 | 결과 |
|---|---|---|---|
| C4 | 덱 공개 중·첫 화면의 보이는 글자에서 `보유`·`남은`·`하락n`·`상승n` 검색 | 0 | ✅ 0 (숨은 개발용 F1 글자·끈 오브젝트만 걸림) |
| C5 | 복권 사용 | 결과 `플레이어: 복권 사용`, 숨은 당첨 내용은 따로 | ✅ (숨은 값 `꽝 (잔고가 이미 가득)`) |
| C6 | 매수 실행 | 결과 `플레이어: 매수 → 상승! …`, 코드 `PlayerAction.Buy` | ✅ |
| C8 | 퇴행 T65~T68·T143(기대 문장만 매수로) | 같음 | ✅ |
