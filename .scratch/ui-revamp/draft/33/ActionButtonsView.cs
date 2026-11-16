using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 보유(나에게)·매도(광대에게) 버튼: 선택 표시, 못 누를 때 회색, 클릭하면 그 행동을 고르고 바로 실행.
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

    void Awake()
    {
        holdHome = holdImage.rectTransform.anchoredPosition;
        sellHome = sellImage.rectTransform.anchoredPosition;
        holdButton.onClick.AddListener(() => OnClick(PlayerAction.Hold));
        sellButton.onClick.AddListener(() => OnClick(PlayerAction.Sell));
    }

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
        holdImage.sprite = SpriteFor(can, game.Selected == PlayerAction.Hold);
        sellImage.sprite = SpriteFor(can, game.Selected == PlayerAction.Sell);

        // 내 실행으로 공개가 막 시작된 순간(클릭이든 Space든) → 고른 버튼이 1도트 눌림
        if (game.IsRevealing && !wasRevealing && game.Current == game.Player)
        {
            bool hold = game.Selected == PlayerAction.Hold;
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
