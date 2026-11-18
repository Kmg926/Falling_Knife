using TMPro;
using UnityEngine;
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
    [SerializeField] TMP_Text countText;       // 바깥 작은 화면의 개수

    [Header("확대된 폰")]
    [SerializeField] GameObject zoomRoot;      // 어둡게 + 펼친 폰 전체
    [SerializeField] Button backdrop;          // 어두운 바깥 클릭 = 한 단계 뒤로
    [SerializeField] GameObject listGroup;     // 목록(제목 + 칸 2×2). 상세가 열리면 숨김
    [SerializeField] TMP_Text titleText;       // "아이템 N/4"
    [SerializeField] TMP_Text softLeft;        // 화면 왼쪽 아래: 목록 "확인" / 상세 "사용"
    [SerializeField] TMP_Text softRight;       // 화면 오른쪽 아래: 목록 "닫기" / 상세 "뒤로"
    [SerializeField] Button greenKey;          // 초록 키 = Space
    [SerializeField] Button redKey;            // 빨간 키 = Esc
    [SerializeField] Button[] keypad;          // 키패드 1~4 (4개)
    [SerializeField] Image[] keyLit;           // 1~4 금색 조각(아이템이 있을 때만 켬)

    const int MaxItems = 4;

    void Awake()
    {
        closedButton.onClick.AddListener(() => game.OpenPhone());
        backdrop.onClick.AddListener(() => game.PhoneBack());
        greenKey.onClick.AddListener(() => game.Execute());   // 목록: 금색 틀 칸 상세 / 상세: 사용
        redKey.onClick.AddListener(() => game.PhoneBack());
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
        countText.text = count.ToString();
        bool can = game.CanOpenPhone && !game.IsPhoneOpen;
        outline.enabled = can;
        closedButton.interactable = can;

        // 확대된 폰
        zoomRoot.SetActive(game.IsPhoneOpen);
        if (!game.IsPhoneOpen) return;

        bool detail = game.IsConfirming;
        listGroup.SetActive(!detail);
        titleText.text = $"아이템 {count}/{MaxItems}";
        softLeft.text = detail ? "사용" : "확인";
        softRight.text = detail ? "뒤로" : "닫기";
        for (int i = 0; i < keyLit.Length; i++) keyLit[i].enabled = i < count;
    }
}
