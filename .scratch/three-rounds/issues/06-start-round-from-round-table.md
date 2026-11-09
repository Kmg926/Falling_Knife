# 06: 라운드 표로 1라운드를 시작하고 화면에서 `라운드 1 / 3`을 본다

**What to build:** 라운드 수치(잔고·하락·상승·고정 순서)를 GameManager Inspector의 라운드 표 3줄로 옮긴다. Play하면 표의 1번째 줄로 1라운드가 시작되고, 화면 맨 위에 `라운드 1 / 3`이 보인다. **그 밖의 동작은 마일스톤 ①과 똑같다**(데이터만 옮김).

**Blocked by:** None (can start immediately)

**Status:** done (2026-11-09)

- [x] GameManager Inspector에 라운드 표 3줄(2/2/3, 3/3/4, 4/4/4)과 줄마다 고정 순서 칸이 있다
- [x] CandleDeck Inspector에는 `Use Fixed Order` 체크만 남고 구성 칸은 없다
- [x] Play → `라운드 1 / 3`, 잔고 2/2, 남은 하락2/상승3 (C1)
- [x] 1번째 줄 값을 바꾸면 1라운드 시작 값도 바뀐다
- [x] 1번째 줄 고정 순서 `D U U D U` + 체크 → 그 순서로 덱이 정해지고, 구성과 다르면 경고 후 무작위
- [x] `candle-deck-turn` 01~05 시연 순서가 그대로 통과한다 (U1, C2) — 05(패배→R)·04(재장전) 경로로 확인

Spec: `../spec.md` (Q4·Q5·Q6·Q10) · Grilling: `../grilling.md`

## 시연 순서
1. Play → 맨 위 `라운드 1 / 3`과 기존 4줄 확인
2. GameManager 라운드 표 1번째 줄 잔고를 3으로 → Play → 잔고 3/3 → 2로 되돌림
3. 1번째 줄 고정 순서 `D U U D U` + `Use Fixed Order` → Play → Console 순서 그대로
4. `candle-deck-turn` 05의 시연 순서(패배 → R → 승리)를 그대로 진행

## Comments

- 2026-11-09 코드는 사용자가 직접 반영(RoundConfig 새로, CandleDeck·GameManager·UIManager 수정). 리뷰: 초안과 코드 동일, 차이는 ★ 표시 주석뿐. 중간 컴파일 오류(RoundConfig.cs CS8803·CS1002)는 사용자가 직접 수정.
- 2026-11-09 테스트는 AI가 MCP로 실행(10주차 5단계, 프롬프트 (e) 방식): Play 시작/정지, Inspector 값 변경, `runtime_simulate_key`로 D·Space·R 입력, 화면 텍스트 읽기·캡처, Console 조회.
  - T37 ✅ `라운드 1 / 3`, 잔고 2/2, 남은 2/3, 플레이어 차례 (캡처)
  - T38 ✅ `Rounds` 3줄 기본값 2/2/3·3/3/4·4/4/4 (기존 Scene 컴포넌트에도 코드 기본값이 들어옴 — analysis 5절 🔶 확인됨), 덱엔 `Use Fixed Order`만
  - T39 ✅ 1번째 줄 잔고 3 → 잔고 3/3
  - T40 ✅ `D U U D U` → Console `Down Up Up Down Up` 2회, 경고 없음
  - T41 ✅ `D D D U U` → 경고 "고정 순서(하락3/상승2)가 덱 구성(하락2/상승3)과 달라…", 무작위
  - T42 ✅ `U U D U D` + Space×3 → `보유 → 하락! 플레이어 -3`, `패배... - R로 재시작`(캡처, `→` 글자 정상 표시) → D·Space 무시 → R → 처음 상태(`라운드 1 / 3`, 2/2, 2/3)
  - T43 ✅ `D U U D U` + (D→Space)×5 → `덱 재장전 (하락2 / 상승3)`, 잔고 1/1·차례 광대 유지, Console에 `Reload()`가 같은 고정 순서로 다시 채움
  - Console 오류 0 (기록의 오류 1건은 Unity AI Assistant 패키지 연결 오류로 무관)
- MCP 테스트를 위해 `Run In Background`를 **에디터 Play용으로만** 켬(MCP 오버라이드, 빌드는 프로젝트 값=꺼짐 사용). 끄지 않으면 Unity 창에 포커스가 없을 때 Play가 멈춰 키 입력 테스트가 안 됨.
- 테스트 뒤 값 원상복구·Scene 저장: 라운드 표 기본값, 고정 순서 빈칸, `Use Fixed Order` 꺼짐.
- 남은 정리: 스크립트의 `// ★` 표시 주석(GameManager 10곳, UIManager 1곳) — 사용자가 지울 예정. 동작 영향 없음.
