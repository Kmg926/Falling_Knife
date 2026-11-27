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

    // item-limit 116(사용자 2026-11-27 "거래 정지는 한 판에 한 번씩만, 체력템도 마찬가지" → 라운드마다·사람마다·회복템은 묶어서):
    // 이 라운드에 이미 받았나. Participant는 라운드마다 새로 만들어지므로 라운드가 바뀌면 저절로 false로 돌아간다.
    public bool GotHalt { get; private set; }
    public bool GotHeal { get; private set; }   // 배당금이든 구제 금융이든 하나라도

    // 받을 수 있는 종류인가(이번 라운드에 이미 받은 제한 아이템이면 false)
    public bool CanReceive(ItemType item)
    {
        if (item == ItemType.TradingHalt) return !GotHalt;
        if (item == ItemType.Dividend || item == ItemType.Bailout) return !GotHeal;
        return true;
    }

    // 실제로 받았을 때 표시(못 받으면 — 가득 차서 버려지면 — 부르지 않는다)
    public void MarkGiven(ItemType item)
    {
        if (item == ItemType.TradingHalt) GotHalt = true;
        if (item == ItemType.Dividend || item == ItemType.Bailout) GotHeal = true;
    }

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
