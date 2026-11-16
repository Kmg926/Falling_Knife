# Ticket 26 초안: 잔고 동전·라운드·턴을 보고, 보유·매도·아이템 슬롯을 클릭으로도 쓴다

> **초안**이다(Assets 밖). 규칙상 **직접 타이핑**(CLAUDE.md 4장). 그림·Scene 배치는 AI가 MCP로 이미 함(값은 아직 고정 — 이 코드가 들어와야 움직임).
> 근거: `../../spec.md` (C3·C4·C7·C8·C9·C12·C15, 제안 4·5·6·8), Grilling Q6·Q8·Q10, gdd-changes #34·#41·#45.

## 0. 바꿀 것
| 파일 | 할 일 | 줄 수 |
|---|---|---|
| `GameManager.cs` | `CanPlayerAct` 한 줄(+주석) | +3 |
| `BalanceView.cs` | **새 파일** — 동전 10개·`7/10`·정지·위험 빨강 | 38 |
| `RoundTurnView.cs` | **새 파일** — `ROUND n/3`·`내 턴/광대 턴`·가운데 안내 | 27 |
| `ActionButtonsView.cs` | **새 파일** — 보유·매도 버튼 그림·회색·클릭 | 45 |
| `ItemSlotsView.cs` | **새 파일** — 슬롯 아이콘·선택 테두리·클릭, 광대 아이콘 | 48 |
| `UIManager.cs` | 조각 칸 5개 + 깜빡임 덮개 + F1 개발용 글자, Refresh에 다섯 줄 | +33 |

순서: **GameManager → 새 파일 4개 → UIManager**(맨 마지막). 저장 뒤 "반영함" → AI가 리뷰하고 Scene 칸을 연결한다. **연결 전에 Play하면** 새 칸이 비어 오류가 난다(정상).
새 파일은 Project 창 `Assets/_Project/Scripts`에서 오른쪽 클릭 → **Create → Scripting → MonoBehaviour Script** → 이름을 **바로** 클래스 이름으로(지난번 `MonoBehaviour.cs` 중간 오류 방지).

## 1. `GameManager.cs`
```csharp
    public bool CanPlayerAct => !IsRevealing && !IsRoundOver && !IsClownTurn && !IsConfirming;
```
| 왜 |
|---|
| `Select()` 첫 줄과 같은 조건(공개 중·라운드 끝·광대 차례·확인 중이면 안 됨)을 **이름 붙여 밖에서 읽게** 했다. 버튼이 밝게/회색으로 보일 때와 클릭을 받을 때 같은 조건을 쓴다(C7) |

## 2. `BalanceView.cs`
| 부분 | 왜 |
|---|---|
| `isPlayer` | 같은 스크립트를 플레이어용·광대용 두 번 붙인다. 체크 하나로 누구를 읽을지 정함 |
| `coins[i].enabled = i < who.StartingBalance` | 동전은 10칸 만들어 두고, 시작 잔고보다 많은 칸은 숨김(잔고 표를 바꿔도 맞게 — 테스트에서 2/3/4로 바꿈) |
| `i < who.Balance ? …` | 남은 잔고만큼 금색(위험하면 빨강), 나머지는 잃은 잔고(회색 점선) |
| `danger = isPlayer && …` | 긴장 색은 **내 잔고만**(Ticket 22와 같음). 그림을 바꾸는 방식이라 팔레트가 지켜진다(색 곱하기 안 함) |
| `Color32(0x77, …)` | 팔레트 hex를 그대로 숫자로. `Color32` → `Color` 자동 변환 |
| `" 정지"` | 거래 정지 표시(C4). 개발용 글자의 `(정지)`와 같은 정보 |

