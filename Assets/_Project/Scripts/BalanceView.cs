using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 잔고 동전 + "7/10". 플레이어용·광대용 두 개를 Scene에 두고 isPlayer로 구분한다. 읽기만 한다.
// art-hd 35: 동전은 테이블 위 탑 2개. coins[0~4] = 왼쪽 탑 아래→위, coins[5~9] = 오른쪽 탑 아래→위.
//            그래서 잔고가 줄면 오른쪽 탑 맨 위부터 사라진다(배열 뒤쪽부터 꺼지므로).
public class BalanceView : MonoBehaviour
{
    [SerializeField] bool isPlayer;
    [SerializeField] Image[] coins;              // 탑 순서 10개 (위 설명)
    [SerializeField] TMP_Text numberText;        // "7/10"
    [SerializeField] Sprite coinOn;
    [SerializeField] Sprite coinDanger;          // 내 잔고가 위험할 때(빨강)
    [SerializeField] Sprite coinOff;             // 빈 탑 바닥 자리(점선 고리) — 탑 맨 아래 칸에만 보임
    [SerializeField] int towerSize = 5;          // 탑 하나에 쌓는 동전 수

    [Header("긴장 연출 (12주차, Ticket 22)")]
    [SerializeField] int dangerBalance = 3;
    [SerializeField] Color normalColor = new Color32(0x77, 0x73, 0x6B, 255);   // 팔레트 gray1
    [SerializeField] Color dangerColor = new Color32(0xC4, 0x3A, 0x3A, 255);   // 팔레트 red3

    [Header("동전 떨어짐 (ui-revamp 33)")]
    [SerializeField] float dropSeconds = 0.3f;
    [SerializeField] int dropDots = 8;
    [SerializeField] float dropSide = 6f;        // art-hd 35: 탑 위에서 옆으로 미끄러지며 떨어짐(탑을 뚫고 내려가 보이지 않게)
    Participant lastWho;       // 라운드가 바뀌면 다른 사람(새 Participant)이라 떨어짐을 보이지 않는다
    int lastBalance;

    public void Refresh(GameManager game)
    {
        Participant who = isPlayer ? game.Player : game.Clown;

        // 같은 사람의 잔고가 줄었으면, 줄어든 칸의 동전이 그 자리에서 떨어진다(새 동전 그림을 잠깐 복사해 아래로)
        if (who == lastWho && who.Balance < lastBalance)
        {
            bool wasDanger = isPlayer && lastBalance <= dangerBalance;   // 떨어지는 동전은 잃기 전 색 그대로
            for (int i = who.Balance; i < lastBalance && i < coins.Length; i++) StartCoroutine(DropCoin(coins[i], wasDanger ? coinDanger : coinOn));
        }
        lastWho = who;
        lastBalance = who.Balance;
        // 위험 표시는 내 잔고에만 (긴장은 플레이어가 느끼는 것)
        bool danger = isPlayer && who.Balance <= dangerBalance;

        for (int i = 0; i < coins.Length; i++)
        {
            bool have = i < who.Balance;
            bool towerBottom = i % towerSize == 0;   // 탑 맨 아래 칸: 비면 점선 고리로 "자리"를 남긴다
            // 시작 잔고보다 많은 칸은 숨긴다 (잔고 표가 10보다 작아져도 맞게). 잃은 동전은 탑 맨 아래 칸만 고리로 보임
            coins[i].enabled = i < who.StartingBalance && (have || towerBottom);
            coins[i].sprite = have ? (danger ? coinDanger : coinOn) : coinOff;
        }

        string halt = who.IsHalted ? " 정지" : "";
        numberText.text = $"{who.Balance}/{who.StartingBalance}{halt}";
        numberText.color = danger ? dangerColor : normalColor;
    }

    // 원래 칸은 이미 꺼졌거나 빈 고리 → 그 위에 동전 복사본을 만들어 옆·아래로 떨어뜨리고 지운다
    IEnumerator DropCoin(Image slot, Sprite look)
    {
        Image falling = Instantiate(slot, slot.transform.parent);
        falling.sprite = look;
        falling.enabled = true;                    // 원래 칸이 꺼져 있으면 복사본도 꺼진 채 만들어지므로 켠다
        RectTransform rt = falling.rectTransform;
        Vector2 from = slot.rectTransform.anchoredPosition;
        yield return Tween.MoveAnchored(rt, from, from + new Vector2(dropSide, -dropDots), dropSeconds, Tween.EaseInCubic);
        Destroy(falling.gameObject);
    }
}
