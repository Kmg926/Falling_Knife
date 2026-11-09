# Ticket 01 초안: 덱을 섞고 남은 하락·상승 개수를 화면에서 본다

> 이 폴더는 **초안**이다. Unity는 이 폴더를 읽지 않는다(Assets 밖).
> **2026-11-02 사용자 지시로 AI가 이 5개를 그대로 `Assets/_Project/Scripts/`에 생성했다(예외, CLAUDE.md 4장). 이유: Parsec 원격 접속 네트워크 지연으로 직접 타이핑할 수 없는 환경.** 아래 3장의 3번(스크립트 만들기)은 건너뛴다.
> 용어는 `GLOSSARY.md`를 따른다.

## 1. 파일 5개와 역할

| 파일 | 종류 | 01에서 하는 일 |
|---|---|---|
| `CandleDeck.cs` | MonoBehaviour (GameObject에 붙임) | 하락2·상승3을 섞어 순서를 정함, 남은 개수, 고정 순서 옵션 |
| `Participant.cs` | 일반 클래스 (안 붙임) | 이름과 잔고 |
| `LeverageCounter.cs` | 일반 클래스 (안 붙임) | 레버리지 값 (지금은 0만) |
| `GameManager.cs` | MonoBehaviour | 라운드 시작: 참가자 2명·레버리지·선공·덱 준비 → 화면 갱신 |
| `UIManager.cs` | MonoBehaviour | 상태를 TextMeshPro 글자로 표시 |

**왜 Participant·LeverageCounter는 MonoBehaviour가 아닌가:** 화면에 놓을 물체가 아니라 "값 묶음"이라서. GameManager가 `new`로 만들고 들고 있다. 재시작(05) 때 `new`로 다시 만들면 깨끗한 처음 상태가 된다.

## 2. 줄마다 왜 그렇게 동작하나

### CandleDeck.cs
| 줄 | 왜 |
|---|---|
| `public enum Candle { Down, Up }` | 캔들은 두 종류뿐이라 문자열 대신 enum(정해진 보기 중 하나만 고르는 칸). 오타가 컴파일 에러로 잡힌다. D/U 글자와 맞춤 |
| `[SerializeField] int downCount = 2;` | `private`인데 Inspector에 보이게 하는 표시. 2·3라운드 값으로 바꿀 수 있게(Spec: 수치 하드코딩 금지) |
| `useFixedOrder`, `fixedOrder` | 테스트 표의 같은 경우를 다시 만들려고(Q23). 평소엔 꺼 둔다 |
| `readonly List<Candle> cards` | 덱 자체. `readonly`는 "리스트를 다른 리스트로 바꿔치기 금지"(안의 내용은 바꿀 수 있음). **0번 = 맨 위** |
| `RemainingDown { get; private set; }` | 밖에서 읽기만, 바꾸는 건 덱만. UI가 실수로 값을 못 바꾼다 |
| `Fill()`의 `if (!useFixedOrder \|\| !TryFillFixedOrder())` | 고정 순서가 꺼져 있거나, 켜져 있어도 실패하면 → 무작위. `\|\|`는 왼쪽이 참이면 오른쪽을 실행하지 않으므로, 꺼져 있을 땐 고정 순서 해석을 아예 안 한다 |
| `Debug.Log("덱 순서...")` | 수락 기준 "섞인 순서가 매번 다름"을 Console로 확인하려고. 실제 게임에서는 숨겨야 하므로 나중에 지운다 |
| `FillShuffled()` 앞 두 `for` | 하락 2장, 상승 3장을 차례로 넣는다(아직 안 섞임: D D U U U) |
| Fisher-Yates `for (int i = cards.Count - 1; i > 0; i--)` | 맨 뒤 칸부터 앞으로 오면서, `0~i` 중 아무 칸과 맞바꾼다. 모든 순서가 똑같은 확률로 나온다(ADR 0002) |
| `Random.Range(0, i + 1)` | 정수 버전은 **끝 숫자를 포함하지 않는다**. `i`까지 뽑으려면 `i + 1` |
| `temp` 3줄 | 두 칸 맞바꾸기. 한쪽을 잠깐 `temp`에 빼 둬야 덮어써서 사라지지 않는다 |
| `TryFillFixedOrder()`의 `ToUpper()` | `d u u`처럼 소문자로 적어도 받아 준다 |
| `else if (c != ' ')` | 띄어쓰기는 무시, 그 밖의 글자는 잘못 적은 것 → 경고 후 실패 |
| 개수 비교 `downs != downCount` | `D D D U U`처럼 구성이 다르면 규칙이 깨지므로 거부(수락 기준) |
| `Recount()` | 남은 개수를 리스트에서 다시 센다. 02에서 한 장 뽑을 때마다 부르면 표시가 항상 실제 덱과 같다 |

### Participant.cs
| 줄 | 왜 |
|---|---|
| `public string Name { get; }` | 생성할 때 한 번 정하면 끝(set 없음) |
| `Balance { get; private set; }` | 잔고는 Participant만 바꾼다. 02에서 `TakeDamage`를 여기 추가한다 |
| 생성자 `Participant(string name, int startingBalance)` | `new Participant("광대", 2)`처럼 만들 때 이름·시작 잔고를 반드시 넣게 한다 |

### LeverageCounter.cs
| 줄 | 왜 |
|---|---|
| `Value { get; private set; }` | int의 기본값이 0이라 `new` 하면 자동으로 0 = "라운드 시작 시 0" 규칙. 02에서 올리기(3에서 멈춤)·0으로 돌리기를 추가 |

