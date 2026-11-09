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

        if (game.IsRoundOver)
        {
            message += game.PlayerWon ? "\n승리! - R로 재시작" : "\n패배... - R로 재시작";
        }

        statusText.text =
            $"차례: {game.Current.Name}\n" +
            $"선택: {choice}\n" +
            $"잔고  플레이어 {game.Player.Balance} / 광대 {game.Clown.Balance}\n" +
            $"레버리지 {game.Leverage.Value}\n" +
            $"남은 캔들  하락 {game.Deck.RemainingDown} / 상승 {game.Deck.RemainingUp}\n" +
            $"{message}";
    }
}
