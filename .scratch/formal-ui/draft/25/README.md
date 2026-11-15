# Ticket 25 초안: 테이블 위 단말기에서 캔들이 그려지고, 레버리지 상자에 불꽃이 쌓인다

> **초안**이다(Assets 밖). **직접 타이핑**(CLAUDE.md 4장). Scene 연결은 AI가 MCP로(Spec 채택).
> 근거: `../../spec.md` (C5·C6, 구조 U6, 제안 7), Grilling Q3·Q10, gdd-changes #39·40. 그림은 이미 들어감(커밋 `dcb1b0f`).
> 구현 전 정상 버전: 커밋 `dcb1b0f`

## 0. 바꿀 것
| 파일 | 할 일 | 줄 수 |
|---|---|---|
| `CandleDeck.cs` | `Remaining`, `JustFilled` 추가 (Fill에서 켜고 Draw에서 끔) | +6 |
| `GameManager.cs` | `ShownCandle`, `RevealSeconds` 추가, 세 곳에서 `ShownCandle` 정하기 | +7 |
| `TerminalView.cs` | **새 파일** — 남은 개수·캔들 그리기·차트 분석 캔들·결과 한 줄 | 74 |
| `LeverageView.cs` | **새 파일** — 불꽃·`2/3`·`다음 피해 N` | 24 |
| `ClownView.cs` | **새 파일** — 평소/생각 중 그림 바꾸기 | 14 |
| `UIManager.cs` | 화면 조각 칸 3개, `Refresh` 맨 앞에서 세 조각 부르기 | +10 |

전문은 같은 폴더(바뀐 파일은 지금 Scripts에 바뀐 줄만 더한 전체 파일).
순서: **CandleDeck → GameManager → 새 파일 3개 → UIManager**(UIManager가 새 클래스 이름을 쓰므로 맨 마지막). 다 저장한 뒤 "반영함" → AI가 리뷰하고 Scene을 연결한다. **연결 전에 Play하면** UIManager의 새 칸이 비어 있어 오류가 난다(정상, 연결하면 사라짐).

