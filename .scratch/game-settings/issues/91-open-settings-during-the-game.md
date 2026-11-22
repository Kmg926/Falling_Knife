# 91: 게임 중에 설정을 연다

**What to build:** 왼쪽 위 톱니 + 일시정지 메뉴 "설정"(계속 / 설정 / 타이틀) → 타이틀과 같은 설정 창(전체 소리·음악·흔들림·전체화면), 바로 반영·저장. 설정 코드를 공통 `SettingsPanel`로 빼서 타이틀·게임이 같이 씀.

**Blocked by:** 없음

**Status:** done

- [x] C1 여는 길 두 가지·닫기 · [x] C2 바로 반영·저장 · [x] C3 타이틀 설정 그대로 · [x] C4 톱니 동작 때 · [x] C5 퇴행·오류 0

Spec: `../spec.md` · Grilling: `../grilling.md` (Q6·Q7)

## Comments
- 2026-11-23 AI 구현(사용자 "커밋 후 91"): 새 `SettingsPanel.cs`, `TitleMenu`·`GameSettings`·`MusicPlayer`·`AudioManager`·`UIManager`·`GameManager`·`PausePanel`(Scripts 직접), 톱니 그림 `tools/make_art.py gear`, Scene(MCP) 메뉴 묶음·설정 버튼·설정 창·톱니. C1~C5 확인(`../draft/91/README.md`).
  - **남은 것(사용자):** 실제 키·마우스.
