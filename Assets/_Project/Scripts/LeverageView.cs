using TMPro;
using UnityEngine;

// 레버리지 상자: 불꽃 3개(켜진 수 = 레버리지), "2/3", "다음 피해 3". 읽기만 한다.
public class LeverageView : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] flames;   // 왼쪽부터 3개
    [SerializeField] Sprite flameOn;
    [SerializeField] Sprite flameOff;
    [SerializeField] TMP_Text countText;        // "2/3"
    [SerializeField] TMP_Text damageText;       // "다음 피해 3"

    public void Refresh(GameManager game)
    {
        int value = game.Leverage.Value;
        for (int i = 0; i < flames.Length; i++)
        {
            flames[i].sprite = i < value ? flameOn : flameOff;
        }
        countText.text = $"{value}/{LeverageCounter.Max}";
        // 하락 피해 = 1 + 레버리지 (GDD 04장)
        damageText.text = $"다음 피해 {1 + value}";
    }
}