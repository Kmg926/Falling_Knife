using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 일시정지 패널: 멈춘 동안 켜지고, 계속 / 타이틀 버튼. 판정은 하지 않는다(GameManager 함수만 부른다).
// title-menu 59: 마우스 클릭이 버튼에 닿지 않는다는 보고 → EventSystem을 거치지 않고
// 타이틀 메뉴처럼 "마우스 위치가 버튼 영역 안인가"를 직접 확인해 실행한다(버튼 onClick도 그대로 둠 — 둘 중 먼저 오는 쪽이 실행).
public class PausePanel : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] GameObject root;     // 어둡게 + 패널 전체
    [SerializeField] Button resumeButton;
    [SerializeField] Button titleButton;

    Camera uiCamera;   // HUD Canvas가 Screen Space Camera라 마우스 위치를 이 카메라로 계산

    void Awake()
    {
        resumeButton.onClick.AddListener(() => game.Resume());
        titleButton.onClick.AddListener(() => game.GoToTitle());
        Canvas canvas = GetComponentInParent<Canvas>();
        uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
    }

    public void Refresh(GameManager game)
    {
        root.SetActive(game.IsPaused);
    }

    bool wasDown;   // 지난 프레임에 왼쪽 버튼이 눌려 있었나 → "지금 눌림 + 전엔 안 눌림" = 막 누른 순간

    // 멈춘 동안(timeScale 0이어도 Update는 돈다) 왼쪽 버튼을 막 누른 위치가 버튼 영역 안이면 바로 실행
    void Update()
    {
        Mouse mouse = Mouse.current;
        bool down = mouse != null && mouse.leftButton.isPressed;
        bool justPressed = down && !wasDown;
        wasDown = down;
        if (game == null || !game.IsPaused || !justPressed) return;
        Vector2 pos = mouse.position.ReadValue();
        if (IsOver(resumeButton, pos)) game.Resume();
        else if (IsOver(titleButton, pos)) game.GoToTitle();
    }

    bool IsOver(Button button, Vector2 screenPos) =>
        button != null && button.isActiveAndEnabled &&
        RectTransformUtility.RectangleContainsScreenPoint((RectTransform)button.transform, screenPos, uiCamera);
}
