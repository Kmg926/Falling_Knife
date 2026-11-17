using System.Collections;
using TMPro;
using UnityEngine;

// GE코인 주식 모니터(stock-screen 40, 전에는 단말기): 공개되는 캔들,
// 차트 분석으로 아는 다음 캔들("다음" 창), 마지막 결과. GameManager 상태를 읽기만 한다(판정은 바꾸지 않음).
public class TerminalView : MonoBehaviour
{
    [SerializeField] TMP_Text resultText;         // 결과 글자(광대 왼쪽 벽)
    [SerializeField] TMP_Text titleText;          // 모니터 위 줄: 평소 "GE코인", 광대 공개 중 "광대 매수"/"광대 매도" (monitor-trade 43)
    [SerializeField] SpriteRenderer shownCandle;  // 공개 중·공개 뒤 캔들(모니터 가운데 기준선 위에 놓임)
    [SerializeField] SpriteRenderer knownCandle;  // "다음" 창 안: 차트 분석으로 아는 다음 캔들(플레이어만)
    [SerializeField] TMP_Text questionMark;       // "다음" 창의 "?" 글자
    [SerializeField] Sprite upSprite;             // "다음" 창용 작은 캔들
    [SerializeField] Sprite downSprite;
    [Header("모니터 큰 캔들 (stock-screen 40)")]
    [SerializeField] Sprite shownUpSprite;        // 기준점이 아래 → 세로 크기를 키우면 기준선에서 위로 자람
    [SerializeField] Sprite shownDownSprite;      // 기준점이 위 → 기준선에서 아래로 자람

    [Header("출렁이다 굳는 공개 (stock-screen 47)")]
    [SerializeField] float wobbleShare = 0.8f;    // 공개 시간 중 출렁이는 몫(나머지 0.2 = 굳기)
    [SerializeField] float wobbleAmount = 0.7f;   // 출렁임 최대 = 캔들 길이의 70%
    [SerializeField] SpriteRenderer flashOutline; // 공개 캔들의 자식: 굳는 순간 하얀 테두리
    [SerializeField] Sprite flashUp;              // 테두리(기준점 아래 = 상승 캔들과 같음)
    [SerializeField] Sprite flashDown;            // 테두리(기준점 위 = 하락 캔들과 같음)
    [SerializeField] SpriteRenderer baselineFlash;// 기준선을 잠깐 밝게
    [SerializeField] float flashSeconds = 0.1f;
    Coroutine flashRoutine;

    bool wasRevealing;
    Coroutine drawRoutine;

    public void Refresh(GameManager game)
    {
        // monitor-trade 42 (#59): 남은 개수·덱 구성은 숫자로 보여 주지 않는다(덱을 채울 때 캔들 색으로만).

        // monitor-trade 43 (Spec 제안 8): 광대는 모니터를 열지 않으므로, 광대가 고른 행동을 공개 동안 내 모니터 위 줄에 보여 준다.
        // 광대가 무엇을 골랐는지는 결과 글자에도 나오므로 공개 전에 보여도 규칙 문제 없음(결과는 캔들이 정함).
        bool clownTrading = game.IsRevealing && game.Current == game.Clown;
        titleText.text = clownTrading ? (game.Selected == PlayerAction.Buy ? "광대 매수" : "광대 매도") : "GE코인";

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
            // stock-screen 47: 공개가 끝나는 순간 = 결과로 굳는 순간 → 한 번 번쩍(그리기 코루틴과 따로라 위에서 멈춰도 안 끊김)
            if (wasRevealing && game.ShownCandle.HasValue)
            {
                if (flashRoutine != null) StopCoroutine(flashRoutine);
                flashRoutine = StartCoroutine(Flash(game.ShownCandle.Value));
            }
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
    // stock-screen 47: 실시간으로 만들어지는 캔들.
    // 앞 80%: 가격(캔들 끝)이 기준선 위·아래로 똑같이 출렁임 → 보고 있어도 결과를 알 수 없음.
    // 뒤 20%: 출렁이던 자리에서 결과(상승 = 위 끝까지, 하락 = 아래 끝까지)로 부드럽게 굳음.
    IEnumerator DrawCandle(Candle? candle, float seconds)
    {
        if (!candle.HasValue) yield break;
        int rows = shownUpSprite != null ? Mathf.RoundToInt(shownUpSprite.rect.height) : 36;   // 그림 높이(새 도트 줄 수)
        float wobbleEnd = seconds * wobbleShare;
        float phaseA = Random.value * Mathf.PI * 2f;   // 매번 다른 출렁임 → 마지막 출렁임 방향으로 결과를 짐작할 수 없게
        float phaseB = Random.value * Mathf.PI * 2f;
        float target = candle.Value == Candle.Up ? 1f : -1f;
        float settleFrom = Wobble(1f, phaseA, phaseB);  // 출렁임이 끝나는 순간의 가격(여기서 굳기 시작)
        float t = 0f;
        while (t < seconds)
        {
            float price = t < wobbleEnd
                ? Wobble(t / wobbleEnd, phaseA, phaseB)
                : Mathf.Lerp(settleFrom, target, Tween.EaseOutCubic((t - wobbleEnd) / (seconds - wobbleEnd)));
            ShowPrice(price, rows);
            t += Time.deltaTime;
            yield return null;
        }
        ShowCandle(shownCandle, candle, 1f, shownUpSprite, shownDownSprite);
        drawRoutine = null;
    }

    // 출렁임: 사인 두 개를 섞어(빠르기 다름) 위아래가 같은 크기로 오감. u = 0~1(출렁이는 시간 중 지난 몫)
    float Wobble(float u, float phaseA, float phaseB)
    {
        float w = 0.6f * Mathf.Sin(u * 2f * Mathf.PI * 1.6f + phaseA) + 0.4f * Mathf.Sin(u * 2f * Mathf.PI * 3.1f + phaseB);
        return w * wobbleAmount;
    }

    // 가격(−1 ~ 1, 위가 +) → 기준선에서 그만큼의 캔들. 위면 상승 그림(기준점 아래), 아래면 하락 그림(기준점 위)
    // 줄 수로 끊어(새 도트 1줄 단위) 흐려지지 않게.
    void ShowPrice(float price, int rows)
    {
        int shown = Mathf.Clamp(Mathf.RoundToInt(Mathf.Abs(price) * rows), 1, rows);
        shownCandle.enabled = true;
        shownCandle.sprite = price >= 0f ? shownUpSprite : shownDownSprite;
        shownCandle.transform.localScale = new Vector3(1f, (float)shown / rows, 1f);
    }

    // 굳는 순간 0.1초: 캔들 하얀 테두리 + 기준선 밝게(한 번)
    IEnumerator Flash(Candle candle)
    {
        if (flashOutline != null)
        {
            flashOutline.sprite = candle == Candle.Up ? flashUp : flashDown;
            flashOutline.enabled = true;
        }
        if (baselineFlash != null) baselineFlash.enabled = true;
        yield return new WaitForSeconds(flashSeconds);
        if (flashOutline != null) flashOutline.enabled = false;
        if (baselineFlash != null) baselineFlash.enabled = false;
        flashRoutine = null;
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