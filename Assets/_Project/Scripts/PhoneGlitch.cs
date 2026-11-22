using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// clown-phone 84: 광대 폰이 가끔 지지직거린다(사용자 "깨져있고, 뭔가 불안정하게"). 읽기만 한다(판정 없음).
// 2~4초마다 0.1~0.2초 동안: 위에 덮은 그림(overlay)을 노이즈 화면 ↔ 빈 화면으로 0.05초씩 바꾸고 1도트 좌우로 떨게 한다.
// 폰 그림(개수 숫자·받을 때 "+N")은 각 View·PhoneBuzz가 그대로 맡고, 이 컴포넌트는 덮개만 켜고 끈다 → 서로 안 부딪힘.
// 소리 없음(아이템 받는 진동과 헷갈리지 않게, Grilling Q2). 받는 진동 중엔 쉼. WaitForSeconds라 일시정지 중엔 멈춤.
public class PhoneGlitch : MonoBehaviour
{
    [SerializeField] PhoneBuzz buzz;             // 받는 진동 중이면 지지직하지 않음
    [SerializeField] Image overlay;              // 폰 그림 위에 같은 크기로 덮는 그림(평소 꺼짐)
    [SerializeField] Sprite noiseSprite;         // 화면에 가로줄 노이즈
    [SerializeField] Sprite blankSprite;         // 화면 꺼짐(빈 화면)

    [Header("간격·길이 (Grilling Q2)")]
    [SerializeField] float gapMin = 2f;
    [SerializeField] float gapMax = 4f;
    [SerializeField] float lengthMin = 0.1f;
    [SerializeField] float lengthMax = 0.2f;
    [SerializeField] float stepSeconds = 0.05f;
    [SerializeField] float jitter = 0.5f;        // 지금 좌표 0.5 = 새 도트 1칸

    RectTransform rt;
    Vector2 home;

    public bool IsGlitching { get; private set; }

    void Awake()
    {
        rt = (RectTransform)overlay.transform;
        home = rt.anchoredPosition;
        overlay.enabled = false;
    }

    void OnEnable()
    {
        StartCoroutine(Loop());
    }

    // 꺼질 때(씬 전환 등) 덮개를 끄고 제자리로
    void OnDisable()
    {
        overlay.enabled = false;
        rt.anchoredPosition = home;
        IsGlitching = false;
    }

    IEnumerator Loop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(gapMin, gapMax));
            if (buzz != null && buzz.Busy) continue;   // 받는 진동("+N") 중엔 건너뜀

            IsGlitching = true;
            overlay.enabled = true;
            float length = Random.Range(lengthMin, lengthMax);
            int step = 0;
            for (float t = 0f; t < length; t += stepSeconds, step++)
            {
                bool odd = step % 2 == 1;
                overlay.sprite = odd ? blankSprite : noiseSprite;            // 노이즈 → 꺼짐 → 노이즈 …
                rt.anchoredPosition = home + new Vector2(odd ? -jitter : jitter, 0f);   // 1도트 좌우 떨림
                yield return new WaitForSeconds(stepSeconds);
            }
            overlay.enabled = false;
            rt.anchoredPosition = home;
            IsGlitching = false;
        }
    }
}
