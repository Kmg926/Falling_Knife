# 23: 배경음악이 흐르고, 캔들 공개 중에는 작아진다

**What to build:** AudioManager의 음악용 AudioSource가 배경음악 칸의 곡을 반복 재생한다(칸이 비면 아무 일 없음, 오류 없음). 캔들 공개 동안 음악 크기를 약 20%로 줄였다가 결과와 함께 되돌린다. 음악 크기 칸(0~1)과 전체 크기를 곱한다. 곡은 사용자가 이용 허락된 곡을 구해 `Assets/_Project/Audio/Music/`에 넣고 칸에 연결한다(AI 생성 안 함).

**Blocked by:** 21

**Status:** done (2026-11-16) — 자리·크기 조절 완료. 곡 연결·이음매 듣기·라이선스 기록은 곡을 구한 뒤(사용자)

- [x] 음악 칸이 비어 있어도 Play·공개·재시작 오류 없음 (C9)
- [x] 공개 중 음악 volume = 전체 × 음악 × 0.2, 결과 뒤 전체 × 음악 (C3)
- [x] 음악 크기 0 → 음악만 무음 / 전체 0 → 전부 무음 (C6)
- [ ] (곡이 오면) 반복 재생·이음매 30초 이상 들어 보기 (사용자) (C9)
- [ ] 곡 출처·라이선스를 `docs/asset-rights.md` 외부 에셋 표에 기록 (사용자 확인)

Spec: `../spec.md` · Grilling: `../grilling.md` (Q10, Q11, 사용자 추가 결정)

## Comments
- 2026-11-16 코드는 사용자가 직접 타이핑(`AudioManager.cs`, `GameManager.cs`): 두 파일 모두 초안과 차이 0. 처음 "반영함" 때는 AudioManager가 저장되지 않아 GameManager의 `SetRevealDuck` 호출이 컴파일되지 않음 → 사용자 저장 뒤 컴파일 00:08:57 > 저장 00:08:52, 에러 0. 음악 크기 시작값 0.6은 AI가 정한 값(곡이 오면 조정).
- **Scene "변경됨" 원인 확인:** 메모리의 Scene을 복사본으로 저장해 파일과 비교 → 차이는 22·23에서 새로 생긴 칸의 코드 기본값(`musicVolume 0.6`, `revealDuck 0.2`, `flashSeconds 0.2`, `flashColor`, `dangerBalance 3`, `dangerColorHex`)뿐. 사용자 변경 없음 확인 → 저장.
- Scene(AI가 MCP로): `Audio` 오브젝트에 음악용 AudioSource 추가(Play On Awake·Loop 켬, 곡 칸 비움), AudioManager `Music Source` 연결(효과음 재생기와 다른 것 확인) → 저장.
- 테스트(AI가 MCP로, Play 중에만 값 변경):
  - T142 ✅ 곡 칸 비운 채 Play(음악 재생 안 함, volume 0.60) → 보유 하락으로 패배 → R → 1라운드, 오류 없음 (C9)
  - T143 ✅ 음악 volume 평소 **0.60** → 공개 중 **0.12** → 결과 뒤 **0.60** (C3)
  - T144 ✅ Music Volume 0 → 음악 0.00 / Master 0 → 음악 0.00 + 클릭 재생 안 됨 (C6)
  - T145 ✅ Play 중에만 임시로 셔플(0.6초)을 곡 칸에 넣고 1.5초 뒤 → `isPlaying` true, `loop` true, 재생 위치 0.31초(반복 중). 끝나고 곡 칸 비움
  - T146 ✅ 퇴행 `clown-ai` T65 패배 / T67 클리어 / T66 클리어 / T68 `차례: 광대` 클리어 → Space → 2라운드 플레이어 (C10)
  - Console 오류 0(목록의 CS1061 `FlashDamage`는 22 반영 중간의 옛 기록). Play를 멈춘 뒤 다시 "변경됨" 표시가 나왔지만 복사본 비교 결과 내용 차이 0 → 저장할 것 없음.
