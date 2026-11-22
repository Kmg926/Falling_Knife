# draft 86: Space를 꾹 눌러 시작 스토리를 건너뛴다

> 2026-11-22. 코드는 AI가 Scripts에 직접(clown-phone Grilling Q7, CLAUDE.md 4장 예외). 키캡 그림은 `tools/make_art.py skipkey`. `IntroStory.before.cs` = 바꾸기 전.
> 근거: clown-phone Grilling Q4~Q6, Spec 제안 4·5. 조사 출처는 `../../../clown-phone/grilling.md`.

## 그림 (`tools/make_art.py skipkey`)
- `skip-key.png`(27×11): 키캡 윗면 9줄 + 앞 옆면 2줄(눌리는 키 두께), 테두리 `gray0`·안 `bg0`·글자 "SPACE"(3×5, `cream`), 모서리 1칸 깎음.
- `skip-key-fill.png`: 같은 모양을 금색으로(윗면 `gold1`·테두리 `gold2`·글자 `bg00`·옆면 `goldD`) — 게임에서 위에 겹쳐 왼쪽부터 채움.
- 미리보기 `skip-key-preview-x8.png`(평소 / 반 / 다 참). 32색 OK.

## 줄별 설명 — IntroStory.cs (면담 대비)
- 새 칸: `skipHint`(CanvasGroup — 묶음 전체 투명도), `skipFill`(Image Type = **Filled**, Horizontal·Left: `fillAmount` 0~1만큼 왼쪽부터 보임), `holdSeconds 1`·`tapSeconds 0.25`·`drainSeconds 0.3`·`idleAlpha 0.4`·`flashSeconds 0.1`.
- `Update`: Enter·클릭은 **누르는 순간** `advance`(전과 같음). Space는 `UpdateHold`가 맡음. `spaceTap`은 매 프레임 처음에 끄고, 이번 프레임에 짧게 뗐을 때만 켬 → 같은 프레임 뒤에 도는 코루틴(`Choose`)이 읽음.
- `UpdateHold`
  - 누르는 동안: `holdTime`을 늘리고 `progress = holdTime / holdSeconds`(비워지는 중에 다시 누르면 `Max`로 그 자리부터). 1이 되면 `flashLeft = 0.1` → 금색이 0.05초씩 꺼졌다 켜진 뒤 `skip = true`(= Esc와 같은 `Finish()`).
  - 뗐을 때: `holdTime`이 0.25초 안이면 짧게 누름 → `spaceTap`·`advance`. 아니면 다음 줄로 안 넘어가고 `progress`만 `dt / 0.3`씩 줄어 0.3초 안에 비워짐.
  - `Time.unscaledDeltaTime`: 시간 배율과 상관없는 실제 시간.
- `ShowSkip`: 채우는(또는 비우는) 중이면 묶음을 밝게(1), 아니면 흐리게(0.4).
- `Choose`: Space 결정 조건을 `wasPressedThisFrame` → `spaceTap`으로(길게 누르면 결정 대신 건너뛰기).
- `simulateHold`(`[NonSerialized]` 공개 칸): 테스트용 — 켜 두면 Space를 누른 것으로 침. Unity 창에 포커스가 없으면 Input System이 키보드 상태를 매 프레임 초기화해서(`ResetAndDisableNonBackgroundDevices`) MCP로 누른 키가 다음 프레임에 풀림. 78의 `Confirm()`과 같은 시험용 입구, 저장되지 않음.
- 첫 시안에서 "다 차면 번쩍"을 색을 1보다 밝게 곱해서 하려 했는데, UI 색은 1을 넘지 못해 효과가 없음 → 금색을 깜빡이는 방식으로 바꿈.

## Scene (MCP)
- `/HUD/Intro/Root/SkipHint`(CanvasGroup, 처음 0.4, 클릭 안 받음) → `Key`·`Fill`(282, −120 · 27×11 = 그림 2배) · `Label` "길게 눌러 건너뛰기"(크기 8, `cream0`, 오른쪽 맞춤, 200~279). 대사 상자 오른쪽 위 바깥.
- 시안 고침 1번: 키캡을 1배(13.5×5.5)로 놓으니 글자보다 너무 작음(`hint-idle-small-key.png`) → 2배.

## 확인 (2026-11-22, MCP — 테스트 동안만 tap 3초·hold 6초·drain 2초로 늘려 eval 지연(약 1초)을 흡수)
| | 결과 |
|---|---|
| ① 짧게 | 누른 동안 줄 안 넘어감 → 뗄 때 다음 줄, 채움 0으로 |
| ② 차다 뗌 | 3.9초(65%)에서 뗌 → 줄 그대로, 채움 0으로 비움, 투명도 0.4로(`hint-holding.png` = 채우는 중 밝아진 모습) |
| ③ 끝까지 | 100% → 시작 스토리 끝·게임 시작 |
| ④ 선택 중 | "아니오" 고르고 짧게 → 결정 → "상관없어…" |
| Enter·클릭·Esc | 코드 경로 그대로(누를 때 `advance` / `skip`) |
| 퇴행 | T65~T68·T143 통과(T143 0.07 / 0.35), Console 오류 0, Scene 변경 없음 |

**남은 것(사용자):** 실제 Space로 1초·짧게 누르기 느낌, 키캡·글자 크기.