## 3. `RoundTurnView.cs`
| 부분 | 왜 |
|---|---|
| `mine` 삼항 | 내 턴 = 초록, 광대 턴 = 빨강(스케치 `내 턴` 초록) |
| `noticeText` | 라운드 클리어·끝 안내(C12). 파산·생존 안내는 **Ticket 28 결과 화면이 생기면 거기로** 옮긴다. R 안내도 그때 사라짐(#19) |

## 4. `ActionButtonsView.cs`
| 부분 | 왜 |
|---|---|
| `Awake`의 `AddListener(() => …)` | `() => OnClick(...)`은 "누르면 이 줄을 실행"이라는 작은 이름 없는 함수(람다). Inspector의 OnClick 칸을 쓰지 않고 **코드에서 한 번 등록** → Scene 연결이 단순해지고 면담에서 설명하기 쉬움 |
| `OnClick`: `Select` → `Execute` | 클릭 한 번 = 고르기 + 실행(제안 4). 키보드는 그대로(U3) |
| 첫 줄 `if (!game.CanPlayerAct) return;` | 안전장치. 확인 중에 `Execute`가 불리면 **아이템을 써 버리므로** 막는다(버튼도 회색이라 원래 안 눌리지만 두 겹) |
| `interactable` | 회색일 때 클릭 자체를 받지 않음(C7) |
| `SpriteFor` | 못 누름 → 회색 그림, 선택 → 금색 2도트, 아니면 남색 |
| `Refresh(GameManager game)` | 매개변수 이름이 칸 `game`과 같아 **함수 안에서는 매개변수가 우선**(같은 GameManager라 결과 같음) |

## 5. `ItemSlotsView.cs`
| 부분 | 왜 |
|---|---|
| `int slot = i;` | 람다는 변수를 **나중에** 읽는다. `i`를 그대로 쓰면 반복이 끝난 뒤의 값(4)을 모든 버튼이 읽는다 → 칸마다 복사본을 만든다(C# 면담 단골) |
| `game.SelectItem(slot)` | 클릭 = 그 번호 키(제안 5). 막는 조건은 `SelectItem` 안에 이미 있음 |
| `itemIcons[(int)items[i]]` | enum은 순서 번호(0~3)를 갖는다 → `ItemType` 순서대로 그림 4장을 넣어 두면 번호로 바로 찾음 |
| `clownIcons` | 광대 아이템도 보인다(#34, 제안 6) — 광대가 무엇을 쓸지 추측하는 재미 |

## 6. `UIManager.cs`
| 부분 | 왜 |
|---|---|
| 조각 칸 5개 | 25와 같은 방식: GameManager는 `ui.Refresh(this)`만, UIManager가 나눠 준다(U6) |
| `damageFlash` | 피해 깜빡임을 화면 전체로(Q10). 같은 0.2초 코루틴에서 켜고 끈다 → 시간은 그대로(U2) |
| `showDevText` + `Update`의 F1 | 개발용 글자는 기본 숨김, F1로 켜고 끔(C15, #41 임시). `Keyboard.current`는 새 Input System에서 키보드를 직접 읽는 방법 — 게임 입력이 아니라 개발 도구라 Input Actions에 넣지 않음 |
| `Keyboard.current != null` | 키보드가 없는 환경에서 오류 방지 |

## 7. Scene·입력 (반영 뒤 AI가 MCP로 — 그림·틀은 이미 배치됨)
1. 저장 안 된 변경 확인
2. `HUD/PlayerBalance`·`ClownBalance`에 BalanceView(동전 10개·숫자·그림 3장, 플레이어만 `Is Player` 켬)
3. `HUD/RoundTurn`에 RoundTurnView, `HUD/ActionButtons`에 ActionButtonsView, `HUD/ItemSlots`에 ItemSlotsView(아이콘 4장 순서: 차트 분석·찌라시·거래 정지·복권)
4. UIManager 칸 5개 + `HUD/DamageFlash` 연결 → 저장
5. **우클릭 = 취소:** `FallingKnife.inputactions`의 `Cancel`에 `<Mouse>/rightButton` 바인딩 추가 — 입력 파일이라 **사용자가 할지, AI가 MCP로 할지(예외 기록)** 반영 때 정함. 직접 하면: Project 창 `Input/FallingKnife.inputactions` 더블클릭 → `Game` 맵 → `Cancel` 옆 `+` → **Add Binding** → Path: **Mouse > Right Button** → **Save Asset**

## 8. 테스트 (Ticket 26)
| # | 방법 | 기대 |
|---|---|---|
| T156 | 시작 | 동전 10/10 두 줄, `ROUND 1/3`·`내 턴`(초록), 버튼 보유 선택, 슬롯·광대 아이콘 = 아이템 (C3·C4·C9) |
| T157 | 하락 피해 / 복권 회복 / 내 잔고 3 | 동전·숫자 즉시, 3 이하 빨강 동전·빨강 숫자, 4로 회복하면 금색 (C3) |
| T158 | 거래 정지 | 정지 걸린 쪽 숫자 옆 `정지` (C4) |
| T159 | 공개 중·광대 차례·확인 중 | 버튼 회색, `onClick.Invoke()` 해도 아무 일 없음 (C7) |
| T160 | 매도 버튼 클릭 | 매도 선택 + 공개 시작 (C8) |
| T161 | 슬롯 2 클릭 / 우클릭(또는 Esc) | 확인 상태·노란 테두리 / 닫힘 (C8·C9) |
| T162 | 아이템 사용 | 그 칸 아이콘 사라짐, 광대 사용도 반영 (C9) |
| T163 | 피해 | `DamageFlash` 0.2초 켜졌다 꺼짐 (Q10, U2) |
| T164 | 라운드 클리어 | 가운데 안내 → Space → 2라운드 (C12) |
| T165 | F1 | 개발용 글자 켜고 끔, 판정 그대로 (C15) |
| T166 | 퇴행 T65~T68·T143, Console 오류 0 | 같음 (C17) |
| T167 | 실제 마우스·키 | 사용자가 직접 확인 |

## 9. 반영 후
"반영함" → AI 리뷰 → Scene 연결 → T156~T166 → gdd-changes·Ticket 26 기록.
