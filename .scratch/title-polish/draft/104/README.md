# draft 104: 타이틀 메뉴 다듬기

> 2026-11-27. `TitleMenu.before.cs` = 바꾸기 전. 그림 도구 `make_title_ui.py`(AI 작성, ADR 0003).

## 줄별 설명 — TitleMenu.cs (면담 대비)
- 새 칸: `selectBar`(고른 줄 뒤 띠), `knife`(칼), `barOffset (−6, 1)`, `knifeOffset (−15, 1)` — 줄의 왼쪽 위 기준으로 얼마나 떨어뜨릴지.
- `RefreshMenu`: 칼이 연결돼 있으면(`useKnife`) "▶ " 글자를 붙이지 않음(글자 시작 선은 그대로). 고른 줄의 `anchoredPosition`에 띠·칼의 자리를 더해 옮김 — 셋 다 같은 부모(Menu) 안이라 좌표를 그대로 쓸 수 있음. 마우스·키보드로 고르면 이 함수가 불려 같이 움직임.

## 그림 — tools/make_title_ui.py
- `sparse()`: level(0~1)만큼만 점을 찍고 나머지는 투명. 4×4 바둑판(Bayer) 순서라 고르게 성김 → 팔레트를 벗어나는 반투명 없이 흐려 보임(광대 조명 빛과 같은 방법).
- `select_bar` 300×26(화면 150×13칸): 몸통 gold0, 위·아래 2줄 gold1, 왼쪽 끝 기둥 gold2, 오른쪽으로 갈수록 점이 줄어듦.
- `knife` 12×26(6×13칸): 남색 손잡이 → 금색 날밑 → 크림색 날(아래로 갈수록 좁아짐, 오른쪽 그늘·왼쪽 빛).
- `logo_glow` 420×90(210×45칸): 타원 안 red0을 가운데 진하게, 더 가운데 red1 몇 점.

## Scene (MCP, TitleScene)
| 대상 | 값 |
|---|---|
| Menu/SelectBar | (−6, 1) 150×13, 글자보다 뒤(첫 자식) |
| Menu/Knife | (−15, 1) 6×13 |
| LogoGlow | (8, −13) 210×45, 로고 바로 뒤 |
| FooterHint / Version | 오른쪽 아래 (196, −168) 120×10 / 왼쪽 아래 (4, −168), 크기 8, gray1 |

## 고침 (2026-11-27, 사용자 "칼은 없애자")
| 줄 | 바뀐 것 | 왜 |
|---|---|---|
| `knife`·`knifeOffset` 칸 | 삭제 | 칼 오브젝트(`Menu/Knife`)와 그림을 없앴으므로 연결할 곳이 없음 |
| `bool useBar = selectBar != null;` | 전에는 `useKnife` | ▶를 안 쓰는 이유가 이제 띠. 띠가 연결 안 됐으면 예전처럼 ▶를 다시 붙임 |
| `if (useBar && ...) selectBar.anchoredPosition = 줄 + barOffset;` | 칼 줄 삭제, 한 줄로 | 띠만 고른 줄 높이로 옮김 |
