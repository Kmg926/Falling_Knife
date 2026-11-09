using System.Collections;
using UnityEngine;

// 차례인 사람이 고를 수 있는 행동
public enum PlayerAction { Hold, Sell }

public class GameManager : MonoBehaviour
{
    [SerializeField] CandleDeck deck;
    [SerializeField] UIManager ui;

    [Header("라운드 구성")]
    [SerializeField] int startingBalance = 2;

    [Header("연출")]
    [SerializeField] float revealSeconds = 1f;

    public Participant Player { get; private set; }
    public Participant Clown { get; private set; }
    public Participant Current { get; private set; }
    public LeverageCounter Leverage { get; private set; }
    public CandleDeck Deck => deck;

    public PlayerAction Selected { get; private set; }
    public bool IsRevealing { get; private set; }
    public string LastResult { get; private set; }

    // 지금 차례가 아닌 쪽
    Participant Opponent => (Current == Player) ? Clown : Player;

    // 둘 중 하나라도 잔고 0
    bool SomeoneBankrupt => Player.IsBankrupt || Clown.IsBankrupt;

    void Start()
    {
        StartRound();
    }

    // R 재시작(05)도 이 함수를 다시 부른다.
    void StartRound()
    {
        Player = new Participant("플레이어", startingBalance);
        Clown = new Participant("광대", startingBalance);
        Leverage = new LeverageCounter();
        Current = Player;
        Selected = PlayerAction.Hold;
        IsRevealing = false;
        LastResult = "";
        deck.Fill();
        ui.Refresh(this);
    }

    // A/D·←/→. 공개 중에는 무시한다.
    public void Select(PlayerAction action)
    {
        if (IsRevealing) return;
        Selected = action;
        ui.Refresh(this);
    }

    // Space. 공개 중에는 무시한다.
    public void Execute()
    {
        if (IsRevealing) return;

        // 04부터는 파산으로 재장전을 건너뛴 경우에만 덱이 비어 있다. 승패 처리는 05.
        if (deck.IsEmpty)
        {
            Debug.Log("덱이 비어 실행할 수 없습니다. (파산 후 승패 처리는 Ticket 05)");
            return;
        }

        StartCoroutine(RevealRoutine(Selected));
    }

    // "공개 중..." 1초 → 캔들 공개 → 고른 행동대로 결과 적용
    IEnumerator RevealRoutine(PlayerAction action)
    {
        IsRevealing = true;
        ui.Refresh(this);

        yield return new WaitForSeconds(revealSeconds);

        Candle candle = deck.Draw();

        // 처리 순서: ① 피해 적용 → ② 잔고 0 판정 → ③ 덱 소진 확인
        if (action == PlayerAction.Hold) ResolveHold(candle);
        else ResolveSell(candle);

        if (!SomeoneBankrupt && deck.IsEmpty)
        {
            Reload();
        }

        IsRevealing = false;
        ui.Refresh(this);
    }

    void ResolveHold(Candle candle)
    {
        if (candle == Candle.Up)
        {
            Leverage.Increase();
            LastResult = $"보유 → 상승! {Current.Name} 레버리지 {Leverage.Value}, 차례 유지";
        }
        else
        {
            int damage = 1 + Leverage.Value;
            Current.TakeDamage(damage);
            Leverage.Reset();
            LastResult = $"보유 → 하락! {Current.Name} -{damage}";
            PassTurn();
        }
    }

    // 매도: 하락이면 상대가 맞는다. 어느 쪽이든 차례는 넘어간다.
    void ResolveSell(Candle candle)
    {
        if (candle == Candle.Down)
        {
            int damage = 1 + Leverage.Value;
            Opponent.TakeDamage(damage);
            Leverage.Reset();
            LastResult = $"매도 → 하락! {Opponent.Name} -{damage}";
        }
        else
        {
            LastResult = $"매도 → 상승! 변화 없음 (레버리지 {Leverage.Value} 그대로)";
        }
        PassTurn();
    }

    // 같은 구성으로 다시 섞어 채운다. 잔고·레버리지·차례는 그대로. (아이템 지급은 ②)
    void Reload()
    {
        deck.Fill();
        LastResult += $"\n덱 재장전 (하락{deck.RemainingDown} / 상승{deck.RemainingUp})";
    }

    // 핫시트: 상대에게 차례를 넘기고 선택은 보유로 되돌린다.
    void PassTurn()
    {
        Current = Opponent;
        Selected = PlayerAction.Hold;
    }
}
