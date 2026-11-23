# draft 94: 마우스를 올린 버튼이 어떤 건지 보인다

> 2026-11-23. 코드는 AI가 Scripts에 직접(사용자 지시). `*.before.cs` = 바꾸기 전. Scene 변경 없음.

## 줄별 설명 — ActionButtonsView.cs (면담 대비)
- `using UnityEngine.InputSystem`: `Mouse.current`(지금 마우스)를 읽으려고.
- `uiCamera`: HUD Canvas가 Screen Space - Camera라 "마우스가 버튼 안인가"를 이 카메라 기준으로 계산(Overlay면 null).
- `Update()`: 내 차례(`CanPlayerAct`)이고 마우스가 **이번 프레임에 움직였을 때만**(`delta`가 0이 아님) 매수·매도 버튼 위인지 보고 `game.Select(...)`.
  - 움직였을 때만 = 키보드 A/D로 고른 것을 가만히 있는 마우스가 바로 덮어쓰지 않게(타이틀 메뉴 76과 같은 규칙).
  - 고르기 자체는 GameManager가 함(판정은 GameManager) → 바뀔 때만 클릭음, 밝은 그림은 기존 `Refresh`가 그림.
- `IsOver()`: `RectangleContainsScreenPoint` = 화면 좌표가 버튼 사각형 안인지(일시정지·폰과 같은 함수).

## 줄별 설명 — ResultPanel.cs
- 새 칸: `highlightText`(gold2), `normalText`(cream0), `dimButton`(0.6 곱하기) — 일시정지 메뉴와 같은 값.
- `Awake`: 버튼 2개(0 다시하기, 1 타이틀)와 그 글자·원래 글을 기억.
- `Update()`: 결과 화면이 켜져 있는 동안 마우스가 올라간 버튼을 강조. 버튼 밖으로 나가면 마지막 강조를 유지(일시정지와 같음).
- `SetHighlight(i)`: 고른 버튼은 "▶ " + 금색 + 원래 밝기, 나머지는 흐린 글 + 어두운 그림.
- `Refresh`: 결과 화면이 **막 켜진** 순간(`wasOver`가 false → true) "다시하기" 강조.

## 확인 (2026-11-23, MCP Play)
| | 결과 |
|---|---|
| 위치 판정 | 매수 (657, 272)·매도 (953, 272)·다시하기·타이틀 가운데 = 안, 매수 자리에서 매도 = 밖 |
| 결과 화면 | 처음 "▶ 다시하기 / 타이틀", 타이틀 강조 → "다시하기 / ▶ 타이틀"(캡처: 금색 ▶ 타이틀) |
| Console | 오류 0 |
| 못 한 것 | 실제 마우스 이동: Unity 창에 포커스가 없어 MCP 마우스 이동이 입력에 남지 않음(키보드와 같은 이유) |
