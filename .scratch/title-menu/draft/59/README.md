# 일시정지 버튼(타이틀·계속하기)이 마우스에 반응 안 함 — 고침

> 사용자 "일시정지에서 타이틀, 계속하기 마우스에 반응 안 함"(2026-11-18). AI가 Scripts에 직접(4장 예외). 전 코드 `PausePanel.before.cs`.

## 확인한 것 (MCP)
| 확인 | 결과 |
|---|---|
| EventSystem·InputSystemUIInputModule·GraphicRaycaster | 있음, 켜짐 |
| 멈춘 뒤 버튼 위 UI 판정(RaycastAll) | 맨 위가 ResumeButton·TitleButton — 가리는 것 없음 |
| 버튼에 클릭 이벤트를 직접 보냄 | 계속하기 정상 실행 → 버튼·연결(onClick)은 정상 |
| 흉내 낸 마우스로 클릭 | 안 됨. 다만 Unity가 포커스 없이 돌아 EventSystem이 입력을 무시하는 상태라 **실제 마우스와 같은 조건으로 재현하지는 못함** |

→ 끊긴 곳은 "마우스 → EventSystem → 버튼" 전달 단계. 정확한 원인은 확인하지 못했다.

## 고친 방법
| 부분 | 왜 |
|---|---|
| `PausePanel.Update`: 멈춘 동안 왼쪽 버튼을 **막 누른 순간**의 마우스 위치가 버튼 영역 안이면 `Resume()`/`GoToTitle()` | 타이틀 메뉴(`TitleMenu`)와 같은 방식 — EventSystem 상태와 상관없이 동작 |
| "막 누른 순간" = 이번 프레임 눌림 && 지난 프레임 안 눌림(`wasDown`) | `wasPressedThisFrame`는 흉내 입력에서 놓치는 경우가 있었음 → 직접 비교가 더 확실 |
| `RectangleContainsScreenPoint(..., uiCamera)` | HUD가 Screen Space Camera라 그 카메라 기준으로 계산 |
| 버튼 `onClick`도 그대로 | EventSystem이 되는 환경에선 그쪽도 동작(`Resume`은 이미 풀렸으면 아무 일 없음) |
| 같은 클릭으로 모니터가 열리지 않음 | 게임 입력(클릭 = 실행)은 멈춘 상태에서 먼저 처리돼 무시되고, 그 뒤 Update에서 풀림 — 확인 `monitorOpen=False` |

## 테스트 (Play, 흉내 마우스 누름·뗌)
| 동작 | 결과 |
|---|---|
| 멈춤 → 계속하기 | ✅ paused false, timeScale 1, 모니터 안 열림 |
| 멈춤 → 타이틀 | ✅ TitleScene, timeScale 1 |
| Console 오류 | 0 |

남은 확인(사용자): 실제 마우스로 두 버튼. 결과 화면 버튼(다시하기·타이틀)도 같은 EventSystem 방식이라 같은 문제가 있으면 같은 방법으로 고칠 수 있음.
