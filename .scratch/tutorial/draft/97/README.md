# draft 97·98: 튜토리얼 판 · 타이틀 권유

> 2026-11-23. 코드는 AI가 Scripts에 직접(tutorial Grilling Q12, CLAUDE.md 4장 예외). `*.before.cs` = 바꾸기 전, 같은 이름 `.cs` = 바꾼 뒤(`TutorialGuide.cs`는 새 파일).
> 근거: `../../grilling.md`(Q1~Q12 전부 채택) · `../../spec.md`.

## 전체 구조 (판정은 GameManager, 진행은 TutorialGuide)
| 누가 | 하는 일 |
|---|---|
| `TitleMenu` | `튜토리얼` 줄 → `TutorialGuide.PlayNext = true` → GameScene. 처음 `시작`이면 권유 창(한 번만) |
| `TutorialGuide` | 대사(대화창) → 할 일 열어 주기 → 해냈나 보기 → 다음 단계. "여기를 봐"·끝 선택·건너뛰기 |
| `GameManager` | 튜토리얼 값(덱·잔고·아이템·광대 각본)으로 라운드, 허용된 행동만 받기, 대사 중엔 멈춤 |

## 줄별 설명 — GameManager.cs (면담 대비)
- `tutorialRound`(잔고 5·8장·고정 순서 `UDUDDUUD`), `tutorialPlayerItems "C"`(차트 분석), `tutorialClownMoves "SB"`(광대: 매도 → 매수): Inspector에서 바꿀 수 있는 각본.
- `RoundCount`: 튜토리얼이면 1. `CurrentConfig`: 튜토리얼이면 튜토리얼 라운드 값, 아니면 라운드 표.
- `IsTutorial`·`IsGuideTalking`·`IsCutscene(= IsIntro || IsGuideTalking)`: 지금까지 `IsIntro`로 막던 곳(행동 가능 여부·Select·Execute·SelectItem·ViewSlot)을 `IsCutscene`으로 → 시작 스토리와 튜토리얼 대사가 같은 방식으로 게임을 멈춤. (`CanPause`는 그대로 `IsIntro` — 튜토리얼 중 Esc 일시정지는 됨)
- `RevealCount`: 공개가 끝날 때마다 +1. 튜토리얼이 "해냈나"를 보는 신호(판정에는 안 씀).
- `BeginTutorial()`: TutorialGuide.Awake가 부름(모든 Start보다 먼저) → 첫 `StartRound`가 튜토리얼 값으로. 광대 각본 글자를 `Queue`(먼저 넣은 것부터 꺼내는 줄)에 넣음.
- `SetGuideTalking(on)`: 대사 시작·끝. `SetTutorialGate(모니터, 폰, 고정 행동)`: 이번 단계에 할 수 있는 것. 고정 행동이 있으면 `Selected`를 미리 그것으로.
- `CanOpenMonitor`·`CanOpenPhone`·`SelectItem`·`Select`: 튜토리얼이면 허용된 것만(`Select`는 고정 행동이 아니면 무시). `Execute`에서도 한 번 더 고정 행동으로(안전장치).
- `StartRound`: `deck.ForceFixedOrder = IsTutorial`.
- `GiveItems`: 튜토리얼이면 라운드 시작 때 내 아이템만 글자대로(광대 없음, 재장전 땐 없음). 글자 → 아이템은 `ItemFromChar`로 묶음(고정 지급 `NextItem`도 같은 함수).
- `RevealRoutine` 끝: `RevealCount++`, 튜토리얼이면 `IsGuideTalking = true` → 광대 차례도 튜토리얼 광대가 말할 때까지 기다림(할 말이 없으면 TutorialGuide가 바로 풂).
- `ClownTurnRoutine`: 처음에 `IsGuideTalking` 동안 기다림. 튜토리얼 각본이 남아 있으면 꺼내서 그 행동(말풍선 "매도."/"매수." 포함), 다 쓰면 원래 `ClownAI`.

## 줄별 설명 — TutorialGuide.cs (새 파일)
- `PlayNext`(static): 타이틀이 켜고 다음 GameScene이 읽고 끔(IntroStory와 같은 방식). 꺼져 있으면 `enabled = false`로 아무것도 안 함.
- `Awake`: `game.BeginTutorial()` + `IntroStory.PlayNext = false`(시작 스토리 없음).
- `Start`: 시작 스토리 검은 화면 끄기, 강조 틀 선 4개 만들기(`BuildEdges` — 창 테두리 그림은 안쪽이 어두운 판이라 대상을 가려서, 그림 없는 단색 선), `Run()` 시작.
- `Run()`: Spec 2의 단계 0~11을 위에서 아래로 쓴 코루틴. `Step`은 지금 단계 번호(테스트용).
- `Talk(줄들)`: 줄마다 `Type` → ▼ → 넘기면 다음 줄. `Type`: 시작 스토리와 같은 속도·문장부호 멈춤(`IntroStory.PauseAfter` 공용 함수)·광대 목소리. `<color>` 태그가 있어 글자 수는 TMP가 센 실제 글자 수(`textInfo.characterCount`)로.
- `Hold()`/`Release()`: 말하기 시작(대화창 켜고 `SetGuideTalking(true)`)·끝(끄고 false).
- `Act(종류, 행동, 공개 수)`: 그 행동만 열어 주고(`SetTutorialGate`) 풀어 줌 → 공개가 그 수만큼 끝날 때까지(폰은 `PlayerKnownNext`가 생길 때까지). 공개가 끝날 때마다 GameManager가 다시 멈춰 두므로 더 해야 하면 다시 풂.
- `Watch()`: 광대 차례 구경 — 풀어 두고 공개 한 번이 끝날 때까지.
- `Look(대상, 요약)`: 대화창이 화면 아래(모니터·잔고·폰)를 가려서, 잠깐 숨기고 대상을 금색 틀로 가리키며 위쪽에 한 줄 요약("내 잔고 5 → 3"). 넘기면 대화창으로.
- `UpdateTask()`: 할 일 글과 강조 대상을 지금 상태로 정함(모니터 닫힘 → 모니터, 열림 → 매수/매도 버튼, 폰 닫힘 → 폰, 목록 → 첫 칸, 상세 → 사용). `PlaceTask()`: 폰이 열리면 할 일 칸을 왼쪽 빈자리로(폰이 위까지 덮어서).
- `Point()`: 틀을 대상 자리·크기 + 여백 3으로, 0.5초 주기 깜빡임(켜짐 0.35).
- `Update()`: 대사 중에만 입력 — Enter·클릭·Space 짧게 = 다음, Space 1초 = 건너뛰기(타이틀로). 일시정지 중엔 무시.
- `Choose()`: 끝 선택 `▶ 시작`/`타이틀`(시작 스토리 예/아니오 자리·방법 그대로). `시작` → `IntroStory.PlayNext = true`로 GameScene 다시(실전은 시작 스토리부터).
- 테스트 입구(저장 안 됨): `simulateAdvance`(한 번 넘김), `autoAdvance`(계속 넘김), `Confirm(i)`.

