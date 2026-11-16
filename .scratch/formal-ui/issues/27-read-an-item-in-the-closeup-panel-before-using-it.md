# 27: 아이템을 쓰기 전에 확인 패널에서 무엇을 하는지 읽는다

**What to build:** `ItemCloseupPanel`(GDD 08장 이름). 아이템 확인 중(`IsConfirming`)이면 가운데 패널을 켜고 배경을 어둡게 한다: 아이콘(크게)·이름·분류·설명 2줄(ItemData)·키 안내(`Space 사용 Esc 취소 1~4 전환`), 선택 슬롯 노란 테두리. 패널의 `사용`/`취소` 버튼 클릭 = Space/Esc(제안 5). 판정은 기존 함수 그대로.

**Blocked by:** 26 (슬롯·클릭)

**Status:** done (2026-11-16) — 작동 확인 완료. 실제 마우스·키는 사용자 직접 확인 남음

- [x] 1~4 또는 슬롯 클릭 → 패널 켜짐, 내용 = 그 아이템, 배경 어둡게, 노란 테두리 (C10)
- [x] 다른 번호 → 패널 내용 전환
- [x] `사용` 클릭/Space → 효과 적용·패널 닫힘 / `취소` 클릭·우클릭·Esc → 닫힘 (C8)
- [x] 패널 중 보유·매도 버튼 회색 (C7)
- [x] 아이콘 4종 큰 그림: 팔레트 안 (C1), asset-rights
- [x] 퇴행 C17

Spec: `../spec.md` · Grilling: `../grilling.md` (Q2) · 스케치 화면 3

## 시연 순서
1. 1 → 차트 분석 패널 → 2 → 찌라시로 전환
2. `사용` 클릭 → 효과 → 패널 닫힘
3. 3 → Esc → 닫힘

## Comments
- 2026-11-16 AI 작업(사용자 "커밋 후 27"): `tools/make_art.py closeup` → 그림 8개(패널 168×110·큰 아이콘 칸·분류 표·배경 바둑판 320×144·큰 아이콘 4개(작은 것 2배)) + 미리보기, 팔레트 검사 OK. 분류는 GDD 아이템 표("정보, 조작, 방해, 도박")에서. 규격 아트 메모 7-4절. **다음:** 사용자 확인 → Sprites·Scene 배치 → 코드(`ItemCloseupPanel`, ItemData에 분류 칸).
- 2026-11-16 사용자 "이대로 진행": AI가 MCP로 그림 8개를 `Sprites/Closeup/`에 가져오고 `HUD/ItemCloseup`(어둡게·패널·아이콘 칸·큰 아이콘·이름·분류 표·설명·`Space 사용`/`Esc 취소` 버튼·`1~4 전환`)을 배치(처음엔 꺼짐, 피해 덮개보다 뒤) → 저장. 게임 코드 초안 `draft/27`(ItemData `category`, GameManager `ItemCategory`, 새 `ItemCloseupPanel`, UIManager 한 줄), README 줄별 설명·테스트 T168~T174.
- 2026-11-16 코드 반영: 사용자 지시("AI가 작성")로 **AI가 `draft/27` 4개 파일을 Scripts에 직접 씀**(CLAUDE.md 4장 예외). 쓰기 전 Scripts = 마지막 커밋 확인, 쓴 뒤 초안과 같음 확인, 다시 컴파일 오류 0.
- Scene·데이터(AI가 MCP로, 저장 안 된 변경 없음 확인): `HUD`에 ItemCloseupPanel(Root·큰 아이콘·이름·분류·설명·사용/취소·큰 아이콘 4장), UIManager `Closeup Panel` 연결, ItemData 4개 Category = 정보/조작/방해/도박 → 저장. 키 안내 글자가 붙어 보여 크기 9·위치 조정.
- 테스트(AI가 MCP로, Play 중에만 고정 덱·아이템):
  - ✅ 시작: 패널 꺼짐
  - T168 ✅ 1번 → 패널 켜짐, `차트 분석`·`정보`·`다음 캔들을 나만 확인한다.`·큰 돋보기, 슬롯 1 노란 테두리, 버튼 회색 (C10·C7)
  - T169 ✅ 2번 → `찌라시`·`조작`, 노란 테두리 2번으로
  - T170 ✅ `사용` 클릭 → 찌라시 효과, 패널 꺼짐, 아이템 1개 (C8)
  - T171 ✅ `취소` 클릭 → 패널 꺼짐, 아이템 남음 (C8)
  - T172 ✅ 광대 정지 상태에서 거래 정지 → `방해`, 설명에 `(사용 불가: 상대가 이미 정지 상태)`
  - T173 ✅ 퇴행 T65~T68·T143 모두 같음, Console 오류 0, Play 뒤 Scene 변경 없음
  - T174 ✅ 캡처 `.scratch/art/captures/27-item-closeup.png` (스케치 화면 3과 같은 구성, 패널만 위로)
- 참고: Console 경고 1개 `Galmuri11 SDF.asset` 가져오기 결과가 달라짐 — 새 글자(한글)가 쓰일 때마다 TMP가 글꼴 그림판에 글자를 더하기 때문(Multi Atlas Textures). 오래 커밋 안 한 폰트 에셋 변경의 원인으로 보임.
