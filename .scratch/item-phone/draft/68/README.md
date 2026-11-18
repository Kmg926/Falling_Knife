# draft/68: 폰 상세 화면 — 돌아가는 아이콘 멈춤, 분류 지우고 이름만 (63 고침)

사용자: "휴대폰 자세히 보기에서 돌아가는 아이템 아트워크가 어색함. 각 아이템의 키워드(조작, 방해 등)는 필요 없고 아이템 이름만 표시해줘".

## ItemCloseupPanel.cs
| 바뀐 줄 | 왜 |
|---|---|
| `[SerializeField] bool spin = false;` | 회전 그림(ui-revamp 32, 8장)을 켜고 끄는 칸. 기본은 끔 |
| `SpinFrame`: `if (!spin \|\| …) return bigIcons[…]` | 끄면 평면 큰 아이콘. 위아래 1도트 뜸·처음 툭 떨어짐은 그대로 |

## Scene (MCP, GameScene `PhoneZoom/Body/ItemCloseup`)
- `Tag`(분류 표 그림)·`Category`(분류 글자) 끔. 코드는 그대로 글자를 넣지만 보이지 않음.
- `Name` (52, −11) → (52, −19): 큰 아이콘(높이 26) 세로 가운데에 이름 한 줄.

## 확인
- 차트 분석 상세: 평면 돋보기 아이콘(`item-chart-big`) + "차트 분석"만, 분류 없음. 캡처 `name-only.png`.
