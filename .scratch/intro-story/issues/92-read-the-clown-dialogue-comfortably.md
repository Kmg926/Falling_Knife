# 92: 시작 스토리 대사를 편하게 읽는다

**What to build:** 시작 스토리 대화창을 다른 게임 사례에 맞춰 다듬는다(사용자 "뭔가 미흡해 → 서치해서 적용", 제안 6가지 전부 채택 2026-11-23).
1. 이름표 탭: '광대'를 대화창 윗선에 걸친 작은 탭(대화창과 같은 테두리 그림), 가운데
2. 문장부호 멈춤: `.` `,` `?` `!` 뒤 0.2초, `…`(또는 `...`) 뒤 0.4초, 말줄임표 점 사이 0.12초
3. ▼ 까딱임: 줄이 다 나오면 1칸 위아래(0.5초 주기)
4. 광대 목소리: 글자 소리를 낮은 높낮이(0.75) + 조금 무작위(±0.05). 다른 효과음엔 영향 없게 따로 재생기
5. 대화창 열림: 처음 나타날 때 0.15초 동안 세로로 펼쳐짐
6. 여백: 안쪽 좌우 여백 8 → 10, 두 줄 사이 간격 조금 늘림

**Blocked by:** 없음

**Status:** done

- [x] 문장부호 멈춤 중에도 누르면 줄 전체가 바로 보임
- [x] ▼ 까딱임·대화창 펼침·이름표 탭 캡처
- [x] 다른 효과음 높낮이 그대로(따로 재생기)
- [x] Console 오류 0, Space 길게 건너뛰기 그대로

근거 사례: 대화창 4요소(이름·초상·본문·넘김 표시), 이름은 상자와 이어진 탭, 문장부호 뒤 150~300ms 멈춤, 캐릭터별 글자 소리(Undertale), 고대비·읽기 쉬움(Game Accessibility Guidelines).

## 시연 순서
1. 타이틀 `시작` → 대화창이 세로로 펼쳐지며 나옴, 위 가운데 '광대' 탭
2. "……또 한 명 왔군." → 점 사이 머뭇거림, 낮은 글자 소리
3. 줄이 끝나면 ▼가 까딱임 → Space 짧게 → 다음 줄

## Comments
- 2026-11-23 AI 구현(사용자 "전부 채택"): `IntroStory.cs`·`AudioManager.cs`(Scripts 직접), Scene(MCP) `NameTab`·`Name`·`Line`·목소리 재생기. 확인 결과와 줄별 설명은 `../draft/92/README.md`.
  - **남은 것(사용자):** 실제로 듣고 목소리 높낮이·멈춤 길이 판단(Inspector에서 조절).

출처:
- https://www.abratabia.com/game-ui-design/dialogue-ui.php
- https://gameaccessibilityguidelines.com/?p=1219
- https://jorge-d-robles.github.io/gemini-fantasy/11-dialogue-system.html
- https://middlebluedev.itch.io/distance/devlog/693361/dev-log-003-dialogue
- https://kitsnack.itch.io/kitsnack-textbox
- https://rhenaudthelukark.github.io/CreateYourFrisk/pages/api-text.html
