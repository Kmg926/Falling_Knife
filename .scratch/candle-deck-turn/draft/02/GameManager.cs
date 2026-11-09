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

        if (Selected == PlayerAction.Sell)
        {
            Debug.Log("매도 실행은 Ticket 03에서 구현합니다.");
            return;
        }
        if (deck.IsEmpty)
        {
            Debug.Log("덱이 비었습니다. 재장전은 Ticket 04에서 구현합니다.");
            return;
        }

        StartCoroutine(RevealRoutine());
    }

    // "공개 중..." 1초 → 캔들 공개 → 결과 적용
    IEnumerator RevealRoutine()
    {
        IsRevealing = true;
        ui.Refresh(this);

        yield return new WaitForSeconds(revealSeconds);

        Candle candle = deck.Draw();
        ResolveHold(candle);

        IsRevealing = false;
        ui.Refresh(this);
    }

    void ResolveHold(Candle candle)
    {
        if (candle == Candle.Up)
        {
            Leverage.Increase();
            LastResult = $"상승! {Current.Name} 레버리지 {Leverage.Value}, 차례 유지";
        }
        else
        {
            int damage = 1 + Leverage.Value;
            Current.TakeDamage(damage);
            Leverage.Reset();
            LastResult = $"하락! {Current.Name} -{damage}";
            PassTurn();
        }
    }

    // 핫시트: 상대에게 차례를 넘기고 선택은 보유로 되돌린다.
    void PassTurn()
    {
        Current = (Current == Player) ? Clown : Player;
        Selected = PlayerAction.Hold;
    }
}
