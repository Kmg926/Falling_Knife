# 타이틀 광대: 몸 아래가 테이블 뒤로, 숨쉬기·깜빡임 (title-menu 이어서)

> 사용자 "타이틀의 광대 상반신 아래가 구현 안되어있는게 보여서 어색해. 그리고 타이틀의 광대에도 애니메이션 적용해줘"(2026-11-18). AI가 Scripts에 직접(4장 예외), Scene은 MCP. 전 코드 `ClownView.before.cs`.

## 원인
타이틀 광대는 **화면 UI 그림**(TitleCanvas/Clown)이라 UI 정렬(100)로 테이블(10)보다 앞에 그려져, 그림의 잘린 몸 아래 끝이 테이블 위에 그대로 보였음. UI라 Animator(스프라이트 그림 바꾸기)도 붙지 않음.

## 바꾼 것
| 대상 | 내용 |
|---|---|
| TitleScene `TitleArt/Clown`(새) | 게임 화면 광대와 같은 구성: SpriteRenderer `clown-idle`(조명 받음, 정렬 0) + 자식 `Eyes`(정렬 1) + Animator `ClownAnimator`(Always Animate) + `ClownView`(게임 칸 비움). 자리 (93, −64) = 기준점(아래 가운데)이 테이블 위 끝(−58)보다 6칸 아래 → 몸 아래가 테이블(정렬 10) 뒤로 숨음. 숨쉬기로 1칸 올라가도 안 보임 |
| TitleScene `TitleCanvas/Clown`(UI) | 끔(지우지 않음 — 되돌리기 쉽게) |
| `ClownView.cs` `Start` | `game`이 비어 있으면(타이틀) `IsLit = true` → 밝은 평소 상태로 숨쉬기·깜빡임·힐끗. 게임 화면은 `game`이 있어 전과 같음(Refresh가 매번 정함) |

## 확인 (Play)
| 방법 | 기대 | 결과 |
|---|---|---|
| 3.5초 동안 몸 그림 순서 | u → a → b → c → d → e → d … (0.25초씩, 맨 위·아래 0.55초) | ✅ |
| 깜빡임 타이머 1초로 | `eyes-blink`(밝은 눈) | ✅ |
| 캡처 | 몸 아래가 테이블 뒤, 전등 빛을 받음 | ✅ `.scratch/art/drafts/title-menu/title-clown.png` |
