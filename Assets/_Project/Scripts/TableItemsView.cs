using UnityEngine;
using UnityEngine.UI;

// monitor-trade 44: 테이블 위(내 모니터 오른쪽)에 놓인 내 아이템 작은 아이콘. 읽기만 한다.
// 클릭하면 1~4 키와 같은 GameManager.SelectItem → 모니터가 그 아이템 상세로 바로 열린다.
public class TableItemsView : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] Image[] icons;          // 4칸(각각 Button 붙음), 왼쪽부터 1~4번
    [SerializeField] Sprite[] itemSprites;   // ItemType 순서: 차트 분석, 찌라시, 거래 정지, 복권

    void Awake()
    {
        for (int i = 0; i < icons.Length; i++)
        {
            int slot = i;                     // 람다가 반복 변수 대신 이 칸 번호를 기억하게 복사
            Button button = icons[i].GetComponent<Button>();
            if (button != null) button.onClick.AddListener(() => game.SelectItem(slot));
        }
    }

    public void Refresh(GameManager game)
    {
        var items = game.Player.Items;
        bool can = game.CanOpenMonitor;      // 내 차례에 모니터가 닫혀 있을 때만 누를 수 있음
        for (int i = 0; i < icons.Length; i++)
        {
            bool has = i < items.Count;
            icons[i].enabled = has;
            if (has) icons[i].sprite = itemSprites[(int)items[i]];
            Button button = icons[i].GetComponent<Button>();
            if (button != null) button.interactable = has && can;
        }
    }
}
