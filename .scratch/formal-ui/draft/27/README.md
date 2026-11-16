# Ticket 27 초안: 아이템을 쓰기 전에 확인 패널에서 무엇을 하는지 읽는다

> **초안**이다(Assets 밖). 그림·Scene 배치는 AI가 MCP로 이미 함(`HUD/ItemCloseup`, 처음엔 꺼짐).
> 근거: `../../spec.md` (C7·C8·C10, 제안 5), 스케치 화면 3, GDD 아이템 표 "정보, 조작, 방해, 도박", 아트 메모 7-4절.

## 0. 바꿀 것
| 파일 | 할 일 | 줄 수 |
|---|---|---|
| `ItemData.cs` | `category` 칸 1개(+주석) | +2 |
| `GameManager.cs` | `ItemCategory()` — `ItemName()`과 같은 모양 | +6 |
| `ItemCloseupPanel.cs` | **새 파일** — 켜고 끄기·큰 아이콘·이름·분류·설명·사용/취소 클릭 | 44 |
| `UIManager.cs` | 칸 1개 + `Refresh`에 한 줄 | +2 |

순서: **ItemData → GameManager → ItemCloseupPanel → UIManager**. 반영 뒤 AI가 Scene 칸 연결 + ItemData 에셋 4개에 분류 값(정보·조작·방해·도박) 입력.

## 1. `ItemData.cs`
| 줄 | 왜 |
|---|---|
| `public string category;` | 스케치 화면 3의 분류 표("정보"). 이름·설명처럼 **데이터**라서 코드에 적지 않고 에셋(ScriptableObject)에 둔다 → 바꿀 때 Inspector에서만 |

## 2. `GameManager.cs`
| 왜 |
|---|
| 화면은 `ItemData`를 직접 찾지 않고 GameManager의 `ItemName`·`ItemDescription`을 쓰고 있었다 → 같은 모양으로 `ItemCategory` 하나 더. 에셋이 비어 있으면 빈 글자 |

## 3. `ItemCloseupPanel.cs` (새 파일)
| 부분 | 왜 |
|---|---|
| `root.SetActive(game.IsConfirming)` | 확인 중일 때만 어둡게 + 패널(C10). 상태는 GameManager가 갖고, 패널은 따라 보이기만 |
| `return` (확인 중 아니면) | 꺼져 있을 땐 내용을 채울 필요 없음. 그리고 `ConfirmingSlot`이 -1이라 `Items[-1]`로 오류가 나는 것을 막는다 |
| `bigIcons[(int)item]` | 26의 슬롯 아이콘과 같은 방법(enum 순서 번호) |
| 거래 정지 `사용 불가` | 개발용 글자에만 있던 안내를 정식 화면에도(UIManager 줄 76~80과 같은 조건) |
| `useButton` → `game.Execute()` | 확인 중의 `Execute`는 "그 아이템 사용"(Ticket 16). Space와 **같은 함수** → 판정이 둘로 갈라지지 않음(U1) |
| `cancelButton` → `game.CancelItem()` | Esc·우클릭과 같은 함수 |
| 컴포넌트를 붙이는 곳 | **꺼지지 않는 `HUD`**에 붙인다. 꺼질 수 있는 `ItemCloseup`에 붙이면 처음 켜질 때까지 `Awake`가 안 불려 클릭 등록이 늦어진다 |

## 4. `UIManager.cs`
- 26과 같은 방식: 칸 하나, `Refresh`에 `closeupPanel.Refresh(game);` 한 줄. 개발용 글자의 확인 줄은 그대로(F1로 볼 때).

## 5. Scene·데이터 (반영 뒤 AI가 MCP로)
1. `HUD`에 ItemCloseupPanel: Root = `HUD/ItemCloseup`, 큰 아이콘·이름·분류·설명·사용·취소, 큰 아이콘 4장(ItemType 순서)
2. UIManager `Closeup Panel` 칸
3. `Data/Items` 4개 Category: 차트 분석 **정보** / 찌라시 **조작** / 거래 정지 **방해** / 복권 **도박**
4. 저장

## 6. 테스트 (Ticket 27)
| # | 방법 | 기대 |
|---|---|---|
| T168 | 1번(차트 분석) 확인 | 패널 켜짐, 큰 돋보기·`차트 분석`·`정보`·설명, 슬롯 1 노란 테두리, 버튼 회색 (C10·C7) |
| T169 | 이어서 2번 | 패널 내용이 찌라시로 바뀜 |
| T170 | `사용` 클릭 | 효과 적용, 패널 꺼짐, 아이콘 사라짐 (C8) |
| T171 | 3번 → `취소` 클릭 / Esc | 패널 꺼짐, 아이템 남음 (C8) |
| T172 | 광대 정지 상태에서 거래 정지 확인 | 설명에 `사용 불가` |
| T173 | 퇴행 T65~T68·T143, 오류 0 | 같음 |
| T174 | 캡처 | 스케치 화면 3과 비교 |
