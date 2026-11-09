using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] TMP_Text statusText;

    // 상태를 읽어서 글자로만 보여준다. 값을 바꾸지 않는다.
    public void Refresh(GameManager game)
    {
        statusText.text =
            $"차례: {game.Current.Name}\n" +
            $"잔고  플레이어 {game.Player.Balance} / 광대 {game.Clown.Balance}\n" +
            $"레버리지 {game.Leverage.Value}\n" +
            $"남은 캔들  하락 {game.Deck.RemainingDown} / 상승 {game.Deck.RemainingUp}";
    }
}
