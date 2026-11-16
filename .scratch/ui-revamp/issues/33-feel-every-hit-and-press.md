# 33: 피해와 버튼 누름이 손에 느껴진다 (작은 반응)

**What to build:** 피해 때 그림·HUD 부모를 2도트·0.15초 흔들고(Inspector `Screen Shake`로 끔, 제안 7), 줄어든 수만큼 동전이 더미에서 떨어지는 움직임 0.3초. 버튼·슬롯 클릭과 Space·A/D 실행 때 0.1초 1도트 눌림. 모두 `Tween` 사용, 판정·시간 그대로.

**Blocked by:** 30 (`Tween`·동전 더미)

**Status:** ready-for-agent

- [ ] 피해 → 흔들림 0.15초 후 정확히 제자리(정수 도트), 끄면 없음 (C8)
- [ ] 동전 떨어짐 = 줄어든 수 (C8)
- [ ] 버튼·키 실행 눌림 0.1초 (C6)
- [ ] 퇴행 (C9)

Spec: `../spec.md` · Grilling: `../grilling.md` (Q5)

## 시연 순서
1. 하락 보유 → 흔들림·동전 떨어짐
2. Inspector `Screen Shake` 끔 → 흔들림 없음

## Comments
