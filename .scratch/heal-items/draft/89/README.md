# draft 89: 회복 아이템으로 잔고를 되찾는다

> 2026-11-23. 코드는 AI가 Scripts에 직접(heal-items Grilling Q8, CLAUDE.md 4장 예외), 그림은 `tools/make_art_hd_items.py heal`. `*.before.cs` = 바꾸기 전(`TitleMenu.before.cs`는 직전 커밋 `b4cedce`에서 꺼냄).

## 그림 (`tools/make_art_hd_items.py heal`)
- 기존 4종처럼 21×16 "설계 좌표"로 모양을 정하고 슬롯(42×32)·확인 패널(68×52) 크기로 각각 그림 → 늘려서 흐려지지 않음. 32색.
- 배당금 = 금화(테 `gold0`·몸통 `gold1`·빛 `gold2/3`·그늘·안쪽 고리 `goldD`) + 초록 "+". 구제 금융 = 끈으로 묶은 크림색 자루 + 가운데 금화 무늬 + "++".
- 시안 고침 1번: 처음 자루는 금화 무늬 2개 + 귀 같은 주둥이라 얼굴처럼 보임 → 부채꼴 주둥이 + 금화 하나. 비교 `icons-preview.png`(위 슬롯 크기 6종, 아래 확인 크기 6종).

## 줄별 설명 (면담 대비)
### ItemType.cs
- `Dividend`, `Bailout`을 **뒤에** 추가 — enum은 순서대로 0, 1, 2… 번호가 붙는데, 앞 4종 번호(0~3)가 그대로라 그림 배열(번호 = 칸)·저장된 값이 안 깨짐.

### GameManager.cs
- `NextItem` — 고정 지급 문자에 `D`·`B` 추가. 그 외엔 `RandomItem()`.
- `ItemWeights` `{1,1,1,1,1,0.5}` + `RandomItem()` — "무게대로 뽑기": 0 ~ 무게 합(5.5) 사이 숫자를 뽑아 앞에서부터 무게를 빼다가 0 밑으로 내려간 칸. 구제 금융은 1/5.5 ≈ 9%, 나머지 1/5.5 ≈ 18%. 복권 "아이템 +1"도 `NextItem`을 써서 같은 확률.
- `HealAmount` — 배당금 1·구제 금융 2·그 외 0. `CannotHeal` — 회복 아이템인데 잔고가 시작 잔고 이상이면 true(확인 화면 안내에도 씀, `static`이라 화면 코드에서 바로 부름).
- `UseHeal` — `Participant.Heal`이 시작 잔고를 넘지 않게 자르므로, 실제로 오른 양(`after − before`)을 결과 글에 씀(9에서 구제 금융 = +1).
- `UseConfirmedItem` — 두 아이템을 같은 `case`로: 가득이면 `used = false`로 아이템을 남기고 "사용 불가", 아니면 지우고 `UseHeal`.
- 광대는 89에선 회복 아이템을 받기만 하고 쓰지 않음(판단은 Ticket 90).

### ItemCloseupPanel.cs · UIManager.cs
- 가득이면 상세 설명 밑에 "(잔고가 가득이라 못 쓴다)"(광대 폰 보기에선 안 붙임), 개발용 글자에도 같은 안내.

### TitleMenu.cs (규칙 창)
- 아이템 6줄은 한 쪽(4줄 자리, 높이 92)에 안 들어감 → 쪽을 5개로: 3쪽 "아이템"(앞 4종), **4쪽 "회복 아이템"**(배당금·구제 금융 + 설명 2줄), 5쪽 "조작".
- `ShowItemRows(first, count)` — 같은 4줄 칸에 `first`번째 아이템부터 `count`개 글·아이콘을 채우고 남는 줄은 숨김. 아이콘 그림은 `itemIconSprites`(ItemType 순서 6개)에서 바꿔 끼움.

## 데이터·Scene (MCP)
- `Data/Items/Dividend.asset`(배당금 / 회복 / 잔고를 1 회복한다.), `Bailout.asset`(구제 금융 / 회복 / 잔고를 2 회복한다.).
- GameScene: `GameManager.itemDatas`·`ItemSlotsView.itemIcons`·`ItemCloseupPanel.bigIcons`·`iconOffsets` 4 → 6. 새 그림 가져오기 설정은 기존 아이콘과 같게.
- TitleScene: `TitleMenu.itemData` 6, `itemIcons`(Icon0~3), `itemIconSprites` 6.

## 확인 (2026-11-23, MCP)
| | 결과 |
|---|---|
| C1 | 폰 목록에 배당금·구제 금융 아이콘(`phone-list.png`), 상세에 이름·큰 그림·설명 |
| C2 | 잔고 7 → 배당금 8, 9 → 구제 금융 10(+1만), 10 → 구제 금융 못 씀·아이템 남음·"(잔고가 가득이라 못 쓴다)"(`detail-full.png`) |
| C3 | 55,000번 뽑기: 각 17.9~18.5%, 구제 금융 9.0%(기대 18.2% / 9.1%) |
| C5 | 규칙 4/5 회복 아이템 쪽, 글 넘침 0(`rules-heal-page.png`), 3쪽으로 돌아가면 앞 4종 그대로 |
| C6 | 퇴행 T65~T68·T143 통과(T143: 이번엔 설정 음악이 60%라 공개 중·평소 모두 0.21로 같음), Console 오류 0 |

**남은 것(사용자):** 실제 플레이에서 회복 아이템이 너무 자주/드물게 나오는지, 아이콘이 알아볼 만한지.
