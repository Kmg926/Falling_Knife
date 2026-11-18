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

    [Header("유사 3D 회전 (ui-revamp 32, 제안 6)")]
    // ItemType 순서 × 8장(45° 간격). 0~7 = 차트 분석, 8~15 = 찌라시, …
    [SerializeField] Sprite[] spinFrames;
    [SerializeField] float frameSeconds = 0.15f;   // 8장 × 0.15초 = 1.2초에 한 바퀴
    [SerializeField] float bobSeconds = 1f;        // 위아래 1도트, 1초 주기
    [SerializeField] float dropSeconds = 0.2f;     // 열릴 때 위에서 툭 떨어지듯 등장

    const int FramesPerItem = 8;
    Vector2 iconHome;          // 큰 아이콘의 원래 자리
    ItemType shownItem;
    bool wasOpen;
    float openedAt;

    void Awake()
    {
        iconHome = bigIcon.rectTransform.anchoredPosition;
        useButton.onClick.AddListener(() => game.Execute());       // 확인 중 Execute = 그 아이템 사용
        cancelButton.onClick.AddListener(() => game.CancelItem());
    }

    // 열려 있는 동안 매 프레임: 회전 그림 넘기기 + 위아래 뜸 + 처음 등장
    void Update()
    {
        if (!root.activeSelf) return;
        float t = Time.time - openedAt;
        bigIcon.sprite = SpinFrame(t);

        float bob = Mathf.Round(Mathf.Sin(t * 2f * Mathf.PI / bobSeconds));           // -1, 0, 1 도트
        float drop = t < dropSeconds ? Mathf.Round(Mathf.Lerp(6f, 0f, Tween.EaseOutBack(t / dropSeconds))) : 0f;
        bigIcon.rectTransform.anchoredPosition = iconHome + new Vector2(0f, bob + drop);
    }

    // 지난 시간 → 그 순간의 각도 그림. 회전 그림이 없으면(연결 전) 평면 큰 아이콘
    Sprite SpinFrame(float t)
    {
        if (spinFrames == null || spinFrames.Length < (int)shownItem * FramesPerItem + FramesPerItem) return bigIcons[(int)shownItem];
        int frame = Mathf.FloorToInt(t / frameSeconds) % FramesPerItem;
        return spinFrames[(int)shownItem * FramesPerItem + frame];
    }

    public void Refresh(GameManager game)
    {
        root.SetActive(game.IsConfirming);
        if (!game.IsConfirming)
        {
            wasOpen = false;
            return;
        }

        ItemType item = game.Player.Items[game.ConfirmingSlot];
        // 새로 열렸거나 다른 아이템으로 바뀌면 처음 각도·등장부터
        if (!wasOpen || item != shownItem) openedAt = Time.time;
        wasOpen = true;
        shownItem = item;
        bigIcon.sprite = SpinFrame(0f);
        nameText.text = game.ItemName(item);
        categoryText.text = game.ItemCategory(item);

        string description = game.ItemDescription(item);
        // 거래 정지는 상대가 이미 정지면 쓸 수 없다고 미리 알려 준다(개발용 글자와 같은 정보).
        if (item == ItemType.TradingHalt && game.Clown.IsHalted)
        {
            description += "\n(상대가 이미 정지 상태라 못 쓴다)";
        }
        descriptionText.text = description;
    }
}
