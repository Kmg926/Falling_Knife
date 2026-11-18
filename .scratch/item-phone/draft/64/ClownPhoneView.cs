using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// item-phone 64: 광대 폰(테이블 뒤에 펼쳐 세움). 화면에 광대 아이템을 작은 아이콘 2×2로 늘 보여 준다. 읽기만 한다(판정 없음).
// 광대가 아이템을 쓰면(GameManager.ClownItemUseCount가 바뀌면) 그 칸을 금색 테두리로 3번 반짝인 뒤 새 목록을 보인다.
public class ClownPhoneView : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] Image[] icons;            // 4칸(왼쪽 위부터 1 2 / 3 4)
    [SerializeField] Image[] flashes;          // 4칸 반짝임 테두리
    // ItemType 순서대로: 차트 분석, 찌라시, 거래 정지, 복권
    [SerializeField] Sprite[] miniIcons;

    [Header("사용 반짝임 (Spec 제안 6)")]
    [SerializeField] int blinks = 3;
    [SerializeField] float blinkSeconds = 0.1f;   // 켜짐 0.05 + 꺼짐 0.05 → 3번 = 0.3초

    int lastUseCount = -1;      // 처음 Refresh 때 지금 값으로 맞춤(시작부터 반짝이지 않게)
    bool flashing;              // 반짝이는 동안은 옛 목록(쓴 아이템이 아직 있는)을 그대로 둔다
    readonly List<ItemType> shown = new List<ItemType>();

    public void Refresh(GameManager game)
    {
        if (lastUseCount < 0) lastUseCount = game.ClownItemUseCount;

        // 광대가 방금 아이템을 썼다 → 그 칸 반짝임(보이는 동안만, 꺼진 오브젝트는 코루틴을 못 돌림)
        if (game.ClownItemUseCount != lastUseCount)
        {
            lastUseCount = game.ClownItemUseCount;
            int slot = game.ClownLastUsedSlot;
            if (slot >= 0 && slot < shown.Count && isActiveAndEnabled)
            {
                StopAllCoroutines();
                StartCoroutine(Flash(slot));
                return;
            }
        }
        if (!flashing) Show(game.Clown.Items);
    }

    IEnumerator Flash(int slot)
    {
        flashing = true;
        for (int i = 0; i < blinks; i++)
        {
            flashes[slot].enabled = true;
            yield return new WaitForSeconds(blinkSeconds / 2f);
            flashes[slot].enabled = false;
            yield return new WaitForSeconds(blinkSeconds / 2f);
        }
        flashing = false;
        Show(game.Clown.Items);   // 반짝임이 끝나면 지금 목록(쓴 칸이 빠지고 뒤 칸이 당겨짐)
    }

    void Show(IReadOnlyList<ItemType> items)
    {
        shown.Clear();
        shown.AddRange(items);
        for (int i = 0; i < icons.Length; i++)
        {
            bool has = i < items.Count;
            icons[i].enabled = has;
            if (has) icons[i].sprite = miniIcons[(int)items[i]];
            flashes[i].enabled = false;
        }
    }

    // 일시정지 등으로 꺼졌다 켜질 때 반짝임이 멈춘 채 남지 않게
    void OnDisable()
    {
        flashing = false;
        if (flashes != null) foreach (var f in flashes) if (f != null) f.enabled = false;
    }
}
