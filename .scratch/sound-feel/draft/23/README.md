# Ticket 23 초안: 배경음악이 흐르고, 캔들 공개 중에는 작아진다

> **초안**이다(Assets 밖). **직접 타이핑**(CLAUDE.md 4장). Scene 연결은 AI가 MCP로(Spec 채택).
> 근거: `../../spec.md` (C3·C6·C9), Grilling Q10·Q11, 사용자 결정(배경음악은 AI로 만들지 않고 직접 구함). 구현 전 정상 버전: 커밋 `444f788`

## 0. 바꿀 것
| 파일 | 할 일 | 전문 |
|---|---|---|
| `AudioManager.cs` | 음악 재생기 칸, 음악 크기·공개 비율 칸, `Start`·`OnValidate`·`SetRevealDuck`·`ApplyMusicVolume` | 같은 폴더 |
| `GameManager.cs` | `RevealRoutine` 시작·끝에 한 줄씩 | 같은 폴더 |
| Scene | `Audio` 오브젝트에 음악용 AudioSource 하나 더(Loop·Play On Awake 켬, 곡 칸은 비움) → AI가 MCP로 | — |

순서: **AudioManager → GameManager**. 반영 뒤 Play해도 오류는 없다(음악 칸이 비어 있으면 아무것도 안 함). 그래도 AI가 연결한 뒤 테스트.

## 1. `AudioManager.cs`
### 1-1. 칸
```csharp
    [SerializeField] AudioSource musicSource;
    [SerializeField, Range(0f, 1f)] float musicVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] float revealDuck = 0.2f;
    bool ducking;
```
| 줄 | 왜 |
|---|---|
| `musicSource` | 음악 전용 재생기. 효과음 재생기와 **따로** 둔다 → 음악 크기를 바꿔도 효과음은 그대로(Q10) |
| `musicVolume` 0.6 | 음악 크기(Q10). **0.6은 AI가 정한 시작값** — 하드 테크노는 효과음보다 크게 들려서 조금 낮춰 둠. 곡이 오면 들어 보고 조정 |
| `revealDuck` 0.2 | 공개 중 음악 비율(Q11 "약 20%") |
| `bool ducking` | 지금 공개 중이라 줄여 둔 상태인지 |

### 1-2. 크기 계산 한 곳
```csharp
    void ApplyMusicVolume()
    {
        if (musicSource == null) return;
        float volume = masterVolume * musicVolume;
        if (ducking) volume *= revealDuck;
        musicSource.volume = volume;
    }
```
| 왜 |
|---|
| **곱하기**: 전체 0이면 무엇을 곱해도 0 → 전체 크기 0이면 음악도 꺼짐(C6, 12주차) / 음악 0이면 음악만 꺼짐 |
| 예: 전체 1 × 음악 0.6 = 0.6, 공개 중 × 0.2 = 0.12 |
| 크기를 정하는 식을 **한 함수**에만 둔다 → `Start`·`OnValidate`·`SetRevealDuck` 모두 이걸 부른다 |

### 1-3. 부르는 곳 세 개
```csharp
    void Start() { ApplyMusicVolume(); }
    void OnValidate() { ApplyMusicVolume(); }
    public void SetRevealDuck(bool on) { ducking = on; ApplyMusicVolume(); }
```
| 함수 | 왜 |
|---|---|
| `Start` | 게임 시작 때 한 번 크기 맞추기 |
| `OnValidate` | **Inspector에서 값을 바꿀 때마다** Unity가 부르는 함수. Play 중에 슬라이더를 움직이면 바로 들린다 → 곡 들으며 크기 맞추기 편함 |
| `SetRevealDuck(true/false)` | GameManager가 공개 시작·끝에 부른다(방송실에 "잠깐 줄여"/"원래대로") |

- 곡 재생은 코드가 아니라 AudioSource의 **Play On Awake + Loop** 설정이 한다(게임 시작과 함께 반복 재생). 곡 칸이 비어 있으면 그냥 조용하다(C9).

## 2. `GameManager.cs` — `RevealRoutine()`
```csharp
        IsRevealing = true;
        audioManager.SetRevealDuck(true);     // ← 추가
        ui.Refresh(this);
…
        IsRevealing = false;
        audioManager.SetRevealDuck(false);    // ← 추가
        ui.Refresh(this);
```
| 왜 |
|---|
| `IsRevealing`을 켜고 끄는 바로 그 자리 = 공개의 시작과 끝. 짝이 맞아서 줄인 채로 남을 일이 없다 |
| 공개 시작 → 음악 작아짐 → 0.2초 뒤 초침(21) → 결과와 함께 음악 원래대로 (GDD "공개 직전 잠깐 무음") |

## 3. Scene (AI가 MCP로, 반영 뒤)
1. 저장 안 된 다른 변경 확인 ⚠ 지금 Scene에 21 테스트 뒤부터 원인 모를 "변경됨" 표시가 남아 있음 → 연결 전에 같이 확인하고 말씀드림
2. `Audio` 오브젝트에 AudioSource 하나 더: Play On Awake **켬**, Loop **켬**, AudioClip **비움**
3. AudioManager `Music Source` 칸 = 새 AudioSource (효과음 칸 `Sfx Source`는 기존 것 그대로)
4. Scene 저장

## 4. 곡을 구해 오면 (직접)
1. Project 창 `Assets/_Project/Audio`에 폴더 `Music` 만들기 → 곡 파일 끌어 넣기
2. Hierarchy `Audio` → **음악용** AudioSource(Loop 켜진 쪽)의 `AudioClip` 칸에 곡 끌어 넣기 → Ctrl+S
3. 곡 이름·만든 사람·받은 곳(주소)·라이선스를 알려 주면 AI가 `docs/asset-rights.md` 외부 에셋 표에 기록
4. Play로 30초 이상 들으며 반복 이음매 확인(12주차), `Music Volume` 슬라이더로 크기 맞추기

## 5. 테스트 (Ticket 23)
| # | 방법 | 기대 |
|---|---|---|
| T142 | 곡 칸 비운 채 Play·공개·R | 오류 없음 (C9) |
| T143 | 음악 재생기 volume 읽기 | 평소 0.6, 공개 중 0.12, 결과 뒤 0.6 (C3) |
| T144 | Music Volume 0 / Master 0 | 음악 0 / 음악 0 + 효과음 없음 (C6) |
| T145 | Play 중에만 임시로 아무 소리(셔플)를 곡 칸에 넣기 | 반복 재생(`isPlaying`·`loop`) — 진짜 곡은 사용자가 |
| T146 | `clown-ai` T65~T68 | 판정·차례 같음, Console 오류 0 (C10) |

## 6. 반영 후
"반영했어" → AI 리뷰 → Scene 연결 → T142~T146 → 소리·화면 반응 기능 마무리(직접 듣기·효과음 고르기·곡은 구해 오면 연결).
