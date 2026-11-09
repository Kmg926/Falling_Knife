using UnityEngine;

// 아이템 한 종류의 이름·설명. 효과 코드는 없다(효과는 GameManager가 처리).
// Project 창 → 우클릭 → Create → Falling Knife → Item Data 로 에셋을 만든다.
[CreateAssetMenu(fileName = "ItemData", menuName = "Falling Knife/Item Data")]
public class ItemData : ScriptableObject
{
    public ItemType type;
    public string displayName;
    [TextArea] public string description;
}
