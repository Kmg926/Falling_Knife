using TMPro;
using UnityEngine;

// 오른쪽 위 "ROUND 1/3" + "내 턴/광대 턴", 가운데 안내(라운드 클리어·끝). 읽기만 한다.
public class RoundTurnView : MonoBehaviour
{
    [SerializeField] TMP_Text roundText;
    [SerializeField] TMP_Text turnText;
    [SerializeField] TMP_Text noticeText;
    [SerializeField] Color myTurnColor = new Color32(0x3F, 0x8F, 0x55, 255);     // 팔레트 green
    [SerializeField] Color clownTurnColor = new Color32(0xC4, 0x3A, 0x3A, 255);  // 팔레트 red3

    public void Refresh(GameManager game)
    {
        roundText.text = $"ROUND {game.RoundNumber}/{game.RoundCount}";

        bool mine = game.Current == game.Player;
        turnText.text = mine ? "내 턴" : "광대 턴";
        turnText.color = mine ? myTurnColor : clownTurnColor;

        // 결과 화면(Ticket 28)이 생기기 전까지는 끝난 상태를 여기서 안내한다.
        if (game.IsRoundCleared) noticeText.text = $"라운드 {game.RoundNumber} 클리어! Space로 다음 라운드";
        else if (game.IsFinalVictory) noticeText.text = "생존! R로 처음부터";
        else if (game.IsRoundOver) noticeText.text = "파산... R로 처음부터";
        else noticeText.text = "";
    }
}
