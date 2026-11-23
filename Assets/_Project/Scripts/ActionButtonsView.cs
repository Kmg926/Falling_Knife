using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 매수(나에게)·매도(광대에게) 버튼: 선택 표시, 못 누를 때 회색, 클릭하면 그 행동을 고르고 바로 실행.
// title-menu 94: 마우스를 올리면 그 버튼이 "선택"됨(밝은 그림 + 클릭음) — 타이틀 메뉴처럼 올린 곳 = 고른 것.
public class ActionButtonsView : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] Button holdButton;
    [SerializeField] Button sellButton;
    [SerializeField] Image holdImage;
    [SerializeField] Image sellImage;
    [SerializeField] Sprite normalSprite;
    [SerializeField] Sprite selectedSprite;
    [SerializeField] Sprite disabledSprite;

    // 버튼을 누르면 부를 함수를 한 번만 등록한다.
    [Header("눌림 (ui-revamp 33)")]
    [SerializeField] float pressSeconds = 0.1f;
    Vector2 holdHome;
    Vector2 sellHome;
    bool wasRevealing;

    Camera uiCamera;   // 94: HUD가 Screen Space Camera라 마우스 위치를 이 카메라로 계산

    void Awake()
    {
        holdHome = holdImage.rectTransform.anchoredPosition;
        sellHome = sellImage.rectTransform.anchoredPosition;
        holdButton.onClick.AddListener(() => OnClick(PlayerAction.Buy));
        sellButton.onClick.AddListener(() => OnClick(PlayerAction.Sell));
        Canvas canvas = GetComponentInParent<Canvas>();
        uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
    }

    // 94: 마우스를 움직여 버튼 위에 올리면 그 행동을 고름. 움직였을 때만(가만히 있는 마우스가 A/D로 고른 것을 덮어쓰지 않게 — 타이틀 메뉴와 같음).
    // 고르기는 GameManager.Select가 함 → 내 차례가 아니면 무시되고, 바뀔 때만 클릭음.
    void Update()
    {
        Mouse mouse = Mouse.current;
        if (game == null || mouse == null || !game.CanPlayerAct) return;
        if (mouse.delta.ReadValue().sqrMagnitude <= 0f) return;
        Vector2 pos = mouse.position.ReadValue();
        if (IsOver(holdButton, pos)) game.Select(PlayerAction.Buy);
        else if (IsOver(sellButton, pos)) game.Select(PlayerAction.Sell);
    }

    bool IsOver(Button button, Vector2 screenPos) =>
        button != null && button.isActiveAndEnabled &&
        RectTransformUtility.RectangleContainsScreenPoint((RectTransform)button.transform, screenPos, uiCamera);

    // 클릭 = 선택 + 실행 (Spec 제안 4). 키보드는 그대로 A/D로 고르고 Space로 실행.
    void OnClick(PlayerAction action)
    {
        if (!game.CanPlayerAct) return;
        game.Select(action);
        game.Execute();
    }

    public void Refresh(GameManager game)
    {
        bool can = game.CanPlayerAct;
        holdButton.interactable = can;
        sellButton.interactable = can;
        holdImage.sprite = SpriteFor(can, game.Selected == PlayerAction.Buy);
        sellImage.sprite = SpriteFor(can, game.Selected == PlayerAction.Sell);

        // 내 실행으로 공개가 막 시작된 순간(클릭이든 Space든) → 고른 버튼이 1도트 눌림
        if (game.IsRevealing && !wasRevealing && game.Current == game.Player)
        {
            bool hold = game.Selected == PlayerAction.Buy;
            StartCoroutine(Press(hold ? holdImage.rectTransform : sellImage.rectTransform, hold ? holdHome : sellHome));
        }
        wasRevealing = game.IsRevealing;
    }

    IEnumerator Press(RectTransform target, Vector2 home)
    {
        target.anchoredPosition = home + new Vector2(0f, -1f);
        yield return new WaitForSeconds(pressSeconds);
        target.anchoredPosition = home;
    }

    Sprite SpriteFor(bool can, bool selected)
    {
        if (!can) return disabledSprite;
        return selected ? selectedSprite : normalSprite;
    }
}
