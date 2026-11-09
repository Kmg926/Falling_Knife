# Ticket 19 초안: 복권을 쓴다 (25%씩 잔고·레버리지·아이템·꽝)

> **초안**이다(Assets 밖). **직접 타이핑**(CLAUDE.md 4장). 반영 후 AI는 리뷰만.
> 근거: `../../spec.md` (C8), Grilling Q6·Q13. 구현 전 정상 버전: 커밋 `fcf7e13`

## 0. 바꿀 것
| 파일 | 추가 | 전문 |
|---|---|---|
| `Participant.cs` | `StartingBalance` 1줄 + 생성자 1줄 + `Heal()` | 같은 폴더 |
| `GameManager.cs` | `LotteryPrize` enum, 테스트용 복권 칸 2개, 복권 `case`, `UseLottery()` | 같은 폴더 |
| `UIManager.cs`·Input·에셋·Scene | **없음** (결과는 기존 결과 줄에 나옴) | — |

순서: Participant → GameManager (중간 에러 없음). 반영 뒤 GameManager Inspector에 `테스트용 복권` 칸이 생긴다(기본값 꺼짐 — 그대로 두면 됨).

## 1. `Participant.cs`
```csharp
    public int StartingBalance { get; }
…
        StartingBalance = startingBalance;      // 생성자 안
…
    public bool Heal(int amount)
    {
        if (Balance >= StartingBalance) return false;
        Balance += amount;
        if (Balance > StartingBalance) Balance = StartingBalance;
        return true;
    }
```
| 줄 | 왜 |
|---|---|
| `StartingBalance { get; }` | 라운드 시작 잔고를 기억. `get`만 있어 생성자에서 한 번 정하면 못 바꾼다(`Name`과 같은 방식) |
| `if (Balance >= StartingBalance) return false;` | 이미 가득이면 회복 못 함 → `false` = "적용할 수 없음" → 꽝(Q6, GDD "적용할 수 없으면 꽝") |
| `if (Balance > StartingBalance) Balance = StartingBalance;` | 넘치지 않게 자른다. `TakeDamage`가 0 밑으로 안 내려가게 자르는 것과 짝 |
| 반환 `bool` | 결과를 쓰는 쪽(GameManager)이 "됐나 안 됐나"로 문구를 고른다. `AddItem`과 같은 방식 |

## 2. `GameManager.cs`
### 2-1. 파일 맨 위 (`PlayerAction` 아래)
```csharp
public enum LotteryPrize { Heal, Leverage, Item, Nothing }
```
복권 결과 4가지 이름. `ItemType`과 같은 이유(문자열 대신 이름). 0~3 번호를 가진다.

### 2-2. 테스트용 칸 (`fixedItemIndex` 아래)
```csharp
    [Header("테스트용 복권")]
    [SerializeField] bool useFixedLottery = false;
    [SerializeField] LotteryPrize fixedLottery = LotteryPrize.Nothing;
```
- Inspector에서 결과를 하나 골라 고정(Q13). enum 칸은 Inspector에 **드롭다운**으로 보인다.
- 최종 빌드 전 끈다(gdd-changes #35).

### 2-3. `switch`에 `case` 추가 (거래 정지 `case` 아래)
```csharp
            case ItemType.Lottery:
                Player.RemoveItemAt(ConfirmingSlot);
                UseLottery(Player);
                break;
```
이제 4종 모두 자기 `case`가 생겼다. `default`(아직 쓸 수 없음)로 가는 아이템은 없다.

### 2-4. 새 함수 `UseLottery()` (`DrawTop()` 위)
```csharp
        LotteryPrize prize = useFixedLottery ? fixedLottery : (LotteryPrize)Random.Range(0, 4);
```
| 왜 |
|---|
| `조건 ? A : B`: 고정 옵션이 켜져 있으면 고른 결과, 아니면 무작위 |
| `(LotteryPrize)Random.Range(0, 4)`: 0~3 → 4가지 중 하나, 각 25%(16의 아이템 뽑기와 같은 방법) |

```csharp
        switch (prize)
        {
            case LotteryPrize.Heal:
                result = user.Heal(1) ? "잔고 +1" : "꽝 (잔고가 이미 가득)";
                break;
            case LotteryPrize.Leverage:
                if (Leverage.Value < LeverageCounter.Max) { Leverage.Increase(); result = …; }
                else { result = "꽝 (레버리지가 이미 최대)"; }
                break;
            case LotteryPrize.Item:
                ItemType bonus = NextItem();
                result = user.AddItem(bonus) ? $"아이템 +1 ({ItemName(bonus)})" : "꽝 (아이템이 이미 가득)";
                break;
            default:
                result = "꽝";
                break;
        }
```
| 갈래 | 왜 |
|---|---|
| `Heal` | `Heal(1)`이 `false`면 꽝 문구. 괄호 안에 이유를 붙여 "왜 꽝인지" 보이게 |
| `Leverage` | `LeverageCounter.Increase()`는 3에서 조용히 멈춘다(반환값 없음) → **먼저** `Max`와 비교해서 적용 가능한지 확인. 레버리지는 공용이라 내 차례 뒤 상대에게 넘어가도 남는다(GDD 규칙) |
| `Item` | `NextItem()`: 16의 지급과 같은 뽑기(고정 문자열이 켜져 있으면 그다음 글자). `AddItem`이 `false`면 꽝 |
| `default` | `Nothing`(꽝) |
| 아이템 가득 꽝 | 복권을 먼저 빼므로(최대 4 → 3) 실제로는 거의 생기지 않는다(Q6에서 확인한 대로). 그래도 `AddItem`이 `false`를 돌려주면 꽝이 되도록 안전하게 둠 |

```csharp
        LastResult = $"{user.Name}: 복권 → {result}";
```
결과 줄 예: `플레이어: 복권 → 레버리지 +1 (레버리지 1)`, `플레이어: 복권 → 꽝 (잔고가 이미 가득)`. 차례는 그대로(U5).

## 3. 수동 테스트 (Ticket 19)
> 고정 복권·아이템·덱은 Play 중에만. 컴파일 시각 확인, Scene 저장 전 변경 확인.

| # | 설정 | 입력 | 기대 |
|---|---|---|---|
| T116 | 고정 `Heal`, 잔고 10, 아이템 `L` | 복권 | `꽝 (잔고가 이미 가득)` (C8 적용 불가) |
| T117 | 고정 `Heal`, 잔고를 7로 줄인 뒤 | 복권 | `잔고 +1` → 8 (C8) |
| T118 | 고정 `Leverage` | 복권 (레버리지 0 / 3) | `레버리지 +1 (레버리지 1)` / `꽝 (레버리지가 이미 최대)` (C8) |
| T119 | 고정 `Item`, 아이템 `L J` | 복권 | `아이템 +1 (찌라시)` (C8) |
| T120 | 고정 `Nothing` | 복권 | `꽝` (C8) |
| T121 | 무작위 | MCP로 복권 결과 4000번 | 4가지 각 약 25% (C8) |
| T122 | 아무 결과 | 복권 | 아이템 −1, 아직 내 차례 |
| T123 | `clown-ai` T65~T68 | 아이템 안 씀 | 판정·차례 같음, Console 오류 0 |

## 4. 반영 후
"반영했어" → AI 리뷰 → T116~T123 → Ticket 20(광대 아이템).
