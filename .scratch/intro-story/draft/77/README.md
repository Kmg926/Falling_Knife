# draft 77: 시작하면 화면이 밝아지며 광대가 말을 건다

> 2026-11-22. 코드는 AI가 Scripts에 직접 작성(intro-story Grilling Q7, CLAUDE.md 4장 예외). `*.before.cs` = 바꾸기 전, 같은 이름 `.cs` = 바꾼 뒤.

## 바뀐 파일
| 파일 | 바뀐 것 |
|---|---|
| `IntroStory.cs` (새) | 페이드 인 → 대사 4줄(한 글자씩) → 끝나면 `game.EndIntro()` |
| `GameManager.cs` | `IsIntro`·`BeginIntro()`·`EndIntro()`, 입력 무시 조건 6곳에 `IsIntro` |
| `DeckRevealView.cs` | 시네마틱 중엔 덱 공개 연출을 시작하지 않음 |
| `TitleMenu.cs` | `시작` = 0.5초 검게 → `IntroStory.PlayNext = true` → Scene 전환 |
| `AudioManager.cs` | `PlayClick(float scale)` — 클릭음을 작게(글자 소리) |

## 줄별 설명 (면담 대비)
### IntroStory.cs
- `public static bool PlayNext;` — `static`은 오브젝트가 아니라 **클래스에 붙은 값**이라 Scene이 바뀌어도 남는다. 타이틀이 켜고, GameScene이 읽고 끈다. 그래서 다시하기(Scene을 다시 열지 않음)에선 안 나온다.
- `Awake`에서 `game.BeginIntro()` — Unity는 **모든 Awake → 모든 Start** 순서로 부른다. GameManager.Start가 1라운드를 준비하며 화면을 그릴 때 이미 "시네마틱 중"이어야 덱 공개 연출이 미뤄진다.
- `#if UNITY_EDITOR ... playInEditorWithoutTitle` — 에디터에서만 쓰는 테스트 칸. 빌드엔 들어가지 않는다.
- `CanvasGroup.alpha = 0` — 숨길 UI를 끄지(SetActive) 않고 투명하게 한다. 각 View가 Refresh 때 자기 오브젝트를 다시 켤 수 있어서. CanvasGroup은 실행 중에만 붙이므로 Scene 파일은 안 바뀐다.
- `Update`는 입력만 받아 두고(`advance`·`skip`), 코루틴 `Play`/`Say`가 그 값을 읽는다. Esc는 어디서든 `Finish()`.
- `Play()` — `black.color`의 알파를 1 → 0으로 1초 동안 줄이고(밝아짐), 대사를 한 줄씩 `yield return Say(line)`(그 줄이 끝날 때까지 기다림).
- `Say()` — `maxVisibleCharacters`: 글은 통째로 넣고 **보이는 글자 수만** 늘린다 → 줄바꿈 자리가 처음부터 정해져 글자가 흔들리지 않는다. 나오는 중에 누르면 `break`로 줄 전체. 그 누름은 `advance = false`로 지워 다음 줄로 넘어가지 않게 한다. 빈칸이 아닌 글자 2개마다 `PlayClick(0.3)`.
- `Finish()` — 두 번 불려도 한 번만(`IsPlaying` 확인), 코루틴 멈춤, 상자 끄기, 숨긴 UI 되돌리기, `game.EndIntro()`.

### GameManager.cs
- `IsIntro` — 판정과 상관없이 "지금 시작 스토리 중"인지. `CanPlayerAct`(모니터·폰 열기 포함)·`Select`·`Execute`·`SelectItem`·`ViewSlot`·`TogglePause`가 이 동안 아무 일도 안 한다 → Space는 대사 넘기기, Esc는 일시정지 대신 넘기기.
- `EndIntro()` — `gameStartTime = Time.time`: 결과 화면의 "한 판 시간"은 시네마틱을 빼고 잰다. `ui.Refresh` → DeckRevealView가 그제야 덱 공개 연출을 시작.

### DeckRevealView.cs
- `if (game.IsIntro) return;` — `lastFillCount`를 바꾸기 **전에** 나가므로, 끝난 뒤 Refresh 때 "아직 안 보인 덱"으로 알고 연출을 시작한다.

### TitleMenu.cs
- `StartGame()` 코루틴 — `Time.unscaledDeltaTime`(일시정지로 시간이 멈춰 있어도 흐르는 시간)으로 0.5초 동안 검정 알파 0 → 1, 그다음 `PlayNext = true`, Scene 전환. `starting` 동안 `Update`는 입력을 무시(두 번 눌러도 한 번).

### AudioManager.cs
- `Play(AudioClip clip, float scale = 1f)` — 기본값 1이라 기존 호출은 그대로. 글자 소리만 `masterVolume × 0.3`.

## Scene (MCP)
- GameScene `/HUD/Intro`(IntroStory) → `Root` → `Black`(320×180 검정), `DialogBox`(10,−132 · 300×44, `closeup-panel` 9칸 늘이기) → `Name` "광대"(9, 금색) · `Line`(8,−14 · 278×28, 11, 크림) · `NextMark` ▼(8, 금색). 숨길 UI 8개: HudBar·PlayerBalance·ClownBalance·RoundTurn·TurnBanner·LeverageTitle·LeverageCount·NextDamage.
- TitleScene `TitleCanvas/Fade`(꺼 둔 전체 검정), `TitleMenu.fade` 연결.

## 확인 (2026-11-22, MCP)
| | 결과 |
|---|---|
| C1 | 타이틀 `시작` → GameScene, 검정 → 밝아짐, 첫 줄 (`line1.png`) |
| C2 | 한 글자씩(`typing.png`, ▼ 숨김), 누름 = 줄 전체 → 다음 줄, 4줄 다음 끝. 3번째 줄 2줄로 넘침 → `Line` 높이 26 → 28·y −15 → −14로 고침(`two-lines.png`) |
| C4 | 넘기기 → 시네마틱 끝·덱 공개 연출 시작·숨긴 UI 되돌아옴(`after-skip.png`). 시네마틱 중 `TogglePause`·`Execute`·`OpenMonitor` 무반응, 끝난 뒤 일시정지 정상 |
| C5 | 시네마틱 중 `CanPlayerAct` false, 잔고 등 투명도 0 |
| C6 | 다시하기는 Scene을 다시 열지 않음 → `PlayNext`가 꺼져 있어 안 나옴(코드 확인) |
| C7 | 퇴행 T65~T68·T143은 아직 안 돌림. Console 오류 0. 경고 "missing script"는 전부터 있던 것(HUD에 지워진 스크립트 칸, monitor-trade 44 때부터) |

- Unity 창에 포커스가 없어 MCP 키 입력이 안 들어감 → `advance`·`skip` 값을 직접 켜서 확인. **실제 Space·Enter·클릭·Esc는 사용자가 직접 확인.**
- 에디터 Play는 늘 타이틀부터 시작함(Spec 제안 3의 "바로 Play"는 실제로는 타이틀을 거침).
