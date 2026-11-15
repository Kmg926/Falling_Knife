# 21: 사건마다 효과음이 난다 (충격음·초침·클릭·셔플)

**What to build:** AI가 `tools/make_sfx.py`로 효과음 후보 12개(4종 × A·B·C)와 측정표를 만든다. 사용자가 듣고 하나씩 고르면 `Assets/_Project/Audio/Sounds/`에 넣는다. 새 `AudioManager`가 하락 피해 때 충격음, 캔들 공개 때 초침, 선택이 바뀔 때·아이템 확인/취소 때 클릭, 재장전 때 셔플을 한 번씩 튼다. 전체 크기(0~1)가 0이면 무음. 판정·시간은 그대로.

**Blocked by:** 없음

**Status:** done (2026-11-15) — 작동 확인 완료, 직접 듣기·효과음 최종 선택은 남음

- [x] 후보 12개 + 측정표(길이·앞 무음·최대 dBFS), 길이가 정한 범위 안 (C1)
- [ ] 사용자가 4개 고름 (지금은 임시로 후보 A) → `Audio/Sounds/` (C1)
- [x] 하락 피해 1번 → 충격음 1번 (플레이어·광대, 보유·매도) (C2)
- [x] 공개 1번 → 초침 1번 (C3)
- [x] 선택 바뀔 때·아이템 확인/취소 때 클릭 (C4)
- [x] 재장전 → 셔플 1번 (C5)
- [x] 전체 크기 0 → 무음 (C6)
- [x] 퇴행·Console 오류 0 (C10)

Spec: `../spec.md` · Grilling: `../grilling.md` (Q3~Q5, Q7~Q10)

## 시연 순서
1. 후보 표와 파일 → 사용자가 듣고 고름
2. Play → A/D(클릭), Space(초침 → 결과), 하락(충격음), 덱 소진(셔플)
3. 전체 크기 0 → 조용함

## Comments
- 2026-11-15 AI가 `tools/make_sfx.py`(numpy 합성, 시드 20261115 고정) 작성 → 후보 12개를 `.scratch/art-sound/candidates/`에 생성, 측정표 `measurements.md`. 12개 모두 정한 길이 범위 안, 앞 무음 0ms, 최대 음량 충격음 −3 / 초침 −9 / 클릭 −12 / 셔플 −6 dBFS. AI는 고르지 않음 → 사용자가 듣고 고를 차례.
- 2026-11-15 사용자 지시 "일단 임시 효과음으로 두고 다음으로 진행" → 사용자가 고르기 전 **임시로 후보 A 4개**를 `Assets/_Project/Audio/Sounds/{impact,tick,click,shuffle}.wav`에 넣음(AI가 고른 것이 아니라 기본값으로 둔 것, 최종 선택은 사용자). `docs/asset-rights.md` AI 생성 에셋 #1~4에 임시로 기록. 게임 코드 초안 `draft/21`(AudioManager 새 파일, GameManager 부르는 줄) 작성.
- 2026-11-15 코드는 사용자가 직접 타이핑(`AudioManager.cs` 새 파일, `GameManager.cs`): 두 파일 모두 초안과 차이 0. 처음 새 파일 이름이 `AudioManagers.cs`(s)라 클래스 이름과 달라 컴포넌트로 붙일 수 없었음 → 사용자가 Project 창에서 `AudioManager`로 이름 변경(.meta 유지). 컴파일 에러 0.
- Scene(AI가 MCP로, 저장 전 변경 없음 확인): `Audio` 오브젝트 = AudioSource(Play On Awake 끔) + AudioManager(클립 4개 = `Audio/Sounds/impact·tick·click·shuffle`, Sfx Source, Master Volume 1), GameManager `Audio Manager` 칸 연결 → 저장.
- 테스트(AI가 MCP로. 소리는 들을 수 없어 "그 소리 클립만 남기고 → 사건 → `sfxSource.isPlaying`"으로 확인. 고정 덱·아이템은 Play 중에만):
  - T133 ✅ 초침: 공개 시작 0.21초 뒤 재생, 공개 끝 0.51초(전체 시간 그대로) (C3, U2)
  - T132 ✅ 충격음: 플레이어 보유 하락(`플레이어 -1`)·플레이어 매도 하락(`광대 -1`)·광대 매도 하락(`플레이어 -1`) 모두 재생 (C2)
  - T134 ✅ 클릭: D(바뀜) 재생 / D(같음) 없음 / A(바뀜) 재생 / 아이템 확인 재생 / 취소 재생 / 빈 슬롯 4번 없음 (C4)
  - T135 ✅ 셔플: 라운드 시작엔 없음 / 찌라시로 마지막 캔들을 버려 재장전 → 재생 (C5)
  - T136 ✅ Master Volume 0: 선택·공개·피해 내내 재생 없음 (C6)
  - T137 ✅ 퇴행 `clown-ai` T65 패배 / T67 클리어 / T66 클리어 / T68 `차례: 광대` 클리어 → Space → 2라운드 플레이어 (C10)
  - Console 오류 0.
- 참고: 테스트 뒤 Play를 멈추니 Scene이 "변경됨" 표시. 값(고정 끔, 잔고 10, 공개 0.5, 생각 1, 초침 0.2, 크기 1, 클립 연결)은 저장한 상태와 같아 보이지만 무엇이 바뀌었는지 확인하지 못해 **저장하지 않음**.
- **남은 것(사용자):** 스피커로 직접 듣기 — 소리 크기·톤, 클릭이 거슬리는지. 임시 효과음(후보 A) 최종 선택.
