# Ticket 07 초안: 광대를 파산시키면 Space로 다음 라운드에 가고, 3라운드를 깨면 최종 승리를 본다

> **초안**이다(Assets 밖). **직접 타이핑**해서 반영한다(CLAUDE.md 4장). 반영 후 AI는 리뷰만 한다.
> 근거: `../../spec.md`(C3·C4·C5·C9, U2~U4), `../../analysis.md` 3절(공유 상태 `IsRoundOver`)
> 바뀌는 파일은 **2개**: `GameManager.cs`, `UIManager.cs`. Scene·Inspector 작업 없음.

## 1. 무엇을 지우고 무엇을 넣나

### GameManager.cs — 3곳

**① `RoundCount` 줄 아래 ~ `Opponent` 위: 판정 칸**

지울 줄:
```csharp
    // 둘 중 하나라도 파산하면 라운드 끝
    public bool IsRoundOver => Player.IsBankrupt || Clown.IsBankrupt;
    // 광대가 파산했으면 플레이어 승리
    public bool PlayerWon => Clown.IsBankrupt;
```
넣을 줄:
```csharp
    public bool IsLastRound => RoundNumber == RoundCount;

    // 둘 중 하나라도 파산하면 라운드 끝 (클리어·최종 승리·패배 모두)
    public bool IsRoundOver => Player.IsBankrupt || Clown.IsBankrupt;
    // 광대 파산 + 마지막 라운드가 아님 → Space로 다음 라운드
    public bool IsRoundCleared => Clown.IsBankrupt && !IsLastRound;
    // 광대 파산 + 마지막 라운드 → 최종 승리
    public bool IsFinalVictory => Clown.IsBankrupt && IsLastRound;
```

**② `Execute()` 전체를 바꿈**

지울 줄:
```csharp
    // Space. 공개 중이거나 라운드가 끝났으면 무시한다.
    public void Execute()
    {
        if (IsRevealing || IsRoundOver) return;
```
넣을 줄 (그 아래 `// 안전장치…`부터 끝까지는 그대로):
```csharp
    // Space. 공개 중이면 무시. 라운드 클리어 화면이면 다음 라운드. 최종 승리·패배면 무시.
    public void Execute()
    {
        if (IsRevealing) return;

        if (IsRoundCleared)
        {
            NextRound();
            return;
        }
        if (IsRoundOver) return;
```

**③ `Execute()`와 `Restart()` 사이에 새 함수**
```csharp
    // 다음 줄로 라운드 번호를 올리고 새로 시작한다.
    void NextRound()
    {
        RoundNumber++;
        StartRound();
    }
```

나머지(`StartRound`, `Select`, `Restart`, `RevealRoutine`, `Resolve*`, `Reload`, `PassTurn`)는 **그대로**.

### UIManager.cs — 1곳

지울 줄:
```csharp
        if (game.IsRoundOver)
        {
            message += game.PlayerWon ? "\n승리! - R로 재시작" : "\n패배... - R로 재시작";
        }
```
넣을 줄:
```csharp
        if (game.IsRoundCleared)
        {
            message += $"\n라운드 {game.RoundNumber} 클리어! - Space로 다음 라운드";
        }
        else if (game.IsFinalVictory)
        {
            message += "\n최종 승리! - R로 처음부터";
        }
        else if (game.IsRoundOver)
        {
            message += "\n패배... - R로 재시작";
        }
```
(패배 문구를 "R로 처음부터"로 바꾸는 건 08에서 한다.)

> 전문은 같은 폴더의 `GameManager.cs`, `UIManager.cs`.
> **순서:** GameManager를 먼저 저장하면 UIManager의 `PlayerWon` 때문에 컴파일 에러가 난다. 두 파일을 다 고친 뒤 저장하거나, UIManager를 먼저 고친 다음 GameManager의 `PlayerWon`을 지운다.

## 2. 바뀐 줄마다 왜

