# 97: 광대가 알려 주는 연습 판을 직접 해 보며 규칙을 배운다

**What to build:** Spec 2~5의 튜토리얼 판(단계 0~11). `GameManager` 튜토리얼 훅, 새 `TutorialGuide`, `CandleDeck.ForceFixedOrder`, `ResultPanel`·`RoundTurnView`·`DeckRevealView`·`PhoneBuzz` 작은 수정, GameScene에 `Tutorial`(할 일 글·강조 틀) 구성.

**Blocked by:** 없음

**Status:** done

- [x] C1 각본대로 끝까지, 잔고 5/5 → 3/0
- [x] C2 다른 행동 막힘
- [x] C3 대사 중 입력 막힘, Space 길게 → 타이틀
- [x] C4 끝 선택 시작 / 타이틀
- [x] C6 퇴행, Console 오류 0

Spec: `../spec.md` · Grilling: `../grilling.md`

## 시연 순서
1. 타이틀 `튜토리얼` → 광대 인사 → 덱 공개
2. 할 일 따라 매수·매도·폰 → 광대 잔고 0
3. "이제 진짜다." → `시작`

## Comments
- 2026-11-23 AI 구현(사용자 "전부 채택"): `TutorialGuide.cs`(새)·`GameManager`·`CandleDeck`·`DeckRevealView`·`PhoneBuzz`·`ResultPanel`·`RoundTurnView`·`IntroStory`(Scripts 직접), GameScene `Tutorial`(MCP). 확인·시안 고침·줄별 설명: `../draft/97/README.md`. C3 건너뛰기는 가상 키보드로.
  - **남은 것(사용자):** 실제로 해 보기(대사 길이·속도, 강조 틀 보임).
- 2026-11-23 사용자 정정 "단말기 → 모니터, 모니터에는 남은 캔들 수를 안 보여 줌": 1단계를 "방금 본 게 이번 덱(덱 공개 연출) · 개수는 채울 때 한 번만 · 직접 세어 둬" + 모니터 "여기를 봐"(공개된 캔들이 그려짐, 매수·매도도 여기서)로 바꿈. 튜토리얼 대사·요약·할 일 전부를 `../../clown-tell/lines.md` 3장(#32~75)에 옮겨 사용자가 고칠 수 있게. 대화창 대사 모두 2줄 안 확인.
- 2026-11-23 대사 반영(lines.md 3장): 사용자 수정 #46·#49. 피해 수식 대사 #40·42·50·54 → "판이 커진다"·"다음 피해"로(AI 제안, 사용자가 대안 없다고 해서). 모두 2줄 안.
