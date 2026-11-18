# draft/73: 폰 상세 — 아이템별 아이콘 자리 (찌라시·복권 오른쪽으로) (63 고침)

사용자: "복권과 찌라시는 현 상태에서 오른쪽으로" (72에서 이름을 왼쪽 선에 맞춘 뒤).

## ItemCloseupPanel.cs
| 바뀐 곳 | 왜 |
|---|---|
| `iconOffsets`(Vector2 ×4, ItemType 순서) | 아이콘 그림마다 빈 공간이 달라 아이템별로 자리를 조금씩 옮김 |
| `shadow` + `shadowHome` | 아이콘 아래 그림자도 아이템별 자리만 따라감(위아래 뜸은 안 따라감) |
| `Update`: `iconHome + ItemOffset() + (0, bob + drop)` | 아이템별 보정 + 뜸 |
| 새 `ItemOffset()` | 지금 보이는 아이템의 보정(칸이 모자라면 0) |

## Scene (MCP, GameScene, 저장 전 dirty 아님 확인)
- `iconOffsets` = 차트 분석 (0,0) · 찌라시 (3,0) · 거래 정지 (0,0) · 복권 (3,0). `shadow` = ItemCloseup/Shadow.
- 숫자는 Inspector의 `Icon Offsets`에서 바로 바꿀 수 있음.

## 확인
- 네 아이템 상세 캡처 `after-4items.png`: 찌라시·복권 아이콘만 오른쪽으로 3(6도트), 이름·설명 정렬 그대로. 컴파일 오류 0.
