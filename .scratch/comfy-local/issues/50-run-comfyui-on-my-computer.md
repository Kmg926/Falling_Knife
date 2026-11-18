# 50: ComfyUI가 내 컴퓨터에서 돈다

**What to build:** 사용자가 공식 배포처에서 **ComfyUI Desktop**(Windows 설치 파일)을 설치하고, 기본 모델 **SDXL base 1.0** + 도트 그림 LoRA 1개를 받는다. AI는 받기 전에 LoRA 후보 2~3개를 찾아 라이선스(비영리 사용·결과물 공개 허락)·예시와 함께 보고하고, SDXL 라이선스도 배포 페이지에서 확인해 알린다. 설치 뒤 AI가 로컬 API 포트를 찾아 모델·LoRA 목록을 읽어 연결을 확인한다(C1). AI는 실행 파일·모델을 직접 받지 않는다.

**Blocked by:** 없음

**Status:** ready-for-human

- [ ] AI: SDXL base 1.0 라이선스 확인·보고
- [ ] AI: 도트 LoRA 후보 2~3개(이름·배포 페이지·라이선스·예시) 보고 → 사용자 선택
- [ ] 사용자: ComfyUI Desktop 설치, SDXL base 1.0·고른 LoRA를 ComfyUI 모델 폴더에 넣기
- [ ] AI: API 포트 찾기, 모델·LoRA 목록 읽기 (C1)
- [ ] AI: 작은 시험 그림 1장(512×512, 짧은 걸음 수)으로 생성 시간 재기(정보용)

Spec: `../spec.md` (제안 1·2·9) · Grilling: `../grilling.md` (Q3·Q4)

## 시연 순서
1. ComfyUI Desktop을 켬
2. AI가 "연결됨, 모델 SDXL base 1.0·LoRA ○○ 보임"이라고 확인
3. 시험 그림 1장이 몇 초 만에 만들어지는지 확인

## Comments
