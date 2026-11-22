# draft 88: 타이틀부터 음악, 시작해도 안 끊김

> 2026-11-22. 사용자 "타이틀에서 노래가 흘러나오게, 시작을 해도 노래가 안 끊기는 식으로". 코드는 AI가 Scripts에 직접(4장 예외).

- 새 `MusicPlayer.cs`: `DontDestroyOnLoad`(씬이 바뀌어도 남는 오브젝트) 하나가 인트로+루프 곡을 이어 틂. 이미 있으면 새 씬의 것은 `Destroy`(곡이 겹치거나 처음부터 다시 안 나오게). 크기 = 설정 전체 × 음악 × 0.35 × 줄임(1).
- `AudioManager.cs`: 음악 부분을 MusicPlayer로 옮기고 효과음만. `SetRevealDuck`은 MusicPlayer에 비율만 넘김.
- `TitleMenu.cs`: 설정에서 소리를 바꾸면 `MusicPlayer.Instance.ApplyVolume()`.
- Scene(MCP): TitleScene·GameScene 맨 위에 `MusicPlayer`(GameScene 것은 바로 Play용 대비 — 타이틀에서 오면 스스로 없어짐), GameScene `Audio`의 안 쓰는 음악 재생기 2개 지움.

## 확인 (MCP)
- 타이틀: 재생 중, 크기 0.35, 플레이어 1개.
- 시작 → 게임: 같은 플레이어(타이틀 것), 재생 시간 8.28 → 9.61초 이어짐, 플레이어 1개.
- 게임 → 타이틀: 14.96 → 18.23초 이어짐.
- 설정 음악 −10% → 0.315, 되돌림 → 0.35.
- 퇴행: T65 ✅(0/1 → 다시하기 2/2), T143 ✅(공개 중 0.35 = 평소), T66 ✅(2/0 → R2) — 2026-11-22. T67 ✅(재장전 뒤 광대 -3 자멸 클리어), T68 ✅(클리어 뒤 광대 행동 없음 → R2) — 2026-11-23. Console 오류 0, Scene 변경 없음.
