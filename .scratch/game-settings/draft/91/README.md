# draft 91: 게임 중에 설정을 연다

> 2026-11-23. 코드는 AI가 Scripts에 직접(game-settings Grilling Q8, CLAUDE.md 4장 예외), 톱니 그림은 `tools/make_art.py gear`. `*.before.cs` = 바꾸기 전.

## 바뀐 파일
| 파일 | 바뀐 것 |
|---|---|
| `SettingsPanel.cs` (새) | 설정 4줄(전체 소리·음악·흔들림·전체화면) 고르기·바꾸기·저장·글 — 타이틀과 게임이 같이 씀 |
| `TitleMenu.cs` | 설정 부분(줄 고르기·값 바꾸기·글·`Bar`)을 빼고 `SettingsPanel`에 맡김. 창 열기(제목·안내)만 남김 |
| `GameSettings.cs` | 값을 바꾸면 `Changed` 알림 |
| `MusicPlayer.cs`·`AudioManager.cs`·`UIManager.cs` | `Changed`를 듣고 음악 크기·효과음 크기·흔들림을 바로 다시 읽음 |
| `GameManager.cs` | `IsSettingsOpen`·`OpenSettings(fromGear)`·`CloseSettings()`·`CanPause`·`CanOpenGear`, Esc가 설정 창이면 닫기 |
| `PausePanel.cs` | 버튼 3개(계속·설정·타이틀)·↑↓/W S + Space·Enter·설정 창 켜고 끄기·톱니 |

## 줄별 설명 (면담 대비)
### SettingsPanel.cs
- 화면(타이틀·일시정지)은 **창을 열고 닫는 일만**, 이 부품은 글 상자(`body`)에 설정 줄만 그림 → 같은 코드를 두 곳에서 씀(전엔 TitleMenu 안에만 있었음).
- `HandleInput` — 마우스가 어느 줄 위인지 `TMP_TextUtilities.FindIntersectingLine`으로 찾고, 클릭하면 그 줄을 한 단계 바꿈. ↑↓ 줄, ←→ 값. Esc는 보지 않음(닫는 길은 각 화면이 정함).
- `Change` — `GameSettings`에 저장 + 클릭음으로 크기를 바로 들려 줌(`PlayOneShot`은 시간이 멈춰도 들림).

### GameSettings.cs — `static event System.Action Changed`
- event = "이 일이 생기면 알려 줘"라고 등록한 함수들을 한꺼번에 부르는 목록. 값 setter 끝에서 `Changed?.Invoke()`(`?.` = 등록한 게 없으면 안 부름).
- 바꾸는 쪽(설정 창)은 누가 듣는지 몰라도 되고, 듣는 쪽(음악·효과음·흔들림)은 `OnEnable`에서 `+=`로 등록, `OnDisable`에서 `-=`로 뺌(씬이 바뀌어 사라진 오브젝트를 부르지 않게).
- 그래서 TitleMenu에 있던 "음악 크기 직접 다시 계산" 줄(88)은 지움 — 알림이 대신함.

### GameManager.cs
- `CanPause` — 전엔 `TogglePause` 안 조건이던 것(시작 스토리·결과 화면·아이템 확인·모니터·폰 중엔 안 멈춤)을 이름 붙여 꺼냄. 톱니도 같은 조건(`CanOpenGear` = 멈출 수 있고 아직 안 멈춤).
- `OpenSettings(fromGear)` — 안 멈춰 있으면 멈추고(시간 0) 설정 창. `settingsFromGear` 기억.
- `CloseSettings()` — 톱니로 열었으면 바로 게임으로(멈춤 풀기), 일시정지 메뉴에서 열었으면 메뉴로.
- `TogglePause` — 설정 창이 열려 있으면 Esc = 설정 닫기. `Resume` — 설정 창도 같이 닫음.

### PausePanel.cs
- `buttons` 3개(계속·설정·타이틀). 마우스 올림 강조·직접 위치 확인은 전과 같음(title-menu 59·60). 키보드 ↑↓/W S로 강조를 옮기고 Space·Enter로 `Activate`.
- `Refresh` — 설정 창이 열려 있으면 메뉴를 숨기고 설정 창을 보이며, 막 열렸을 때 `settings.Begin()`(첫 줄부터). 설정에서 돌아오면 "설정"을 강조. 톱니는 `CanOpenGear`일 때만 보임.
- `Update` — 멈추지 않았을 때는 톱니 클릭만 봄, 설정 창이 열려 있으면 그 창에 입력을 넘김.
- 일시정지 중 Space는 PlayerInputHandler의 실행으로도 가지만 `Execute`가 멈춤 중엔 무시하므로 겹치지 않음. 클릭은 원래 "실행"에 묶여 있지 않아(Space만) 톱니 클릭이 모니터를 열지 않음.

## 그림·Scene (MCP)
- 톱니 22×22(지금 좌표 11×11): 이빨 8개 + 가운데 구멍, 평소 회색·올림 금색(`gear-preview-x8.png`). 왼쪽 위 (4, −4).
- `PauseScreen`: 패널·제목·버튼을 `Menu` 묶음으로(설정 중 한꺼번에 숨김), 패널을 3버튼 높이(95)로 — 그림은 같은 모양(1도트 금 테 + bg0)이면서 9칸 늘이기가 되는 `closeup-panel`. `SettingsButton`(계속 버튼 복사, 글 "설정").
- `PauseScreen/Settings`(새): 창 220×92 + 제목 "설정" + 설정 4줄(크기 9) + 안내 "↑↓ 고르기 ←→ 바꾸기 Esc 닫기" + `SettingsPanel`(효과음은 AudioManager 재생기·클릭음).
- TitleScene: `TitleCanvas`에 `SettingsPanel`(글 상자 = 원래 창 본문, 효과음 `TitleSfx`), `TitleMenu.settings` 연결.

## 확인 (2026-11-23, MCP)
| | 결과 |
|---|---|
| C3 | 타이틀 설정 창 모양 그대로(`title-settings.png`), 음악 −10% → 음악 크기 0.210 → 0.175 → 되돌림 0.210(알림으로) |
| C1 | 톱니 → 멈춤(시간 0) + 설정 창(`game-settings.png`), Esc → 바로 게임(멈춤 풀림) / Esc → 일시정지 메뉴(`pause-menu.png`, 계속·설정·타이틀) → 설정 → Esc → 메뉴(멈춘 채, "설정" 강조) |
| C2 | 흔들림 끔 → `UIManager.screenShake` false → 되돌림 / 전체 소리 −10% → 효과음 0.9·음악 0.189 → 되돌림 |
| C4 | 시작 스토리 중 톱니 숨김·눌러도 무시 / 광대 차례(생각 중)에 톱니 → 멈추고 광대도 기다림 → 닫으면 이어서 / 결과 화면에서 톱니 숨김·무시 |
| C5 | 퇴행 T65~T68·T143 통과(T65는 광대 차례에 설정을 열었다 닫은 채로도 같음), Console 오류 0 |

- 테스트 중 설정은 바꾼 뒤 모두 원래 값으로 되돌림.
- 실제 키(↑↓·Space·Enter·Esc)·마우스(톱니 올림 금색·클릭)는 Unity 창 포커스가 없어 함수로 확인 → **사용자가 직접 확인**.
