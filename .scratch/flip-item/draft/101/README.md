# draft 101·102: 작전(다음 캔들 뒤집기)

> 2026-11-23. 코드는 AI가 Scripts에 직접(flip-item Grilling Q13, CLAUDE.md 4장 예외). `*.before.cs` = 바꾸기 전.

## 줄별 설명 (면담 대비)
- `ItemType`: `Flip`을 **맨 뒤**에 — 앞 6종 번호(0~5)가 그대로라 그림 배열·저장된 값이 안 깨짐. 그림 배열의 7번째 칸.
- `CandleDeck.FlipTop()`: 맨 위 카드를 반대로 바꾸고 `Recount()`로 하락·상승 수를 다시 셈.
- `GameManager`
  - `UseConfirmedItem`·`UseClownItem`: `Flip`이면 `UseFlip(쓴 사람)`. 나는 폰 칸에서 지우고 씀(차례 그대로).
  - `UseFlip`: 덱이 비면 아무 일 없음. 전·후 캔들을 보고 뒤집음 → 차트 분석으로 알던 값(`PlayerKnownNext`·`clownKnownNext`)이 있으면 뒤집힌 값으로(Q3). 내가 쓰면 결과 판 한 줄(`LastFlipText`)과 신호 번호(`PlayerFlipCount`).
  - `ItemWeights`에 0.5(Q6), `ItemFromChar`에 'F'(Q12), `ItemLine`/`ItemResultLine`에 대사 2개(Q8).
- `ClownAI` ①′: 다음이 하락인 걸 알고 작전이 있으면 작전. GameManager가 아이템을 쓴 뒤 다시 물으므로 그때는 상승을 알아 ①에서 매수(Q7).
- `ClownLine`: `UseFlip`·`FlipDone`(맨 뒤). `ClownSpeechView`: 기본 대사 2줄(Scene 표에도 같은 2줄을 붙임).
- `LotteryPopupView`: 작전 신호를 먼저 보고 `ShowFlip()` — 제목 "작전", 한 줄, 금색·높은 소리, 틀 없음. 복권은 제목을 "복권"으로 되돌림. 튀어나오는 부분은 `Pop()`으로 묶음.
- `TitleMenu`: 4쪽 제목 "회복·기타 아이템", 아이템 줄 4·5·6번(배당금·구제 금융·작전).
- `TutorialGuide`: 10단계 아이템 설명에 작전 한 줄.

## 그림 (102, `tools/make_art_hd_items.py flip`)
- `icon_flip`: 설계 좌표 21×16 — 왼쪽 빨간(하락) 캔들, 오른쪽 초록(상승) 캔들(심지 + 몸통 빛·그늘·테두리), 가운데 금색 화살표 2개(위 → 오른쪽, 아래 → 왼쪽 = 맞바꿈). 32색 팔레트 검사 통과.
- 첫 시안은 다른 아이콘보다 가늘어 몸통 반폭 1.9 → 2.4, 화살표 0.6 → 0.75로 굵게.
- `Sprites/HUD/item-flip.png`(42×32), `Sprites/Closeup/item-flip-big.png`(68×52), 가져오기 설정은 배당금 아이콘에서 복사(PPU 2, Point).

## 에셋·Scene (MCP)
- `Data/Items/Flip.asset`: 작전 / 조작 / "다음 캔들의 하락·상승을 뒤집는다."
- GameScene: `ItemSlotsView.itemIcons`·`ItemCloseupPanel.bigIcons`·`iconOffsets`·`GameManager.itemDatas` 7번째, 말풍선 표 2줄, 결과 판 제목 칸 연결. TitleScene: `itemData`·`itemIconSprites` 7번째.

## 확인 (2026-11-23, MCP Play)
| | 결과 |
|---|---|
| 내 작전 | 차트 분석(하락) → 작전: 맨 위 상승, 하락/상승 1/5 → 0/6, 알던 값 상승, 결과 판 "다음 캔들: 하락 → 상승", 차례·레버리지 그대로 |
| 광대 판단 | 하락 앎 + 작전 → 작전 / 상승 앎 → 매수 / 모름 → 규칙 ④ |
| 광대 사용 | 맨 위 상승, 광대가 아는 값 상승 → 다음 판단 매수, 대사 UseFlip·FlipDone |
| 규칙 쪽 | 4쪽 3줄(아이콘 포함) 캡처 |
| 퇴행 | T66 통과, Console 오류 0 |
