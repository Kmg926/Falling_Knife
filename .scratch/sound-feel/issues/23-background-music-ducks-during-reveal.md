# 23: 배경음악이 흐르고, 캔들 공개 중에는 작아진다

**What to build:** AudioManager의 음악용 AudioSource가 배경음악 칸의 곡을 반복 재생한다(칸이 비면 아무 일 없음, 오류 없음). 캔들 공개 동안 음악 크기를 약 20%로 줄였다가 결과와 함께 되돌린다. 음악 크기 칸(0~1)과 전체 크기를 곱한다. 곡은 사용자가 이용 허락된 곡을 구해 `Assets/_Project/Audio/Music/`에 넣고 칸에 연결한다(AI 생성 안 함).

**Blocked by:** 21

**Status:** ready-for-agent

- [ ] 음악 칸이 비어 있어도 Play·공개·재시작 오류 없음 (C9)
- [ ] 공개 중 음악 volume = 전체 × 음악 × 0.2, 결과 뒤 전체 × 음악 (C3)
- [ ] 음악 크기 0 → 음악만 무음 / 전체 0 → 전부 무음 (C6)
- [ ] (곡이 오면) 반복 재생·이음매 30초 이상 들어 보기 (사용자) (C9)
- [ ] 곡 출처·라이선스를 `docs/asset-rights.md` 외부 에셋 표에 기록 (사용자 확인)

Spec: `../spec.md` · Grilling: `../grilling.md` (Q10, Q11, 사용자 추가 결정)

## Comments
