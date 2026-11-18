# 55: 크레딧에서 만든 사람과 도구를 본다

**What to build:** 타이틀 메뉴 "크레딧" → 53의 창 그림으로 크레딧 창, 위아래(↑/↓·마우스 휠)로 넘기는 글: 게임 "Falling Knife" / 만든 사람 **Kmg926**(본명·학번·학교 없음 — 공개 저장소) / 엔진 Unity 6.6 (URP 2D) / 글꼴 Galmuri11 by Lee Minseo (quiple), SIL Open Font License 1.1 / 그림·효과음: Claude(Anthropic)가 작성한 코드로 생성, 고르기·지시는 만든 사람 / 코드 도움: Claude / 배경음악: (정해지면 추가) / "비영리 학교 과제". `docs/asset-rights.md` 3장 크레딧 초안을 같은 문구로 고치고 "제출 전 확인 — 크레딧 표기" 줄을 갱신. Esc 닫기. Scene은 MCP, 코드가 필요하면 AI가 작성(4장 예외).

**Blocked by:** 53

**Status:** done

- [x] 크레딧 창 글, 위아래 넘김, Esc 닫기
- [x] 글 넘침 0, 본명·학번·학교·이메일 없음 (C5)
- [x] `asset-rights.md` 3장 초안·확인 줄 갱신
- [x] GameScene 퇴행·Console 오류 0 (C6)

Spec: `../spec.md` (제안 5·8) · Grilling: `../grilling.md` (Q6·Q7)

## 시연 순서
1. 타이틀 → 크레딧 → 만든 사람·엔진·글꼴(OFL)·AI 도움이 보임
2. ↓로 끝까지 → Esc로 닫기

## Comments
- 2026-11-18 AI 구현(사용자 "54 진행" 뒤 이어서 — 사용량 한도로 끊긴 뒤 "continue"): `TitleMenu.cs` 크레딧 창(Scripts 직접, 전 코드 `draft/55/TitleMenu.before.cs`). Scene 변경 없음(53의 창 재사용). 글이 창보다 길어 TMP 쪽 나누기로 2쪽(↑↓·휠·클릭).
  - `docs/asset-rights.md` 3장을 게임 안 문구 전체로 고치고 "크레딧 표기" 확인 줄 체크.
  - 테스트 C5·C6 통과(`../draft/55/README.md`), Console 오류 0. 캡처 `.scratch/art/drafts/title-menu/credits1.png`·`credits2.png`.
  - **남은 것(사용자):** 배경음악이 정해지면 크레딧 문구(`TitleMenu`의 `credits` 칸)와 asset-rights에 추가.
