# draft 82: 시작 스토리 다듬기 — 음악 70%, 진동은 스토리 뒤, 게임 UI 숨김

> 2026-11-22. 사용자 "음악 사운드 70%로 감소시키고, 아이템 지급(핸드폰 진동)은 시네마틱 끝나고 진행. 그리고 레버리지 화면같은 게임에 필요한 UI도 시네마틱에서는 제외시켜".
> 코드는 AI가 Scripts에 직접(intro-story, CLAUDE.md 4장 예외). `*.before.cs` = 바꾸기 전.

## 바뀐 것
| 파일 | 바뀐 것 | 이유 |
|---|---|---|
| `AudioManager.cs` | `musicBase` 0.5 → **0.35**(코드 기본값 + Scene 값) | 0.5 × 0.7. 설정 100%일 때 음악 크기 0.35 |
| `PhoneBuzz.cs` | 기다리는 조건 `IsPresenting` → `IsIntro \|\| IsPresenting` | 시작 스토리 중엔 덱 공개 연출이 아직 시작 전이라 바로 울렸음. 스토리가 끝나면 같은 프레임에 덱 공개 연출이 시작되므로 그 끝까지 기다리게 됨 |
| `IntroStory.cs` | 새 칸 `hideArtDuringIntro`(탁자 위 그림) → `Renderer.forceRenderingOff`, 숨긴 HUD는 `blocksRaycasts = false` | 아래 |

## 줄별 설명 (면담 대비)
- `forceRenderingOff` — 그림을 **그리기만** 끈다. 오브젝트를 끄거나(SetActive) 색을 바꾸지 않으니, 레버리지 불꽃·공개 캔들처럼 각 View가 켜고 끄는 그림과 부딪히지 않는다. 스토리가 끝나면 false로 되돌림.
- `GetComponentsInChildren<Renderer>(true)` — 그 오브젝트와 자식(꺼져 있는 것 포함)의 그림을 모두 모음(예: 공개 캔들의 테두리 반짝임).
- `blocksRaycasts = false` — 투명하게 한 폰·모니터 버튼이 마우스를 받지 않게(게임 입력은 원래도 무시되지만 마우스 올림 표시 등이 생기지 않게). 끝나면 true.

## 숨기는 것 (Scene, MCP)
| 종류 | 숨김 | 남김(방 분위기) |
|---|---|---|
| 탁자 위 그림(`hideArtDuringIntro`, 10개) | `Terminal`(내 GE코인 모니터), `LeverageBox`, `Flame1~3`, `ShownCandle`, `KnownCandle`, `MonitorOutline`, `BaselineFlash`, `NextCandleBox` | `Room`, `Table`, 조명 3개, `ChartLines`(벽 차트 선), `ClownMonitor`(광대 모니터), 광대 |
| HUD(`hideDuringIntro`, 8 → 14개) | 전부터: `HudBar`·잔고 2·`RoundTurn`·`TurnBanner`·레버리지 글자 2·`NextDamage` / 추가: `TerminalTitle`("GE코인")·`RemainingText`·`QuestionMark`·`PhoneClosed`·`ClownPhoneClosed`·`MonitorButton` | |

- intro-story Spec 제안 1("탁자 위 폰·모니터는 그대로")을 사용자 지시로 바꿈.

## 확인 (2026-11-22, MCP)
| | 결과 |
|---|---|
| 음악 | 설정 100% → 0.350 (공개 중은 0.07) |
| 스토리 중 화면 | 방·탁자·광대·광대 모니터·벽 차트 선만(`intro-clean.png`) |
| 스토리 중 진동 | 진동음 시각 −1(안 울림), 진동은 기다리는 중 |
| 넘긴 뒤 | 숨긴 것 모두 돌아옴(레버리지 상자 그림 켜짐, 폰 투명도 1·클릭 받음), 덱 공개 연출 → 끝난 직후 진동음(`after-intro.png`) |
| Console | 오류 0 |

**남은 것(사용자):** 음악 크기, 스토리 화면 느낌 직접 보기/듣기.
