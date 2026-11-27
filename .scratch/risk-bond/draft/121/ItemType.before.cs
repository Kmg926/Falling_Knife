// 아이템 종류. 테스트용 고정 지급 문자열 기호: C 차트 분석, J 찌라시, S 거래 정지, L 복권, D 배당금, B 구제 금융
// heal-items 89: 회복 아이템 2종을 **뒤에** 추가 — 앞 4종의 번호(0~3)가 그대로라 저장된 값·그림 배열 순서가 안 깨진다.
// flip-item 101: 작전(F, 다음 캔들 뒤집기)도 맨 뒤에 — 그림 배열의 7번째 칸
public enum ItemType { ChartAnalysis, Rumor, TradingHalt, Lottery, Dividend, Bailout, Flip }
