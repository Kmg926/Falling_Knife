# draft/61: 모니터를 열면 차트가 넓게 보인다

게임 코드(Scripts)는 바꾸지 않았다. 그림 도구와 Scene만 바뀜.

## 그림: `tools/make_art_trade.py`
| 바뀐 곳 | 왜 |
|---|---|
| `zoom_panel(pal, frames=True, keys=True)` — `keys` 인자 추가 | 옛 그림(키 안내 줄 있음)도 다시 만들 수 있게 기본값은 그대로 두고, 61 그림만 `keys=False`로 |
| `cbot`·`cmid`·`dbot` 세 숫자 | 키 줄이 없으면 차트 칸 아래 끝 90 → 96, 기준선·점선 캔들 가운데 66 → 69, 세로 구분선 아래 끝 133 → 147(화면 아래까지) |
| `if keys:` 안의 y 135 구분선 | 조작법 줄 위 가로선 — 조작법을 지우니 같이 지움 |
| `zoom61` 명령 | 확대 화면 그림 1장만 다시 만듦(`game` 명령은 테이블·버튼까지 덮어써서 쓰지 않음) |

## Scene (MCP, GameScene `HUD/ShakeRoot/MonitorZoom/Main`)
| 바뀐 것 | 값 |
|---|---|
| `ZoomPrompt`("다음 캔들은?") | 지움 |
| `ZoomKeys`(조작법 줄) | 지움 — 조작법은 규칙 4쪽에만 |
| `HoldButton`·`SellButton` | y −96 → −102 (차트 칸이 늘어난 만큼 아래로) |
| `ZoomKnown`·`ZoomUnknown` | y −57 → −60, −58 → −61 (점선 캔들 자리 가운데에 맞춤) |

아이템 칸·"아이템" 글·세로 구분선은 Ticket 63에서 폰으로 옮길 때 지운다(그 전까지 아이템을 계속 쓸 수 있게).

## 확인
- 캡처 `zoom-after.png`: 위 "GE코인"만, 조작법 줄 없음, 차트 칸 아래로 넓어짐, "?"가 점선 칸 안
- 매도 실행 → 공개 → 결과·차례 넘김 정상, Console 오류 0
