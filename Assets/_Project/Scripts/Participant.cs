using System.Collections.Generic;

// 플레이어·광대 한 명. MonoBehaviour가 아니라서 GameObject에 붙이지 않는다.
public class Participant
{
    public const int MaxItems = 4;

    public string Name { get; }
    public int Balance { get; private set; }
    // 라운드 시작 잔고. 복권으로 회복할 때 이 값을 넘지 않는다.
    public int StartingBalance { get; }
    public bool IsBankrupt => Balance == 0;

    // 아이템 슬롯 (0번 = 1번 키). 밖에서는 읽기만 한다.
    readonly List<ItemType> items = new List<ItemType>();
    public IReadOnlyList<ItemType> Items => items;

    // 거래 정지에 걸렸는가. 다음 차례 한 번을 건너뛰면 풀린다.
    public bool IsHalted { get; private set; }

    public Participant(string name, int startingBalance)
    {
        Name = name;
        Balance = startingBalance;
        StartingBalance = startingBalance;
    }

    // 잔고는 0 밑으로 내려가지 않는다.
    public void TakeDamage(int amount)
    {
        Balance -= amount;
        if (Balance < 0) Balance = 0;
    }

    // 잔고 회복. 이미 시작 잔고면 회복하지 못한다(false).
    public bool Heal(int amount)
    {
        if (Balance >= StartingBalance) return false;
        Balance += amount;
        if (Balance > StartingBalance) Balance = StartingBalance;
        return true;
    }

    // 4개가 차 있으면 받지 못한다(버림). 받았으면 true.
    public bool AddItem(ItemType item)
    {
        if (items.Count >= MaxItems) return false;
        items.Add(item);
        return true;
    }

    // 거래 정지를 건다 / 푼다.
    public void Halt()
    {
        IsHalted = true;
    }

    public void ClearHalt()
    {
        IsHalted = false;
    }

    // 그 종류의 아이템 하나를 뺀다(광대용: 슬롯 번호 대신 종류로). 없으면 false.
    public bool RemoveItem(ItemType item)
    {
        return items.Remove(item);
    }

    // 쓴 아이템을 슬롯에서 뺀다. 뒤 슬롯은 한 칸씩 앞으로 당겨진다.
    public void RemoveItemAt(int slot)
    {
        items.RemoveAt(slot);
    }
}
