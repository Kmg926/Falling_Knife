# monitor-trade Ticket 44 초안: 아이템을 모니터 안에서 쓴다 (+ 맞은 사람별 흔들림)

> AI가 Scripts에 직접 쓴다(monitor-trade Grilling Q8 "코드는 AI가 작성", CLAUDE.md 4장 예외). 면담 대비로 읽어 둔다.
> 근거: `../../spec.md` (C3·C7·C8, 제안 3·6), 사용자 지시 "이대로 구현, 그리고 '나'가 피격시 전체 화면 흔들림, 광대가 피격시 광대 스프라이트 흔들림".

## 바꾼 것
| 파일 | 할 일 |
|---|---|
| `GameManager.cs` | `SelectItem`이 모니터를 열면서 상세로(`IsMonitorOpen = true`), 피해 때 `ui.FlashDamage(맞은 사람이 나인가)` |
| `MonitorZoomPanel.cs` | 칸 `mainGroup` — 상세가 열려 있으면 첫 화면(차트 칸·매수·매도·아이템 칸·키 안내) 숨김 |
| `TableItemsView.cs` | **새 파일** — 테이블 위 내 아이템 작은 아이콘 4칸, 클릭 = `SelectItem(칸)` |
| `UIManager.cs` | 칸 `tableItems`·`clownShakeTargets`, `FlashDamage(bool playerHit)`: 나면 화면 전체(`shakeTargets`), 광대면 광대 그림만(`clownShakeTargets`), 흔드는 중 대상이 바뀌면 앞 대상을 먼저 제자리로 |
| 그림 (`make_art_trade.py items44`) | 상세 페이지 바탕(새 `zoom-page`), 큰 아이콘 칸·분류 표(옛 1배 `closeup-iconbox`·`closeup-tag`를 2배로 다시 그려 덮어씀) |
| Scene (AI가 MCP로, 세 번에 나눠 저장) | ① 확대 화면 첫 화면을 `Main` 묶음으로, 확인 창 내용을 확대 화면 안 자리로(확인 창의 어둡게 끔, 바탕 = 상세 페이지, `Esc 뒤로`) ② `TableItems` 4칸(x 222 + 24n, y 146), `Space` 안내 (222, 166) ③ HUD 아이들을 `ShakeRoot` 하나로 묶음(빨간 깜빡임 판만 밖), 흔들 대상: 나 = PlayArt·Clown·ShakeRoot / 광대 = Clown |

## 줄별 핵심
| 부분 | 왜 |
|---|---|
| `SelectItem`에서 `IsMonitorOpen = true` | 1~4 키·테이블 아이콘으로 고르면 "모니터를 열고 그 아이템을 들여다본다"(제안 6). 상세에서 Esc(`CancelItem`)는 확인만 닫으므로 **첫 화면으로 돌아가고**, 한 번 더 Esc면 모니터가 닫힘 |
| `mainGroup.SetActive(!game.IsConfirming)` | 상세 페이지는 지금 확인 창(`ItemCloseupPanel`: 회전 그림·이름·분류·설명·사용/뒤로)을 그대로 쓰고 자리만 확대 화면 안으로 옮김(제안 3). 겹치지 않게 첫 화면을 숨김 |
| `int slot = i;` (TableItemsView) | 반복문 변수 `i`를 람다가 그대로 잡으면 클릭할 때의 `i`(4)를 쓰게 됨 → 칸마다 복사해 둠 |
| `button.interactable = has && game.CanOpenMonitor` | 광대 차례·공개 중·모니터가 열려 있을 때는 누를 수 없게 |
| `FlashDamage(bool playerHit)` | 사용자 지시: 내가 맞으면 **화면 전체**, 광대가 맞으면 **광대 그림만**. 맞은 사람은 판정 쪽(GameManager)이 아니까 인자로 알려 줌(화면은 판정하지 않음) |
| HUD를 `ShakeRoot`로 묶음 | 글자·패널을 하나씩 흔들면 턴 띠처럼 스스로 움직이는 조각과 부딪힘(끝날 때 제자리로 돌려놓다 띠를 튀게 함). 묶음 하나만 움직이면 안쪽 조각들의 움직임은 그대로. 빨간 깜빡임 판은 화면 전체를 덮어서 밖에 둠 |
| 흔드는 중 대상 바꾸기 | 광대가 맞고 바로 내가 맞는 경우 등: 앞 대상을 제자리로 돌린 뒤 새 대상의 자리를 기억(제자리를 잘못 기억하지 않게) |

## 테스트 (Ticket 44)
| # | 방법 | 기대 | 결과 |
|---|---|---|---|
| — | 아이템 2개일 때 테이블 아이콘 | 2개 보임, 닫힘 글자 13개 겹침 0 | ✅ |
| C3 | 테이블 아이콘 2 클릭 | 모니터가 그 아이템 상세로 열림, 첫 화면 숨김, 상세 글자 6개 겹침 0 | ✅ |
| C3 | 상세에서 Esc | 첫 화면으로(모니터는 열린 채) | ✅ |
| C3 | 닫힌 상태에서 1번 키 → Space | 상세로 바로 열림 → 찌라시 사용(효과 그대로), 첫 화면으로 | ✅ |
| — | 내가 맞음 / 광대가 맞음 | 나: PlayArt·Clown·HUD 모두 최대 2 / 광대: Clown만 2, 나머지 0. 끝나면 모두 제자리 | ✅ |
| C8 | 퇴행 T65~T68·T143(열기 → 고르기 → 실행) | 같음 | ✅ |
