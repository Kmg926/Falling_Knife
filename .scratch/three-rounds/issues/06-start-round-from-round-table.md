# 06: 라운드 표로 1라운드를 시작하고 화면에서 `라운드 1 / 3`을 본다

**What to build:** 라운드 수치(잔고·하락·상승·고정 순서)를 GameManager Inspector의 라운드 표 3줄로 옮긴다. Play하면 표의 1번째 줄로 1라운드가 시작되고, 화면 맨 위에 `라운드 1 / 3`이 보인다. **그 밖의 동작은 마일스톤 ①과 똑같다**(데이터만 옮김).

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

- [ ] GameManager Inspector에 라운드 표 3줄(2/2/3, 3/3/4, 4/4/4)과 줄마다 고정 순서 칸이 있다
- [ ] CandleDeck Inspector에는 `Use Fixed Order` 체크만 남고 구성 칸은 없다
- [ ] Play → `라운드 1 / 3`, 잔고 2/2, 남은 하락2/상승3 (C1)
- [ ] 1번째 줄 값을 바꾸면 1라운드 시작 값도 바뀐다
- [ ] 1번째 줄 고정 순서 `D U U D U` + 체크 → 그 순서로 덱이 정해지고, 구성과 다르면 경고 후 무작위
- [ ] `candle-deck-turn` 01~05 시연 순서가 그대로 통과한다 (U1, C2)

Spec: `../spec.md` (Q4·Q5·Q6·Q10) · Grilling: `../grilling.md`

## 시연 순서
1. Play → 맨 위 `라운드 1 / 3`과 기존 4줄 확인
2. GameManager 라운드 표 1번째 줄 잔고를 3으로 → Play → 잔고 3/3 → 2로 되돌림
3. 1번째 줄 고정 순서 `D U U D U` + `Use Fixed Order` → Play → Console 순서 그대로
4. `candle-deck-turn` 05의 시연 순서(패배 → R → 승리)를 그대로 진행
