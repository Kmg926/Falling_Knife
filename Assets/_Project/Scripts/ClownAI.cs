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
