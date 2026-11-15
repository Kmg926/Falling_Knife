# 24: 테이블 너머에 도트로 그린 광대가 앉아 있다 (아트 방향 메모 + 기준 애셋)

**What to build:** 11주차 1교시 체크포인트. 사이트 형식(7항목)으로 `.scratch/art/art-direction.md`를 쓴다(기준 이미지·시점·표현 방식·색·넣지 않을 것·고른/버린 시안·파일 규격 숫자, 확인한 것과 정하지 않은 것 구분). 스케치 플레이 화면에서 16색 이하 팔레트를 뽑아 `tools/`에 고정한다. AI가 `tools/make_art.py`로 광대(평소 모습) 시안 여러 개를 `.scratch/art/drafts/`에 만들면 사용자가 고르고 보정해 `Assets/_Project/Sprites/`에 넣는다. GameScene에 SpriteRenderer로 놓고 Main Camera에 Pixel Perfect Camera(320×180, PPU 1)를 붙여 Play 화면에 보이게 한다. 텍스트 UI는 아직 그대로(겹쳐 보여도 됨).

**Blocked by:** 없음

**Status:** ready-for-agent

- [ ] 아트 방향 메모 7항목, 광대 파일 규격 숫자 확정(스케치를 4로 나눠 잼, 제안 3)
- [ ] 기준 이미지는 스케치 머리글(이름·학번)을 잘라 게임 화면 부분만 저장
- [ ] 팔레트 ≤ 16색, 광대 PNG가 팔레트 안 색만 씀 (C1)
- [ ] 광대 시안 여러 개 → 사용자가 고르고 보정 → Sprites (Point·PPU 1·압축 없음)
- [ ] Game 뷰 1280×720에서 4배, 흐림 없음 (C2) + 지금 화면 캡처와 나란히 비교
- [ ] `docs/asset-rights.md` + AI 제작 로그(목적·도구·입력·결과·판단)
- [ ] 퇴행: 판정·소리 그대로, Console 오류 0 (C17)

Spec: `../spec.md` (아트 방향 메모, 팔레트, 그림 만들기, 카메라, 제안 1~3) · Grilling: `../grilling.md` (Q1, Q4, Q5, Q7)

## 시연 순서
1. 아트 방향 메모 읽기 → 팔레트 그림
2. 시안들과 고른 것·버린 이유
3. Play → 테이블 너머 광대 (이전 캡처와 비교)

## Comments
- 2026-11-16 AI 작업(사용자 "커밋 후 24 시작"): 스케치 5장을 게임 화면 부분만 잘라 320×180으로 줄여 `.scratch/art/reference/`(머리글의 이름 없음). 팔레트 16색 `tools/palette.txt`. `tools/make_art.py clown`으로 광대 시안 A(스케치 그대로)·B(위쪽 조명 테두리)·C(아래 그림자) + `drafts/clown-preview-x4.png`, 팔레트 검사 3개 모두 OK. 처음 만든 B(잡티 정리)는 눈·입 디테일이 지워져 버림. 아트 방향 메모 `.scratch/art/art-direction.md`(7항목, 광대 규격 96×96·몸 70×87·기준점 아래 가운데). **남은 것:** 사용자가 시안 고르기·나비넥타이 색 결정·보정 → Sprites → Scene(MCP) → Play 캡처 비교 → asset-rights.
- 2026-11-16 사용자 요청 "모든 안을 페니와이즈처럼" → AI가 특정 캐릭터 재현이라 거절·대안 제시 → "머리 모양만 가운데 비워" → `-bald` 판 3개 추가(`bald_top`). 사용자 선택: **A(원래 머리)**, 나비넥타이 회색 그대로.
- 2026-11-16 "먼저 넣기"(보정 전, AI가 MCP로): Scene "변경됨" → 복사본 비교로 차이 0 확인. `clown-A.png` → `Assets/_Project/Sprites/Clown/clown-idle.png`(Sprite·PPU 1·Point·압축 없음·밉맵 끔·기준점 아래 가운데). Main Camera에 Pixel Perfect Camera(320×180, PPU 1), 배경색 파랑 → `bg0 #0E1016`(아트 메모 4절). `Clown` 오브젝트(SpriteRenderer) 위치 (0, −10) = 스케치 (160, 100) → 저장.
  - ✅ C2: Game 뷰(1610×788, Free Aspect)에서 Pixel Ratio 4, 광대 너비 280px = 70도트 × 4, 흐림 없음. 1280×720 정확한 화면은 Game 창 해상도를 1280×720으로 두고 사용자 확인 필요.
  - ✅ 비교 캡처: `.scratch/art/captures/24-before-text.png`(Play 중에만 광대 끄고 원래 파랑) / `24-after-clown.png` / 나란히 `24-compare.png`.
  - ✅ C1 팔레트 검사 OK. Console: 새 오류 0(목록의 CS1061은 22 반영 중 옛 기록, 나머지 1개는 AI의 잘못된 캡처 경로 요청). 코드 변경 없음 → 판정 퇴행 테스트는 생략.
  - AI 실수: 첫 캡처를 `Assets/.scratch/…`에 저장(도구 기준 경로가 Assets) → 점 폴더라 .meta 없음, 바로 지움.
  - **남은 것:** 고른 이유 한 줄(사용자), 보정(사용자, 같은 이름으로 덮어쓰기), 1280×720 직접 보기.
