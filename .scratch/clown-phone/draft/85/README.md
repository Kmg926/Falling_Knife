# draft 85: 광대 폰을 열면 깨진 광대 폰 화면이 보인다

> 2026-11-22. 그림 코드는 AI 작성 규칙, 게임 코드는 AI가 Scripts에 직접(clown-phone Grilling Q7, CLAUDE.md 4장 예외). `PhoneView.before.cs` = 바꾸기 전.

## 그림 (`tools/make_art_phone.py clownzoom`)
| 그림 | 내용 |
|---|---|
| `phone-open-clown.png`(200×354) | `phone_open`(지금 게임 그림과 똑같음 확인)을 바탕으로 남색 → 빨강(화면은 그대로), 키 "6" 금, 키 "*" 빠짐(구멍 + 접점 2개), 뚜껑 왼쪽 위 테이프, 끊기 키는 밝은 빨강(`red3`, 빨간 몸통에 묻혀서) |
| `phone-crack.png`(200×354, 투명) | 화면 오른쪽 위 충격점에서 갈라진 금 + 왼쪽 아래 짧은 금, 가운데(칸 2×2)는 비움 |
| `phone-glitch.png`(172×10) | 화면 폭 가로줄 노이즈(초록·회색·빨강) |
- 아이템 키 1~4(금색 조각이 겹치는 자리)는 그대로. 32색 OK. 미리보기 `clownzoom-preview-x2.png`(내 폰 / 광대 폰 + 금 / + 글리치 띠).
- 시안 고침 1번: 끊기 키가 빨간 몸통에 묻힘 → `red3`, 테이프가 작아 안 보임 → 띠 넓힘.

## 줄별 설명 (면담 대비)
### PhoneView.cs (바뀐 줄)
- 새 칸 `bodyImage`·`mineBody`·`clownBody`·`clownCrack`·`clownGlitch`.
- `Refresh`에서 `clown`(광대 폰을 보는가)에 따라: 폰 그림을 바꾸고, 금 그림을 켜고 끄고, `ScreenGlitch`를 켜고 끔. 내 폰이면 원래 그림·금 없음·글리치 없음.

### ScreenGlitch.cs (새)
- `Loop()`: 2~4초 기다림 → 0.1초 동안 0.05초마다 띠 높이를 화면 안에서 새로 고르고(0.5 단위 = 도트에 맞춤) 화면 글자 묶음(목록·상세·소프트키 4개)을 ±0.5 또는 ±1(새 도트 1~2칸) 가로로 밂 → `Restore()`로 띠 숨기고 제자리.
- `Awake`에서 글자 묶음의 원래 자리(`homes`)를 기억해 두고 늘 거기로 되돌림.
- **`OnDisable`에서 `StopAllCoroutines()`** — `enabled = false`로 컴포넌트를 꺼도 코루틴은 멈추지 않는다(오브젝트를 끌 때만 멈춤). 처음엔 이게 없어 광대 폰을 닫고 내 폰을 열어도 띠가 계속 나왔음(`bug-mine-still-glitching.png`) → 고친 뒤 `mine-after-fix.png`. 같은 이유로 `PhoneGlitch.cs`(84)에도 넣음.

## Scene (MCP)
- `PhoneZoom/Body` 자식: `Crack`(맨 앞 순서 = 글자 **아래**, 부모 크기, 처음 꺼짐), `GlitchBand`(86×5, 맨 뒤 순서 = 글자 위, 처음 꺼짐).
- `Body`에 `ScreenGlitch`(처음 꺼짐 — PhoneView가 켬), `PhoneClosed`의 `PhoneView` 새 칸 연결.

## 확인 (2026-11-22, MCP)
| | 결과 |
|---|---|
| C3 | 광대 폰 목록: 빨간 몸통·금 간 화면, 제목·칸 글자는 금 위로 읽힘(`clown-list.png`) / 상세(찌라시) 글리치 순간: 띠 + 글자 1도트 밀림(`clown-detail-glitch.png`) / 내 폰: `phone-open`·금 없음·글리치 없음, 광대 폰에서 긴 글리치 도중 내 폰으로 바꿔도 2초 동안 띠 없음·글자 제자리(`mine-after-fix.png`) |
| C6 | Console 오류 0. 판정 코드 안 바뀜(퇴행은 86 뒤에 한 번에) |

**남은 것(사용자):** 실제 속도에서 화면 튐이 거슬리지 않는지, 금이 글자 읽기를 방해하지 않는지.
