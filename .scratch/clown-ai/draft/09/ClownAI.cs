// 광대의 "판단"만 한다. 기억할 상태가 없어서 Scene에 붙이지 않는다(static).
// GDD 광대 규칙 ④만: 남은 덱의 하락 비율이 50% 이상이면 매도, 미만이면 보유.
// 아이템 규칙(①~③)은 아이템을 만들 때 여기에 덧붙인다.
public static class ClownAI
{
    public static PlayerAction Decide(int remainingDown, int remainingUp)
    {
        int total = remainingDown + remainingUp;

        // 안전장치: 덱이 비면 재장전되므로 실제로는 오지 않는다.
        if (total == 0) return PlayerAction.Hold;

        float downRatio = (float)remainingDown / total;
        return downRatio >= 0.5f ? PlayerAction.Sell : PlayerAction.Hold;
    }
}
