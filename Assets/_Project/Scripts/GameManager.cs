using System.Collections;
using System.Collections.Generic;   // item-phone 64: IReadOnlyList
using UnityEngine;
using UnityEngine.SceneManagement;

// 차례인 사람이 고를 수 있는 행동
public enum PlayerAction { Buy, Sell }   // monitor-trade 42: 보유(Hold) → 매수(Buy). 0번 값 그대로라 저장된 값도 같은 뜻

// 복권 결과 4가지 (각 25%)
public enum LotteryPrize { Heal, Leverage, Item, Nothing }

public class GameManager : MonoBehaviour
{
    [SerializeField] CandleDeck deck;
    [SerializeField] UIManager ui;
    [SerializeField] AudioManager audioManager;

    [Header("라운드 표 (잔고 / 최소 장수 / 최대 장수 / 고정 순서)")]
    [SerializeField] RoundConfig[] rounds =
    {
        new RoundConfig { startingBalance = 10, minCards = 4, maxCards = 6 },
        new RoundConfig { startingBalance = 10, minCards = 6, maxCards = 8 },
        new RoundConfig { startingBalance = 10, minCards = 7, maxCards = 9 },
    };

    [Header("연출")]
    [SerializeField] float revealSeconds = 1f;
    [SerializeField] float clownThinkSeconds = 0.5f;
    // 공개가 시작되고 이만큼 뒤에 초침 (앞부분은 GDD "공개 직전 잠깐 무음")
    [SerializeField] float tickDelaySeconds = 0.2f;

    [Header("아이템")]
    [SerializeField] ItemData[] itemDatas;
    [SerializeField] int itemsPerGrant = 2;
    // 광대 규칙 ③: 잔고가 이 값 이하이면 복권을 쓴다 (GDD는 1, gdd-changes #32)
    [SerializeField] int clownLotteryThreshold = 3;

    [Header("테스트용 아이템 (C 차트 분석 / J 찌라시 / S 거래 정지 / L 복권)")]
    [SerializeField] bool useFixedItems = false;
    [SerializeField] string fixedItems = "";
    int fixedItemIndex;

    [Header("테스트용 복권")]
    [SerializeField] bool useFixedLottery = false;
    [SerializeField] LotteryPrize fixedLottery = LotteryPrize.Nothing;

    public Participant Player { get; private set; }
    public Participant Clown { get; private set; }
    public Participant Current { get; private set; }
    public LeverageCounter Leverage { get; private set; }
    public CandleDeck Deck => deck;

    public PlayerAction Selected { get; private set; }
    public bool IsRevealing { get; private set; }
    public bool IsClownThinking { get; private set; }
    public string LastResult { get; private set; }
    // 단말기에 떠 있는 캔들(공개 중·공개 뒤, 찌라시로 본 것). 라운드 시작엔 없음(null). // ← 25 추가
    public Candle? ShownCandle { get; private set; }                                   // ← 25 추가
    // 단말기가 캔들을 그리는 시간 = 공개 시간                                           // ← 25 추가
    public float RevealSeconds => revealSeconds;                                       // ← 25 추가

    // 1~4로 확인 중인 내 아이템 슬롯 번호. 확인 중이 아니면 -1.
    public int ConfirmingSlot { get; private set; } = -1;
    public bool IsConfirming => ConfirmingSlot >= 0;

    // 플레이어가 차트 분석으로 알게 된 다음 캔들. 모르면 null(값 없음).
    public Candle? PlayerKnownNext { get; private set; }
    // 광대가 차트 분석으로 알게 된 다음 캔들. 플레이어에게는 보이지 않는다.
    Candle? clownKnownNext;

    // 지금 라운드 (1부터). StartRound 밖에서만 바꾼다.
    public int RoundNumber { get; private set; }
    public int RoundCount => rounds.Length;
    public bool IsLastRound => RoundNumber == RoundCount;

