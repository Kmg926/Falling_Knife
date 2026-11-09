# Ticket 20 초안: 광대가 아이템을 쓴다 (GDD 광대 규칙 ①~④)

> **초안**이다(Assets 밖). **직접 타이핑**(CLAUDE.md 4장). 반영 후 AI는 리뷰만.
> 근거: `../../spec.md` (C9, C10), Grilling Q7·Q11·Q14, Spec 채택 `ClownMove`. 구현 전 정상 버전: 커밋 `5b9494a`. gdd-changes #21(임시) 해소.

## 0. 바꿀 것
| 파일 | 할 일 | 전문 |
|---|---|---|
| `ClownAI.cs` | `ClownMove` 추가, `Decide`를 규칙 ①~④로 **새로 씀** | 같은 폴더 |
| `Participant.cs` | `RemoveItem(ItemType)` 1개 | 같은 폴더 |
| `GameManager.cs` | 복권 기준값 칸, `clownKnownNext`, 차트 분석 2줄, `DrawTop` 1줄, `ClownTurnRoutine` 고침, `UseClownItem()` | 같은 폴더 |
| UIManager·Input·에셋 | **없음** | — |

⚠ 순서: **Participant → ClownAI → GameManager**. ClownAI를 바꾸면 GameManager의 옛 호출 `ClownAI.Decide(down, up)`이 맞지 않아 **잠깐 에러(CS7036 등)** → GameManager까지 저장하면 사라진다.

## 1. `ClownAI.cs`
### 1-1. `ClownMove` (파일 위쪽)
```csharp
public struct ClownMove
{
    public bool UsesItem;
    public ItemType Item;
    public PlayerAction Action;

    public static ClownMove UseItem(ItemType item) { return new ClownMove { UsesItem = true, Item = item }; }
    public static ClownMove Act(PlayerAction action) { return new ClownMove { UsesItem = false, Action = action }; }
}
```
| 줄 | 왜 |
|---|---|
| 왜 필요한가 | 예전 `Decide`는 `PlayerAction`(보유/매도)만 돌려줬다 → "찌라시를 쓴다"를 표현할 수 없다(Spec 채택) |
| `struct` | 값 몇 개를 묶은 **작은 쪽지**. class와 비슷하지만 복사해서 건네진다(작고 잠깐 쓰는 값에 적합). `Vector2`·`Color`도 struct |
| `UsesItem` true면 `Item`을, false면 `Action`을 본다 | 쪽지에 "아이템 사용: 찌라시" 또는 "행동: 매도" 둘 중 하나가 적혀 있는 것 |
| `static ClownMove UseItem(...)` / `Act(...)` | 쪽지를 만드는 지름길. `ClownMove.UseItem(ItemType.Rumor)`처럼 읽기 쉽게 |
| `new ClownMove { UsesItem = true, Item = item }` | 만들면서 칸을 채우는 문법(`new RoundConfig { startingBalance = 10, … }`과 같음) |

### 1-2. `Decide` — GDD 규칙 ①~④를 위에서부터
```csharp
    public static ClownMove Decide(int remainingDown, int remainingUp, IReadOnlyList<ItemType> items,
                                   int balance, bool opponentHalted, Candle? knownNext, int lotteryThreshold)
```
| 입력 | 왜 |
|---|---|
| 남은 하락·상승 | 규칙 ④ (예전과 같음) |
| `items` | 광대가 가진 아이템 (읽기 전용) |
| `balance`, `lotteryThreshold` | 규칙 ③ (잔고 3 이하, Inspector 값) |
| `opponentHalted` | 규칙 ② (플레이어가 이미 정지면 안 씀) |
| `knownNext` | 규칙 ① (차트 분석으로 아는 다음 캔들, 모르면 null) |
- 광대가 **아는 것만** 받는다(09와 같은 생각). 덱 순서 전체는 주지 않는다.

| 규칙 | 코드 | 왜 |
|---|---|---|
| ① | `knownNext.HasValue`면 상승 → 보유, 하락 → 매도 / 차트 분석 있으면 사용 | GDD ①. 쓰고 나면 다시 물어보므로 다음 번엔 `knownNext`가 채워져 있다 |
| ② | 거래 정지 있고 `!opponentHalted` → 사용 | GDD ② |
| ③ | `balance <= lotteryThreshold` 이고 복권 → 사용 | gdd-changes #32 (잔고 1 → 3 이하) |
| ④ | 하락 비율 ≥ 0.5: 찌라시 있으면 찌라시, 없으면 매도 / < 0.5: 보유 | GDD ④ "찌라시를 쓰거나 매도" |
- 위에서 먼저 맞는 것을 `return` → **우선순위 = 줄 순서**.
- `Has()`: 목록에 그 종류가 있는지 `foreach`로 찾는다.

## 2. `Participant.cs`
```csharp
    public bool RemoveItem(ItemType item)
    {
        return items.Remove(item);
    }
```
플레이어는 **슬롯 번호**로 고르지만(1~4 키), 광대는 **종류**로 고른다. `List.Remove`는 앞에서 처음 만나는 같은 값 하나를 뺀다.

