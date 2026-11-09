# Ticket 18 초안: 거래 정지로 상대 턴을 한 번 건너뛴다

> **초안**이다(Assets 밖). **직접 타이핑**(CLAUDE.md 4장). 반영 후 AI는 리뷰만.
> 근거: `../../spec.md` (C7), Grilling Q8·Q16. 구현 전 정상 버전: 커밋 `e9b7e16`

## 0. 바꿀 것
| 파일 | 추가 | 전문 |
|---|---|---|
| `Participant.cs` | 정지 상태 1줄 + `Halt()`·`ClearHalt()` | 같은 폴더 |
| `GameManager.cs` | 거래 정지 `case`, `UseTradingHalt()`, `PassTurn()` 안 건너뛰기 6줄 | 같은 폴더 |
| `UIManager.cs` | 잔고 옆 `(정지)`, 확인 줄 `사용 불가` | 같은 폴더 |
| Input·에셋·Scene | **없음** | — |

순서: Participant → GameManager → UIManager (중간 에러 없음).

## 1. `Participant.cs`
```csharp
    public bool IsHalted { get; private set; }

    public void Halt() { IsHalted = true; }
    public void ClearHalt() { IsHalted = false; }
```
| 왜 |
|---|
| 정지는 **그 사람의 상태**라서 Participant에 둔다(GDD "상대 잔고 옆 정지 표시" — 잔고처럼 사람마다 하나) |
| `private set` + 함수 2개: 밖에서는 "건다/푼다"만 할 수 있다(`Balance`를 `TakeDamage`로만 바꾸는 것과 같은 방식) |
| 라운드 시작 초기화 코드 없음: `StartRound()`가 `new Participant(...)`로 새로 만든다(아이템 슬롯과 같음) |

## 2. `GameManager.cs`
### 2-1. `switch`에 `case` 추가 (차트 분석 `case` 아래)
```csharp
            case ItemType.TradingHalt:
                if (Clown.IsHalted)
                {
                    LastResult = $"{ItemName(item)}: 사용 불가 (상대가 이미 정지 상태)";
                    break;
                }
                Player.RemoveItemAt(ConfirmingSlot);
                UseTradingHalt(Player);
                break;
```
| 줄 | 왜 |
|---|---|
| `if (Clown.IsHalted) { …; break; }` | GDD "상대가 이미 정지 상태가 아닐 때"(정지 중복 불가, F05). `break`로 `switch`를 빠져나가 **아이템을 빼지 않는다** → 남음(Q8) |
| 빼고 → 효과 | 다른 `case`와 같은 순서 |

### 2-2. 새 함수 `UseTradingHalt()` (`DrawTop()` 위)
```csharp
    void UseTradingHalt(Participant user)
    {
        Opponent.Halt();
        LastResult = $"{user.Name}: 거래 정지 사용 → {Opponent.Name} 다음 턴 건너뜀";
    }
```
| 왜 |
|---|
| 여기서는 **표시만 건다.** 실제로 건너뛰는 일은 차례가 넘어가는 순간(`PassTurn`)에 한다 |
| `Opponent` = 지금 차례가 아닌 쪽. 내 차례에 쓰면 광대. Ticket 20에서 광대가 쓰면 자동으로 플레이어가 된다(같은 코드) |

### 2-3. `PassTurn()` — 핵심
```csharp
    void PassTurn()
    {
        Current = Opponent;
        Selected = PlayerAction.Hold;

        if (Current.IsHalted && !IsRoundOver)
        {
            Current.ClearHalt();
            LastResult += $"\n{Current.Name}: 거래 정지로 턴을 건너뜀";
            Current = Opponent;
        }
    }
```
| 줄 | 왜 |
|---|---|
| 왜 `PassTurn`인가 | 차례가 넘어가는 길이 여러 개다: 보유+하락(`ResolveHold`), 매도(`ResolveSell`). 두 길 모두 `PassTurn()`을 지난다 → **여기 한 곳에서** 검사하면 빠지는 길이 없다(Spec 결정). 17의 `DrawTop()`과 같은 생각 |
| `Current = Opponent;` (첫 줄) | 일단 상대에게 넘긴다 |
| `if (Current.IsHalted …)` | 넘겨받은 사람이 정지면 |
| `ClearHalt()` | 1회 건너뛰면 풀린다(GDD "1회") |
| `LastResult += …` | 결과 줄 아래에 건너뜀 문구를 붙인다. 나중에 `RevealRoutine`이 맨 앞에 `플레이어:`를 붙여도 이 줄은 그대로 아래에 남는다 |
| `Current = Opponent;` (두 번째) | 다시 넘긴다 = 원래 행동한 사람에게 돌아온다. 레버리지는 건드리지 않음(Q8) |
| `&& !IsRoundOver` | 라운드가 끝났으면(파산) 건너뜀 문구를 띄우지 않는다. 클리어 화면에 `광대: 거래 정지로…`가 나오는 어색함 방지 |

- 내 차례에 건너뛰어 다시 내 차례가 되면 `IsClownTurn`이 false라 광대 턴이 시작되지 않는다 → 바로 내가 다시 고른다. 코드 추가 없음.

## 3. `UIManager.cs`
```csharp
            $"잔고  플레이어 {game.Player.Balance}{HaltMark(game.Player)} / 광대 {game.Clown.Balance}{HaltMark(game.Clown)}\n" +
…
    string HaltMark(Participant who)
    {
        return who.IsHalted ? " (정지)" : "";
    }
```
- 잔고 옆 `광대 9 (정지)`(Q16). 정지가 아니면 빈 글자.
- 확인 줄: 거래 정지를 고른 상태에서 상대가 이미 정지면 끝에 ` (사용 불가: 상대가 이미 정지 상태)`를 붙여 **쓰기 전에** 알려 준다.

## 4. 수동 테스트 (Ticket 18)
> 고정 아이템·덱·잔고는 Play 중에만. 컴파일 시각 확인, Scene 저장 전 변경 확인.

| # | 설정 | 입력 | 기대 |
|---|---|---|---|
| T110 | 아이템 `S`, 덱 `U D U` | 1 → Space | `플레이어: 거래 정지 사용 → 광대 다음 턴 건너뜀`, `광대 10 (정지)`, 아이템 −1, 아직 내 차례 (C7) |
| T111 | 이어서 | 매도 | `플레이어: 매도 → 상승! …` 아래 `광대: 거래 정지로 턴을 건너뜀`, 다시 `차례: 플레이어`, `(정지)` 사라짐, 레버리지 그대로 (C7) |
| T112 | 아이템 `S`, 덱 `D U U` | 정지 → 보유 | `보유 → 하락! 플레이어 -1` 아래 건너뜀 → 다시 내 차례 (보유+하락 길도 같음) |
| T113 | 아이템 `S S` | 정지 → 2번째 거래 정지 확인 → Space | 확인 줄에 `(사용 불가: …)`, Space → `거래 정지: 사용 불가 …`, 아이템 남음 (C7) |
| T114 | 아이템 `S`, 덱 `D`, 잔고 1 | 정지 → 매도 | 광대 파산 → `라운드 1 클리어!`, 건너뜀 문구 없음 |
| T115 | `clown-ai` T65~T68 | 아이템 안 씀 | 판정·차례 같음, Console 오류 0 |

## 5. 반영 후
"반영했어" → AI 리뷰 → T110~T115 → Ticket 19(복권).
