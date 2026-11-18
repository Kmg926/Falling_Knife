# 50: ComfyUI가 내 컴퓨터에서 돈다

**What to build:** 사용자가 공식 배포처에서 **ComfyUI Desktop**(Windows 설치 파일)을 설치하고, 기본 모델 **SDXL base 1.0** + 도트 그림 LoRA 1개를 받는다. AI는 받기 전에 LoRA 후보 2~3개를 찾아 라이선스(비영리 사용·결과물 공개 허락)·예시와 함께 보고하고, SDXL 라이선스도 배포 페이지에서 확인해 알린다. 설치 뒤 AI가 로컬 API 포트를 찾아 모델·LoRA 목록을 읽어 연결을 확인한다(C1). AI는 실행 파일·모델을 직접 받지 않는다.

**Blocked by:** 없음

**Status:** done

- [x] AI: SDXL base 1.0 라이선스 확인·보고
- [x] AI: 도트 LoRA 후보 2~3개(이름·배포 페이지·라이선스·예시) 보고 → 사용자 선택
- [x] 사용자: ComfyUI Desktop 설치, SDXL base 1.0·고른 LoRA를 ComfyUI 모델 폴더에 넣기
- [x] AI: API 포트 찾기, 모델·LoRA 목록 읽기 (C1)
- [x] AI: 작은 시험 그림 1장(512×512, 짧은 걸음 수)으로 생성 시간 재기(정보용)

Spec: `../spec.md` (제안 1·2·9) · Grilling: `../grilling.md` (Q3·Q4)

## 시연 순서
1. ComfyUI Desktop을 켬
2. AI가 "연결됨, 모델 SDXL base 1.0·LoRA ○○ 보임"이라고 확인
3. 시험 그림 1장이 몇 초 만에 만들어지는지 확인

## Comments
- 2026-11-18 AI 조사(사용자 "커밋 후 50", 커밋 `bcb6e2a`). Hugging Face 페이지 메타데이터 기준(라이선스 전문은 아직 안 읽음):
  - **SDXL base 1.0** (`stabilityai/stable-diffusion-xl-base-1.0`): CreativeML Open RAIL++-M, `sd_xl_base_1.0.safetensors` 6.94 GB, 받기 전 동의 절차 안 보임(로그인 안 한 상태는 확인 못 함). 모델 카드 "Direct Use"에 "연구 목적" 문구 — 라이선스 자체는 OpenRAIL++-M(사용 제한 조항형).
  - LoRA 후보 ① `nerijs/pixel-art-xl`: creativeml-openrail-m, 171 MB, 트리거 없음, "8배 줄이기(Nearest)·고친 VAE 권장·refiner 쓰지 말 것" / ② `artificialguybr/PixelArtRedmond`: creativeml-openrail-m, 171 MB, 트리거 `PixArFK`, 사용법 설명 적음 / ③ `ntc-ai/SDXL-LoRA-slider.pixel-art`: MIT, 8.79 MB, 슬라이더형(세기 ±), 예시는 SDXL 기본이 아닌 turbo 계열 모델.
  - ComfyUI Desktop 기본 포트 8188(쓰는 중이면 다음 빈 포트) — docs.comfy.org.
  - 사용자 선택: **① `nerijs/pixel-art-xl`** (2026-11-18).
- 2026-11-18 AI 라이선스 전문 확인(SDXL `LICENSE.md` = CreativeML Open RAIL++-M 2023-07-26, LoRA 태그가 가리키는 CreativeML Open RAIL-M 2022-08-22):
  - 만든 그림(결과물): 라이선스 주인은 결과물에 권리를 주장하지 않음, 쓴 사람이 책임. 비영리·연구 전용 제한 **없음**.
  - 금지 용도(부록 A, 11가지): 불법, 미성년자 해침, 남을 해치는 거짓 정보·개인정보, 명예훼손·괴롭힘, 법적 권리에 영향 주는 자동 결정, 차별, 의료 조언, 수사·이민 판단 등 → 이 게임 배경 그림과 무관.
  - 모델 파일을 **다시 배포**할 때만 라이선스 사본·부록 A 전달 의무 → 우리는 모델을 저장소에 올리지 않음(그림만 씀). `.gitignore`·저장소에 모델 파일 없음을 51에서 확인.
  - 고친 VAE `madebyollin/sdxl-vae-fp16-fix`: **MIT**, 파일 `sdxl.vae.safetensors`(크기는 페이지에 없음, 약 0.3 GB 추정). SDXL VAE가 fp16에서 깨지는 문제를 고친 판 — pixel-art-xl 권장.
  - 확인 못 한 것: pixel-art-xl 카드가 가리키는 라이선스 파일이 정확히 2022 RAIL-M인지(태그만 확인).
- 2026-11-18 AI 연결 확인 1차(사용자 "설치 끝, 확인해"): ComfyUI Desktop 설치됨(ComfyUI v0.45.6, NVIDIA용, 모델 폴더 = Desktop의 공유 `models` 폴더). 하지만 ① `127.0.0.1:8188`에 응답 없음(로그엔 13:54 서버 시작 기록, 지금 듣는 포트 없음) ② 모델 3개 파일을 공유 `models`·내려받기 폴더·사용자 폴더 어디에서도 못 찾음. → 사용자에게 서버 켜기·파일 넣을 정확한 폴더 안내. C1 아직.
- 2026-11-18 AI 연결 확인 2차: 파일 3개가 모두 `models/checkpoints/`에 있음(`sd_xl_base_1.0` 6.94 GB·`pixel-art-xl` 171 MB·`sdxl_vae` 335 MB) → LoRA는 `loras/`, VAE는 `vae/`로 옮겨야 ComfyUI가 그 종류로 읽음. 서버는 한 번 응답(200) 뒤 Desktop이 다시 시작되며(14:04) 지금 듣는 포트 없음. C1 아직.
- 2026-11-18 사용자 "옮겨줘": AI가 `pixel-art-xl.safetensors` → `models/loras/`, `sdxl_vae.safetensors` → `models/vae/`로 옮김(같은 폴더 안 이동, 지운 것 없음). 확인: checkpoints 1·loras 1·vae 1. 서버는 아직 응답 없음 → 사용자가 ComfyUI 실행 뒤 C1.
- 2026-11-18 사용자 "실행했어, 확인": **C1 통과** — `127.0.0.1:8188` 응답 200(ComfyUI 0.45.6), 모델 목록 checkpoints `sd_xl_base_1.0`·loras `pixel-art-xl`·vae `sdxl_vae`.
  - 시험 그림(512×512, 12걸음, euler, cfg 6, LoRA 1.0, 시드 1, "pixel art, a single hanging lamp over a dark table, flat colors"): 첫 생성 8.1초(모델 불러오기 포함), 다음 생성(시드 2) 3.0초. 결과 `.scratch/art/drafts/comfy/test-512-seed1.png`(시험용, 게임에 안 씀).
  - 모델 파일은 저장소 밖(ComfyUI 공유 models 폴더) — 재배포 의무 없음.
