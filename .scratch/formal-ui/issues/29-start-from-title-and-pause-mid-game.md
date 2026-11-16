# 29: 타이틀에서 시작하고, 게임 중 Esc로 멈춘다

**What to build:** 새 `TitleScene`(Build Settings 0번, GameScene 1번): 로고·떨어지는 빨간 그래프·부제·목표 한 줄·조작 요약·`Space 시작 / Esc 종료`·광대(스케치 화면 1). Space → GameScene, Esc → 종료(에디터는 Play 멈춤). GameScene `PausePanel`: 게임 중 Esc(아이템 확인 중이 아닐 때) → 패널 + `Time.timeScale = 0`(공개·광대 생각도 멈춤), `계속` → 1, `타이틀` → 1로 되돌리고 TitleScene. 결과 화면 `타이틀` 버튼도 연결.

**Blocked by:** 28 (결과 화면 타이틀 버튼)

**Status:** done (2026-11-16) — 작동 확인 완료. 실제 키·마우스는 사용자 직접 확인 남음

- [x] Play(TitleScene부터) → Space → GameScene 1라운드 (C13)
- [x] 타이틀 Esc → 종료 (C13)
- [x] 게임 중 Esc → 일시정지, 공개 중이면 캔들 공개도 멈췄다가 계속 뒤 이어짐 (C14)
- [x] 아이템 확인 중 Esc → 취소(일시정지 아님) (U3)
- [x] 일시정지·결과 `타이틀` → TitleScene, 다시 시작해도 시간 정상(timeScale 1) (C11, C14)
- [x] 타이틀 그림 팔레트 안 (C1), asset-rights
- [x] 퇴행 C17

Spec: `../spec.md` (타이틀·일시정지) · Grilling: `../grilling.md` (Q11) · 스케치 화면 1

## 시연 순서
1. 타이틀 → Space → 게임
2. 캔들 공개 중 Esc → 멈춤 → 계속 → 결과
3. Esc → 타이틀 → Space → 새 판

## Comments
- 2026-11-16 AI 작업(사용자 "커밋 후 29", 28 커밋 `8f56756`): 스케치 화면 1을 보고 `tools/make_art.py title` 추가 → 그림 5개(떨어지는 그래프(스케치에서 떼어 냄)·조작 상자·시작 버튼·일시정지 어둡게·일시정지 패널) + 미리보기, 팔레트 검사 OK. 규격 아트 메모 7-6절. **다음:** 사용자 확인 → TitleScene 만들기·일시정지 패널 배치(MCP) → 코드(TitleMenu·GameManager 일시정지·PausePanel·결과 화면 타이틀 버튼).
- 2026-11-16 사용자 "진행, AI가 작성": AI가 `draft/29`(GameManager 일시정지·GoToTitle, PlayerInputHandler Esc/우클릭 분기, 새 PausePanel·TitleMenu, ResultPanel 타이틀 버튼, UIManager)를 만들고 Scripts에 직접 씀(CLAUDE.md 4장 예외). 쓰기 전 Scripts = 마지막 커밋, 쓴 뒤 초안과 같음, 컴파일 오류 0.
- AI가 MCP로: 그림 5개 `Sprites/Title/`, GameScene `HUD/PauseScreen`(어둡게·패널·제목·계속·타이틀) + PausePanel 연결 → 저장. 새 `TitleScene`(카메라 Pixel Perfect 320×180 Windowbox, EventSystem, Canvas 320×180, 그래프·로고·부제·목표·조작 상자·Space 시작 버튼·Esc 종료 버튼·광대, TitleMenu) → 저장. **Build Settings: 0 TitleScene, 1 GameScene.**
  - 발견·수정: 새 Scene을 처음 저장할 때 TMP 글자 색이 파일에 흰색으로 저장됨(Play 뒤 "변경됨"으로 발견, 복사본 비교) → 다시 저장해 색 반영 확인.
- 테스트(AI가 MCP로, TitleScene에서 Play):
  - T182 ✅ 시작 버튼 → GameScene 1라운드, timeScale 1 (C13)
  - T183 ✅ 일시정지: 패널 켜짐, timeScale 0, 버튼 회색, 매도·Space·1번 모두 무시 / 계속 버튼 → 풀림 (C14)
  - T184 ✅ 공개 0.1초 뒤 멈춤 → 실제 2초 동안 공개 중 그대로(덱 장수 그대로) → 계속 → 공개 끝나고 판정 (C14)
  - T185 ✅ 아이템 확인 중에는 일시정지 안 됨(Esc는 취소 경로) (U3)
  - T186 ✅ 일시정지 → 타이틀 → TitleScene, timeScale 1 → 시작 → GameScene 정상 (C14)
  - T187 ✅ 결과 화면(파산) → 일시정지 안 됨, 타이틀 버튼 켜짐 → TitleScene (C11)
  - T188 ✅ 퇴행 T65~T68·T143 모두 같음, Console 오류 0
  - 캡처 `.scratch/art/captures/29-title.png`, `29-pause.png`
  - △ 실제 Esc(멈춤/계속)·우클릭(취소만)·Space(타이틀 시작)·Esc(타이틀 종료) 키 입력은 사용자 확인 필요(T189)
