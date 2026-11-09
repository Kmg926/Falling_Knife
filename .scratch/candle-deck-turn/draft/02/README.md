# Ticket 02 초안: 보유를 실행하면 1초 공개 후 상승이면 레버리지+1·턴 유지, 하락이면 내 잔고가 줄고 턴이 넘어간다

> **초안**이다(Assets 밖). 원래 규칙은 직접 타이핑이다(CLAUDE.md 4장).
> **2026-11-02 사용자 지시로 AI가 이 폴더의 코드 6개와 `FallingKnife.inputactions`를 그대로 `Assets/_Project/`에 생성했다(예외). 이유: Parsec 원격 접속 네트워크 지연으로 직접 타이핑할 수 없는 환경(01과 같음).** 아래 3장의 1~3번은 건너뛴다.
> 용어는 `GLOSSARY.md`. 01에서 **바뀐 줄만** 설명한다.

## 1. 파일

| 파일 | 01 대비 | 한 줄 요약 |
|---|---|---|
| `LeverageCounter.cs` | 수정 | `Max`, `Increase()`, `Reset()` 추가 |
| `Participant.cs` | 수정 | `TakeDamage()` 추가 |
| `CandleDeck.cs` | 수정 | `IsEmpty`, `Draw()` 추가 |
| `GameManager.cs` | 수정 | `PlayerAction` enum, 선택·실행·1초 공개(코루틴)·보유 판정·차례 넘기기 |
| `UIManager.cs` | 수정 | 선택 줄, 결과/"공개 중..." 줄 추가 (6줄) |
| `PlayerInputHandler.cs` | **새로** | Input Actions → GameManager 전달 |
| `FallingKnife.inputactions` | **새로** | `Game` 맵: Select / Execute / Restart |

## 2. 바뀐 줄마다 왜

### LeverageCounter.cs
| 줄 | 왜 |
|---|---|
| `public const int Max = 3;` | 상한을 숫자 3으로 여기저기 쓰지 않고 이름 하나로. `const`는 "절대 안 바뀌는 값" |
| `if (Value < Max) Value++;` | 3에서 보유+상승이면 3에 머문다(Q8). `Value++`는 1 더하기 |
| `Reset()` → `Value = 0` | 하락 피해가 들어간 뒤 0(규칙 표) |

### Participant.cs
| 줄 | 왜 |
|---|---|
| `Balance -= amount;` | 피해만큼 뺀다 |
| `if (Balance < 0) Balance = 0;` | 잔고는 0 밑으로 안 내려간다(Q9). `Mathf.Max`를 안 쓴 이유: 이 클래스는 Unity 없이도 돌아가는 순수 C#으로 두려고 |

### CandleDeck.cs
| 줄 | 왜 |
|---|---|
| `IsEmpty => cards.Count == 0;` | 남은 장이 0인지. `=>`는 "읽을 때마다 계산"하는 짧은 속성 |
| `Draw()`의 `cards[0]` → `RemoveAt(0)` | 0번이 맨 위(01에서 정함). 꺼내고 지운다 |
| `Recount();` | 꺼낸 뒤 남은 개수 표시를 실제 덱과 맞춘다 |
| 주석 "비어 있을 때 부르면 안 된다" | 빈 리스트의 `[0]`은 에러. 그래서 GameManager가 `IsEmpty`를 먼저 본다 |

