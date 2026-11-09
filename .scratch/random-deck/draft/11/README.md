# Ticket 11 초안: 라운드를 시작하거나 재장전할 때마다 덱의 하락·상승 개수가 랜덤으로 정해진다

> **초안**이다(Assets 밖). **직접 타이핑**해서 반영한다(CLAUDE.md 4장). 반영 후 AI는 리뷰만 한다.
> 근거: `../../spec.md` (C1~C6), Grilling Q2~Q5·Q7·Q10. 구현 전 정상 버전: 커밋 `115c9ba`

## 0. 바꿀 파일 한눈에
| 파일 | 할 일 | 전문 |
|---|---|---|
| `RoundConfig.cs` | `downCount`·`upCount` 삭제, `minCards`·`maxCards` 추가, 잔고 기본값 10 | `RoundConfig.cs` |
| `CandleDeck.cs` | `SetComposition` 안전장치, `FillShuffled` → `FillRandom`(개수 뽑기 3줄 추가), `TryFillFixedOrder` 단순화 | `CandleDeck.cs` |
| `GameManager.cs` | 라운드 표 기본값 3줄, `SetComposition` 한 줄, 주석 2개 | `GameManager.cs` |

⚠ **반영 순서와 빨간 에러:** RoundConfig를 먼저 고치면 GameManager의 `downCount`가 없어져 **에러(CS0117/CS1061)가 잠깐 난다.** 세 파일을 다 고치고 저장하면 사라진다(07의 PlayerWon 때와 같음).

## 1. `RoundConfig.cs`
```csharp
    public int startingBalance = 10;
    public int minCards = 4;
    public int maxCards = 6;
    public string fixedOrder = "";
```
| 줄 | 왜 |
|---|---|
| `downCount`·`upCount` 삭제 | 개수는 이제 고정값이 아니라 뽑는 값(Q2). 칸을 남기면 "이 숫자가 쓰이나?" 헷갈린다 |
| `minCards`·`maxCards` | 총 장수 범위(Q10). 줄마다 4~6 / 6~8 / 7~9(Q7) |
| `startingBalance = 10` | 새 줄을 만들 때의 기본값. **Scene에 이미 저장된 2/3/4는 안 바뀐다** → 실제 잔고 변경은 Ticket 12에서 Inspector로 |

- ⚠ **정정(반영 후 확인):** 실제로는 새 칸에 줄마다 4~6 / 6~8 / 7~9가 들어왔다(GameManager 배열 기본값이 쓰임). 그래도 Inspector에서 값을 확인하고 저장하는 순서는 그대로 필요하다(Scene 파일에 새 칸을 기록하기 위해). 자세한 건 Ticket 11 Comments.
- **비유(처음 설명, 일부 틀림):** 클래스의 `= 값`은 "새 서류 양식에 미리 인쇄된 값"이다. 이미 작성해서 낸 서류(Scene에 저장된 줄)는 양식을 바꿔도 그대로다. 새로 생긴 칸(`minCards`)만 인쇄된 값(4, 6)으로 채워진다 → 그래서 3줄 모두 4/6이 되고, **2·3라운드는 Inspector에서 직접 고쳐야 한다**(5절).

## 2. `CandleDeck.cs`
### 2-1. 필드
`int downCount; int upCount;` → `int minCards; int maxCards;` (주석도 "총 장수 범위")

### 2-2. `SetComposition(int min, int max, string order)` — 안전장치
```csharp
        if (min < 2) { Debug.LogWarning(...); min = 2; }
        if (max < min) { Debug.LogWarning(...); max = min; }
```
| 왜 |
|---|
| 하락·상승을 1장씩 넣으려면 최소 2장(Spec, 사용자 채택). 1장이면 `Random.Range(1, 1)`이 되어 하락 수를 뽑을 수 없다 |
| 최대가 최소보다 작으면 `Random.Range`가 이상한 값을 준다 → 최소로 맞춤(그 줄은 장수 고정) |
| 라운드 시작 때 한 번만 불리므로 경고도 라운드마다 한 번만 뜬다 |

### 2-3. `FillRandom()` (원래 `FillShuffled`) — 맨 앞 3줄이 핵심
```csharp
        int total = Random.Range(minCards, maxCards + 1);
        int down = Random.Range(1, total);
        int up = total - down;
```
| 줄 | 왜 |
|---|---|
| `Random.Range(minCards, maxCards + 1)` | ⚠ **정수용 `Random.Range(a, b)`는 b를 포함하지 않는다.** `Random.Range(4, 6)`은 4·5만 나온다. 6까지 나오게 `+ 1` |
| `Random.Range(1, total)` | 같은 규칙 덕분에 1 ~ total−1이 나온다 → 하락 최소 1, 상승 최소 1(Q2) |
| `up = total - down` | 나머지가 상승. down이 total−1 이하라 up은 항상 1 이상 |
| 그 아래 `for` 두 줄 | 원래 `downCount`·`upCount` 자리에 `down`·`up`(방금 뽑은 값) |
| Fisher-Yates | 그대로. 순서를 섞어 숨긴다(U1) |
| 이름 변경 | "섞기"만 하던 함수가 이제 "개수 뽑기 + 섞기" → `FillRandom` (`Fill()` 안의 호출 이름도 같이) |

