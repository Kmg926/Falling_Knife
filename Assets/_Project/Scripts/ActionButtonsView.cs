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
    void Awake()
    {
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
    }

    Sprite SpriteFor(bool can, bool selected)
    {
        if (!can) return disabledSprite;
        return selected ? selectedSprite : normalSprite;
    }
}
