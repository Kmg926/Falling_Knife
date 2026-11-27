# 105 줄별 설명 — 규칙을 짧은 카드 3쪽에서 찾아본다

> 2026-11-27, AI가 Scripts에 직접 씀(rules-card Grilling Q8). 바꾸기 전 `TitleMenu.before.cs`, 뒤 `TitleMenu.cs`.

## TitleMenu.cs
| 줄 | 바뀐 것 | 왜 |
|---|---|---|
| 옛 `rulesPage1`·`rulesLeverage`·`rulesItems`·`rulesHealItems`·`rulesControls` | 지움 | 5쪽 → 3쪽 |
| 새 `rulesScreen`·`rulesScreenLegend`·`rulesScreenNotes` | 1쪽 그림 묶음(그림·이름표·아래 글) | 1쪽에서만 켬. 묶음 하나라 켜고 끄기 한 줄 |
| 새 `rulesScreenLegendText`(1~7 이름표) | 그림 배지 숫자와 같은 순서 | 숫자 자리는 `tools/make_rules_screen.py`의 SPOTS |
| 새 `rulesScreenNotesText` | 목표 한 줄 + 조작 두 줄 | 옛 5쪽 "조작"을 1쪽에 합침(Q3) |
| 새 `rulesActions` | 행동 표 + 레버리지 + 덱 개수 | 결과 칸을 "다음 피해만큼 잃음"으로(수식 없음, Q2). 9줄이라 `<line-height=88%>` |
| 새 `rulesActionsFooter` | "한 번 더가 아니면 차례가 넘어간다" | 2쪽 아래 안내 줄 위에 |
| **필드 이름을 모두 새로** | | Unity는 Scene에 저장된 값이 코드 기본값보다 먼저다. 이름이 같으면 옛 글이 계속 나옴 |
| `PageTitles`·`PageCount = 3`·`ItemPage = 2` | 쪽 3개 | |
| `rulesItemsIntro` | 받는 때(2개씩·최대 4) + "내 폰을 열어" | 어긋남 2·4 |
| `itemShort[]` | 빈 칸 = ItemData 설명 그대로, 복권·회복 2종만 규칙 창 글 | 복권 설명은 한 줄에 안 들어감. 회복 한도(시작 잔고까지)는 옛 4쪽 글을 줄 안으로 |
| `ShowPage()` | 1쪽 = 묶음 켜고 본문 비움, 3쪽 = 아이템 줄 전부 | |
| `ShowItemRows` | 줄 글에 `itemShort` 먼저 | |
| `Back()` | 묶음 끄기 | 규칙 뒤 설정·크레딧을 열 때 그림이 남지 않게 |

## TitleScene (MCP, 편집 모드 스크립트)
| 대상 | 값 |
|---|---|
| Panel/RulesScreen | (10, −23) 240×118, 처음 꺼짐 |
| RulesScreen/Shot | (0, 0) 160×90, `rules-screen.png`(640×360 → 1280×720 화면에서 1:1) |
| RulesScreen/Legend | (166, −1) 74×90, 글자 8, 줄 간격 6 |
| RulesScreen/Notes | (0, −94) 240×34, 글자 8, 줄 간격 0 |
| ItemRows | (10, −50), 줄 7개(Icon4~6·Text4~6은 Icon3·Text3 복제), 줄 간격 12, 아이콘 10.5×8(그림 1배) |
| TitleMenu 칸 | itemIcons·itemTexts 7개, rulesScreen·Legend·Notes 연결 |

## 그림
`tools/make_rules_screen.py`: Unity 게임 화면 캡처(1280×720, 내 차례 첫 화면)를 2칸마다 1칸 골라 640×360으로 줄이고(그림이 640 격자라 깨지지 않음), 금색 배지(23×23, 숫자 3배)를 7곳에. 캡처 때 Game 창을 잠시 1280×720으로 바꿨다가 Free Aspect로 되돌림.
