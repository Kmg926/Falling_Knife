# Ticket 21 초안: 사건마다 효과음이 난다 (충격음·초침·클릭·셔플)

> **초안**이다(Assets 밖). 게임 코드는 **직접 타이핑**(CLAUDE.md 4장). Scene 연결은 AI가 MCP로(Spec 채택).
> 근거: `../../spec.md` (C2~C6, C10), Grilling Q4·Q5·Q9, Spec 채택(초침 0.2초 뒤, 선택이 바뀔 때만 클릭). 구현 전 정상 버전: 커밋 `436c7dd`
> 효과음 파일: **임시**로 후보 A 4개를 `Assets/_Project/Audio/Sounds/{impact,tick,click,shuffle}.wav`에 넣어 둠(사용자 "일단 임시 효과음으로"). 나중에 고르면 같은 이름으로 바꾸기만 하면 된다.

## 0. 바꿀 것
| 파일 | 할 일 | 전문 |
|---|---|---|
| `AudioManager.cs` | **새 파일** (Project 창 `Scripts` → 우클릭 → Create → Scripting → Empty C# Script → `AudioManager`) | 같은 폴더 |
| `GameManager.cs` | 칸 2개 + 부르는 줄 7곳 + 공개 대기 2줄로 나누기 | 같은 폴더 |
| Scene `Audio` 오브젝트 | AI가 MCP로 (반영 뒤) | — |

⚠ 순서: **AudioManager → GameManager** (반대면 잠깐 CS0246 `AudioManager` 없음). 반영 뒤 **Play 금지** — GameManager의 `Audio Manager` 칸이 비어 있으면 첫 클릭에서 NullReferenceException. AI가 연결한 뒤 Play.

## 1. `AudioManager.cs` (새 파일)
```csharp
public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioClip impactClip;   // … tick, click, shuffle
    [SerializeField] AudioSource sfxSource;
    [SerializeField, Range(0f, 1f)] float masterVolume = 1f;

    public void PlayImpact() { Play(impactClip); }   // … PlayTick, PlayClick, PlayShuffle

    void Play(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        if (masterVolume <= 0f) return;
        sfxSource.PlayOneShot(clip, masterVolume);
    }
}
```
| 줄 | 왜 |
|---|---|
| 방송실 비유 | GameManager(심판)가 "충격음!" 하고 부르면 AudioManager가 튼다. 판정은 모른다(Q9) |
| `AudioClip` | 소리 **파일**(테이프). `AudioSource` = **재생기**(카세트 플레이어). `AudioListener`(Main Camera에 이미 있음) = **귀** |
| `PlayOneShot(clip, 크기)` | 재생기 하나로 여러 소리를 겹쳐 틀 수 있다(초침이 끝나기 전에 충격음이 나도 끊기지 않음). 두 번째 값이 이번 소리의 크기 |
| `[Range(0f, 1f)]` | Inspector에 0~1 **슬라이더**로 보인다(Q5) |
| `masterVolume <= 0f`면 return | 12주차 "크기 0이면 소리가 나지 않는지 확인"(C6). 0이면 아예 틀지 않는다 |
| 클립·재생기 null이면 return | 칸이 비어도 오류 대신 조용히 넘어간다 |
| 함수 4개가 `Play` 하나를 부름 | GameManager는 "무슨 소리"만 고르고, 어떻게 트는지는 한 곳(`Play`)에서 |

## 2. `GameManager.cs` 바꿀 곳
| # | 위치 | 추가 | 왜 |
|---|---|---|---|
| ① | `ui` 칸 아래 | `[SerializeField] AudioManager audioManager;` | 방송실 연결 칸 |
| ② | `clownThinkSeconds` 아래 | `[SerializeField] float tickDelaySeconds = 0.2f;` | 초침 시점(Spec 채택) |
| ③ | `Select()` | `if (Selected != action) audioManager.PlayClick();` (`Selected = action;` **앞**) | 바뀔 때만 클릭(Spec 채택). 바꾸기 **전에** 비교해야 "바뀌는가"를 알 수 있다 |
| ④ | `SelectItem()` | `ConfirmingSlot = slot;` 다음 `audioManager.PlayClick();` | 아이템 확인(Q4) |
| ⑤ | `CancelItem()` | `ConfirmingSlot = -1;` 다음 `audioManager.PlayClick();` | 취소(Q4) |
| ⑥ | `RevealRoutine()` | 대기 한 줄 → 두 줄 + 초침(아래) | 공개 |
| ⑦ | `ResolveHold()` 하락, `ResolveSell()` 하락 | `TakeDamage` 다음 `audioManager.PlayImpact();` | 하락으로 잔고가 줄 때 = 이 두 곳뿐(Q4). 플레이어·광대 모두 |
| ⑧ | `Reload()` | `GiveItems();` 다음 `audioManager.PlayShuffle();` | 재장전(Q4). 라운드 시작(`StartRound`)에는 넣지 않음 |

### ⑥ `RevealRoutine()`
```csharp
        float delay = Mathf.Min(tickDelaySeconds, revealSeconds);
        yield return new WaitForSeconds(delay);
        audioManager.PlayTick();
        yield return new WaitForSeconds(revealSeconds - delay);
```
| 줄 | 왜 |
|---|---|
| 한 번 기다리던 것을 두 번으로 나눔 | 0.2초 조용 → 초침 → 나머지 0.3초. **합은 그대로 0.5초**(U2: 시간 불변) |
| `Mathf.Min(…)` | 공개 시간을 0.2초보다 짧게 바꿔도 전체가 늘어나지 않게(테스트에서 0으로 둘 때도) |
| 앞 0.2초 | GDD "캔들 공개 직전 잠깐 무음". Ticket 23에서 여기서 음악도 줄인다 |

- 판정·차례·아이템 코드는 **한 줄도 바뀌지 않는다**(U1). 추가는 전부 "부르기"뿐.

## 3. Scene (AI가 MCP로, 반영 뒤)
1. 저장 안 된 다른 변경 확인
2. 빈 오브젝트 `Audio` → `AudioSource`(Play On Awake 끔) + `AudioManager`
3. AudioManager 칸: 클립 4개 = `Audio/Sounds/impact·tick·click·shuffle`, `Sfx Source` = 같은 오브젝트의 AudioSource, Master Volume 1
4. GameManager `Audio Manager` 칸 = `Audio`
5. Scene 저장

## 4. 테스트 (Ticket 21)
> 소리는 MCP로 들을 수 없다 → 그 순간 `sfxSource.isPlaying`과 호출 횟수로 확인, 실제 소리는 **직접 스피커로**.

| # | 방법 | 기대 |
|---|---|---|
| T131 | 측정표 | 후보 12개 길이 범위 안(완료) (C1) |
| T132 | 고정 덱 하락에서 보유 / 매도, 광대 차례 하락 | 충격음 1번씩 (C2) |
| T133 | Space | 0.2초 뒤 초침 1번, 공개 전체 0.5초 그대로 (C3, U2) |
| T134 | D·D·A, 1 → Esc | 클릭: D(바뀜) 1, D(같음) 0, A 1, 확인 1, 취소 1 (C4) |
| T135 | 덱 소진 | 셔플 1번, 라운드 시작엔 없음 (C5) |
| T136 | Master Volume 0 | 아무 소리 없음(재생기 안 돔) (C6) |
| T137 | `clown-ai` T65~T68, 아이템 흐름 | 판정·차례 같음, Console 오류 0 (C10) |
| 직접 | 스피커로 한 판 | 소리 크기·톤 어울림, 클릭이 거슬리지 않음 |

## 5. 반영 후
"반영했어" → AI 리뷰 → Scene 연결(MCP) → T132~T137 → 직접 듣기 → Ticket 22.
