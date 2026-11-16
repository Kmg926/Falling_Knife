using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 결과 화면 (스케치 화면 4 파산 / 5 생존). 한 판이 끝나면 켜지고, 원인·결정타와 한 판 기록을 보여 준다.
// 판정은 하지 않는다: 다시하기 버튼은 GameManager.Restart()를 부를 뿐.
public class ResultPanel : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] GameObject root;             // 결과 화면 전체 (켜고 끄는 단위)

    [Header("그림 (파산 / 생존)")]
    [SerializeField] Image background;
    [SerializeField] Image box;
    [SerializeField] Image clown;
    [SerializeField] Sprite backgroundBankrupt;
    [SerializeField] Sprite backgroundSurvive;
    [SerializeField] Sprite boxBankrupt;
    [SerializeField] Sprite boxSurvive;
    [SerializeField] Sprite clownIdle;
    [SerializeField] Sprite clownDark;            // 생존: 조명이 꺼져 어둡게 남은 광대

    [Header("글자")]
    [SerializeField] TMP_Text titleText;          // 파산 / 생존
    [SerializeField] TMP_Text reasonText;         // 잔고가 0이 되었다
    [SerializeField] TMP_Text causeLabel;         // 원인 / 결정타
    [SerializeField] TMP_Text causeText;          // R2, 광대 매도, 하락, 피해 3 (레버리지 2)
    [SerializeField] TMP_Text statsText;          // 도달 라운드·최대 레버리지·아이템 사용·시간
    [SerializeField] Color bankruptColor = new Color32(0xA3, 0x28, 0x2B, 255);   // 팔레트 red2
    [SerializeField] Color surviveColor = new Color32(0xCD, 0xC4, 0xB6, 255);    // 팔레트 cream
    [SerializeField] Color causeBankruptColor = new Color32(0xC4, 0x3A, 0x3A, 255); // 팔레트 red3

    [Header("버튼")]
    [SerializeField] Button retryButton;
    [SerializeField] Button titleButton;          // TitleScene이 생기는 Ticket 29까지 꺼 둔다

    void Awake()
    {
        retryButton.onClick.AddListener(() => game.Restart());
        titleButton.interactable = false;
    }

    public void Refresh(GameManager game)
    {
        root.SetActive(game.IsGameOver);
        if (!game.IsGameOver) return;

        bool survived = game.IsFinalVictory;

        background.sprite = survived ? backgroundSurvive : backgroundBankrupt;
        box.sprite = survived ? boxSurvive : boxBankrupt;
        clown.sprite = survived ? clownDark : clownIdle;

        titleText.text = survived ? "생존" : "파산";
        titleText.color = survived ? surviveColor : bankruptColor;
        reasonText.text = survived ? "광대의 잔고가 0이 되었다" : "잔고가 0이 되었다";
        causeLabel.text = survived ? "결정타" : "원인";
        causeText.text = game.LastDamage;
        causeText.color = survived ? surviveColor : causeBankruptColor;

        // 생존 = 모든 라운드 클리어, 파산 = 쓰러진 라운드
        string reach = survived
            ? $"클리어 {game.RoundCount}/{game.RoundCount}"
            : $"도달 라운드 {game.RoundNumber}/{game.RoundCount}";
        statsText.text = $"{reach}   최대 레버리지 {game.MaxLeverage}\n" +
                         $"아이템 사용 {game.ItemUses}회   시간 {Clock(game.PlaySeconds)}";
    }

    // 초 → "3:12"  (:00 = 두 자리로 채움)
    static string Clock(float seconds)
    {
        int total = Mathf.FloorToInt(seconds);
        return $"{total / 60}:{total % 60:00}";
    }
}
