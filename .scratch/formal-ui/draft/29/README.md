# Ticket 29 초안: 타이틀에서 시작하고, 게임 중 Esc로 멈춘다

> AI가 Scripts에 직접 쓴다(사용자 지시 "진행, AI가 작성", CLAUDE.md 4장 예외). 면담 대비로 이 README를 읽어 둔다.
> 근거: `../../spec.md` (C11·C13·C14, U3), Grilling Q11, 스케치 화면 1, 아트 메모 7-6절.

## 0. 바꿀 것
| 파일 | 할 일 |
|---|---|
| `GameManager.cs` | `IsPaused`, `TogglePause()`·`Resume()`·`GoToTitle()`, 입력 함수 4개 첫 줄에 `IsPaused`, `CanPlayerAct`에 `!IsPaused`, `Start`에서 시간 정상화 |
| `PlayerInputHandler.cs` | Esc·우클릭(Cancel): 확인 중이면 취소, 아니면 **Esc만** 일시정지 |
| `PausePanel.cs` | **새 파일** — 멈춘 동안 패널, 계속·타이틀 버튼 |
| `TitleMenu.cs` | **새 파일** (TitleScene) — Space·시작 → GameScene, Esc·종료 → 끝 |
| `ResultPanel.cs` | 타이틀 버튼 켜고 `GoToTitle()` 연결 |
| `UIManager.cs` | 칸 1개 + `Refresh` 한 줄 |

## 1. 일시정지 (`GameManager`)
| 부분 | 왜 |
|---|---|
| `Time.timeScale = 0f` | Unity의 "시간 흐르는 속도". 0이면 `WaitForSeconds`·`Time.deltaTime`이 멈춘다 → 캔들 공개 0.5초·광대 생각 1초·단말기 캔들 그리기가 **그 자리에서 멈췄다가** 계속 누르면 이어진다(C14). 판정 코드는 손대지 않음 |
| `IsGameOver \|\| IsConfirming`이면 무시 | 결과 화면에서는 멈출 게 없고, 확인 중 Esc는 취소(U3) |
| 입력 함수 첫 줄 `IsPaused` | 멈춘 동안 Space·A/D·1~4·버튼 클릭이 게임을 움직이지 않게. 계속은 Esc 또는 계속 버튼 |
| `GoToTitle()`에서 `timeScale = 1f` 먼저 | 멈춘 채 Scene을 바꾸면 다음 Scene도 멈춘 시간으로 시작한다(timeScale은 Scene을 바꿔도 남음) |
| `Start()`의 `timeScale = 1f` | 안전장치 두 번째 겹 |

## 2. 입력 (`PlayerInputHandler.OnCancel`)
| 부분 | 왜 |
|---|---|
| `game.IsConfirming`이면 취소 | 지금까지와 같음(Esc·우클릭) |
| `context.control.device is Keyboard` | 이 입력을 보낸 장치가 키보드인가. Cancel에는 Esc와 마우스 오른쪽이 함께 묶여 있어서, **우클릭은 취소 전용**으로 두고 Esc만 일시정지(우클릭으로 실수로 멈추지 않게) |

## 3. `PausePanel` / `TitleMenu`
| 부분 | 왜 |
|---|---|
| `root.SetActive(game.IsPaused)` | 27·28 패널과 같은 방식. 컴포넌트는 꺼지지 않는 `HUD`에 |
| TitleMenu `Keyboard.current` | 타이틀은 판정이 없는 화면 이동이라 F1처럼 키보드를 직접 읽음(Input Actions는 게임 조작만) |
| `#if UNITY_EDITOR` | 에디터에서는 `Application.Quit()`이 아무 일도 안 해서, Play를 멈추는 것으로 대신(빌드에서는 프로그램 종료) |
| `gameSceneName` | Scene 이름을 Inspector에서 바꿀 수 있게(이름 오타 방지) |

## 4. Scene (AI가 MCP로)
- 새 `TitleScene`(카메라 = Pixel Perfect 320×180 Windowbox, EventSystem, Canvas 320×180): 그래프·로고·부제·목표·조작 상자·`Space 시작`(버튼)·`Esc 종료`(버튼)·광대
- Build Settings: 0 TitleScene, 1 GameScene
- GameScene `HUD/PauseScreen`(어둡게·패널·제목·계속·타이틀), PausePanel 연결, ResultPanel 타이틀 버튼

## 5. 테스트 (Ticket 29)
| # | 방법 | 기대 |
|---|---|---|
| T182 | TitleScene Play → 시작 클릭 | GameScene 1라운드 (C13) |
| T183 | 게임 중 TogglePause | 패널 켜짐, timeScale 0, 버튼 회색·입력 무시 (C14) |
| T184 | 공개 중 멈춤 → 2초 → 계속 | 멈춘 동안 판정 안 남(아직 공개 중), 계속 뒤 공개 끝 (C14) |
| T185 | 확인 중 Cancel | 취소만, 멈춤 아님 (U3) |
| T186 | 일시정지 → 타이틀 → 시작 | TitleScene → GameScene, timeScale 1 (C14) |
| T187 | 결과 화면 타이틀 클릭 | TitleScene (C11) |
| T188 | 퇴행 T65~T68·T143, 오류 0 | 같음 |
| T189 | 실제 Esc·Space·마우스 | 사용자 확인 |
