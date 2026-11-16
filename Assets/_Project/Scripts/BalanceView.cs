using System.Collections;
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

    [Header("동전 떨어짐 (ui-revamp 33)")]
    [SerializeField] float dropSeconds = 0.3f;
    [SerializeField] int dropDots = 8;
    Participant lastWho;       // 라운드가 바뀌면 다른 사람(새 Participant)이라 떨어짐을 보이지 않는다
    int lastBalance;

    public void Refresh(GameManager game)
    {
        Participant who = isPlayer ? game.Player : game.Clown;

        // 같은 사람의 잔고가 줄었으면, 줄어든 칸의 동전이 그 자리에서 떨어진다(새 동전 그림을 잠깐 복사해 아래로)
        if (who == lastWho && who.Balance < lastBalance)
        {
            for (int i = who.Balance; i < lastBalance && i < coins.Length; i++) StartCoroutine(DropCoin(coins[i]));
        }
        lastWho = who;
        lastBalance = who.Balance;
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

    // 원래 칸은 이미 빈 동전 그림 → 그 위에 금색 동전 복사본을 만들어 아래로 떨어뜨리고 지운다
    IEnumerator DropCoin(Image slot)
    {
        Image falling = Instantiate(slot, slot.transform.parent);
        falling.sprite = coinOn;
        RectTransform rt = falling.rectTransform;
        Vector2 from = slot.rectTransform.anchoredPosition;
        yield return Tween.MoveAnchored(rt, from, from + new Vector2(0f, -dropDots), dropSeconds, Tween.EaseInCubic);
        Destroy(falling.gameObject);
    }
}
