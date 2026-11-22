using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// clown-phone 85: 광대 폰 확대 화면이 가끔 튄다. 읽기만 한다(판정 없음).
// 2~4초마다 0.1초 동안: 화면 안 아무 높이에 가로줄 노이즈 띠(band)를 보이고, 화면 글자 묶음(shiftTargets)을 1~2도트 가로로 민다.
// PhoneView가 광대 폰을 볼 때만 켠다(enabled). 꺼지면 띠를 숨기고 글자를 제자리로.
public class ScreenGlitch : MonoBehaviour
{
    [SerializeField] Image band;                    // 글리치 띠(화면 폭), 평소 꺼짐
    [SerializeField] RectTransform[] shiftTargets;  // 같이 밀릴 화면 글자 묶음(목록·상세·소프트키)
    [SerializeField] Vector2 bandYRange = new Vector2(-12f, -88f);   // 띠 위쪽 끝이 놓일 높이(화면 안, 지금 좌표)

    [Header("간격·길이 (Grilling Q3)")]
    [SerializeField] float gapMin = 2f;
    [SerializeField] float gapMax = 4f;
    [SerializeField] float lengthSeconds = 0.1f;
    [SerializeField] float stepSeconds = 0.05f;
    [SerializeField] float[] shifts = { 0.5f, 1f };  // 0.5 = 새 도트 1칸, 1 = 2칸

    Vector2[] homes;

    void Awake()
    {
        homes = new Vector2[shiftTargets.Length];
        for (int i = 0; i < shiftTargets.Length; i++) homes[i] = shiftTargets[i].anchoredPosition;
        band.enabled = false;
    }

    void OnEnable()
    {
        StartCoroutine(Loop());
    }

    // enabled = false로 꺼도 코루틴은 저절로 멈추지 않는다(오브젝트를 끌 때만 멈춤) → 직접 멈춤.
    // (처음엔 이 줄이 없어 광대 폰을 닫고 내 폰을 열어도 띠가 계속 나왔음)
    void OnDisable()
    {
        StopAllCoroutines();
        Restore();
    }

    void Restore()
    {
        band.enabled = false;
        if (homes == null) return;
        for (int i = 0; i < shiftTargets.Length; i++) shiftTargets[i].anchoredPosition = homes[i];
    }

    IEnumerator Loop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(gapMin, gapMax));
            band.enabled = true;
            for (float t = 0f; t < lengthSeconds; t += stepSeconds)
            {
                // 매 0.05초: 띠 높이를 새로 고르고(도트 단위로 반올림), 글자 묶음을 오른쪽 또는 왼쪽으로
                Vector2 p = band.rectTransform.anchoredPosition;
                p.y = Mathf.Round(Random.Range(bandYRange.y, bandYRange.x) * 2f) / 2f;
                band.rectTransform.anchoredPosition = p;
                float dx = shifts[Random.Range(0, shifts.Length)] * (Random.value < 0.5f ? -1f : 1f);
                for (int i = 0; i < shiftTargets.Length; i++) shiftTargets[i].anchoredPosition = homes[i] + new Vector2(dx, 0f);
                yield return new WaitForSeconds(stepSeconds);
            }
            Restore();
        }
    }
}
