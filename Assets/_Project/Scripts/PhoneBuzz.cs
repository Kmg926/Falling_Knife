using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// item-phone 65: 아이템을 받으면 테이블 위 접힌 폰이 울린다. 읽기만 한다(판정 없음).
// - GameManager의 "받았다" 신호(내 폰 = PlayerGrantCount, 광대 폰 = ClownGrantCount)가 바뀌면
//   덱 공개 연출(라운드 안내)이 끝나길 기다렸다가: 진동음 1번 + 0.6초 동안 좌우 ±1 흔들림 + 바깥 화면 "+N" 1.2초 깜빡임
// - 깜빡이는 동안(Busy)은 폰 그림을 이 컴포넌트가 맡고, 끝나면 OnDone으로 원래 개수 그림을 다시 그리게 한다
// - WaitForSeconds라 일시정지(timeScale 0) 중엔 같이 멈춘다
public class PhoneBuzz : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] AudioManager audioManager;
    [SerializeField] bool clown;               // 광대 폰이면 켬(광대 "받았다" 신호를 봄)
    [SerializeField] Image phoneImage;
    [SerializeField] Sprite blankSprite;       // 깜빡임 꺼진 순간: 빈 화면
    [SerializeField] Sprite[] plusSprites;     // [0] = "+1" … [3] = "+4"

    [Header("Spec 제안 5")]
    [SerializeField] float shakeSeconds = 0.6f;
    [SerializeField] float stepSeconds = 0.05f;   // 흔들림 한 번 = 0.05초
    [SerializeField] float blinkSeconds = 1.2f;
    [SerializeField] float blinkStep = 0.2f;      // "+N" 0.2초 켜짐 / 0.2초 꺼짐

    public bool Busy { get; private set; }
    public Action OnDone;                          // 끝나면 부를 일(뷰가 개수 그림을 다시 그림)

    RectTransform rt;
    Vector2 home;
    int lastCount = -1;                            // 처음 Refresh 때 지금 값으로 맞춤

    void Awake()
    {
        rt = (RectTransform)transform;
        home = rt.anchoredPosition;
    }

    public void Refresh(GameManager game)
    {
        int count = clown ? game.ClownGrantCount : game.PlayerGrantCount;
        if (lastCount < 0)
        {
            // 첫 화면: 라운드 1 지급은 이미 일어난 뒤라 그 신호도 울리게 0부터 비교
            lastCount = 0;
        }
        if (count == lastCount) return;
        lastCount = count;
        int n = clown ? game.ClownLastGrant : game.PlayerLastGrant;
        if (n <= 0 || !isActiveAndEnabled) return;
        StopAllCoroutines();
        StartCoroutine(Buzz(n));
    }

    IEnumerator Buzz(int n)
    {
        Busy = true;
        rt.anchoredPosition = home;
        // 라운드 시작·재장전 때는 덱 공개 연출(라운드 안내)이 끝난 뒤에(겹치지 않게)
        // intro-story 82: 시작 스토리 중에도 기다림(스토리가 끝나면 바로 덱 공개 연출이 시작되므로 그것까지 기다리게 됨)
        while (game.IsIntro || game.IsPresenting) yield return null;

        if (audioManager != null) audioManager.PlayBuzz();
        Sprite plus = plusSprites[Mathf.Clamp(n, 1, plusSprites.Length) - 1];
        float t = 0f;
        int step = 0;
        float total = Mathf.Max(shakeSeconds, blinkSeconds);
        while (t < total)
        {
            // 흔들림: 0.6초 동안 +1, −1 번갈아
            rt.anchoredPosition = home + (t < shakeSeconds ? new Vector2(step % 2 == 0 ? 1f : -1f, 0f) : Vector2.zero);
            // "+N" 깜빡임: 0.2초 켜짐 / 0.2초 꺼짐
            bool on = Mathf.FloorToInt(t / blinkStep) % 2 == 0;
            phoneImage.sprite = on ? plus : blankSprite;
            yield return new WaitForSeconds(stepSeconds);
            t += stepSeconds;
            step++;
        }
        rt.anchoredPosition = home;
        Busy = false;
        OnDone?.Invoke();
    }

    // 꺼졌다 켜질 때(씬 전환 등) 흔들린 자리에 남지 않게
    void OnDisable()
    {
        if (rt != null) rt.anchoredPosition = home;
        Busy = false;
    }
}
