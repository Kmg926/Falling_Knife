using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 일시정지 패널: 멈춘 동안 켜지고, 계속 / 설정 / 타이틀 버튼. 판정은 하지 않는다(GameManager 함수만 부른다).
// title-menu 59: 마우스 클릭이 버튼에 닿지 않는다는 보고 → EventSystem을 거치지 않고
// 타이틀 메뉴처럼 "마우스 위치가 버튼 영역 안인가"를 직접 확인해 실행한다(버튼 onClick도 그대로 둠 — 둘 중 먼저 오는 쪽이 실행).
// title-menu 60: 마우스를 올린 버튼을 강조(금색 글 + "▶", 다른 버튼은 흐리게) — 무엇을 누를지 분명하게.
// game-settings 91: "설정" 버튼·설정 창(공통 SettingsPanel)·왼쪽 위 톱니 버튼, ↑/↓·W/S + Space·Enter로도 고름.
public class PausePanel : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] GameObject root;     // 어둡게 + 패널 전체
    [SerializeField] Button resumeButton;
    [SerializeField] Button titleButton;

    [Header("game-settings 91")]
    [SerializeField] Button settingsButton;      // 계속과 타이틀 사이
    [SerializeField] GameObject menuPanel;       // 버튼이 있는 창(설정을 열면 숨김)
    [SerializeField] GameObject settingsWindow;  // 설정 창(제목·4줄·아래 안내)
    [SerializeField] SettingsPanel settings;     // 설정 4줄(타이틀과 같은 부품)
    [SerializeField] Button gearButton;          // 왼쪽 위 톱니(멈추고 바로 설정)

    [Header("마우스 올림 강조 (title-menu 60)")]
    [SerializeField] Color highlightText = new Color32(0xD9, 0xB4, 0x5A, 255);   // gold2 (타이틀 메뉴와 같음)
    [SerializeField] Color normalText = new Color32(0xA3, 0x9A, 0x8C, 255);      // cream0
    [SerializeField] Color dimButton = new Color(0.6f, 0.6f, 0.6f, 1f);          // 강조 안 된 버튼 그림(곱하는 색)

    Camera uiCamera;   // HUD Canvas가 Screen Space Camera라 마우스 위치를 이 카메라로 계산
    Button[] buttons;
    TMP_Text[] labels;
    string[] labelTexts;
    int highlighted = -1;   // 강조된 버튼(0 = 계속, 1 = 설정, 2 = 타이틀)
    bool wasPaused;
    bool wasSettings;

    void Awake()
    {
        resumeButton.onClick.AddListener(() => game.Resume());
        titleButton.onClick.AddListener(() => game.GoToTitle());
        if (settingsButton != null) settingsButton.onClick.AddListener(() => game.OpenSettings(false));
        if (gearButton != null) gearButton.onClick.AddListener(() => game.OpenSettings(true));
        Canvas canvas = GetComponentInParent<Canvas>();
        uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;

        buttons = settingsButton != null ? new[] { resumeButton, settingsButton, titleButton } : new[] { resumeButton, titleButton };
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
        bool settingsOpen = game.IsSettingsOpen;
        if (menuPanel != null) menuPanel.SetActive(!settingsOpen);
        if (settingsWindow != null) settingsWindow.SetActive(settingsOpen);
        if (settingsOpen && !wasSettings && settings != null) settings.Begin();     // 막 열렸을 때 첫 줄부터
        // 막 멈췄을 때는 "계속"을, 설정에서 돌아왔을 때는 "설정"을 강조
        if (game.IsPaused && !wasPaused) SetHighlight(0);
        else if (!settingsOpen && wasSettings && game.IsPaused) SetHighlight(1);
        wasPaused = game.IsPaused;
        wasSettings = settingsOpen;
        if (gearButton != null) gearButton.gameObject.SetActive(game.CanOpenGear);   // 멈춤·결과 화면·시작 스토리·열린 화면 중엔 숨김
    }

    bool wasDown;   // 지난 프레임에 왼쪽 버튼이 눌려 있었나 → "지금 눌림 + 전엔 안 눌림" = 막 누른 순간

    // 멈춘 동안(timeScale 0이어도 Update는 돈다): 마우스를 올린 버튼 강조, 왼쪽 버튼을 막 누른 위치가 버튼 안이면 실행
    void Update()
    {
        Mouse mouse = Mouse.current;
        Keyboard kb = Keyboard.current;
        bool down = mouse != null && mouse.leftButton.isPressed;
        bool justPressed = down && !wasDown;
        wasDown = down;
        if (game == null) return;

        // 91: 톱니(멈추지 않은 때) — 직접 위치 확인(title-menu 59와 같은 이유)
        if (!game.IsPaused)
        {
            if (justPressed && gearButton != null && game.CanOpenGear && IsOver(gearButton, mouse.position.ReadValue())) game.OpenSettings(true);
            return;
        }

        // 91: 설정 창이 열려 있으면 그 창이 입력을 받음(Esc 닫기는 PlayerInputHandler → TogglePause → CloseSettings)
        if (game.IsSettingsOpen)
        {
            if (settings != null)
            {
                settings.HandleInput(kb, mouse);
                if (settings.CloseClicked) game.CloseSettings();   // 95: X = Esc와 같은 닫기
            }
            return;
        }

        // 91: 키보드 ↑/↓·W/S로 고르고 Space·Enter로 실행(타이틀 메뉴와 같은 조작)
        if (kb != null)
        {
            if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) SetHighlight((highlighted - 1 + buttons.Length) % buttons.Length);
            else if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) SetHighlight((highlighted + 1) % buttons.Length);
            else if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame) { Activate(highlighted); return; }
        }

        if (mouse == null) return;
        Vector2 pos = mouse.position.ReadValue();
        int over = -1;
        for (int i = 0; i < buttons.Length; i++) if (IsOver(buttons[i], pos)) over = i;
        if (over >= 0 && over != highlighted) SetHighlight(over);   // 버튼 밖으로 나가면 마지막 강조를 그대로 둠
        if (justPressed && over >= 0) Activate(over);
    }

    // 고른 버튼 실행(버튼 순서: 계속 · 설정 · 타이틀)
    public void Activate(int index)
    {
        if (index < 0 || index >= buttons.Length) return;
        Button b = buttons[index];
        if (b == resumeButton) game.Resume();
        else if (b == settingsButton) game.OpenSettings(false);
        else if (b == titleButton) game.GoToTitle();
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
