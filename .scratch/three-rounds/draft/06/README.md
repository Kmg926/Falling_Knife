# Ticket 06 초안: 라운드 표로 1라운드를 시작하고 화면에서 `라운드 1 / 3`을 본다

> **초안**이다(Assets 밖). 이번부터 **직접 타이핑**해서 `Assets/_Project/Scripts/`에 반영한다(CLAUDE.md 4장). 반영 후 AI는 리뷰만 한다.
> 이 Ticket은 **데이터만 옮긴다.** 반영 뒤 동작은 마일스톤 ①과 똑같아야 한다(U1). 다른 점은 맨 위 `라운드 1 / 3` 줄뿐.
> 근거: `../../spec.md`, `../../analysis.md`

## 1. 파일

| 파일 | 05 대비 |
|---|---|
| `RoundConfig.cs` | **새로** (라운드 표의 한 줄) |
| `CandleDeck.cs` | 구성 칸 3개 삭제, `SetComposition()` 추가 |
| `GameManager.cs` | `startingBalance` → 라운드 표 `rounds`, `RoundNumber`, `StartRound`·`Restart` 수정 |
| `UIManager.cs` | 맨 위 `라운드 N / 3` 줄 |

`Participant`, `LeverageCounter`, `PlayerInputHandler`, Input Actions는 그대로.

## 2. 바뀐 줄마다 왜

### RoundConfig.cs (새로)
| 줄 | 왜 |
|---|---|
| `using System;` | `[Serializable]`이 `System` 안에 있다 |
| `[Serializable]` | 이 표시가 있어야 일반 클래스가 Inspector에 칸으로 보이고 Scene에 저장된다. 비유: "이 상자는 Inspector에 펼쳐서 보여줘도 됨" 스티커 |
| `public class RoundConfig` (MonoBehaviour 아님) | 물체에 붙일 컴포넌트가 아니라 **값 묶음**이다. Participant와 같은 이유 |
| `public int startingBalance = 2;` 등 4칸 | 순수 데이터 칸이라 `public`으로 둔다(규칙 없음). `= 2`는 새로 만들 때의 기본값 |
| `fixedOrder = ""` | 라운드마다 구성이 달라서 고정 순서도 줄마다(Q6). 빈칸이면 무작위 |

### CandleDeck.cs
| 줄 | 왜 |
|---|---|
| `[Header("라운드 구성")]`와 `downCount/upCount/fixedOrder`의 `[SerializeField]` 삭제 | 같은 값이 덱과 라운드 표 두 곳에 있으면 헷갈린다(Q10). 이제 **라운드 표가 주인** |
| `useFixedOrder`만 `[SerializeField]`로 남김 | 고정 순서를 쓸지 말지는 덱의 테스트 스위치(Q6) |
| `int downCount; int upCount; string fixedOrder = "";` (Inspector에 안 보임) | 받은 구성을 **저장해 두는 칸**. 저장하지 않으면 재장전(`Fill`) 때 쓸 값이 없다(analysis 3절) |
| `SetComposition(int down, int up, string order)` | GameManager가 라운드 시작 때 이번 구성을 넘겨주는 입구 |
| `fixedOrder = order ?? "";` | `??`는 "왼쪽이 비어 있으면(null) 오른쪽 값". 혹시 null이 와도 `ToUpper()`에서 에러가 나지 않게 |
| `Fill()` 이하 | **그대로.** 칸 이름이 같아서 고칠 필요가 없다. 재장전도 저장된 구성을 쓰므로 U3 충족 |

