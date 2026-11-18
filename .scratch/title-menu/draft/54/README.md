# title-menu Ticket 54: 설정을 바꾸면 게임에 남는다

> AI가 Scripts에 직접 쓰고(title-menu Grilling Q9, CLAUDE.md 4장 예외), Scene은 MCP로. 면담 대비로 읽어 둔다.
> 근거: `../../spec.md`(제안 4·7·8), Grilling Q5. 전 코드 `*.before.cs`.

## 바꾼 것
| 파일 | 할 일 |
|---|---|
| 새 `GameSettings.cs` | static 설정: 전체 소리·음악(0~10단계), 화면 흔들림·전체화면(켬/끔). `PlayerPrefs` 키 `fk.master`·`fk.music`·`fk.shake`·`fk.fullscreen`, 기본값 = 전과 같은 값(100%·60%·켬·지금 창) |
| `TitleMenu.cs` | 창 종류에 `Settings` 추가, 설정 4줄(`ShowSettings`), `MoveSetting`(줄)·`ChangeSetting`(값), 마우스 올리기·클릭, 바꿀 때 클릭음 |
| `AudioManager.cs` | `Awake`에서 `masterVolume`·`musicVolume`을 `GameSettings`에서 읽음 |
| `UIManager.cs` | `Awake`에서 `screenShake = GameSettings.Shake` |
| TitleScene | `TitleSfx`(AudioSource, 자동 재생 끔) + `click.wav` 연결 |

## 줄별 핵심
| 부분 | 왜 |
|---|---|
| `static class GameSettings` | 타이틀과 GameScene은 씬이 달라 오브젝트를 같이 못 씀 → 컴퓨터 저장소(PlayerPrefs)를 통해 같은 값을 봄 |
| setter에서 `Mathf.Clamp(value, 0, Steps)` + `PlayerPrefs.Save()` | 0% 아래·100% 위로 안 넘어감, 바꾸는 즉시 저장(게임이 갑자기 꺼져도 남음) |
| `Fullscreen` setter의 `Screen.fullScreen = value` | 저장과 함께 바로 창에 적용(에디터에선 효과 없음 → 빌드에서 확인) |
| `ChangeSetting(dir)`: 소리 `+= dir`, 켬/끔은 `= !값` | ←는 줄이고 →는 늘림. 켬/끔은 방향 상관없이 바뀜 |
| `PlayOneShot(clickClip, MasterVolume)` | 소리를 바꾸면 그 크기로 바로 들려 줌. 0%면 안 냄 |
| `TMP_TextUtilities.FindIntersectingLine` | 글 한 덩어리에서 마우스가 몇 번째 줄 위에 있는지 → 줄마다 오브젝트를 따로 만들지 않아도 됨 |
| `<color=#…>` 와 `<pos=40%>` (TMP 태그) | 고른 줄만 금색, 이름 길이와 상관없이 값이 같은 자리에서 시작 |
| `AudioManager.Awake`(Start보다 먼저) | `Start`의 `ApplyMusicVolume`이 이미 설정값으로 계산 |

## 테스트 (Ticket 54)
| # | 방법 | 기대 | 결과 |
|---|---|---|---|
| — | 설정 창 열기·줄 돌기·값 바꾸기, 100%에서 더 올리기, 글 넘침 | 줄 0~3에서 돎, 100%에서 멈춤, 넘침 0 | ✅ |
| C4 | 바꾼 값 `PlayerPrefs` | 음악 3(30%) 등 저장 | ✅ |
| C4 | 타이틀 다시 켬 | 전체 0%·음악 30%·흔들림 끔 그대로 | ✅ |
| C4 | GameScene `AudioManager`·`UIManager` | master 0·music 0.3·음악 소스 0.00·shake false | ✅ |
| — | 소리 0%에서 클릭 / 흔들림 끔에서 내가 맞음 | 재생 안 됨 / 화면 안 움직임 | ✅ |
| C6 | 기본값으로 되돌린 뒤 GameScene 퇴행 T65~T68·T143, Console 오류 | 같음(음악 0.6), 0 | ✅ |

테스트 뒤 설정 키는 지워 기본값으로 되돌림. 남은 확인(사용자): 빌드에서 전체화면, 실제 키·마우스로 설정 바꾸기.
