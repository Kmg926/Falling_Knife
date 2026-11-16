# ui-revamp Ticket 33 초안: 피해와 버튼 누름이 손에 느껴진다

> AI가 Scripts에 직접 쓴다(Grilling Q8, CLAUDE.md 4장 예외). 면담 대비로 읽어 둔다.
> 근거: `../../spec.md` (C6·C8·C9), Grilling Q5, 조사 "피드백(주스)"·"움직임 원칙"(짧은 반응 0.1~0.2초).

## 바꾼 것
| 파일 | 할 일 |
|---|---|
| `UIManager.cs` | 칸 4개(`screenShake`·`shakeTargets`·`shakeSeconds`·`shakeDots`), 피해 깜빡임 때 흔들림 시작, `ShakeRoutine`·`HomesOf` |
| `BalanceView.cs` | 칸 2개(`dropSeconds`·`dropDots`), 잔고가 줄면 줄어든 칸마다 `DropCoin` |
| `ActionButtonsView.cs` | 칸 1개(`pressSeconds`), 내 실행으로 공개가 시작된 순간 고른 버튼 `Press` |
| `ItemSlotsView.cs` | 칸 1개(`pressSeconds`), 확인 칸이 바뀐 순간 그 칸 `Press` |

판정 코드(GameManager 등)는 손대지 않았다. 모두 **화면 쪽에서 상태 변화를 보고** 움직인다.

## 줄별 핵심
| 부분 | 왜 |
|---|---|
| `screenShake` (bool) | 흔들림을 싫어하거나 멀미하는 사람을 위한 끄기 칸(조사: 흔들림 줄이기 옵션). 설정 화면은 범위 밖이라 Inspector 칸으로 |
| `shakeTargets` = PlayArt·Clown | 카메라를 흔들면 Pixel Perfect Camera가 반 칸 위치에서 흐려질 수 있어서, **월드 그림 묶음**만 흔든다. 글자(HUD)는 읽기 쉬움(Q6)을 위해 그대로 |
| `if (shakeRoutine != null) StopCoroutine(...) else shakeHomes = ...` | 흔드는 도중 또 피해가 나면 이어서 흔든다. 이때 원래 자리를 다시 기억하면 **흔들린 자리**를 제자리로 착각하므로, 흔드는 중이 아닐 때만 기억 |
| `pattern` (2,0)(−2,0)(0,1)(−1,0) | 무작위 대신 정해진 정수 도트 무늬 → 항상 같은 느낌, 흐려짐 없음. 4단계 × 0.0375초 = 0.15초 |
| 끝에 `= shakeHomes[i]` | 무늬가 어디서 끝나든 **정확히 제자리**(T200) |
| `lastWho`·`lastBalance` | 지난번 화면의 잔고를 기억해 "줄었는가"를 안다. 라운드가 바뀌면 Participant가 새로 만들어지므로 `who == lastWho`로 새 라운드 시작을 떨어짐으로 착각하지 않음 |
| `for (i = who.Balance; i < lastBalance; ...)` | 줄어든 칸 번호(예: 5→3이면 3·4번)마다 하나씩 → 떨어진 동전 수 = 잃은 잔고(T201) |
| `DropCoin`: `Instantiate` 후 `coinOn` | 원래 칸은 이미 빈 동전 그림으로 바뀌었다. 그 위에 **금색 복사본**을 만들어 아래로 8도트 떨어뜨리고 지운다. `EaseInCubic` = 천천히 시작해 빨라짐(중력 느낌) |
| `yield return Tween.MoveAnchored(...)` 뒤 `Destroy` | 움직임이 끝날 때까지 기다린 다음 복사본을 지움 → 남는 물건 0 |
| `IsRevealing && !wasRevealing && Current == Player` | 공개가 **막 시작된 한 순간**만 잡는다(전 프레임은 아니었는데 지금은 공개 중). 클릭이든 Space든 같은 순간이라 입력 코드를 고치지 않아도 됨. 광대 차례 실행은 제외 |
| `Press`: −1도트 → 0.1초 → 제자리 | 조사: 버튼 눌림 = 짧게 찌그러짐/내려감, 0.1~0.2초 |
| `holdHome`·`slotHomes`를 `Awake`에서 기억 | 눌린 상태에서 또 눌러도 기준 자리가 바뀌지 않게 |
| `ConfirmingSlot != lastConfirming` | 1~4 키든 클릭이든 **확인 칸이 바뀐 순간** 한 번만 눌림 |

## 테스트 (Ticket 33)
| # | 방법 | 기대 | 결과 |
|---|---|---|---|
| T198 | 2번 칸 확인 시작 | 슬롯 y −150 → −151 → −150 | ✅ |
| T199 | 매도 실행 | 고른 버튼 y −147 → −148 → −147 | ✅ |
| T200 | 하락 보유(피해) | 흔들림 최대 2도트, 끝나면 제자리 | ✅ |
| T201 | 잔고 2 잃음 | 떨어진 동전 2개, 끝난 뒤 복사본 0 | ✅ |
| T202 | `Screen Shake` 끔 → 피해 | 흔들림 0 | ✅ |
| 퇴행 | T65~T68·T143 | 같음 | ✅ |
