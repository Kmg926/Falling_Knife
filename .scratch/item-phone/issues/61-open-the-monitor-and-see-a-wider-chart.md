# 61: 모니터를 열면 차트가 넓게 보인다

**What to build:** 모니터 확대 화면(`MonitorZoom`)에서 "다음 캔들은?"(`ZoomPrompt`)과 아래 조작법 줄(`ZoomKeys`)을 없앤다. `tools/make_art_trade.py zoom_panel`을 고쳐 키 안내 구분선(y 135)을 지우고 차트 칸을 아래로 늘린다(y 42~90 → 42~96). 매수·매도 버튼은 y −102로 내린다. 아이템 칸·"아이템" 글·세로 구분선(x 199)은 63까지 그대로 둔다(아이템을 계속 쓸 수 있게). 그림은 AI가 tools로, Scene은 MCP, 코드가 바뀌면 AI가 Scripts에 직접(4장 예외).

**Blocked by:** 없음

**Status:** ready-for-agent

- [ ] "다음 캔들은?"·조작법 줄·키 안내 구분선 없음 (C1)
- [ ] 차트 칸이 아래로 넓어지고, 차트 분석으로 아는 캔들·"?"가 칸 가운데에 맞음
- [ ] 매수·매도 클릭·A/D·Space·Esc 그대로, 아이템 칸 1~4 그대로 (C1)
- [ ] 퇴행 T65~T68·T143, Console 오류 0 (C7)

Spec: `../spec.md` (제안 1 앞부분) · Grilling: `../grilling.md` (Q4·Q5)

## 시연 순서
1. 게임 시작 → Space로 모니터 열기
2. 위에 "GE코인"만, 아래 조작법 줄 없음, 차트 칸이 전보다 아래로 넓음
3. A/D로 매수·매도 고르고 Space로 실행 → 지금처럼 결과

## Comments
