using System.Collections.Generic;

// 광대가 할 일 하나: 아이템을 쓰거나, 매수/매도를 한다.
public struct ClownMove
{
    public bool UsesItem;
    public ItemType Item;
    public PlayerAction Action;

    public static ClownMove UseItem(ItemType item)
    {
        return new ClownMove { UsesItem = true, Item = item };
    }

    public static ClownMove Act(PlayerAction action)
    {
        return new ClownMove { UsesItem = false, Action = action };
    }
}

// 광대의 "판단"만 한다. 기억할 상태가 없어서 Scene에 붙이지 않는다(static).
// GDD 광대 규칙 ①~④ 순서. 아이템을 하나 쓰면 GameManager가 다시 물어본다(①부터 다시).
public static class ClownAI
{
    public static ClownMove Decide(int remainingDown, int remainingUp, IReadOnlyList<ItemType> items,
                                   int balance, bool opponentHalted, Candle? knownNext, int lotteryThreshold,
                                   int maxBalance)   // heal-items 90: 회복 상한(= 시작 잔고)
    {
        // ① 차트 분석으로 다음 캔들을 알면 그대로: 상승이면 매수, 하락이면 매도
        if (knownNext.HasValue)
        {
            return ClownMove.Act(knownNext.Value == Candle.Up ? PlayerAction.Buy : PlayerAction.Sell);
        }
        if (Has(items, ItemType.ChartAnalysis)) return ClownMove.UseItem(ItemType.ChartAnalysis);

        // ② 거래 정지가 있고 플레이어가 정지 상태가 아니면 사용
        if (Has(items, ItemType.TradingHalt) && !opponentHalted) return ClownMove.UseItem(ItemType.TradingHalt);

        // ③-1 heal-items 90: 잔고가 기준값 이하이고 회복 아이템이 있으면 사용(복권보다 먼저 — 확실한 회복이 운보다 앞).
        //      2 이상 비었으면 구제 금융(+2)을 먼저, 1만 비었으면 배당금(+1)을 먼저(구제 금융을 아낌). 가득이면 쓰지 않음.
        if (balance <= lotteryThreshold && balance < maxBalance)
        {
            bool bigFirst = maxBalance - balance >= 2;
            ItemType first = bigFirst ? ItemType.Bailout : ItemType.Dividend;
            ItemType second = bigFirst ? ItemType.Dividend : ItemType.Bailout;
            if (Has(items, first)) return ClownMove.UseItem(first);
            if (Has(items, second)) return ClownMove.UseItem(second);
        }
        // ③ 잔고가 기준값 이하이고 복권이 있으면 사용 (GDD는 잔고 1, gdd-changes #32)
        if (balance <= lotteryThreshold && Has(items, ItemType.Lottery)) return ClownMove.UseItem(ItemType.Lottery);

        // ④ 남은 덱의 하락 비율 50% 이상이면 찌라시(있으면) 또는 매도, 미만이면 매수
        int total = remainingDown + remainingUp;
        if (total == 0) return ClownMove.Act(PlayerAction.Buy);    // 안전장치

        float downRatio = (float)remainingDown / total;
        if (downRatio >= 0.5f)
        {
            if (Has(items, ItemType.Rumor)) return ClownMove.UseItem(ItemType.Rumor);
            return ClownMove.Act(PlayerAction.Sell);
        }
        return ClownMove.Act(PlayerAction.Buy);
    }

    static bool Has(IReadOnlyList<ItemType> items, ItemType item)
    {
        foreach (ItemType i in items)
        {
            if (i == item) return true;
        }
        return false;
    }
}