## 3. `GameManager.cs`
| # | 바꿀 곳 | 왜 |
|---|---|---|
| ① | `[SerializeField] int clownLotteryThreshold = 3;` (아이템 칸 아래) | 규칙 ③ 기준값(Q7) |
| ② | `Candle? clownKnownNext;` (`PlayerKnownNext` 아래) | 광대가 아는 캔들. `private` → UI가 못 읽음 → 플레이어에게 숨김(Q10) |
| ③ | `StartRound()`·`DrawTop()`에 `clownKnownNext = null;` | 17과 같은 이유: 꺼내면 잊는다 |
| ④ | `UseChartAnalysis()`: `if (user == Player) PlayerKnownNext = …; else clownKnownNext = …;` | 쓴 사람만 안다. 결과 줄은 둘 다 `이름: 차트 분석 사용`(광대 결과는 안 보임) |
| ⑤ | `ClownTurnRoutine()` (아래) | 아이템 → 다시 생각 |
| ⑥ | 새 함수 `UseClownItem()` | 광대 아이템 사용 |

```csharp
        ClownMove move = ClownAI.Decide(deck.RemainingDown, deck.RemainingUp, Clown.Items,
                                        Clown.Balance, Player.IsHalted, clownKnownNext, clownLotteryThreshold);

        if (move.UsesItem)
        {
            UseClownItem(move.Item);
            ui.Refresh(this);
            StartCoroutine(ClownTurnRoutine());
            yield break;
        }

        Selected = move.Action;
        StartCoroutine(RevealRoutine(Selected));
```
| 줄 | 왜 |
|---|---|
| `if (move.UsesItem)` | 쪽지가 "아이템"이면 |
| `UseClownItem(...)` → `ui.Refresh` | 쓰고 결과 줄을 보여 준다(`광대: 거래 정지 사용 → …`) |
| `StartCoroutine(ClownTurnRoutine());` | **다시 생각 1초**(Q11) → `Decide`를 ①부터 다시(Q14). 찌라시 뒤 비율이 바뀌면 다른 판단 |
| `yield break;` | 이 코루틴은 여기서 끝(새로 시작한 코루틴이 이어감) |
| 아니면 `Selected = move.Action;` → 공개 | 예전과 같은 길 |
- 아이템 사용으로는 아무도 파산하지 않으므로 다시 생각해도 라운드가 끝나 있지 않다. 찌라시로 덱이 비면 `UseRumor`가 재장전까지 한다.

```csharp
    void UseClownItem(ItemType item)
    {
        Clown.RemoveItem(item);
        switch (item)
        {
            case ItemType.ChartAnalysis: UseChartAnalysis(Clown); break;
            case ItemType.Rumor: UseRumor(Clown); break;
            case ItemType.TradingHalt: UseTradingHalt(Clown); break;
            case ItemType.Lottery: UseLottery(Clown); break;
        }
    }
```
- **효과 함수는 플레이어와 같은 것**을 쓴다(16~19에서 `Participant user`를 받게 만든 이유). 거래 정지의 `Opponent`는 광대 차례라 자동으로 플레이어 → 18의 `PassTurn()`이 플레이어 턴을 건너뛰고 광대에게 돌려준다.
- 한 줄 `case …: …; break;` 모양: 짧은 갈래는 한 줄로 써도 같다.

## 4. 수동 테스트 (Ticket 20)
> 고정 아이템 문자열은 지급 순서 **플레이어 2 → 광대 2**. 고정 덱·잔고는 Play 중에만. 퇴행은 `Items Per Grant`를 Play 중에만 0으로.

| # | 설정 | 기대 |
|---|---|---|
| T124 | MCP `ClownAI.Decide` 상황표 | ① 알면 그대로(상승 보유/하락 매도), 차트 분석 우선 ② 정지(플레이어 정지면 건너뜀) ③ 잔고 3 → 복권, 4 → 안 씀 ④ 비율 ≥ 0.5 + 찌라시 → 찌라시 / 없으면 매도 / < 0.5 → 보유, 우선순위 C > S > L (C9) |
| T125 | 아이템 `J J S C`(광대 S·C), 덱 `U D U D U`, 내 매도 | 광대: `차트 분석 사용`(생각 1초) → 다음 하락을 알고 **매도** → `플레이어 -1`. 화면에 광대 차트 결과 없음 (C9) |
| T126 | `J J S L`(광대 S·L), 같은 덱 | 광대: `거래 정지 사용 → 플레이어 다음 턴 건너뜀` → 다시 생각 → 매도 → 내 차례가 오면 `플레이어: 거래 정지로 턴을 건너뜀` → 광대 계속 (C9) |
| T127 | `J J L L`(광대 L·L), 광대 잔고 3, 고정 복권 `Heal` | 복권 → 4 → 더 안 씀(잔고 4 > 3) → 규칙 ④ (C9) |
| T128 | `C C J J`(광대 J·J), 덱 `U D D D U`, 내 매도 | 광대 남은 3/1 → 찌라시 → 2/1 → 찌라시 → 1/1 → 매도 (C9) |
| T129 | 위 흐름 시각 | 아이템마다 생각 1초 + 결과 줄 (C9) |
| T130 | `Items Per Grant` 0(Play 중), `clown-ai` T65~T68 | 예전과 같음 (C10) |

## 5. 반영 후
"반영했어" → AI 리뷰 → T124~T130 → 직접 플레이(광대가 상대처럼 느껴지는가 — 09·13에서 보류한 경험 확인) → gdd-changes #21 해소 기록 → 아이템 기능 마무리.
