# draft 123 — 규칙 아이템 쪽: 설명 그대로 + 쪽 늘리기

바뀐 파일: `TitleMenu.cs` (전: `TitleMenu.before.cs`). 복권 설명은 `Data/Items/Lottery.asset`(MCP로 입력).

| 바뀐 곳 | 왜 이렇게 동작하나 |
|---|---|
| `using System.Collections.Generic;` | 쪽마다 첫 아이템 번호를 담는 `List<int>`를 쓰려고 |
| `PageTitles`·`const PageCount` 삭제 → `FixedTitles`, `itemPageStart`, `PageCount =>` | 쪽 수가 고정 3이 아니라 "앞 2쪽 + 아이템 쪽 수"라서 계산식(프로퍼티)으로 바꿈. 아이템 쪽이 0개로 계산돼도 최소 1쪽 |
| `itemRowsTopFirst`(-50)·`itemRowsTopRest`(-23)·`itemRowsBottom`(-138)·`itemRowGap`(3)·`itemRowMinHeight`(10) | 첫 아이템 쪽은 소개 글 아래(-50)부터, 그 뒤 쪽은 소개 글이 없어 본문 자리(-23)부터. -138 아래는 쪽 안내 글(-140) 자리 |
| `itemShort` 삭제 | 요약하지 않기로 함. 모든 줄이 ItemData 설명 그대로 |
| `OpenRules`에서 `LayoutItemPages()` | 규칙 창을 열 때 한 번 쪽을 나눔(설명이 바뀌어도 자동으로 맞음) |
| `ItemLine(i)` | 줄 글 = 금색 이름 + 두 칸 + 설명. 재는 곳·보여 주는 곳이 같은 글을 쓰도록 한 곳에 |
| `ItemRowHeight(i)` | `GetPreferredValues(글, 폭 226, 0).y` = 폭 226에서 줄바꿈했을 때의 높이(한 줄 약 10, 두 줄 약 20). 아이콘 높이 때문에 최소 10 |
| `LayoutItemPages()` | 위에서부터 높이+빈칸을 더해 가다가 남은 자리를 넘으면 새 쪽을 시작하고 그 아이템 번호를 `itemPageStart`에 넣음. 첫 쪽만 자리가 좁음(-50~-138), 이후는 -23~-138 |
| `ShowPage` 제목 | `규칙 {쪽}/{전체} · 화면/행동/아이템` 을 코드로 만듦 |
| `ShowPage` 아이템 쪽 | 이 쪽의 첫 번호~다음 쪽 첫 번호 직전까지만 보여 줌. 줄 묶음(ItemRows) 위치를 첫 쪽 -50 / 그 뒤 -23으로 옮김. 본문 글은 2쪽 = 행동, 첫 아이템 쪽 = 소개, 나머지 = 빈칸 |
| `ShowItemRows` | 줄 높이가 다르므로 줄마다 y를 누적해 글·아이콘 위치를 코드로 정함. 아이콘은 첫 줄에 맞춰 1 아래 |
| 쪽 넘기기(`Turn`)·안내 글 | `PageCount`가 계산식이 됐을 뿐 그대로 동작(마지막 쪽에서 "← 앞 쪽") |

## 같이 바꾼 것 (코드 아님)
- `Lottery.asset` 설명 = "사용 시 무작위로 다음 중 하나의 결과가 발생한다.\n잔고 +1 / 레버리지 +1 / 무작위 아이템 +1 / 꽝" (사용자 문구 그대로)
- GameScene `ItemCloseup/Description`(TMP): Auto Size 켬, 최소 6·최대 7.5. 상자(82×44)보다 긴 글만 줄어듦(복권 약 6.7). 다른 아이템은 7.5 그대로
