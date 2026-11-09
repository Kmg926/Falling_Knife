// 플레이어·광대 한 명. MonoBehaviour가 아니라서 GameObject에 붙이지 않는다.
public class Participant
{
    public string Name { get; }
    public int Balance { get; private set; }
    public bool IsBankrupt => Balance == 0;

    public Participant(string name, int startingBalance)
    {
        Name = name;
        Balance = startingBalance;
    }

    // 잔고는 0 밑으로 내려가지 않는다.
    public void TakeDamage(int amount)
    {
        Balance -= amount;
        if (Balance < 0) Balance = 0;
    }
}
