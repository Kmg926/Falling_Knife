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

    [Header("라운드 표 (잔고 최소·최대 / 최소 장수 / 최대 장수 / 고정 순서)")]
    [SerializeField] RoundConfig[] rounds =
    {
        new RoundConfig { minBalance = 5,  maxBalance = 7,  minCards = 4, maxCards = 6 },
        new RoundConfig { minBalance = 5,  maxBalance = 7,  minCards = 6, maxCards = 8 },
        new RoundConfig { minBalance = 10, maxBalance = 10, minCards = 7, maxCards = 9 },   // 93: 마지막 라운드는 10 고정
    };

    [Header("연출")]
    [SerializeField] float revealSeconds = 1f;
    // 광대가 행동(매수·매도·아이템) 하나를 하기 전 생각 시간 = 광대 행동 사이 간격.
    // 0.5 → 1(#22) → 2026-11-23 사용자 "광대 행동이 너무 빨라, 사이에 텀을 2초씩" → 2
    [SerializeField] float clownThinkSeconds = 2f;
    // clown-tell 80: 광대가 말풍선으로 말한 뒤 읽을 시간(Grilling Q4)
    [SerializeField] float clownDeclareSeconds = 0.6f;   // 매수·매도를 말하고 → 공개까지
    [SerializeField] float clownItemSeconds = 0.8f;      // 아이템을 말하고 → 다음 판단까지
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

    [Header("튜토리얼 (tutorial 97, 각본 — Spec 3)")]
    [SerializeField] RoundConfig tutorialRound = new RoundConfig { minBalance = 5, maxBalance = 5, minCards = 8, maxCards = 8, fixedOrder = "UDUDDUUD" };
    [SerializeField] string tutorialPlayerItems = "C";    // 내 아이템(NextItem 글자 규칙): 차트 분석 1개
    [SerializeField] string tutorialClownMoves = "SB";    // 광대 행동 순서: S 매도 · B 매수(다 쓰면 원래 판단)

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
    public int RoundCount => IsTutorial ? 1 : rounds.Length;   // tutorial 97: 튜토리얼은 1라운드
    RoundConfig CurrentConfig => IsTutorial ? tutorialRound : rounds[RoundNumber - 1];
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
    public bool CanPlayerAct => !IsCutscene && !IsRevealing && !IsRoundOver && !IsClownTurn && !IsConfirming && !IsPaused && !IsPresenting && !IsPhoneOpen;

    // monitor-trade 43: 모니터 확대 화면이 열려 있는가(매수·매도를 고르는 중)
    public bool IsMonitorOpen { get; private set; }
    // 지금 모니터를 열 수 있는가 = 내가 행동할 수 있고 아직 안 열려 있을 때(광대 차례·공개 중·연출 중·멈춤·폰 열림엔 못 엶)
    public bool CanOpenMonitor => CanPlayerAct && !IsMonitorOpen && (!IsTutorial || tutorialAllowMonitor);   // tutorial 97: 허용된 단계에서만

    // item-phone 63: 내 폰(아이템)이 열려 있는가. 모니터와 동시에 열리지 않는다.
    public bool IsPhoneOpen { get; private set; }
    // 지금 폰을 열 수 있는가 = 모니터와 같은 조건(내 차례·공개 중 아님, Q12) + 둘 다 닫혀 있을 때
    public bool CanOpenPhone => CanPlayerAct && !IsMonitorOpen && (!IsTutorial || tutorialAllowPhone);   // tutorial 97
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

    // ── 시작 스토리 시네마틱 (intro-story 77). 판정은 하지 않고, 시네마틱 동안 게임 입력과 덱 공개 연출만 미룬다. ──
    public bool IsIntro { get; private set; }

    // IntroStory.Awake가 부름(Start보다 먼저) → StartGame의 ui.Refresh 때 덱 공개 연출이 시작되지 않는다
    public void BeginIntro()
    {
        IsIntro = true;
    }

    // 시네마틱이 끝나면(마지막 줄·Esc) 부름 → 한 판 시간은 여기서부터 재고, 화면을 새로 그려 덱 공개 연출을 시작
    public void EndIntro()
    {
        if (!IsIntro) return;
        IsIntro = false;
        gameStartTime = Time.time;
        ui.Refresh(this);
    }

    // ── tutorial 97: 튜토리얼 판. 판정은 그대로 두고, 값(각본)과 "지금 해도 되는 행동"만 바꾼다. ──
    public bool IsTutorial { get; private set; }
    // 튜토리얼 광대가 말하는 중 = 게임 입력·광대 차례·덱 공개 연출을 멈춤(시작 스토리와 같은 효과)
    public bool IsGuideTalking { get; private set; }
    // 지금 연출(시작 스토리·튜토리얼 대사) 중이라 게임이 기다리는가
    public bool IsCutscene => IsIntro || IsGuideTalking;
    // 공개가 끝날 때마다 +1(튜토리얼이 "해냈나"를 보는 신호). 판정에는 쓰지 않는다
    public int RevealCount { get; private set; }

    bool tutorialAllowMonitor, tutorialAllowPhone;
    PlayerAction? tutorialForced;                       // 매수·매도 중 이것만(null = 둘 다)
    readonly Queue<PlayerAction> tutorialClown = new Queue<PlayerAction>();

    // TutorialGuide.Awake가 부름(모든 Start보다 먼저) → 첫 라운드가 튜토리얼 값으로 시작. 처음엔 말하는 중
    public void BeginTutorial()
    {
        IsTutorial = true;
        IsGuideTalking = true;
        tutorialClown.Clear();
        foreach (char c in tutorialClownMoves.ToUpper())
        {
            if (c == 'S') tutorialClown.Enqueue(PlayerAction.Sell);
            else if (c == 'B') tutorialClown.Enqueue(PlayerAction.Buy);
        }
    }

    public void SetGuideTalking(bool on)
    {
        IsGuideTalking = on;
        ui.Refresh(this);
    }

    // 지금 단계에서 할 수 있는 것: 모니터를 열 수 있나, 폰을 열 수 있나, 매수·매도 중 무엇만(null = 둘 다)
    public void SetTutorialGate(bool monitor, bool phone, PlayerAction? forced)
    {
        tutorialAllowMonitor = monitor;
        tutorialAllowPhone = phone;
        tutorialForced = forced;
        if (forced.HasValue) Selected = forced.Value;   // 모니터를 열면 해야 할 버튼이 이미 골라져 있게
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

    // ── clown-tell 80: 광대 말풍선 신호. "지금 어떤 대사를 할 순간인지"만 알리고 글은 ClownSpeechView에 있다. 판정에는 쓰지 않는다. ──
    public ClownLine LastClownLine { get; private set; }
    public int ClownLineCount { get; private set; }     // 말할 때마다 +1 → 화면은 "바뀌었나"만 본다(같은 대사를 두 번 해도 다시 뜸)

    int clownLineFrame = -1;          // clown-tell 81: 마지막으로 말한 프레임(같은 순간에 두 대사가 겹치는지 보려고)
    Coroutine laterLine;              // 잠시 뒤에 할 대사(겹칠 때 미뤄 둔 "…한 번 쉬지.")
    [SerializeField] float clownLaterSeconds = 1.2f;   // 미룬 대사를 앞 대사가 보인 뒤 이만큼 뒤에

    void ClownSay(ClownLine line)
    {
        LastClownLine = line;
        ClownLineCount++;
        clownLineFrame = Time.frameCount;
    }

    // 81: 같은 순간에 이미 결과 대사를 했으면(예: "…칫." 뒤 바로 광대가 쉬는 차례) 덮어쓰지 않고 잠시 뒤에 말한다
    void ClownSayAfterCurrent(ClownLine line)
    {
        if (clownLineFrame != Time.frameCount)
        {
            ClownSay(line);
            return;
        }
        if (laterLine != null) StopCoroutine(laterLine);
        laterLine = StartCoroutine(SayLater(line));
    }

    IEnumerator SayLater(ClownLine line)
    {
        yield return new WaitForSeconds(clownLaterSeconds);
        laterLine = null;
        ClownSay(line);
        ui.Refresh(this);
    }

    // 내가 행동하면(모니터·폰 열기, 1~4) 말풍선을 바로 닫는다(Grilling Q9). 미뤄 둔 대사도 취소
    void ClownSilence()
    {
        if (laterLine != null)
        {
            StopCoroutine(laterLine);
            laterLine = null;
        }
        if (LastClownLine == ClownLine.None) return;
        ClownSay(ClownLine.None);
    }

    // 81: 아이템을 쓴 뒤의 결과 대사. 복권은 실제로 된 것으로
    ClownLine ItemResultLine(ItemType item)
    {
        switch (item)
        {
            case ItemType.ChartAnalysis: return ClownLine.ChartDone;
            case ItemType.Rumor: return ClownLine.RumorDone;
            case ItemType.TradingHalt: return ClownLine.HaltDone;
            case ItemType.Dividend:
            case ItemType.Bailout: return ClownLine.Healed;   // heal-items 90: "숨 좀 돌리지."
            case ItemType.Flip: return ClownLine.FlipDone;    // flip-item 101: "흐름을 바꿨다."
        }
        switch (LastLotteryOutcome)
        {
            case LotteryPrize.Heal: return ClownLine.LotteryHeal;
            case LotteryPrize.Leverage: return ClownLine.LotteryLeverage;
            case LotteryPrize.Item: return ClownLine.LotteryItem;
            default: return ClownLine.LotteryNothing;
        }
    }

    static ClownLine ItemLine(ItemType item)
    {
        switch (item)
        {
            case ItemType.ChartAnalysis: return ClownLine.UseChart;
            case ItemType.Rumor: return ClownLine.UseRumor;
            case ItemType.TradingHalt: return ClownLine.UseHalt;
            case ItemType.Dividend: return ClownLine.UseDividend;   // heal-items 90
            case ItemType.Bailout: return ClownLine.UseBailout;
            case ItemType.Flip: return ClownLine.UseFlip;          // flip-item 101
            default: return ClownLine.UseLottery;
        }
    }

    // ── item-phone 65: 아이템을 받은 신호(폰이 울리고 "+N"을 깜빡이는 데 씀). 판정에는 쓰지 않는다. ──
    public int PlayerGrantCount { get; private set; }   // 내가 아이템을 받을 때마다 +1
    public int PlayerLastGrant { get; private set; }    // 방금 실제로 받은 개수(가득 차서 못 받은 것은 빼고)
    public int ClownGrantCount { get; private set; }
    public int ClownLastGrant { get; private set; }
    // 내 폰 목록에서 "N"을 붙일 칸 수 = 맨 뒤 몇 칸이 새것인가(받으면 뒤에 붙으므로). 폰을 한 번 열었다 닫으면 0
    public int PlayerNewItems { get; private set; }

    // 받은 개수를 기록하고 신호를 올린다(0개면 신호 없음 — 가득 차서 못 받으면 울리지 않음)
    void RecordGrant(Participant who, int added)
    {
        if (added <= 0) return;
        if (who == Player)
        {
            PlayerLastGrant = added;
            PlayerGrantCount++;
            PlayerNewItems = Mathf.Min(Player.Items.Count, PlayerNewItems + added);
        }
        else
        {
            ClownLastGrant = added;
            ClownGrantCount++;
        }
    }

    // 내 폰을 열었다 닫으면(PhoneView가 부름) 새 칸 표시를 지운다
    public void ClearNewItems() { PlayerNewItems = 0; }

    // 내 아이템을 쓰며 뺄 때: 뺀 칸이 "새것"(맨 뒤 PlayerNewItems칸) 중 하나였으면 새것 수도 하나 줄임
    void RemovePlayerItemAt(int slot)
    {
        if (slot >= Player.Items.Count - PlayerNewItems) PlayerNewItems = Mathf.Max(0, PlayerNewItems - 1);
        Player.RemoveItemAt(slot);
    }
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
        RoundConfig config = CurrentConfig;   // tutorial 97: 튜토리얼이면 각본 라운드
        deck.ForceFixedOrder = IsTutorial;

        // 93: 시작 잔고를 범위에서 하나 뽑아 둘 다 같은 값으로(공정하게). Random.Range(int, int)는 끝 값을 빼므로 + 1
        int balance = Random.Range(config.minBalance, Mathf.Max(config.minBalance, config.maxBalance) + 1);
        Player = new Participant("나", balance);
        Clown = new Participant("광대", balance);
        Leverage = new LeverageCounter();
        Current = Player;
        Selected = PlayerAction.Buy;
        IsRevealing = false;
        IsClownThinking = false;
        ConfirmingSlot = -1;
        IsMonitorOpen = false;      // monitor-trade 43
        IsPhoneOpen = false;        // item-phone 63
        PlayerNewItems = 0;         // item-phone 65: 새 라운드 = 새 아이템(아래 GiveItems에서 다시 셈)
        PhoneCursor = 0;
        PlayerKnownNext = null;
        clownKnownNext = null;
        LastResult = "";
        ClownSilence();             // clown-tell 80: 새 라운드엔 말풍선 없이
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
        if (IsCutscene || IsPaused || IsPresenting || IsRevealing || IsRoundOver || IsClownTurn || IsConfirming || IsPhoneOpen) return;
        if (IsTutorial && tutorialForced.HasValue && action != tutorialForced.Value) return;   // tutorial 97: 이번 단계 행동만
        if (Selected != action) audioManager.PlayClick();   // 실제로 바뀔 때만
        Selected = action;
        ui.Refresh(this);
    }

    // Space. 공개 중이면 무시. 아이템 확인 중이면 그 아이템 사용. 라운드 클리어 화면이면 다음 라운드.
    // 최종 승리·패배면 무시. 광대 차례면 무시.
    public void Execute()
    {
        if (IsCutscene || IsPaused || IsRevealing) return;

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
        if (IsTutorial && tutorialForced.HasValue) Selected = tutorialForced.Value;   // tutorial 97: 안전장치

        StartCoroutine(RevealRoutine(Selected));
    }

    // 1~4. 내 차례(공개 중·광대 차례·라운드 끝이 아닐 때)에 그 슬롯에 아이템이 있으면 확인을 연다.
    public void SelectItem(int slot)
    {
        if (IsCutscene || IsPaused || IsPresenting || IsRevealing || IsRoundOver || IsClownTurn) return;
        if (IsTutorial && !tutorialAllowPhone) return;   // tutorial 97: 폰 단계에서만
        if (slot < 0 || slot >= Player.Items.Count) return;
        ConfirmingSlot = slot;
        ClownSilence();             // clown-tell 80: 1~4도 내 행동
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
        if (IsCutscene || IsPaused || IsPresenting || IsRevealing || IsRoundOver || IsClownTurn) return;
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
        ClownSilence();             // clown-tell 80: 내가 행동하면 말풍선 닫기
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
        ClownSilence();             // clown-tell 80
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
        ClownSilence();             // clown-tell 80: 내가 행동하면 말풍선 닫기
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
        // game-settings 91: 설정 창이 열려 있으면 Esc = 설정 닫기(일시정지 메뉴로, 톱니로 열었으면 게임으로)
        if (IsSettingsOpen)
        {
            CloseSettings();
            return;
        }
        if (!CanPause) return;
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
        ui.Refresh(this);
    }

    // 지금 멈출 수 있나: 시네마틱 중 Esc = 넘기기(intro-story 77), 결과 화면 아님, 열린 화면은 Esc로 먼저 닫음(monitor-trade 43·item-phone 63)
    public bool CanPause => !(IsIntro || IsGameOver || IsConfirming || IsMonitorOpen || IsPhoneOpen);

    // 일시정지 패널의 계속 버튼
    public void Resume()
    {
        if (!IsPaused) return;
        if (IsSettingsOpen) IsSettingsOpen = false;
        TogglePause();
    }

    // ── game-settings 91: 게임 중 설정. 판정은 하지 않고, 멈춘 상태에서 설정 창을 켜고 끌 뿐 ──
    public bool IsSettingsOpen { get; private set; }
    bool settingsFromGear;     // 톱니로 열었나(닫으면 바로 게임으로) / 일시정지 메뉴에서 열었나(닫으면 메뉴로)
    public bool CanOpenGear => CanPause && !IsPaused;   // 톱니 버튼이 보이고 눌리는 때

    public void OpenSettings(bool fromGear)
    {
        if (IsSettingsOpen) return;
        if (!IsPaused)
        {
            if (!CanPause) return;
            IsPaused = true;
            Time.timeScale = 0f;
        }
        settingsFromGear = fromGear;
        IsSettingsOpen = true;
        audioManager.PlayClick();
        ui.Refresh(this);
    }

    public void CloseSettings()
    {
        if (!IsSettingsOpen) return;
        IsSettingsOpen = false;
        if (settingsFromGear)
        {
            IsPaused = false;
            Time.timeScale = 1f;
        }
        settingsFromGear = false;
        audioManager.PlayClick();
        ui.Refresh(this);
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
                RemovePlayerItemAt(ConfirmingSlot);
                UseRumor(Player);
                break;
            case ItemType.ChartAnalysis:
                RemovePlayerItemAt(ConfirmingSlot);
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
                RemovePlayerItemAt(ConfirmingSlot);
                UseTradingHalt(Player);
                break;
            case ItemType.Lottery:
                RemovePlayerItemAt(ConfirmingSlot);
                UseLottery(Player);
                break;
            case ItemType.Flip:   // flip-item 101
                RemovePlayerItemAt(ConfirmingSlot);
                UseFlip(Player);
                break;
            case ItemType.Dividend:
            case ItemType.Bailout:
                // heal-items 89: 잔고가 가득이면 못 씀(아이템은 남음) — 거래 정지와 같은 방식
                if (CannotHeal(Player, item))
                {
                    LastResult = $"{ItemName(item)}: 사용 불가 (잔고가 가득)";
                    used = false;
                    break;
                }
                RemovePlayerItemAt(ConfirmingSlot);
                UseHeal(Player, item);
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

    // flip-item 101: 작전 — 덱 맨 위 1장을 반대로. 차트 분석으로 알던 사람은 뒤집힌 값으로 안다(쓴 사실은 둘 다 앎, Grilling Q3).
    // 차례·레버리지는 그대로(아이템은 행동이 아님). 덱이 비어 있으면(재장전 직전 등) 아무 일 없음
    void UseFlip(Participant user)
    {
        if (deck.IsEmpty) return;
        Candle before = deck.Peek();
        deck.FlipTop();
        Candle after = deck.Peek();
        if (PlayerKnownNext.HasValue) PlayerKnownNext = after;
        if (clownKnownNext.HasValue) clownKnownNext = after;
        LastResult = $"{user.Name}: 작전 사용 → 다음 캔들을 뒤집음";
        if (user == Player)
        {
            // 결과 판(LotteryPopupView)에 보일 한 줄: 알고 있었으면 무엇에서 무엇으로, 모르면 뒤집었다는 사실만
            LastFlipText = PlayerKnownNext.HasValue ? $"다음 캔들: {CandleName(before)} → {CandleName(after)}" : "다음 캔들을 뒤집었다";
            PlayerFlipCount++;
        }
    }

    // flip-item 101: 내가 작전을 쓸 때마다 +1(결과 판 신호)과 그 한 줄. 판정에는 쓰지 않는다
    public int PlayerFlipCount { get; private set; }
    public string LastFlipText { get; private set; } = "";

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

        LastLotteryOutcome = LotteryPrize.Nothing;   // clown-tell 81: 실제로 된 것(적용 못 하면 꽝)

        switch (prize)
        {
            case LotteryPrize.Heal:
                bool healed = user.Heal(1);
                if (healed) LastLotteryOutcome = LotteryPrize.Heal;
                result = healed ? "잔고 +1" : "꽝 (잔고가 이미 가득)";
                break;
            case LotteryPrize.Leverage:
                if (Leverage.Value < LeverageCounter.Max)
                {
                    Leverage.Increase();
                    RecordLeverage();
                    LastLotteryOutcome = LotteryPrize.Leverage;
                    result = $"레버리지 +1 (현재 {Leverage.Value})";
                }
                else
                {
                    result = "꽝 (레버리지가 이미 최대)";
                }
                break;
            case LotteryPrize.Item:
                ItemType bonus = NextItem(user);
                bool got = Give(user, bonus);   // item-limit 116: 복권으로 받는 아이템도 라운드 제한을 따름
                if (got) RecordGrant(user, 1);   // item-phone 65: 복권으로 받은 아이템도 폰이 울림
                if (got) LastLotteryOutcome = LotteryPrize.Item;
                result = got ? $"아이템 +1 ({ItemName(bonus)})" : "꽝 (아이템이 이미 가득)";
                break;
            default:
                result = "꽝";
                break;
        }

        // monitor-trade 42 (#60): 무엇이 당첨됐는지는 화면 글자로 알리지 않는다 — 잔고·레버리지·아이템 변화로만 보인다.
        // 당첨 내용은 테스트·개발용으로만 남긴다(화면에 쓰지 않음).
        LastLotteryDetail = result;
        LastResult = $"{user.Name}: 복권 사용";
        // round-feedback 100: 내가 쓴 복권은 결과를 화면에 보여 준다(#60을 바꿈) → 화면은 이 번호가 바뀌었나만 본다
        if (user == Player) PlayerLotteryCount++;
    }

    // round-feedback 100: 내가 복권을 쓸 때마다 +1(결과 연출 신호). 판정에는 쓰지 않는다
    public int PlayerLotteryCount { get; private set; }

    // 마지막 복권의 당첨 내용(화면에 보이지 않음, 테스트용)
    public string LastLotteryDetail { get; private set; } = "";
    // clown-tell 81: 마지막 복권에서 실제로 된 것(못 받은 것 = Nothing). 광대 대사를 고를 때 씀
    public LotteryPrize LastLotteryOutcome { get; private set; } = LotteryPrize.Nothing;

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
        // item-phone 65: 실제로 받은 개수를 세서 "받았다" 신호(가득 차면 덜 받음)
        int mine = 0, clowns = 0;
        // tutorial 97: 튜토리얼은 라운드 시작 때 내 아이템만 각본대로(광대는 없음), 재장전 땐 없음
        if (IsTutorial)
        {
            if (!LastFillWasRoundStart) return;
            foreach (char c in tutorialPlayerItems)
            {
                ItemType? t = ItemFromChar(c);
                if (t.HasValue && Player.AddItem(t.Value)) mine++;
            }
            RecordGrant(Player, mine);
            return;
        }
        for (int i = 0; i < itemsPerGrant; i++) if (Give(Player, NextItem(Player))) mine++;
        for (int i = 0; i < itemsPerGrant; i++) if (Give(Clown, NextItem(Clown))) clowns++;
        RecordGrant(Player, mine);
        RecordGrant(Clown, clowns);
    }

    // item-limit 116: 슬롯에 넣고, 실제로 받았으면 "이번 라운드에 받음" 표시(거래 정지·회복템 제한용)
    static bool Give(Participant who, ItemType item)
    {
        if (!who.AddItem(item)) return false;
        who.MarkGiven(item);
        return true;
    }

    // 다음에 줄 아이템. 고정 지급이 켜져 있으면 문자열에서 순서대로, 다 쓰면 무작위.
    // item-limit 116: 무작위는 받을 사람이 이번 라운드에 이미 받은 거래 정지·회복템을 빼고 고름(고정 지급은 시험용이라 그대로)
    ItemType NextItem(Participant who)
    {
        if (useFixedItems)
        {
            while (fixedItemIndex < fixedItems.Length)
            {
                ItemType? t = ItemFromChar(fixedItems[fixedItemIndex]);
                fixedItemIndex++;
                if (t.HasValue) return t.Value;
                // 공백 등 다른 글자는 건너뛴다.
            }
        }
        return RandomItem(who);
    }

    // 글자 → 아이템(C 차트 분석 / J 찌라시 / S 거래 정지 / L 복권 / D 배당금 / B 구제 금융). tutorial 97: 튜토리얼 아이템도 같이 씀
    static ItemType? ItemFromChar(char ch)
    {
        switch (char.ToUpper(ch))
        {
            case 'C': return ItemType.ChartAnalysis;
            case 'J': return ItemType.Rumor;
            case 'S': return ItemType.TradingHalt;
            case 'L': return ItemType.Lottery;
            case 'D': return ItemType.Dividend;   // heal-items 89
            case 'B': return ItemType.Bailout;
            case 'F': return ItemType.Flip;          // flip-item 101
            default: return null;
        }
    }

    // heal-items 89: 아이템마다 나올 무게(ItemType 순서). 구제 금융(+2)은 세서 절반(Grilling Q3). 숫자만 바꾸면 조정
    // flip-item 101: 작전도 절반(0.5, Grilling Q6)
    static readonly float[] ItemWeights = { 1f, 1f, 1f, 1f, 1f, 0.5f, 0.5f };

    // 무게대로 하나 고르기: 0 ~ 무게 합 사이 숫자를 뽑고, 무게를 앞에서부터 빼 가다 0 밑으로 내려간 칸
    // item-limit 116: 받을 수 없는 종류(who.CanReceive가 false)는 무게 0 — 나머지 비율은 그대로
    static ItemType RandomItem(Participant who)
    {
        float W(int i) => who.CanReceive((ItemType)i) ? ItemWeights[i] : 0f;
        float total = 0f;
        for (int i = 0; i < ItemWeights.Length; i++) total += W(i);
        float r = Random.Range(0f, total);
        int last = 0;
        for (int i = 0; i < ItemWeights.Length; i++)
        {
            if (W(i) <= 0f) continue;
            last = i;
            r -= W(i);
            if (r < 0f) return (ItemType)i;
        }
        return (ItemType)last;   // 소수점 오차 대비(받을 수 있는 마지막 종류)
    }

    // heal-items 89: 회복량(배당금 1·구제 금융 2, 회복 아이템이 아니면 0)
    public static int HealAmount(ItemType item) =>
        item == ItemType.Dividend ? 1 : item == ItemType.Bailout ? 2 : 0;

    // 잔고가 가득이면 회복 아이템을 쓸 수 없다(Grilling Q2) — 확인 화면 안내에도 씀
    public static bool CannotHeal(Participant p, ItemType item) =>
        HealAmount(item) > 0 && p.Balance >= p.StartingBalance;

    // 회복: Participant.Heal이 시작 잔고를 넘지 않게 자름(1만 비었으면 구제 금융도 1)
    void UseHeal(Participant user, ItemType item)
    {
        int before = user.Balance;
        user.Heal(HealAmount(item));
        LastResult = $"{user.Name}: {ItemName(item)} 사용 → 잔고 +{user.Balance - before}";
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
        RevealCount++;
        // tutorial 97: 공개가 끝날 때마다 튜토리얼 광대가 먼저 말할 수 있게 멈춰 둠(광대 차례도 대사 뒤로). 할 말이 없으면 TutorialGuide가 바로 풀어 줌
        if (IsTutorial) IsGuideTalking = true;
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
        // tutorial 97: 튜토리얼 광대가 말하는 동안에도 기다림
        while (IsPresenting || IsGuideTalking) yield return null;

        IsClownThinking = true;
        ui.Refresh(this);

        yield return new WaitForSeconds(clownThinkSeconds);

        IsClownThinking = false;
        // tutorial 97: 튜토리얼 각본이 남아 있으면 그 행동(아이템 없이). 다 쓰면 아래 원래 판단
        if (IsTutorial && tutorialClown.Count > 0)
        {
            Selected = tutorialClown.Dequeue();
            ClownSay(Selected == PlayerAction.Buy ? ClownLine.Buy : ClownLine.Sell);
            ui.Refresh(this);
            yield return new WaitForSeconds(clownDeclareSeconds);
            StartCoroutine(RevealRoutine(Selected));
            yield break;
        }
        ClownMove move = ClownAI.Decide(deck.RemainingDown, deck.RemainingUp, Clown.Items,
                                        Clown.Balance, Player.IsHalted, clownKnownNext, clownLotteryThreshold,
                                        Clown.StartingBalance);   // heal-items 90

        if (move.UsesItem)
        {
            ClownSay(ItemLine(move.Item));                  // clown-tell 80: "찌라시를 쓰지." 등
            UseClownItem(move.Item);
            ui.Refresh(this);
            yield return new WaitForSeconds(clownItemSeconds);   // 읽을 시간 뒤 다음 판단
            // clown-tell 81: 아이템 결과 대사("다들 봤지?" 등) → 다음 생각 동안 떠 있음
            ClownSay(ItemResultLine(move.Item));
            ui.Refresh(this);
            StartCoroutine(ClownTurnRoutine());
            yield break;
        }

        Selected = move.Action;
        ClownSay(move.Action == PlayerAction.Buy ? ClownLine.Buy : ClownLine.Sell);   // clown-tell 80
        ui.Refresh(this);
        yield return new WaitForSeconds(clownDeclareSeconds);   // 말풍선을 읽은 뒤 공개
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
            case ItemType.Dividend:
            case ItemType.Bailout: UseHeal(Clown, item); break;   // heal-items 90
            case ItemType.Flip: UseFlip(Clown); break;           // flip-item 101
        }
    }

    void ResolveBuy(Candle candle)
    {
        if (candle == Candle.Up)
        {
            Leverage.Increase();
            RecordLeverage();
            LastResult = $"매수 → 상승! 레버리지 {Leverage.Value}, 차례 유지";
            if (Current == Clown) ClownSay(ClownLine.OneMore);   // clown-tell 81: "한 번 더."
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
            // clown-tell 81: 맞은 사람이 광대면 "…칫.", 나면 광대가 "하하."(Q8: 내 차례엔 피해 때만)
            ClownSay(Current == Clown ? ClownLine.Hurt : ClownLine.Laugh);
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
            // clown-tell 81: 광대가 넘겨 내가 맞으면 "잘 받았지?", 내가 넘겨 광대가 맞으면 "…칫."
            ClownSay(Current == Clown ? ClownLine.Gotcha : ClownLine.Hurt);
        }
        else
        {
            LastResult = $"매도 → 상승! 변화 없음 (레버리지 {Leverage.Value} 그대로)";
            if (Current == Clown) ClownSay(ClownLine.NothingHappened);   // clown-tell 81: 내 매도+상승엔 말 없음(Q8)
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
            if (Current == Clown) ClownSayAfterCurrent(ClownLine.Halted);   // clown-tell 80(Spec 제안 3): "…한 번 쉬지." 81: 결과 대사와 겹치면 잠시 뒤
            Current.ClearHalt();
            LastResult += $"\n{Current.Name}: 거래 정지로 턴을 건너뜀";
            Current = Opponent;
        }
    }
}