# draft/65: 아이템을 받으면 폰이 울린다

코드는 AI가 Scripts에 직접 썼다(CLAUDE.md 4장 예외). 고치기 전 `*.before.cs`, 고친 뒤 같은 이름 `.cs`.

## 66 뒤라 바뀐 점(Ticket 원문과 다른 것)
- 광대 폰도 이제 접혀 있어 "새 칸이 톡톡 나타남"은 보일 곳이 없음 → 광대 폰도 내 폰과 같게 **진동 + "+N" 깜빡임**.
- "+N"·빈 화면은 글자가 아니라 그림(폰과 같은 원근·각도, 66과 같은 이유) — `tools/make_art_phone.py table`이 `-blank`·`-p1~p4` 그림을 더 만든다.
- 복권으로 아이템 +1을 받으면 받은 사람의 폰만 울린다.

## GameManager.cs
| 바뀐 곳 | 왜 |
|---|---|
| `PlayerGrantCount`·`PlayerLastGrant`·`ClownGrantCount`·`ClownLastGrant` | "받았다" 신호. 화면(폰)은 횟수가 바뀌었는지만 보고 그때 받은 개수(N)를 깜빡인다. 판정에는 안 씀 |
| `PlayerNewItems` | 내 폰 목록에서 "N"을 붙일 칸 수. 받으면 맨 뒤에 붙으므로 "맨 뒤 몇 칸"으로 셈 |
| `RecordGrant(who, added)` | 실제로 받은 개수가 0보다 크면 신호를 올리고 새 칸 수를 늘림(가득 차서 못 받으면 울리지 않음) |
| `ClearNewItems()` | 내 폰을 열었다 닫으면(PhoneView가 부름) 새 칸 표시를 지움 |
| `RemovePlayerItemAt(slot)` | 아이템을 쓸 때 뺀 칸이 새 칸 중 하나였으면 새 칸 수도 줄임(4곳의 `Player.RemoveItemAt`을 바꿈) |
| `GiveItems()` | `AddItem`이 성공한 수를 세어 `RecordGrant` — 뽑는 순서(무작위)는 전과 같음 |
| 복권 아이템 +1 | `got`이면 `RecordGrant(user, 1)` |
| `StartRound`에 `PlayerNewItems = 0` | 새 라운드 = 아이템을 새로 받음(바로 뒤 GiveItems가 다시 셈) |

## PhoneBuzz.cs (새 파일, 두 폰에 하나씩)
- 신호가 바뀌면 코루틴: 덱 공개 연출(`IsPresenting`, 라운드 안내)이 끝나길 기다림 → 진동음 → 0.6초 동안 0.05초마다 x +1/−1 → "+N" 0.2초 켜짐·0.2초 꺼짐을 1.2초 → 제자리, `OnDone`으로 개수 그림 다시.
- 처음 값을 0으로 두어 라운드 1의 첫 지급도 울림.
- `WaitForSeconds`라 일시정지(timeScale 0) 중엔 멈춤.

## AudioManager.cs
- `buzzClip`(62에서 고른 B, 볼륨 절반) + `PlayBuzz()`: 두 폰이 같은 순간 울려도 0.1초 안의 두 번째는 무시 → 소리는 한 번.

## PhoneView.cs·ClownPhoneView.cs
- `buzz` 칸. `Refresh`에서 `buzz.Refresh` 먼저, 깜빡이는 중(`Busy`)엔 개수 그림을 덮어쓰지 않음, 끝나면 다시 그림.
- PhoneView: 내 폰이 열려 있다가 닫히는 순간 `game.ClearNewItems()`.

## ItemSlotsView.cs
- `newMarks`(칸 오른쪽 위 금색 "N") — 내 폰을 볼 때만, 맨 뒤 `PlayerNewItems`칸에.

## Scene (MCP)
- `PhoneClosed`·`ClownPhoneClosed`에 `PhoneBuzz`(빈 화면·"+1~+4" 그림), `PhoneView`·`ClownPhoneView`의 `buzz` 칸, `AudioManager.buzzClip`, 칸 4개에 `New`("N", 크기 5.5, 처음 꺼짐).

## 확인 (Play)
- 게임 시작: 덱 공개 연출이 끝난 직후 두 폰 `Busy`(진동·깜빡임 시작), 끝나면 개수 그림 "2".
- 신호를 직접 올리고 시간을 느리게: 두 폰 "+1" 그림 + x +1 흔들림 캡처(`bz-crop.png`).
- 내 폰 열기: 새 칸 3개에 "N"(`nw-crop.png`) → 닫았다 다시 열면 "N" 없음.
- Console 오류 0. **소리는 귀로 확인하지 못함**(재생 호출만 연결).
