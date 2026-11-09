# Ticket 16 초안: 라운드 시작과 재장전 때 아이템 2개를 받고, 1~4로 확인해 찌라시를 쓴다

> **초안**이다(Assets 밖). 스크립트는 **직접 타이핑**(CLAUDE.md 4장). Input Actions도 **직접**(Spec 채택). 반영 후 AI는 리뷰만.
> 근거: `../../spec.md` (C1~C5, C10), Grilling Q1~Q5·Q9·Q12·Q13·Q15·Q17. 구현 전 정상 버전: 커밋 `fe388c5`

## 0. 바꿀 것 한눈에
| 파일 | 할 일 | 전문 |
|---|---|---|
| `ItemType.cs` | **새 파일** (2줄) | 같은 폴더 |
| `ItemData.cs` | **새 파일** (ScriptableObject) | 같은 폴더 |
| `Participant.cs` | 아이템 슬롯 추가 | 같은 폴더 |
| `GameManager.cs` | 지급·확인·찌라시·고정 지급 | 같은 폴더 |
| `UIManager.cs` | 아이템 줄·확인 줄 | 같은 폴더 |
| `PlayerInputHandler.cs` | 1~4·Esc 칸 5개 | 같은 폴더 |
| `FallingKnife.inputactions` | `Item1`~`Item4`·`Cancel` 액션 | 아래 6단계 |
| `ItemData` 에셋 4개 | 이름·설명 | 아래 7단계 |
| `CandleDeck.cs` | **바꾸지 않음** — 찌라시는 기존 `Draw()`로 충분(버리기 = 꺼내고 판정 안 함) | — |

## 1. 단계별 반영 (단계마다 저장 → Unity 클릭 → Console 에러 0 확인)
| 단계 | 할 일 | 끝나면 |
|---|---|---|
| 1 | 새 스크립트 2개: Project 창 `Assets/_Project/Scripts` → 우클릭 → Create → Scripting → **Empty C# Script** → `ItemType`, `ItemData` → 내용 타이핑 | 에러 0 |
| 2 | `Participant.cs` | 에러 0 |
| 3 | `GameManager.cs` | 에러 0 |
| 4 | `UIManager.cs` | 에러 0 |
| 5 | `PlayerInputHandler.cs` | 에러 0. ⚠ **아직 Play 금지**(새 칸이 비어 있으면 OnEnable에서 NullReferenceException) |
| 6 | Input Actions 추가 → PlayerInputHandler 칸 연결 | 아래 |
| 7 | ItemData 에셋 4개 → GameManager `Item Datas` 연결 → Ctrl+S | "반영했어" |

## 2. `ItemType.cs` / `ItemData.cs`
```csharp
public enum ItemType { ChartAnalysis, Rumor, TradingHalt, Lottery }
```
| 줄 | 왜 |
|---|---|
| `enum` | "정해진 몇 가지 중 하나"를 이름으로 쓰는 것. `PlayerAction { Hold, Sell }`, `Candle { Down, Up }`과 같은 방식. 문자열("찌라시")로 비교하면 오타가 나도 컴파일러가 못 잡는다 |
| 영어 이름 | 코드 이름은 영어, 화면 이름은 에셋(`displayName`)에서 → 화면 글자를 바꿔도 코드는 그대로 |
| `Rumor` | 찌라시(소문). `TradingHalt` = 거래 정지 |

