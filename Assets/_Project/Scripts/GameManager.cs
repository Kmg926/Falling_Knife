using System.Collections;
using UnityEngine;

// 차례인 사람이 고를 수 있는 행동
public enum PlayerAction { Hold, Sell }

public class GameManager : MonoBehaviour
{
    [SerializeField] CandleDeck deck;
    [SerializeField] UIManager ui;

    [Header("라운드 표 (잔고 / 하락 / 상승 / 고정 순서)")]
    [SerializeField] RoundConfig[] rounds =
    {
        new RoundConfig { startingBalance = 2, downCount = 2, upCount = 3 },
        new RoundConfig { startingBalance = 3, downCount = 3, upCount = 4 },
        new RoundConfig { startingBalance = 4, downCount = 4, upCount = 4 },
    };

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

    // 지금 라운드 (1부터). StartRound 밖에서만 바꾼다.
    public int RoundNumber { get; private set; }
    public int RoundCount => rounds.Length;
    public bool IsLastRound => RoundNumber == RoundCount;

    // 둘 중 하나라도 파산하면 라운드 끝 (클리어·최종 승리·패배 모두)
    public bool IsRoundOver => Player.IsBankrupt || Clown.IsBankrupt;
    // 광대 파산 + 마지막 라운드가 아님 → Space로 다음 라운드
    public bool IsRoundCleared => Clown.IsBankrupt && !IsLastRound;
    // 광대 파산 + 마지막 라운드 → 최종 승리
    public bool IsFinalVictory => Clown.IsBankrupt && IsLastRound;

    // 지금 차례가 아닌 쪽
    Participant Opponent => (Current == Player) ? Clown : Player;

    void Start()
    {
        RoundNumber = 1;
        StartRound();
    }

    // RoundNumber 줄의 값으로 라운드를 새로 시작한다. 라운드 번호는 여기서 바꾸지 않는다.
    void StartRound()
    {
        RoundConfig config = rounds[RoundNumber - 1];

        Player = new Participant("플레이어", config.startingBalance);
        Clown = new Participant("광대", config.startingBalance);
        Leverage = new LeverageCounter();
        Current = Player;
        Selected = PlayerAction.Hold;
        IsRevealing = false;
        LastResult = "";
        deck.SetComposition(config.downCount, config.upCount, config.fixedOrder);
        deck.Fill();
        ui.Refresh(this);
    }

    // A/D·←/→. 공개 중이거나 라운드가 끝났으면 무시한다.
    public void Select(PlayerAction action)
    {
        if (IsRevealing || IsRoundOver) return;
        Selected = action;
        ui.Refresh(this);
    }

    // Space. 공개 중이면 무시. 라운드 클리어 화면이면 다음 라운드. 최종 승리·패배면 무시.
    public void Execute()
    {
        if (IsRevealing) return;

        if (IsRoundCleared)
        {
            NextRound();
            return;
        }
        if (IsRoundOver) return;

        // 안전장치: 파산하지 않았다면 재장전되므로 여기까지 올 일이 없다.
        if (deck.IsEmpty) return;

        StartCoroutine(RevealRoutine(Selected));
    }

    // 다음 줄로 라운드 번호를 올리고 새로 시작한다.
    void NextRound()
    {
        RoundNumber++;
        StartRound();
    }

    // R. 라운드가 끝난 화면에서만 받는다. 항상 1라운드부터. (테스트용, 최종 빌드 전 제거)
    public void Restart()
    {
        if (IsRevealing || !IsRoundOver) return;
        RoundNumber = 1;
        StartRound();
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

        if (!IsRoundOver && deck.IsEmpty)
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