using System.Collections;
using UnityEngine;

// 차례인 사람이 고를 수 있는 행동
public enum PlayerAction { Hold, Sell }

// 복권 결과 4가지 (각 25%)
public enum LotteryPrize { Heal, Leverage, Item, Nothing }

public class GameManager : MonoBehaviour
{
    [SerializeField] CandleDeck deck;
    [SerializeField] UIManager ui;

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

    // 지금 차례가 아닌 쪽
    Participant Opponent => (Current == Player) ? Clown : Player;

    void Start()
    {
        fixedItemIndex = 0;
        RoundNumber = 1;
        StartRound();
    }

    // RoundNumber 줄의 값으로 라운드를 새로 시작한다. 라운드 번호는 여기서 바꾸지 않는다.
    void StartRound()
    {
        RoundConfig config = rounds[RoundNumber - 1];

        Player = new Participant("플레이어", config.startingBalance);
        Clown = new Participant("광대", config.startingBalance);
        Leverage = new LeverageCounter();
        Current = Player;
        Selected = PlayerAction.Hold;
        IsRevealing = false;
        IsClownThinking = false;
        ConfirmingSlot = -1;
        PlayerKnownNext = null;
        clownKnownNext = null;
        LastResult = "";
        deck.SetComposition(config.minCards, config.maxCards, config.fixedOrder);
        deck.Fill();
        GiveItems();
        ui.Refresh(this);
    }

    // A/D·←/→. 공개 중, 라운드 끝, 광대 차례, 아이템 확인 중이면 무시한다.
    public void Select(PlayerAction action)
    {
        if (IsRevealing || IsRoundOver || IsClownTurn || IsConfirming) return;
        Selected = action;
        ui.Refresh(this);
    }

    // Space. 공개 중이면 무시. 아이템 확인 중이면 그 아이템 사용. 라운드 클리어 화면이면 다음 라운드.
    // 최종 승리·패배면 무시. 광대 차례면 무시.
    public void Execute()
    {
        if (IsRevealing) return;

        if (IsConfirming)
        {
            UseConfirmedItem();
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

        StartCoroutine(RevealRoutine(Selected));
    }

    // 1~4. 내 차례(공개 중·광대 차례·라운드 끝이 아닐 때)에 그 슬롯에 아이템이 있으면 확인을 연다.
    public void SelectItem(int slot)
    {
        if (IsRevealing || IsRoundOver || IsClownTurn) return;
        if (slot < 0 || slot >= Player.Items.Count) return;
        ConfirmingSlot = slot;
        ui.Refresh(this);
    }

    // Esc. 확인을 닫는다. 아이템은 그대로 남는다.
    public void CancelItem()
    {
        if (!IsConfirming) return;
        ConfirmingSlot = -1;
        ui.Refresh(this);
    }

    // 다음 줄로 라운드 번호를 올리고 새로 시작한다.
    void NextRound()
    {
        RoundNumber++;
        StartRound();
    }

    // R. 라운드가 끝난 화면에서만 받는다. 항상 1라운드부터. (테스트용, 최종 빌드 전 제거)
    public void Restart()
    {
        if (IsRevealing || !IsRoundOver) return;
        fixedItemIndex = 0;
        RoundNumber = 1;
        StartRound();
    }

    // 확인 중인 내 아이템을 쓴다. 아이템은 행동이 아니라서 차례는 넘어가지 않는다.
    void UseConfirmedItem()
    {
        ItemType item = Player.Items[ConfirmingSlot];

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
                break;
        }

        ConfirmingSlot = -1;
        ui.Refresh(this);
    }

    // 찌라시: 다음 캔들을 모두 앞에서 공개하고 버린다. 덱이 비면 바로 재장전.
    void UseRumor(Participant user)
    {
        Candle top = DrawTop();
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
        LastResult = $"{user.Name}: 거래 정지 사용 → {Opponent.Name} 다음 턴 건너뜀";
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
                    result = $"레버리지 +1 (레버리지 {Leverage.Value})";
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

        LastResult = $"{user.Name}: 복권 → {result}";
    }

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
        ui.Refresh(this);

        yield return new WaitForSeconds(revealSeconds);

        Candle candle = DrawTop();

        // 행동한 사람은 판정 전에 기억한다: 판정 안의 PassTurn()이 Current를 상대로 바꾸기 때문.
        Participant actor = Current;

        // 처리 순서: ① 피해 적용 → ② 잔고 0 판정 → ③ 덱 소진 확인
        if (action == PlayerAction.Hold) ResolveHold(candle);
        else ResolveSell(candle);

        // 결과 문구 앞에 행동한 사람. 재장전 줄은 이 뒤에 붙는다.
        LastResult = $"{actor.Name}: {LastResult}";

        if (!IsRoundOver && deck.IsEmpty)
        {
            Reload();
        }

        IsRevealing = false;
        ui.Refresh(this);

        // 결과 뒤에도 광대 차례면(내가 넘겼거나, 광대가 보유+상승) 광대가 행동한다.
        if (IsClownTurn)
        {
            StartCoroutine(ClownTurnRoutine());
        }
    }

    // "광대 생각 중..." (clownThinkSeconds) → ClownAI에게 묻기
    // → 아이템이면 쓰고 다시 생각(①부터), 보유·매도면 기존 공개 처리
    IEnumerator ClownTurnRoutine()
    {
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

    // 광대가 아이템 하나를 쓴다. 효과 함수는 플레이어와 같은 것을 쓴다.
    void UseClownItem(ItemType item)
    {
        Clown.RemoveItem(item);

        switch (item)
        {
            case ItemType.ChartAnalysis: UseChartAnalysis(Clown); break;
            case ItemType.Rumor: UseRumor(Clown); break;
            case ItemType.TradingHalt: UseTradingHalt(Clown); break;
            case ItemType.Lottery: UseLottery(Clown); break;
        }
    }

    void ResolveHold(Candle candle)
    {
        if (candle == Candle.Up)
        {
            Leverage.Increase();
            LastResult = $"보유 → 상승! 레버리지 {Leverage.Value}, 차례 유지";
        }
        else
        {
            int damage = 1 + Leverage.Value;
            Current.TakeDamage(damage);
            Leverage.Reset();
            LastResult = $"보유 → 하락! {Current.Name} -{damage}";
            PassTurn();
        }
    }

    // 매도: 하락이면 상대가 맞는다. 어느 쪽이든 차례는 넘어간다.
    void ResolveSell(Candle candle)
    {
        if (candle == Candle.Down)
        {
            int damage = 1 + Leverage.Value;
            Opponent.TakeDamage(damage);
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
        deck.Fill();
        GiveItems();
        LastResult += $"\n덱 재장전 (하락{deck.RemainingDown} / 상승{deck.RemainingUp}), 아이템 +{itemsPerGrant}";
    }

    // 상대에게 차례를 넘기고 선택은 보유로 되돌린다.
    // 넘겨받은 사람이 거래 정지 상태면 그 차례를 건너뛰고 다시 넘긴다(모든 차례 넘김이 여기를 지난다).
    void PassTurn()
    {
        Current = Opponent;
        Selected = PlayerAction.Hold;

        if (Current.IsHalted && !IsRoundOver)
        {
            Current.ClearHalt();
            LastResult += $"\n{Current.Name}: 거래 정지로 턴을 건너뜀";
            Current = Opponent;
        }
    }
}
