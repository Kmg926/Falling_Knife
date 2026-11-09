using System.Collections.Generic;

// 광대가 할 일 하나: 아이템을 쓰거나, 보유/매도를 한다.
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
                                   int balance, bool opponentHalted, Candle? knownNext, int lotteryThreshold)
    {
        // ① 차트 분석으로 다음 캔들을 알면 그대로: 상승이면 보유, 하락이면 매도
        if (knownNext.HasValue)
        {
            return ClownMove.Act(knownNext.Value == Candle.Up ? PlayerAction.Hold : PlayerAction.Sell);
        }
        if (Has(items, ItemType.ChartAnalysis)) return ClownMove.UseItem(ItemType.ChartAnalysis);

        // ② 거래 정지가 있고 플레이어가 정지 상태가 아니면 사용
        if (Has(items, ItemType.TradingHalt) && !opponentHalted) return ClownMove.UseItem(ItemType.TradingHalt);

        // ③ 잔고가 기준값 이하이고 복권이 있으면 사용 (GDD는 잔고 1, gdd-changes #32)
        if (balance <= lotteryThreshold && Has(items, ItemType.Lottery)) return ClownMove.UseItem(ItemType.Lottery);

        // ④ 남은 덱의 하락 비율 50% 이상이면 찌라시(있으면) 또는 매도, 미만이면 보유
        int total = remainingDown + remainingUp;
        if (total == 0) return ClownMove.Act(PlayerAction.Hold);    // 안전장치

        float downRatio = (float)remainingDown / total;
        if (downRatio >= 0.5f)
        {
            if (Has(items, ItemType.Rumor)) return ClownMove.UseItem(ItemType.Rumor);
            return ClownMove.Act(PlayerAction.Sell);
        }
        return ClownMove.Act(PlayerAction.Hold);
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
