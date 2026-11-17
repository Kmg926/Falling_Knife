using System.Collections;
using TMPro;
using UnityEngine;

// GE코인 주식 모니터(stock-screen 40, 전에는 단말기): 공개되는 캔들,
// 차트 분석으로 아는 다음 캔들("다음" 창), 마지막 결과. GameManager 상태를 읽기만 한다(판정은 바꾸지 않음).
public class TerminalView : MonoBehaviour
{
    [SerializeField] TMP_Text resultText;         // 결과 글자(광대 왼쪽 벽)
    [SerializeField] SpriteRenderer shownCandle;  // 공개 중·공개 뒤 캔들(모니터 가운데 기준선 위에 놓임)
    [SerializeField] SpriteRenderer knownCandle;  // "다음" 창 안: 차트 분석으로 아는 다음 캔들(플레이어만)
    [SerializeField] TMP_Text questionMark;       // "다음" 창의 "?" 글자
    [SerializeField] Sprite upSprite;             // "다음" 창용 작은 캔들
    [SerializeField] Sprite downSprite;
    [Header("모니터 큰 캔들 (stock-screen 40)")]
    [SerializeField] Sprite shownUpSprite;        // 기준점이 아래 → 세로 크기를 키우면 기준선에서 위로 자람
    [SerializeField] Sprite shownDownSprite;      // 기준점이 위 → 기준선에서 아래로 자람

    bool wasRevealing;
    Coroutine drawRoutine;

    public void Refresh(GameManager game)
    {
        // monitor-trade 42 (#59): 남은 개수·덱 구성은 숫자로 보여 주지 않는다(덱을 채울 때 캔들 색으로만).

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
            ShowCandle(shownCandle, game.ShownCandle, 1f, shownUpSprite, shownDownSprite);
        }
        wasRevealing = game.IsRevealing;

        // 차트 분석: 알면 ? 대신 그 캔들
        ShowCandle(knownCandle, game.PlayerKnownNext, 1f, upSprite, downSprite);
        questionMark.enabled = !game.PlayerKnownNext.HasValue;

        // 공개 중에는 비우고, 끝나면 결과
        resultText.text = game.IsRevealing ? "" : game.LastResult;
    }

    // 캔들을 기준선에서 한 줄씩 seconds 동안 키운다(상승은 위로, 하락은 아래로 — 그림의 기준점이 정함).
    // 높이를 그림의 도트 줄 수로 끊어(새 도트 1줄씩) 반 칸에 걸쳐 흐려지지 않게.
    IEnumerator DrawCandle(Candle? candle, float seconds)
    {
        Sprite look = candle == Candle.Up ? shownUpSprite : shownDownSprite;
        int rows = look != null ? Mathf.RoundToInt(look.rect.height) : 36;   // 그림 높이(새 도트 줄 수)
        float t = 0f;
        while (t < seconds)
        {
            int shown = Mathf.Clamp(Mathf.CeilToInt(rows * t / seconds), 1, rows);
            ShowCandle(shownCandle, candle, (float)shown / rows, shownUpSprite, shownDownSprite);
            t += Time.deltaTime;
            yield return null;
        }
        ShowCandle(shownCandle, candle, 1f, shownUpSprite, shownDownSprite);
        drawRoutine = null;
    }

    // 캔들이 없으면 숨기고, 있으면 up/down 그림을 heightRatio(0~1) 높이만큼 보인다.
    // 세로 크기만 바꾸므로 그림의 기준점(상승 = 아래, 하락 = 위)은 제자리 → 그쪽 끝이 기준선에 붙은 채 자란다.
    void ShowCandle(SpriteRenderer target, Candle? candle, float heightRatio, Sprite up, Sprite down)
    {
        target.enabled = candle.HasValue;
        if (!candle.HasValue) return;
        target.sprite = candle.Value == Candle.Up ? up : down;
        target.transform.localScale = new Vector3(1f, heightRatio, 1f);
    }
}