    // 둘 중 하나라도 파산하면 라운드 끝 (클리어·최종 승리·패배 모두)
    public bool IsRoundOver => Player.IsBankrupt || Clown.IsBankrupt;
    // 광대 파산 + 마지막 라운드가 아님 → Space로 다음 라운드
    public bool IsRoundCleared => Clown.IsBankrupt && !IsLastRound;
    // 광대 파산 + 마지막 라운드 → 최종 승리
    public bool IsFinalVictory => Clown.IsBankrupt && IsLastRound;

    // 라운드가 진행 중이고 광대 차례 → 광대가 스스로 행동, 플레이어 입력은 무시
    public bool IsClownTurn => Current == Clown && !IsRoundOver;

    // 지금 플레이어가 매수·매도를 고를 수 있는가 (버튼을 밝게/회색으로 보일 때 쓴다)
    // item-phone 63: 폰이 열려 있어도 매수·매도는 못 고른다(A/D 무시)
    public bool CanPlayerAct => !IsRevealing && !IsRoundOver && !IsClownTurn && !IsConfirming && !IsPaused && !IsPresenting && !IsPhoneOpen;

    // monitor-trade 43: 모니터 확대 화면이 열려 있는가(매수·매도를 고르는 중)
    public bool IsMonitorOpen { get; private set; }
    // 지금 모니터를 열 수 있는가 = 내가 행동할 수 있고 아직 안 열려 있을 때(광대 차례·공개 중·연출 중·멈춤·폰 열림엔 못 엶)
    public bool CanOpenMonitor => CanPlayerAct && !IsMonitorOpen;

    // item-phone 63: 내 폰(아이템)이 열려 있는가. 모니터와 동시에 열리지 않는다.
    public bool IsPhoneOpen { get; private set; }
    // 지금 폰을 열 수 있는가 = 모니터와 같은 조건(내 차례·공개 중 아님, Q12) + 둘 다 닫혀 있을 때
    public bool CanOpenPhone => CanPlayerAct && !IsMonitorOpen;
    // 폰 목록에서 Space를 누르면 열릴 칸(금색 틀). 상세에서 Esc로 돌아오면 그 칸에 남는다.
    public int PhoneCursor { get; private set; }
    // item-phone 66: 열린 폰이 광대 폰인가(보기 전용 — 상세는 볼 수 있지만 쓸 수 없다)
    public bool IsViewingClownPhone { get; private set; }
    // 지금 열린 폰의 아이템 목록(화면이 읽는 곳 하나로 모음): 광대 폰이면 광대 것, 아니면 내 것
    public IReadOnlyList<ItemType> PhoneItems => IsViewingClownPhone ? Clown.Items : Player.Items;

    // 일시정지 중인가 (Ticket 29). 멈춘 동안은 Time.timeScale = 0 → 공개·광대 생각의 WaitForSeconds도 멈춘다.
    public bool IsPaused { get; private set; }

    // ── 덱 공개 연출 (ui-revamp 31). 판정은 하지 않고, 연출 동안 입력과 광대 차례 시작만 미룬다. ──
    public bool IsPresenting { get; private set; }
    // 연출 중 Space를 누르면 켜진다 → 화면(DeckRevealView)이 보고 연출을 바로 끝낸다
    public bool SkipRequested { get; private set; }
    // 마지막으로 덱을 채운 게 라운드 시작이었나(true) 재장전이었나(false) → 연출 길이(1.5초 / 1.0초)
    public bool LastFillWasRoundStart { get; private set; }

    public void BeginPresentation()
    {
        IsPresenting = true;
        SkipRequested = false;
        IsMonitorOpen = false;      // monitor-trade 43: 덱 공개 연출이 시작되면 열린 화면을 닫는다(연출을 가리지 않게)
        IsPhoneOpen = false;        // item-phone 63: 폰도 같이 닫는다
        ui.Refresh(this);
    }

    public void EndPresentation()
    {
        IsPresenting = false;
        SkipRequested = false;
        ui.Refresh(this);
    }

    // 지금 차례가 아닌 쪽
    Participant Opponent => (Current == Player) ? Clown : Player;

    // 한 판이 끝났는가: 내가 파산(어느 라운드든) 또는 마지막 라운드에서 광대 파산 → 결과 화면
    public bool IsGameOver => Player.IsBankrupt || IsFinalVictory;

