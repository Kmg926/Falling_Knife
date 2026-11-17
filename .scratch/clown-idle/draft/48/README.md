# clown-idle Ticket 48 초안: 광대가 눈에 보이게 숨쉬고, 맞으면 광대만 빨개진다

> AI가 Scripts에 직접 쓴다(clown-idle과 같은 방식, CLAUDE.md 4장 예외). 면담 대비로 읽어 둔다.
> 근거: 사용자 "idle에 숨 쉬는 것 같은 애니메이션 추가해줘. 광대가 피격될 때는 전체 화면이 아닌 광대에게만 흔들림, 빨간색 효과" → 제안 "이대로 진행", 시안 "이대로 넣기". 바꾸기 전 코드는 `UIManager.before.cs`.

## 바꾼 것
| 파일 | 할 일 |
|---|---|
| `UIManager.cs` | `FlashDamage`: 화면 전체 빨강은 **내가 맞을 때만**. 광대가 맞으면 `ClownTintRoutine`으로 광대 몸·눈 그림만 빨강 ↔ 원래 2번(0.2초). 칸 3개(`clownTintTargets`·`clownHitColor`·`clownHitBlinks`) |
| 그림 (`tools/make_art_clown_hd.py breath`) | 숨 c장 3장 `clown-idle-c`·`clown-thinking-c`·`clown-dim-c`(몸 2도트·머리 1도트 아래). b장은 46과 같음 |
| Animator 클립 (AI가 MCP로) | `Clown_Idle`·`Clown_Thinking`·`Clown_Dim`: a(0) → b(0.75) → c(1.5) → b(2.25) → a(3.0) 반복 + `Eyes` 위치 y: c장 동안만 −0.5(새 도트 1칸) |
| Scene (AI가 MCP로) | UIManager `clownTintTargets` = `Clown`·`Clown/Eyes` |

## 줄별 핵심
| 부분 | 왜 |
|---|---|
| `if (playerHit) FlashRoutine() else ClownTintRoutine()` | 광대가 맞았는데 화면 전체가 빨개지면 내가 맞은 것처럼 보임 → 맞은 쪽에만 반응 |
| `SpriteRenderer.color = clownHitColor` | 색은 그림 색에 **곱해짐**: 흰색 = 그림 그대로, 빨강 = 밝은 얼굴은 빨갛게·어두운 곳은 더 어둡게. 새 그림 없이 빨간 효과 |
| `half = flashSeconds / (clownHitBlinks * 2)` | 빨강·원래를 한 쌍으로 두 번 → 0.05초씩 4칸 = 0.2초(내가 맞을 때 깜빡임 시간과 같음) |
| 끝에 `SetClownTint(Color.white)` 한 번 더 | 중간에 다시 맞아 코루틴이 새로 시작돼도 마지막엔 반드시 원래 색 |
| Animator와 안 부딪힘 | Animator 클립은 그림(`m_Sprite`)과 눈 위치만 바꾸고 색은 안 건드림 |
| 눈 위치 곡선 Constant(계단) | c장에서 머리가 1도트 내려가니 눈 덮개(따로 있는 자식 그림)도 같이 내려야 깜빡임이 눈에 맞음. 부드럽게 움직이면 반 도트에 걸려 흐려지므로 계단 |

## 테스트 (Ticket 48)
| # | 방법 | 기대 | 결과 |
|---|---|---|---|
| C12 | 7초 동안 몸 그림·눈 위치 기록 | a → b → c → b, 0.75초씩·3초 한 바퀴, c일 때만 눈 −0.5 | ✅ (0.95 b · 1.70 c/−0.5 · 2.45 b · 3.20 a …) |
| C13 | 광대 맞음 0.5초 기록 | 화면 빨강 없음·화면 흔들림 없음, 광대만 흔들림, 색 R·W·R·W(0.05초 간격), 끝 흰색·제자리 | ✅ |
| C14 | 내가 맞음 | 화면 빨강·화면 흔들림, 광대 색 변화 없음 | ✅ |
| C15 | 퇴행 T65~T68·T143, Console 오류 | 같음, 0 | ✅ |
