# title-bg Ticket 52: 타이틀에서 어두운 방과 흐르는 시세가 보인다

> AI가 Scripts에 직접 쓰고(title-bg Grilling Q8, CLAUDE.md 4장 예외), Scene은 MCP로. 면담 대비로 읽어 둔다.
> 근거: `../../spec.md`(제안 1~8), Grilling Q1~Q8, 시안 2차 사용자 선택("2차로 저장"). 전 코드 `ChartLinesView.before.cs`.

## 바꾼 것
| 대상 | 내용 | 왜 |
|---|---|---|
| `ChartLinesView.cs` | 칸 `idleSpikeMin`·`idleSpikeMax`(기본 0 = 끔), `Update`에서 간격이 0보다 크면 그 범위 임의 시간마다 `StartSpike(downDots)` | 타이틀엔 게임 상태(공개)가 없어 급락할 계기가 없음 → 가끔 스스로 급락해 "떨어지는 칼"을 보여 줌. GameScene은 0이라 전처럼 공개 결과에만 반응 |
| TitleScene `TitleArt` | `Room`(−160, 90, 정렬 −10)·`Table`(−160, **−58**, 10)·`Lamp`(**93**, 90, 6, 조명 안 받음)+`LampLight`(Spot, 게임과 같은 값)·`LampBeam`(93, 79, −1, 알파 0.2)·`ChartLines`(**0**, 72, −5, 폭 **320**, 급락 8~12초)·`Global Light 2D`(0.35·남색) | 게임 스프라이트·조명 재사용(새 그림 없음). 전등은 광대(UI 가운데 x 93) 위, 테이블은 광대 몸 아래 끝 높이로 내림 |
| `TitleCanvas` 정렬 순서 | 0 → **100** | 2D 화면은 거리 대신 정렬 순서로 앞뒤를 정함 → 테이블(10)·전등(6)이 UI를 가렸음(1차 시안). GameScene HUD도 100 |
| 시세 선 폭 | 640 → **320**(화면 오른쪽 절반) | 화면 전체 폭이면 로고 "Falling Knife"를 가로지름(1차 시안) |

## 핵심 개념 (면담용)
| 개념 | 설명 |
|---|---|
| 정렬 순서(Sorting Order) vs 거리 | URP 2D는 같은 정렬 층에서 숫자가 큰 것이 앞. Canvas(Screen Space Camera)도 이 숫자로 섞여 그려짐 → UI를 앞에 두려면 Canvas 순서를 크게 |
| 같은 코드 다른 설정 | `ChartLinesView` 하나를 게임(급락 0, 공개에 반응)·타이틀(급락 8~12초, 게임 없음)에서 Inspector 값만 다르게 씀 |

## 테스트 (Ticket 52)
| # | 방법 | 기대 | 결과 |
|---|---|---|---|
| C1 | Play에서 조명·재질·정렬 읽기 | 조명 2개(전체 0.35·Spot 1.3), 전등·빛줄기·시세 선 Unlit | ✅ |
| C2 | 캡처 밝기(0~255) | 전등 아래 > 제목 자리 | ✅ 빛줄기 32·광대 뒤 벽 19·테이블 18 / 제목 자리·왼쪽 벽 5 |
| C3 | 13초 동안 시세 선 | 흐름(step 320→710), 급락 −36 한 번(10.5초) | ✅ |
| C4 | 시작 버튼 | GameScene으로, GameScene 시세 선 급락 0 | ✅ |
| C5 | GameScene 퇴행 T65~T68·T143, Console 오류 | 같음, 0 | ✅ |
