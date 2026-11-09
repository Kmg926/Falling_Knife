using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] CandleDeck deck;
    [SerializeField] UIManager ui;

    [Header("라운드 구성")]
    [SerializeField] int startingBalance = 2;

    public Participant Player { get; private set; }
    public Participant Clown { get; private set; }
    public Participant Current { get; private set; }
    public LeverageCounter Leverage { get; private set; }
    public CandleDeck Deck => deck;

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
        deck.Fill();
        ui.Refresh(this);
    }
}
