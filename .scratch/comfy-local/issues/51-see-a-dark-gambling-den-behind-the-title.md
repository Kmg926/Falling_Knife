# 51: 타이틀에 어두운 도박판 일러스트가 깔린다

**What to build:** AI가 `tools/`에 생성 스크립트(워크플로 `tools/comfy/title-bg.json`을 로컬 API로 보내고, 그림과 같은 이름 `.json`에 프롬프트·시드·모델·LoRA·크기·걸음 수를 `.scratch/art/drafts/comfy/`에 저장)와 후처리 스크립트(A: 640×360으로 줄여 32색 / B: 320×180으로 줄여 32색 뒤 2배, 각각 dither 켬·끔)를 쓴다. 사람·글자 없는 배경(어두운 방, 전등 하나의 노란빛, 테이블, 벽의 떨어지는 빨간 차트, 왼쪽은 제목 자리로 어둡게)을 1344×768로 시드 4개 생성 → 후처리 → 지금 타이틀과 한 장에 비교. 마음에 드는 게 없으면 사용자가 말로 고칠 곳을 주고 다시(최대 3번). 사용자가 고르면 `Sprites/Title/title-bg.png`(640×360, PPU 2)를 TitleScene 맨 뒤에 넣고(MCP), 지금 빨간 차트 선(`title-graph`)을 끌지는 비교 캡처로 사용자가 정함. 광대·글자·버튼은 그대로. 채택이든 거절이든 끝에 ADR 0004·CLAUDE.md 4장 `tools/` 문단·asset-rights·gdd-changes #67을 갱신.

**Blocked by:** 50

**Status:** ready-for-agent

- [ ] 생성 스크립트·워크플로, 시드 4개 생성, 그림마다 설정 `.json`
- [ ] 같은 워크플로·시드로 다시 만들면 같은 그림 (C2)
- [ ] 후처리 A·B(dither 켬·끔) 결과 640×360·32색·PPU 2 (C3)
- [ ] 지금 타이틀과 나란히 비교 → 사용자 선택(또는 지금 그대로)
- [ ] (고른 경우) TitleScene 배경 적용, 글자·버튼 읽힘, 시작 버튼으로 GameScene 진입 (C4)
- [ ] GameScene 변화 없음, Console 오류 0 (C5)
- [ ] asset-rights(도구·모델·LoRA·라이선스·프롬프트·시드)·ADR 0004·CLAUDE.md·gdd-changes #67 (C6)

Spec: `../spec.md` (제안 3~9) · Grilling: `../grilling.md` (Q1·Q2·Q5~Q9)

## 시연 순서
1. 타이틀을 켬 → 어두운 방·전등 빛·떨어지는 차트 배경 위에 제목·광대·버튼
2. 제목·조작 안내가 잘 읽힘
3. Space로 시작 → 게임 화면은 전과 같음

## Comments
