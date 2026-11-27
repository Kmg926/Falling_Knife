# 115 줄별 설명 — 방이 살아 있게 깜빡인다 (RoomFlicker, 새 파일)

> 2026-11-27, room-dress Grilling Q7·Q10(AI가 draft + Scripts 직접). 꾸밈 전용 — 판정과 상관없음.

| 줄 | 무엇을 | 왜 |
|---|---|---|
| `leds`·`ledsA`·`ledsB` | LED만 그린 겹침 그림 2장(`room-leds-a·b.png`)을 번갈아 | 장비 불빛이 켜졌다 꺼짐. LED 하나하나를 오브젝트로 만들지 않고 그림 2장으로 |
| `ledSeconds = (0.6, 1.6)` | 바뀌는 간격을 이 범위에서 매번 무작위 | 규칙적이면 기계적으로 보여서 |
| `lampLights`(LampBeam·LampPool) | 전등 빛 두 그림의 투명도만 잠깐 낮춤 | 그림은 그대로, 다른 코드는 이 그림을 안 건드림(먼저 확인) |
| `flickerGap = (4, 9)`·`dimAlpha 0.7`·`dimSeconds 0.06` | 4~9초마다 0.06초 동안 70%로, 40%는 두 번 연달아 | 낡은 전등 느낌, 시선을 뺏지 않게 짧고 약하게 |
| `Start()` | 원래 투명도 기억 | 깜빡인 뒤 정확히 원래대로 |
| `Update()` | 타이머 두 개(LED·전등) | Time 기준 → 일시정지 중엔 같이 멈춤 |
| `Flicker()` 코루틴 | 옅게 → 0.06초 → 원래 → 0.08초 (×1~2) | |

## GameScene (MCP)
| 대상 | 값 |
|---|---|
| PlayArt/RoomLeds | Room과 같은 자리(−160, 90), 순서 −9(배경 바로 위), 재질 Sprite-Unlit-Default(조명 영향 없이 밝게 — 처음 Lit이라 어두웠음), RoomFlicker 연결 |
| PlayArt/ChartLines | 폭 640 → 448, 높이 160 → 152, 자리 (−160, 72) → (−112, 70) = 시세판 안쪽 화면(코드는 그대로, Scene 값만) |
