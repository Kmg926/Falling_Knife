using System.Collections;
using TMPro;
using UnityEngine;

// GE코인 단말기: 남은 개수(또는 방금 채운 덱 구성), 공개되는 캔들, 차트 분석으로 아는 다음 캔들, 마지막 결과 한 줄.
// GameManager 상태를 읽기만 한다(판정은 바꾸지 않음).
public class TerminalView : MonoBehaviour
{
    [SerializeField] TMP_Text remainingText;      // "남은 5" 또는 "하락 2 · 상승 3"
    [SerializeField] TMP_Text resultText;         // 단말기 아래 결과 한 줄
    [SerializeField] SpriteRenderer shownCandle;  // 공개 중·공개 뒤 캔들
    [SerializeField] SpriteRenderer knownCandle;  // ? 칸 안: 차트 분석으로 아는 다음 캔들(플레이어만)
    [SerializeField] TMP_Text questionMark;       // ? 칸의 "?" 글자
    [SerializeField] Sprite upSprite;
    [SerializeField] Sprite downSprite;

    bool wasRevealing;
    Coroutine drawRoutine;

    public void Refresh(GameManager game)
    {
        CandleDeck deck = game.Deck;
        remainingText.text = deck.JustFilled
            ? $"하락{deck.RemainingDown} 상승{deck.RemainingUp}"   // ui-revamp 30: 좁은 칸에 9 이상 크기로 들어가게 짧게
            : $"남은 {deck.Remaining}";

        // 공개가 막 시작된 순간(전에는 아니었는데 지금은 공개 중) → 캔들 그리기 시작
        if (game.IsRevealing && !wasRevealing)
        {
            if (drawRoutine != null) StopCoroutine(drawRoutine);
            drawRoutine = StartCoroutine(DrawCandle(game.ShownCandle, game.RevealSeconds));
        }
        // 공개 중이 아니면 그리기를 멈추고 다 그린 모양으로
        else if (!game.IsRevealing)
        {
            if (drawRoutine != null) StopCoroutine(drawRoutine);
            drawRoutine = null;
            ShowCandle(shownCandle, game.ShownCandle, 1f);
        }
        wasRevealing = game.IsRevealing;

        // 차트 분석: 알면 ? 대신 그 캔들
        ShowCandle(knownCandle, game.PlayerKnownNext, 1f);
        questionMark.enabled = !game.PlayerKnownNext.HasValue;

        // 공개 중에는 비우고, 끝나면 결과
        resultText.text = game.IsRevealing ? "" : game.LastResult;
    }

    // 캔들을 위에서부터 한 줄씩 seconds 동안 그린다. 높이를 정수 줄로 끊어 도트가 흐려지지 않게.
    IEnumerator DrawCandle(Candle? candle, float seconds)
    {
        int rows = 18;   // 캔들 그림 높이(도트)
        float t = 0f;
        while (t < seconds)
        {
            int shown = Mathf.Clamp(Mathf.CeilToInt(rows * t / seconds), 1, rows);
            ShowCandle(shownCandle, candle, (float)shown / rows);
            t += Time.deltaTime;
            yield return null;
        }
        ShowCandle(shownCandle, candle, 1f);
        drawRoutine = null;
    }

    // 캔들이 없으면 숨기고, 있으면 그 그림을 heightRatio(0~1) 높이만큼 보인다(기준점이 위라 위에서 아래로 자람).
    void ShowCandle(SpriteRenderer target, Candle? candle, float heightRatio)
    {
        target.enabled = candle.HasValue;
        if (!candle.HasValue) return;
        target.sprite = candle.Value == Candle.Up ? upSprite : downSprite;
        target.transform.localScale = new Vector3(1f, heightRatio, 1f);
    }
}