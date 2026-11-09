# Ticket 14 초안: 왼쪽 아래 로그에 최근 행동이 남는다

> **초안**이다(Assets 밖). 스크립트는 **직접 타이핑**(CLAUDE.md 4장). Scene의 `Log Text` 생성·연결은 AI가 MCP로(Spec 채택).
> 근거: `../../spec.md` (C4~C9), Grilling Q3~Q6. 구현 전 정상 버전: 커밋 `2ede120`

## 0. 바꿀 것
| 무엇 | 누가 | 내용 |
|---|---|---|
| `GameManager.cs` | 직접 | 기록 목록·`AddLog` 추가, 5곳에서 기록 (전문 같은 폴더) |
| `UIManager.cs` | 직접 | `Log Text`·`Log Lines` 칸, 최근 8줄 표시 (전문 같은 폴더) |
| Scene `Canvas/Log Text` | AI (MCP) | 생성 → 위치·글꼴 → UIManager 칸에 연결 → 캡처 |

⚠ **순서 주의:** 스크립트를 반영한 뒤 `Log Text`가 연결되기 전에 Play하면 UIManager에서 **NullReferenceException**(빈 칸을 쓰려 함)이 난다. 반영 후 Play하지 말고 "반영했어" → AI가 연결 → 그다음 Play.

## 1. `GameManager.cs` — 기록(상태)
### 1-1. 맨 위
```csharp
using System.Collections.Generic;
```
`List<string>`·`IReadOnlyList<string>`를 쓰기 위해(CandleDeck과 같음).

### 1-2. 필드 (`LastResult` 아래)
```csharp
    readonly List<string> log = new List<string>();
    public IReadOnlyList<string> Log => log;
```
| 줄 | 왜 |
|---|---|
| `List<string> log` | 판 전체 기록(Spec 채택: 전부 보관, 보이는 줄 수는 UI) |
| `readonly` | 목록 **상자 자체**는 바꾸지 않는다(새 상자로 바꿔 끼우지 않음). 안의 줄은 `Add`·`Clear`로 바뀐다 |
| `IReadOnlyList<string> Log => log;` | 밖(UIManager)에는 **읽기 전용 창문**만 준다. UI가 실수로 기록을 지우거나 넣을 수 없다 — "값을 바꾸지 않는다"는 UIManager 규칙을 코드로 보장. `Player { get; private set; }`와 같은 생각 |

### 1-3. 기록하는 5곳
| 위치 | 추가 | 로그 줄 예 |
|---|---|---|
| `StartRound()` `LastResult = "";` 다음 | `AddLog($"— 라운드 {RoundNumber} —");` | `— 라운드 2 —` |
| `Restart()` `RoundNumber = 1;` 앞 | `log.Clear();` | (지움 → 곧 `— 라운드 1 —`) |
| `RevealRoutine()` 이름 붙인 줄 다음 | `AddLog(LastResult);` | `광대: 매도 → 하락! 플레이어 -1` |
| `RevealRoutine()` 재장전 `if` 다음 | 끝 줄 3가지(아래) | `라운드 1 클리어!` / `최종 승리!` / `패배...` |
| `Reload()` | 재장전 줄을 변수로 만들어 결과 줄에도 붙이고 기록에도 | `덱 재장전 (하락3 / 상승2)` |

```csharp
        if (IsRoundCleared) AddLog($"라운드 {RoundNumber} 클리어!");
        else if (IsFinalVictory) AddLog("최종 승리!");
        else if (IsRoundOver) AddLog("패배...");
```
- `Restart()`의 `log.Clear()`는 `StartRound()` **앞**이어야 한다. 뒤면 방금 넣은 `— 라운드 1 —`까지 지워진다.
- `NextRound()`는 안 지운다 → 라운드가 바뀌어도 이어진다(Q5).
- 결과 줄 `AddLog(LastResult)`는 `Reload()` **앞** → 로그에서는 결과 한 줄, 재장전 한 줄로 따로(C4). 화면 결과 줄은 지금처럼 두 줄이 붙어 보인다.
- 끝 줄 판정은 UIManager의 클리어·승리·패배 `if`와 같은 순서·같은 값(판정은 GameManager에 이미 있음).

