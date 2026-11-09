# Ticket 09 초안: 내가 턴을 넘기면 광대가 남은 덱 비율로 스스로 보유·매도한다

> **초안**이다(Assets 밖). **직접 타이핑**해서 반영한다(CLAUDE.md 4장). 반영 후 AI는 리뷰만 한다.
> 근거: `../../spec.md` (C1~C5), Grilling Q1~Q8. 구현 전 정상 버전: 커밋 `f242545`

## 0. 바꿀 파일 한눈에
| 파일 | 할 일 | 전문 |
|---|---|---|
| `ClownAI.cs` | **새 파일** (Project 창 `Assets/_Project/Scripts` → 우클릭 → Create → Scripting → **Empty C# Script**, 이름 `ClownAI`) | `ClownAI.cs` |
| `GameManager.cs` | 7곳 추가·수정 | `GameManager.cs` |
| `UIManager.cs` | 4줄 추가 | `UIManager.cs` |
| `PlayerInputHandler.cs` | 주석 한 줄만 | 아래 4절 |

> 새 파일은 Unity Project 창에서 만든다(.meta가 같이 생겨야 함). 만든 뒤 안의 기본 내용을 지우고 전문을 타이핑.

## 1. `ClownAI.cs` (새 파일, 전문)
```csharp
public static class ClownAI
{
    public static PlayerAction Decide(int remainingDown, int remainingUp)
    {
        int total = remainingDown + remainingUp;
        if (total == 0) return PlayerAction.Hold;
        float downRatio = (float)remainingDown / total;
        return downRatio >= 0.5f ? PlayerAction.Sell : PlayerAction.Hold;
    }
}
```
| 줄 | 왜 |
|---|---|
| `public static class` | **계산기** 같은 것. 기억할 값(상태)이 없어서 Scene 오브젝트에 붙일 필요가 없다. `ClownAI.Decide(…)`로 어디서든 바로 부른다. `MonoBehaviour`를 상속하지 않으므로 `using UnityEngine;`도 필요 없다 |
| `Decide(int remainingDown, int remainingUp)` | 광대는 화면에 보이는 **남은 개수만** 안다(다음 캔들은 모름, Q2). 덱 전체를 넘기지 않고 숫자 두 개만 받아서 "광대가 아는 것"을 코드로 제한한다 |
| 반환형 `PlayerAction` | 플레이어가 고르는 것과 같은 보유/매도. 이름은 "Player"지만 "차례인 사람의 행동"이라는 뜻(GameManager 4번째 줄 주석) |
| `if (total == 0) return Hold;` | 0으로 나누기 방지 안전장치. 덱이 비면 곧바로 재장전되므로 실제로는 안 온다 |
| `(float)remainingDown / total` | ⚠ **(float)를 빼면 안 된다.** 정수 ÷ 정수는 소수점을 버린다: `2 / 4 = 0`. 그러면 늘 0이 되어 광대가 항상 보유한다. 앞쪽을 float로 바꾸면 `2f / 4 = 0.5` |
| `>= 0.5f ? Sell : Hold` | GDD "50% 이상이면 매도, 미만이면 보유". 정확히 50%면 매도(Q2). `조건 ? A : B`는 "조건이 참이면 A, 아니면 B"를 한 줄로 쓴 것 |

## 2. `GameManager.cs` 바꿀 곳 (전문은 같은 폴더)
| # | 위치 | 바꿀 것 | 왜 |
|---|---|---|---|
| ① | `[Header("연출")]` 아래 | `[SerializeField] float clownThinkSeconds = 0.5f;` 추가 | 생각 시간을 Inspector 숫자 칸으로(Q4). 코드 기본값 0.5 |
| ② | `IsRevealing` 아래 | `public bool IsClownThinking { get; private set; }` 추가 | UI가 `광대 생각 중...`을 보여줄지 읽는 값. 바꾸는 건 GameManager만(`private set`) |
| ③ | `IsFinalVictory` 아래 | `public bool IsClownTurn => Current == Clown && !IsRoundOver;` 추가 | "광대가 행동해야 하는 때"를 한 곳에서 정의. `!IsRoundOver`가 있어서 라운드가 끝나면 광대가 멈춘다(Q9) |
| ④ | `StartRound()` | `IsClownThinking = false;` 추가 (`IsRevealing = false;` 아래) | 새 라운드는 깨끗한 상태로 |
| ⑤ | `Select()` | 조건에 `\|\| IsClownTurn` 추가 | 광대 차례에 A/D 무시(Q5) |
| ⑥ | `Execute()` | `if (IsRoundOver) return;` **다음 줄에** `if (IsClownTurn) return;` 추가 | 광대 차례에 Space 무시. 주의: 내 매도로 광대가 파산하면 차례가 **광대로 넘어간 채** 클리어 화면이 된다. 그래서 무시 조건을 `Current == Clown`만으로 쓰면 클리어 화면의 Space가 막힌다. `IsClownTurn`은 `!IsRoundOver`를 포함해 이 경우 false라 안전하고, 줄도 클리어 확인 **뒤**에 두어 "클리어 → 끝 → 광대 차례" 순서로 읽히게 한다(Ticket 10 C9에서 확인) |
| ⑦ | `RevealRoutine()` 끝 | `ui.Refresh(this);` 다음에 `if (IsClownTurn) StartCoroutine(ClownTurnRoutine());` 추가 | 공개가 끝난 뒤 차례가 광대면 광대가 움직인다. 내가 넘긴 경우·광대가 보유+상승으로 차례를 유지한 경우가 **이 한 곳**으로 처리된다(Q3). 라운드가 끝났으면 `IsClownTurn`이 false라 시작 안 함(Q9) |
| ⑧ | `RevealRoutine()` 아래 | `ClownTurnRoutine()` 새 함수 (아래) | 광대 턴 진행 |
| ⑨ | 주석 2곳 | `RevealRoutine` 주석 "1초" → "(revealSeconds)", `PassTurn` 주석의 "핫시트: " 삭제 | 지금 동작과 맞게 |

```csharp
    IEnumerator ClownTurnRoutine()
    {
        IsClownThinking = true;
        ui.Refresh(this);

        yield return new WaitForSeconds(clownThinkSeconds);

        IsClownThinking = false;
        Selected = ClownAI.Decide(deck.RemainingDown, deck.RemainingUp);
        StartCoroutine(RevealRoutine(Selected));
    }
```
| 줄 | 왜 |
|---|---|
| `IsClownThinking = true; ui.Refresh(this);` | 화면에 `광대 생각 중...`을 띄운다 |
| `yield return new WaitForSeconds(clownThinkSeconds);` | 코루틴: 여기서 **0.5초 멈췄다가** 다음 줄부터 이어간다. 그동안 게임(입력·화면)은 계속 돈다. `RevealRoutine`의 1초 대기와 같은 방식 |
| `Selected = ClownAI.Decide(...)` | 두뇌(ClownAI)에게 묻고 답을 `Selected`에 넣는다. 그래서 화면 `선택:` 줄에 광대 선택이 보인다(Q7) |
| `StartCoroutine(RevealRoutine(Selected));` | 플레이어가 Space를 눌렀을 때와 **똑같은 공개·판정**을 탄다(U2). 광대 전용 판정 코드를 따로 만들지 않는다 |

**흐름 (광대가 보유 2번 → 매도)**
```
내 Space → RevealRoutine → (차례 광대) → ClownTurnRoutine → RevealRoutine(보유·상승, 차례 유지)
        → ClownTurnRoutine → RevealRoutine(보유·상승) → ClownTurnRoutine → RevealRoutine(매도, 차례 넘김) → 끝(내 차례)
```
- 광대 생각·공개 중 R: `Restart()`는 `!IsRoundOver`면 무시 → 그대로 막힌다(코드 변경 없음).

## 3. `UIManager.cs` 추가 (message 줄 다음)
```csharp
        if (game.IsClownThinking)
        {
            message += "\n광대 생각 중...";
        }
```
| 왜 |
|---|
| `공개 중...`처럼 결과 줄을 **덮지 않고 덧붙인다**(Spec, 사용자 채택). 내 공개가 끝나자마자 광대가 생각을 시작하므로, 덮으면 내 결과(`매도 → 하락! 광대 -1`)를 읽을 틈이 없다 |
| 클리어·패배 문구 `if`보다 **앞**에 둔다. 생각 중에는 라운드가 끝난 상태가 아니라 둘이 같이 나올 일은 없지만, 읽는 순서(결과 → 생각 중 → 끝 문구)대로 둔다 |

## 4. `PlayerInputHandler.cs` 주석 한 줄
지울 줄: `// 핫시트라서 광대 차례에도 같은 입력을 쓴다.`
넣을 줄: `// 광대 차례 입력은 GameManager가 무시한다.`
(동작 변경 없음. 무시 판단은 GameManager가 한다 — 입력 담당은 전달만)

## 5. Unity Editor에서 할 순서
1. `ClownAI` 새 스크립트 만들기 → 타이핑
2. `GameManager.cs` → `UIManager.cs` → `PlayerInputHandler.cs` 순서로 반영, 저장
3. Unity 창을 클릭해 컴파일 → Console 에러 0
   - `ClownAI` 없다는 에러(CS0103)가 나면 1번 파일 이름·저장 확인
4. GameManager Inspector `연출`에 `Clown Think Seconds` 0.5가 보이는지 확인 → Ctrl+S
5. "반영했어"라고 알려주기

## 6. 수동 테스트 (Ticket 09)
> 고정 순서: CandleDeck `Use Fixed Order` 켜기 → GameManager `Rounds` 첫 줄 `Fixed Order`. MCP 테스트 전 **컴파일 시각 > 저장 시각** 확인.

| # | 고정 순서 | 입력 | 기대 |
|---|---|---|---|
| T59 | — | MCP로 `ClownAI.Decide` 호출: (2,3) (2,2) (3,1) (0,4) (4,0) (0,0) | 보유, 매도, 매도, 보유, 매도, 보유 (C1) |
| T60 | `U D U D U` | D → Space | 내 결과 `매도 → 상승! 변화 없음 (레버리지 0 그대로)` + `광대 생각 중...`, `차례: 광대` → 광대 `[매도]` + `공개 중...` → `매도 → 하락! 플레이어 -1`, 잔고 1/2 (C2) |
| T61 | 이어서 | — | `차례: 플레이어`, `[보유]`, 남은 1/2 → A/D가 다시 먹힌다 (C5) |
| T62 | `D U U U D` | D → Space | 광대 -1(2/1) → 광대 보유(레버리지 1) → 보유(2) → 매도(`변화 없음 (레버리지 2 그대로)`) → `차례: 플레이어`, `[보유]`, 남은 1/0 (C4, C5) |
| T63 | `U D U D U` | D → Space 직후, 생각 중·공개 중에 A·D·Space·R | 변화 없음, 광대 행동은 T60과 같음 (C3) |
| T64 | 기본(무작위) | 몇 턴 직접 플레이 | Console 오류 0, 광대 턴이 끝나면 항상 내 차례 |

끝나면 고정 순서 비우기 → `Use Fixed Order` 끄기 → Ctrl+S.

## 7. 반영 후
"반영했어" → AI 리뷰(초안과 다른 점, 버그 가능성) → MCP로 T59~T64 → 직접 플레이로 경험 확인(Q13) → Ticket 10.
