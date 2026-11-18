# draft/63: 내 폰을 열어 아이템을 고르고 쓴다

코드는 AI가 Scripts에 직접 썼다(CLAUDE.md 4장 예외, item-phone Grilling Q6). 고치기 전 파일은 `*.before.cs`, 고친 뒤 파일은 같은 이름 `.cs`로 이 폴더에 있다.

## 한 줄 요약
- 아이템은 이제 **폰**에서만 다룬다. 테이블 위 접힌 폰(개수 + 열 수 있을 때 금색 테두리) → 클릭하면 확대된 폰(목록 2×2) → 칸·1~4·Space로 상세 → Space 사용 / Esc 뒤로.
- 모니터와 폰은 **동시에 열리지 않는다**. 모니터는 차트·매수·매도만 남아 차트 칸이 화면 폭 전체가 됐다.

## GameManager.cs (바뀐 곳)
| 바뀐 줄 | 왜 |
|---|---|
| `CanPlayerAct`에 `&& !IsPhoneOpen` | 폰이 열려 있으면 매수·매도를 못 고르고(A/D 무시), 모니터도 못 연다(`CanOpenMonitor`가 이걸 씀) |
| `IsPhoneOpen` 속성 | 폰이 열려 있는가. 값은 GameManager 안에서만 바꿈(`private set`) |
| `CanOpenPhone => CanPlayerAct && !IsMonitorOpen` | 모니터와 같은 조건(내 차례·공개 중 아님, Q12) + 모니터가 닫혀 있을 때 |
| `PhoneCursor` | 목록에서 Space를 누르면 열릴 칸. 상세에서 Esc로 돌아오면 그 칸이 금색으로 남는다 |
| `BeginPresentation`·`StartRound`·`PassTurn`에 `IsPhoneOpen = false` | 덱 공개 연출·새 라운드·차례 넘김 때 열린 폰을 닫는다(모니터와 같은 규칙) |
| `Select`에 `|| IsPhoneOpen` | 폰이 열려 있으면 A/D 무시(Spec 제안 7) |
| `Execute`의 `if (IsPhoneOpen) { SelectItem(PhoneCursor); return; }` | 폰 목록에서 Space = 금색 칸 상세. 상세에서 Space(=사용)는 바로 위 `IsConfirming`이 먼저 처리 |
| `SelectItem`: `PhoneCursor = slot; IsPhoneOpen = true; IsMonitorOpen = false;` | 1~4·칸 클릭 → 폰이 그 아이템 상세로 열림. 모니터가 열려 있었으면 닫음(둘은 동시에 안 열림). 옛 코드는 모니터를 열었다 |
| `OpenPhone()` | 접힌 폰 클릭 → 목록. 열 수 없으면 아무 일 없음 |
| `ClosePhone()` | 목록에서 닫기. 상세가 열려 있으면 닫지 않음(상세부터 닫아야 함) |
| `PhoneBack()` | 빨간 키·바깥 클릭: 상세면 목록으로(`CancelItem`), 목록이면 닫기 |
| `TogglePause`에 `|| IsPhoneOpen` | 폰이 열려 있을 때 Esc는 일시정지가 아니라 "닫기" |
| `UseConfirmedItem`: `if (used) IsPhoneOpen = false;` | 아이템을 실제로 쓰면 폰을 닫아 결과(모니터 결과 글, 차트 분석 캔들)가 보이게. 못 썼으면(거래 정지 불가) 목록에 남음 |

## PlayerInputHandler.cs
| 바뀐 줄 | 왜 |
|---|---|
| `OnCancel`에 `if (game.IsPhoneOpen) { game.ClosePhone(); return; }` | Esc·우클릭 순서: 상세 → 목록 → 폰 닫기 → (모니터 닫기) → 일시정지 |

## ItemSlotsView.cs
| 바뀐 줄 | 왜 |
|---|---|
| 금색 틀 기준 `ConfirmingSlot` → `PhoneCursor` | 칸이 폰 목록으로 옮겨 감. 목록에서 "Space를 누르면 열릴 칸"을 보여 줌 |
| 눌림 시작 조건에 `&& gameObject.activeInHierarchy` | 칸이 꺼진 폰 안에 있을 때 코루틴을 시작하면 오류("inactive") → 보일 때만 눌림 |
| `OnDisable()`에서 칸 자리 되돌리기 | 칸을 누르면 상세가 열리며 목록이 꺼짐 → 눌림 도중 멈춰 1도트 내려간 채 남지 않게 |