```csharp
    void Reload()
    {
        deck.Fill();
        string line = $"덱 재장전 (하락{deck.RemainingDown} / 상승{deck.RemainingUp})";
        LastResult += "\n" + line;
        AddLog(line);
    }

    void AddLog(string line)
    {
        log.Add(line);
    }
```
- `line`을 한 번 만들어 두 곳에 쓴다(같은 문구를 두 번 쓰지 않기).
- `AddLog`는 지금은 `Add` 한 줄이지만, 기록 방식이 바뀌면(시각 붙이기 등) 이 한 곳만 고치면 된다.

## 2. `UIManager.cs` — 보여 주기
```csharp
    [Header("행동 로그 (왼쪽 아래)")]
    [SerializeField] TMP_Text logText;
    [SerializeField] int logLines = 8;
...
        logText.text = RecentLog(game.Log);
...
    string RecentLog(IReadOnlyList<string> log)
    {
        int start = Mathf.Max(0, log.Count - logLines);
        List<string> recent = new List<string>();
        for (int i = start; i < log.Count; i++)
        {
            recent.Add(log[i]);
        }
        return string.Join("\n", recent);
    }
```
| 줄 | 왜 |
|---|---|
| `logLines = 8` | 보이는 줄 수는 보여 주기 문제 → UIManager Inspector(Spec 채택) |
| `Mathf.Max(0, log.Count - logLines)` | 마지막 8줄의 시작 번호. 기록이 8줄보다 적으면 음수가 되므로 0으로 막는다(예: 3줄이면 3−8=−5 → 0) |
| `for (i = start; i < log.Count; i++)` | 시작 번호부터 끝까지 = 최근 8줄, 오래된 것이 위·새 것이 아래(Q3) |
| `string.Join("\n", recent)` | 줄바꿈으로 이어 한 덩어리 글자로. CandleDeck 로그의 `string.Join(" ", cards)`와 같은 함수 |
| `Refresh` 맨 끝 | 상태가 바뀔 때마다 이미 `Refresh`가 불리므로 로그도 같이 갱신된다. 새 호출 지점 없음 |
| `using System.Collections.Generic;` | `List`·`IReadOnlyList` |

## 3. Scene: `Log Text` (AI가 MCP로, 반영 후)
1. Canvas 아래 `Log Text` 생성 (TextMeshPro - Text (UI))
2. 기준점·피벗 왼쪽 아래(0,0), 위치 (20, 20), 크기 560×220
3. 글꼴 Galmuri11 SDF, 크기 20, 왼쪽 아래 정렬, 밝은 회색
4. UIManager(Canvas)의 `Log Text` 칸에 연결, `Log Lines` 8 확인
5. **저장 전 다른 변경 확인**(사용자가 바꾼 값이 있으면 먼저 묻기) → Scene 저장
6. Play 캡처로 겹침 확인 → 필요하면 위치·크기 조정(사용자와 같이)

## 4. 반영 순서
1. `GameManager.cs` → `UIManager.cs` 타이핑, 저장 (중간 에러 없음: UIManager가 쓰는 `game.Log`가 GameManager에 먼저 생김. 반대 순서면 잠깐 CS1061)
2. Unity 창 클릭 → 컴파일 → Console 에러 0
3. **Play하지 말고** "반영했어"

## 5. 수동 테스트 (Ticket 14)
| # | 방법 | 기대 |
|---|---|---|
| T82 | Play | 로그 `— 라운드 1 —` (C6) |
| T83 | 고정 순서 `D D U U U`(잔고 2, Play 중에만) 보유 → 광대 → 매도 → 광대… | 행동마다 한 줄, `덱 재장전 (하락2 / 상승3)` 별도 줄 (C4) |
| T84 | 행동 9번 이상 | 8줄만, 맨 위가 밀려남 (C5) |
| T85 | 클리어 → Space | `라운드 1 클리어!` 다음 `— 라운드 2 —`, 앞 기록 유지 (C6·C7) |
| T86 | 패배 → R | 로그가 `— 라운드 1 —`만 (C7) |
| T87 | 캡처 | 왼쪽 아래, 상태 텍스트와 겹치지 않음 (C8) |
| T88 | `clown-ai` T65~T68 고정 순서(잔고 2) | 판정·차례 같음 (C9) |

끝나면 고정 순서 비우기 → `Use Fixed Order` 끄기 → (다른 변경 확인 후) 저장.

## 6. 반영 후
"반영했어" → AI 리뷰 → `Log Text` 생성·연결 → T82~T88 → 직접 플레이(경험: 광대 행동 따라가기, 로그 방해 여부) → random-deck 12에서 미룬 판 길이·잔고 판단.
