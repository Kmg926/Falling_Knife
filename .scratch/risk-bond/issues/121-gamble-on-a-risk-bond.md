# 121: 위험 채권으로 반반 도박을 한다

**What to build:** 구제 금융(잔고 +2 회복)을 위험 채권(50% 잔고 +2 / 50% 잔고 −1, 언제든 사용, 지면 파산 가능)으로. 결과 판·광대 규칙·대사·아이콘·글 바꿈. 지급 제한은 배당금과 묶음 그대로.

**Blocked by:** 없음

**Status:** done

- [x] 코드: GameManager(UseRiskBond·결과 신호), ClownAI(③-2), LotteryPopupView(결과 판), ClownSpeechView·TutorialGuide·TitleMenu 글
- [x] 아이콘(32색)
- [x] ItemData 에셋 이름·분류·설명(MCP), Scene 글 값(광대 대사 GameScene·규칙 창 TitleScene)
- [x] 컴파일, 시험(성공·실패·가득·파산·광대) — README 표
- [ ] (사용자) 직접 해 보기

Spec: Grilling 표(`../grilling.md`)가 Spec을 겸함(작은 기능)

## Comments
- 2026-11-27 Unity 편집기가 꺼져 있어 에셋·Scene·컴파일·시험은 다시 켠 뒤에.
- 2026-11-27 Unity 다시 켠 뒤: 에셋·Scene 값·시험 완료. 처음 큰 아이콘을 HUD 폴더에 잘못 넣어(상세 화면은 Closeup 폴더 그림을 씀) 그 파일은 지우고 Closeup에 넣음. README `../draft/121/README.md`.
- 2026-11-27 사용자 "위험 채권은 따로 스프라이트 만들어줘" → 새 파일 `Sprites/HUD/item-riskbond.png`·`Sprites/Closeup/item-riskbond-big.png`(가져오기 설정은 구제 금융 그림과 같게), `item-bailout*`는 구제 금융 원래 그림으로 되돌림(커밋 f168b44 때 파일), 참조 3곳(GameScene ItemSlotsView·ItemCloseupPanel, TitleScene TitleMenu의 [5])을 새 파일로(MCP, 다른 줄은 안 바뀜 확인). 도구 출력 이름도 item-riskbond.
