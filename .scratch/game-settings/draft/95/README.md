# draft 95: X로 설정을 닫고, 음량 막대를 눌러·끌어 맞춘다

> 2026-11-23. 코드는 AI가 Scripts에 직접(game-settings 방식, CLAUDE.md 4장 예외). `*.before.cs` = 바꾸기 전.

## 줄별 설명 — SettingsPanel.cs (면담 대비)
- 새 칸: `closeButton`(X 사각형), `closeLabel`(X 글자), `closeNormal`(cream0).
- `CloseClicked`: 이번 프레임에 X를 눌렀나. 이 부품은 "닫아 달라"고만 알리고, 실제 닫기는 각 화면이 함(타이틀 `Back()`, 게임 중 `game.CloseSettings()`) — 닫는 방법이 화면마다 달라서.
- `dragRow`: 음량 막대를 누른 채 끄는 중인 줄(0 전체 소리, 1 음악, −1 아님).
- `Begin()`: X를 켜고 끌기·닫기 상태를 처음으로. `End()`: X 숨김(타이틀 창은 규칙·크레딧과 같이 씀).
- `HandleInput()`
  1. 끄는 중이면: 버튼을 누르고 있는 동안 `StepAt(..., clamp: true)`로 그 자리 크기, 떼면 끝. 끄는 동안은 다른 처리 안 함.
  2. X: 마우스가 X 사각형 안이면 금색, 누르면 `CloseClicked = true`.
  3. 줄: 전과 같이 올리면 그 줄. 누르면 음량 줄(0·1)은 `StepAt(..., clamp: false)` — 막대 근처면 그 크기 + 끌기 시작, 아니면(이름 쪽) 전과 같은 `Change(1)`.
- `StepAt()`: TMP가 그린 글자 정보(`textInfo.characterInfo`)에서 그 줄의 ■□ 10칸 왼쪽 끝(`origin`)·오른쪽 끝(`xAdvance`)을 찾음 → 한 칸 폭. 마우스 화면 좌표를 글 상자 안 좌표로 바꿔(`ScreenPointToLocalPointInRectangle`) 몇 번째 칸인지 `Floor + 1`(누른 칸까지 켜짐). 막대 왼쪽 바깥 = 0. `clamp`가 false면 막대 근처(왼쪽 1칸 ~ 오른쪽 % 글자)가 아닐 때 −1.
  - 값 글자는 `<pos=40%>`로 늘 같은 자리에서 시작해서, ▶ 표시가 붙어도 막대 위치가 안 움직임.
- `SetVolume()`: 크기가 바뀔 때만 GameSettings에 저장(→ 음악·효과음이 알림으로 바로 따라옴) + 딸깍 + 다시 그림.

## PausePanel.cs / TitleMenu.cs
- 설정 창 입력을 `SettingsPanel`에 넘긴 뒤 `CloseClicked`면 각자 닫기. 타이틀 `Back()`은 설정을 닫을 때 `settings.End()`로 X를 숨김.

## Scene (MCP)
| Scene | 대상 | 값 |
|---|---|---|
| GameScene | `PauseScreen/Settings/CloseButton` + `Label` "X" | (254, −46) 13×13, 창 테두리 그림, 처음 꺼짐 |
| TitleScene | `MenuPanel/CloseButton` + `Label` "X" | (274, −10) 13×13, 같은 그림, 처음 꺼짐 |
- TitleScene 저장 때 Camera 화면 비율(rect) 값도 같이 저장됨(창 크기에 따라 자동으로 바뀌는 값).

## 확인 (2026-11-23, MCP Play, 가상 마우스 장치)
| | 결과 |
|---|---|
| 칸 클릭 7·1·10·3 | 70·10·100·30% |
| 막대 왼쪽 바로 바깥 | 0% |
| 5번째 칸 누르고 8번째로 끌기 | 80% |
| 이름 쪽 클릭 | 한 단계(+10%) |
| X 올림 / 클릭 | 금색 / 타이틀 창 닫힘·X 숨김, 규칙 창엔 X 없음 |
| 게임 중 톱니 → X | 설정 닫힘, 멈춤 풀림(timeScale 1) |
| Console | 오류 0 |
- 시안 고침: 처음엔 반올림이라 7번째 칸 가운데를 누르면 60% → "누른 칸까지"로.
