# 93: 라운드마다 다른 잔고로 시작한다

**What to build:** 라운드 시작 잔고를 라운드 표의 범위에서 뽑는다. 1·2라운드 5~7, 3라운드 10 고정(사용자 "체력을 라운드마다 랜덤, 마지막 라운드만 10, 나머지 5~7", 2026-11-23). 나·광대는 같은 값. 다시하기·다음 라운드마다 새로 뽑음. 회복 상한은 그 라운드 시작 잔고(전과 같은 `StartingBalance`).

**Blocked by:** 없음 (#25·Ticket 12의 10/10/10을 바꿈, gdd-changes #79)

**Status:** done

- [x] R1·R2 각 60번: 5·6·7만 나옴, R3 60번: 10만 / 나·광대 같음, 시작 = 가득
- [x] 화면: "나 7/7 · 광대 7/7", 동전 7개
- [x] Console 오류 0
- [ ] 퇴행 T65~T68(절차의 잔고 칸을 범위 2·2로 바꿈) — 아직 안 돌림
- [ ] 직접 플레이: 5~7이 너무 짧지 않은가 (사용자)

## 시연 순서
1. Play → 시작 잔고가 5~7 중 하나(둘 같음)
2. 2라운드 → 다시 5~7 중 하나 / 3라운드 → 10

## Comments
- 2026-11-23 AI 구현(사용자 지시): `RoundConfig.cs`(`startingBalance` → `minBalance`·`maxBalance`), `GameManager.cs` `StartRound`(Scripts 직접), Scene 라운드 표 값(MCP). 줄별 설명 `../draft/93/README.md`.