### GameManager.cs
| 줄 | 왜 |
|---|---|
| `[SerializeField] CandleDeck deck; UIManager ui;` | Inspector에서 드래그로 연결. `Find`보다 빠르고 어떤 물체를 쓰는지 눈에 보인다 |
| `startingBalance = 2` | 1라운드 잔고. Inspector에서 바꿀 수 있다 |
| `Current` | 지금 차례인 참가자. 01에서는 표시만, 02부터 바뀐다 |
| `Deck => deck` | UI가 남은 개수를 읽을 수 있게 덱을 **읽기 전용**으로 내보낸다 |
| `Start()` → `StartRound()` | Play를 누르면 바로 1라운드(스토리 1). 함수로 뺀 이유는 05의 R 재시작이 같은 함수를 다시 부르기 때문 |
| `Current = Player;` | 1라운드 선공은 플레이어(Q6) |
| `deck.Fill();` 다음 `ui.Refresh(this);` | 덱을 채운 **뒤에** 화면을 갱신해야 남은 개수가 0이 아니라 2/3으로 나온다. 순서가 중요 |

### UIManager.cs
| 줄 | 왜 |
|---|---|
| `using TMPro;` | TextMeshPro 글자 타입(`TMP_Text`)을 쓰려고 |
| `TMP_Text statusText` | 글자 1개에 4줄을 다 넣는다. 7항목 중 ②선택·⑥결과·⑦공개/승패는 02·05에서 줄을 추가 |
| `Refresh(GameManager game)` | 값을 **읽기만** 한다. 규칙 판정은 GameManager에만(Spec) |
| `$"...{game.Current.Name}..."` | 문자열 보간: `{}` 안의 값을 글자로 끼워 넣는다 |

## 3. Unity Editor에서 할 순서

> 메뉴 이름은 Unity 6 기준이다. 6.6에서 다르게 보이면 알려줘.

1. **폴더:** Project 창 `Assets` 우클릭 → `Create > Folder` → `_Project`, 그 안에 `Scenes`, `Scripts`, `Fonts`
2. **Scene:** `File > New Scene` → 템플릿 `Basic 2D (URP)` (없으면 기본 2D) → `File > Save As` → `Assets/_Project/Scenes/GameScene`
3. **스크립트 5개:** `Scripts` 폴더 우클릭
   - `CandleDeck`, `GameManager`, `UIManager` → `Create > Scripting > MonoBehaviour Script`
   - `Participant`, `LeverageCounter` → `Create > Scripting > Empty C# Script`
   - **파일 이름 = 클래스 이름**이어야 한다. 직접 타이핑 후 저장 → Console에 빨간 에러가 없는지 확인
4. **GameManager 물체:** Hierarchy 우클릭 → `Create Empty` → 이름 `GameManager` → Inspector `Add Component`로 `GameManager`, `CandleDeck` 두 개 추가
5. **글자:** Hierarchy 우클릭 → `UI > Text - TextMeshPro`
   - "TMP Importer" 창이 뜨면 `Import TMP Essentials` 클릭
   - 생긴 `Text (TMP)` 이름을 `StatusText`로, Rect Transform을 크게(예: Width 900, Height 400), Font Size 32
6. **UIManager:** Hierarchy의 `Canvas` 선택 → `Add Component` → `UIManager` → `Status Text` 칸에 `StatusText` 드래그
7. **연결:** `GameManager` 선택 → `Deck` 칸에 `GameManager`(자기 자신, CandleDeck이 붙어 있음), `Ui` 칸에 `Canvas` 드래그
8. **한글 폰트 (필수):** 기본 TMP 폰트에는 한글이 없어서 글자가 □로 나온다.
   - Galmuri(SIL OFL 1.1)를 공식 배포처에서 직접 내려받아 `Assets/_Project/Fonts/`에 넣기 (권리 기록: `_작업기록.md`·GDD 변경 기록 대상)
   - 폰트 파일 우클릭 → `Create > TextMeshPro > Font Asset` (Atlas Population Mode는 기본값 Dynamic 그대로)
   - `StatusText`의 `Font Asset` 칸에 만든 폰트 에셋 드래그
   - Windows 기본 폰트(맑은 고딕 등)는 빌드에 넣어 배포하면 안 되므로 쓰지 않는다
9. **Ctrl+S** 로 Scene 저장

## 4. 수동 테스트 (Ticket 01 수락 기준)

| # | 준비 | 할 일 | 기대 결과 |
|---|---|---|---|
| T01 | 고정 순서 끔 | Play | 화면: `차례: 플레이어` / `잔고 플레이어 2 / 광대 2` / `레버리지 0` / `남은 캔들 하락 2 / 상승 3` |
| T02 | 고정 순서 끔 | Play → 정지 → Play 3번 | Console의 "덱 순서"가 매번 같지는 않다(하락2·상승3 구성은 유지) |
| T03 | 고정 순서 켬, `D U U D U` | Play | Console: `Down Up Up Down Up`, 경고 없음 |
| T04 | 고정 순서 켬, `D D D U U` | Play | 노란 경고 "덱 구성과 달라 무작위로 섞습니다", 덱은 하락2·상승3 |
| T05 | 고정 순서 켬, `D U X D U` | Play | 노란 경고 "D/U가 아닌 글자 'X'", 무작위로 섞음 |
| T06 | CandleDeck `Down Count` 3, `Up Count` 4 / GameManager `Starting Balance` 3 | Play | `잔고 3 / 3`, `하락 3 / 상승 4` |

T06 뒤에는 값을 2 / 3 / 2로 되돌린다.

## 5. 반영 후

내가 `Assets/_Project/Scripts/`에 반영하면 AI에게 리뷰를 요청한다: 초안과 다른 점, 버그 가능성.
