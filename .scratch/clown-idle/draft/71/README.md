# draft/71: 타이틀 광대 눈 발작 (clown-idle 이어서)

사용자: "타이틀 화면의 광대 눈이 기괴하게 떨리거나 불규칙하면 좋겠음" → 제안(바늘·갈라짐·위로 그림 + 불규칙 떨림) → "발작형, 2도트", 시안 "이대로 진행".
코드는 AI가 Scripts에 직접 썼다(CLAUDE.md 4장 예외, clown-idle과 같은 방식). 그림은 `tools/make_art_clown_hd.py twitch`(AI 코드).

## 그림 (`Sprites/Clown/eyes-twitch-pin·cross·up.png`)
- 힐끗 덮개와 같은 방식: 눈 안쪽을 얼굴색으로 지우고 눈동자를 다시 그림. pin = 가운데 2×2, cross = 왼눈 왼쪽·오른눈 오른쪽 3도트, up = 위로 5도트. 비교 `eyes-twitch-sheet-x4.png`.

## ClownEyeTwitch.cs (새 파일, 타이틀 광대에만)
| 부분 | 왜 |
|---|---|
| `eyes`·`twitch`·`twitchSprites` | 원래 눈 덮개(Animator가 숨쉬기에 맞춰 움직임) / 따로 둔 발작 덮개 / 그림 3장 |
| `quietMin~Max` 0.5~3, `burstMin~Max` 0.3~1, `stepMin~Max` 0.03~0.08 | 조용 → 발작 → 조용. 발작 동안 그림·흔들림이 자주 바뀜 |
| `jitterDots` 2, `pixelsPerUnit` 2 | 흔들림 ±2도트(사용자), 1도트 = 0.5 |
| `LateUpdate` | Animator가 이번 프레임에 Eyes 자리(숨쉬기 y)를 정한 **뒤에** 발작 덮개를 그 자리 + 흔들림으로 → 숨쉬기와 안 어긋남. Animator·클립은 건드리지 않음 |
| `Time.deltaTime` | 시간이 멈추면 같이 멈춤 |

## Scene (MCP, TitleScene `TitleArt/Clown`, 저장 전 dirty 아님 확인)
- 새 자식 `TwitchEyes`(SpriteRenderer, Eyes와 같은 재질·정렬 +1, 처음 꺼짐) + `ClownEyeTwitch`. 게임 광대(GameScene)는 그대로.

## 확인
- 발작을 바로 시작시키고 시간을 느리게: `TwitchEyes` 켜짐, 그림 `eyes-twitch-up`, 자리 = Eyes 자리(y −2, 숨쉬기 중) + 흔들림. 캡처 `twitch-crop.png`(눈동자가 위로 쏠림). 컴파일 오류 0.

## 고침: 눈동자만 흔들리게 (사용자 "눈동자만 움직여야 하는데 흰 배경도 움직여서 파란 선(눈 화장?)을 넘어가")
- 원인: 발작 덮개 한 장에 눈 안쪽 흰 바탕 + 눈동자가 같이 있어 통째로 ±2도트 → 흰 바탕이 파란 테두리를 넘음.
- 그림(`twitch` 명령이 더 만듦): `eyes-twitch-fill`(눈 안쪽 흰 바탕), `eyes-twitch-mask`(눈 안쪽 모양 가리개), `eyes-twitch-pin/cross/up-pupil`(눈동자만).
- `ClownEyeTwitch.cs`: `fill`(SpriteRenderer)·`mask`(SpriteMask)는 숨쉬기 눈 자리에 그대로, `pupil`만 + 흔들림. 눈동자는 `VisibleInsideMask`라 가리개 밖(테두리 밖)으로 나간 부분은 안 보임. 전 코드 `ClownEyeTwitch.before-pupil.cs`.
- TitleScene: `TwitchEyes` → `TwitchFill`(흰 바탕), 새 `TwitchMask`·`TwitchPupil`(정렬 Eyes+2).
- 확인: 발작 + 흔들림 (+2, +2도트)을 강제로: Eyes·Fill·Mask 자리 같음(y −1.5), Pupil만 (1, −0.5). 캡처 `twitch-pupil-crop.png` — 파란 테두리 그대로, 넘친 눈동자는 가려짐.
