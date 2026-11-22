using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

// 덱 공개 연출 (ui-revamp 31): 덱을 채울 때 하락·상승 캔들을 한 줄로 놓아 개수를 보여 준 뒤,
// 뒷면으로 뒤집고 단말기 쪽으로 모아 사라지게 한다. 판정은 하지 않는다 — 시작·끝을 GameManager에 알릴 뿐.
public class DeckRevealView : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] GameObject root;              // 어둡게 + 캔들 줄 (monitor-trade 42: 안내 글자 없음)
    [SerializeField] RectTransform row;            // 캔들들이 놓이는 줄(가로 가운데)
    [SerializeField] Image candlePrefab;           // 캔들 한 개(꺼 둔 견본). 복사해서 쓴다
    [SerializeField] Sprite upSprite;
    [SerializeField] Sprite downSprite;
    [SerializeField] Sprite backSprite;
    [SerializeField] Vector2 gatherPoint = new Vector2(0f, 0f);   // row 기준 모이는 곳(단말기 쪽)

    [Header("시간 (제안 2: 라운드 시작 약 1.5초 / 재장전 약 1.0초)")]
    [SerializeField] float placeIntervalStart = 0.08f;
    [SerializeField] float placeIntervalReload = 0.05f;
    [SerializeField] float holdStart = 0.35f;
    [SerializeField] float holdReload = 0.2f;
    [SerializeField] float gatherSeconds = 0.25f;

    const int Spacing = 10;                        // 캔들 간격(도트)
    readonly List<Image> candles = new List<Image>();
    int lastFillCount = -1;
    Coroutine routine;

    public void Refresh(GameManager game)
    {
        // intro-story 77: 시작 스토리 중엔 연출을 미룸(lastFillCount를 안 바꿔 두면 끝난 뒤 Refresh 때 시작됨)
        if (game.IsIntro) return;
        int fill = game.Deck.FillCount;
        if (fill == lastFillCount) return;
        lastFillCount = fill;                      // 먼저 바꿔 둔다: 아래 BeginPresentation이 Refresh를 다시 불러도 두 번 시작하지 않게

        // 연출 도중 새로 채워지면(새 판 등) 앞 연출과 캔들 움직임을 모두 멈추고 다시 시작
        StopAllCoroutines();
        routine = StartCoroutine(Present(game.Deck.RemainingDown, game.Deck.RemainingUp, game.LastFillWasRoundStart));
    }

    IEnumerator Present(int down, int up, bool roundStart)
    {
        game.BeginPresentation();
        root.SetActive(true);
        Clear();

        float interval = roundStart ? placeIntervalStart : placeIntervalReload;
        float hold = roundStart ? holdStart : holdReload;
        int total = down + up;
        float left = -(total - 1) * Spacing / 2f;  // 가운데 정렬

        // ① 하락 먼저, 상승 다음으로 한 장씩 놓기 (제안 3: 개수 세기 쉽게. 실제 순서 아님)
        // monitor-trade 42 (#59): 글자(하락 n · 상승 m) 없이 캔들 색만 보여 준다
        for (int i = 0; i < total; i++)
        {
            Image c = Instantiate(candlePrefab, row);
            c.gameObject.SetActive(true);
            c.sprite = i < down ? downSprite : upSprite;
            candles.Add(c);
            Vector2 target = new Vector2(Mathf.Round(left + i * Spacing), 0f);
            StartCoroutine(Tween.MoveAnchored(c.rectTransform, target + new Vector2(0f, 6f), target, interval * 2f, Tween.EaseOutBack));
            yield return Wait(interval);            // 기다리는 중에도 건너뛰기를 본다
            if (Skipped()) { Finish(); yield break; }
        }

        // ② 잠깐 보여 주기
        yield return Wait(hold);
        if (Skipped()) { Finish(); yield break; }

        // ③ 뒤집기 → 섞는 중
        // (글자 없음 — 뒤집히는 모습만)
        foreach (Image c in candles) c.sprite = backSprite;
        yield return Wait(hold * 0.5f);
        if (Skipped()) { Finish(); yield break; }

        // ④ 단말기 쪽으로 모으기
        foreach (Image c in candles)
        {
            StartCoroutine(Tween.MoveAnchored(c.rectTransform, c.rectTransform.anchoredPosition, gatherPoint, gatherSeconds, Tween.EaseInCubic));
        }
        yield return Wait(gatherSeconds);
        Finish();
    }

    // 기다리는 동안에도 Space(건너뛰기)를 보고 바로 빠져나온다
    IEnumerator Wait(float seconds)
    {
        float t = 0f;
        while (t < seconds && !Skipped())
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    bool Skipped() => game.SkipRequested;

    void Finish()
    {
        StopAllCoroutines();
        Clear();
        root.SetActive(false);
        routine = null;
        game.EndPresentation();
    }

    void Clear()
    {
        foreach (Image c in candles) Destroy(c.gameObject);
        candles.Clear();
    }
}