## 1. `CandleDeck.cs`
```csharp
    public int Remaining => cards.Count;
    public bool JustFilled { get; private set; }
…  (Fill 안, Recount() 다음)
        JustFilled = true;
…  (Draw 안, Recount() 다음)
        JustFilled = false;
```
| 줄 | 왜 |
|---|---|
| `Remaining => cards.Count` | 단말기 `남은 N`. `=>`는 "읽을 때마다 계산"(값을 따로 저장하지 않음) |
| `JustFilled` | "방금 채웠고 아직 한 장도 안 뽑음". 이때만 하락/상승 개수를 보여 준다(#40). 기억력 재미: 한 장 뽑으면 구성은 사라지고 숫자 하나만 |
| Fill에서 `true` | 라운드 시작·재장전 모두 `Fill`을 지나므로 한 곳이면 된다 |
| Draw에서 `false` | 공개·찌라시 모두 `Draw`를 지난다(GameManager `DrawTop` → `deck.Draw`) |

## 2. `GameManager.cs`
```csharp
    public Candle? ShownCandle { get; private set; }
    public float RevealSeconds => revealSeconds;
…  StartRound():  ShownCandle = null;
…  RevealRoutine() 시작:  ShownCandle = deck.Peek();
…  UseRumor():  ShownCandle = top;
```
| 줄 | 왜 |
|---|---|
| `Candle?` | `?` = "값이 없을 수도 있다"(17 차트 분석의 `PlayerKnownNext`와 같음). 라운드 시작엔 단말기가 비어 있다 |
| `RevealSeconds` | 단말기가 캔들을 공개 시간과 **같은 시간** 동안 그리도록. 숫자를 두 군데 적지 않는다(Inspector `Reveal Seconds` 하나) |
| `StartRound`에서 `null` | 새 라운드는 빈 단말기 |
| 공개 시작에 `deck.Peek()` | 판정은 0.5초 뒤 `DrawTop()`에서 하지만, 단말기는 **공개가 시작될 때부터** 그 캔들을 그려야 한다. `Peek`은 보기만 하고 꺼내지 않으므로(17) 판정·순서는 그대로(U1) |
| 찌라시에서 `top` | 찌라시는 다음 캔들을 모두에게 보여 주고 버린다 → 단말기에 뜬다 |

## 3. `TerminalView.cs` (새 파일)
| 부분 | 왜 |
|---|---|
| 칸 7개 | 글자 3개(남은 개수·결과 한 줄·`?`)는 Canvas TMP, 캔들 2개는 SpriteRenderer(그림), 캔들 그림 2장 |
| `remainingText` 삼항 `? :` | `JustFilled`면 `하락 2 · 상승 3`, 아니면 `남은 5` |
| `game.IsRevealing && !wasRevealing` | "지난번엔 공개 중이 아니었는데 지금은 공개 중" = **공개가 막 시작된 순간**. `Refresh`는 여러 번 불리므로 시작 순간에 한 번만 코루틴을 시작하려고 지난 값(`wasRevealing`)을 기억한다 |
| `StopCoroutine` 먼저 | 이전 그리기가 남아 있으면 겹치지 않게 |
| `else if (!game.IsRevealing)` | 공개가 끝나면 그리기를 멈추고 다 그린 모양(높이 1) |
| `knownCandle` / `questionMark.enabled` | 차트 분석으로 알면 `?` 대신 그 캔들(플레이어 것만 — 광대 것은 `PlayerKnownNext`에 없음, C5) |
| `resultText` | 공개 중엔 비우고 끝나면 `LastResult`(제안 7: 광대 행동을 놓치지 않게, 기록은 남기지 않음 #29) |
| `DrawCandle` 코루틴 | 매 프레임 `t`를 늘려 보일 줄 수 = 18 × 진행 비율. `CeilToInt`로 **정수 줄**로 끊는다 → 도트가 반 줄로 흐려지지 않음. `Clamp(…, 1, 18)`: 처음부터 1줄은 보이게 |
| `ShowCandle`의 `localScale` y | 그림 기준점이 **왼쪽 위**라 y 크기를 줄이면 위에서 아래로 자란다(차트가 그려지는 느낌) |
| `target.enabled = candle.HasValue` | 캔들이 없으면(라운드 시작) 숨김 |

## 4. `LeverageView.cs` (새 파일)
| 줄 | 왜 |
|---|---|
| `i < value ? flameOn : flameOff` | 레버리지 2면 0·1번 켜짐, 2번 꺼짐 |
| `LeverageCounter.Max` | 최대 3을 숫자로 적지 않고 원래 정한 곳에서 읽음 |
| `1 + value` | 하락 피해 = 1 + 레버리지(GDD 04장). 판정 코드와 같은 식을 **보여 주기만** |

## 5. `ClownView.cs` (새 파일)
- `IsClownThinking`이면 생각 중 그림(눈동자 위로), 아니면 평소 그림(Q10 "광대 2장면").

## 6. `UIManager.cs`
```csharp
    [Header("플레이 화면 조각 (Ticket 25)")]
    [SerializeField] TerminalView terminalView;
    [SerializeField] LeverageView leverageView;
    [SerializeField] ClownView clownView;
…  Refresh() 맨 앞:
        terminalView.Refresh(game);
        leverageView.Refresh(game);
        clownView.Refresh(game);
```
| 왜 |
|---|
| GameManager는 지금처럼 `ui.Refresh(this)`만 부른다(9곳 그대로, U6). UIManager가 "화면 담당"으로 조각들에게 같은 상태를 나눠 준다 → GameManager를 고칠 곳이 늘지 않는다 |
| 개발용 글자는 그대로 둔다(F1 숨기기는 Ticket 26) |

## 7. Scene (AI가 MCP로, 반영 뒤)
1. 저장 안 된 변경 확인
2. `PlayArt` 아래: `ShownCandle`(단말기 안 (122, 108)), `KnownCandle`(? 칸 안 (189, 113)), `Flame1~3`((218/226/234, 118)), 정렬 순서 22
3. `Clown` 오브젝트에 `ClownView`(평소·생각 중 그림), `Terminal`에 `TerminalView`, `LeverageBox`에 `LeverageView`
4. 새 Canvas `HUD`(Screen Space - Camera, Main Camera, Scale With Screen Size 320×180) + 글자: `GE코인`(고정), 남은 개수, `?`, 결과 한 줄, `레버리지`(고정), `2/3`, `다음 피해 N`(red3) — Galmuri11, 크기 11, 팔레트 색
5. UIManager 칸 3개 연결 → 저장

## 8. 테스트 (Ticket 25)
| # | 방법 | 기대 |
|---|---|---|
| T147 | 라운드 시작(고정 덱 `D U U`) | 단말기 빈 캔들, `하락 1 · 상승 2`, `?` 보임 (C5) |
| T148 | 보유 → 공개 중 0.1·0.3초 캔들 높이, 0.5초 뒤 | 높이가 줄 단위로 늘고 결과 뒤 18줄 하락 캔들, `남은 2`, 공개 시간 그대로 (C5, U2) |
| T149 | 상승 보유 2번 | 불꽃 2개 켜짐, `2/3`, `다음 피해 3` / 하락 뒤 0개·`다음 피해 1` (C6) |
| T150 | 차트 분석(플레이어) / 광대 차트 분석 | 플레이어: ? 칸에 캔들, `?` 숨김 / 광대: 그대로 `?` (C5) |
| T151 | 찌라시 | 버린 캔들이 단말기에 뜸 |
| T152 | 광대 차례 | 생각 중 그림 → 행동 뒤 평소, 결과 한 줄에 `광대: …` (Q10, 제안 7) |
| T153 | 덱 소진 → 재장전 | `하락 n · 상승 m` 다시 보임 → 다음 공개 뒤 `남은` |
| T154 | 퇴행 `clown-ai` T65~T68, 소리 T142~T143 | 판정·차례·소리 같음, Console 오류 0 (C17) |
| T155 | 캡처 1280×720 | 스케치 화면 2와 나란히 비교 (C2) |

## 9. 반영 후
"반영함" → AI 리뷰(초안과 차이, 컴파일 시각) → Scene 연결 → T147~T155 → gdd-changes(결과 한 줄 유지 구체화) → Ticket 25 done.
