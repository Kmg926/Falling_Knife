# Ticket 17 초안: 차트 분석으로 다음 캔들을 나만 본다

> **초안**이다(Assets 밖). **직접 타이핑**(CLAUDE.md 4장). 반영 후 AI는 리뷰만.
> 근거: `../../spec.md` (C6), Grilling Q10. 구현 전 정상 버전: 커밋 `f041b75`

## 0. 바꿀 것 (작음)
| 파일 | 추가 | 전문 |
|---|---|---|
| `CandleDeck.cs` | `Peek()` 함수 1개 (5줄) | 같은 폴더 |
| `GameManager.cs` | 상태 1줄, 초기화 1줄, `case` 4줄, 함수 2개, `deck.Draw()` → `DrawTop()` 2곳 | 같은 폴더 |
| `UIManager.cs` | 차트 분석 줄 6줄 | 같은 폴더 |
| Input·에셋·Scene | **없음** (16에서 다 준비됨) | — |

순서: CandleDeck → GameManager → UIManager (이 순서면 중간 에러 없음).

## 1. `CandleDeck.cs` — `Draw()` 아래에 추가
```csharp
    public Candle Peek()
    {
        return cards[0];
    }
```
| 왜 |
|---|
| `Draw()`는 맨 위를 **꺼내고**(`RemoveAt`) 남은 개수도 줄인다. `Peek()`는 맨 위를 **보기만** 한다 → 덱·남은 개수 그대로(차트 분석은 덱을 바꾸지 않음) |
| 비유: 카드 더미 맨 위를 살짝 들춰 보고 다시 내려놓는 것 |
| 비어 있으면 `cards[0]`이 에러 → 그래서 비어 있을 때 부르면 안 된다. 실제로는 덱이 비면 바로 재장전되므로 내 차례에는 항상 1장 이상 |

## 2. `GameManager.cs`
### 2-1. 상태 (`IsConfirming` 아래)
```csharp
    public Candle? PlayerKnownNext { get; private set; }
```
| 줄 | 왜 |
|---|---|
| `Candle?` | **물음표 = "값이 없을 수도 있음"**(nullable). `Candle`만 쓰면 하락·상승 둘 중 하나가 늘 들어 있어야 해서 "모른다"를 표현할 수 없다. `null` = 모름 |
| 읽을 때 | `.HasValue`(아는가?) → `.Value`(그 캔들) |
| 이름에 `Player` | 쓴 사람만 안다(Q10). 광대가 아는 캔들은 Ticket 20에서 따로 |

### 2-2. `StartRound()` — `ConfirmingSlot = -1;` 아래
```csharp
        PlayerKnownNext = null;
```
새 라운드는 모르는 상태로 시작.

### 2-3. `UseConfirmedItem()`의 `switch`에 `case` 추가
```csharp
            case ItemType.ChartAnalysis:
                Player.RemoveItemAt(ConfirmingSlot);
                UseChartAnalysis(Player);
                break;
```
16에서 `default`(아직 쓸 수 없음)로 가던 차트 분석이 이제 자기 갈래를 갖는다. 찌라시 `case`와 같은 모양.

### 2-4. 새 함수 2개 (`GiveItems()` 위)
```csharp
    void UseChartAnalysis(Participant user)
    {
        PlayerKnownNext = deck.Peek();
        LastResult = $"{user.Name}: 차트 분석 사용";
    }

    Candle DrawTop()
    {
        PlayerKnownNext = null;
        return deck.Draw();
    }
```
| 줄 | 왜 |
|---|---|
| `PlayerKnownNext = deck.Peek();` | 맨 위를 보고 기억. 덱·차례는 그대로(U5) |
| 결과 줄에는 "사용"만 | 다음 캔들은 따로 `차트 분석:` 줄에 보인다(그 캔들이 나올 때까지) |
| `DrawTop()` | **캔들을 꺼내는 곳을 한 군데로 모은다.** 꺼내는 순간 알던 캔들은 지나간 것이므로 잊는다 |
| 왜 한 군데로 | 캔들은 두 곳에서 꺼낸다: 보유·매도 공개(`RevealRoutine`), 찌라시(`UseRumor`). 두 곳 모두 `DrawTop()`을 쓰면 "알던 캔들 지우기"를 잊을 수 없다 → 보유+상승 뒤(C6), 찌라시로 버린 뒤(C6) 모두 자동으로 사라짐 |
| 재장전 | 덱이 비어야 재장전되고, 비우려면 꺼내야 하므로 이미 `DrawTop()`에서 지워진 상태 → `Reload()`는 안 바꿈 |

### 2-5. `deck.Draw()` → `DrawTop()` 2곳
- `UseRumor()`: `Candle top = deck.Draw();` → `Candle top = DrawTop();`
- `RevealRoutine()`: `Candle candle = deck.Draw();` → `Candle candle = DrawTop();`

## 3. `UIManager.cs` — `// 아이템 확인 중` 위에 추가
```csharp
        if (game.PlayerKnownNext.HasValue)
        {
            string next = game.PlayerKnownNext.Value == Candle.Down ? "하락" : "상승";
            message += $"\n차트 분석: 다음 캔들은 {next}";
        }
```
| 왜 |
|---|
| 아는 동안만 한 줄 보인다. 지우는 일은 GameManager가 한다(UI는 읽기만) |
| 공개 중(`공개 중...`)에도 보인다 → 그 캔들이 실제로 나오는 순간 사라짐 |
| 광대 차례에도 보인다(내가 아는 정보) |

## 4. 수동 테스트 (Ticket 17)
> 고정 아이템·고정 덱은 Play 중에만. MCP 테스트 전 컴파일 시각 > 저장 시각, Scene 저장 전 다른 변경 확인.

| # | 설정 | 입력 | 기대 |
|---|---|---|---|
| T105 | 아이템 `C`, 덱 `U D` | 1 → Space | `플레이어: 차트 분석 사용` + `차트 분석: 다음 캔들은 상승`, 남은 개수 그대로(1/1), 아이템 −1, 아직 내 차례 (C6) |
| T106 | 이어서 | Space(보유) | 상승 공개 → `차트 분석:` 줄 사라짐, 차례 유지(다음 캔들은 모름) (C6) |
| T107 | 아이템 `C J`, 덱 `D U` | 차트 분석 → 찌라시 | `다음 캔들은 하락` → 찌라시가 하락을 버림 → `차트 분석:` 줄 사라짐 (C6) |
| T108 | 아이템 `C`, 덱 `D U` | 차트 분석 → 매도 | 광대 차례·공개 중에도 줄이 보이다가 공개 순간 사라짐 |
| T109 | `clown-ai` T65~T68 | 아이템 안 씀 | 판정·차례 같음, Console 오류 0 |

끝나면 고정값 끄고 비우기.

## 5. 반영 후
"반영했어" → AI 리뷰 → T105~T109 → 직접 플레이 → Ticket 18 또는 19.
