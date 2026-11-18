using UnityEngine;

// 타이틀 광대 눈 발작(사용자 2026-11-19 "발작형, 2도트"). 타이틀 광대에만 붙인다(게임 광대는 그대로).
// 조용히 0.5~3초 → 0.3~1초 동안 0.03~0.08초마다 떨림 눈동자(바늘·갈라짐·위로) 중 하나 + 눈동자만 ±2도트 → 평소 눈으로.
// 고침2: 조용한 시간이 끝날 때 lookChance 확률로 발작 대신 아래·위·오른쪽 중 한쪽을 0.8~1.6초 가만히 봄.
// Animator(숨쉬기·깜빡임)는 건드리지 않는다: 원래 눈 덮개(Eyes)와 같은 자리에 따로 둔 덮개들을 LateUpdate에서
// Animator가 이번 프레임에 정한 Eyes 자리로 옮겨 그 위에 그린다.
// 고침(사용자 "눈동자만 움직여야 하는데 흰 배경도 움직여서 파란 선을 넘어가"): 덮개를 셋으로 나눔
//   fill  = 눈 안쪽 흰 바탕(원래 눈동자를 지움) — 제자리
//   mask  = 눈 안쪽 모양 가리개(SpriteMask) — 제자리
//   pupil = 떨림 눈동자만 — 흔들림, 가리개 안에서만 보임(테두리 밖으로 안 나감)
public class ClownEyeTwitch : MonoBehaviour
{
    [SerializeField] Transform eyes;               // 원래 눈 덮개(Animator가 숨쉬기에 맞춰 위아래로 움직임)
    [SerializeField] SpriteRenderer fill;          // 흰 바탕(eyes와 같은 부모, 그 위 정렬)
    [SerializeField] SpriteMask mask;              // 눈 안쪽 가리개
    [SerializeField] SpriteRenderer pupil;         // 떨림 눈동자(가리개 안에서만 보임)
    [SerializeField] Sprite[] pupilSprites;        // 바늘·갈라짐·위로(눈동자만)

    [Header("발작형 (조용 0.5~3초, 발작 0.3~1초, 0.03~0.08초마다 바뀜)")]
    [SerializeField] float quietMin = 0.5f;
    [SerializeField] float quietMax = 3f;
    [SerializeField] float burstMin = 0.3f;
    [SerializeField] float burstMax = 1f;
    [SerializeField] float stepMin = 0.03f;
    [SerializeField] float stepMax = 0.08f;
    [SerializeField] int jitterDots = 2;           // 눈동자 흔들림 ±2도트(사용자)
    [SerializeField] float pixelsPerUnit = 2f;     // 그림 PPU 2 → 1도트 = 0.5

    [Header("조용히 한쪽 보기 (사용자 \"발작만 말고 아래·위·오른쪽만 보는 것도\")")]
    [SerializeField] Sprite[] lookSprites;         // 아래·위·오른쪽(눈동자만, 흔들림 없음)
    [SerializeField, Range(0f, 1f)] float lookChance = 0.5f;   // 조용한 시간이 끝날 때 발작 대신 한쪽 보기를 할 확률
    [SerializeField] float lookMin = 0.8f;
    [SerializeField] float lookMax = 1.6f;

    bool looking;                                  // 지금이 한쪽 보기(흔들림 없음)인가
    float quietLeft;
    float burstLeft;
    float stepLeft;
    Vector3 offset;

    void Start()
    {
        quietLeft = Random.Range(quietMin, quietMax);
        Show(false);
    }

    void Show(bool on)
    {
        fill.enabled = on;
        mask.enabled = on;
        pupil.enabled = on;
    }

    // Animator가 Eyes를 움직인 뒤(LateUpdate) 덮개 자리를 정해야 숨쉬기와 어긋나지 않는다
    void LateUpdate()
    {
        float dt = Time.deltaTime;
        if (burstLeft <= 0f)
        {
            quietLeft -= dt;
            if (quietLeft > 0f) return;
            // 조용한 시간이 끝나면: 가끔은 발작 대신 한쪽을 가만히 봄
            looking = lookSprites != null && lookSprites.Length > 0 && Random.value < lookChance;
            if (looking)
            {
                burstLeft = Random.Range(lookMin, lookMax);
                pupil.sprite = lookSprites[Random.Range(0, lookSprites.Length)];
                offset = Vector3.zero;
                stepLeft = float.MaxValue;                   // 한 번 고른 쪽을 끝까지 봄(바뀌지 않음)
            }
            else
            {
                burstLeft = Random.Range(burstMin, burstMax);   // 발작 시작
                stepLeft = 0f;
            }
        }

        burstLeft -= dt;
        if (burstLeft <= 0f)
        {
            Show(false);                                     // 평소 눈으로
            quietLeft = Random.Range(quietMin, quietMax);
            return;
        }

        stepLeft -= dt;
        if (stepLeft <= 0f)
        {
            // 새 떨림 눈동자 + 새 흔들림(가로·세로 각각 −2~+2도트)
            pupil.sprite = pupilSprites[Random.Range(0, pupilSprites.Length)];
            offset = new Vector3(Random.Range(-jitterDots, jitterDots + 1), Random.Range(-jitterDots, jitterDots + 1), 0f) / pixelsPerUnit;
            stepLeft = Random.Range(stepMin, stepMax);
        }
        Show(true);
        Vector3 at = eyes.localPosition;                     // 숨쉬기로 움직인 눈 자리
        fill.transform.localPosition = at;                   // 흰 바탕·가리개는 제자리
        mask.transform.localPosition = at;
        pupil.transform.localPosition = at + offset;         // 눈동자만 흔들림
    }
}
