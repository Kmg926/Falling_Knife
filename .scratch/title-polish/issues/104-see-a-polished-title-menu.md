# 104: 다듬어진 타이틀 메뉴를 본다

**What to build:** 사용자 레퍼런스 4개(Memory Leak·Halo·Hollow Knight·Rock Paper Scissors Shoot!)를 보고 고른 개선안 1~4(2026-11-27 "1~4 진행"). 광대는 그대로.
1. 고른 줄 뒤 금색 띠(Halo) — 왼쪽 진하고 오른쪽으로 사라짐
2. ▶ 대신 고른 줄 왼쪽 작은 칼(끝이 아래, Hollow Knight 장식을 게임 주제로)
3. 아래 구석 안내 "↑↓ 고르기 · Space 결정"(오른쪽)·"v1.0"(왼쪽)
4. 로고 뒤 은은한 붉은 빛
(5 메뉴 테두리 버튼은 6줄이라 답답해져 제외, 6 메뉴 아이콘은 선택 — 이번엔 안 함)

**Blocked by:** 없음

**Status:** done

- [x] 띠·칼이 고른 줄로 따라감(시작 → 튜토리얼 캡처)
- [x] 아래 안내·버전, 로고 빛 캡처
- [x] 컴파일 오류 0
- [x] 칼 없앰(사용자 2026-11-27)
- [ ] (사용자) 실제로 보고 띠 진하기 판단

## Comments
- 2026-11-27 AI: 그림 `tools/make_title_ui.py`(3장, 32색 팔레트, 반투명 대신 바둑판 점), `TitleMenu.cs`(Scripts 직접, 띠·칼 자리 옮김, 칼이 있으면 ▶ 안 씀), TitleScene(MCP) `Menu/SelectBar`·`Menu/Knife`·`LogoGlow`·`FooterHint`·`Version`. 시안 고침: 칼이 띠 왼쪽 기둥에 겹쳐 4칸 왼쪽으로(−11 → −15). 줄별 설명 `../draft/104/README.md`.
- 같은 날 앞서: 타이틀 글자 위로·메뉴 간격 16(Ticket 98 댓글).
- 2026-11-27 사용자 "칼은 없애자": TitleScene `Menu/Knife` 삭제(MCP), `title-knife.png` 삭제, `TitleMenu.cs` 칼 칸·자리 값 삭제(▶를 안 쓰는 조건을 "칼이 있으면" → "띠가 있으면"으로). 띠만 남은 캡처 확인, 컴파일 오류 0. README 끝에 바뀐 줄.
