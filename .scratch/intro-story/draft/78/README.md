# draft 78: "준비됐나?"에 예·아니오로 대답한다

> 2026-11-22. 코드는 AI가 Scripts에 직접 작성(intro-story Grilling Q7, CLAUDE.md 4장 예외). `IntroStory.before.cs` = 77 끝난 상태.

## 바뀐 것 (IntroStory.cs만)
- 대사 4줄 뒤 `Type(question)` → `Choose()` → `Say(replies[Choice])` → `Finish()`.
- 예전 `Say`를 둘로 나눔: `Type`(한 글자씩 보이기) + `Say`(Type 뒤 ▼ + 넘김 기다림). 질문 줄은 넘김을 기다리지 않고 바로 선택지가 나와야 해서.

## 줄별 설명 (면담 대비)
- `choicesRoot`·`choiceTexts`·`choiceLabels`·`replies` — Inspector에서 고칠 수 있는 칸. 0 = 예, 1 = 아니오. 대답만 다르고 결과(시작)는 같음.
- `Choose()`
  - `Choice = 0` — 처음은 `예`. `nextMark` 끄고 선택지 켬.
  - `yield return null` — 질문 줄을 넘긴 그 누름이 같은 프레임에 "결정"으로도 읽히지 않게 한 프레임 쉼.
  - 매 프레임: 마우스가 어느 글자 위인지 `RectTransformUtility.RectangleContainsScreenPoint`(사각형 안에 점이 있나)로 잼. **움직였을 때만** 그 칸을 고름(키보드로 고른 칸을 가만히 있는 마우스가 덮지 않게 — 타이틀 메뉴와 같음). 클릭 = 그 칸으로 결정.
  - ←/A/↑/W = 예, →/D/↓/S = 아니오(칸이 2개라 돌지 않고 끝에서 멈춤), Space·Enter = 지금 칸으로 결정.
  - Esc는 여기서 따로 안 봄: `Update`가 언제나 Esc → `Finish()`(= "예"처럼 바로 시작, Q9).
- `Confirm(int)` — 그 칸으로 결정. 키 입력을 보낼 수 없는 MCP 테스트에서 부름(타이틀의 `Choose()`처럼 바깥에서 부를 수 있는 입구).
- `Pick(int)` — 칸을 바꾸고 바뀔 때만 작은 클릭음.
- `RefreshChoices()` — 고른 칸 = 금색 + "▶ ", 음수 여백으로 ▶를 글자 왼쪽 바깥에(글자 자리는 안 움직임, title-menu 76과 같은 방법). "▶ " 폭은 끝 빈칸이 빠지지 않게 "▶ 가" − "가"로 잼.

## Scene (MCP)
- `/HUD/Intro/Root/DialogBox/Choices`(처음 꺼짐) → `Yes` "예"(214,−29) · `No` "아니오"(252,−29), 크기 11, 줄바꿈 없음. `IntroStory`의 `choicesRoot`·`choiceTexts` 연결.
- 시안 고침 1번: 처음 `예` x 222 → 아니오를 고르면 ▶가 "예"에 붙어 "예 ▶ 아니오"처럼 보임 → 214로(사이 여백 3 → 11).

## 확인 (2026-11-22, MCP)
| | 결과 |
|---|---|
| C3 | 4줄 뒤 "준비됐나?" + `▶ 예`(금색)·아니오(`choice-yes.png`, 고치기 전 자리) → 아니오로 옮김(`choice-no.png`) → 결정 → "상관없어. 칼은 이미 떨어지고 있거든." → 넘김 → 덱 공개 연출 / 다시: 예 → "좋아. 시작하자."(`reply-yes.png`, 7초 넘게 넘김 기다림) → 넘김 → 덱 공개 연출 |
| C4 | 선택 중 Esc = `Update`의 넘기기(77에서 확인한 같은 길), 코드로 확인 |
| C7 | Console 오류 0. 퇴행 T65~T68·T143은 아직 안 돌림 |

- 실제 마우스가 Game 창 위를 지나가면 칸이 바뀜(테스트 중 한 번 아니오가 저절로 골라짐) → 의도한 동작. 첫 시도에서 대답 줄이 저절로 넘어간 것도 실제 입력으로 보임(두 번째 시도에선 7초 넘게 그대로).
- **남은 것(사용자):** 실제 키(←/→·A/D·↑/↓·W/S·Space·Enter·Esc)와 마우스 올리기·클릭.
