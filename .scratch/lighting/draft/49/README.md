# lighting Ticket 49: 어두운 방에서 전등이 판을 노랗게 비춘다

> 새 게임 코드 없음 — Scene 값(조명·재질·오브젝트)만 AI가 MCP로 바꿈. 그림은 `tools/make_art_lamp.py`(AI 작성). 면담 대비로 읽어 둔다.
> 근거: `../../spec.md`(제안 1~8), Grilling Q1~Q8, 시안 B안(빛줄기가 광대 뒤) 사용자 선택.

## 바꾼 것 (Scene `GameScene`)
| 대상 | 값 | 왜 |
|---|---|---|
| `Global Light 2D` | 밝기 1 → **0.35**, 색 (0.75, 0.8, 1) 살짝 남색 | 방 전체를 어둡고 차갑게 → 전등 빛이 도드라짐 |
| 새 `PlayArt/Lamp` | 그림 `Sprites/Play/lamp.png`(80×24 새 도트, 기준점 위 가운데), 자리 (0, 90), 정렬 6(광대 앞), 조명 안 받는 재질 | 빛이 어디서 오는지 보이게. 화면 위 끝에서 매달림, 갓이 광대 머리카락 꼭대기에 살짝 겹침 |
| 새 `Lamp/LampLight` | **Spot**(Light 2D의 Point 종류 + 각도): 자리 (0, 78), z 180°(아래 방향), 안쪽 40°·바깥 100°, 반지름 60·190, 색 (0.85, 0.71, 0.35), 밝기 1.3, Falloff 0.5 | 광대·테이블 가운데·내 모니터까지 원뿔로 노란빛 |
| 새 `PlayArt/LampBeam` | 그림 `lamp-beam.png`(440×236, gold2 점 바둑판 무늬), 자리 (0, 79), **정렬 −1(광대 뒤, 시세 선 앞)**, 색 알파 0.2, 조명 안 받는 재질 | 벽에 비친 빛줄기. 광대 앞(A안)은 점무늬가 얼굴을 가려 B안으로 |
| 재질 → `Sprite-Unlit-Default` | `ShownCandle`·`KnownCandle`·`FlashOutline`·`BaselineFlash`·`MonitorOutline`·`Flame1~3`·`ChartLines` | 스스로 빛나는 물건(모니터 화면 안·불꽃·벽 시세 선)은 어둠에 안 묻히게 |

## 핵심 개념 (면담용)
| 개념 | 설명 |
|---|---|
| `Sprite-Lit-Default` vs `Sprite-Unlit-Default` | Lit = 조명(Light 2D)을 받아 밝기가 바뀌는 재질, Unlit = 조명을 무시하고 그림 색 그대로. 전구·화면처럼 "스스로 빛나는 것"은 Unlit |
| Global Light 2D | 화면 전체에 깔리는 기본 밝기(방의 어둠 정도) |
| Point Light 2D + 각도 | 한 점에서 퍼지는 빛. 안쪽·바깥 각도를 좁히면 스포트라이트처럼 원뿔이 됨. 방향은 오브젝트 회전(z 180° = 아래) |
| 정렬 순서(Sorting Order) | 숫자가 클수록 앞. 방 −10 < 시세 선 −5 < 빛줄기 −1 < 광대 0 < 전등 6 < 테이블 10 |
| 화면 UI(HUD Canvas) | 조명을 안 받음 → 잔고·글자는 그대로 밝음(Q5) |

## 테스트 (Ticket 49)
| # | 방법 | 기대 | 결과 |
|---|---|---|---|
| C1 | 조명 값 읽기 | 전체 0.35·남색, LampLight Point·40°/100°·z 180°·노랑 1.3 | ✅ |
| C2 | Play 캡처 밝기(0~255 평균) | 전등 아래 > 위 구석 벽 | ✅ 광대 얼굴 109·테이블 가운데 28·빛 닿은 벽 26 / 왼위·오위 벽 5 |
| C3 | 재질 이름 | 9개 Unlit, 나머지 Lit | ✅ |
| C4 | 화면 UI | Canvas라 조명 영향 없음, 글자 겹침 없음(캡처) | ✅ |
| C5 | 팔레트·PPU | 32색 OK, PPU 2·Point·기준점 위 가운데 | ✅ |
| C6 | 퇴행 T65~T68·T143, Console 오류 | 같음, 0 | ✅ |

캡처: `.scratch/art/drafts/hd/lamp/capture-before.png`(전) · `capture-after-a.png`·`capture-after-b.png`(시안) · `capture-49.png`(적용 후).
