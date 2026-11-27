# 105: 규칙을 짧은 카드 3쪽에서 찾아본다

**What to build:** 규칙 창 5쪽 → 3쪽. ① 번호 달린 게임 화면 그림 + 이름표 + 목표·조작, ② 행동 표("다음 피해" 말로, 수식 없음)·레버리지·차례·덱 개수, ③ 아이템 7종 한 쪽.

**Blocked by:** 없음

**Status:** done

- [x] ① 그림·번호·이름표 캡처
- [x] ② 수식 없음, 덱 개수·레버리지 최대 3 있음
- [x] ③ 7종이 한 쪽에, 폰에서 고른다는 말
- [x] 컴파일 오류 0

Spec: `../spec.md`

## Comments
- 2026-11-27 AI 구현: `TitleMenu.cs`(Scripts 직접), TitleScene `RulesScreen`·아이템 줄 7개(MCP), 그림 `tools/make_rules_screen.py`. 1·2·3쪽 캡처 확인(2쪽 마지막 줄이 아래 안내와 겹쳐 줄 높이 88%로, 1쪽 아래 글 줄 간격 0). 규칙 뒤 설정 창에 그림 안 남음 확인. 컴파일 오류 0. README `../draft/105/README.md`.
- 2026-11-27 device-art 107·108 뒤 규칙 1쪽 그림을 새 게임 화면으로 다시 캡처(`rules-screen.png`), 배지 1(모니터)을 테두리가 넓어진 만큼 왼쪽으로.
