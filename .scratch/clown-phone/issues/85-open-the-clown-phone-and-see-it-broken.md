# 85: 광대 폰을 열면 깨진 광대 폰 화면이 보인다

**What to build:** 광대 폰을 클릭해 연 확대 화면만 빨강 몸통·키패드(키 1개 금·1개 빠짐) + 화면 금 간 유리(가장자리) + 2~4초마다 0.1초 글리치(가로줄 띠 + 글자 묶음 1~2도트 밀림). 내 폰 확대 화면은 그대로. 글자·칸 배치 그대로, 가림·넘침 0.

**Blocked by:** 84

**Status:** done

- [x] C3 광대 폰일 때만 바뀜, 글리치, 글자 가림 0
- [x] C6 퇴행 T65~T68·T143(86 뒤 통과), Console 오류 0

Spec: `../spec.md` (제안 3) · Grilling: `../grilling.md` (Q3)

## 시연 순서
1. 광대 폰 클릭 → 빨간 깨진 폰 화면, 가끔 화면이 튐
2. 닫고 내 폰 클릭 → 남색 폰 그대로

## Comments
- 2026-11-22 AI 구현(사용자 "커밋 후 85"): `tools/make_art_phone.py clownzoom`(광대 확대 폰·화면 금·글리치 띠), `PhoneView.cs` 바뀐 줄 + 새 `ScreenGlitch.cs`(Scripts 직접), Scene(MCP) `Crack`·`GlitchBand`·컴포넌트 연결.
  - 시안 고침 1번: 끊기 키 묻힘·테이프 작음.
  - 고친 버그: `enabled = false`로는 코루틴이 안 멈춰 내 폰에서도 띠가 나옴 → `OnDisable`에서 `StopAllCoroutines()`(84의 `PhoneGlitch`에도).
  - C3 확인(`../draft/85/README.md`, 캡처 4장), Console 오류 0.
  - **남은 것(사용자):** 실제 속도에서 화면 튐·금 느낌.
