# 98: 타이틀에서 튜토리얼을 찾고, 처음엔 권유를 받는다

**What to build:** 타이틀 메뉴 `시작` 아래 `튜토리얼` 줄(→ 튜토리얼 판). 처음 `시작`을 누르면 한 번만 "처음이라면 튜토리얼부터 볼래?" `▶ 튜토리얼 먼저` / `바로 시작`(저장 `fk.tutorialAsked`).

**Blocked by:** 97

**Status:** done

- [x] C5 `튜토리얼` 줄 → 튜토리얼, 처음 `시작` 권유 한 번만(두 번째부터 안 물음)
- [ ] (사용자) 실제 키보드 ↑↓·Space·Enter, 마우스 올림·클릭, Esc = 닫기(시작 안 함)

Spec: `../spec.md`

## Comments
- 2026-11-23 AI 구현: `TitleMenu.cs`(Scripts 직접), TitleScene `ItemTutorial`·메뉴 줄 간격 13(MCP). 권유 창 캡처·답 저장 확인(시험 뒤 저장값 지움 → 처음 `시작` 때 다시 물음). README: `../draft/97/README.md`.
