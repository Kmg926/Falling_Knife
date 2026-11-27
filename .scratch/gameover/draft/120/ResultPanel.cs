using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
    [SerializeField] Button titleButton;          // TitleScene으로 (Ticket 29)

    [Header("gameover 120: 패배 화면(간소화) — 검은 화면 + GAME OVER + 플레이 타임 + 글자 버튼 2개. 승리는 위 화면 그대로")]
    [SerializeField] GameObject gameOverRoot;     // 패배 때만 켬(위 root 대신)
    [SerializeField] TMP_Text playTimeText;       // "플레이 타임 3:12"
    [SerializeField] Button gameOverRetry;
    [SerializeField] Button gameOverTitle;

    [Header("마우스 올림 강조 (title-menu 94, 일시정지 메뉴 60과 같음)")]
    [SerializeField] Color highlightText = new Color32(0xD9, 0xB4, 0x5A, 255);   // gold2
    [SerializeField] Color normalText = new Color32(0xA3, 0x9A, 0x8C, 255);      // cream0
    [SerializeField] Color dimButton = new Color(0.6f, 0.6f, 0.6f, 1f);          // 강조 안 된 버튼 그림(곱하는 색)

    Camera uiCamera;
    Button[] buttons;      // 지금 화면의 버튼(0 = 다시하기, 1 = 타이틀) — 승리 화면 것 또는 패배 화면 것
    TMP_Text[] labels;
    string[] labelTexts;
    int highlighted = -1;
    bool wasOver;          // 지난 Refresh 때 결과 화면이 켜져 있었나
    bool wasSurvived;      // 120: 켜질 때 어느 화면(버튼 묶음)이었나

    void Awake()
    {
        retryButton.onClick.AddListener(() => game.Restart());
        titleButton.onClick.AddListener(() => game.GoToTitle());
        if (gameOverRetry != null) gameOverRetry.onClick.AddListener(() => game.Restart());   // 120
        if (gameOverTitle != null) gameOverTitle.onClick.AddListener(() => game.GoToTitle());
        Canvas canvas = GetComponentInParent<Canvas>();
        uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        UseButtons(retryButton, titleButton);
    }

    // 120: 강조할 버튼 묶음을 고름(승리 화면 / 패배 화면). 글자 원문은 처음 한 번 기억(▶를 붙였다 떼려고)
    void UseButtons(Button retry, Button title)
    {
        buttons = new[] { retry, title };
        labels = new TMP_Text[buttons.Length];
        labelTexts = new string[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            labels[i] = buttons[i].GetComponentInChildren<TMP_Text>(true);
            labelTexts[i] = labels[i] != null ? labels[i].text.Replace("▶ ", "") : "";
        }
    }

    // 94: 결과 화면이 켜져 있는 동안 마우스를 올린 버튼을 강조. 버튼 밖으로 나가면 마지막 강조를 그대로 둠(일시정지와 같음)
    void Update()
    {
        if (game == null || !game.IsGameOver || game.IsTutorial || Mouse.current == null) return;
        Vector2 pos = Mouse.current.position.ReadValue();
        for (int i = 0; i < buttons.Length; i++)
            if (i != highlighted && IsOver(buttons[i], pos)) SetHighlight(i);
    }

    // 고른 버튼은 금색 글 + "▶ " + 원래 밝기, 나머지는 흐린 글 + 어두운 그림
    void SetHighlight(int index)
    {
        highlighted = index;
        for (int i = 0; i < buttons.Length; i++)
        {
            bool on = i == index;
            if (labels[i] != null)
            {
                labels[i].text = (on ? "▶ " : "") + labelTexts[i];
                labels[i].color = on ? highlightText : normalText;
            }
            // 120: 그림 있는 버튼(승리 화면)만 밝기를 바꿈 — 패배 화면의 글자 버튼은 투명한 클릭 칸이라 색을 칠하면 흰 사각형이 됨
            if (buttons[i].targetGraphic is Image g && g.sprite != null) g.color = on ? Color.white : dimButton;
        }
    }

    bool IsOver(Button button, Vector2 screenPos) =>
        button != null && button.isActiveAndEnabled &&
        RectTransformUtility.RectangleContainsScreenPoint((RectTransform)button.transform, screenPos, uiCamera);

    public void Refresh(GameManager game)
    {
        bool over = game.IsGameOver && !game.IsTutorial;   // tutorial 97: 튜토리얼 끝은 결과 화면 대신 광대 대사
        bool survived = over && game.IsFinalVictory;
        // gameover 120(사용자 "게임 오버 창… 최대한 간소화, 플레이 타임은 넣고" → 패배만): 패배는 간소화 화면, 승리는 원래 화면
        bool simple = over && !survived && gameOverRoot != null;
        root.SetActive(over && !simple);
        if (gameOverRoot != null) gameOverRoot.SetActive(simple);
        if (over && (!wasOver || survived != wasSurvived))
        {
            if (simple) UseButtons(gameOverRetry, gameOverTitle);
            else UseButtons(retryButton, titleButton);
            SetHighlight(0);   // 94: 막 켜졌을 때는 "다시하기" 강조
        }
        wasOver = over;
        wasSurvived = survived;
        if (!over) return;

        if (simple)
        {
            playTimeText.text = $"플레이 타임 {Clock(game.PlaySeconds)}";
            return;
        }

        background.sprite = survived ? backgroundSurvive : backgroundBankrupt;
        box.sprite = survived ? boxSurvive : boxBankrupt;
        clown.sprite = survived ? clownDark : clownIdle;

        titleText.text = survived ? "생존" : "파산";
        titleText.color = survived ? surviveColor : bankruptColor;
        reasonText.text = survived ? "광대의 잔고가 0이 되었다" : "내 잔고가 0이 되었다";
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
