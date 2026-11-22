# 89: 회복 아이템으로 잔고를 되찾는다

**What to build:** 아이템 2종 — 배당금(잔고 +1), 구제 금융(잔고 +2), 최대 = 시작 잔고. 가득이면 못 쓰고 아이템 남음(상세에 "(잔고가 가득이라 못 쓴다)"). 지급 무게 기존 각 1·배당금 1·구제 금융 0.5(복권 +1도). 그림(작은·폰 칸·큰) `tools/`, 데이터 에셋, 폰·규칙 창에 표시.

**Blocked by:** 없음

**Status:** done

- [x] C1 지급·폰 표시 · [x] C2 회복량·가득 · [x] C3 지급 비율 · [x] C5 규칙 창 · [x] C6 퇴행·오류 0

Spec: `../spec.md` (제안 1~5) · Grilling: `../grilling.md` (Q1~Q3·Q5)

## 시연 순서
1. 잔고가 줄었을 때 폰에서 배당금 → 잔고 +1
2. 잔고 10에서 구제 금융 → "가득이라 못 쓴다"

## Comments
- 2026-11-23 AI 구현(사용자 "채택, 89 진행"): 아이콘 `tools/make_art_hd_items.py heal`(시안 고침 1번: 자루가 얼굴처럼 보임), `ItemType`·`GameManager`·`ItemCloseupPanel`·`UIManager`·`TitleMenu`(Scripts 직접), 데이터 에셋 2개·Scene 배열·규칙 5쪽(MCP). C1~C3·C5·C6 확인(`../draft/89/README.md`).
  - **남은 것(사용자):** 실제 플레이 빈도·아이콘 느낌.
