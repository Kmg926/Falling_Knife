# draft/70: 폰 소프트키 마우스 올림 반응 + 상세 아이콘 첫 움직임 없앰 (63 고침)

사용자: "폰에서 확인, 닫기, 사용, 뒤로 마우스 반응 생성해줘. 아이템 상세에 들어왔을 때 아이템이 갑자기 아래로 움직이는 게 어색해".

## PhoneView.cs
| 바뀐 곳 | 왜 |
|---|---|
| `using UnityEngine.InputSystem;` | 마우스 위치(`Mouse.current`) |
| `softNormal`(cream)·`softHover`(gold2) | 보통·올림 글자 색(일시정지·타이틀 메뉴와 같은 금색) |
| `leftLabel`·`rightLabel`, `Refresh` → `ShowSoftKeys()` | 원래 글(확인/사용, 닫기/뒤로)을 따로 두고, 올림이면 앞에 "▶ "를 붙여 그림 |
| `Awake`에서 `uiCamera` | HUD Canvas가 Screen Space Camera라 마우스 위치를 이 카메라로 계산 |
| 새 `Update()` | 폰이 열려 있을 때 매 프레임 마우스가 `SoftLeftHit`·`SoftRightHit` 안인지 직접 확인(일시정지 버튼과 같은 방식). 바뀔 때만 다시 그림. 글이 빈 칸(광대 폰 상세 왼쪽)은 반응 안 함 |
| `IsOver(button, pos)` | `RectTransformUtility.RectangleContainsScreenPoint` |

## ItemCloseupPanel.cs
| 바뀐 곳 | 왜 |
|---|---|
| `dropIn = false` + `drop = dropIn && …` | 열릴 때 6도트 위에서 툭 떨어지던 등장 끔(Scene에 `dropSeconds` 0.2가 저장돼 있어 새 칸으로 끔) |
| `Bob(t + bobSeconds / 4f)` | 뜸이 맨 아래(−1)에서 시작해 열자마자 1도트 내려갔음 → 올라가는 중간(0)부터 시작 |

## 확인 (Play)
- 상세를 연 순간 아이콘 y = −12(제자리). 처음 0.6초: 0 → +1(위 멈춤).
- "뒤로" 가운데 화면 좌표에서 `IsOver` = 참, 올림 표시 "▶ 뒤로"·#D9B45A. 실제 마우스 올림은 직접 확인 필요(MCP는 진짜 마우스를 못 움직임).
