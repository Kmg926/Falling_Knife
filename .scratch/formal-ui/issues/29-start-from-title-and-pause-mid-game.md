# 29: 타이틀에서 시작하고, 게임 중 Esc로 멈춘다

**What to build:** 새 `TitleScene`(Build Settings 0번, GameScene 1번): 로고·떨어지는 빨간 그래프·부제·목표 한 줄·조작 요약·`Space 시작 / Esc 종료`·광대(스케치 화면 1). Space → GameScene, Esc → 종료(에디터는 Play 멈춤). GameScene `PausePanel`: 게임 중 Esc(아이템 확인 중이 아닐 때) → 패널 + `Time.timeScale = 0`(공개·광대 생각도 멈춤), `계속` → 1, `타이틀` → 1로 되돌리고 TitleScene. 결과 화면 `타이틀` 버튼도 연결.

**Blocked by:** 28 (결과 화면 타이틀 버튼)

**Status:** ready-for-agent

- [ ] Play(TitleScene부터) → Space → GameScene 1라운드 (C13)
- [ ] 타이틀 Esc → 종료 (C13)
- [ ] 게임 중 Esc → 일시정지, 공개 중이면 캔들 공개도 멈췄다가 계속 뒤 이어짐 (C14)
- [ ] 아이템 확인 중 Esc → 취소(일시정지 아님) (U3)
- [ ] 일시정지·결과 `타이틀` → TitleScene, 다시 시작해도 시간 정상(timeScale 1) (C11, C14)
- [ ] 타이틀 그림 팔레트 안 (C1), asset-rights
- [ ] 퇴행 C17

Spec: `../spec.md` (타이틀·일시정지) · Grilling: `../grilling.md` (Q11) · 스케치 화면 1

## 시연 순서
1. 타이틀 → Space → 게임
2. 캔들 공개 중 Esc → 멈춤 → 계속 → 결과
3. Esc → 타이틀 → Space → 새 판

## Comments