| 줄 | 왜 |
|---|---|
| `IsLastRound => RoundNumber == RoundCount` | 3라운드인지. 숫자 3을 박지 않고 라운드 표 길이와 비교 |
| `IsRoundOver` (그대로) | "라운드가 끝났다"는 공통 기준. `Select`·`Restart`·재장전 조건이 계속 이걸 쓴다(analysis 3절: 공유 상태 5곳). 이걸 바꾸지 않아서 U2(재장전 안 함)·U4(A/D 무시)가 그대로 유지됨 |
| `IsRoundCleared => Clown.IsBankrupt && !IsLastRound` | 광대 파산 **그리고**(`&&`) 마지막 라운드가 **아님**(`!`) |
| `IsFinalVictory => Clown.IsBankrupt && IsLastRound` | 광대 파산 그리고 마지막 라운드 |
| `PlayerWon` 삭제 | 클리어·최종 승리로 나뉘어 더 이상 안 씀. 안 쓰는 코드는 남기지 않음 |
| `Execute`: `if (IsRevealing) return;`을 **맨 앞에 따로** | 공개 중 Space는 무엇이든 무시(U4, analysis 주의점 4). `StartRound`는 돌고 있는 코루틴을 멈추지 않으므로, 공개 중에 다음 라운드로 넘어가면 안 된다 |
| `if (IsRoundCleared) { NextRound(); return; }` | 클리어 화면의 Space만 "다음 라운드"(Q2). `return`으로 아래 공개 코드는 실행 안 함 |
| `if (IsRoundOver) return;` | 남은 경우(최종 승리·패배)는 Space 무시(Q8) |
| `NextRound()`: `RoundNumber++;` → `StartRound();` | 라운드 번호는 **`StartRound` 밖에서** 올린다(analysis 주의점 2). `StartRound`가 다음 줄 값으로 잔고·레버리지·덱·차례를 전부 새로(Q11) |
| UI `if / else if / else if` | 세 경우는 동시에 일어나지 않는다. 위에서부터 하나만 붙는다 |
| `$"\n라운드 {game.RoundNumber} 클리어! …"` | 몇 라운드를 깼는지 표시(Q2) |

## 3. Unity Editor에서 할 순서
1. 구현 전 정상 버전: 커밋 `549278e` (06 완료 상태)
2. 두 파일 수정 → 저장 → Console 빨간 에러 0
3. Ctrl+S → 테스트 (Inspector에서 라운드 표 각 줄의 `Fixed Order` + `CandleDeck` → `Use Fixed Order`)

## 4. 수동 테스트 (Ticket 07)

> 고정 순서는 라운드 표 줄마다 넣는다. `Element 0` = 1라운드, `Element 1` = 2라운드, `Element 2` = 3라운드.
> 매도는 매번 **D → Space** (차례가 바뀌면 선택이 보유로 돌아가므로).

| # | 고정 순서 | 입력 | 기대 |
|---|---|---|---|
| T44 | E0 `D U U D U` | D·Space, D·Space, D·Space, Space | 4번째(광대 보유+하락)에서 광대 0 → `라운드 1 클리어! - Space로 다음 라운드` |
| T45 | 이어서 | A, D | 변화 없음 (U4) |
| T46 | 이어서 | Space | `라운드 2 / 3`, 잔고 3/3, 남은 3/4, 레버리지 0, 플레이어 차례, `[보유]`, 결과 줄 빈칸 (C3) |
| T47 | E1 `U U U U D D D` | (D→Space)×7 | 7번째 뒤 잔고 2/1, `덱 재장전 (하락3 / 상승4)` (C4, U3) |
| T48 | 이어서 | (D→Space)×6 | 6번째(플레이어 매도+하락)에서 광대 0 → `라운드 2 클리어!` → Space → `라운드 3 / 3`, 4/4, 남은 4/4 |
| T49 | E2 `U U U D D D D U` | Space×3 → D·Space | 레버리지 1→2→3 → `매도 → 하락! 광대 -4` → `최종 승리! - R로 처음부터` (C5) |
| T50 | 이어서 | Space | 변화 없음 (Q8) |
| T51 | E0 `U U D U D` (새 Play) | (D→Space)×5 | 5번째(마지막 캔들)에서 광대 0 → 재장전 줄 **없이** `라운드 1 클리어!` (C9, U2) |
| T52 | 아무 판 | Space 직후 "공개 중..." 동안 Space | 캔들 1장만 줄어듦 (U4) |

끝나면 고정 순서 3칸 비우기 → `Use Fixed Order` 끄기 → Ctrl+S.

## 5. 반영 후
"반영했어"라고 알려주면 AI가 리뷰(초안과 다른 점, 버그 가능성)하고, MCP로 T44~T52를 실행한다.
