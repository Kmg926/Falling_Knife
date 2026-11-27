# 115: 방이 살아 있게 깜빡인다

**What to build:** RoomFlicker: 장비 LED 번갈아 깜빡임, 전등 빛 가끔 약하게.

**Blocked by:** 113

**Status:** done

- [x] 구현
- [x] 게임 캡처
- [x] (사용자) 선택: A(녹슨 철판)

Spec: `../spec.md`

## Comments
- 2026-11-27 AI: `RoomFlicker.cs`(Scripts 직접) + GameScene `PlayArt/RoomLeds`(SpriteRenderer, 순서 −9, LED는 조명 영향 안 받는 Sprite-Unlit-Default — 처음 Lit이라 어두웠음). LED 그림 2장 0.6~1.6초 번갈아, 전등 빛 4~9초마다 0.06초 70%. 같은 날 잔고 전광판 꺼진 숫자를 mat0 → bg0으로 더 어둡게(7이 "87"처럼 읽혀서).
