using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;   // item-phone 70: 마우스 위치(소프트키 올림 표시)
using UnityEngine.UI;

// item-phone 63: 내 폴더폰. 읽기만 한다(판정 없음) — 클릭은 GameManager 함수를 부를 뿐.
// - 테이블 위 접힌 폰: 바깥 작은 화면에 아이템 개수, 열 수 있을 때 금색 테두리, 클릭 = 열기
// - 확대된 펼친 폰: 위 화면 = 목록(칸 2×2, ItemSlotsView) 또는 상세(ItemCloseupPanel), 아래 소프트키 글(왼쪽 = Space, 오른쪽 = Esc)
//   아래 몸체의 초록 키 = Space와 같음, 빨간 키 = Esc와 같음, 키패드 1~4 = 1~4 키와 같음(아이템이 있는 칸 번호만 금색)
public class PhoneView : MonoBehaviour
{
    [SerializeField] GameManager game;

    [Header("테이블 위 접힌 폰")]
    [SerializeField] Button closedButton;      // 접힌 폰 그림(클릭 = 열기)
    [SerializeField] Image outline;            // 열 수 있을 때 금색 테두리(모니터와 같은 방식, 깜빡임 없음)
    [SerializeField] TMP_Text countText;       // 바깥 작은 화면의 개수(아래 그림 5장이 있으면 끄고 그림으로)
    // item-phone 66: 숫자를 키우려고 폰을 크게 그리면서, 개수(0~4)를 화면 칸에 그려 넣은 폰 그림 5장으로 바꿔 끼운다(광대 폰과 같은 방식)
    [SerializeField] Image phoneImage;
    [SerializeField] Sprite[] countSprites;    // [0] = 0개 … [4] = 4개

    [Header("확대된 폰")]
    [SerializeField] GameObject zoomRoot;      // 어둡게 + 펼친 폰 전체
    [SerializeField] Button backdrop;          // 어두운 바깥 클릭 = 한 단계 뒤로
    [SerializeField] GameObject listGroup;     // 목록(제목 + 칸 2×2). 상세가 열리면 숨김
    [SerializeField] TMP_Text titleText;       // "아이템 N/4"
    [SerializeField] TMP_Text softLeft;        // 화면 왼쪽 아래: 목록 "확인" / 상세 "사용"
    [SerializeField] TMP_Text softRight;       // 화면 오른쪽 아래: 목록 "닫기" / 상세 "뒤로"
    [SerializeField] Button greenKey;          // 초록 키 = Space
    [SerializeField] Button redKey;            // 빨간 키 = Esc
    // 화면 아래 소프트키 글자("확인·사용" / "닫기·뒤로")도 누를 수 있게(사용자: "자세히 보기 때 사용, 뒤로가 안 눌려" — 글자만 있고 버튼이 아니었음)
    [SerializeField] Button softLeftButton;    // = 초록 키·Space
    [SerializeField] Button softRightButton;   // = 빨간 키·Esc
    [SerializeField] Button[] keypad;          // 키패드 1~4 (4개)
    [SerializeField] Image[] keyLit;           // 1~4 금색 조각(아이템이 있을 때만 켬)

    [Header("받을 때 울림 (item-phone 65)")]
    [SerializeField] PhoneBuzz buzz;           // 진동·"+N" 깜빡임(깜빡이는 동안은 폰 그림을 buzz가 맡음)

    [Header("clown-phone 85: 광대 폰 확대 화면 (빨강·금 간 유리·가끔 튐)")]
    [SerializeField] Image bodyImage;          // 펼친 폰 그림
    [SerializeField] Sprite mineBody;          // phone-open
    [SerializeField] Sprite clownBody;         // phone-open-clown
    [SerializeField] GameObject clownCrack;    // 화면 금(글자 아래 깔림)
    [SerializeField] ScreenGlitch clownGlitch; // 가끔 화면이 튐

    [Header("소프트키 마우스 올림 (item-phone 70)")]
    [SerializeField] Color softNormal = new Color32(0xCD, 0xC4, 0xB6, 255);   // cream
    [SerializeField] Color softHover = new Color32(0xD9, 0xB4, 0x5A, 255);    // gold2 (일시정지·타이틀 메뉴와 같음)

    const int MaxItems = 4;
    string leftLabel = "", rightLabel = "";    // 지금 소프트키 글(올림 표시 "▶ "를 붙이기 전)
    bool hoverLeft, hoverRight;
    Camera uiCamera;                           // HUD Canvas가 Screen Space Camera라 마우스 위치를 이 카메라로 계산
    bool wasOpenMine;                          // 지난번에 내 폰이 열려 있었나(닫히는 순간 새 칸 "N"을 지움)

