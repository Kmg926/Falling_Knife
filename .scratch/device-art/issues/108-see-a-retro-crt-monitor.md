# 108: 레트로 CRT 모니터를 본다

**What to build:** 내 모니터(`terminal`)를 어두운 베이지 CRT로(두꺼운 테두리·둥근 화면 모서리·유리 반사·버튼 줄·LED), 열 수 있음 테두리(`monitor-outline`)도 맞춤, 광대 모니터 뒷면(`clown-monitor`)을 같은 CRT 뒷면으로. 화면 안 자리·색 그대로. 시안 A/B → 선택 → 적용.

**Blocked by:** 없음

**Status:** done

- [x] 시안 A/B 합성 캡처
- [x] (사용자) 선택: A
- [x] 적용 후 캔들 공개·"다음" 창·금색 테두리 확인
- [x] 32색 검사(시안)

Spec: `../spec.md`

## Comments
- 2026-11-27 AI: 시안 A(금속·어두운 베이지)·B(노키아식 크림 링·밝은 베이지) — `tools/make_art_device.py draft` → `.scratch/art/drafts/device/`(compare-zoom·far·open·game). 게임 화면 캡처에 합성, 모니터는 캡처에서 잰 조명 배율(내 모니터 0.55·광대 0.8)로 어림. `make_art_phone.py`는 바깥 화면 색만 바꿀 수 있게(기본값 같음). 사용자 선택 기다림.
- 2026-11-27 사용자 "A, 모니터는 좋은 것 같아" → A 적용: `terminal.png`(216×136)·`monitor-outline.png`(220×132)·`clown-monitor.png`를 덮어씀(.meta 그대로, 왼쪽 위 기준점 유지). GameScene(MCP) Terminal (−48, −26) → (−52, −22), MonitorOutline (−49, −25) → (−53, −21) — 화면 칸은 같은 자리. 게임 캡처: 금색 테두리 맞음, 매수 → 캔들이 기준선 위. MonitorButton(클릭 칸)은 그대로(테두리 바깥 4칸은 클릭 안 됨).
