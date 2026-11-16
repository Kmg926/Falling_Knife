using System;
using System.Collections;
using UnityEngine;

// 공통 움직임 도구 (ui-revamp 30, 제안 8). 외부 패키지(DOTween 등) 없이 코루틴 + easing 함수.
// 위치는 정수 도트로 반올림한다 → 4배 확대된 도트가 반 칸에 걸쳐 흐려지지 않는다.
public static class Tween
{
    // easing: 0~1 진행률을 받아 0~1(조금 넘을 수도 있음) 움직임 비율로 바꾼다. 직선 속도는 딱딱해 보여서 피한다.
    public static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);   // 빠르게 시작해 천천히 멈춤
    public static float EaseInCubic(float t) => t * t * t;                      // 천천히 시작해 빠르게 나감
    public static float EaseOutBack(float t)                                     // 살짝 지나쳤다가 돌아옴(튀는 등장)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    // Canvas 위 물건(RectTransform)을 from → to로 seconds 동안 옮긴다.
    public static IEnumerator MoveAnchored(RectTransform target, Vector2 from, Vector2 to, float seconds, Func<float, float> ease)
    {
        float t = 0f;
        while (t < seconds)
        {
            target.anchoredPosition = Snap(Vector2.LerpUnclamped(from, to, ease(t / seconds)));
            t += Time.deltaTime;
            yield return null;
        }
        target.anchoredPosition = Snap(to);
    }

    // 월드 물건(Transform)을 부모 기준 from → to로 옮긴다.
    public static IEnumerator MoveLocal(Transform target, Vector3 from, Vector3 to, float seconds, Func<float, float> ease)
    {
        float t = 0f;
        while (t < seconds)
        {
            target.localPosition = Snap(Vector3.LerpUnclamped(from, to, ease(t / seconds)));
            t += Time.deltaTime;
            yield return null;
        }
        target.localPosition = Snap(to);
    }

    static Vector2 Snap(Vector2 v) => new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
    static Vector3 Snap(Vector3 v) => new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), v.z);
}
