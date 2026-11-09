# Ticket 05 초안: 한쪽 잔고가 0이 되면 승패가 나오고 R로 다시 시작한다

> **초안**이다(Assets 밖). 원래 규칙은 직접 타이핑(CLAUDE.md 4장). 04에서 **바뀐 줄만** 설명한다.
> 마일스톤 ①의 마지막 Ticket.
> **2026-11-02 사용자 지시로 AI가 세 파일을 그대로 반영하고 `Restart Action` 칸을 MCP로 연결했다(예외). 이유: Parsec 원격 접속 네트워크 지연(01~04와 같음).** 아래 3장의 1·2번은 건너뛴다.

## 1. 파일

| 파일 | 04 대비 |
|---|---|
| `GameManager.cs` | `IsRoundOver`, `PlayerWon`, `Restart()`, 입력 막기 |
| `UIManager.cs` | 승패 줄 |
| `PlayerInputHandler.cs` | `restartAction` 칸·연결 추가 |

`Participant`·`CandleDeck`·`LeverageCounter`·Input Actions 에셋은 그대로다(R 키는 02에서 미리 만들어 둠).

## 2. 바뀐 줄마다 왜

### GameManager.cs
| 줄 | 왜 |
|---|---|
| `public bool IsRoundOver => Player.IsBankrupt \|\| Clown.IsBankrupt;` | 04의 `SomeoneBankrupt`(private)를 **public으로 이름 바꿈**. UI·입력 막기·재장전 판단이 모두 같은 기준을 쓴다 |
| `public bool PlayerWon => Clown.IsBankrupt;` | 광대 파산 = 승리, 아니면 패배. 한 번 공개에 한 명만 맞으므로 둘이 동시에 파산할 일은 없다 |
| `Select()`·`Execute()`의 `\|\| IsRoundOver` | 승패 화면에서는 A/D·Space를 무시(수락 기준) |
| `Execute()`의 빈 덱 줄 → 조용히 `return` | 04까지는 안내 로그가 있었지만, 이제 빈 덱 = 파산 = `IsRoundOver`에서 먼저 막힌다. 혹시 모를 에러만 막는 안전장치로 남김 |
| `Restart()`의 `if (IsRevealing \|\| !IsRoundOver) return;` | R은 **라운드가 끝난 화면에서만**(Q22). 플레이 중·공개 중 R은 무시 |
| `Restart()` → `StartRound()` | 01부터 "재시작도 이 함수를 다시 부른다"로 만들어 둔 그 함수. 참가자·레버리지를 `new`로 새로 만들고 덱도 다시 섞으니 깨끗한 처음 상태 |
| 재장전 조건 `!IsRoundOver && deck.IsEmpty` | 이름만 바뀜. 동작은 04와 같다 |

### UIManager.cs
| 줄 | 왜 |
|---|---|
| `if (game.IsRoundOver)` | 라운드가 끝났을 때만 승패 줄을 붙인다 |
| `message += game.PlayerWon ? "\n승리! - R로 재시작" : "\n패배... - R로 재시작";` | 마지막 결과 줄(누가 얼마 잃었는지) **아래에** 승패를 붙여서, 왜 끝났는지 같이 보인다(Q18 ⑦). `-`는 키보드 하이픈 |

### PlayerInputHandler.cs
| 줄 | 왜 |
|---|---|
| `InputActionReference restartAction` | 새 칸. `FallingKnife` 에셋의 `Game/Restart`(R)를 끌어다 놓는다 |
| `OnEnable/OnDisable`에 `+=`/`-=`, `Enable/Disable` 한 줄씩 | Select·Execute와 같은 방식 |
| `OnRestart` → `game.Restart()` | 판정(언제 받을지)은 GameManager가 한다 |

## 3. Unity Editor에서 할 순서

1. 세 파일 교체 → Console 에러 0
2. **`GameManager` 물체 → `Player Input Handler` → `Restart Action` 칸 ← Project 창 `FallingKnife` ▶ `Game/Restart`**
   - ⚠ 비워 두면 Play 시작하자마자 NullReference 에러(`OnEnable`에서 빈 칸을 읽음)
3. 승패 줄 때문에 7줄 이상이 될 수 있다. `StatusText`가 잘리면 Height를 늘린다
4. Ctrl+S → Play → Game 창 클릭

## 4. 수동 테스트 (Ticket 05)

> 덱 기본(하락2/상승3). 정지 상태에서 고정 순서 설정. 끝나면 고정 끄기 → Ctrl+S.

| # | 고정 순서 | 입력 | 기대 화면 |
|---|---|---|---|
| T30 | `U U D U D` | Space × 3 (모두 보유) | 3번째: `보유 → 하락! 플레이어 -3`, 플레이어 0, 그 아래 `패배... - R로 재시작` |
| T31 | 이어서 | A, D, Space | 화면 변화 없음, "공개 중..." 안 뜸 |
| T32 | 이어서 | R | 처음 상태: 차례 플레이어, `[보유]`, 잔고 2/2, 레버리지 0, 남은 2/3, 결과 줄 빈칸. Console에 덱 순서 다시 찍힘 |
| T33 | `D U U D U` | D·Space → D·Space → D·Space → Space | 4번째(광대 보유+하락): `광대 -1`, 광대 0, `승리! - R로 재시작` |
| T34 | `U U D U D` | (D → Space) × 5 | 5번째(마지막 캔들)에서 광대 0 → `승리!`, **재장전 줄 없음**(04와 같은 상황, 이제 승패가 먼저 보임) |
| T35 | 기본 | 플레이 중 R / Space 직후 "공개 중..." 동안 R | 둘 다 아무 일 없음 |
| T36 | 기본 | 승패 → R → 다시 한 판 → 승패 → R | 여러 번 반복해도 오류 없음. 가능하면 한 판에서 재장전 2번 이상(04의 T29) |

## 5. 반영 후
반영하면 AI에게 리뷰를 요청한다. 05가 끝나면 **마일스톤 ①(텍스트 UI로 1라운드 완주) 완료**.
