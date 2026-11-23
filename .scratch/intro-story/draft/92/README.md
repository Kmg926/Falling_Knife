# draft 92: 시작 스토리 대사를 편하게 읽는다

> 2026-11-23. 코드는 AI가 Scripts에 직접(intro-story Grilling Q7 방식, CLAUDE.md 4장 예외). `*.before.cs` = 바꾸기 전, 같은 이름 `.cs` = 바꾼 뒤.
> 근거: 사용자 "대사 창 미흡 → 다른 게임 사례 서치해서 적용", 제안 6가지 전부 채택. 출처는 Ticket 92 끝.

## 조사에서 가져온 것
| 사례 | 적용 |
|---|---|
| 대화창 4요소: 이름·초상·본문·넘김 표시. 이름은 상자와 이어진 작은 탭 | ① 이름표 탭 |
| 문장부호 뒤 150~300ms 멈춤, 말줄임표는 더 길게 | ② 문장부호 멈춤 |
| 다 나오면 오른쪽 아래에서 움직이는 화살표 | ③ ▼ 까딱임 |
| 캐릭터마다 글자 소리로 목소리(Undertale) | ④ 광대 목소리 |
| 상자 모양 변화로 분위기(Undertale) | ⑤ 열림 펼침 |
| 큰 글자·고대비·읽기 쉬움(Game Accessibility Guidelines) | ⑥ 여백·줄 간격 |

## 줄별 설명 — IntroStory.cs (면담 대비)
- 새 칸(`[Header("읽기 편하게")]`): `punctSeconds 0.2`, `ellipsisSeconds 0.4`, `dotSeconds 0.12`, `voicePitch 0.75`, `voicePitchJitter 0.05`, `nextMarkRect`(▼의 RectTransform), `bobSeconds 0.5`, `openSeconds 0.15`.
- `markBase`·`markShownAt`: ▼의 원래 자리와 나타난 시각. `Start`에서 원래 자리를 기억.
- `BobMark()`(Update마다): ▼가 켜져 있으면 `(지금 − 나타난 시각) % 0.5`를 보고 앞 절반은 원래 자리, 뒤 절반은 1칸 아래. 도트 그림이라 부드럽게 움직이지 않고 1칸씩 뚝뚝 움직임. `%` = 나머지(0.5초마다 처음으로 돌아감).
- `ShowMark(true)`: ▼를 켜면서 시각·자리를 처음으로 → 매 줄 까딱임이 같은 박자로 시작.
- `Play()` ②: 대사 칸을 비우고 대화창을 켠 뒤 `Open()`이 끝나야 첫 줄이 나옴.
- `Open()`: `localScale.y`를 0 → 1로(0.15초). 대화창 기준점(pivot)이 **위쪽**이라 그냥 줄이면 위에 붙어서 펼쳐짐 → 줄인 높이의 절반만큼 아래로 내려 **가운데에서** 위아래로 펼쳐지는 것처럼 보이게. `Mathf.Round` = 도트 칸에 맞춤. 끝나면 크기·자리를 원래대로 하고, 펼쳐지는 중에 누른 입력은 버림(`advance = false`).
- `Type()`:
  - 글자 소리: `PlayClick` → `PlayVoice(크기, 0.75 ± 0.05)`. 문장부호(`IsPunct`)에는 소리 없음 — 멈춤이 대신 쉼표 역할.
  - 기다림: `WaitForSeconds(charSeconds)` → `Delay()`가 정한 시간만큼 **프레임마다 `advance`를 보며** 기다림. 그래서 0.4초 멈춤 중에 눌러도 바로 줄 전체가 보임(`WaitForSeconds`는 중간에 못 끊음).
- `Delay(line, i)`: i번째 글자를 보인 뒤 쉴 시간.
  - 줄 끝 → 보통(곧 ▼가 나옴).
  - 지금·다음이 모두 점(`.` `…`) → 0.12초(말줄임표 안의 머뭇거림).
  - 보통 글자, 또는 `?!`처럼 부호가 이어지는 앞쪽 → 보통 0.03초.
  - 부호 뒤: 말줄임표(`…`, 또는 앞도 `.`인 `.`)면 0.4초, 아니면 0.2초.
  - 결과: 첫 줄 "……또 한 명 왔군." 0.33 → 0.79초, "...상관없다. 칼은 …" 0.72 → 1.44초.
- `static`: 객체 값(필드)을 쓰지 않는 도우미 함수라 붙임.

## 줄별 설명 — AudioManager.cs
- `voiceSource`: 광대 목소리 전용 재생기. 높낮이(`pitch`)는 재생기 단위라서, 효과음 재생기(`sfxSource`)의 pitch를 바꾸면 그때 같이 울리는 다른 효과음까지 낮아짐 → 따로 둠.
- `PlayVoice(scale, pitch)`: 재생기가 없으면 전과 같은 `PlayClick(scale)`. 있으면 pitch를 정하고 같은 클릭음을 `PlayOneShot`. 전체 소리 크기(`masterVolume`)는 다른 효과음과 같게 곱함. pitch 0.75 = 25% 낮고 조금 길게 들림.

## Scene (MCP, GameScene)
| 대상 | 바뀐 것 |
|---|---|
| `DialogBox/NameTab`(새 Image) | 대화창과 같은 테두리 그림(`closeup-panel`, Sliced), 32×13, 가운데(x 134), 윗선에 걸침(y +6). `Name` 바로 뒤에 그림 |
| `DialogBox/Name` | 탭과 같은 칸, 가운데·가운데 정렬 |
| `DialogBox/Line` | (8, −18) 278×28 → (10, −13) 276×30, 줄 간격 0 → 10 |
| `Audio` | AudioSource 하나 더(Play On Awake 끔) → `AudioManager.voiceSource` |
| `IntroStory.nextMarkRect` | `DialogBox/NextMark` |

## 확인 (2026-11-23, MCP Play — 타이틀 없이 `PlayNext = true` 후 GameScene 다시 불러옴)
| | 결과 |
|---|---|
| 이름표 탭·여백 | 캡처: 탭이 윗선 가운데에 걸침, 두 줄 대사 안 넘침 |
| 펼침 | `openSeconds`를 시험 동안만 30초로 → 세로 0.02, 가운데(y −152)에서 얇은 줄 → 원래대로 |
| 문장부호 멈춤 | `Delay` 합계: 6줄 모두 위 표대로 |
| 나오는 중 누름 | 시간 배율 0.02로 느리게 → 2/29글자에서 누름 → 29/29, ▼ 켜짐, 다음 줄로 안 넘어감 |
| ▼ 까딱임 | 박자 0.457 → y −38, 0.062 → −37 |
| 목소리 | `voiceSource.pitch` 0.785, `sfxSource.pitch` 1 그대로 |
| Space 길게 | `simulateHold` 1초 → 시작 스토리 끝(`IsPlaying` false) |
| Console | 오류 0 |

**남은 것(사용자):** 실제로 듣고 목소리 높낮이(0.75)·멈춤 길이가 괜찮은지. Inspector `IntroStory` "읽기 편하게"에서 바로 바꿀 수 있음.
