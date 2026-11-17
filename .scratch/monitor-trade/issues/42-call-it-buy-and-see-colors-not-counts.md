# 42: 매수라고 부르고, 숫자 대신 캔들 색으로 본다

**What to build:** "보유"를 모든 곳에서 "매수"로 바꾼다 — 코드 `PlayerAction.Hold` → `Buy`(GameManager·ClownAI·PlayerInputHandler·ActionButtonsView·UIManager), 결과 문장 `보유 → …` → `매수 → …`, 화면 글자(버튼 `보유` → `매수`), GLOSSARY "보유" 항목 → "매수". 모니터의 `남은 n`/`하락n 상승m` 글자를 끄고, 덱 공개 연출의 글자(`하락 n · 상승 m`, `섞는 중…`)를 없애 **캔들 색만** 보이게 한다. 복권 결과 문장을 `{이름}: 복권 사용`으로. 아래 띠·버튼은 아직 그대로(43에서).

**Blocked by:** 없음

**Status:** done

- [x] 화면 글자·결과 문장에 "보유" 0개, 코드 `PlayerAction.Buy` (C6)
- [x] 화면 어디에도 `남은`·`하락n`·`상승n` 글자 없음, 덱 공개 연출 글자 없음(캔들 색만) (C4)
- [x] 복권 사용 결과 = `…: 복권 사용` (C5)
- [x] 퇴행 T65~T68·T143(이름만 매수), 광대 행동 그대로, Console 오류 0 (C8)

Spec: `../spec.md` (제안 10) · Grilling: `../grilling.md` (Q2~Q4)

## 시연 순서
1. 라운드 시작 → 덱 공개에 캔들 색만, 모니터에 남은 개수 없음
2. 매수 → 결과 `플레이어: 매수 → 상승! …`
3. 복권 사용 → 결과 `플레이어: 복권 사용`

## Comments
- 2026-11-17 AI 작업(사용자 "커밋 후 42", Spec·Ticket 커밋 `32c09f0`): 코드 `draft/42`(GameManager·ClownAI·PlayerInputHandler·ActionButtonsView·UIManager·LeverageCounter·TerminalView·DeckRevealView)를 Scripts에 직접 씀(4장 예외, monitor-trade는 Grilling Q8), 재컴파일 오류 0. Scene(MCP): 버튼 `보유` → `매수`, `RemainingText`·`DeckReveal/Caption` 끔(사용자 변경 없음 확인 후 저장). GLOSSARY "보유" → "매수". 줄별 설명 `draft/42/README.md`.
  - 정해서 한 것: enum은 이름만(번호 0 그대로), `ActionButtonsView`의 Inspector 칸 이름 `holdButton`·`holdImage`는 연결이 끊기지 않게 그대로(43에서 버튼 없앰). 복권 당첨 내용은 화면에 안 쓰고 테스트용 `LastLotteryDetail`에만.
- 테스트(AI가 MCP로): C4 ✅ 덱 공개 중·첫 화면 보이는 글자에 개수·`보유` 0(숨은 F1 개발용 글자에만 남음 — #41로 최종 빌드 전 제거) / C5 ✅ `플레이어: 복권 사용`(숨은 값 `꽝 (잔고가 이미 가득)`) / C6 ✅ `플레이어: 매수 → 상승! 레버리지 1, 차례 유지` / C8 ✅ T65·T143·T66·T67·T68(기대 문장 매수로) / Console 새 오류 0.