### GameManager.cs
| 줄 | 왜 |
|---|---|
| `using System.Collections;` | 코루틴 반환형 `IEnumerator`가 여기 있다 |
| `enum PlayerAction { Hold, Sell }` | 보유/매도 두 가지뿐. GLOSSARY: 보유=Hold, 매도=Sell |
| `revealSeconds = 1f` | 공개 1초(Q10). Inspector에서 조절 가능. `f`는 float(소수) 표시 |
| `Selected`, `IsRevealing`, `LastResult` | UI가 읽을 상태 3개. 바꾸는 건 GameManager만(`private set`) |
| `StartRound()`에 세 줄 추가 | 재시작(05) 때도 처음 상태: 보유 선택, 공개 아님, 결과 줄 빈칸 |
| `Select()`의 `if (IsRevealing) return;` | **공개 중 입력 무시를 GameManager 한 곳에서** 막는다. 입력 쪽에서 막으면 규칙이 두 군데로 흩어짐(Spec) |
| `Execute()`의 매도 분기 | 매도는 03 범위. 지금은 Console에 안내만 하고 아무 일도 안 한다 |
| `Execute()`의 `deck.IsEmpty` 분기 | 재장전은 04 범위. 빈 덱에서 `Draw()`하면 에러라서 미리 막는다 |
| `StartCoroutine(RevealRoutine())` | 코루틴 시작. 비유: "1초 뒤에 이어서 할 일"을 예약하고 게임은 계속 돌아간다 |
| `IsRevealing = true; ui.Refresh(this);` | 먼저 "공개 중..."을 화면에 띄운다 |
| `yield return new WaitForSeconds(revealSeconds);` | 여기서 1초 쉬었다가 다음 줄부터 이어서 실행. 그동안 다른 입력은 위 `return`으로 무시됨 |
| `deck.Draw()` → `ResolveHold(candle)` | 1초 뒤 맨 위 캔들 공개 → 보유 판정 |
| `IsRevealing = false; ui.Refresh(this);` | 입력을 다시 받고 결과를 보여준다 |
| `ResolveHold` 상승 분기 | 레버리지 +1(최대 3), 차례 그대로(Q8) |
| 하락 분기 `int damage = 1 + Leverage.Value;` | 피해는 1+레버리지. **Reset보다 먼저** 계산해야 한다(먼저 0으로 만들면 항상 1) |
| `Current.TakeDamage(damage)` | 보유는 자신에게 건 것이라 **지금 차례인 사람**이 맞는다 |
| `LastResult = $"하락! ... -{damage}"` | 결과 줄. 마이너스는 폰트에 확실히 있는 `-`(키보드 하이픈) |
| `PassTurn()` | 하락이면 차례 넘김 |
| `Current = (Current == Player) ? Clown : Player;` | `조건 ? A : B` = 조건이 참이면 A, 아니면 B. 플레이어면 광대로, 광대면 플레이어로(핫시트, Q1) |
| `Selected = PlayerAction.Hold;` | 차례가 바뀌면 선택을 보유로(Q21) |

> 처리 순서(피해 → 파산 판정 → 덱 소진)는 05·04에서 이 자리에 끼워 넣는다. 02에서는 잔고가 0이 돼도 게임이 계속된다(승패는 05).

### UIManager.cs
| 줄 | 왜 |
|---|---|
| `choice = ... ? "[보유]  매도" : " 보유  [매도]"` | 고른 쪽을 괄호로(Q18 ②). 앞 공백은 글자 위치를 맞추려고 |
| `message = game.IsRevealing ? "공개 중..." : game.LastResult` | 공개 중이면 "공개 중...", 끝나면 결과 줄(Q18 ⑥⑦) |
| `"선택: ..."`, 마지막 `{message}` 줄 | 01의 4줄 → 6줄 |

### PlayerInputHandler.cs (새로)
| 줄 | 왜 |
|---|---|
| `using UnityEngine.InputSystem;` | 새 Input System. 이 프로젝트는 옛 `Input.GetKey`가 꺼져 있다 |
| `InputActionReference selectAction` 등 | Inspector에 Input Actions 에셋 안의 **행동 하나**를 끌어다 놓는 칸 |
| `OnEnable()`의 `+= OnSelect` | "Select가 일어나면 OnSelect를 불러 달라"고 등록(구독). 비유: 초인종에 연결 |
| `.action.Enable()` | 행동은 켜야 입력을 받는다 |
| `OnDisable()`의 `-=`, `Disable()` | 물체가 꺼지거나 지워질 때 연결을 끊는다. 안 끊으면 사라진 물체를 부르다 에러가 날 수 있다 |
| `context.ReadValue<float>()` | Select는 축(-1~+1). A·← = -1, D·→ = +1 |
| `< 0` → Hold, `> 0` → Sell | GDD: A/← = 보유(왼쪽), D/→ = 매도(오른쪽) |
| `game.Execute()` | Space. 판정은 GameManager가 한다 |
| Restart는 연결 안 함 | 05에서 추가. 에셋에는 미리 만들어 둔다(수락 기준) |

### FallingKnife.inputactions (새로)
| 부분 | 왜 |
|---|---|
| 맵 이름 `Game` | 게임 조작 묶음. 나중에 아이템 키(1~4), Esc도 여기에 추가 |
| `Select`: Value / Axis | 왼쪽·오른쪽을 숫자 하나(-1/+1)로 받는다 |
| 1D Axis 합성 2개(`AD`, `Arrows`) | negative = A·←, positive = D·→ |
| `Execute`: Button, Space | 실행 |
| `Restart`: Button, R | 05에서 사용. 테스트용, 최종 빌드 전 제거 |
| 마우스 바인딩 없음 | ①은 키보드만(Q17) |

## 3. Unity Editor에서 할 순서

