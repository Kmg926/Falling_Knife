# 77: 시작하면 화면이 밝아지며 광대가 말을 건다

**What to build:** 타이틀 `시작` → 0.5초 검게 → GameScene 검은 화면에서 1초 페이드 인 → 아래 대사 상자(이름 "광대" 금색)에 대사 4줄(Grilling Q3 ①~④)이 한 글자씩(0.03초, 클릭음 작게). Space·Enter·클릭 = 줄 전체/다음 줄, Esc = 바로 1라운드. 시네마틱 동안 게임 입력 무시·글자 정보 UI 숨김·덱 공개 연출 미룸. 다시하기 땐 없음. 코드는 AI가 Scripts에 직접(4장 예외), Scene은 MCP.

**Blocked by:** 없음

**Status:** done

- [x] C1 타이틀 어두워짐 → 검정 → 1초 밝아짐
- [x] C2 대사 4줄 한 글자씩, 넘김 조작
- [x] C4 Esc 바로 시작(일시정지 안 열림)
- [x] C5 게임 입력 무시·UI 숨김, 끝나면 덱 공개 연출
- [x] C6 다시하기 → 없음
- [x] C7 퇴행 T65~T68·T143, Console 오류 0 (2026-11-22, `../../regression/README.md`)

Spec: `../spec.md` · Grilling: `../grilling.md` (Q1~Q7·Q10)

## 시연 순서
1. 타이틀 `시작` → 어두워졌다 게임 화면이 밝아짐
2. 광대 대사 → Space로 넘김
3. 다시 시작해서 Esc → 바로 1라운드

## Comments
- 2026-11-22 AI 구현(사용자 "채택, 77 진행"): 새 `IntroStory.cs`, `GameManager`·`DeckRevealView`·`TitleMenu`·`AudioManager` 바뀐 줄(Scripts 직접, 전 코드 `../draft/77/*.before.cs`). Scene(MCP): GameScene `/HUD/Intro`, TitleScene `Fade`.
  - 시안 고침 1번: 3번째 줄이 2줄로 넘침 → 대사 칸 높이 28.
  - C1·C2·C4~C6 MCP 확인, C7은 Console 오류 0만(퇴행 테스트는 아직)(`../draft/77/README.md`, 캡처 4장), Console 오류 0.
  - **남은 것(사용자):** 실제 Space·Enter·클릭·Esc, 글자 소리 크기·속도 느낌.
- 2026-11-22 퇴행 T65~T68·T143 모두 통과(사용자 "퇴행 테스트 돌려줘", 코드 `80d6d49`), Console 오류 0. 절차·결과 `../../regression/README.md`.
