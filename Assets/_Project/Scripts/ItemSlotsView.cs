using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 내 아이템 슬롯 4칸(아이콘·선택 테두리·클릭). 클릭은 1~4 키와 같다.
// item-phone 63: 4칸은 모니터에서 폰 화면(2×2)으로 옮겨 감. item-phone 64: 광대 아이콘은 광대 폰으로.
public class ItemSlotsView : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] Button[] slotButtons;   // 4개
    [SerializeField] Image[] slotFrames;     // 4개 (선택이면 노란 테두리)
    [SerializeField] Image[] slotIcons;      // 4개
    // item-phone 64: 광대 아이템 아이콘(왼쪽 위)은 광대 폰(ClownPhoneView)으로 옮겨 감
    [SerializeField] Sprite slotNormal;
    [SerializeField] Sprite slotSelected;
    // ItemType 순서대로: 차트 분석, 찌라시, 거래 정지, 복권
    [SerializeField] Sprite[] itemIcons;

    [Header("눌림 (ui-revamp 33)")]
    [SerializeField] float pressSeconds = 0.1f;
    Vector2[] slotHomes;
    int lastConfirming = -1;

    void Awake()
    {
        slotHomes = new Vector2[slotFrames.Length];
        for (int i = 0; i < slotFrames.Length; i++) slotHomes[i] = slotFrames[i].rectTransform.anchoredPosition;
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slot = i;   // 반복 변수 i를 그대로 쓰면 모든 버튼이 마지막 값(4)을 보게 된다 → 칸마다 따로 복사
            slotButtons[i].onClick.AddListener(() => game.SelectItem(slot));
        }
    }

    public void Refresh(GameManager game)
    {
        ShowItems(slotIcons, game.Player.Items);
        // item-phone 64: 광대 아이템은 ClownPhoneView가 보여 준다

        // item-phone 63: 칸이 폰 목록 안으로 옮겨 감 → 금색 틀 = Space를 누르면 열릴 칸(PhoneCursor)
        for (int i = 0; i < slotFrames.Length; i++)
        {
            slotFrames[i].sprite = game.PhoneCursor == i ? slotSelected : slotNormal;
        }

        // 다른 칸을 확인하기 시작한 순간(1~4 키든 클릭이든) → 그 칸이 1도트 눌림
        // item-phone 63: 칸이 폰 안(꺼져 있을 수 있음)으로 옮겨 감 → 보이지 않을 때는 눌림을 건너뜀(꺼진 오브젝트는 코루틴을 못 돌림)
        if (game.ConfirmingSlot >= 0 && game.ConfirmingSlot != lastConfirming && gameObject.activeInHierarchy)
        {
            StartCoroutine(Press(game.ConfirmingSlot));
        }
        lastConfirming = game.ConfirmingSlot;
    }

    // item-phone 63: 칸을 누르면 상세가 열리며 목록이 꺼진다 → 눌림 도중 멈춘 칸이 1도트 내려간 채 남지 않게 제자리로
    void OnDisable()
    {
        if (slotHomes == null) return;
        for (int i = 0; i < slotFrames.Length; i++) slotFrames[i].rectTransform.anchoredPosition = slotHomes[i];
    }

    IEnumerator Press(int slot)
    {
        RectTransform rt = slotFrames[slot].rectTransform;
        rt.anchoredPosition = slotHomes[slot] + new Vector2(0f, -1f);
        yield return new WaitForSeconds(pressSeconds);
        rt.anchoredPosition = slotHomes[slot];
    }

    // 아이템이 있는 칸만 아이콘을 보인다. (int)item = enum 순서 번호 → itemIcons의 같은 번호 그림
    void ShowItems(Image[] icons, IReadOnlyList<ItemType> items)
    {
        for (int i = 0; i < icons.Length; i++)
        {
            bool has = i < items.Count;
            icons[i].enabled = has;
            if (has) icons[i].sprite = itemIcons[(int)items[i]];
        }
    }
}
