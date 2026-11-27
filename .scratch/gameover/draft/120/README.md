# 120 줄별 설명 — 패배 화면을 간소화한다

> 2026-11-27, 사용자 "게임 오버 창을 수정한다. 레퍼런스 이미지 참고해. 최대한 간소화 하자. 플레이 타임은 넣고" + 레퍼런스 2장(검은 화면 붉게 빛나는 GAME OVER / 글리치 GAME OVER) → AI 질문 → "패배만 바꿈"(승리 화면 그대로). AI가 Scripts에 직접 씀.

## ResultPanel.cs (`ResultPanel.before.cs` → `ResultPanel.cs`)
| 줄 | 무엇을 | 왜 |
|---|---|---|
| `gameOverRoot`·`playTimeText`·`gameOverRetry`·`gameOverTitle` | 패배 전용 화면 칸 | 승리 화면(`root`)은 건드리지 않고 따로 둠 |
| `Awake()` | 패배 화면 버튼 2개도 다시하기·타이틀에 연결 | 같은 GameManager 함수 |
| `UseButtons(retry, title)` | 마우스 강조할 버튼 묶음을 고름. 글자 원문은 "▶ "를 뺀 채 기억 | 두 화면이 같은 강조 코드를 씀 |
| `Refresh()` 앞부분 | `simple` = 판이 끝났고 패배. 패배면 `gameOverRoot`만, 승리면 `root`만 켬. 켜지는 순간 그 화면 버튼으로 바꾸고 "다시하기" 강조 | 승리 화면은 예전 그대로 |
| `Refresh()` 패배 | `플레이 타임 m:ss`만 쓰고 끝 | 간소화: 원인·라운드·레버리지·아이템 기록은 안 보임 |
| `SetHighlight()` | 그림 있는 버튼(승리 화면)만 밝기를 바꿈 | 패배 화면 글자 버튼은 투명한 클릭 칸이라 색을 칠하면 흰 사각형이 됨(처음 시험에서 그랬음 — Button을 붙일 때 Unity가 그 칸을 자동으로 연결) |

## GameScene (MCP)
`HUD/ShakeRoot/GameOverScreen`(ResultScreen 바로 뒤, 처음 꺼짐): 화면 전체 Image(`gameover-bg.png`, 뒤 클릭 막음) + PlayTime TMP (60, −92) 200×10 글자 8 가운데 + RetryButton (110, −116)·TitleButton (110, −131) 100×12(투명 Image + Button 전환 없음 + 글자 9).

## 그림 `tools/make_art_gameover.py`
검은 바탕 + 옅은 주사선 + 글자 뒤 붉은 빛(바둑판) + 7×9 도트 글자 ×6 "GAME OVER"(빨강 3단계·검은 테두리) + 글자 밖에만 얹은 색 어긋남 2줄 + 가장자리 글리치 띠. 처음엔 글리치가 글자를 검게 끊어 안 읽혀 끊지 않게 고침.