    // ── 한 판 기록 (Ticket 28 결과 화면). 보여 주기만 하고 판정에는 쓰지 않는다. ──
    public int MaxLeverage { get; private set; }
    public int ItemUses { get; private set; }          // 플레이어가 실제로 쓴 아이템 수

    // ── item-phone 64: 광대 아이템 사용 신호(광대 폰이 그 칸을 반짝이는 데 씀). 판정에는 쓰지 않는다. ──
    public int ClownItemUseCount { get; private set; }  // 광대가 아이템을 쓸 때마다 +1 → 화면은 "바뀌었나"만 본다
    public int ClownLastUsedSlot { get; private set; }  // 방금 쓴 아이템이 쓰기 직전 몇 번째 칸이었나(0~3)
    public string LastDamage { get; private set; }     // 마지막 피해 한 줄 (파산 = 원인, 생존 = 결정타)
    float gameStartTime;
    float gameEndTime = -1f;                           // 끝나기 전에는 -1
    // 한 판에 걸린 시간(초). 끝났으면 끝난 순간에서 멈춘다.
    public float PlaySeconds => (gameEndTime >= 0f ? gameEndTime : Time.time) - gameStartTime;

    void Start()
    {
        // 타이틀에서 왔든, 일시정지 중 타이틀로 갔다 왔든 시간은 정상 속도로 시작
        Time.timeScale = 1f;
        IsPaused = false;
        StartGame();
    }

    // 1라운드부터 새 판. 처음 시작과 다시하기가 같은 길을 지난다.
    void StartGame()
    {
        fixedItemIndex = 0;
        RoundNumber = 1;
        MaxLeverage = 0;
        ItemUses = 0;
        LastDamage = "";
        gameStartTime = Time.time;
        gameEndTime = -1f;
        StartRound();
    }

    // RoundNumber 줄의 값으로 라운드를 새로 시작한다. 라운드 번호는 여기서 바꾸지 않는다.
    void StartRound()
    {
        RoundConfig config = rounds[RoundNumber - 1];

        Player = new Participant("나", config.startingBalance);
        Clown = new Participant("광대", config.startingBalance);
        Leverage = new LeverageCounter();
        Current = Player;
        Selected = PlayerAction.Buy;
        IsRevealing = false;
        IsClownThinking = false;
        ConfirmingSlot = -1;
        IsMonitorOpen = false;      // monitor-trade 43
        IsPhoneOpen = false;        // item-phone 63
        PhoneCursor = 0;
        PlayerKnownNext = null;
        clownKnownNext = null;
        LastResult = "";
        ShownCandle = null;                                                            // ← 25 추가
        deck.SetComposition(config.minCards, config.maxCards, config.fixedOrder);
        LastFillWasRoundStart = true;
        deck.Fill();
        GiveItems();
        ui.Refresh(this);
    }

    // A/D·←/→. 공개 중, 라운드 끝, 광대 차례, 아이템 확인 중이면 무시한다.
    public void Select(PlayerAction action)
    {
        if (IsPaused || IsPresenting || IsRevealing || IsRoundOver || IsClownTurn || IsConfirming || IsPhoneOpen) return;
        if (Selected != action) audioManager.PlayClick();   // 실제로 바뀔 때만
        Selected = action;
        ui.Refresh(this);
    }

