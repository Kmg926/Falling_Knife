# 99: 라운드를 이기면 다음 라운드로 가는 법이 분명히 보인다

**What to build:** 사용자 "라운드 승리 시 Space바 눌러서 넘기라는 텍스트가 잘 안 보임"(2026-11-23, 제안 A 채택). 광대 얼굴 위 금색 글자 대신 가운데 판: 뒤 어둡게(45%) + 창 테두리 판 170×56 + "라운드 N 클리어"(16, 금색) + `SPACE` 키 그림 + "다음 라운드"(천천히 깜빡임 1 ↔ 0.35, 1.2초).

**Blocked by:** 없음

**Status:** done

- [x] 클리어 → 판 보임(캡처), Space → 라운드 2, 판 사라짐
- [x] Console 오류 0
- [ ] 실제로 보기 (사용자)

## Comments
- 2026-11-23 AI 구현(사용자 "전부 채택"): `RoundTurnView.cs`(Scripts 직접), GameScene `ShakeRoot/ClearPanel`(MCP). 줄별 설명 `../draft/99/README.md`.
