using System;
using UnityEngine;

// stock-screen 41: 배경 벽의 시세 선 2줄(빨강·파랑). 판정은 하지 않는다 — GameManager 상태를 읽기만 한다.
// 1초에 30번 맨 오른쪽에 새 값을 붙이고 한 칸씩 왼쪽으로 민다 → 오른쪽에서 왼쪽으로 흐르는 시세 줄.
// 새 값 = 큰 물결(사인 2개) + 잔 흔들림(규칙적인 의사 난수) + 출렁임.
// 출렁임: 캔들 공개가 **끝나는 순간** 하락이면 급락, 상승이면 급등(공개 중에는 반응 없음 → 결과를 미리 알려 주지 않음).
[RequireComponent(typeof(SpriteRenderer))]
public class ChartLinesView : MonoBehaviour
{
    [SerializeField] GameManager game;

    [Header("영역(새 도트): 640 x 160 = 지금 좌표 x 0~320, y 18~98")]
    [SerializeField] int width = 640;
    [SerializeField] int height = 160;

    [Header("흐름 (Spec 제안 7)")]
    [SerializeField] float samplesPerSecond = 30f;
    [SerializeField] float waveDots = 24f;          // 큰 물결 ±12(지금 도트) = ±24(새 도트)
    [SerializeField] float jitterDots = 4f;         // 잔 흔들림 ±2 = ±4
    [SerializeField] float redBase = 84f;           // 빨간 선 가운데 높이(텍스처 아래에서, 새 도트) = 지금 y 56
    [SerializeField] float blueBase = 56f;          // 파란 선 = 지금 y 70

    [Header("출렁임 (Spec 제안 8)")]
    [SerializeField] float downDots = -36f;         // 하락 −18 = −36
    [SerializeField] float upDots = 24f;            // 상승 +12 = +24
    [SerializeField] float dropSeconds = 0.25f;     // 0.25초에 떨어지고(오르고)
    [SerializeField] float recoverSeconds = 1f;     // 1초에 절반 회복
    [SerializeField] float settleSeconds = 2f;      // 그 뒤 2초에 걸쳐 0으로(선이 영역 밖으로 밀려나지 않게)

    [Header("색 (팔레트, 어두운 단계 — Spec 제안 6)")]
    [SerializeField] Color32 redLine = new Color32(0x6E, 0x26, 0x24, 255);     // red1
    [SerializeField] Color32 redShadow = new Color32(0x4E, 0x1A, 0x19, 255);   // red0
    [SerializeField] Color32 blueLine = new Color32(0x3F, 0x4E, 0x7C, 255);    // navy2
    [SerializeField] Color32 blueShadow = new Color32(0x28, 0x34, 0x56, 255);  // navy

    Texture2D tex;
    Color32[] pixels;
    float[] red;            // 화면 가로 칸마다 높이(맨 오른쪽 = 가장 새 값)
    float[] blue;
    int step;               // 지금까지 붙인 값의 수(시간 대신 쓰는 시계)
    float sampleTimer;
    bool wasRevealing;
    float spikeFrom, spikeTarget, spikeTime = -1f;

    public float Spike { get; private set; }                // 지금 출렁임 값(테스트용)
    public float NewestRed => red[width - 1];                // 가장 새 빨간 값(테스트용)
    public float NewestBlue => blue[width - 1];
    public int Step => step;

