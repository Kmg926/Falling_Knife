# 80: 광대가 무엇을 하는지 말풍선으로 본다

**What to build:** 광대 머리 왼쪽 말풍선(어두운 창 9칸 늘이기 + 새 꼬리 그림, 글자 11, 강조 금색). 광대가 매수·매도를 정하면 "**매수**. 들고 간다."/"**매도**. 너한테 넘기지." → 0.6초 뒤 공개. 아이템을 쓰면 "**(이름)**을/를 쓰지." → 0.8초 뒤 다음 판단. 한 번에 다 보임 + 클릭음 30%. 내가 행동하면 사라짐. 코드는 AI가 Scripts에 직접(4장 예외), 그림은 `tools/`, Scene은 MCP.

**Blocked by:** 없음

**Status:** done

- [x] C1 매수·매도 말풍선 → 0.6초 뒤 공개
- [x] C2 아이템 4종 말풍선 → 0.8초 뒤 다음 판단
- [x] C5 내가 행동하면 사라짐
- [ ] C7 퇴행 T65~T68·T143(아직 안 돌림), Console 오류 0(확인)

Spec: `../spec.md` · Grilling: `../grilling.md` (Q1·Q2·Q4·Q6·Q7·Q9·Q10)

## 시연 순서
1. 내가 매도 → 광대 차례 → 광대 머리 옆 "**매수**. 들고 간다." → 잠깐 뒤 캔들 공개
2. 광대가 아이템이 있을 때 → "**찌라시**를 쓰지."

## Comments
- 2026-11-22 AI 구현(사용자 "채택, 80 진행"): 새 `ClownLine.cs`·`ClownSpeechView.cs`, `GameManager`·`UIManager` 바뀐 줄(Scripts 직접, 전 코드 `../draft/80/*.before.cs`), 꼬리 그림 `tools/make_art.py clowntell`, Scene(MCP) `/HUD/ShakeRoot/ClownSpeech`.
  - Spec 제안 3(광대가 쉬는 차례 "…한 번 쉬지.")도 여기서 넣음.
  - C1·C2·C5 MCP 확인(`../draft/80/README.md`, 캡처 2장), Console 오류 0.
  - **남은 것(사용자):** 실제 플레이에서 읽을 시간·자리·대사 느낌.
