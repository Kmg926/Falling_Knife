# 에셋 권리·크레딧 기록

> 최종 제출 때 그대로 쓰는 문서. 기말 평가 "AI 투명성·선택 근거·권리" 항목 근거.
> **프로젝트 성격:** 대학 수업 과제, 전 과정 비영리 (사용자 확인 2026-11-02).

## 1. 외부 에셋

| # | 에셋 | 버전 | 제작자 | 출처 | 라이선스 | 받은 날 | 사용 파일 | 의무 | 확인 |
|---|---|---|---|---|---|---|---|---|---|
| 1 | Galmuri (폰트) | v2.40.4 | Lee Minseo (quiple) | https://github.com/quiple/galmuri/releases/tag/v2.40.4 | SIL OFL 1.1 | 2026-11-02 | `Assets/_Project/Fonts/Galmuri/`에 `Galmuri11.ttf`·`Galmuri11-Bold.ttf`·`LICENSE.txt` + TMP 에셋 `Galmuri11 SDF`만 보관 (나머지 37개는 2026-11-02 삭제, 필요하면 출처에서 다시 받음) | 라이선스 파일 동봉, 크레딧 표기, 폰트 단독 판매 금지 | LICENSE.txt 확인 ✅ |

## 2. AI 생성 에셋

> 광대·아이템·배경 등. 만들 때마다 한 줄씩 추가한다.