    // Space. 공개 중이면 무시. 아이템 확인 중이면 그 아이템 사용. 라운드 클리어 화면이면 다음 라운드.
    // 최종 승리·패배면 무시. 광대 차례면 무시.
    public void Execute()
    {
        if (IsPaused || IsRevealing) return;

        // 연출 중 Space = 건너뛰기 (Q3)
        if (IsPresenting)
        {
            SkipRequested = true;
            return;
        }

        if (IsConfirming)
        {
            // item-phone 66: 광대 폰 상세는 보기만 — Space를 눌러도 아무 일 없음
            if (!IsViewingClownPhone) UseConfirmedItem();
            return;
        }

        // item-phone 63: 폰 목록에서 Space = 금색 틀 칸의 상세 열기(빈칸이면 아무 일 없음)
        if (IsPhoneOpen)
        {
            ViewSlot(PhoneCursor);
            return;
        }

        if (IsRoundCleared)
        {
            NextRound();
            return;
        }
        if (IsRoundOver) return;

        // 클리어 확인보다 뒤에 둔다: 클리어 화면에서도 Current가 광대일 수 있다.
        if (IsClownTurn) return;

        // 안전장치: 파산하지 않았다면 재장전되므로 여기까지 올 일이 없다.
        if (deck.IsEmpty) return;

        // monitor-trade 43: 닫혀 있으면 먼저 모니터를 연다(들여다보기). 열려 있으면 고른 행동을 실행하며 닫는다 → 모니터에서 공개.
        if (!IsMonitorOpen)
        {
            OpenMonitor();
            return;
        }
        IsMonitorOpen = false;

        StartCoroutine(RevealRoutine(Selected));
    }

    // 1~4. 내 차례(공개 중·광대 차례·라운드 끝이 아닐 때)에 그 슬롯에 아이템이 있으면 확인을 연다.
    public void SelectItem(int slot)
    {
        if (IsPaused || IsPresenting || IsRevealing || IsRoundOver || IsClownTurn) return;
        if (slot < 0 || slot >= Player.Items.Count) return;
        ConfirmingSlot = slot;
        PhoneCursor = slot;
        // item-phone 63: 1~4 키·폰 칸으로 고르면 폰이 그 아이템 상세로 바로 열린다(모니터가 열려 있었으면 닫음 — 둘은 동시에 안 열림)
        IsPhoneOpen = true;
        IsViewingClownPhone = false;   // item-phone 66: 1~4는 늘 내 폰(광대 폰을 보고 있었어도 내 폰으로 바뀜)
        IsMonitorOpen = false;
        audioManager.PlayClick();
        ui.Refresh(this);
    }

    // item-phone 66: 폰 목록의 칸 클릭·Space — 내 폰이면 1~4와 같고, 광대 폰이면 그 칸 상세를 "보기만"
    public void ViewSlot(int slot)
    {
        if (!IsViewingClownPhone)
        {
            SelectItem(slot);
            return;
        }
        if (IsPaused || IsPresenting || IsRevealing || IsRoundOver || IsClownTurn) return;
        if (slot < 0 || slot >= Clown.Items.Count) return;
        ConfirmingSlot = slot;
        PhoneCursor = slot;
        audioManager.PlayClick();
        ui.Refresh(this);
    }

    // Esc. 확인을 닫는다(폰 목록으로 돌아감). 아이템은 그대로 남는다.
    public void CancelItem()
    {
        if (IsPaused || !IsConfirming) return;
        ConfirmingSlot = -1;
        audioManager.PlayClick();
        ui.Refresh(this);
    }

    // item-phone 63: 테이블 위 접힌 폰 클릭 → 확대된 폰의 아이템 목록. 판정은 바꾸지 않는다.
    public void OpenPhone()
    {
        if (!CanOpenPhone) return;
        IsPhoneOpen = true;
        IsViewingClownPhone = false;
        PhoneCursor = 0;
        audioManager.PlayClick();
        ui.Refresh(this);
    }

    // item-phone 66: 테이블 위 접힌 광대 폰 클릭 → 같은 확대 화면에 광대 아이템 목록(보기 전용). 열 수 있는 때는 내 폰과 같다.
    public void OpenClownPhone()
    {
        if (!CanOpenPhone) return;
        IsPhoneOpen = true;
        IsViewingClownPhone = true;
        PhoneCursor = 0;
        audioManager.PlayClick();
        ui.Refresh(this);
    }

    // item-phone 63: 폰 목록에서 Esc·빨간 키·바깥 클릭 → 폰 닫기(상세 화면이면 상세부터 닫는다 — PhoneBack)
    public void ClosePhone()
    {
        if (!IsPhoneOpen || IsConfirming) return;
        IsPhoneOpen = false;
        audioManager.PlayClick();
        ui.Refresh(this);
    }