## 줄별 설명 — 나머지
- `CandleDeck.ForceFixedOrder`: Inspector 테스트 칸과 별개로 코드에서 켜는 고정 순서.
- `IntroStory.PauseAfter`(static): 문장부호 멈춤 계산을 값으로 받는 공용 함수로 꺼냄(전 `Delay`가 이걸 부름 — 동작 같음).
- `DeckRevealView`·`PhoneBuzz`: `IsIntro` → `IsCutscene`(튜토리얼 대사 중에도 덱 공개·폰 진동을 미룸).
- `ResultPanel`: 튜토리얼이면 결과 화면을 띄우지 않음. `RoundTurnView`: 튜토리얼이면 `TUTORIAL`.
- `TitleMenu`(98): 메뉴 6줄(`labels`), `Choose` 번호 하나씩 밀림. `OpenAsk`/`ShowAsk`/`UpdateAsk`/`AnswerAsk`: 권유 창(설정 창과 같은 틀, 글 3·4번째 줄이 선택지), 답하면 `fk.tutorialAsked = 1` 저장. `StartGame(tutorial)`.

## Scene (MCP)
| Scene | 대상 | 값 |
|---|---|---|
| GameScene | `HUD/Tutorial`(TutorialGuide) | 시작 스토리 대화창·선택지·건너뛰기 안내를 같이 씀, 대상 연결(모니터·매수·매도·폰·첫 칸·사용) |
| | `Tutorial/Task` + `Text` | (20, −3) 234×15, 창 테두리 그림, 금색 9(자동 7~9) |
| | `Tutorial/Highlight` | 선 4개를 코드가 붙임(굵기 1, gold2) |
| | `LeverageArea`·`MyBalanceArea`·`ClownBalanceArea` | "여기를 봐" 자리(빈 칸) |
| | `PauseScreen/Settings` | 줄 간격 0 → 28(타이틀과 같게), 창 (40, −34) 240×112, 글 상자 (54, −54) 216×70, 아래 안내 y −130 |
| TitleScene | `Menu/ItemTutorial` | `시작` 아래 새 줄, 메뉴 줄 간격 15 → 13, `Menu` y −99, 로고·부제 4칸 위 |

## 확인 (2026-11-23, MCP Play)
| | 결과 |
|---|---|
| C1 처음부터 끝까지 | 단계 0~11 각본대로: 매수 상승(레버리지 1) → 매수 하락(나 5→3) → 광대 매도 상승 → 내 매도 하락(광대 5→4) → 광대 매수 하락(4→3) → 매수 2번(레버리지 2) → 차트 분석(다음 = 하락) → 매도(광대 3→0) |
| C2 막힘 | 매수 단계에서 폰·아이템 안 열림, 매도 선택 안 됨 / 폰 단계에서 모니터 안 열림 |
| C3 건너뛰기 | 가상 키보드로 Space 1초 → TitleScene |
| C4 끝 | 결과 화면 안 뜸, `▶ 시작 / 타이틀` → `시작` = 시작 스토리부터 실전(라운드 1/3, 아이템 2개씩) |
| C5 타이틀 | 메뉴 6줄 화면 안, 처음 `시작` → 권유 창 → "튜토리얼 먼저" → 튜토리얼, 저장 1(시험 뒤 원래대로 지움) |
| C6 퇴행 | T66(2/0 클리어 → Space → 라운드 2), T65(0/1 패배·결과 화면 → 다시하기 2/2) 통과, Console 오류 0 |
- 시안 고침: ① 대화창이 모니터·잔고를 가림 → "여기를 봐" 단계 ② 강조 틀이 창 그림이라 0.02칸으로 안 보이거나(배율 100) 버튼을 덮음(배율 1) → 단색 선 4개 ③ 할 일 칸이 `TUTORIAL`·폰 제목을 가림 → 폭 234, 폰이 열리면 왼쪽.
- 못 한 것: 실제 키·마우스, 듣기, T67·T68.

## 정정 (2026-11-23, 사용자)
- "단말기" → "모니터". 모니터는 남은 캔들 수를 보여 주지 않음(개수는 덱을 채울 때 공개 연출로만, #39·#59) → 1단계 대사를 고치고 모니터 "여기를 봐"는 "공개된 캔들이 여기 그려진다"로.
- 튜토리얼 대사는 `../../../clown-tell/lines.md` 3장에서 사용자가 고치고 "반영해" → AI가 `TutorialGuide.cs`에 넣음.
