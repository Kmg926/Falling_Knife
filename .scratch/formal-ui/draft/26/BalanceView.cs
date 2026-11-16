using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 잔고 동전 + "7/10". 플레이어용·광대용 두 개를 Scene에 두고 isPlayer로 구분한다. 읽기만 한다.
public class BalanceView : MonoBehaviour
{
    [SerializeField] bool isPlayer;
    [SerializeField] Image[] coins;              // 왼쪽부터 10개
    [SerializeField] TMP_Text numberText;        // "7/10"
    [SerializeField] Sprite coinOn;
    [SerializeField] Sprite coinDanger;          // 내 잔고가 위험할 때(빨강)
    [SerializeField] Sprite coinOff;             // 잃은 잔고(회색 점선)

    [Header("긴장 연출 (12주차, Ticket 22)")]
    [SerializeField] int dangerBalance = 3;
    [SerializeField] Color normalColor = new Color32(0x77, 0x73, 0x6B, 255);   // 팔레트 gray1
    [SerializeField] Color dangerColor = new Color32(0xC4, 0x3A, 0x3A, 255);   // 팔레트 red3

    public void Refresh(GameManager game)
    {
        Participant who = isPlayer ? game.Player : game.Clown;
        // 위험 표시는 내 잔고에만 (긴장은 플레이어가 느끼는 것)
        bool danger = isPlayer && who.Balance <= dangerBalance;

        for (int i = 0; i < coins.Length; i++)
        {
            // 시작 잔고보다 많은 칸은 숨긴다 (잔고 표가 10보다 작아져도 맞게)
            coins[i].enabled = i < who.StartingBalance;
            if (i < who.Balance) coins[i].sprite = danger ? coinDanger : coinOn;
            else coins[i].sprite = coinOff;
        }

        string halt = who.IsHalted ? " 정지" : "";
        numberText.text = $"{who.Balance}/{who.StartingBalance}{halt}";
        numberText.color = danger ? dangerColor : normalColor;
    }
}
