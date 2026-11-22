# draft 84: 탁자 위 깨진 빨간 폰이 광대 것임을 알아본다

> 2026-11-22. 그림 만드는 코드(`tools/make_art_phone.py`)는 AI 작성 규칙, 게임 코드 `PhoneGlitch.cs`(새 파일)는 AI가 Scripts에 직접(clown-phone Grilling Q7, CLAUDE.md 4장 예외). 판정 코드 변경 없음.

## 그림 (`tools/make_art_phone.py table`)
| 바뀐 곳 | 내용 |
|---|---|
| `TABLE_PHONES["clown"]` | `style="clown"` 추가(자리·크기·돌림은 그대로) |
| `clown_damage(cv)` (새) | 위에서 본 원본에 깨진 흔적 — 눕히기 전에 그려서 원근과 같이 기울어짐 |
| `top_view_src(…, clown)` | 광대면 `clown_damage`, 깨진 모서리는 mask에서 뺌. `"noise"` 라벨 = 화면 가로줄 노이즈(지지직 순간) |
| `phone_on_table_rot` | 바깥 화면에서 온 칸(`scr`)을 기억 → 광대면 그 밖의 남색 계열을 빨강 계열로(`navy0→red0`, `navy→red1`, `navy2→red2`) — 윗면 음영·옆면까지 같은 단계 |
| `make_table` | 광대 폰은 `-noise` 그림도 |

- 테이프: 왼쪽 위(먼 쪽) 모서리를 대각선으로 가로지르는 띠(`cream0`, 가장자리 `gray1`, 윤기 `cream2`). LED는 피함.
- 깨진 모서리: 왼쪽 아래(가까운 쪽), 계단 모양 가장자리 + 깨진 면 `bg00`. 그래서 그림 높이 50 → 48.
- 화면 금: 화면 바탕(`bg00`·`navy0`) 위에만 → 숫자(`green2`)는 안 가림. 큰 금 `cream0`, 갈래 `gray1`, 충격점 `cream2`.
- 시안 고침 1번(`try1-tape-chip-weak.png`): ① 테이프가 왼쪽 끝 세로 띠라 눕히면 밝은 모서리처럼 보임 → 모서리를 대각선으로 가로지르게 ② 깨진 모서리가 원래 둥근 모서리 바깥이라 아무것도 안 떨어짐 → 크게 떼어 냄. 결과 `clown-phone-frames.png`(내 폰·광대 2·4·+2·노이즈·테두리).
- 32색 검사 OK. 내 폰 그림은 바뀌지 않음(같은 파일).

## 줄별 설명 — PhoneGlitch.cs (면담 대비)
- 폰 그림 자체를 바꾸지 않고, **위에 같은 크기 그림(overlay)을 덮었다 치우는** 방식. 개수 숫자 그림(ClownPhoneView)·"+N" 깜빡임(PhoneBuzz)이 폰 그림을 바꿔도 서로 부딪히지 않는다.
- `Loop()` 코루틴: `WaitForSeconds(Random.Range(2, 4))` → 받는 진동 중(`buzz.Busy`)이면 이번엔 건너뜀 → 0.1~0.2초 동안 0.05초마다 노이즈 ↔ 빈 화면을 바꾸고 덮개를 좌우 0.5(= 새 도트 1칸) 떨게 함 → 덮개 끄고 제자리.
  - 덮개가 1도트 비켜 있어 아래 원래 폰이 살짝 보임 → 화면이 튀는 느낌.
  - `WaitForSeconds`는 일시정지(timeScale 0) 중엔 멈춤.
- `OnEnable`에서 시작, `OnDisable`에서 덮개 끄고 제자리(씬 전환·시작 스토리 숨김과도 안전).
- 소리 없음(Grilling Q2).

## Scene (MCP)
- `ClownPhoneClosed` 크기 37×24.5 → 37×24, 테두리 39×27 → 39×26.
- 자식 `Glitch`(Image, 부모 크기 그대로, 처음 꺼짐, 클릭 안 받음) — `Outline` 다음 순서.
- `ClownPhoneClosed`에 `PhoneGlitch`(buzz = 같은 오브젝트의 `PhoneBuzz`, noise·blank 그림).
- `phone-table-clown-noise.png` 가져오기 설정은 `-blank`와 같게.

## 확인 (2026-11-22, MCP)
| | 결과 |
|---|---|
| C1 | 탁자 위 빨간 깨진 폰 vs 남색 내 폰(`table.png`), 숫자·"+2" 읽힘, 32색 |
| C2 | 간격을 잠깐 줄이고 시간 0.05배로 잡음: 덮개 켜짐·노이즈 ↔ 빈 화면·좌우 −0.5/+0.5(`glitch-in-game.png`: 평소 / 노이즈 / 꺼짐). 진동 중 건너뜀은 코드로 확인 |
| C6 | Console 오류 0. 판정 코드 안 바뀜(퇴행은 86 뒤에 한 번에) |

**남은 것(사용자):** 실제 속도에서 지지직이 거슬리지 않는지, 광대 것으로 바로 알아보는지.
