# tutorial — Grilling (2026-11-23)

> 사용자: "튜토리얼 스테이지를 만들어줘. 형식은 상관없어. 좋은 튜토리얼 예시를 서치해서 적용시켜줘."
> AI가 사례를 조사해 질문마다 추천안을 냈다. **2026-11-23 사용자 "전부 채택"** — 이유: 사례 근거(해 보면서 배우기·한 번에 하나·각본 첫 판)를 그대로 따름.

## 조사 요약
| 사례 / 원칙 | 내용 | 가져올 점 |
|---|---|---|
| 해 보면서 배우기 | 읽기보다 직접 해 볼 때 잘 배움(Celeste·Portal: 판 자체가 튜토리얼) | 설명 한 줄 → 바로 그 행동 |
| 한 번에 하나 | 복잡한 규칙은 잘게 나눠 순서대로, 처음에 몰아넣지 말 것 | 단계마다 규칙 1개 |
| 즉시 피드백 | 행동 결과를 바로 분명하게 | 결과가 나오면 광대가 한 줄로 짚어 줌 |
| 실제 플레이 순서대로 | 튜토리얼 순서 = 실제 한 판 순서 | 덱 보기 → 매수 → 매도 → 광대 차례 → 아이템 |
| Buckshot Roulette 1라운드 | 상대(딜러)가 규칙을 말하고, 첫 라운드는 아이템 없이 + 위험 낮게 | 광대가 설명, 기본 행동 먼저·아이템 나중, 지지 않음 |
| Inscryption 첫 판 | 각본대로 흘러가는 첫 대결로 규칙을 하나씩 공개 | 덱 순서·아이템 고정(각본). 단, 지나친 각본은 비판받음 → 짧게 |

## 질문과 추천안
| Q | 질문 | 추천안 | 사용자 답 |
|---|---|---|---|
| Q1 | 어떤 형태? | **타이틀 메뉴에 `튜토리얼` 추가**(시작 아래). GameScene을 그대로 쓰는 튜토리얼 판 1라운드 | 채택 |
| Q2 | 진행 방식? | **해 보면서 배우기**: 단계마다 광대가 대화창(시작 스토리와 같은 창)으로 1~2줄 → 그 행동만 할 수 있음 → 해야 다음 단계 | 채택 |
| Q3 | 결과를 정해 둘까? | **각본**: 덱 순서·아이템 고정(이미 있는 고정 순서 장치를 튜토리얼 모드에서만). 매번 같은 결과 | 채택 |
| Q4 | 단계 구성? | ① 단말기: 남은 캔들·하락/상승 개수 ② 매수 + 상승 → 레버리지 +1, 내 차례 계속 ③ 매수 + 하락 → 내 잔고 −(1+레버리지), 레버리지 0 ④ 매도 + 하락 → 광대 잔고 −1 ⑤ 광대 차례 구경(광대 매수·매도) ⑥ 폰 열기 → 차트 분석으로 다음 캔들 보기 → 보고 결정 ⑦ 마무리 한 수로 광대 잔고 0 → 끝 | 채택 |
| Q5 | 나머지 아이템(찌라시·거래 정지·복권·배당금·구제 금융)? | **⑥ 뒤 폰 화면 설명으로 한 줄씩만**(직접 쓰기는 실전에서). 한 번에 몰아 가르치지 않기 | 채택 |
| Q6 | 질 수 있나? | **지지 않음**: 튜토리얼 잔고는 줄어도 0이 되지 않게 각본(Buckshot 1라운드처럼 위험 낮음) | 채택 |
| Q7 | 지금 단계가 아닌 입력은? | **막고, 해야 할 곳을 강조**: 금색 테두리 깜빡임(모니터·버튼·폰) + 대화창에 "Space로 실행" 같은 키 안내 | 채택 |
| Q8 | 건너뛰기·나가기? | 시작 스토리와 같은 **Space 길게 = 건너뛰기**(→ 타이틀), Esc 일시정지에서 `타이틀` | 채택 |
| Q9 | 끝나면? | "이제 진짜다." → **`시작`(실전) / `타이틀`** 고르기 | 채택 |
| Q10 | 처음 하는 사람에게 권할까? | **처음 `시작`을 누르면 한 번만** "튜토리얼부터 볼래?" 예/아니오(저장, 다시 안 물음) | 채택 |
| Q11 | 길이 | **2~3분**(7단계, 단계당 20초 안) | 채택 |
| Q12 | 코드 작성 | 지금까지와 같이 **AI가 draft + Scripts 직접**, 줄별 설명 README | 채택 |

## 정할 것 다음 단계
- 답을 받으면 Spec(`spec.md`) → Ticket(`issues/`) → 구현.

## 출처
- https://www.gamedeveloper.com/design/how-to-make-game-tutorials-better-immediate-feedback
- https://www.gamedeveloper.com/game-platforms/6-tips-for-designing-a-game-tutorial
- https://medium.com/super-jump/making-tutorials-better-31116c02687b
- https://gamedeveloper.com/design/examining-organic-tutorials
- https://www.gamedeveloper.com/game-platforms/the-4-required-parts-of-any-game-tutorial
- https://www.casino.org/blog/how-to-play-buckshot-roulette-rules-items-strategy/
- https://www.ludo.guide/guide/buckshot-roulette/main-walkthrough/the-first-shot/the-first-round
- https://www.ludo.guide/guide/inscryption/walkthrough/first-encounter-tutorial
- https://dmullinsgames.itch.io/sacrifices-must-be-made/comments
