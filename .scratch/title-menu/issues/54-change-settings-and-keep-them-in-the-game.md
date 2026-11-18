# 54: 설정을 바꾸면 게임에 남는다

**What to build:** 새 `GameSettings.cs`(정적: `PlayerPrefs` 키 `fk.master`·`fk.music`·`fk.shake`·`fk.fullscreen` 불러오기·저장·기본값 = 전체 100%·음악 60%·흔들림 켬·전체화면 = 지금 창 상태). 타이틀 메뉴 "설정" → 53의 창 그림으로 설정 창 4줄: "전체 소리 ■■■■■■■■■■ 100%" · "음악 ■■■■■■□□□□ 60%" · "화면 흔들림 켬" · "전체화면 켬". ↑/↓ 줄, ←/→ 값(소리 10% 단위, 켬/끔 전환), 줄 클릭 = 오른쪽 한 단계, 소리 바꾸면 클릭음으로 바로 들려 줌, Esc 닫기. GameScene: `AudioManager.Awake`가 `masterVolume`·`musicVolume`(막대 100% = 지금 0.6 기준 비율)을, `UIManager.Awake`가 `screenShake`를 `GameSettings`에서 읽음. 전체화면은 `Screen.fullScreen`(에디터 효과 없음 → 빌드에서 확인). 코드는 AI가 Scripts에 직접(4장 예외), Scene은 MCP.

**Blocked by:** 53

**Status:** ready-for-agent

- [ ] 설정 창 4줄, ↑/↓·←/→·클릭·Esc, 막대·켬/끔 글 바로 바뀜
- [ ] 값 저장 → 타이틀 다시 켜도 그대로 (C4)
- [ ] GameScene에서 `AudioManager` 전체·음악 값, `UIManager.screenShake`가 설정을 따름 (C4)
- [ ] 소리 0%면 효과음 안 남, 흔들림 끔이면 맞아도 안 흔들림
- [ ] GameScene 퇴행 T65~T68·T143, Console 오류 0 (C6)

Spec: `../spec.md` (제안 4·7·8) · Grilling: `../grilling.md` (Q5)

## 시연 순서
1. 타이틀 → 설정 → 음악 30%, 흔들림 끔
2. Esc → 시작 → 음악이 작고, 맞아도 화면이 안 흔들림
3. 게임 다시 켬 → 설정이 그대로

## Comments
