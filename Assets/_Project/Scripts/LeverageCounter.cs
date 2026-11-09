// 두 참가자가 함께 쓰는 레버리지 (0~3).
public class LeverageCounter
{
    public const int Max = 3;

    public int Value { get; private set; }

    // 보유+상승. 3에서는 더 오르지 않는다.
    public void Increase()
    {
        if (Value < Max) Value++;
    }

    // 하락 피해가 들어간 뒤 0으로.
    public void Reset()
    {
        Value = 0;
    }
}
