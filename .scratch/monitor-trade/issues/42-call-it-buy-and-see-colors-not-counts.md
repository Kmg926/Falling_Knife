# 42: 매수라고 부르고, 숫자 대신 캔들 색으로 본다

**What to build:** "보유"를 모든 곳에서 "매수"로 바꾼다 — 코드 `PlayerAction.Hold` → `Buy`(GameManager·ClownAI·PlayerInputHandler·ActionButtonsView·UIManager), 결과 문장 `보유 → …` → `매수 → …`, 화면 글자(버튼 `보유` → `매수`), GLOSSARY "보유" 항목 → "매수". 모니터의 `남은 n`/`하락n 상승m` 글자를 끄고, 덱 공개 연출의 글자(`하락 n · 상승 m`, `섞는 중…`)를 없애 **캔들 색만** 보이게 한다. 복권 결과 문장을 `{이름}: 복권 사용`으로. 아래 띠·버튼은 아직 그대로(43에서).

**Blocked by:** 없음

**Status:** ready-for-agent

- [ ] 화면 글자·결과 문장에 "보유" 0개, 코드 `PlayerAction.Buy` (C6)
- [ ] 화면 어디에도 `남은`·`하락n`·`상승n` 글자 없음, 덱 공개 연출 글자 없음(캔들 색만) (C4)
- [ ] 복권 사용 결과 = `…: 복권 사용` (C5)
- [ ] 퇴행 T65~T68·T143(이름만 매수), 광대 행동 그대로, Console 오류 0 (C8)

Spec: `../spec.md` (제안 10) · Grilling: `../grilling.md` (Q2~Q4)

## 시연 순서
1. 라운드 시작 → 덱 공개에 캔들 색만, 모니터에 남은 개수 없음
2. 매수 → 결과 `플레이어: 매수 → 상승! …`
3. 복권 사용 → 결과 `플레이어: 복권 사용`

## Comments
