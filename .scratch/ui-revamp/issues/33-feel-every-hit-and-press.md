# 33: 피해와 버튼 누름이 손에 느껴진다 (작은 반응)

**What to build:** 피해 때 그림·HUD 부모를 2도트·0.15초 흔들고(Inspector `Screen Shake`로 끔, 제안 7), 줄어든 수만큼 동전이 더미에서 떨어지는 움직임 0.3초. 버튼·슬롯 클릭과 Space·A/D 실행 때 0.1초 1도트 눌림. 모두 `Tween` 사용, 판정·시간 그대로.

**Blocked by:** 30 (`Tween`·동전 더미)

**Status:** done

- [x] 피해 → 흔들림 0.15초 후 정확히 제자리(정수 도트), 끄면 없음 (C8)
- [x] 동전 떨어짐 = 줄어든 수 (C8)
- [x] 버튼·키 실행 눌림 0.1초 (C6)
- [x] 퇴행 (C9)

Spec: `../spec.md` · Grilling: `../grilling.md` (Q5)

## 시연 순서
1. 하락 보유 → 흔들림·동전 떨어짐
2. Inspector `Screen Shake` 끔 → 흔들림 없음

## Comments
- 2026-11-17 AI 작업(사용자 "커밋 후 33", 32 커밋 `37104bf`): 코드 `draft/33`(UIManager 흔들림·BalanceView 동전 떨어짐·ActionButtonsView/ItemSlotsView 눌림)을 Scripts에 직접 씀(4장 예외), 재컴파일 오류 0. 줄별 설명은 `draft/33/README.md`.
  - 구체화: 흔드는 대상은 **월드 그림 묶음(PlayArt·Clown)만**. 카메라는 Pixel Perfect 흐려짐 위험, HUD 글자는 읽기 쉬움(Q6) 때문에 그대로 둠. UIManager `Shake Targets`에 MCP로 연결 후 Scene 저장.
- 테스트(AI가 MCP로):
  - T198 ✅ 2번 칸 확인 시작 → 슬롯 y −150 → −151 → −150 (C6)
  - T199 ✅ 매도 실행 → 고른 버튼 y −147 → −148 → −147 (C6)
  - T200 ✅ 피해 → 흔들림 최대 2.0도트, 0.15초 뒤 정확히 제자리 (C8)
  - T201 ✅ 잔고 2 잃음 = 떨어진 동전 2개, 끝난 뒤 남은 복사본 0 (C8)
  - T202 ✅ `Screen Shake` 끔 → 흔들림 최대 0, 테스트 뒤 다시 켬 (C8)
  - 퇴행 ✅ T65 패배·다시하기 / T143 공개 중 음악 0.12 → 0.60 / T66 클리어·2라운드 / T67 자멸 클리어 / T68 클리어 뒤 광대 행동 없음 (C9)
  - Console 새 오류 0(목록의 MissingReference 2개는 31 수정 전 기록). Play 뒤 "변경됨"은 복사본 비교로 차이 0 → 저장 안 함.
- 남은 것(사용자): 실제 키·마우스로 눌림·흔들림 체감, 흔들림이 거슬리면 `Screen Shake` 끔
