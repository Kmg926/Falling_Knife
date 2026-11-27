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
        // ①′ flip-item 101: 다음이 하락인 걸 알고 작전이 있으면 → 작전(뒤집은 뒤 다시 물으면 상승을 아니까 ①에서 매수)
        if (knownNext.HasValue && knownNext.Value == Candle.Down && Has(items, ItemType.Flip)) return ClownMove.UseItem(ItemType.Flip);

        // ① 차트 분석으로 다음 캔들을 알면 그대로: 상승이면 매수, 하락이면 매도
        if (knownNext.HasValue)
        {
            return ClownMove.Act(knownNext.Value == Candle.Up ? PlayerAction.Buy : PlayerAction.Sell);
        }
        if (Has(items, ItemType.ChartAnalysis)) return ClownMove.UseItem(ItemType.ChartAnalysis);

        // ② 거래 정지가 있고 플레이어가 정지 상태가 아니면 사용
        if (Has(items, ItemType.TradingHalt) && !opponentHalted) return ClownMove.UseItem(ItemType.TradingHalt);

        // ③-1 heal-items 90: 잔고가 기준값 이하이고 회복 아이템(배당금)이 있으면 사용(복권보다 먼저 — 확실한 회복이 운보다 앞). 가득이면 쓰지 않음.
        if (balance <= lotteryThreshold && balance < maxBalance && Has(items, ItemType.Dividend)) return ClownMove.UseItem(ItemType.Dividend);
        // ③-2 risk-bond 121: 위험 채권(전 구제 금융, 50% +2 / 50% −1)은 잔고가 기준값 이하일 때 "마지막 희망"으로(Grilling Q7).
        //      단 잔고 1이면 지면 바로 파산이라 쓰지 않음(2 이상일 때만)
        if (balance <= lotteryThreshold && balance >= 2 && Has(items, ItemType.Bailout)) return ClownMove.UseItem(ItemType.Bailout);
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
