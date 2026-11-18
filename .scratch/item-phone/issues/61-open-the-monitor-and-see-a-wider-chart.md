# 61: 모니터를 열면 차트가 넓게 보인다

**What to build:** 모니터 확대 화면(`MonitorZoom`)에서 "다음 캔들은?"(`ZoomPrompt`)과 아래 조작법 줄(`ZoomKeys`)을 없앤다. `tools/make_art_trade.py zoom_panel`을 고쳐 키 안내 구분선(y 135)을 지우고 차트 칸을 아래로 늘린다(y 42~90 → 42~96). 매수·매도 버튼은 y −102로 내린다. 아이템 칸·"아이템" 글·세로 구분선(x 199)은 63까지 그대로 둔다(아이템을 계속 쓸 수 있게). 그림은 AI가 tools로, Scene은 MCP, 코드가 바뀌면 AI가 Scripts에 직접(4장 예외).

**Blocked by:** 없음

**Status:** done

- [x] "다음 캔들은?"·조작법 줄·키 안내 구분선 없음 (C1)
- [x] 차트 칸이 아래로 넓어지고 "?"가 칸 가운데에 맞음 (차트 분석으로 아는 캔들 표시는 자리만 같이 옮김, 화면 확인 안 함)
- [ ] 매수·매도 클릭·A/D·Space·Esc·아이템 1~4 그대로 (C1) — 코드 변경 없음, 매도 실행만 확인. 실제 키·마우스는 사용자 확인
- [x] Console 오류 0 (C7, 하네스는 판정 코드 변경 없어 생략)

Spec: `../spec.md` (제안 1 앞부분) · Grilling: `../grilling.md` (Q4·Q5)

## 시연 순서
1. 게임 시작 → Space로 모니터 열기
2. 위에 "GE코인"만, 아래 조작법 줄 없음, 차트 칸이 전보다 아래로 넓음
3. A/D로 매수·매도 고르고 Space로 실행 → 지금처럼 결과

## Comments
- 2026-11-18 AI 구현(사용자 "커밋 후 61"): `tools/make_art_trade.py`에 `keys` 인자·`zoom61` 명령, `zoom-panel.png` 다시 만듦(32색 OK). GameScene(MCP, 저장 전 dirty 아님 확인): `ZoomPrompt`·`ZoomKeys` 지움, 매수·매도 버튼 y −102, 캔들 자리 y −60/−61. 게임 코드 변경 없음. 설명 `../draft/61/README.md`, 캡처 `../draft/61/zoom-after.png`.
  - 확인: 확대 화면 캡처, 매도 실행 → 공개 → 결과·차례 넘김 정상, Console 오류 0. T65~T68·T143 전체 하네스는 이번엔 돌리지 않음(판정 코드 변경 없음).
  - **남은 것(사용자):** 실제 마우스로 매수·매도 버튼 클릭 확인.
