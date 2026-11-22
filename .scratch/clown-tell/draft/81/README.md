# draft 81: 결과를 광대의 반응으로 알고, 왼쪽 결과 글자는 없다

> 2026-11-22. 코드는 AI가 Scripts에 직접 작성(clown-tell Grilling Q5, CLAUDE.md 4장 예외). `*.before.cs` = 80 끝난 상태.

## 바뀐 파일
| 파일 | 바뀐 것 |
|---|---|
| `ClownLine.cs` | 결과 대사 이름 12개 추가(OneMore·Hurt·Gotcha·NothingHappened·Laugh·ChartDone·RumorDone·HaltDone·Lottery 4개) |
| `ClownSpeechView.cs` | 대사 표 기본값 12줄 추가(Scene 표도 MCP로 19줄로 맞춤) |
| `GameManager.cs` | 매수·매도 결과에서 대사, 아이템 결과 대사, 복권 실제 결과(`LastLotteryOutcome`), "쉬지" 대사 미루기 |
| `TerminalView.cs` | 결과 글자(9시)를 쓰지 않음 |

## 줄별 설명 (면담 대비)
### GameManager.cs
- `ResolveBuy` — 상승이고 광대 차례면 `OneMore`("한 번 더."). 하락이면 맞은 사람(`Current`)이 광대면 `Hurt`("…칫."), 나면 `Laugh`("하하."). `PassTurn()` **전에** 말한다(PassTurn이 Current를 바꾸므로).
- `ResolveSell` — 하락: 광대가 넘겼으면(내가 맞음) `Gotcha`("잘 받았지?"), 내가 넘겼으면(광대가 맞음) `Hurt`. 상승: 광대 차례면 `NothingHappened`, 내 차례면 말 없음(Q8: 내 차례엔 피해 때만).
- `ClownTurnRoutine` 아이템 — 아이템 이름 → 0.8초 → `ItemResultLine`("다들 봤지?" 등) → 다음 생각(1초) 동안 떠 있음.
- `ItemResultLine` — `switch`로 아이템별 대사. 복권은 `LastLotteryOutcome`(실제로 된 것)으로.
- `UseLottery` — 처음에 `LastLotteryOutcome = Nothing`, 실제로 적용됐을 때만 그 상(Heal·Leverage·Item)으로 바꿈 → "이미 가득"이라 못 받으면 꽝 대사(Spec 제안 2).
- `ClownSay`가 `clownLineFrame = Time.frameCount`(지금 몇 번째 프레임인지)를 기억.
- `ClownSayAfterCurrent` — 같은 프레임에 이미 말했으면(예: 내가 매도+하락 → "…칫." → 바로 PassTurn에서 광대가 쉬는 차례) 덮어쓰지 않고 `SayLater`로 1.2초 뒤에 "…한 번 쉬지.". 다른 때는 바로.
- `ClownSilence` — 미뤄 둔 대사(`laterLine`)도 취소(내가 행동하거나 새 라운드면 늦게 튀어나오지 않게).

### TerminalView.cs
- 결과 글자에 `""`만 넣음(오브젝트는 Scene에서 끔). `LastResult`는 결과 화면·개발용 글자(F1)에서 계속 씀(Spec 제안 5).

## Scene (MCP)
- `ResultText` 끔.
- `ClownSpeech` 대사 표: 코드 기본값으로 19줄(이미 Scene에 저장된 표는 코드 기본값이 자동으로 안 바뀌어서, 잠깐 새 컴포넌트를 만들어 표를 복사하고 지움).
- 시안 고침 1번: 덱이 바닥나 재장전 연출이 바로 시작되면 말풍선이 어둡게 덮임(`result-gotcha-dimmed.png`) → `ClownSpeech`를 `DeckReveal` 바로 위 순서로(`result-over-reveal.png`). 확대 화면·결과·일시정지보다는 아래.

## 확인 (2026-11-22, MCP)
| | 결과 |
|---|---|
| C3·C4 | 판정 함수를 직접 불러 8가지: 나 매수+상승 말 없음 / 나 매수+하락 Laugh / 나 매도+하락 Hurt / 나 매도+상승 말 없음 / 광대 매수+상승 OneMore / 광대 매수+하락 Hurt / 광대 매도+하락 Gotcha / 광대 매도+상승 NothingHappened. 아이템 3종·복권 4종 결과 대사 맞음. 복권 잔고 +1을 가득 찬 잔고에 → 꽝 |
| 미루기 | 광대 정지 + 내 매도+하락 → "…칫." → 1.2초 뒤 "…한 번 쉬지." |
| 실제 흐름 | "넌 한 번 쉬어."(`result-halt.png`), "잘 받았지?"(`result-gotcha-dimmed.png` → 고친 뒤 `result-over-reveal.png`) |
| C6 | 왼쪽 결과 글자 없음(`no-result-text.png`) |
| C7 | Console 오류 0. 퇴행 T65~T68·T143은 아직 안 돌림 |

- 복권 결과 대사("운이 좋군." 등)는 gdd-changes #60("복권 당첨 내용을 화면 글자로 알리지 않음")을 바꿈 → 광대 말로만 알림(Grilling Q6 채택). #73에 적음.
- **남은 것(사용자):** 실제 플레이에서 대사가 너무 잦거나 거슬리지 않는지, 결과가 분명히 보이는지.
