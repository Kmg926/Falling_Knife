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