- 재장전(`GameManager.Reload()` → `deck.Fill()`)도 이 함수를 타므로 **재장전 때마다 새 구성**이 된다(Q3). GameManager 재장전 코드는 안 바뀐다.
- 참고: 정수 `Random.Range`가 끝을 빼는 이유는 배열 번호 `Random.Range(0, list.Count)`를 쓰기 편하게 하려는 것. Fisher-Yates의 `Random.Range(0, i + 1)`도 같은 이유로 `+ 1`이 붙어 있었다.

### 2-4. `TryFillFixedOrder()` — 구성 비교 삭제
```csharp
        if (string.IsNullOrWhiteSpace(fixedOrder)) return false;
```
| 바뀐 것 | 왜 |
|---|---|
| 맨 앞 빈칸 확인 추가 | 빈칸이면 **경고 없이** 랜덤(Spec 채택). 10 T68에서 2라운드 빈칸 때문에 나던 경고가 사라진다 |
| `downs`·`ups` 세기와 "덱 구성과 달라" 경고 삭제 | 이제 문자열이 구성까지 정한다(Q5). 비교할 고정 구성이 없다 |
| D/U가 아닌 글자 경고 | 그대로(문구만 "랜덤으로 채웁니다") |

## 3. `GameManager.cs`
| 바꿀 것 | 왜 |
|---|---|
| `[Header]` 글자 "잔고 / 최소 장수 / 최대 장수 / 고정 순서" | Inspector 안내 |
| 배열 기본값 3줄 `10, 4, 6` / `10, 6, 8` / `10, 7, 9` | 새로 붙인 GameManager용 기본값(Q1·Q7). 지금 Scene에는 적용 안 됨(1절 비유) |
| `deck.SetComposition(config.minCards, config.maxCards, config.fixedOrder);` | 넘기는 값만 바뀜(Q10) |
| `Reload()` 주석 "구성을 새로 뽑아 채운다" | 동작 설명을 지금 코드에 맞게 |

판정·재장전·광대 코드는 **그대로**다.

## 4. 반영 순서
1. `RoundConfig.cs` → `CandleDeck.cs` → `GameManager.cs` 타이핑, 저장 (중간 에러는 정상)
2. Unity 창 클릭 → 컴파일 → Console 에러 0
3. 5절 Inspector 입력 → Ctrl+S
4. "반영했어"

## 5. Inspector에서 할 일 (반영 직후, 중요)
GameManager → `Rounds`
| 줄 | Starting Balance | Min Cards | Max Cards | Fixed Order |
|---|---|---|---|---|
| Element 0 | 2 (그대로) | 4 | 6 | 빈칸 |
| Element 1 | 3 (그대로) | **6** | **8** | 빈칸 |
| Element 2 | 4 (그대로) | **7** | **9** | 빈칸 |
- 잔고는 **11에서는 그대로** 둔다(광대 테스트 T65~T68 퇴행 확인용). 10으로는 Ticket 12에서.

## 6. 수동 테스트 (Ticket 11)
| # | 방법 | 기대 |
|---|---|---|
| T69 | MCP: 라운드별 범위로 `SetComposition` + `Fill()` 1000번씩 (로그는 테스트 동안 끔) | 총 장수 항상 범위 안·모든 값 등장, 하락 1 ~ 총−1·모든 값 등장 (C2) |
| T70 | Play 몇 번 | 1라운드 `남은 캔들` 합 4~6, 하락·상승 ≥ 1, Play마다 바뀜 (C1) |
| T71 | 덱을 다 씀 | `덱 재장전 (하락N / 상승M)` 합이 그 라운드 범위 (C3) |
| T72 | 2·3라운드로 | 시작 합 6~8 / 7~9 (C4) |
| T73 | `Use Fixed Order` + 1라운드 `D U U` / 2라운드 빈칸 / `D X U` | 정확히 `Down Up Up`(남은 1/2), 경고 없음 / 빈칸 줄은 경고 없이 랜덤 / 경고 + 랜덤 (C5) |
| T74 | MCP: `SetComposition(1, 6)`, `SetComposition(5, 3)` | 경고 + 2~6장 / 경고 + 항상 5장 |
| T75 | `clown-ai` T65~T68 고정 순서 | 그대로 통과 (C6) |

끝나면 고정 순서 비우기 → `Use Fixed Order` 끄기 → Ctrl+S.

## 7. 반영 후
"반영했어" → AI 리뷰 → MCP로 T69~T75 → 직접 플레이(구성이 바뀌는 느낌) → Ticket 12(잔고 10, 자동 대전).
