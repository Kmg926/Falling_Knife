using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 일시정지 패널: 멈춘 동안 켜지고, 계속 / 타이틀 버튼. 판정은 하지 않는다(GameManager 함수만 부른다).
// title-menu 59: 마우스 클릭이 버튼에 닿지 않는다는 보고 → EventSystem을 거치지 않고
// 타이틀 메뉴처럼 "마우스 위치가 버튼 영역 안인가"를 직접 확인해 실행한다(버튼 onClick도 그대로 둠 — 둘 중 먼저 오는 쪽이 실행).
// title-menu 60: 마우스를 올린 버튼을 강조(금색 글 + "▶", 다른 버튼은 흐리게) — 무엇을 누를지 분명하게.
public class PausePanel : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] GameObject root;     // 어둡게 + 패널 전체
    [SerializeField] Button resumeButton;
    [SerializeField] Button titleButton;

    [Header("마우스 올림 강조 (title-menu 60)")]
    [SerializeField] Color highlightText = new Color32(0xD9, 0xB4, 0x5A, 255);   // gold2 (타이틀 메뉴와 같음)
    [SerializeField] Color normalText = new Color32(0xA3, 0x9A, 0x8C, 255);      // cream0
    [SerializeField] Color dimButton = new Color(0.6f, 0.6f, 0.6f, 1f);          // 강조 안 된 버튼 그림(곱하는 색)

    Camera uiCamera;   // HUD Canvas가 Screen Space Camera라 마우스 위치를 이 카메라로 계산
    Button[] buttons;
    TMP_Text[] labels;
    string[] labelTexts;
    int highlighted = -1;   // 강조된 버튼(0 = 계속, 1 = 타이틀)
    bool wasPaused;

    void Awake()
    {
        resumeButton.onClick.AddListener(() => game.Resume());
        titleButton.onClick.AddListener(() => game.GoToTitle());
        Canvas canvas = GetComponentInParent<Canvas>();
        uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

        buttons = new[] { resumeButton, titleButton };
        labels = new TMP_Text[buttons.Length];
        labelTexts = new string[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            labels[i] = buttons[i].GetComponentInChildren<TMP_Text>(true);
            labelTexts[i] = labels[i] != null ? labels[i].text : "";
        }
    }

    public void Refresh(GameManager game)
    {
        root.SetActive(game.IsPaused);
        // 막 멈췄을 때는 "계속"을 먼저 강조
        if (game.IsPaused && !wasPaused) SetHighlight(0);
        wasPaused = game.IsPaused;
    }

    bool wasDown;   // 지난 프레임에 왼쪽 버튼이 눌려 있었나 → "지금 눌림 + 전엔 안 눌림" = 막 누른 순간

    // 멈춘 동안(timeScale 0이어도 Update는 돈다): 마우스를 올린 버튼 강조, 왼쪽 버튼을 막 누른 위치가 버튼 안이면 실행
    void Update()
    {
        Mouse mouse = Mouse.current;
        bool down = mouse != null && mouse.leftButton.isPressed;
        bool justPressed = down && !wasDown;
        wasDown = down;
        if (game == null || !game.IsPaused || mouse == null) return;

        Vector2 pos = mouse.position.ReadValue();
        int over = IsOver(resumeButton, pos) ? 0 : IsOver(titleButton, pos) ? 1 : -1;
        if (over >= 0 && over != highlighted) SetHighlight(over);   // 버튼 밖으로 나가면 마지막 강조를 그대로 둠

        if (!justPressed) return;
        if (over == 0) game.Resume();
        else if (over == 1) game.GoToTitle();
    }

    // 강조: 고른 버튼은 금색 글 + "▶ " + 원래 밝기, 나머지는 흐린 글 + 어두운 그림
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
            if (buttons[i].targetGraphic != null) buttons[i].targetGraphic.color = on ? Color.white : dimButton;
        }
    }

    bool IsOver(Button button, Vector2 screenPos) =>
        button != null && button.isActiveAndEnabled &&
        RectTransformUtility.RectangleContainsScreenPoint((RectTransform)button.transform, screenPos, uiCamera);
}
