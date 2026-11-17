# clown-idle Ticket 46 초안: 광대가 숨쉬고, 깜빡이고, 가끔 나를 본다

> AI가 Scripts에 직접 쓴다(clown-idle Grilling Q7 "코드도 AI가 작성", CLAUDE.md 4장 예외). 면담 대비로 읽어 둔다.
> 근거: `../../spec.md` 제안 1~4·7·9, Grilling Q1·Q2·Q4~Q6. 바꾸기 전 코드는 `ClownView.before.cs`.

## 바꾼 것
| 파일 | 할 일 |
|---|---|
| `ClownView.cs` | 그림을 직접 고르던 코드를 없애고 **Animator 파라미터만** 넣음(`IsThinking`·`IsLit`). 깜빡임(3~6초, 15% 두 번 연속)·힐끗(15~25초) 타이머로 trigger(`Blink`·`Glance`) |
| 그림 (`tools/make_art_clown_hd.py idle`) | 숨쉬기 둘째 장 `clown-idle-b`·`clown-thinking-b`·`clown-dim-b`(턱 아래 y 136부터 1도트 내림), 눈 덮개 `eyes-blink`·`eyes-glance`(+ `-dim`) |
| Animator (AI가 MCP로) | `Animations/ClownAnimator.controller` + 클립 7개(아래 표) |
| Scene (AI가 MCP로) | `Clown`에 Animator(Update Normal, Always Animate), 자식 `Clown/Eyes` SpriteRenderer(정렬 1, 몸 0 위), ClownView 칸 연결 |

## Animator 구조
| 레이어 | 상태 | 들어가는 조건(Any State에서) | 클립 |
|---|---|---|---|
| Body | Thinking | `IsThinking` | `Clown_Thinking`: a → b(0.6초) → a(1.8초) 반복, 한 바퀴 약 2.4초 |
| Body | Idle | `!IsThinking && IsLit` | `Clown_Idle` (같은 숨쉬기) |
| Body | Dim | `!IsThinking && !IsLit` | `Clown_Dim` (같은 숨쉬기) |
| Eyes(덮어쓰기) | Open | 기본 | `Eyes_Open`: 눈 그림 없음(몸 그림의 눈이 보임) |
| Eyes | Blink / BlinkDim | `Blink` trigger, `IsLit`으로 밝은·어두운 | 0.14초 감은 눈 → 끝나면 Open |
| Eyes | Glance / GlanceDim | `Glance` trigger, `IsLit` | 0.62초 눈동자 아래(나를 봄) → Open |

## 줄별 핵심 (`ClownView.cs`)
| 부분 | 왜 |
|---|---|
| `Refresh`: `SetBool(IsThinking)`, `SetBool(IsLit, 광대 차례 \|\| 라운드 끝)` | 어떤 그림을 쓸지는 Animator가 정함. 같은 그림을 코드와 Animator 두 곳에서 바꾸면 서로 덮어써서(Q6) 코드는 "지금 상태"만 알려 줌 |
| `Awake`에서 타이머를 `Random.Range`로 | 처음부터 규칙적이면 기계처럼 보임 |
| `Update`: `blinkTimer -= Time.deltaTime` → 0 이하면 `SetTrigger(Blink)` | Animator만으로는 "3~6초 중 아무 때나"가 어려워서 코드가 신호를 보냄. `deltaTime`이라 일시정지(timeScale 0)면 같이 멈춤 |
| `twice ? doubleBlinkGap : Random.Range(...)` | 15%로 0.25초 뒤 한 번 더 → 사람처럼 가끔 두 번 깜빡 |
| 힐끗: 생각 중·공개 중이면 `glanceTimer = 1f` | 생각 중엔 눈동자가 위라 힐끗이 어색하고, 공개 중엔 캔들에 눈이 가야 해서 1초 뒤로 미룸 |
| Animator Update 방식 Normal | `timeScale` 0이면 숨쉬기도 멈춤(제안 7) |
| 맞을 때 흔들림은 그대로(`UIManager`가 위치만 움직임) | Animator는 그림만 바꾸고 위치를 건드리지 않아서 서로 안 부딪힘 |

## 테스트 (Ticket 46)
| # | 방법 | 기대 | 결과 |
|---|---|---|---|
| C2 | 내 차례 / 광대 생각 중 / 광대 공개 중 몸 그림 이름 | dim / thinking / idle | ✅ |
| C3 | 12초 동안 몸 그림 바뀐 시각 | a↔b 약 1.2초마다, 한 바퀴 2.4초 | ✅ (0.38 b · 1.58 a · 2.79 b …) |
| C4 | 12초 동안 눈 그림 | 깜빡임 3번(2.9·6.8·10.1초), 0.14초, 어두운 차례라 `-dim` | ✅ |
| C4 | 힐끗 타이머를 0으로 / 생각 중에 0으로 | `eyes-glance-dim` 0.62초 / 생각 중 힐끗 없음 | ✅ |
| C5 | 광대 맞음 흔들림 → 1초 뒤 위치, 일시정지 3초 | 중간 2.00 이동 → 차이 0.000 / 진행 28.710 → 28.711, 그림 그대로 | ✅ |
| C6 | 결과 화면 광대(Image) 3초 / TitleScene | 그림 그대로·Animator 없음 / TitleScene에 Animator 0개 | ✅ |
| C7 | 퇴행 T65~T68·T143, Console 오류 | 같음, 0 | ✅ |

참고: 클립 길이가 설계(2.4·0.12·0.6초)보다 한 프레임씩 김(2.42·0.14·0.62초) — 마지막 그림이 한 프레임 보이게 하려는 키 배치라 그대로 둠.
