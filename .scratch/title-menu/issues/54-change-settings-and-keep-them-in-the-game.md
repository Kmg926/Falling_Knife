# 54: 설정을 바꾸면 게임에 남는다

**What to build:** 새 `GameSettings.cs`(정적: `PlayerPrefs` 키 `fk.master`·`fk.music`·`fk.shake`·`fk.fullscreen` 불러오기·저장·기본값 = 전체 100%·음악 60%·흔들림 켬·전체화면 = 지금 창 상태). 타이틀 메뉴 "설정" → 53의 창 그림으로 설정 창 4줄: "전체 소리 ■■■■■■■■■■ 100%" · "음악 ■■■■■■□□□□ 60%" · "화면 흔들림 켬" · "전체화면 켬". ↑/↓ 줄, ←/→ 값(소리 10% 단위, 켬/끔 전환), 줄 클릭 = 오른쪽 한 단계, 소리 바꾸면 클릭음으로 바로 들려 줌, Esc 닫기. GameScene: `AudioManager.Awake`가 `masterVolume`·`musicVolume`(막대 100% = 지금 0.6 기준 비율)을, `UIManager.Awake`가 `screenShake`를 `GameSettings`에서 읽음. 전체화면은 `Screen.fullScreen`(에디터 효과 없음 → 빌드에서 확인). 코드는 AI가 Scripts에 직접(4장 예외), Scene은 MCP.

**Blocked by:** 53

**Status:** done

- [x] 설정 창 4줄, ↑/↓·←/→·클릭·Esc, 막대·켬/끔 글 바로 바뀜
- [x] 값 저장 → 타이틀 다시 켜도 그대로 (C4)
- [x] GameScene에서 `AudioManager` 전체·음악 값, `UIManager.screenShake`가 설정을 따름 (C4)
- [x] 소리 0%면 효과음 안 남, 흔들림 끔이면 맞아도 안 흔들림
- [x] GameScene 퇴행 T65~T68·T143, Console 오류 0 (C6)

Spec: `../spec.md` (제안 4·7·8) · Grilling: `../grilling.md` (Q5)

## 시연 순서
1. 타이틀 → 설정 → 음악 30%, 흔들림 끔
2. Esc → 시작 → 음악이 작고, 맞아도 화면이 안 흔들림
3. 게임 다시 켬 → 설정이 그대로

## Comments
- 2026-11-18 AI 구현(사용자 "54 진행"): 새 `GameSettings.cs`, `TitleMenu.cs` 설정 창, `AudioManager`·`UIManager` `Awake`에서 설정 읽기(Scripts 직접, 전 코드 `draft/54/*.before.cs`). TitleScene에 `TitleSfx`(AudioSource) + `click.wav`(MCP).
  - **실수 기록:** TitleScene이 저장 안 된 상태(dirty)였는데 비교 없이 저장함 → 바로 마지막 커밋과 비교: 132줄 추가·0줄 삭제, 모두 이번에 넣은 `TitleSfx`·연결 칸뿐(사용자 변경 없음 확인). 이후 Play 뒤 dirty는 TMP 글 색 동기화라 저장하지 않음.
  - 시안 고침: 설정 값이 이름 길이에 따라 들쭉날쭉 → TMP `<pos=40%>`로 같은 자리.
  - 테스트 C4·C6 통과(`../draft/54/README.md`), Console 오류 0. 테스트 뒤 설정 키를 지워 기본값으로 되돌림. 캡처 `.scratch/art/drafts/title-menu/settings.png`.
  - **남은 것(사용자):** 빌드에서 전체화면, 실제 키·마우스로 설정 바꾸기.
