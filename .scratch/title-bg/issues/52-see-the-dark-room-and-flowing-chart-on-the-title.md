# 52: 타이틀에서 어두운 방과 흐르는 시세가 보인다

**What to build:** TitleScene에 빈 부모 `TitleArt`를 두고, 그 아래 게임 스프라이트를 그대로 배치한다(새 그림 없음): `Room`(`Sprites/Play/room.png`, 화면 전체), `Table`(`table.png`, 광대 몸 아래 끝 근처로 **내려서** 앞부분만 보이게), `Lamp`(`lamp.png`, **광대 머리 위** x, 위 끝 y 90, 조명 안 받음) + 자식 `LampLight`(Spot Light 2D, 게임과 같은 값), `LampBeam`(`lamp-beam.png`, 같은 x, 알파 0.2, 조명 안 받음, 정렬 −1), `ChartLines`(`ChartLinesView`, `game` 비움, 게임과 같은 자리, 조명 안 받음), `Global Light 2D`(0.35·(0.75, 0.8, 1)). 정렬 순서·재질은 GameScene과 같게. `ChartLinesView`에 칸 `idleSpikeMin`·`idleSpikeMax`(초, 0이면 끔)를 더해 타이틀에서만 8~12초마다 급락(`StartSpike(downDots)`) — GameScene 값은 0으로 그대로. 타이틀 카메라를 GameScene처럼 Pixel Perfect(640×360·PPU 2)로 맞춤(다르면). 로고 차트 선·광대·글자·버튼은 그대로. Play에서 임시 배치 캡처(지금 / 적용 후)를 먼저 보여 주고, 확인받은 뒤 Scene 저장. 코드는 AI가 Scripts에 직접 작성(4장 예외), Scene은 MCP.

**Blocked by:** 없음

**Status:** ready-for-agent

- [ ] 시안 캡처(지금 / 적용 후) 사용자 확인
- [ ] TitleScene 조명 2개 + 방·테이블·전등·빛줄기·시세 선, 재질(전등·빛줄기·시세 선 = 조명 안 받음) (C1)
- [ ] 캡처 밝기: 전등 아래(광대 뒤 벽·테이블)가 왼쪽 위(제목 자리)보다 밝음 (C2)
- [ ] 시세 선이 흐르고, 타이틀에서 8~12초마다 급락, `game` 없어도 오류 없음 / GameScene에선 급락 꺼짐 (C3)
- [ ] 제목·안내·버튼 읽힘, Space·클릭으로 GameScene 진입, Esc 종료 그대로 (C4)
- [ ] GameScene 퇴행 T65~T68·T143, Console 오류 0 (C5)

Spec: `../spec.md` (제안 1~8) · Grilling: `../grilling.md` (Q1~Q8)

## 시연 순서
1. 게임 실행 → 타이틀: 어두운 방, 광대 머리 위 전등에서 노란빛, 벽에 시세 선이 흐름
2. 10초쯤 기다림 → 시세 선이 한 번 급락(떨어지는 칼)
3. 제목·조작 안내 읽기 → Space로 시작 → 게임 화면은 전과 같음

## Comments
