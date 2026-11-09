using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] TMP_Text statusText;

    // 상태를 읽어서 글자로만 보여준다. 값을 바꾸지 않는다.
    public void Refresh(GameManager game)
    {
        string choice = game.Selected == PlayerAction.Hold ? "[보유]  매도" : " 보유  [매도]";
        string message = game.IsRevealing ? "공개 중..." : game.LastResult;

        // 결과 줄을 덮지 않고 덧붙인다: 내 직전 결과를 읽을 수 있게.
        if (game.IsClownThinking)
        {
            message += "\n광대 생각 중...";
        }

        // 차트 분석으로 아는 다음 캔들 (플레이어만). 그 캔들이 나오면 GameManager가 지운다.
        if (game.PlayerKnownNext.HasValue)
        {
            string next = game.PlayerKnownNext.Value == Candle.Down ? "하락" : "상승";
            message += $"\n차트 분석: 다음 캔들은 {next}";
        }

        // 아이템 확인 중: 이름·설명·조작 안내
        if (game.IsConfirming)
        {
            ItemType item = game.Player.Items[game.ConfirmingSlot];
            message += $"\n확인: {game.ItemName(item)} — {game.ItemDescription(item)} / Space 사용, Esc 취소";

            // 거래 정지는 상대가 이미 정지면 쓸 수 없다고 미리 알려 준다.
            if (item == ItemType.TradingHalt && game.Clown.IsHalted)
            {
                message += " (사용 불가: 상대가 이미 정지 상태)";
            }
        }

        if (game.IsRoundCleared)
        {
            message += $"\n라운드 {game.RoundNumber} 클리어! - Space로 다음 라운드";
        }
        else if (game.IsFinalVictory)
        {
            message += "\n최종 승리! - R로 처음부터";
        }
        else if (game.IsRoundOver)
        {
            message += "\n패배... - R로 처음부터";
        }

        statusText.text =
            $"라운드 {game.RoundNumber} / {game.RoundCount}\n" +
            $"차례: {game.Current.Name}\n" +
            $"선택: {choice}\n" +
            $"잔고  플레이어 {game.Player.Balance}{HaltMark(game.Player)} / 광대 {game.Clown.Balance}{HaltMark(game.Clown)}\n" +
            $"레버리지 {game.Leverage.Value}\n" +
            $"남은 캔들  하락 {game.Deck.RemainingDown} / 상승 {game.Deck.RemainingUp}\n" +
            $"아이템  나: {ItemList(game, game.Player, true)} / 광대: {ItemList(game, game.Clown, false)}\n" +
            $"{message}";
    }

    // 거래 정지 상태면 잔고 옆에 붙일 표시.
    string HaltMark(Participant who)
    {
        return who.IsHalted ? " (정지)" : "";
    }

    // 아이템 이름을 쉼표로 잇는다. 내 것은 키 번호(1~4)를 앞에 붙인다. 없으면 "-".
    string ItemList(GameManager game, Participant who, bool withKeys)
    {
        if (who.Items.Count == 0) return "-";

        List<string> names = new List<string>();
        for (int i = 0; i < who.Items.Count; i++)
        {
            string name = game.ItemName(who.Items[i]);
            names.Add(withKeys ? $"{i + 1} {name}" : name);
        }
        return string.Join(", ", names);
    }
}
