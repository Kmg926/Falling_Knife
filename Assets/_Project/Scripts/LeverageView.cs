using TMPro;
using UnityEngine;

// 레버리지 상자: 불꽃 3개(켜진 수 = 레버리지), "2/3", "다음 피해 3". 읽기만 한다.
// device-art 110(사용자 2026-11-27 "레버리지를 숨겨보자" → "바뀔 때만 잠깐" 채택):
// 평소엔 숨기고, 레버리지 값이 바뀔 때만 상자 전체가 나타났다가(0.25초) showSeconds 동안 보이고 사라진다.
// 튜토리얼 판에선 광대가 이 상자를 가리키며 설명하므로 늘 보인다.
public class LeverageView : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] flames;   // 왼쪽부터 3개
    [SerializeField] Sprite flameOn;
    [SerializeField] Sprite flameOff;
    [SerializeField] TMP_Text countText;        // "2/3"
    [SerializeField] TMP_Text damageText;       // "다음 피해 3"

    [Header("device-art 110: 바뀔 때만 잠깐 보이기")]
    [SerializeField] SpriteRenderer box;        // 상자 그림(LeverageBox)
    [SerializeField] TMP_Text titleText;        // "레버리지"
    [SerializeField] float showSeconds = 1.5f;  // 다 나타난 뒤 보이는 시간
    [SerializeField] float fadeSeconds = 0.25f; // 나타나기·사라지기 시간

    int lastValue = -1;      // 처음 Refresh 전 = -1(판 시작 때 0은 "바뀜"으로 안 침)
    float shownUntil = -1f;  // 이 시각(Time.time)까지 보임
    bool pinned;             // 튜토리얼 = 늘 보임
    float alpha = -1f;       // 지금 보이는 정도(0~1), -1 = 아직 한 번도 안 칠함

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

        // 값이 바뀌었으면(오름·0으로 돌아감 모두) 지금부터 보이기 시작
        if (lastValue >= 0 && value != lastValue) shownUntil = Time.time + fadeSeconds + showSeconds;
        lastValue = value;
        pinned = game.IsTutorial;
    }

    void Update()
    {
        float target = pinned || Time.time < shownUntil ? 1f : 0f;
        // 첫 프레임은 바로 그 상태로(판 시작에 상자가 한 번 깜빡 사라지는 것 막기)
        float next = alpha < 0f ? target : Mathf.MoveTowards(alpha, target, Time.deltaTime / Mathf.Max(0.01f, fadeSeconds));
        if (Mathf.Approximately(next, alpha)) return;   // 바뀔 때만 색을 다시 칠함
        alpha = next;
        SetAlpha(box, alpha);
        foreach (var f in flames) SetAlpha(f, alpha);
        if (titleText != null) titleText.alpha = alpha;
        countText.alpha = alpha;
        damageText.alpha = alpha;
    }

    static void SetAlpha(SpriteRenderer sr, float a)
    {
        if (sr == null) return;
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }
}
