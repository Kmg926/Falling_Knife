// 플레이어·광대 한 명. MonoBehaviour가 아니라서 GameObject에 붙이지 않는다.
public class Participant
{
    public string Name { get; }
    public int Balance { get; private set; }

    public Participant(string name, int startingBalance)
    {
        Name = name;
        Balance = startingBalance;
    }
}