    void Awake()
    {
        if (buzz != null) buzz.OnDone = () => Refresh(game);   // 깜빡임이 끝나면 개수 그림으로 되돌림
        closedButton.onClick.AddListener(() => game.OpenPhone());
        backdrop.onClick.AddListener(() => game.PhoneBack());
        greenKey.onClick.AddListener(() => game.Execute());   // 목록: 금색 틀 칸 상세 / 상세: 사용
        redKey.onClick.AddListener(() => game.PhoneBack());
        Canvas canvas = GetComponentInParent<Canvas>();
        uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        if (softLeftButton != null) softLeftButton.onClick.AddListener(() => game.Execute());
        if (softRightButton != null) softRightButton.onClick.AddListener(() => game.PhoneBack());
        for (int i = 0; i < keypad.Length; i++)
        {
            int slot = i;   // 람다가 반복 변수 대신 이 칸 번호를 기억하게 복사
            keypad[i].onClick.AddListener(() => game.SelectItem(slot));
        }
    }

    public void Refresh(GameManager game)
    {
        int count = game.Player.Items.Count;

        // 접힌 폰: 개수는 늘 보이고(광대 차례에도), 열 수 있을 때만 테두리·클릭
        if (buzz != null) buzz.Refresh(game);
        bool useSprites = phoneImage != null && countSprites != null && count < countSprites.Length;
        if (useSprites && !(buzz != null && buzz.Busy)) phoneImage.sprite = countSprites[count];

        // item-phone 65: 내 폰을 한 번 열었다 닫으면 새 칸 "N"을 지움
        bool openMine = game.IsPhoneOpen && !game.IsViewingClownPhone;
        if (wasOpenMine && !openMine) game.ClearNewItems();
        wasOpenMine = openMine;
        countText.enabled = !useSprites;
        countText.text = count.ToString();
        bool can = game.CanOpenPhone && !game.IsPhoneOpen;
        outline.enabled = can;
        closedButton.interactable = can;

        // 확대된 폰
        zoomRoot.SetActive(game.IsPhoneOpen);
        if (!game.IsPhoneOpen) return;

        // item-phone 66: 광대 폰을 보고 있으면 광대 아이템·"광대" 제목, 사용 칸(왼쪽 소프트키)은 비우고 키패드는 못 누름
        bool clown = game.IsViewingClownPhone;
        // clown-phone 85: 광대 폰이면 빨간 깨진 폰 그림 + 화면 금 + 글리치, 내 폰이면 원래대로
        if (bodyImage != null) bodyImage.sprite = clown ? clownBody : mineBody;
        if (clownCrack != null) clownCrack.SetActive(clown);
        if (clownGlitch != null) clownGlitch.enabled = clown;
        int shown = game.PhoneItems.Count;
        bool detail = game.IsConfirming;
        listGroup.SetActive(!detail);
        titleText.text = (clown ? "광대 아이템 " : "아이템 ") + $"{shown}/{MaxItems}";
        leftLabel = detail ? (clown ? "" : "사용") : "확인";
        rightLabel = detail ? "뒤로" : "닫기";
        ShowSoftKeys();
        for (int i = 0; i < keyLit.Length; i++) keyLit[i].enabled = i < shown;
        for (int i = 0; i < keypad.Length; i++) keypad[i].interactable = !clown;
    }

    // item-phone 70: 매 프레임 마우스가 소프트키 위에 있는지 직접 확인(일시정지 버튼과 같은 방식) → 금색 + "▶ "
    void Update()
    {
        if (game == null || !game.IsPhoneOpen || Mouse.current == null) return;
        Vector2 pos = Mouse.current.position.ReadValue();
        bool l = leftLabel != "" && IsOver(softLeftButton, pos);
        bool r = rightLabel != "" && IsOver(softRightButton, pos);
        if (l == hoverLeft && r == hoverRight) return;   // 바뀔 때만 다시 그림
        hoverLeft = l;
        hoverRight = r;
        ShowSoftKeys();
    }

    void ShowSoftKeys()
    {
        softLeft.text = (hoverLeft && leftLabel != "" ? "▶ " : "") + leftLabel;
        softLeft.color = hoverLeft ? softHover : softNormal;
        softRight.text = (hoverRight ? "▶ " : "") + rightLabel;
        softRight.color = hoverRight ? softHover : softNormal;
    }

    bool IsOver(Button button, Vector2 screenPos) =>
        button != null && button.isActiveAndEnabled &&
        RectTransformUtility.RectangleContainsScreenPoint((RectTransform)button.transform, screenPos, uiCamera);
}