## PhoneView.cs (새 파일, 옛 TableItemsView 자리)
- 접힌 폰: `countText` = 내 아이템 수(광대 차례에도 보임), `outline`·`closedButton` = 열 수 있을 때만.
- 확대된 폰: `zoomRoot` 켜기/끄기, 상세면 `listGroup` 숨김, 제목 "아이템 N/4", 화면 아래 소프트키 글 "확인/닫기"(목록)·"사용/뒤로"(상세), 키패드 1~4 금색 = 그 칸에 아이템이 있을 때.
- 클릭은 모두 GameManager 함수로: 접힌 폰 = `OpenPhone`, 바깥·빨간 키 = `PhoneBack`, 초록 키 = `Execute`(Space와 같음), 키패드 1~4 = `SelectItem`.
- `PhoneView`는 항상 켜져 있는 접힌 폰 오브젝트에 붙였다(꺼진 오브젝트는 `Awake`가 안 돌아 클릭 연결이 안 됨).

## UIManager.cs
- `TableItemsView tableItems` → `PhoneView phoneView`, `Refresh`도 같이. `TableItemsView.cs`는 지움(테이블 아이콘이 폰으로 바뀜).

## 그림
- `tools/make_art_phone.py`: 상세 화면이 들어가게 폰 화면을 위아래로 늘림(화면 86×71 → 86×86, 폰 100×165 → 100×177).
- `tools/make_art_trade.py zoom63`: 모니터 확대 화면에서 아이템 칸·세로 구분선을 없애고 차트 칸을 전체 폭(x 48~272)으로, 점선 캔들 자리를 가운데(x 152)로.

## Scene (MCP, GameScene `HUD/ShakeRoot`)
| 바뀐 것 | 값 |
|---|---|
| `TableItems`·`MonitorZoom/Main/ZoomItemsLabel` | 지움 |
| 매수·매도 버튼 x | 52·126 → 88·162(가운데) |
| `ZoomKnown`·`ZoomUnknown` x | 117 → 152 |
| 새 `PhoneClosed`(262, −142, 40×24) | 그림 + Button + `Count`(초록 숫자) + `Outline`(금색 테두리) + `PhoneView` |
| 새 `PhoneZoom` | `Dim`(어둡게 + 바깥 클릭) + `Body`(펼친 폰 110, −1, 100×177) + `List`(제목 + 옮긴 `ItemSlots` 2×2, 칸 28×28) + `SoftLeft`/`SoftRight` + `GreenKey`/`RedKey` + `Key1~4`(+ 금색 `Lit`) |
| `ItemCloseup` | 폰 `Body` 안으로 옮김. 어둡게·패널·아이콘 상자·"1~4 전환" 지움, "Space 사용"·"Esc 뒤로" 글은 끔(소프트키가 대신). 큰 아이콘(9, −12)·이름(52, −11, 크기 9)·분류(52, −25)·설명(9, −40, 폭 82) |

## Spec과 다른 점(구현 중 판단)
- 상세 화면 아래 "Space 사용 · Esc 뒤로"(폭 98)가 폰 화면(86)에 안 들어가서, 폴더폰처럼 **소프트키**(화면 왼쪽 아래 "사용"/오른쪽 아래 "뒤로" + 아래 초록·빨간 키)로 바꿨다. 키 이름(Space·Esc)은 화면에 없고 규칙 4쪽에 있다.
- 아이템을 쓰면 폰이 닫힌다(결과를 보이게).

## 확인 (Play, 테스트 흉내)
- 접힌 폰 "2" + 금색 테두리(`t0.png`), 열기 → 목록(`t1.png`), Space → 상세(`t2.png`·`t4-detail.png`)
- 뒤로 → 목록(상세 칸 그대로 금색) → 뒤로 → 닫힘
- 모니터가 열린 채 2 → 모니터 닫히고 폰 상세 / 폰 열린 채 A/D 무시 / Esc 일시정지 안 됨
- 차트 분석 사용 → 아이템 2 → 1, 폰 닫힘, 모니터 차트 칸 가운데에 아는 캔들(`t3.png`)
- 광대 차례에 열기·1~4 무시
- 칸 클릭 후 칸 자리 제자리, Console 오류 0(고친 뒤)
