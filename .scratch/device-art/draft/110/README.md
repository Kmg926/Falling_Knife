# 110 줄별 설명 — 레버리지는 바뀔 때만 잠깐 보인다 · 두 폰 오른쪽으로

> 2026-11-27, 사용자 "두 휴대폰 모두 약간 오른쪽으로. 그리고 레버리지를 숨겨보자" → 질문에 "바뀔 때만 잠깐"·"12칸" 선택. AI가 Scripts에 직접 씀.

## LeverageView.cs (`LeverageView.before.cs` → `LeverageView.cs`)
| 줄 | 바뀐 것 | 왜 |
|---|---|---|
| `box`·`titleText` 칸 | 새로 | 숨길 때 상자 그림·"레버리지" 제목도 같이(불꽃·숫자·다음 피해는 원래 칸) |
| `showSeconds = 1.5`·`fadeSeconds = 0.25` | 새로 | 나타나기 0.25초 → 1.5초 보임 → 0.25초 사라짐 |
| `lastValue = -1` | 새로 | 판 시작의 첫 값(0)은 "바뀜"으로 치지 않음 → 시작할 때 상자가 안 뜸 |
| Refresh 끝 `if (lastValue >= 0 && value != lastValue) shownUntil = ...` | 새로 | 오를 때도, 피해·새 라운드로 0이 될 때도 잠깐 보임 |
| `pinned = game.IsTutorial` | 새로 | 튜토리얼 판은 광대가 상자를 가리키며("저 다음 피해") 설명하므로 늘 보임 |
| `Update()` | 새로 | 보일 정도(alpha)를 목표(1 또는 0)로 조금씩 옮기고, 바뀐 프레임에만 상자·불꽃(SpriteRenderer 색 a)·글자(TMP alpha)를 칠함. Time 기준이라 일시정지 중엔 같이 멈춤 |
| `SetAlpha` | 새로 | SpriteRenderer 색의 a만 바꾸는 도우미 |

## GameScene (MCP)
| 대상 | 전 → 후 |
|---|---|
| LeverageView 칸 | box = LeverageBox, titleText = LeverageTitle |
| PhoneClosed(내 폰) | (218, −147) → (230, −147) |
| ClownPhoneClosed(광대 폰) | (216, −102) → (228, −102) |
| LeverageBox·Flame1~3·LeverageTitle·LeverageCount·NextDamage·Tutorial/LeverageArea | 사용자 "8픽셀 위로" → 모두 y +8 (상자 (96, 19) → (96, 27)) |
