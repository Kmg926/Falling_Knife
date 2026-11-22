# Spec: 게임 중 설정 (game-settings)

Status: ready-for-agent

> 근거: 2026-11-23 Grilling Q6~Q8(`grilling.md`). 기준: title-menu 54(설정 창·`GameSettings`), formal-ui 29(`PausePanel`), sound-feel 88(`MusicPlayer`), gdd-changes #77.

## 변경·유지·확인
**변경:** 게임 화면 왼쪽 위 톱니 버튼 + 일시정지 메뉴 "설정" 줄 → 타이틀과 같은 설정 창. 설정 코드는 공통 `SettingsPanel`로.

**유지:** 설정 값·저장(`GameSettings`), 타이틀 설정 창 모양·조작, 일시정지 동안 게임 멈춤.

**확인 – 작동:**
- C1 톱니 클릭 → 일시정지 + 설정 창 / Esc 일시정지 → "설정" → 설정 창 / Esc·닫기 → 일시정지 메뉴(톱니로 왔으면 게임으로)
- C2 값 바꾸면 바로: 음악(MusicPlayer)·효과음(AudioManager)·흔들림(UIManager)·전체화면, 저장되어 타이틀에서도 같음
- C3 타이틀 설정 창 그대로(퇴행)
- C4 시작 스토리·결과 화면·광대 차례 중 톱니 동작(아래 제안)
- C5 퇴행 T65~T68·T143, Console 오류 0

## Problem Statement
게임 중에 소리·흔들림을 바꾸려면 타이틀로 나가야 한다.

## Solution
게임 안에서 언제든 설정 창을 연다.

## User Stories
1. 플레이어로서, 게임 중에 음악 크기를 바로 바꾸고 싶다.
2. 멀미에 약한 플레이어로서, 게임 중 흔들림을 끄고 싶다.

## Implementation Decisions
- 코드(AI 작성, 4장 예외): 새 `SettingsPanel.cs`(설정 줄·값 바꾸기·저장·바로 반영), `TitleMenu.cs`(설정 부분을 SettingsPanel로), `PausePanel.cs`(설정 줄·톱니), `AudioManager`·`UIManager`(바뀐 값 다시 읽기). 그림: 톱니 아이콘 `tools/`. Scene: MCP.

### AI 제안 → 사용자 채택 (2026-11-23)
1. **톱니 자리:** 왼쪽 위(지금 좌표 약 (4,−4), 12×12). 오른쪽 위는 라운드·턴 글자라 피함.
2. **톱니가 동작하는 때:** 일시정지가 되는 때와 같음(결과 화면에선 안 보임, 시작 스토리 중엔 숨김 — 다른 UI와 같이). 광대 차례에도 됨(일시정지처럼 멈춤).
3. **창 모양:** 타이틀 설정 창 그림·글자 그대로 복사해 GameScene 일시정지 창 위에(`HUD/ShakeRoot/PauseScreen` 안).
4. **바로 반영:** `GameSettings`에 "바뀜" 알림(event)을 하나 두고 MusicPlayer·AudioManager·UIManager가 듣게 함 → 어디서 바꿔도 같은 길.
5. **일시정지 메뉴 키:** ↑/↓로 계속·설정·타이틀 이동(지금은 마우스만 강조) — 타이틀 메뉴와 같은 조작.
6. **Ticket:** 91 하나.
