using UnityEngine;

// 타이틀 광대 눈 발작(사용자 2026-11-19 "발작형, 2도트"). 타이틀 광대에만 붙인다(게임 광대는 그대로).
// 조용히 0.5~3초 → 0.3~1초 동안 0.03~0.08초마다 떨림 그림(바늘·갈라짐·위로) 중 하나 + 위치 ±2도트 → 평소 눈으로.
// Animator(숨쉬기·깜빡임)는 건드리지 않는다: 원래 눈 덮개(Eyes)와 같은 자리에 따로 둔 덮개(twitch)를 LateUpdate에서
// Animator가 이번 프레임에 정한 Eyes 자리 + 흔들림으로 옮겨 그 위에 그린다.
public class ClownEyeTwitch : MonoBehaviour
{
    [SerializeField] Transform eyes;               // 원래 눈 덮개(Animator가 숨쉬기에 맞춰 위아래로 움직임)
    [SerializeField] SpriteRenderer twitch;        // 발작 덮개(eyes와 같은 부모, 그 위 정렬)
    [SerializeField] Sprite[] twitchSprites;       // 바늘·갈라짐·위로

    [Header("발작형 (Spec 제안: 조용 0.5~3초, 발작 0.3~1초, 0.03~0.08초마다 바뀜)")]
    [SerializeField] float quietMin = 0.5f;
    [SerializeField] float quietMax = 3f;
    [SerializeField] float burstMin = 0.3f;
    [SerializeField] float burstMax = 1f;
    [SerializeField] float stepMin = 0.03f;
    [SerializeField] float stepMax = 0.08f;
    [SerializeField] int jitterDots = 2;           // 흔들림 ±2도트(사용자)
    [SerializeField] float pixelsPerUnit = 2f;     // 그림 PPU 2 → 1도트 = 0.5

    float quietLeft;
    float burstLeft;
    float stepLeft;
    Vector3 offset;

    void Start()
    {
        quietLeft = Random.Range(quietMin, quietMax);
        twitch.enabled = false;
    }

    // Animator가 Eyes를 움직인 뒤(LateUpdate) 덮개 자리를 정해야 숨쉬기와 어긋나지 않는다
    void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (burstLeft <= 0f)
        {
            quietLeft -= dt;
            if (quietLeft > 0f) return;
            burstLeft = Random.Range(burstMin, burstMax);   // 발작 시작
            stepLeft = 0f;
        }

        burstLeft -= dt;
        if (burstLeft <= 0f)
        {
            twitch.enabled = false;                          // 평소 눈으로
            quietLeft = Random.Range(quietMin, quietMax);
            return;
        }

        stepLeft -= dt;
        if (stepLeft <= 0f)
        {
            // 새 떨림 그림 + 새 흔들림(가로·세로 각각 −2~+2도트)
            twitch.sprite = twitchSprites[Random.Range(0, twitchSprites.Length)];
            offset = new Vector3(Random.Range(-jitterDots, jitterDots + 1), Random.Range(-jitterDots, jitterDots + 1), 0f) / pixelsPerUnit;
            stepLeft = Random.Range(stepMin, stepMax);
        }
        twitch.enabled = true;
        twitch.transform.localPosition = eyes.localPosition + offset;
    }
}
