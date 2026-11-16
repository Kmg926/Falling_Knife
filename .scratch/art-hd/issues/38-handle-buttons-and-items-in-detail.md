# 38: 버튼·아이템 슬롯·확인 패널이 세밀하게 보인다

**What to build:** Canvas 그림(제안 1)을 2배로 다시 그린다: `hud-bar`, `button-*` 3종, `slot-*` 2종, `turn-banner`, 작은 아이콘 4종, 확인 패널(`closeup-dim`·`panel`·`iconbox`·`tag`), 큰 아이콘 4종, **회전 그림 32장·그림자**(두께·명암 단계 늘림). Image 크기(RectTransform)는 지금 값 그대로.

**Blocked by:** 35

**Status:** ready-for-agent

- [ ] 그림 모두 640 격자·32색 (C1·C2)
- [ ] 버튼·슬롯 글자 겹침 0 (C6)
- [ ] 확인 패널 회전 8장 순서·뜸·그림자 그대로 (ui-revamp T195~T196) (C7)
- [ ] 눌림 1도트(= 새 2도트) 그대로 (C7)

Spec: `../spec.md` (제안 1·8)

## 시연 순서
1. 2번 아이템 확인 → 회전하는 새 아이콘
2. 보유 클릭 → 버튼 눌림

## Comments
