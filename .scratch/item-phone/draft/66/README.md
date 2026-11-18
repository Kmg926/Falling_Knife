# draft/66: 광대 폰을 열어 광대 아이템을 본다 (+ 내 폰 자리, "Space" 안내 지움)

코드는 AI가 Scripts에 직접 썼다(CLAUDE.md 4장 예외). 고치기 전 `*.before.cs`, 고친 뒤 같은 이름 `.cs`. 계기: Grilling Q13~Q16 + 사용자 지시(내 폰 자리, "Space" 글 지우기, 게임 안 조작 안내 줄이기).

## GameManager.cs
| 바뀐 줄 | 왜 |
|---|---|
| `IsViewingClownPhone` | 열린 폰이 광대 폰인가(보기 전용) |
| `PhoneItems => IsViewingClownPhone ? Clown.Items : Player.Items` | 화면(목록·상세)이 읽는 목록을 하나로 — 어느 폰이든 같은 화면 코드를 씀 |
| `Execute`: 상세에서 `if (!IsViewingClownPhone) UseConfirmedItem();` | 광대 폰 상세에서 Space는 아무 일 없음(사용 불가) |
| `Execute`: 목록에서 `ViewSlot(PhoneCursor)` | 목록 Space = 금색 칸 상세(내 폰이든 광대 폰이든) |
| `SelectItem`에 `IsViewingClownPhone = false` | 1~4 키는 늘 내 아이템 → 광대 폰을 보고 있었어도 내 폰으로 바뀜 |
| 새 `ViewSlot(slot)` | 칸 클릭·Space: 내 폰이면 `SelectItem`과 같고, 광대 폰이면 그 칸 상세를 보기만(광대 아이템 수로 범위 확인) |
| `OpenPhone`에 `IsViewingClownPhone = false`, 새 `OpenClownPhone()` | 접힌 내 폰 / 접힌 광대 폰 클릭. 열 수 있는 때는 같음(`CanOpenPhone`: 내 차례·공개 중 아님·모니터 닫힘) |

## ItemCloseupPanel.cs
- `game.Player.Items[...]` → `game.PhoneItems[...]`: 광대 폰이면 광대 아이템 상세.
- 거래 정지 "못 쓴다" 안내는 내 폰일 때만.

## ItemSlotsView.cs
- 칸 아이콘 = `game.PhoneItems`, 칸 클릭 = `game.ViewSlot(slot)`.

## PhoneView.cs
- 광대 폰이면 제목 "광대 아이템 N/4", 상세 왼쪽 소프트키 비움("사용" 없음), 키패드 1~4는 금색 표시만 하고 누를 수 없음(`interactable = !clown`), 금색 키 수 = 보고 있는 폰의 아이템 수.

## ClownPhoneView.cs (다시 씀)
- 64의 "세운 폰 + 아이콘 2×2 + 쓴 칸 반짝임" → **접힌 광대 폰**: 개수, 열 수 있을 때 금색 테두리·클릭(`OpenClownPhone`).
- 광대 사용 신호(`ClownItemUseCount`)가 바뀌면 테두리 0.05초 켜고 끄기 × 3 → 끝나면 원래(열 수 있으면 켜짐) 상태로.

## MonitorZoomPanel.cs
- `openHint`("Space" 글) 칸과 줄 지움 — 게임 안 조작 안내 줄이기(사용자 지시).

## Scene (MCP, GameScene `HUD/ShakeRoot`)
| 바뀐 것 | 값 |
|---|---|
| `ClownPhone`(세운 폰) | 지움 |
| 새 `ClownPhoneClosed`(24, −122) | 내 접힌 폰을 복제(그림·개수·테두리) + `ClownPhoneView`. 두 잔고 판 사이 |
| `PhoneClosed`(내 폰) | (262, −142) → (217, −142), 레버리지 상자 아래 |
| `OpenHint`("Space") | 지움 |
| `ResultText` 폭 | 84 → 106 (되돌림) |

세운 광대 폰 그림(`phone-clown`·`phone-clown-flash`·`mini-*`)은 이제 쓰지 않는다(파일은 남김).

## 확인 (Play)
- 시작: 왼쪽 광대 폰 "2"·오른쪽 내 폰 "2", 둘 다 금색 테두리, "Space" 없음(`k0-table.png`)
- 광대 폰 열기 → "광대 아이템 2/4" → Space → 복권 상세, 왼쪽 소프트키 없음(`k1-clown-detail.png`) → Space 다시 → 광대·내 아이템 수 그대로
- 뒤로 → 목록, 광대 폰을 보다가 1 키 → 내 폰 상세(거래 정지)로 바뀜, 뒤로 × 2 → 닫힘
- 광대 차례에 신호를 올리면 꺼져 있던 테두리가 켜짐(반짝임), 개수 줄어듦(`k2-clown-flash.png`)
- Console 오류 0