    // item-phone 63: 한 단계 뒤로 — 상세면 목록으로, 목록이면 폰 닫기
    public void PhoneBack()
    {
        if (IsConfirming) CancelItem();
        else ClosePhone();
    }

    // monitor-trade 43: 모니터 클릭·Space(닫혀 있을 때) → 확대 화면 열기. 판정은 바꾸지 않는다.
    public void OpenMonitor()
    {
        if (!CanOpenMonitor) return;
        IsMonitorOpen = true;
        audioManager.PlayClick();
        ui.Refresh(this);
    }

    // Esc·우클릭·화면 밖 클릭 → 닫기(아이템 확인 중이면 그 확인부터 닫는다 — 입력 쪽에서 순서를 정함)
    public void CloseMonitor()
    {
        if (!IsMonitorOpen || IsConfirming) return;
        IsMonitorOpen = false;
        audioManager.PlayClick();
        ui.Refresh(this);
    }

    // Esc(아이템 확인 중이 아닐 때): 멈추기 ↔ 계속. 한 판이 끝난 결과 화면에서는 멈추지 않는다.
    public void TogglePause()
    {
        if (IsGameOver || IsConfirming || IsMonitorOpen || IsPhoneOpen) return;   // monitor-trade 43·item-phone 63: 열린 화면은 Esc로 먼저 닫음
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
        ui.Refresh(this);
    }

    // 일시정지 패널의 계속 버튼
    public void Resume()
    {
        if (!IsPaused) return;
        TogglePause();
    }

