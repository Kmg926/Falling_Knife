# draft 80: 광대가 무엇을 하는지 말풍선으로 본다

> 2026-11-22. 코드는 AI가 Scripts에 직접 작성(clown-tell Grilling Q5, CLAUDE.md 4장 예외). `*.before.cs` = 바꾸기 전.

## 바뀐 파일
| 파일 | 바뀐 것 |
|---|---|
| `ClownLine.cs` (새) | 대사 종류 이름표(enum): None·Buy·Sell·UseChart·UseRumor·UseHalt·UseLottery·Halted |
| `ClownSpeechView.cs` (새) | 말풍선: 번호가 바뀌면 표에서 대사를 찾아 보이고 1.5초 뒤 닫음, 틀 폭을 글자에 맞춤 |
| `GameManager.cs` | `ClownSay`·`ClownSilence`·`ItemLine`, 광대 차례에서 말하고 기다림(0.6·0.8초), 거래 정지로 쉬는 차례 대사, 내 행동 4곳에서 말풍선 닫기, 새 라운드에서 닫기 |
| `UIManager.cs` | `clownSpeech` 칸 + Refresh |
| `tools/make_art.py` | `clowntell`: 말풍선 꼬리 4×7 (`Sprites/HUD/speech-tail.png`) |

## 줄별 설명 (면담 대비)
### ClownLine.cs
- `enum` = 정해진 이름 몇 개 중 하나만 갖는 값(신호등의 빨강·노랑·초록처럼). 판정 코드는 "Buy를 말할 순간"이라고만 알리고, 실제 글("매수. 들고 간다.")은 화면 쪽 표에 둔다(Spec 제안 1).

### GameManager.cs
- `LastClownLine`·`ClownLineCount` — 광대 폰 깜빡임(`ClownItemUseCount`)과 같은 방식: 번호가 바뀌었는지만 보면 화면이 Refresh를 여러 번 받아도 한 번만 반응하고, 같은 대사를 두 번 해도 다시 뜬다.
- `ClownSilence()` — 이미 None이면 아무것도 안 함(번호가 쓸데없이 오르지 않게).
- `ClownTurnRoutine`
  - 아이템: `ClownSay(ItemLine(...))` → 아이템 효과 → 화면 새로 그림 → `WaitForSeconds(clownItemSeconds)`(0.8초) → 다시 생각.
  - 매수·매도: `ClownSay(Buy/Sell)` → 화면 → `WaitForSeconds(clownDeclareSeconds)`(0.6초) → 공개. `WaitForSeconds`는 일시정지(timeScale 0) 중엔 멈춘다.
- `PassTurn` — 차례를 받을 사람이 거래 정지면 건너뛰는 자리. 그 사람이 광대면 "…한 번 쉬지."(Spec 제안 3).
- `SelectItem`·`OpenPhone`·`OpenClownPhone`·`OpenMonitor` — 할 수 있는지 확인한 **뒤**에 `ClownSilence()`(못 하는 때 눌러도 말풍선은 그대로).
- `StartRound` — 새 라운드는 말풍선 없이.

### ClownSpeechView.cs
- `Entry` 표 — Inspector에서 대사를 고칠 수 있음. `<color=#D9B45A>…</color>`는 TMP 태그(그 부분만 금색).
- `Refresh` — 번호가 그대로면 바로 나감. None이면 닫음. 아니면 글을 넣고 `GetPreferredValues`(이 글이 차지할 폭)로 틀 폭 = 글 폭 + 양옆 7. 틀의 기준점이 오른쪽 위(pivot 1,1)라 폭이 바뀌어도 꼬리(광대 쪽)는 제자리, 왼쪽으로 늘어난다. 클릭음 30%.
- `Update` — 보인 지 1.5초가 지나면 닫음. `Time.time`은 일시정지 중엔 멈춤 → 멈춘 동안은 그대로 남음.
- 이 스크립트는 `ClownSpeech`(항상 켜짐)에 붙고, 켜고 끄는 건 자식 `Bubble` — 자기 자신을 끄면 Update가 안 돌아서.

### 꼬리 그림 (`tools/make_art.py clowntell`)
- 줄 r마다 길이 e = 3 − |r − 3| (가운데 줄이 가장 김) → 0..e−1은 창 안쪽 색, e는 금색 테두리. 왼쪽 줄이 창 안쪽 색이라 창 오른쪽 테두리 위에 1칸 겹쳐 놓으면 테두리가 지워지며 꼬리와 이어진다(`speech-tail-preview-x8.png`).

## Scene (MCP)
- `/HUD/ShakeRoot/ClownSpeech`(ClownSpeechView) → `Bubble` → `Box`(오른쪽 위 기준 (138,−48), 높이 17, `closeup-panel` 9칸 늘이기) → `Tail`(틀 오른쪽 가운데, −1 겹침, 4×7) · `Text`(11, 크림, 왼쪽 정렬, 양옆 7). DeckReveal·결과·일시정지 화면보다 아래 순서. UIManager `clownSpeech` 연결.

## 확인 (2026-11-22, MCP, 시간 0.25배로 캡처)
| | 결과 |
|---|---|
| C2 | 광대에게 찌라시·거래 정지를 줌 → "**거래 정지**를 쓰지."(`item-halt.png`) → 다음 판단 → "찌라시를 쓰지."(상태값 UseRumor) |
| C1 | "**매수**. 들고 간다."(`declare-buy.png`, 틀이 글에 맞게 좁아짐) → 말한 순간 공개 안 됨(rev False) → 그 뒤 공개, 1.5초 뒤 닫힘. 매도 말풍선(상태값 Sell)도 나옴 |
| C5 | 내 차례에 말풍선 띄움 → `OpenMonitor` → 바로 닫힘(line None) |
| C7 | Console 오류 0. 퇴행 T65~T68·T143은 아직 안 돌림 |

- 9시 결과 글자가 아직 있어 말풍선 뒤에 겹쳐 보임 → Ticket 81에서 없앰.
- 0.6·0.8초를 정확히 잰 건 아님(코드 순서로 확인: 말함 → 기다림 → 공개).
- **남은 것(사용자):** 실제 플레이에서 읽을 시간이 충분한지, 말풍선 자리·대사 느낌.
