# 01: 덱을 섞고 남은 하락·상승 개수를 화면에서 본다

**What to build:** GameScene에서 Play를 누르면 1라운드 덱(하락2·상승3)이 섞이고, 텍스트 UI에 처음 상태가 보인다: 플레이어 잔고 2 / 광대 잔고 2, 레버리지 0, 남은 하락 2 / 상승 3, "플레이어 차례". 테스트용으로 Inspector에서 덱 순서를 고정할 수 있다. 아직 행동은 실행할 수 없다.

**Blocked by:** None (can start immediately)

**Status:** done (2026-11-02)

- [x] GameScene이 있고, 그 Scene에서 Play하면 위 처음 상태가 Canvas + TextMeshPro 글자로 보인다
- [x] 라운드 수치(잔고, 하락 수, 상승 수)를 Inspector에서 바꾸면 화면 처음 상태도 그 값으로 바뀐다
- [x] 고정 순서를 끄면 Play할 때마다 섞인 순서가 달라진다 (Console 등으로 순서를 확인할 방법 하나)
- [x] 고정 순서를 켜고 `D U U D U`를 적으면 그 순서로 덱이 정해진다
- [x] 고정 순서가 구성과 맞지 않으면(예: `D D D U U`) Console에 경고가 뜨고 무작위로 섞는다
- [x] 1라운드 선공은 플레이어로 표시된다

Spec: `../spec.md` (Q4·Q5·Q6·Q11·Q13·Q23) · Grilling: `../grilling.md`

## 시연 순서
1. GameScene에서 Play → 화면 4줄 확인: `차례: 플레이어` / `잔고 플레이어 2 / 광대 2` / `레버리지 0` / `남은 캔들 하락 2 / 상승 3`
2. Console의 "덱 순서" 확인 → 정지·Play를 반복해 순서가 바뀌는 것 보여주기
3. `Use Fixed Order` 켜고 `D U U D U` → Play → Console 순서가 그대로
4. `D D D U U` → Play → 노란 경고, 무작위로 섞임

## Comments

- 2026-11-02 수동 테스트 T01~T06 통과 (사용자 실행·보고). T01은 스크린샷으로 확인: Galmuri11 SDF 한글 4줄, Console 덱 순서 `Up Up Up Down Down`.
- 스크립트 5개는 사용자 지시로 AI가 초안 그대로 생성 (Parsec 원격 지연으로 타이핑 불가, CLAUDE.md 4장 예외 기록).
- `Status: done`은 기본 라벨 5개에 없는 완료 표시로 추가해 썼다.