### GameManager.cs
| 줄 | 왜 |
|---|---|
| `[SerializeField] RoundConfig[] rounds = { ... }` | 라운드 표 3줄. `[]`는 배열(같은 종류를 여러 개 담는 칸). `{ new ... }`는 처음 기본값 2/2/3, 3/3/4, 4/4/4 |
| `startingBalance` 칸 삭제 | 라운드 표 1줄로 옮김 |
| `public int RoundNumber { get; private set; }` | 지금 라운드(1부터). 밖에서는 읽기만 |
| `public int RoundCount => rounds.Length;` | 전체 라운드 수. UI가 `/ 3`을 숫자로 박지 않도록 |
| `Start()`에 `RoundNumber = 1;` | 게임 시작은 1라운드 |
| 주석 "라운드 번호는 여기서 바꾸지 않는다" | analysis의 주의점 2: StartRound 안에서 1로 만들면 "다음 라운드가 늘 1라운드" 버그 |
| `RoundConfig config = rounds[RoundNumber - 1];` | 배열은 **0부터** 센다. 1라운드 = `rounds[0]` |
| `new Participant("플레이어", config.startingBalance)` | 잔고를 표에서 읽음 |
| `deck.SetComposition(...)` → `deck.Fill()` | **순서 중요:** 구성을 먼저 넘겨야 Fill이 그 구성으로 섞는다 |
| `Restart()`에 `RoundNumber = 1;` | R은 항상 1라운드부터(Q9). 06에서는 1라운드뿐이라 동작이 05와 같다 |
| 나머지 | **그대로** (07·08에서 판정을 나눈다) |

### UIManager.cs
| 줄 | 왜 |
|---|---|
| `$"라운드 {game.RoundNumber} / {game.RoundCount}\n"` 맨 위 | Q5. 6줄 → 7줄 |

## 3. Unity Editor에서 할 순서

1. **커밋 확인:** 구현 전 정상 버전 `406b1cf` (이미 함)
2. `Scripts` 폴더 우클릭 → `Create > Scripting > Empty C# Script` → `RoundConfig` → 타이핑
3. `CandleDeck`, `GameManager`, `UIManager` 수정 → 저장 → Console 빨간 에러 0
4. `GameManager` 선택 → Inspector에 **`Rounds` 3줄**이 2/2/3, 3/3/4, 4/4/4로 보이는지 확인
   - 🔶 analysis 5절: 이미 Scene에 있던 컴포넌트라 기본값이 들어오는지 여기서 확인. 비어 있으면(0이나 0줄) 직접 입력
5. `CandleDeck` Inspector에 `Use Fixed Order`만 남았는지 확인
6. `StatusText`가 7줄이라 잘리면 Height를 늘림
7. Ctrl+S → Play

## 4. 수동 테스트 (Ticket 06)

| # | 준비 | 입력 | 기대 화면 |
|---|---|---|---|
| T37 | 기본 | Play | 맨 위 `라운드 1 / 3`, 잔고 2/2, 남은 2/3, 플레이어 차례 (C1) |
| T38 | — | Inspector 확인 | GameManager `Rounds` 3줄 기본값, CandleDeck엔 `Use Fixed Order`만 |
| T39 | `Rounds[0]` 잔고 3 | Play | 잔고 3/3 → 끝나면 2로 되돌림 |
| T40 | `Rounds[0]` 고정 `D U U D U` + 체크 | Play | Console `Down Up Up Down Up`, 경고 없음 |
| T41 | `Rounds[0]` 고정 `D D D U U` + 체크 | Play | 노란 경고(구성과 다름), 무작위 |
| T42 | `Rounds[0]` 고정 `U U D U D` | `candle-deck-turn` 05 시연 순서(Space×3 → 패배 → R) | 05와 같은 결과, R 뒤에도 `라운드 1 / 3` (U1, C2) |
| T43 | `Rounds[0]` 고정 `D U U D U` | (D → Space)×5 | 04처럼 `덱 재장전 (하락2 / 상승3)` (U3) |

끝나면 고정 순서 끄기 → Ctrl+S.

## 5. 반영 후
"반영했어"라고 알려주면 AI가 **리뷰만** 한다: 초안과 다른 점, 버그 가능성, Inspector 값, Console(MCP).
