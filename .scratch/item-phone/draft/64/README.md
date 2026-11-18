# draft/64: 광대 폰에서 광대 아이템이 보이고, 쓸 때 반짝인다

코드는 AI가 Scripts에 직접 썼다(CLAUDE.md 4장 예외, item-phone Grilling Q6). 고치기 전 `*.before.cs`, 고친 뒤 같은 이름 `.cs`.

## GameManager.cs
| 바뀐 줄 | 왜 |
|---|---|
| `using System.Collections.Generic;` | 아래 `IndexOf`가 `IReadOnlyList`를 받아서 |
| `ClownItemUseCount`(+1씩), `ClownLastUsedSlot` | 광대가 아이템을 쓴 "신호". 화면(광대 폰)은 숫자가 바뀌었는지만 보고 그 칸을 반짝인다. 판정에는 안 씀 |
| `UseClownItem` 처음에 칸 번호 기억 → `ClownItemUseCount++` → 그다음 `RemoveItem` | 지우고 나면 뒤 칸이 앞으로 당겨져 어느 칸이었는지 알 수 없으니 먼저 기억 |
| `static int IndexOf(...)` | 그 종류가 처음 나오는 칸. `RemoveItem`(List.Remove)도 처음 나오는 것을 지우므로 같은 칸 |

## ClownPhoneView.cs (새 파일)
- `Refresh`: 처음엔 지금 신호 값을 기억(시작부터 반짝이지 않게). 신호가 바뀌었으면 `Flash(칸)` 코루틴, 아니면 지금 목록을 그린다.
- `Flash`: 반짝이는 동안은 **옛 목록**(쓴 아이템이 아직 보이는)을 그대로 두고 그 칸 금색 테두리를 0.05초 켜고 0.05초 끄기 × 3번(0.3초) → 끝나면 지금 목록(빠지고 당겨짐).
- `Show`: 칸마다 아이템이 있으면 작은 아이콘(`miniIcons[(int)종류]`), 없으면 숨김.
- `OnDisable`: 꺼졌다 켜질 때 반짝임이 켜진 채 남지 않게.
- `WaitForSeconds`라 일시정지(timeScale 0) 중엔 반짝임도 멈춘다. 광대 생각 시간(1초)은 바꾸지 않았다.

## ItemSlotsView.cs
- `clownIcons` 칸과 그리는 줄을 지움(왼쪽 위 광대 아이콘은 광대 폰으로).

## UIManager.cs
- `ClownPhoneView clownPhone` 칸 + `Refresh`에서 같이 부름.

## Scene (MCP, GameScene `HUD/ShakeRoot`)
| 바뀐 것 | 값 |
|---|---|
| `ClownItems`(왼쪽 위 아이콘 4개) | 지움 |
| 새 `ClownPhone`(90, −60, 32×44) | `phone-clown` 그림 + `Icon0~3`(12×9, 2×2) + `Flash0~3`(13×10, 처음 꺼짐) + `ClownPhoneView` |
| `ResultText` 폭 | 106 → 84 (광대 폰과 겹치지 않게) |

## 확인 (Play)
- 시작 화면 광대 폰에 광대 아이템 2개(`c0-before.png`)
- 실제 광대 차례: 광대가 차트 분석 사용 → 칸 빠지고 거래 정지가 앞으로(`c1-flash.png`는 반짝임이 끝난 뒤)
- 반짝임 자체는 시간을 느리게 하고 신호를 직접 올려 확인: 복권 칸(2번째)에 금색 테두리 → 0.3초 뒤 [거래 정지, 찌라시](`c2-flash.png`)
- Console 오류 0
