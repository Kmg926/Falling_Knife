using System.Collections;
using UnityEngine;

// room-dress 115(2026-11-27, Grilling Q7): 방이 살아 있게 — 판정과 상관없는 꾸밈. 읽는 것도 바꾸는 것도 그림뿐.
// - 장비 LED: LED만 그린 겹침 그림 2장(room-leds-a·b)을 0.6~1.6초마다 번갈아 → 깜빡이는 장비 불빛
// - 전등: 4~9초마다 한 번, 빛줄기·바닥 빛(LampBeam·LampPool)이 아주 잠깐(0.06초) 옅어짐(1~2번) → 낡은 전등 느낌
// 시선을 뺏지 않게 짧고 약하게. Time 기준이라 일시정지(timeScale 0) 중엔 같이 멈춤.
public class RoomFlicker : MonoBehaviour
{
    [Header("장비 LED")]
    [SerializeField] SpriteRenderer leds;
    [SerializeField] Sprite ledsA;
    [SerializeField] Sprite ledsB;
    [SerializeField] Vector2 ledSeconds = new Vector2(0.6f, 1.6f);   // 바뀌는 간격(최소·최대)

    [Header("전등 깜빡임")]
    [SerializeField] SpriteRenderer[] lampLights;                       // LampBeam·LampPool
    [SerializeField] Vector2 flickerGap = new Vector2(4f, 9f);          // 깜빡임 사이 간격(최소·최대)
    [SerializeField] float dimAlpha = 0.7f;                             // 옅어질 때 원래 투명도에 곱함
    [SerializeField] float dimSeconds = 0.06f;

    float ledTimer, flickTimer;
    float[] baseAlpha;
    bool showingA = true;

    void Start()
    {
        baseAlpha = new float[lampLights.Length];
        for (int i = 0; i < lampLights.Length; i++) baseAlpha[i] = lampLights[i] != null ? lampLights[i].color.a : 1f;
        ledTimer = Random.Range(ledSeconds.x, ledSeconds.y);
        flickTimer = Random.Range(flickerGap.x, flickerGap.y);
        if (leds != null) leds.sprite = ledsA;
    }

    void Update()
    {
        ledTimer -= Time.deltaTime;
        if (ledTimer <= 0f && leds != null)
        {
            showingA = !showingA;
            leds.sprite = showingA ? ledsA : ledsB;
            ledTimer = Random.Range(ledSeconds.x, ledSeconds.y);
        }
        flickTimer -= Time.deltaTime;
        if (flickTimer <= 0f)
        {
            StartCoroutine(Flicker(Random.value < 0.4f ? 2 : 1));   // 가끔은 두 번 연달아
            flickTimer = Random.Range(flickerGap.x, flickerGap.y);
        }
    }

    IEnumerator Flicker(int times)
    {
        for (int k = 0; k < times; k++)
        {
            SetLamp(dimAlpha);
            yield return new WaitForSeconds(dimSeconds);
            SetLamp(1f);
            yield return new WaitForSeconds(0.08f);
        }
    }

    void SetLamp(float mul)
    {
        for (int i = 0; i < lampLights.Length; i++)
        {
            if (lampLights[i] == null) continue;
            Color c = lampLights[i].color;
            c.a = baseAlpha[i] * mul;
            lampLights[i].color = c;
        }
    }
}
