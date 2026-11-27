# 103: 실행되는 Windows 빌드를 만든다

**What to build:** 최종 빌드 전 정리(F1 개발용 글자, 빌드 폴더 Git 제외, 빌드 동안만 Run In Background 끄기) + 퇴행 전체 + Windows 64비트 빌드 + 실행 확인. 플레이테스트 기록표(`../../playtest/README.md`).

**Blocked by:** 없음

**Status:** done

- [x] F1: `UIManager.Update`를 `#if UNITY_EDITOR || DEVELOPMENT_BUILD`로 감쌈(최종 빌드에선 F1 없음, 편집기·개발용 빌드엔 그대로)
- [x] `.gitignore`에 `/Builds/`
- [x] 퇴행 T65~T68·T143 통과(`40dec5f` 뒤), Console 오류 0
- [x] 빌드 `Builds/FallingKnife/FallingKnife.exe`(149MB, Git 제외) 성공, 실행 → Player.log 오류·예외 없음(D3D12, RTX 4070 SUPER)
- [ ] (사용자) 빌드를 직접 켜서 한 판 해 보기

## Comments
- 2026-11-27 AI(사용자 "진행"): 위 내용. Run In Background는 빌드 동안만 끄고 다시 켬(편집기 MCP 시험용). 줄별 설명 `../draft/103/README.md`.
- 빌드 보고서의 "오류 2개" = "More than one global light on layer Default" — 두 Scene 모두 Global Light 2D는 1개씩이라 빌드가 두 Scene을 함께 검사하며 낸 알림으로 판단(실행 로그엔 없음).
- Player.log의 "Bloom·피사계 심도·모션 블러·Panini 셰이더가 빠짐" 경고: 카메라 후처리 꺼짐(GameScene `m_RenderPostProcessing: 0`, TitleScene 카메라엔 설정 없음), Scene에 Volume 없음 → 쓰지 않는 효과라 화면에 영향 없음. 처음엔 URP 설정 탓으로 보고 다시 빌드했으나(두 번째는 스크립트만 다시 만든 빌드라 폴더를 지우고 세 번째 전체 빌드) 경고는 같음 → 영향 없음으로 결론.
- 빌드 중 Unity가 바꾼 설정(URP 셰이더 사전 거르기 값·Global Settings 자원 목록·Unity Connect 켜짐·Pipeline 백그라운드 덮어쓰기 표시)은 커밋하지 않음(내가 바꾼 것이 아님).
- 경고 513개 대부분은 AI 추론 패키지(Sentis) 셰이더 경고 — 게임은 쓰지 않음. 빌드 크기를 줄이려면 패키지 정리를 따로 검토.
- 2026-11-27 다시 빌드(사용자 "exe에는 바로 반영이 안 되는거지? 반영해줘", 커밋 `6d8ef1d`까지: 폰·모니터·잔고 전광판·배경·지급 제한 등): Run In Background를 빌드 동안만 끄고 다시 켬. 빌드가 Unity 창의 팝업(사용자가 닫음) 때문에 약 10분 멈췄다가 성공(158MB, 오류 2 = 전과 같은 Global Light 알림). 새 exe 15초 실행 → Player.log 오류·예외 없음. 팝업이 Scene을 저장하며 들어간 카메라 rect(Play 흔적)는 MCP로 (0, 0, 1, 1)로 되돌림.
- 2026-11-27 122(콘솔 경고): HUD의 빈 스크립트 자리(TableItemsView) 지움, F1 개발용 글자 조건을 `#if DEVELOPMENT_BUILD` → `Debug.isDebugBuild`(뜻 같음). 글꼴 경고는 그대로. README `../draft/122/README.md`.
