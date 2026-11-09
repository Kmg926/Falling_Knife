# Falling Knife — Claude 작업 메모

> 이 파일은 Claude Code가 세션을 시작할 때 자동으로 읽는다.
> Cowork(클라우드) 세션에서 이어 온 내용을 정리했다. 마지막 갱신: 2026-11-02

## 0. 나에 대해 · 대화 방식
- 대학 수업(Unity / C#) 기말 프로젝트. **이름·학번·학교·과목·로컬 경로는 `CLAUDE.local.md`(Git 제외)에 있다.** 이 저장소는 공개이므로 추적 파일에 개인 정보를 쓰지 않는다.
- **한국어, 짧게.** 결론 먼저, 표·번호 목록 위주. 코드는 필요한 부분만 짧게.
- Unity 메뉴 위치·버튼 이름은 정확히 쓴다. 내가 모르는 용어(Int/Float, Controller 등)는 비유로 풀어서 설명한다.
- 내가 "진행"이라고 하면 다음 단계를 순서 있는 체크리스트로 준다.

## 1. 이 프로젝트
- **게임:** Falling Knife — 턴제 위험 선택 대전 (Buckshot Roulette 구조, 코인 차트 소재)
- **Unity 프로젝트 루트:** `C:\Falling_Knife` (이 폴더)
- **기획서(GDD, 기준 문서):** 중간고사 GDD v1.6 (PDF 제출본, 경로는 `CLAUDE.local.md`)
  - 화면 스케치: 같은 폴더 `week08_FallingKnife_화면스케치_v2.pdf`
- **강의 실습 프로젝트(별개):** `C:\GameEngineI` — 주차별 실습용. 여기서 만든 코드(PlayerAnimation, PlayerProgress 등)는 참고만 한다.
- **강의 자료:** 수업 사이트(주소는 `CLAUDE.local.md`, 주차별 `week-NN-periodN.html`). **로컬 md보다 사이트가 최신**이다.

## 2. 게임 규칙 요약 (GDD 04장 기준, 숫자는 테스트로 조정)
- 1인칭 고정 화면, 320×180 도트를 4배 → 1280×720. 플레이어 vs 광대(AI). 3라운드.
- 한 판 3~5분. 성공: 3라운드에서 광대 잔고 0 / 실패: 어느 라운드든 내 잔고 0
- **행동:** 보유(나에게) / 매도(상대에게). 숨겨진 캔들 덱에서 1장 공개.
  - 보유+상승: 레버리지 +1, 턴 유지 / 보유+하락: 내 잔고 −(1+레버리지), 레버리지 0, 턴 넘김
  - 매도+하락: 상대 잔고 −(1+레버리지), 레버리지 0, 턴 넘김 / 매도+상승: 변화 없음, 턴 넘김
- **레버리지:** 공용, 0~3. 라운드 시작 시 0.
- **라운드별:** 잔고 2/3/4, 덱(하락:상승) 2:3 / 3:4 / 4:4. 덱 소진 시 같은 구성 재장전 + 아이템 2개씩.
  - **변경 결정(2026-11-09, gdd-changes #23~26):** 잔고 10/10/10, 덱은 채울 때마다(라운드 시작·재장전) 총 장수 2~6 / 4~8 / 7~9(처음 4~6 / 6~8 / 7~9, #28)와 하락 수(1 ~ 총−1)를 랜덤. 개수 공개·순서 숨김 유지. 구현은 `.scratch/random-deck/`
- **아이템 4종 (최대 4개):** 차트 분석(다음 캔들 나만 확인), 찌라시(다음 캔들 공개 후 버림), 거래 정지(상대 턴 1회 건너뜀), 복권(25%씩: 잔고+1 / 레버리지+1 / 아이템+1 / 꽝)
- **처리 순서:** 피해 적용 → 잔고 0 판정 → 덱 소진 확인 (파산이 재장전보다 먼저)
- **입력:** A/D·←/→로 보유·매도 선택, Space/클릭 실행, 1~4 아이템 확인, Space 사용, Esc/우클릭 취소. 캔들 공개(1초)·광대 턴엔 입력 무시.
- **광대 AI:** 차트 분석 있으면 사용→결과대로 / 거래 정지 사용 / 잔고 1이면 복권 / 그 외 남은 덱 하락 비율 50% 이상이면 매도·찌라시, 미만이면 보유
- **미정:** 남은 하락·상승 개수를 화면에 계속 보여줄지 (테스트 후 결정)
  - 2026-11-09: 지금 가운데 텍스트는 **개발용 시스템 UI**(아트·정식 UI가 생기면 없앰). 그래서 이 결정은 **정식 UI(단말기) 만들 때** 한다. 기억력 재미를 위해 행동 로그 자체를 없앴다(gdd-changes #29). 단말기의 "공개 기록"도 같은 이유로 정식 UI 때 다시 판단

## 3. 구현 계획 (GDD 08·09장)
- Scene: `TitleScene`, `GameScene` (결과·일시정지는 GameScene 안 패널)
- 주요 스크립트: GameManager, CandleDeck, LeverageCounter(GDD의 StackCounter, ADR 0001), Participant, PlayerInputHandler, ClownAI, UIManager, ItemCloseupPanel, AudioManager, ItemData/ItemEffect(ScriptableObject)
- 수업에서 안 배우는 것(직접 학습): 코루틴, ScriptableObject, 셔플, Scene 전환, Pixel Perfect Camera
- **마일스톤:** ① 텍스트 UI로 덱·보유·매도·레버리지·잔고·턴, 1라운드 완주 → ② 아이템·AI·3라운드·정식 UI → ③ Alpha + 3인 플레이테스트 → ④ 수정·제출 후보 → ⑤ 최종 빌드
  - GDD는 원래 주차(9~16주)로 적혀 있지만 **연휴로 실제 수업이 약 2주 밀렸다.** 실제 공지에 맞춰 조정 필요.
- **범위 밖:** 멀티플레이, 상점, 추가 아이템, 여러 상대, 실제 시세 연동

## 4. 작업 원칙
- **9주차 흐름을 따른다: Grilling → Spec → Ticket → 구현.** Grilling·Spec·Ticket 단계에서는 구현하지 않는다.
  - Spec: `.scratch/<기능>/spec.md` (7개 섹션) / Ticket: `.scratch/<기능>/issues/` (파일 1개 = Ticket 1개)
  - Ticket 제목은 플레이어 행동으로. `Blocked by`는 실제 선행만. Status: `ready-for-agent` / `ready-for-human` / `needs-info`
- **합의하지 않은 내용을 사실처럼 채우지 않는다.** 확인한 사실 / 내가 정한 결정 / 아직 가정인 것을 구분해서 말한다. 모르면 "확인하지 못했다".
- **코드 작성 범위 (기말 코드 구술 면담 있음, 확정 2026-11-02):** AI 초안 + 내가 재작성
  - AI는 Ticket 단위로 초안 코드를 `.scratch/<기능>/draft/`에 주고, **바뀐 줄마다 왜 그렇게 동작하는지** 설명한다.
  - 내가 직접 타이핑·수정해서 `Assets/_Project/Scripts/`에 반영한다. AI는 Scripts 폴더에 직접 쓰지 않는다.
  - 반영 후 AI는 내 코드를 리뷰만 한다(초안과 다른 점, 버그 가능성).
  - **예외 기록:** Ticket 01 스크립트 5개(CandleDeck·Participant·LeverageCounter·GameManager·UIManager)는 내 지시로 AI가 초안 그대로 Scripts에 생성했다 (2026-11-02). 이유: Parsec 원격 접속의 네트워크 지연 때문에 그때 직접 타이핑할 수 있는 환경이 아니었다. 면담 대비로 draft/01/README의 줄별 설명을 직접 읽고 설명할 수 있어야 한다. 02부터는 원래 규칙대로 한다(따로 지시하면 예외).
    - Ticket 02(스크립트 6개 + `Input/FallingKnife.inputactions`)도 내 지시로 AI가 초안 그대로 생성했다 (2026-11-02). 이유: 01과 같음(Parsec 원격 접속 네트워크 지연으로 직접 타이핑할 수 없는 환경).
    - Ticket 03(`GameManager.cs`)도 같은 이유로 내 지시에 따라 AI가 초안 그대로 반영했다 (2026-11-02).
    - Ticket 04(`Participant.cs`, `GameManager.cs`)도 같은 이유로 내 지시에 따라 AI가 초안 그대로 반영했다 (2026-11-02).
    - Ticket 05(`GameManager.cs`, `UIManager.cs`, `PlayerInputHandler.cs` + Restart Action 칸 연결)도 같은 이유로 내 지시에 따라 AI가 반영했다 (2026-11-02).
    - 2026-11-02 Parsec 원격 접속을 끝내고 메인 컴퓨터로 접속했다. 이후 Ticket부터는 원래 규칙(내가 직접 타이핑)으로 돌아간다.
    - Ticket 15(로그 제거: `GameManager.cs`, `UIManager.cs`에서 Ticket 14 코드 삭제 → Ticket 13 버전 `2ede120`과 같은 내용)는 내 지시로 AI가 반영했다 (2026-11-09). 이유: 여러 곳에 흩어진 줄을 내가 지우다 일부만 지워 불안정해질 것 같아서("이번에 한해서"). 새로 쓴 코드는 없고 지우기만 했다. 이후는 다시 원래 규칙.
    - Ticket 16의 Input Actions(`Item1`~`4`·`Cancel`을 `FallingKnife.inputactions`에 추가, PlayerInputHandler 칸 재연결, 기본 파일 `Assets/Settings/InputSystem_Actions.inputactions`에 잘못 들어간 `Game` 맵 되돌리기)와 ItemData 에셋 4개는 내 지시로 AI가 MCP로 했다 (2026-11-09). 이유: 편집 창에서 두 파일의 `Game` 맵이 헷갈려 기본 파일에 들어갔고, 다시 해도 저장되지 않았다. 스크립트 6개는 내가 직접 타이핑.
- **파일을 바꾸기 전에 무엇을 바꿀지 먼저 말하고 허락받는다.** 바꾼 뒤에는 다시 읽어서 실제로 반영됐는지 확인하고 결과를 알려준다. (확인 없이 "완료"라고 하지 않는다.)
- **Unity 파일 안전 규칙**
  - `.meta`, `Library/`, `Temp/`, `Logs/`는 건드리지 않는다. 파일 이동·이름 변경은 Unity Project 창에서 하도록 안내한다(.meta가 같이 움직여야 연결이 유지됨).
  - `.unity`/`.prefab` YAML을 직접 고치지 않는다. MCP·Pipeline으로 Editor를 통해 바꾸거나, 내가 Inspector에서 할 순서를 알려준다. (간단한 수치 변경은 내가 허락하면 예외)
  - 씬 상태를 읽기 전에 내가 **Ctrl+S로 저장했는지** 확인한다. Editor가 열려 있을 때 파일을 바꾸면 Reload 안내를 한다.
  - 상태 확인 요청에는 읽기 전용으로 동작한다("변경하지 마").
- **폴더 구조:** `Assets/_Project/{Scenes, Scripts, Prefabs, Sprites, Audio, Input, Data}` 로 처음부터 정리한다.

## 5. 기록 (기말 평가 15점: AI 투명성·선택 근거·권리)
- 도움을 줄 때마다 묻지 않고 아래 파일에 한 줄 추가한다:
  작업기록 파일 `_작업기록.md` (경로는 `CLAUDE.local.md`)
  `| 날짜 | 과제 | AI가 도운 내용 | 내가 직접 한 부분 |`
- Grilling에서 내가 AI 추천을 채택·거절한 **이유**를 Spec이나 `docs/adr/`에 남기게 돕는다.
- AI 생성 에셋(광대·아이템·배경)은 도구·프롬프트·직접 보정한 부분·권리 조건을 기록한다. 폰트 후보: Galmuri (SIL OFL 1.1).
- **GDD(v1.6)는 제출본이라 수정 불가.** GDD와 달라지는 결정(변경·구체화·임시)이 생길 때마다 묻지 않고 `docs/gdd-changes.md` 표에 한 줄 추가한다. 최종 제출 때 이 표를 쓴다.

## 6. 진행 현황 (마지막 갱신 2026-11-09)
- [x] 첫 세션 설정: 코드 작성 범위 확정, Unity 6.6(6000.6.2f1) 통일(GDD는 `docs/gdd-changes.md` #1), 9주차 1교시 설치(CLI beta.15·Pipeline·MCP `unity-editor-mcp`·스킬) 확인. 자세한 과정은 `_작업기록.md`
  - 주의: 앱 터미널(PowerShell)에서는 `npx` 대신 `npx.cmd`. 명령은 `C:\Falling_Knife`에서 실행
  - 주의: Unity를 다시 켜면 MCP 연결이 끊길 수 있다 → Unity가 완전히 열린 뒤 다시 확인
- [x] **마일스톤 ① 완료: 캔들 덱 + 보유/매도 한 턴** (`.scratch/candle-deck-turn/`: spec·issues 01~05·draft 01~05, 모두 done)
  - 01 덱을 섞고 남은 개수를 본다 / 02 보유 → 1초 공개 → 판정 / 03 매도 / 04 덱 소진 시 재장전 / 05 승패·R 재시작
  - 코드는 Parsec 지연 때문에 AI가 반영(4장 예외 기록). 다음 Ticket부터 직접 타이핑
- [x] 되돌아갈 지점: Git 저장소(`main`) (2026-11-09, 9주차 1교시 6단계). Ticket 구현 전마다 커밋한다
  - 원격: GitHub **Public** 저장소(주소는 `CLAUDE.local.md`). 개인 정보는 `CLAUDE.local.md`로 분리했고, 공개 전 기록은 개인 정보 없는 새 커밋 하나로 다시 만들었다. push는 내가 요청할 때만
- [x] **10주차 완료: 3라운드 진행** (`.scratch/three-rounds/`: grilling·spec·analysis·issues 06~08·draft 06~08·`making-log.md`, 모두 done, 커밋 `cc6fff4`)
  - 코드는 내가 직접 타이핑. 공개 시간 0.5초(gdd-changes #20)
  - 경험: 텍스트 UI라 라운드 전환 느낌 약함 → 정식 UI 과제 / 클리어 화면 `차례: 광대` 보류
  - 주의: MCP 테스트 전 컴파일 시각이 스크립트 저장 시각보다 나중인지 확인 (Unity 포커스 없으면 재컴파일 안 됨)
  - 임시: Run In Background 켜짐(MCP 테스트용) → 최종 빌드 전 끄기
- 다음 할 일
  - [x] 9주차 보충: Grilling 대화 기록(`.scratch/candle-deck-turn/grilling.md`), Ticket "시연 순서" (2026-11-09)
  - [x] **광대 행동** (`.scratch/clown-ai/`): Ticket 09·10 done (경험 확인은 보류)
  - [x] **덱 개수 랜덤 + 잔고 10** (`.scratch/random-deck/`): Ticket 11·12 done. 잔고 10은 임시 유지, 경험 판단은 행동 로그 뒤
  - [x] **광대 속도 + 행동 로그** (`.scratch/action-log/`): 13(광대 생각 1초·결과에 행동한 사람) done, 14(로그) 철회 → 15 로그 제거 done (`81e03f1`). 15는 내 지시로 AI가 지우기만 함(4장 예외)
    - 지금 값: 잔고 10/10/10, 덱 범위 2~6 / 4~8 / 7~9, 공개 0.5초(`Reveal Seconds`), 광대 생각 1초(`Clown Think Seconds`)
    - 주의: AI는 Scene 저장 전에 내가 Inspector에서 바꾼 값이 있는지 먼저 확인한다
  - [ ] **마일스톤 ② 아이템** (`.scratch/items/`): Grilling 완료(Q1~Q17) → Spec → Ticket 16 찌라시·지급 / 17 차트 분석 / 18 거래 정지 / 19 복권 / 20 광대 아이템
  - [ ] 11주차: Comfy Cloud 아트 시안(유료 구독 필요 → 내 결정), 시점·스타일 비교, AI 제작 로그
  - [ ] 정식 UI(단말기·잔고·레버리지 그림). 남은 개수·공개 기록 표시 여부는 이때 결정
  - [ ] 나중 과제: 더 고차원적인 광대 규칙(사용자 메모)
  - [ ] 구술 면담 대비: `draft/01`~`15` README의 줄별 설명을 읽고 내 말로 설명할 수 있게 하기
  - [ ] GDD 원문 대조: PDF 텍스트 추출로 가능해짐. `docs/gdd-changes.md` "원문 확인" ☐ 21줄 남음
  - [ ] 아직 확인 못 함: 3라운드 패배 직접 확인(재장전 2번 이상은 자동 대전에서 확인됨)

## 7. 지금까지의 진행 상황 (강의 실습, `C:\GameEngineI`)
- 4주차 점프 코스, 5주차 수집·위험·목표(영상 제출 완료) 끝.
- 6주차: 1교시 Clip 5개 완료. 2교시 Animator 파라미터·전환·대시 연결 중. 남은 것: Any State→Fall 조건 `Less`→`Equals` 수정 확인, Ground Layer + PlayerAnimation 연결, Cinemachine 설치·추적·Confiner, 3교시 테스트 T01~T08·영상·CSV.
- 실습 씬 이름은 `WeekNN_Prac`/`WeekNN_Sub` 형식으로 쓰고 있다(사이트 이름과 다를 수 있음).

## Agent skills

### Issue tracker
로컬 Markdown: `.scratch/<기능>/` 아래 spec.md + issues/NN-*.md. See `docs/agents/issue-tracker.md`.

### Triage labels
기본 5개 라벨 그대로 사용 (`Status:` 줄에 기록). See `docs/agents/triage-labels.md`.

### Domain docs
single-context: 루트 `GLOSSARY.md` + `docs/adr/`. See `docs/agents/domain.md`.
