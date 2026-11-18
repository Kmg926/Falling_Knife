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
    // item-phone 68: 폰 화면 안에서는 돌아가는 그림이 어색하다(사용자) → 꺼 두고 평면 큰 아이콘 + 위아래 뜸만. 켜면 예전처럼 회전
    [SerializeField] bool spin = false;
    [SerializeField] float frameSeconds = 0.15f;   // 8장 × 0.15초 = 1.2초에 한 바퀴
    [SerializeField] float bobSeconds = 1f;        // 위아래 1도트, 움직이는 시간 합(올라감 0.5 + 내려감 0.5)
    [SerializeField] float bobHoldSeconds = 0.5f;  // item-phone 69: 맨 위·맨 아래에서 잠깐 멈춤(광대 숨쉬기처럼). 처음 0.3 → 사용자 0.5
    [SerializeField] float dropSeconds = 0.2f;     // 열릴 때 위에서 툭 떨어지듯 등장
    // item-phone 70: 폰 화면에서는 "갑자기 아래로 움직이는 게 어색"(사용자) → 떨어짐 끔(켜면 예전처럼)
    [SerializeField] bool dropIn = false;

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

        // item-phone 70: 처음 순간 제자리(0)에서 시작하도록 올라가는 중간부터(Bob(0)은 맨 아래 −1이라 열자마자 1도트 툭 내려갔음)
        float bob = Bob(t + bobSeconds / 4f);                                          // -1, 0, 1 도트
        float drop = dropIn && t < dropSeconds ? Mathf.Round(Mathf.Lerp(6f, 0f, Tween.EaseOutBack(t / dropSeconds))) : 0f;
        bigIcon.rectTransform.anchoredPosition = iconHome + new Vector2(0f, bob + drop);
    }

    // item-phone 69: 위아래 뜸 = 올라감 → 맨 위 멈춤 → 내려감 → 맨 아래 멈춤(한 바퀴 = bobSeconds + 멈춤 2번)
    // 올라가고 내려가는 동안은 부드럽게(SmoothStep), 도트에 맞게 반올림 → -1, 0, 1
    float Bob(float t)
    {
        float move = bobSeconds / 2f;
        float cycle = 2f * (move + bobHoldSeconds);
        float p = t % cycle;
        if (p < move) return Mathf.Round(Mathf.SmoothStep(-1f, 1f, p / move));        // 올라감
        p -= move;
        if (p < bobHoldSeconds) return 1f;                                             // 맨 위 멈춤
        p -= bobHoldSeconds;
        if (p < move) return Mathf.Round(Mathf.SmoothStep(1f, -1f, p / move));         // 내려감
        return -1f;                                                                    // 맨 아래 멈춤
    }

    // 지난 시간 → 그 순간의 각도 그림. 회전 그림이 없으면(연결 전) 평면 큰 아이콘
    Sprite SpinFrame(float t)
    {
        if (!spin || spinFrames == null || spinFrames.Length < (int)shownItem * FramesPerItem + FramesPerItem) return bigIcons[(int)shownItem];
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

        ItemType item = game.PhoneItems[game.ConfirmingSlot];   // item-phone 66: 광대 폰을 보고 있으면 광대 아이템
        // 새로 열렸거나 다른 아이템으로 바뀌면 처음 각도·등장부터
        if (!wasOpen || item != shownItem) openedAt = Time.time;
        wasOpen = true;
        shownItem = item;
        bigIcon.sprite = SpinFrame(0f);
        nameText.text = game.ItemName(item);
        categoryText.text = game.ItemCategory(item);

        string description = game.ItemDescription(item);
        // 거래 정지는 상대가 이미 정지면 쓸 수 없다고 미리 알려 준다(개발용 글자와 같은 정보).
        if (item == ItemType.TradingHalt && game.Clown.IsHalted && !game.IsViewingClownPhone)   // 광대 폰은 보기만이라 안내 없음
        {
            description += "\n(상대가 이미 정지 상태라 못 쓴다)";
        }
        descriptionText.text = description;
    }
}
