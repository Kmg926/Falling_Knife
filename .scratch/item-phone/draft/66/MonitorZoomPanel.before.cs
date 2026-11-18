using TMPro;
using UnityEngine;
using UnityEngine.UI;

// monitor-trade 43: 내 모니터 확대 화면(매수·매도·아이템을 고르는 곳)과, 테이블 위 모니터의 "열 수 있음" 표시.
// 판정은 하지 않는다 — 열기·닫기를 GameManager에 알리고, 상태를 읽어 보여 줄 뿐.
// 매수·매도 버튼(ActionButtonsView)과 아이템 칸(ItemSlotsView)은 이 화면 안으로 옮겼고 동작은 그대로다.
public class MonitorZoomPanel : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] GameObject root;              // 어둡게 + 확대 화면(버튼·아이템 칸·글자 포함)
    [SerializeField] GameObject mainGroup;         // monitor-trade 44: 첫 화면(차트 칸·매수·매도·아이템 칸·키 안내). 상세 페이지가 열리면 숨김
    [SerializeField] Button backdrop;              // 어두운 바깥 클릭 = 닫기
    [SerializeField] Button monitorButton;         // 테이블 위 내 모니터 자리의 투명 버튼 = 열기
    [SerializeField] SpriteRenderer outline;       // 열 수 있을 때 금색 테두리
    [SerializeField] TMP_Text openHint;            // 열 수 있을 때 "Space"
    [SerializeField] Image knownCandle;            // 차트 칸 가운데: 차트 분석으로 아는 다음 캔들
    [SerializeField] TMP_Text unknownMark;         // 모르면 "?"
    [SerializeField] Sprite upSprite;
    [SerializeField] Sprite downSprite;

    void Awake()
    {
        backdrop.onClick.AddListener(() => game.CloseMonitor());
        monitorButton.onClick.AddListener(() => game.OpenMonitor());
    }

    public void Refresh(GameManager game)
    {
        root.SetActive(game.IsMonitorOpen);
        // monitor-trade 44: 아이템을 고르면 같은 화면 안이 상세 페이지(ItemCloseupPanel)로 바뀐다 → 첫 화면은 숨김
        mainGroup.SetActive(!game.IsConfirming);

        // 열 수 있을 때만 금색 테두리·Space 안내·클릭 가능(깜빡임 없음, Spec 제안 5)
        bool can = game.CanOpenMonitor;
        outline.enabled = can;
        openHint.enabled = can;
        monitorButton.interactable = can;

        // 차트 칸: 차트 분석으로 알면 그 캔들, 모르면 "?"
        bool known = game.PlayerKnownNext.HasValue;
        knownCandle.enabled = known;
        unknownMark.enabled = !known;
        if (known) knownCandle.sprite = game.PlayerKnownNext.Value == Candle.Up ? upSprite : downSprite;
    }
}
