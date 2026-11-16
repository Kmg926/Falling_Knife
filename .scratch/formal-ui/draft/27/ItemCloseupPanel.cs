using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 아이템 확인 패널 (GDD 08장 이름). 확인 중이면 배경을 어둡게 하고 큰 아이콘·이름·분류·설명·키 안내를 보인다.
// 판정은 하지 않는다: `사용`·`취소` 클릭은 Space·Esc와 같은 GameManager 함수를 부른다.
public class ItemCloseupPanel : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] GameObject root;            // 어둡게 + 패널 전체 (켜고 끄는 단위)
    [SerializeField] Image bigIcon;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text categoryText;
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] Button useButton;           // "Space 사용"
    [SerializeField] Button cancelButton;        // "Esc 취소"
    // ItemType 순서대로: 차트 분석, 찌라시, 거래 정지, 복권
    [SerializeField] Sprite[] bigIcons;

    void Awake()
    {
        useButton.onClick.AddListener(() => game.Execute());       // 확인 중 Execute = 그 아이템 사용
        cancelButton.onClick.AddListener(() => game.CancelItem());
    }

    public void Refresh(GameManager game)
    {
        root.SetActive(game.IsConfirming);
        if (!game.IsConfirming) return;

        ItemType item = game.Player.Items[game.ConfirmingSlot];
        bigIcon.sprite = bigIcons[(int)item];
        nameText.text = game.ItemName(item);
        categoryText.text = game.ItemCategory(item);

        string description = game.ItemDescription(item);
        // 거래 정지는 상대가 이미 정지면 쓸 수 없다고 미리 알려 준다(개발용 글자와 같은 정보).
        if (item == ItemType.TradingHalt && game.Clown.IsHalted)
        {
            description += "\n(사용 불가: 상대가 이미 정지 상태)";
        }
        descriptionText.text = description;
    }
}