```csharp
[CreateAssetMenu(fileName = "ItemData", menuName = "Falling Knife/Item Data")]
public class ItemData : ScriptableObject
{
    public ItemType type;
    public string displayName;
    [TextArea] public string description;
}
```
| 줄 | 왜 |
|---|---|
| `ScriptableObject` | **Scene에 놓는 물건이 아니라 Project 창의 "데이터 카드"**. MonoBehaviour(GameObject에 붙는 부품)와 달리 에셋 파일로 저장된다. 같은 카드를 여러 곳에서 참조할 수 있다 (직접 학습 항목, GDD 08장) |
| `[CreateAssetMenu(...)]` | Project 창 우클릭 메뉴에 `Create → Falling Knife → Item Data`를 만들어 준다 |
| `[TextArea]` | Inspector에서 설명 칸을 여러 줄 입력창으로 보여 준다 |
| 효과 코드 없음 | 효과는 GameManager가 처리(Spec #33). 판정을 한 곳에 둔다 |

## 3. `Participant.cs`
```csharp
    public const int MaxItems = 4;
    readonly List<ItemType> items = new List<ItemType>();
    public IReadOnlyList<ItemType> Items => items;

    public bool AddItem(ItemType item) { if (items.Count >= MaxItems) return false; items.Add(item); return true; }
    public void RemoveItemAt(int slot) { items.RemoveAt(slot); }
```
| 줄 | 왜 |
|---|---|
| `const int MaxItems = 4` | GDD "최대 4". `LeverageCounter.Max = 3`과 같은 방식 |
| `List<ItemType>` + `IReadOnlyList` | 슬롯 목록. 밖(UI)에는 읽기 전용 창문만 준다 — 아이템을 넣고 빼는 건 이 클래스의 함수로만 |
| `AddItem` 4개면 `false` | GDD "초과분은 버림". 받지 못한 아이템은 그냥 사라진다 |
| `RemoveItemAt(slot)` | 쓴 슬롯을 빼면 뒤 아이템이 한 칸씩 당겨진다(3번이 2번 키로) |
| 라운드 시작 초기화 | 따로 코드 없음 — `StartRound()`가 `new Participant(...)`로 새로 만들기 때문에 빈 슬롯으로 시작 |
| `using System.Collections.Generic;` | `List` |

## 4. `GameManager.cs` 바뀌는 곳
| # | 위치 | 추가·수정 | 왜 |
|---|---|---|---|
| ① | `[Header("아이템")]` | `ItemData[] itemDatas`, `int itemsPerGrant = 2` | 에셋 4개 연결 칸 / GDD "2개씩"을 Inspector 값으로 |
| ② | `[Header("테스트용 아이템 …")]` | `useFixedItems`, `fixedItems`, `int fixedItemIndex` | 테스트 고정 지급(Spec 채택: 한 칸, 앞에서부터) |
| ③ | `LastResult` 아래 | `ConfirmingSlot`(-1 = 아님), `IsConfirming` | 확인 중 상태 |
| ④ | `Start()`·`Restart()` | `fixedItemIndex = 0;` | 새 판마다 고정 문자열 처음부터 |
| ⑤ | `StartRound()` | `ConfirmingSlot = -1;`, `deck.Fill();` 다음 `GiveItems();` | 라운드 시작 지급(Q3) |
| ⑥ | `Select()` | 조건에 `\|\| IsConfirming` | 확인 중 A/D 무시(Q4) |
| ⑦ | `Execute()` | `IsRevealing` 다음에 `if (IsConfirming) { UseConfirmedItem(); return; }` | 확인 중 Space = 사용(GDD 입력표) |
| ⑧ | 새 함수 `SelectItem(int slot)` | 공개 중·라운드 끝·광대 차례·빈 슬롯이면 무시 | 1~4 |
| ⑨ | 새 함수 `CancelItem()` | 확인 닫기, 아이템 남음 | Esc |
| ⑩ | 새 함수 `UseConfirmedItem()` | `switch`: 찌라시만 효과, 나머지 `아직 쓸 수 없음`(아이템 남음) | 17~19에서 `case`를 하나씩 추가 |
| ⑪ | 새 함수 `UseRumor(Participant user)` | `deck.Draw()` → `이름: 찌라시 → 다음 캔들은 하락 (버림)` → 비면 `Reload()` | Q9. 판정·`PassTurn` 없음 → 차례 그대로(U5) |
| ⑫ | 새 함수 `GiveItems()`, `NextItem()` | 두 사람 2개씩 / 고정 문자열 → 다 쓰면 무작위 | Q2·Q3·Q13 |
| ⑬ | 새 함수 `ItemName()`, `ItemDescription()`, `DataOf()`, `CandleName()` | 에셋에서 이름·설명 찾기, 없으면 enum 이름 | UI가 쓸 화면 글자 |
| ⑭ | `Reload()` | `GiveItems();` + 문구 끝 `, 아이템 +2` | 재장전 지급(Q3) |

핵심 줄 설명:
```csharp
        switch (item)
        {
            case ItemType.Rumor:
                Player.RemoveItemAt(ConfirmingSlot);
                UseRumor(Player);
                break;
            default:
                LastResult = $"{ItemName(item)}: 아직 쓸 수 없음";
                break;
        }
```
| 줄 | 왜 |
|---|---|
| `switch (item)` | 값에 따라 갈래를 고르는 문법. `if (item == …) else if …`를 여러 번 쓰는 것과 같지만, 종류별로 한눈에 보인다 |
| `case ItemType.Rumor:` … `break;` | 이 종류일 때 할 일. `break`로 갈래를 끝낸다(빠뜨리면 컴파일 에러) |
| `default:` | 위 `case`에 없는 나머지 전부. 17~19에서 `case`가 늘어나면 `default`로 오는 종류가 줄어든다 |
| 아이템을 먼저 빼고 효과 | 효과 도중 재장전으로 아이템이 새로 들어와도 슬롯 번호가 꼬이지 않게 |

```csharp
        return (ItemType)Random.Range(0, 4);
```
| 왜 |
|---|
| 정수 `Random.Range(0, 4)` = 0~3(끝 값 미포함, Ticket 11과 같은 규칙). `(ItemType)`으로 enum 번호(0 차트 분석, 1 찌라시, 2 거래 정지, 3 복권)로 바꾼다 → 각 25%(Q2) |

```csharp
    void GiveItems()
    {
        for (int i = 0; i < itemsPerGrant; i++) Player.AddItem(NextItem());
        for (int i = 0; i < itemsPerGrant; i++) Clown.AddItem(NextItem());
    }
```
- 플레이어 2개 → 광대 2개 순서(고정 문자열을 그 순서로 읽음, Spec 채택).
- `AddItem`이 `false`(가득)여도 무시 = 버림.

## 5. `UIManager.cs`
- 상태 텍스트에 `아이템  나: 1 찌라시, 2 복권 / 광대: 거래 정지, 차트 분석` 줄(Q17). 내 것만 키 번호.
- 확인 중이면 결과 아래 `확인: 찌라시 — 다음 캔들을 공개하고 버린다. / Space 사용, Esc 취소`.
- `ItemList()`: 이름을 `string.Join(", ", …)`으로 잇는다. 없으면 `-`. 판정 없음, 읽기만.

## 6. Input Actions (직접, 편집 창)
1. Project 창 `Assets/_Project/Input/FallingKnife.inputactions` 더블클릭 → Input Actions 편집 창
2. Action Maps `Game` 선택 → Actions의 `+`로 5개 추가:
   | Action | Action Type | Binding (Path) |
   |---|---|---|
   | `Item1` | Button | Keyboard → `1` |
   | `Item2` | Button | Keyboard → `2` |
   | `Item3` | Button | Keyboard → `3` |
   | `Item4` | Button | Keyboard → `4` |
   | `Cancel` | Button | Keyboard → `Escape` |
   - 바인딩 고르기: 액션 아래 `<No Binding>` 클릭 → Path 칸 → **Listen** 누르고 키를 누르면 편함(숫자 키패드가 아니라 윗줄 숫자)
3. 편집 창 위쪽 **Save Asset** (자동 저장 체크가 켜져 있으면 생략)
4. Hierarchy에서 PlayerInputHandler가 붙은 오브젝트 선택 → Inspector `아이템` 칸 5개에 `FallingKnife.inputactions` 펼친 아래 `Game/Item1`… `Game/Cancel` 드래그 (기존 Select·Execute·Restart와 같은 방식)

## 7. ItemData 에셋 4개
1. Project 창 `Assets/_Project`에 폴더 `Data` → 그 안에 `Items` (우클릭 → Create → Folder)
2. `Items`에서 우클릭 → Create → **Falling Knife → Item Data** ×4
   | 파일 이름 | Type | Display Name | Description |
   |---|---|---|---|
   | `ChartAnalysis` | Chart Analysis | 차트 분석 | 다음 캔들을 나만 확인한다. |
   | `Rumor` | Rumor | 찌라시 | 다음 캔들을 공개하고 버린다. |
   | `TradingHalt` | Trading Halt | 거래 정지 | 상대의 다음 턴을 1회 건너뛴다. |
   | `Lottery` | Lottery | 복권 | 25%씩 잔고 +1 / 레버리지 +1 / 아이템 +1 / 꽝. |
3. GameManager Inspector `Item Datas` 크기 4 → 4개 드래그
4. Ctrl+S
- (원하면 에셋 만들기·연결은 AI가 MCP로 할 수 있다 — 말해 주면)

## 8. 수동 테스트 (Ticket 16)
> 고정 지급: GameManager `Use Fixed Items` 켜고 `Fixed Items`. MCP 테스트 전 컴파일 시각 > 저장 시각, Scene 저장 전 다른 변경 확인.

| # | 설정 | 입력 | 기대 |
|---|---|---|---|
| T94 | 기본 | Play | 두 사람 아이템 2개, `아이템  나: 1 …, 2 … / 광대: …, …` (C1) |
| T95 | MCP | 지급 4000번 | 4종 각 약 25% (C3) |
| T96 | 고정 `J J` | 1 → Esc / 3(빈 슬롯) / 1 → A·D | 확인 줄 → 닫힘·아이템 남음 / 변화 없음 / 선택 안 바뀜 (C4) |
| T97 | 고정 `J J`, 덱 `D U U` | 1 → Space | `플레이어: 찌라시 → 다음 캔들은 하락 (버림)`, 남은 0/2, 아이템 1개, 아직 내 차례 (C5) |
| T98 | 이어서 | 1 → Space → 보유 | 찌라시 두 번째 → 남은 0/1 → 보유 실행 (U5: 여러 개 뒤 행동) |
| T99 | 고정 `J`, 덱 `D` 1장(잔고 2) | 1 → Space | 찌라시 → `덱 재장전 (…), 아이템 +2`, 두 사람 아이템 증가 (C5, C2) |
| T100 | 고정 `C S` 등 | 1 → Space | `차트 분석: 아직 쓸 수 없음`, 아이템 남음 |
| T101 | 기본 | 아이템을 4개 넘게 받는 상황(재장전 반복) | 최대 4 (C2) |
| T102 | 클리어 → Space | 2라운드 | 아이템 2개로 다시 시작 (C2) |
| T103 | 광대 차례·공개 중 | 1 | 변화 없음 (C4) |
| T104 | `clown-ai` T65~T68 고정 순서, 아이템 안 씀 | | 판정·차례 같음, Console 오류 0 (C10) |

끝나면 고정 지급·고정 순서 끄고 비우기 → (변경 확인 후) 저장.

## 9. 반영 후
"반영했어" → AI 리뷰 → T94~T104 → 직접 플레이(아이템 확인·찌라시 느낌) → Ticket 17·18·19.
