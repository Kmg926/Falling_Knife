# Ticket 22 초안: 피해를 입으면 화면이 빨갛게 깜빡이고, 잔고가 위험하면 계속 빨강으로 보인다

> **초안**이다(Assets 밖). **직접 타이핑**(CLAUDE.md 4장). 반영 후 AI는 리뷰만.
> 근거: `../../spec.md` (C7·C8, U1~U3), Grilling Q6. 구현 전 정상 버전: 커밋 `9839d6f`

## 0. 바꿀 것
| 파일 | 할 일 | 전문 |
|---|---|---|
| `UIManager.cs` | 칸 4개, `Awake()`, `FlashDamage()`·`FlashRoutine()`, 잔고 줄 색 감싸기 | 같은 폴더 |
| `GameManager.cs` | 피해 두 곳에 `ui.FlashDamage();` 한 줄씩 | 같은 폴더 |
| Scene·에셋 | **없음** (Inspector에 새 칸이 기본값으로 생김) | — |

순서: **UIManager → GameManager** (반대면 잠깐 CS1061 `FlashDamage` 없음).

## 1. `UIManager.cs`
### 1-1. 맨 위
```csharp
using System.Collections;
```
코루틴(`IEnumerator`)을 쓰려고. GameManager 맨 위에 있는 것과 같다.

### 1-2. 칸과 기억해 둘 값 (`statusText` 아래)
```csharp
    [Header("화면 반응")]
    [SerializeField] float flashSeconds = 0.2f;
    [SerializeField] Color flashColor = new Color(0.85f, 0.25f, 0.22f);
    [SerializeField] int dangerBalance = 3;
    [SerializeField] string dangerColorHex = "#8A3A34";

    Color normalColor;
    Coroutine flashRoutine;
```
| 줄 | 왜 |
|---|---|
| `flashSeconds` 0.2 | 깜빡임 길이(Q6). 12주차 "길이를 초 단위로 적기" |
| `Color` 칸 | Inspector에 **색 고르는 칸**으로 보인다. `new Color(r, g, b)`는 0~1 숫자(빨강 0.85) |
| `dangerBalance` 3 | 긴장 기준(Q6). 광대 복권 기준(3)과 같은 숫자지만 따로 둔다(하나를 바꿔도 다른 건 안 바뀌게) |
| `dangerColorHex` | 잔고 줄 색. 글자 안에 넣는 **색 태그**는 `#RRGGBB` 글자로 써야 해서 `Color`가 아니라 `string` |
| `normalColor` | 깜빡임이 끝나면 돌아갈 원래 색 |
| `Coroutine flashRoutine` | 지금 돌고 있는 깜빡임을 기억(멈추기용) |

### 1-3. `Awake()`
```csharp
    void Awake()
    {
        normalColor = statusText.color;
    }
```
| 왜 |
|---|
| `Awake`는 오브젝트가 깨어날 때 **한 번**, `Start`보다 먼저 불린다. GameManager의 `Start()`가 첫 화면을 그리기 전에 원래 색을 기억해 두려고 |
| 색을 코드에 적지 않고 **지금 Inspector 색을 읽는다** → Status Text 색을 바꿔도 깜빡임 뒤 그 색으로 돌아간다 |

### 1-4. `FlashDamage()` / `FlashRoutine()`
```csharp
    public void FlashDamage()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        statusText.color = flashColor;
        yield return new WaitForSeconds(flashSeconds);
        statusText.color = normalColor;
        flashRoutine = null;
    }
```
| 줄 | 왜 |
|---|---|
| `StopCoroutine` 먼저 | 깜빡이는 중에 또 피해가 오면 **처음부터 다시** 0.2초(Spec C7). 안 멈추면 앞 깜빡임이 먼저 끝나면서 색을 일찍 되돌린다 |
| `StartCoroutine(...)`의 반환값 저장 | 나중에 그 코루틴만 골라 멈추려고 |
| 색 바꾸기 → 0.2초 → 원래 색 | GameManager의 공개(`RevealRoutine`)와 같은 코루틴 모양 |
| 판정·시간과 무관 | UIManager 안에서만 돈다. 게임 진행을 기다리게 하지 않는다(U2) |

### 1-5. 잔고 줄 (`statusText.text =` 바로 위)
```csharp
        string balanceLine = $"잔고  플레이어 {game.Player.Balance}{HaltMark(game.Player)} / 광대 {game.Clown.Balance}{HaltMark(game.Clown)}";
        if (game.Player.Balance <= dangerBalance)
        {
            balanceLine = $"<color={dangerColorHex}>{balanceLine}</color>";
        }
```
그리고 `statusText.text` 안의 잔고 줄을 `$"{balanceLine}\n" +`으로 바꾼다.
| 왜 |
|---|
| **TMP 색 태그**: `<color=#8A3A34>…</color>` 사이 글자만 그 색. HTML 태그처럼 글자 안에 적는다(TextMeshPro 기본 기능 Rich Text) |
| `Refresh`가 불릴 때마다 지금 잔고로 다시 판단 → 복권으로 4가 되면 태그가 빠져 원래 색(C8). 따로 "끄기" 코드가 필요 없다 |
| 깜빡임 중에도 잔고 줄은 태그 색이 이긴다(태그가 글자 색보다 우선) — 위험 표시가 깜빡임에 묻히지 않음 |

## 2. `GameManager.cs` — 피해 두 곳
```csharp
            audioManager.PlayImpact();
            ui.FlashDamage();          // ← 추가 (ResolveHold 하락, ResolveSell 하락)
```
충격음과 같은 자리 = "하락으로 잔고가 줄 때"(Q4·Q6). 판정 코드는 그대로.

## 3. 테스트 (Ticket 22)
| # | 방법 | 기대 |
|---|---|---|
| T138 | 고정 덱 하락에서 보유 | 결과 순간 글자 색 빨강 → 약 0.2초 뒤 원래 색, 캡처(결과 글자 읽힘) (C7) |
| T139 | 0.1초 간격으로 피해 두 번(`FlashDamage` 두 번) | 두 번째부터 다시 0.2초 → 약 0.3초에 원래 색 |
| T140 | 내 잔고 10 → 3 / 복권 회복 4 | 잔고 줄 `<color=#8A3A34>` 붙음(어두운 빨강, 캡처) / 빠짐 (C8) |
| T141 | `clown-ai` T65~T68 | 판정·차례 같음, Console 오류 0 (C10) |

## 4. 반영 후
"반영했어" → AI 리뷰 → T138~T141 → 직접 보기(깜빡임이 거슬리거나 안 보이지 않는지, 어두운 빨강이 읽히는지) → Ticket 23.