    // 일시정지·결과 화면의 타이틀 버튼. 멈춘 시간을 먼저 되돌려야 다음 Scene이 정상 속도로 움직인다.
    public void GoToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TitleScene");
    }

    // 다음 줄로 라운드 번호를 올리고 새로 시작한다.
    void NextRound()
    {
        RoundNumber++;
        StartRound();
    }

    // 결과 화면의 다시하기 버튼. 한 판이 끝났을 때만 받는다. 항상 1라운드부터, 기록도 처음부터.
    // (R 키는 Ticket 28에서 없앰, gdd-changes #19)
    public void Restart()
    {
        if (IsRevealing || !IsGameOver) return;
        StartGame();
    }

    // 확인 중인 내 아이템을 쓴다. 아이템은 행동이 아니라서 차례는 넘어가지 않는다.
    void UseConfirmedItem()
    {
        ItemType item = Player.Items[ConfirmingSlot];
        bool used = true;   // 실제로 썼는가(기록용). 쓸 수 없던 경우만 false

        switch (item)
        {
            case ItemType.Rumor:
                Player.RemoveItemAt(ConfirmingSlot);
                UseRumor(Player);
                break;
            case ItemType.ChartAnalysis:
                Player.RemoveItemAt(ConfirmingSlot);
                UseChartAnalysis(Player);
                break;
            case ItemType.TradingHalt:
                // 상대가 이미 정지 상태면 쓸 수 없다. 아이템은 남는다.
                if (Clown.IsHalted)
                {
                    LastResult = $"{ItemName(item)}: 사용 불가 (상대가 이미 정지 상태)";
                    used = false;
                    break;
                }
                Player.RemoveItemAt(ConfirmingSlot);
                UseTradingHalt(Player);
                break;
            case ItemType.Lottery:
                Player.RemoveItemAt(ConfirmingSlot);
                UseLottery(Player);
                break;
            default:
                // 17~19에서 하나씩 채운다. 지금은 아이템을 남겨 둔다.
                LastResult = $"{ItemName(item)}: 아직 쓸 수 없음";
                used = false;
                break;
        }

        if (used) ItemUses++;
        ConfirmingSlot = -1;
        // item-phone 63: 썼으면 폰을 닫아 결과(모니터 글·차트 칸 캔들)가 보이게. 못 썼으면(거래 정지 불가) 목록에 남는다.
        if (used) IsPhoneOpen = false;
        ui.Refresh(this);
    }

    // 찌라시: 다음 캔들을 모두 앞에서 공개하고 버린다. 덱이 비면 바로 재장전.
    void UseRumor(Participant user)
    {
        Candle top = DrawTop();
        ShownCandle = top;                                                             // ← 25 추가
        LastResult = $"{user.Name}: 찌라시 → 다음 캔들은 {CandleName(top)} (버림)";

        if (deck.IsEmpty)
        {
            Reload();
        }
    }

    // 차트 분석: 다음 캔들을 쓴 사람만 안다. 덱은 그대로, 차례도 그대로.
    void UseChartAnalysis(Participant user)
    {
        if (user == Player) PlayerKnownNext = deck.Peek();
        else clownKnownNext = deck.Peek();
        LastResult = $"{user.Name}: 차트 분석 사용";
    }

    // 거래 정지: 상대의 다음 차례 한 번을 건너뛰게 한다. 실제로 건너뛰는 일은 PassTurn()이 한다.
    void UseTradingHalt(Participant user)
    {
        Opponent.Halt();
        LastResult = $"{user.Name}: 거래 정지 사용 → {Opponent.Name}의 다음 턴 건너뜀";
    }

    // 복권: 25%씩 잔고 +1 / 레버리지 +1 / 아이템 +1 / 꽝. 적용할 수 없으면 꽝.
    void UseLottery(Participant user)
    {
        LotteryPrize prize = useFixedLottery ? fixedLottery : (LotteryPrize)Random.Range(0, 4);
        string result;

        switch (prize)
        {
            case LotteryPrize.Heal:
                result = user.Heal(1) ? "잔고 +1" : "꽝 (잔고가 이미 가득)";
                break;
            case LotteryPrize.Leverage:
                if (Leverage.Value < LeverageCounter.Max)
                {
                    Leverage.Increase();
                    RecordLeverage();
                    result = $"레버리지 +1 (현재 {Leverage.Value})";
                }
                else
                {
                    result = "꽝 (레버리지가 이미 최대)";
                }
                break;
            case LotteryPrize.Item:
                ItemType bonus = NextItem();
                result = user.AddItem(bonus) ? $"아이템 +1 ({ItemName(bonus)})" : "꽝 (아이템이 이미 가득)";
                break;
            default:
                result = "꽝";
                break;
        }

        // monitor-trade 42 (#60): 무엇이 당첨됐는지는 화면 글자로 알리지 않는다 — 잔고·레버리지·아이템 변화로만 보인다.
        // 당첨 내용은 테스트·개발용으로만 남긴다(화면에 쓰지 않음).
        LastLotteryDetail = result;
        LastResult = $"{user.Name}: 복권 사용";
    }

    // 마지막 복권의 당첨 내용(화면에 보이지 않음, 테스트용)
    public string LastLotteryDetail { get; private set; } = "";

    // 맨 위 캔들을 꺼낸다. 알고 있던 다음 캔들은 이제 지나갔으므로 잊는다.
    Candle DrawTop()
    {
        PlayerKnownNext = null;
        clownKnownNext = null;
        return deck.Draw();
    }

    // 두 사람에게 itemsPerGrant개씩 준다. 4개를 넘는 것은 버려진다.
    void GiveItems()
    {
        for (int i = 0; i < itemsPerGrant; i++) Player.AddItem(NextItem());
        for (int i = 0; i < itemsPerGrant; i++) Clown.AddItem(NextItem());
    }

    // 다음에 줄 아이템. 고정 지급이 켜져 있으면 문자열에서 순서대로, 다 쓰면 무작위.
    ItemType NextItem()
    {
        if (useFixedItems)
        {
            while (fixedItemIndex < fixedItems.Length)
            {
                char c = char.ToUpper(fixedItems[fixedItemIndex]);
                fixedItemIndex++;
                if (c == 'C') return ItemType.ChartAnalysis;
                if (c == 'J') return ItemType.Rumor;
                if (c == 'S') return ItemType.TradingHalt;
                if (c == 'L') return ItemType.Lottery;
                // 공백 등 다른 글자는 건너뛴다.
            }
        }
        return (ItemType)Random.Range(0, 4);
    }

    // 화면에 쓸 이름. 에셋이 연결되지 않았으면 enum 이름.
    public string ItemName(ItemType item)
    {
        ItemData data = DataOf(item);
        return data != null ? data.displayName : item.ToString();
    }

    public string ItemCategory(ItemType item)
    {
        ItemData data = DataOf(item);
        return data != null ? data.category : "";
    }

    public string ItemDescription(ItemType item)
    {
        ItemData data = DataOf(item);
        return data != null ? data.description : "";
    }

    ItemData DataOf(ItemType item)
    {
        if (itemDatas == null) return null;
        foreach (ItemData data in itemDatas)
        {
            if (data != null && data.type == item) return data;
        }
        return null;
    }

    static string CandleName(Candle candle) => candle == Candle.Down ? "하락" : "상승";

    // "공개 중..." (revealSeconds) → 캔들 공개 → 고른 행동대로 결과 적용
    IEnumerator RevealRoutine(PlayerAction action)
    {
        IsRevealing = true;
        ShownCandle = deck.Peek();   // 공개될 캔들(맨 위). 공개 중 단말기가 이걸 그린다 // ← 25 추가
        audioManager.SetRevealDuck(true);
        ui.Refresh(this);

        // 잠깐 조용히 → 초침 → 남은 시간. 공개 시간 전체(revealSeconds)는 그대로.
        float delay = Mathf.Min(tickDelaySeconds, revealSeconds);
        yield return new WaitForSeconds(delay);
        audioManager.PlayTick();
        yield return new WaitForSeconds(revealSeconds - delay);

        Candle candle = DrawTop();

        // 행동한 사람은 판정 전에 기억한다: 판정 안의 PassTurn()이 Current를 상대로 바꾸기 때문.
        Participant actor = Current;

        // 처리 순서: ① 피해 적용 → ② 잔고 0 판정 → ③ 덱 소진 확인
        if (action == PlayerAction.Buy) ResolveBuy(candle);
        else ResolveSell(candle);

        // 결과 문구 앞에 행동한 사람. 재장전 줄은 이 뒤에 붙는다.
        LastResult = $"{actor.Name}: {LastResult}";

        if (!IsRoundOver && deck.IsEmpty)
        {
            Reload();
        }

        // 한 판이 끝난 순간의 시간을 남긴다(결과 화면 시간이 계속 늘지 않게).
        if (IsGameOver && gameEndTime < 0f) gameEndTime = Time.time;

        IsRevealing = false;
        audioManager.SetRevealDuck(false);
        ui.Refresh(this);

        // 결과 뒤에도 광대 차례면(내가 넘겼거나, 광대가 매수+상승) 광대가 행동한다.
        if (IsClownTurn)
        {
            StartCoroutine(ClownTurnRoutine());
        }
    }

    // "광대 생각 중..." (clownThinkSeconds) → ClownAI에게 묻기
    // → 아이템이면 쓰고 다시 생각(①부터), 매수·매도면 기존 공개 처리
    IEnumerator ClownTurnRoutine()
    {
        // 덱 공개 연출이 끝난 뒤에 생각을 시작한다(연출 중에 광대가 움직이지 않게)
        while (IsPresenting) yield return null;

        IsClownThinking = true;
        ui.Refresh(this);

        yield return new WaitForSeconds(clownThinkSeconds);

        IsClownThinking = false;
        ClownMove move = ClownAI.Decide(deck.RemainingDown, deck.RemainingUp, Clown.Items,
                                        Clown.Balance, Player.IsHalted, clownKnownNext, clownLotteryThreshold);

        if (move.UsesItem)
        {
            UseClownItem(move.Item);
            ui.Refresh(this);
            StartCoroutine(ClownTurnRoutine());
            yield break;
        }

        Selected = move.Action;
        StartCoroutine(RevealRoutine(Selected));
    }

    // item-phone 64: 목록에서 그 아이템이 처음 나오는 칸 번호(없으면 -1). RemoveItem도 처음 나오는 것을 지운다.
    static int IndexOf(IReadOnlyList<ItemType> items, ItemType item)
    {
        for (int i = 0; i < items.Count; i++) if (items[i] == item) return i;
        return -1;
    }

    // 광대가 아이템 하나를 쓴다. 효과 함수는 플레이어와 같은 것을 쓴다.
    void UseClownItem(ItemType item)
    {
        // item-phone 64: 지우기 전에 칸 번호를 기억해 두고 신호를 올린다(지우면 뒤 칸이 앞으로 당겨져서)
        ClownLastUsedSlot = IndexOf(Clown.Items, item);
        ClownItemUseCount++;
        Clown.RemoveItem(item);

        switch (item)
        {
            case ItemType.ChartAnalysis: UseChartAnalysis(Clown); break;
            case ItemType.Rumor: UseRumor(Clown); break;
            case ItemType.TradingHalt: UseTradingHalt(Clown); break;
            case ItemType.Lottery: UseLottery(Clown); break;
        }
    }

    void ResolveBuy(Candle candle)
    {
        if (candle == Candle.Up)
        {
            Leverage.Increase();
            RecordLeverage();
            LastResult = $"매수 → 상승! 레버리지 {Leverage.Value}, 차례 유지";
        }
        else
        {
            int damage = 1 + Leverage.Value;
            Current.TakeDamage(damage);
            RecordDamage($"{Current.Name} 매수", damage);
            audioManager.PlayImpact();
            ui.FlashDamage(Current == Player);   // monitor-trade 44: 맞은 사람이 나인가(흔들림 대상이 다름)
            Leverage.Reset();
            LastResult = $"매수 → 하락! {Current.Name} -{damage}";
            PassTurn();
        }
    }

    // 기록: 지금까지 가장 높았던 레버리지
    void RecordLeverage()
    {
        MaxLeverage = Mathf.Max(MaxLeverage, Leverage.Value);
    }

    // 기록: 마지막 피해. 피해 = 1 + 레버리지이므로 레버리지 = 피해 - 1 (Reset 전 값). 예) "R2, 광대 매도, 하락, 피해 3 (레버리지 2)"
    void RecordDamage(string actorAction, int damage)
    {
        LastDamage = $"R{RoundNumber}, {actorAction}, 하락, 피해 {damage} (레버리지 {damage - 1})";
    }

    // 매도: 하락이면 상대가 맞는다. 어느 쪽이든 차례는 넘어간다.
    void ResolveSell(Candle candle)
    {
        if (candle == Candle.Down)
        {
            int damage = 1 + Leverage.Value;
            Opponent.TakeDamage(damage);
            RecordDamage($"{Current.Name} 매도", damage);
            audioManager.PlayImpact();
            ui.FlashDamage(Opponent == Player);
            Leverage.Reset();
            LastResult = $"매도 → 하락! {Opponent.Name} -{damage}";
        }
        else
        {
            LastResult = $"매도 → 상승! 변화 없음 (레버리지 {Leverage.Value} 그대로)";
        }
        PassTurn();
    }

    // 구성을 새로 뽑아 채우고 두 사람에게 아이템을 준다. 잔고·레버리지·차례는 그대로.
    void Reload()
    {
        LastFillWasRoundStart = false;
        deck.Fill();
        GiveItems();
        audioManager.PlayShuffle();
        LastResult += $"\n덱 재장전 (하락 {deck.RemainingDown} / 상승 {deck.RemainingUp}), 아이템 +{itemsPerGrant}";
    }

    // 상대에게 차례를 넘기고 선택은 매수로 되돌린다.
    // 넘겨받은 사람이 거래 정지 상태면 그 차례를 건너뛰고 다시 넘긴다(모든 차례 넘김이 여기를 지난다).
    void PassTurn()
    {
        Current = Opponent;
        Selected = PlayerAction.Buy;
        IsMonitorOpen = false;      // monitor-trade 43: 차례가 넘어가면 닫힌 상태로
        IsPhoneOpen = false;        // item-phone 63

        if (Current.IsHalted && !IsRoundOver)
        {
            Current.ClearHalt();
            LastResult += $"\n{Current.Name}: 거래 정지로 턴을 건너뜀";
            Current = Opponent;
        }
    }
}