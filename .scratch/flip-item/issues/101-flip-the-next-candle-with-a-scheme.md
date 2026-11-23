# 101: 작전으로 다음 캔들을 뒤집는다

**What to build:** Spec 3·5의 규칙·코드(ItemType·CandleDeck·GameManager·ClownAI·ClownLine·말풍선·결과 판·튜토리얼 한 줄)와 ItemData 에셋.

**Blocked by:** 없음

**Status:** done

- [x] C1 내가 작전
- [x] C2 광대 작전
- [x] C4 퇴행·오류 0

Spec: `../spec.md` · Grilling: `../grilling.md`

## Comments
- 2026-11-23 AI 구현(사용자 "전부 채택"): ItemType.Flip·CandleDeck.FlipTop·GameManager.UseFlip(무게 0.5·글자 F·대사·결과 판 신호)·ClownAI ①′·ClownLine 2개·말풍선 기본값·LotteryPopupView 작전 판·TutorialGuide 한 줄(Scripts 직접), Flip.asset(MCP), GameScene 배열·말풍선 표(MCP). 확인: 내 작전(하락→상승, 개수 D1U5→D0U6, 알던 값 뒤집힘, 결과 판), 광대(하락 알면 작전 → 매수, 모르면 안 씀), T66, 오류 0. README `../draft/101/README.md`.
