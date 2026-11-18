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

## 고침: 광대 폰 숫자를 그림에 넣음 (사용자: "광대 폰의 숫자는 아직 겹쳐 그리고 각도가 틀어진 만큼 숫자도 각도를 바꿔야 해")
- 글자(TMP)를 따로 돌리면 픽셀 글자가 깨지고 칸 밖으로 넘침 → `tools/make_art_phone.py table`이 개수 0~4를 바깥 화면에 미리 그린 뒤 폰과 함께 눕히고 18° 돌린 그림 5장(`phone-table-clown-0~4.png`)을 만든다.
- `ClownPhoneView.cs`: `phoneImage`·`countSprites` 칸 추가. `Refresh`에서 개수에 맞는 그림으로 바꿔 끼우고(`countSprites[count]`), 그림이 있으면 글자(`countText`)는 끈다(그림이 없을 때만 예전처럼 글자). 전 코드 `ClownPhoneView.before-digits.cs`.
- 확인: 광대 아이템 2 → 그림 2, 늘리면 3·4, 비우면 0으로 바뀜, 글자 꺼짐, Console 오류 0. 캡처 `dg-applied.png`·`dg-crop.png`.

## 고침: 두 폰 크기 키움 + 내 폰도 숫자를 그림에 (사용자: "두 폰 크기만 좀 늘리자, 숫자 가독성")
- `tools/make_art_phone.py table`: 내 폰도 바닥 원근 계산 방식(돌림 0)으로 1.35배(56×24 지금 좌표), 광대 폰 1.15배(46.5×30). 두께 10·6.
- 숫자: 3×5 숫자는 "2"가 "ㄹ"처럼 보여 **5×7 둥근 픽셀 숫자**(×2 = 10×14)로. 바깥 화면을 위아래로 키워(안쪽 14 → 18줄) 숫자 위아래 여백 2줄. 돌릴 때 획이 끊기지 않게 한 칸 안 4곳을 더 봐서 숫자 색이면 칠함(화면 안쪽에서만).
- 버그 수정: "열 수 있음" 금색 테두리를 만들 때 검은 화면 칸을 바깥으로 보고 칸 둘레에도 줄을 그렸음(숫자 위를 가로지르는 금색 줄) → 폰 바깥 윤곽(`body`)으로만.
- `PhoneView.cs`: 광대 폰과 같은 `phoneImage`·`countSprites`(0~4 그림 바꿔 끼우기, 그림이 있으면 글자 끔). 전 코드 `PhoneView.before-digits.cs`.
- Scene(MCP): 내 폰 (214, −150.5) 56×24, 광대 폰 (213.5, −100.5) 46.5×30, 그림 0~4 연결. 캡처 `bg-bigger.png`·`bg-crop.png`.

## 고침: 두 폰 크기 줄임 + 내 폰도 18° (사용자: "두 폰 너무 커. 지금보다 많이 작고 이전 것보다 아주 조금 커야 해. 내 폰도 광대 폰과 같은 각도로")
- `tools/make_art_phone.py table`: 내 폰 1.35 → 1.12배·18° 틀기(45×27.5 지금 좌표), 광대 폰 1.15 → 0.92배(37×24.5). 두께 8·5.
- Scene(MCP, 저장 전 dirty 아님 확인): 내 폰 (218, −148), 광대 폰 (216, −102). 캡처 `sm-smaller.png`·`sm-crop.png`.
