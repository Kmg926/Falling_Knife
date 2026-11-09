using System.Collections.Generic;

// 플레이어·광대 한 명. MonoBehaviour가 아니라서 GameObject에 붙이지 않는다.
public class Participant
{
    public const int MaxItems = 4;

    public string Name { get; }
    public int Balance { get; private set; }
    public bool IsBankrupt => Balance == 0;

    // 아이템 슬롯 (0번 = 1번 키). 밖에서는 읽기만 한다.
    readonly List<ItemType> items = new List<ItemType>();
    public IReadOnlyList<ItemType> Items => items;

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

    // 4개가 차 있으면 받지 못한다(버림). 받았으면 true.
    public bool AddItem(ItemType item)
    {
        if (items.Count >= MaxItems) return false;
        items.Add(item);
        return true;
    }

    // 쓴 아이템을 슬롯에서 뺀다. 뒤 슬롯은 한 칸씩 앞으로 당겨진다.
    public void RemoveItemAt(int slot)
    {
        items.RemoveAt(slot);
    }
}
