# 52: 타이틀에서 어두운 방과 흐르는 시세가 보인다

**What to build:** TitleScene에 빈 부모 `TitleArt`를 두고, 그 아래 게임 스프라이트를 그대로 배치한다(새 그림 없음): `Room`(`Sprites/Play/room.png`, 화면 전체), `Table`(`table.png`, 광대 몸 아래 끝 근처로 **내려서** 앞부분만 보이게), `Lamp`(`lamp.png`, **광대 머리 위** x, 위 끝 y 90, 조명 안 받음) + 자식 `LampLight`(Spot Light 2D, 게임과 같은 값), `LampBeam`(`lamp-beam.png`, 같은 x, 알파 0.2, 조명 안 받음, 정렬 −1), `ChartLines`(`ChartLinesView`, `game` 비움, 게임과 같은 자리, 조명 안 받음), `Global Light 2D`(0.35·(0.75, 0.8, 1)). 정렬 순서·재질은 GameScene과 같게. `ChartLinesView`에 칸 `idleSpikeMin`·`idleSpikeMax`(초, 0이면 끔)를 더해 타이틀에서만 8~12초마다 급락(`StartSpike(downDots)`) — GameScene 값은 0으로 그대로. 타이틀 카메라를 GameScene처럼 Pixel Perfect(640×360·PPU 2)로 맞춤(다르면). 로고 차트 선·광대·글자·버튼은 그대로. Play에서 임시 배치 캡처(지금 / 적용 후)를 먼저 보여 주고, 확인받은 뒤 Scene 저장. 코드는 AI가 Scripts에 직접 작성(4장 예외), Scene은 MCP.

**Blocked by:** 없음

**Status:** done

- [x] 시안 캡처(지금 / 적용 후) 사용자 확인
- [x] TitleScene 조명 2개 + 방·테이블·전등·빛줄기·시세 선, 재질(전등·빛줄기·시세 선 = 조명 안 받음) (C1)
- [x] 캡처 밝기: 전등 아래(광대 뒤 벽·테이블)가 왼쪽 위(제목 자리)보다 밝음 (C2)
- [x] 시세 선이 흐르고, 타이틀에서 8~12초마다 급락, `game` 없어도 오류 없음 / GameScene에선 급락 꺼짐 (C3)
- [x] 제목·안내·버튼 읽힘, Space·클릭으로 GameScene 진입, Esc 종료 그대로 (C4)
- [x] GameScene 퇴행 T65~T68·T143, Console 오류 0 (C5)

Spec: `../spec.md` (제안 1~8) · Grilling: `../grilling.md` (Q1~Q8)

## 시연 순서
1. 게임 실행 → 타이틀: 어두운 방, 광대 머리 위 전등에서 노란빛, 벽에 시세 선이 흐름
2. 10초쯤 기다림 → 시세 선이 한 번 급락(떨어지는 칼)
3. 제목·조작 안내 읽기 → Space로 시작 → 게임 화면은 전과 같음

## Comments
- 2026-11-18 AI 작업(사용자 "커밋 후 52", 커밋 `545551d`): `ChartLinesView`에 `idleSpikeMin`·`idleSpikeMax`(0이면 끔) 추가(Scripts 직접, 전 코드 `draft/52/ChartLinesView.before.cs`), 컴파일 오류 0. TitleScene 확인: 카메라 Pixel Perfect 있음(제안 6 그대로), 광대 UI x 45~141·y −58~38(가운데 x 93).
  - Play에서만 임시 배치(Scene 저장 안 함): 전등·빛 x 93, 테이블 위 끝 y −58, 조명 게임과 같음, 시세 선 + 가끔 급락 8~12초.
  - 1차 캡처 문제 2개: ① **테이블이 조작 안내·버튼을 가림** — 2D 정렬은 깊이가 아니라 정렬 순서로 그려서, 타이틀 Canvas(순서 0)보다 테이블(10)·전등(6)이 앞 ② **시세 선이 로고 "Falling Knife"를 가로지름**.
  - 고친 2차: ① `TitleCanvas` 정렬 순서 0 → **100**(GameScene HUD와 같음) ② 시세 선을 **오른쪽 절반**(x 0~160, 텍스처 폭 320)만 — 광대 뒤 벽에서만 흐름. 캡처 `.scratch/art/drafts/title-bg/title-before.png`·`title-after1.png`·`title-after2.png`. 사용자 확인 대기.
- 2026-11-18 사용자 "2차로 저장": TitleScene에 `TitleArt`(방·테이블·전등+스포트·빛줄기·시세 선·전체 조명) 배치, `TitleCanvas` 정렬 100, 시세 선 폭 320·급락 8~12초 → 저장(MCP, 저장 전 사용자 변경 없음 확인 — dirty false).
  - 테스트 C1~C5 모두 통과(값은 `../draft/52/README.md`): 밝기 전등 아래 18~32 / 제목 자리 5, 시세 선 첫 급락 10.5초, 시작 버튼 → GameScene(시세 선 급락 0), 퇴행 T65~T68·T143, Console 오류 0. 캡처 `.scratch/art/drafts/title-bg/title-52.png`.
  - **남은 것(사용자):** 직접 보고 분위기·급락 빈도 확인.