| # | 에셋 | 도구 (모델·버전) | 프롬프트 (요약 또는 원문 위치) | 내가 직접 보정한 부분 | 도구 이용약관상 권리 조건 | 만든 날 |
|---|---|---|---|---|---|---|
| 1 | 효과음 `impact.wav` (충격음, 0.35초) | Claude(Anthropic)가 작성한 코드 `tools/make_sfx.py` (numpy 합성, 시드 20261115), 후보 `impact-A` | 코드 안 `impact()` 변형 A: 110→45Hz 사인 + 짧은 잡음 | 없음 — **임시**: 사용자가 아직 고르지 않고 "일단 임시 효과음으로" 지시(2026-11-15), AI가 후보 A를 넣어 둠. 최종 선택 전 | 외부 소리·모델을 쓰지 않은 합성음. 이용약관 해당 없음 | 2026-11-15 |
| 2 | 효과음 `tick.wav` (초침, 0.06초) | 위와 같음, 후보 `tick-A` | `tick()` 변형 A: 3.2kHz·5.4kHz 짧은 클릭 | 없음 — 임시(위와 같음) | 위와 같음 | 2026-11-15 |
| 3 | 효과음 `click.wav` (클릭, 0.04초) | 위와 같음, 후보 `click-A` | `click()` 변형 A: 900Hz 짧은 사인 | 없음 — 임시(위와 같음) | 위와 같음 | 2026-11-15 |
| 4 | 효과음 `shuffle.wav` (셔플, 0.60초) | 위와 같음, 후보 `shuffle-A` | `shuffle()` 변형 A: 짧은 잡음 14번 | 없음 — 임시(위와 같음) | 위와 같음 | 2026-11-15 |
| 5 | 그림 `Sprites/Clown/clown-idle.png` (광대 평소, 96×96, 8색) | Claude(Anthropic)가 작성한 코드 `tools/make_art.py clown`, 시안 `clown-A` (무작위 없음) | 사용자가 그린 GDD 화면 스케치 v2 플레이 화면의 광대(게임 화면만 잘라 320×180으로 줄인 `.scratch/art/reference/sketch-play-320.png`)를 `tools/palette.txt` 16색으로 맞춤. 영역 x 112~207·y 4~99, 단말기 선 아래·떨어진 점 제거 | 없음(#42). 나비넥타이 회색은 사용자 결정. 2026-11-16 눈동자 투명 구멍을 막은 판으로 교체(보이는 모습 같음) | 외부 이미지·모델 없음(원본은 사용자 스케치). 이용약관 해당 없음. 특정 캐릭터 재현 요청(페니와이즈)은 거절, 머리 모양 시안(`-bald`)은 버림 | 2026-11-16 |
| 6 | 그림 `Sprites/Play/room.png` (방 배경 320×180) | `tools/make_art.py play` (Claude 작성, 무작위 없음) | 바탕 bg0 + 사용자 스케치의 차트 선 위치를 red0로(떨어진 점 제거) | 없음(#42: 사용자는 고르기만) | 외부 이미지·모델 없음. 이용약관 해당 없음 | 2026-11-16 |
| 7 | 그림 `Sprites/Play/table.png` (테이블 320×41, 시안 A 민무늬) | 위와 같음 `table(grain=False)` | 스케치에서 잰 사다리꼴(위 x 30~290, 아래 0~319), 위 모서리 gold0 | 없음 — 시안 A(민무늬) 사용자 선택, B(나뭇결) 버림 | 위와 같음 | 2026-11-16 |
| 8 | 그림 `Sprites/Play/terminal.png`·`terminal-qbox.png`·`leverage-box.png` (틀 3개) | 위와 같음 `frame()` | 1도트 테두리 + 안쪽 색, 크기는 스케치에서 잼 | 없음 | 위와 같음 | 2026-11-16 |
| 9 | 그림 `Sprites/Play/candle-up/down.png`·`flame-on/off.png` | 위와 같음 `candle()`·`flame()` | 캔들 5×18(심지·몸통), 불꽃 6×9 모양 문자열 | 없음 | 위와 같음 | 2026-11-16 |
| 10 | 그림 `Sprites/Clown/clown-thinking.png` (광대 생각 중) | 위와 같음 `clown_thinking()` | #5 광대에서 눈동자만 2칸 위로 | 없음 — 사용자 별도 의견 없어 이 시안으로 진행(바꾸려면 지시) | 위와 같음 | 2026-11-16 |
| 11 | 그림 `Sprites/HUD/` 틀·띠·버튼·슬롯 9개(`hud-box`·`hud-box-wide`·`hud-bar`·`button-normal/selected/disabled`·`slot-normal/selected`) | `tools/make_art.py hud` (Claude 작성, 무작위 없음) `frame()`·`button()` | 1도트 테두리 + 안쪽 색, 크기는 스케치에서 잼(광대 잔고 틀만 넓힘) | 없음(#42) | 외부 이미지·모델 없음. 이용약관 해당 없음 | 2026-11-16 |
| 12 | 그림 `Sprites/HUD/coin-on/danger/off.png` | 위와 같음 `coin()` | 7×7 모양 문자열, 금색·빨강·회색 점선 | 없음 | 위와 같음 | 2026-11-16 |
| 13 | 그림 `Sprites/HUD/item-chart/rumor/halt.png` | 위와 같음 `sketch_icon()` | 사용자가 그린 스케치 슬롯 아이콘을 팔레트로 맞춰 떼어 냄 | 없음 | 원본은 사용자 스케치 | 2026-11-16 |
| 14 | 그림 `Sprites/HUD/item-lottery.png` | 위와 같음 `lottery_icon()` | 스케치에 없어 새로 그림: 금색 표 + 크림 안 + 빨간 별 점 | 없음 | 외부 이미지·모델 없음 | 2026-11-16 |
| 15 | 그림 `Sprites/Closeup/` 패널·아이콘 칸·분류 표·배경 바둑판(`closeup-panel/iconbox/tag/dim`) | `tools/make_art.py closeup` (Claude 작성, 무작위 없음) | 1도트 테두리 틀, 바둑판 점(반투명 대신) | 없음(#42) | 외부 이미지·모델 없음 | 2026-11-16 |
| 16 | 그림 `Sprites/Closeup/item-*-big.png` (큰 아이콘 4개) | 위와 같음 `big_icon()` | #13·#14 아이콘을 2배(도트 하나 = 2×2) | 없음 | #13(사용자 스케치)·#14와 같음 | 2026-11-16 |
| 17 | 그림 `Sprites/Result/` 바탕 2·상자 2·버튼 2 | `tools/make_art.py result` (Claude 작성, 무작위 없음) | 사선 줄무늬 바탕, 1도트 테두리 상자·버튼(크기는 스케치 화면 4·5에서 잼) | 없음(#42) | 외부 이미지·모델 없음 | 2026-11-16 |
| 18 | 그림 `Sprites/Result/clown-dark.png` | 위와 같음 `clown_lights_off()` | #5 광대를 두 단계 어둡게 | 없음 | #5와 같음(원본 사용자 스케치) | 2026-11-16 |
| 19 | 그림 `Sprites/Title/` 그래프·조작 상자·시작 버튼·일시정지 어둡게·패널 | `tools/make_art.py title` (Claude 작성, 무작위 없음) | 그래프는 사용자 스케치 화면 1의 선을 떼어 냄, 나머지 1도트 테두리·바둑판 | 없음(#42) | 원본 사용자 스케치 / 외부 이미지·모델 없음 | 2026-11-16 |
| 20 | 그림 `Sprites/Clown/clown-dim.png`, `Sprites/HUD/turn-banner.png` | `tools/make_art.py revamp30` (Claude 작성, 무작위 없음) | #5 광대를 한 단계 어둡게 / 바둑판 bg0 점 320×16 | 없음(#42) | #5와 같음 / 외부 이미지 없음 | 2026-11-16 |
| 21 | 그림 `Sprites/Play/candle-back.png` (뒤집힌 캔들) | `tools/make_art.py revamp31` (Claude 작성, 무작위 없음) | 5×18 회색 테두리·남색 몸통·금색 점 | 없음(#42) | 외부 이미지 없음 | 2026-11-17 |
| 22 | 그림 `Sprites/Closeup/Spin/spin-*-0~7.png` (아이템 4종 × 회전 8장) + `spin-shadow.png` | `tools/make_art.py revamp32` (Claude 작성, 무작위 없음) `spin_frames()` | #13·#14 아이콘을 두께 3도트 물체로 세워 Y축 45°마다 정사영, 옆면 한 단계 어둡게, 2배 | 없음(#42) | 원본 #13(사용자 스케치)·#14 / 외부 이미지·모델 없음 | 2026-11-17 |
| 23 | 그림(640 격자, PPU 2) `Sprites/Play/table.png`(테이블·펠트 매트 2장, 640×160), `Sprites/HUD/hud-bar.png`(464×72), `coin-on/danger/off.png`(내 동전 탑 1개·빈 자리 고리 17×10), `coin-clown-on/spot.png`(광대 15×7) — 옛 #7·#8의 같은 이름 파일을 덮어씀 | `tools/make_art_hd.py game` (Claude 작성, 무작위 없음) | 32색 팔레트, 원근 판자·결·이음새·램프 빛, 펠트 매트 테두리·바느질, 동전 윗면 타원·옆면 층·반짝임. 시안 3개 중 사용자가 B 선택(art-hd Ticket 34) | 없음(#42) | 외부 이미지 없음 | 2026-11-17 |
| 24 | 그림(640 격자, PPU 2) `Sprites/Play/candle-up/down/back.png`(10×36), `flame-on/off.png`(12×18), `leverage-box.png`(120×76), `Sprites/HUD/slot-normal/selected.png`(50×60), `button-normal/selected/disabled.png`(116×60), `item-chart/rumor/halt/lottery.png`(42×32), `Sprites/Closeup/item-*-big.png`(68×52), `Closeup/Spin/spin-*-0~7.png`(80×52)·`spin-shadow.png`(64×6) — 옛 #7·#8·#13·#14·#21·#22의 같은 이름 파일을 덮어씀 | `tools/make_art_hd_items.py game` (Claude 작성, 무작위 없음) | 32색, 아이콘은 설계 좌표(21×16)로 모양을 정해 크기별로 따로 그림(옛 아이콘의 모양만 이어받음: 돋보기·신문·자물쇠·복권), 회전은 두께 6 정사영 | 없음(#42) | 외부 이미지 없음, 원래 아이콘 모양은 #13(사용자 스케치) | 2026-11-17 |
| 25 | 그림(640 격자, PPU 2) `Sprites/Play/terminal.png`(주식 모니터 200×124 — 옛 #7 단말기 파일을 덮어씀), `candle-up-big.png`·`candle-down-big.png`(16×36, 기준점 아래·위) | `tools/make_art_monitor.py game` (Claude 작성, 무작위 없음) | 32색, 테두리·전원 불빛·주사선·가장자리 어둡게·격자·가격 눈금·금색 기준선·"다음" 창 틀·받침대. 시안 A·B 중 사용자가 A 선택(stock-screen Ticket 40) | 없음(#42) | 외부 이미지 없음 | 2026-11-17 |
| 26 | 그림(640 격자, PPU 2) `Sprites/Play/table.png`(화면 아래까지 늘린 테이블 640×160 — #23 덮어씀), `monitor-outline.png`(금색 테두리 204×116), `clown-monitor.png`(광대 모니터 뒷면 160×80), `Sprites/HUD/zoom-panel.png`(확대 화면 480×280; 2026-11-18 item-phone 61: 키 안내 줄 지우고 차트 칸 아래로 늘림, `make_art_trade.py zoom61`), `button-normal/selected/disabled.png`(140×72 — #24 덮어씀), `slot-normal/selected.png`(60×60 — #24 덮어씀) | `tools/make_art_trade.py game` (Claude 작성, 무작위 없음) | 32색, 확대 화면(테두리·주사선·차트 칸·점선 캔들 자리), 광대 모니터 뒷면(통풍구·가운데 판·새는 빛). 사용자 수정 지시(내 모니터 앞으로, 광대 모니터 추가) 반영 | 없음(#42) | 외부 이미지 없음 | 2026-11-17 |
| 27 | 그림(640 격자, PPU 2) `Sprites/HUD/zoom-page.png`(상세 페이지 바탕 464×192), `Sprites/Closeup/closeup-iconbox.png`(100×100)·`closeup-tag.png`(60×24 — 옛 #13 계열 1배 그림을 덮어씀) | `tools/make_art_trade.py items44` (Claude 작성, 무작위 없음) | 32색, 화면색 + 주사선, 남색 테두리·나무 받침 | 없음(#42) | 외부 이미지 없음 | 2026-11-17 |
| 28 | 그림(640 격자, PPU 2) `Sprites/Play/room.png`(방 배경 640×360 — 옛 #6 방 그림을 덮어씀, 스케치의 고정 빨간 선 없앰) + 실행 중 코드가 그리는 시세 선(`ChartLinesView`, 그림 파일 없음) | `tools/make_art_stock.py room` / `Assets/_Project/Scripts/ChartLinesView.cs` (Claude 작성, 무작위 없음 — 사인·해시) | 어두운 벽·옅은 판자 이음새·걸레받이 / 빨강(red1·red0)·파랑(navy2·navy) 2줄 | 없음(#42) | 외부 이미지 없음 | 2026-11-18 |
| 29 | 그림(640 격자, PPU 2) `Sprites/Clown/clown-idle·thinking·dim.png`, `Sprites/Result/clown-dark.png`(광대 192×192 — 옛 #5·#20 계열 1배 그림을 덮어씀) | `tools/make_art_clown_hd.py draft` (Claude 작성, 무작위 없음) — 1배 그림을 Scale2x로 늘린 뒤 넓은 색 구역에만 빛·그늘 한 단계 | 원본 = 사용자가 고른 광대 시안(#5, 머리 모양 지시 반영). 윤곽·색 배치·표정 그대로 | 없음(#42) | 원본 #5와 같음, 외부 이미지 없음 | 2026-11-18 |
| 30 | 그림(640 격자, PPU 2) `Sprites/Play/candle-flash-up.png`·`candle-flash-down.png`(큰 캔들 테두리 16×36, 기준점 아래·위), `baseline-flash.png`(기준선 밝은 줄 142×2) | `tools/make_art_monitor.py flash` (Claude 작성, 무작위 없음) | cream2 테두리 / gold2 줄 | 없음(#42) | 외부 이미지 없음 | 2026-11-18 |
| 31 | 그림(640 격자, PPU 2) `Sprites/Clown/clown-idle-b·thinking-b·dim-b.png`(숨쉬기 둘째 장 192×192), `eyes-blink·eyes-glance(-dim).png`(눈 덮개 4장) + Animator 클립 7개(`Animations/`, 그림 순서·시간만) | `tools/make_art_clown_hd.py idle` (Claude 작성, 무작위 없음) — #29 그림에서 턱 아래 1도트 내림, 눈 안을 눈꺼풀 색·감은 줄로 / 눈동자 5도트 아래로 | 원본 = #29 광대 | 없음(#42) | 외부 이미지 없음 | 2026-11-18 |
| 32 | 그림(640 격자, PPU 2) `Sprites/Clown/clown-*-u·b·c·d·e.png`(숨쉬기 192×192, b는 #31을 덮어씀, 3상태 × 5장) + Animator 클립 3개 갱신(그림 순서·눈 위치만) | `tools/make_art_clown_hd.py breath` (Claude 작성, 무작위 없음) — #29 그림 전체를 1·2·3도트, 턱 아래를 그 두 배(2·4·6도트) 내림 | 원본 = #29 광대 | 없음(#42) | 외부 이미지 없음 | 2026-11-18 |
| 33 | 그림(640 격자, PPU 2) `Sprites/Play/lamp.png`(매달린 전등 80×24: 줄·갓·안쪽 빛·전구), `lamp-beam.png`(빛줄기 440×236, gold2 점 바둑판 무늬, 밀도 0.22→0.05로 낮춤), `lamp-beam-title.png`(타이틀용 440×274, 테이블 모서리까지) | `tools/make_art_lamp.py draft`·`pool` (Claude 작성, 무작위 없음) | 없음(새로 그림) | 없음(#42) | 외부 이미지 없음 | 2026-11-18 |
| 34 | 그림(640 격자, PPU 2) `Sprites/Play/lamp-pool.png`(테이블 위 빛 웅덩이 600×110, gold2 점 바둑판 무늬 반타원, 밀도 0.22) | `tools/make_art_lamp.py pool` (Claude 작성, 무작위 없음) | 없음(새로 그림) — 시안 2(테이블 정중앙 온전한 타원)도 만들었으나 사용자가 시안 1로 되돌림 | 없음(#42) | 외부 이미지 없음 | 2026-11-18 |
| 35 | 그림(640 격자, PPU 2) `Sprites/Phone/`: `phone-closed.png`(접힌 내 폰 80×48)·`phone-closed-outline.png`(금색 테두리 84×52)·`phone-open.png`(펼친 내 폰 200×330, 화면 2×2 칸 자리 + 키패드 3×4)·`phone-key-lit-1~4.png`(금색 키 44×20)·`phone-clown.png`(펼쳐 세운 광대 폰 64×88)·`phone-clown-flash.png`(반짝임 테두리 26×20)·`mini-chart/rumor/halt/lottery.png`(작은 아이템 아이콘 24×18) | `tools/make_art_phone.py draft` (Claude 작성, 무작위 없음) | 없음(새로 그림) — 사용자 고침 지시 1번: 펼친 폰 폭 좁게(300→200)·화면 2×2 | 없음(#42) | 외부 이미지 없음 | 2026-11-18 |
| 36 | 효과음 `Audio/Sounds/buzz.wav` (진동음 "지잉 지잉", 0.48초) | Claude(Anthropic)가 작성한 코드 `tools/make_sfx.py buzz` (numpy 합성, 시드 20261115), 후보 `buzz-B` | `buzz()` 변형 B: 170Hz 톱니파+사인 0.2초 ×2, 쉼 0.08초. 사용자 지시로 최대 −6 → −12 dBFS(볼륨 절반) | 없음 | 외부 소리 없음 | 2026-11-18 |

## 3. 게임 내 크레딧 문구

타이틀 메뉴 "크레딧" 창에 표시(title-menu Ticket 55, 2026-11-18). 문구는 `TitleMenu.cs`의 `credits` 칸. 공개 저장소라 만든 사람은 GitHub 이름만(본명·학번·학교 없음).

```
Falling Knife
만든 사람   Kmg926
엔진   Unity 6.6 (URP 2D)
글꼴   Galmuri11 — Lee Minseo (quiple)
        SIL Open Font License 1.1
        github.com/quiple/galmuri
그림·효과음   Claude(Anthropic)가 작성한 코드로 생성,
        고르기·고칠 곳 지시는 만든 사람
코드 도움   Claude(Anthropic)
배경음악   (정해지면 추가)
비영리 학교 과제
```

## 제출 전 확인
- [ ] 빌드 폴더나 제출물에 Galmuri `LICENSE.txt` 동봉
- [x] 게임 안(타이틀 또는 결과 화면) 또는 README에 크레딧 문구 표기 — 타이틀 "크레딧" 창(title-menu 55). 배경음악이 정해지면 문구에 추가
- [ ] 2장의 AI 생성 에셋을 모두 기록했는지, 각 도구의 이용약관을 다시 확인