1. **Input 폴더:** `Assets/_Project` 우클릭 → `Create > Folder` → `Input`
2. **Input Actions 에셋 (둘 중 하나)**
   - (가) AI에게 `FallingKnife.inputactions`를 `_Project/Input/`에 만들어 달라고 한다(위 JSON 그대로)
   - (나) 직접: `Input` 폴더 우클릭 → `Create > Input Actions` → 이름 `FallingKnife` → 더블클릭
     - `Action Maps` + → `Game`
     - `Actions` + → `Select`: Action Type `Value`, Control Type `Axis` → `+` 옆 `Add Positive\Negative Binding` → Negative `A`, Positive `D` / 한 번 더 → Negative `Left Arrow`, Positive `Right Arrow`
     - `Execute`: Action Type `Button` → 바인딩 `Space`
     - `Restart`: Action Type `Button` → 바인딩 `R`
     - 창 위 `Save Asset`
3. **스크립트:** 01 파일 5개를 이 폴더 버전으로 고치고, `PlayerInputHandler`는 `Scripts` 우클릭 → `Create > Scripting > MonoBehaviour Script`로 새로 만든다. Console 빨간 에러 0 확인
4. **PlayerInputHandler 붙이기:** `GameManager` 물체 선택 → `Add Component` → `PlayerInputHandler`
   - `Game` 칸 ← `GameManager` 물체
   - Project 창에서 `FallingKnife` 에셋 왼쪽 ▶ 펼치기 → `Game/Select`를 `Select Action` 칸에, `Game/Execute`를 `Execute Action` 칸에 드래그
   - **칸이 비어 있으면 Play 때 에러**(NullReference)가 난다
5. **StatusText 크기:** 6줄이 되니 `Height`가 모자라면 늘린다(예: 500)
6. **Ctrl+S** → Play → **Game 창을 한 번 클릭**(키 입력이 Game 창으로 가게)

> **기존 `Settings/InputSystem_Actions`는 그대로 둔다(추천).** 프로젝트 전체 입력으로 등록돼 EventSystem(UI)이 쓸 수 있고, 우리 `FallingKnife`와 따로 동작해 충돌하지 않는다. 지울지는 정식 UI(②)에서 다시 본다.

## 4. 수동 테스트 (Ticket 02)

> Inspector 값은 **정지 상태**에서 바꾼다. 끝나면 고정 순서 끄기 → Ctrl+S.

| # | 준비 (CandleDeck) | 입력 | 기대 화면 |
|---|---|---|---|
| T07 | 기본 | A, D, ←, → 차례로 | 선택 줄이 `[보유]  매도` ↔ ` 보유  [매도]`로 바뀜 |
| T08 | 고정 `U U D U D` | Space | "공개 중..." 1초 → `상승! 플레이어 레버리지 1, 차례 유지`, 남은 상승 2 |
| T09 | 이어서 | Space | 레버리지 2, 차례 플레이어 |
| T10 | 이어서 | Space | `하락! 플레이어 -3`, 플레이어 잔고 **0**(2-3이지만 0에서 멈춤), 레버리지 0, 차례 **광대**, 선택 `[보유]` |
| T11 | 이어서 (광대 차례) | Space | `상승! 광대 레버리지 1, 차례 유지` |
| T12 | 이어서 | Space | `하락! 광대 -2`, 광대 잔고 0, 차례 플레이어 |
| T13 | 이어서 (덱 0장) | Space | 화면 변화 없음, Console "덱이 비었습니다…04" |
| T14 | Down 2 / **Up 4**, 고정 `U U U U D D` | Space 4번 | 레버리지 1→2→3→**3**(상한) |
| T15 | 기본 | Space 누르고 "공개 중..." 동안 Space·D 연타 | 캔들은 1장만 줄고 선택도 안 바뀜 |
| T16 | 기본 | D로 매도 선택 → Space | 화면 변화 없음, Console "매도 실행은 Ticket 03…" |
| T17 | 기본 | Game 창 마우스 클릭 | 아무 일 없음 |

T14 뒤에는 Up Count를 3으로 되돌린다.

> **"차례가 바뀌면 선택이 보유로" (Q21):** 02에서는 차례가 보유+하락으로만 바뀌어서 원래부터 보유가 선택돼 있다. 눈으로 확인되는 건 03(매도 후 차례 넘김)이다. 02에서는 `PassTurn()`의 `Selected = PlayerAction.Hold;` 줄로 확인한다.

## 5. 반영 후
직접 반영하면 AI에게 리뷰를 요청한다(초안과 다른 점, 버그 가능성).