    void Awake()
    {
        tex = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
        pixels = new Color32[width * height];
        red = new float[width];
        blue = new float[width];
        for (int i = 0; i < width; i++)     // 처음부터 화면이 선으로 차 있게 과거 값을 미리 채움
        {
            red[i] = Sample(0, i);
            blue[i] = Sample(1, i);
        }
        step = width;
        // 기준점 = 왼쪽 위(0, 1), 1단위에 새 도트 2개(PPU 2) → 다른 그림과 같은 격자
        GetComponent<SpriteRenderer>().sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0f, 1f), 2f);
        Draw();
    }

    void OnDestroy()
    {
        if (tex != null) Destroy(tex);
    }

    void Update()
    {
        // 공개가 끝나는 순간(공개 중이었는데 지금은 아님) → 공개된 캔들 방향으로 출렁임 시작
        if (game != null)
        {
            if (wasRevealing && !game.IsRevealing && game.ShownCandle.HasValue)
                StartSpike(game.ShownCandle.Value == Candle.Down ? downDots : upDots);
            wasRevealing = game.IsRevealing;
        }
        UpdateSpike(Time.deltaTime);

        // 정해진 간격마다 새 값을 붙이고 왼쪽으로 민다(멈춤 = timeScale 0이면 같이 멈춤)
        sampleTimer += Time.deltaTime;
        float interval = 1f / samplesPerSecond;
        bool changed = false;
        while (sampleTimer >= interval)
        {
            sampleTimer -= interval;
            Push();
            changed = true;
        }
        if (changed) Draw();
    }

    // 테스트·연출에서 직접 부를 수 있게 공개(GameManager 판정과는 무관)
    public void StartSpike(float target)
    {
        spikeFrom = Spike;
        spikeTarget = target;
        spikeTime = 0f;
    }

    void UpdateSpike(float dt)
    {
        if (spikeTime < 0f) return;
        spikeTime += dt;
        float t = spikeTime;
        if (t < dropSeconds) Spike = Mathf.Lerp(spikeFrom, spikeTarget, Tween.EaseOutCubic(t / dropSeconds));
        else if ((t -= dropSeconds) < recoverSeconds) Spike = Mathf.Lerp(spikeTarget, spikeTarget * 0.5f, t / recoverSeconds);
        else if ((t -= recoverSeconds) < settleSeconds) Spike = Mathf.Lerp(spikeTarget * 0.5f, 0f, t / settleSeconds);
        else
        {
            Spike = 0f;
            spikeTime = -1f;
        }
    }

    void Push()
    {
        Array.Copy(red, 1, red, 0, width - 1);
        Array.Copy(blue, 1, blue, 0, width - 1);
        red[width - 1] = Sample(0, step);
        blue[width - 1] = Sample(1, step);
        step++;
    }

    // 한 선의 step번째 값(텍스처 아래에서 잰 높이). 같은 step이면 늘 같은 값(무작위 없음) + 지금 출렁임.
    float Sample(int line, int s)
    {
        float t = s / samplesPerSecond;
        float phase = line == 0 ? 0f : 2.1f;          // 두 선이 같은 모양으로 겹치지 않게
        float wave = 0.6f * Mathf.Sin(t * 0.9f + phase) + 0.4f * Mathf.Sin(t * 2.3f + phase * 1.7f);
        float jitter = Hash(s * 2 + line) * 2f - 1f;  // −1 ~ 1
        float v = (line == 0 ? redBase : blueBase) + wave * waveDots + jitter * jitterDots + Spike;
        return Mathf.Clamp(v, 2f, height - 3f);       // 영역 밖으로 나가지 않게(C4)
    }

    // 정수 → 0~1의 규칙적인 값(같은 입력이면 같은 출력). 셰이더에서 자주 쓰는 sin 해시.
    static float Hash(int n)
    {
        float x = Mathf.Sin(n * 12.9898f) * 43758.5453f;
        return x - Mathf.Floor(x);
    }

    void Draw()
    {
        Array.Clear(pixels, 0, pixels.Length);        // 투명으로 비움(뒤 방 그림이 보이게)
        DrawLine(blue, blueLine, blueShadow);
        DrawLine(red, redLine, redShadow);            // 빨강을 위에
        tex.SetPixels32(pixels);
        tex.Apply(false);
    }

    // 이웃 칸끼리 세로로 이어 끊김 없는 선(두께 새 도트 2개) + 바로 아래 한 줄 그림자
    void DrawLine(float[] values, Color32 line, Color32 shadow)
    {
        for (int x = 0; x < width; x++)
        {
            int a = Mathf.RoundToInt(values[x]);
            int b = Mathf.RoundToInt(values[Mathf.Min(x + 1, width - 1)]);
            int lo = Mathf.Min(a, b), hi = Mathf.Max(a, b) + 1;
            for (int y = lo; y <= hi; y++) Set(x, y, line);
            if (pixels[Index(x, lo - 1)].a == 0) Set(x, lo - 1, shadow);
        }
    }

    int Index(int x, int y) => Mathf.Clamp(y, 0, height - 1) * width + x;

    void Set(int x, int y, Color32 c)
    {
        if (y < 0 || y >= height) return;
        pixels[y * width + x] = c;
    }
}
