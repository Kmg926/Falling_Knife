# 공용 배수 모듈 이름을 StackCounter가 아니라 LeverageCounter로 한다

GDD 08장에는 공용 배수를 다루는 스크립트가 `StackCounter`로 적혀 있지만, 이 이름은 2026-10-30에 버린 속어 "존버 스택"에서 왔다. 게임 용어가 "레버리지"로 확정됐으므로 코드 이름도 `LeverageCounter`로 맞춘다(2026-11-02 Grilling Q14). 기말 코드 구술 면담에서 화면 용어와 코드 이름이 같아야 설명이 막히지 않기 때문이다. GDD는 제출본이라 고치지 않고 `docs/gdd-changes.md` #2로 기록한다.
