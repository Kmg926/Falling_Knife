# 86: Space를 꾹 눌러 시작 스토리를 건너뛴다

**What to build:** 시작 스토리 동안 오른쪽 아래(대사 상자 위)에 픽셀 키캡 `SPACE` + "길게 눌러 건너뛰기"(평소 40%). Space를 누르는 동안 밝아지고 키 안이 왼→오 금색으로 1.0초에 차면 바로 1라운드. 0.25초 안에 떼면 다음 줄(뗄 때), 차다 떼면 0.3초에 비움. Enter·클릭·Esc는 그대로. "준비됐나?" 선택 중: 짧게 = 결정, 길게 = 건너뛰기. 키캡 그림은 `tools/make_art.py`.

**Blocked by:** 없음 (clown-phone Spec 제안 4·5)

**Status:** done

- [x] C4 길게·짧게·차다 뗌·Enter·클릭·Esc
- [x] C5 선택 중 짧게·길게
- [x] C6 퇴행, Console 오류 0

Spec: `../../clown-phone/spec.md` (제안 4·5) · Grilling: `../../clown-phone/grilling.md` (Q4~Q6)

## 시연 순서
1. 타이틀 `시작` → 오른쪽 아래 흐린 `SPACE 길게 눌러 건너뛰기`
2. Space 짧게 → 다음 줄 / Space 꾹 → 키가 금색으로 차고 1라운드

## Comments
- 2026-11-22 AI 구현(사용자 "커밋 후 86"): 키캡 그림 `tools/make_art.py skipkey`, `IntroStory.cs`(길게 누르기·표시·`simulateHold` 테스트 입구, Scripts 직접), Scene(MCP) `SkipHint`.
  - 시안 고침: 키캡 1배는 글자보다 작음 → 2배, 번쩍임은 색 곱하기(1 넘김 무효) → 깜빡임.
  - ①~④ 확인, 퇴행 T65~T68·T143 통과, Console 오류 0(`../draft/86/README.md`).
  - **남은 것(사용자):** 실제 Space 느낌.
