# title-menu Ticket 53: 타이틀에서 메뉴를 고르고 규칙을 읽는다

> AI가 Scripts에 직접 쓰고(title-menu Grilling Q9, CLAUDE.md 4장 예외), Scene은 MCP로. 면담 대비로 읽어 둔다.
> 근거: `../../spec.md`(제안 1·2·3·6·8), Grilling Q1~Q4. 전 코드 `TitleMenu.before.cs`.

## 바꾼 것
| 대상 | 내용 |
|---|---|
| `TitleMenu.cs` | 버튼 2개(시작·종료) + Space/Esc → **글자 메뉴 5줄**(`items`) + 고른 줄 번호 `Selected` + 열린 창 `Open`(없음·규칙·준비 중) + 규칙 쪽 `Page` |
| TitleScene | 테이블 `flipX`(위치 x 160), `ControlsBox`·`Controls`·`StartButton`·`QuitButton`·`Goal` 지움. `Menu`(Item0~4, x 22·y −108, 줄 간격 12, 크기 11), `MenuPanel`(`Dim` + `Panel` 260×150 + `Title`·`Body`·`Footer` + `ItemRows` 아이콘 4·글 4) |
| `closeup-panel.png` 가져오기 설정 | 9칸 늘이기 테두리 4도트(Unity 가져오기 설정으로, 게임 화면의 아이템 확인 창은 Simple이라 영향 없음). 타이틀 창 Image는 Sliced + `pixelsPerUnitMultiplier` 100 |

## 줄별 핵심 (`TitleMenu.cs`)
| 부분 | 왜 |
|---|---|
| `Move(dir)`: `(Selected + dir + 개수) % 개수` | 끝에서 반대쪽으로 돎(맨 위에서 ↑ → 종료) |
| `Choose()`의 `switch` | 0 시작(씬 이동) · 1 규칙 · 2 설정 · 3 크레딧(54·55 전까지 "준비 중") · 4 종료 |
| `Back()`: `Open == None`이면 아무것도 안 함 | 첫 화면 Esc로 실수로 꺼지지 않게(Grilling Q2) |
| `Turn(dir)`: `Mathf.Clamp(Page + dir, 0, 2)` | 규칙 1~3쪽에서 처음·끝에 멈춤 |
| 마우스: `RectangleContainsScreenPoint` + **마우스가 움직였을 때만** 고름 | 키보드로 고른 줄을 가만히 있는 마우스가 덮어쓰지 않게 |
| `uiCamera = canvas.worldCamera` | Canvas가 Screen Space Camera라 마우스 위치를 그 카메라로 계산해야 맞음 |
| 아이템 글 = `itemData[i].displayName — description` | 규칙 창 글이 게임 속 아이템 설명과 늘 같음(ItemData 하나만 고치면 됨) |
| 규칙 글은 `[SerializeField, TextArea]` | Inspector에서 문구를 바로 고칠 수 있음 |
| 공개 `Move`·`Choose`·`Back`·`Turn` | `Update`(키·마우스)와 테스트가 같은 길로 동작 |

## 시안에서 고친 것
| 문제 | 원인 | 고침 |
|---|---|---|
| 창 테두리가 온통 금색 | 그림이 1도트 = 1단위(PPU 1)라 UI가 테두리 4도트를 100배로 늘림 | Image `pixelsPerUnitMultiplier` 100 |
| 복권 글이 창 밖으로 | 글 칸이 너무 넓고 줄바꿈 꺼짐(복사한 부제 설정) | 폭 204·줄바꿈 켬·크기 8·높이 20 |
| 규칙 1쪽 넘침(9줄) | 빈 줄 하나 | 빈 줄 빼서 8줄 |
| 메뉴 아래 줄이 테이블 매트와 겹침 | 매트가 테이블 왼쪽에 있음 | 줄 간격은 12 그대로(11은 글자가 붙어 보임), **테이블을 좌우로 뒤집음**(`flipX`, 기준점이 왼쪽이라 x −160 → 160) → 매트가 오른쪽으로 |

## 테스트 (Ticket 53)
| # | 방법 | 기대 | 결과 |
|---|---|---|---|
| C1 | `Move(−1)` 맨 위에서 / 다시 `Move(1)` | 4(종료) / 0 | ✅ |
| C2 | 첫 화면 `Back` / 창에서 `Back` / "시작" `Choose` | 그대로 / 닫힘 / GameScene | ✅ |
| C3 | 규칙 3쪽 TMP 넘침, 끝·처음에서 더 넘김, 아이템 글 | 넘침 0, 2·0에서 멈춤, ItemData와 같음 | ✅ |
| — | 설정·크레딧 줄 | "준비 중" 창 | ✅ |
| C6 | GameScene 퇴행 T65~T68·T143, Console 오류 | 같음, 0 | ✅ |

남은 확인(사용자): 실제 키보드(↑↓·W S·Enter·Esc)·마우스 올리기·클릭.
