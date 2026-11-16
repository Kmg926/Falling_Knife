# 37: 방·단말기·캔들·레버리지 불꽃이 세밀하게 보인다

**What to build:** `room`(640×360, 차트 선), `candle-up/down/back`, `terminal`·`terminal-qbox`, `leverage-box`, `flame-on/off`를 2배 격자·32색으로 다시 그린다(제안 8). 단말기·레버리지 상자는 테이블 위 물건처럼 그림자 1줄(Spec 테이블 결정). 위치·크기·Canvas 글자 자리 그대로.

**Blocked by:** 35

**Status:** ready-for-agent

- [ ] 그림 모두 640 격자·32색 (C1·C2)
- [ ] 단말기 안 글자·`?`·레버리지 글자와 겹침 0 (C6)
- [ ] 덱 공개 연출의 캔들 앞·뒷면 새 그림, 놓인 개수 = 덱 구성 (C7)
- [ ] 퇴행 T65~T68·T143 (C7)

Spec: `../spec.md` (제안 8)

## 시연 순서
1. 라운드 시작 → 덱 공개 연출(새 캔들)
2. 보유 상승 2번 → 불꽃 2개

## Comments
