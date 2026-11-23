# draft 99: 라운드 클리어 판

> 2026-11-23. 코드는 AI가 Scripts에 직접(사용자 지시, CLAUDE.md 4장 예외와 같은 방식). `RoundTurnView.before.cs` = 바꾸기 전.

## 줄별 설명 — RoundTurnView.cs (면담 대비)
- 새 칸: `clearPanel`(판 전체), `clearTitle`("라운드 N 클리어"), `clearHint`(CanvasGroup — SPACE 키와 안내를 묶어 투명도로 깜빡임), `hintBlinkSeconds 1.2`.
- `Refresh`: 판이 연결돼 있으면 옛 가운데 글자(`noticeText`)는 비우고, `IsRoundCleared` 동안만 판을 켬. 연결이 없으면 전처럼 글자(안전장치).
- `Update`: 판이 켜져 있으면 `Mathf.PingPong`(0 → 1 → 0을 오가는 값)으로 안내 투명도 1 ↔ 0.35. `unscaledTime`이라 멈춤과 상관없음.

## Scene (MCP, GameScene `HUD/ShakeRoot/ClearPanel`, 모니터 확대 화면 바로 앞 순서)
| 대상 | 값 |
|---|---|
| Dim | 화면 전체 검정 45% |
| Box | 가운데 (0, 8) 170×56, 창 테두리 그림 |
| Title | "라운드 1 클리어" 16, gold2 |
| Hint/Key·Label | 시작 스토리 건너뛰기의 `SPACE` 키 그림 27×11 + "다음 라운드" 9, cream |
