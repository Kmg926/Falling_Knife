# 53: 타이틀에서 메뉴를 고르고 규칙을 읽는다

**What to build:** TitleScene의 조작 안내 상자(`ControlsBox`)·시작/종료 버튼·"목표" 줄을 없애고, 왼쪽 아래에 글자 메뉴 5줄 **시작 / 규칙 / 설정 / 크레딧 / 종료**(크기 11, 줄 간격 12, 고른 줄 gold2 + "▶", 나머지 cream0)를 둔다. `TitleMenu.cs`를 다시 써서 ↑/↓·W/S 이동(끝에서 반대쪽으로), Space·Enter·클릭 선택, 마우스를 올리면 선택 이동, Esc = 창 닫기(첫 화면에선 아무 일 없음). 시작 → GameScene, 종료 → 끝(에디터 Play 멈춤). **규칙 창**: `closeup-panel.png`를 9-slice로 늘인 창(약 260×150) + 뒤 `pause-dim`, 3쪽(←/→, 아래 "1/3"): ① 목표·턴 + 매수/매도 결과표 4줄 ② 레버리지 0~3 공용·피해 시 0 + 아이템 4종(아이콘 + `ItemData` 글) + 지급 규칙 ③ 조작법. 설정·크레딧 줄은 54·55 전까지 "준비 중" 창. 코드는 AI가 Scripts에 직접(4장 예외), Scene은 MCP.

**Blocked by:** 없음

**Status:** ready-for-agent

- [ ] 메뉴 5줄 표시, 조작 안내 상자·시작/종료 버튼·목표 줄 없음
- [ ] ↑/↓·W/S 이동(돌기), Space·Enter·클릭 선택, 마우스 올리면 선택 (C1)
- [ ] 시작 → GameScene, 종료 → Play 멈춤, 첫 화면 Esc 무반응, 창에서 Esc → 메뉴 (C2)
- [ ] 규칙 3쪽 ←/→ 넘김, 글 넘침 0, 아이템 글 = `ItemData` (C3)
- [ ] GameScene 퇴행 T65~T68·T143, Console 오류 0 (C6)

Spec: `../spec.md` (제안 1·2·3·6·8) · Grilling: `../grilling.md` (Q1~Q4·Q9)

## 시연 순서
1. 타이틀 → 왼쪽 아래 메뉴, "▶ 시작"이 금색
2. ↓로 "규칙" → Space → 규칙 창 1/3 → → 2/3(아이템) → 3/3(조작법) → Esc로 닫기
3. "시작" → 게임 화면

## Comments
