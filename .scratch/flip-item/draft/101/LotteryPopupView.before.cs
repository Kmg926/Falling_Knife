using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 내 복권 결과 연출 (round-feedback 100). 판정은 하지 않는다 — GameManager가 남긴 결과(LastLotteryOutcome·LastLotteryDetail)를 보여 줄 뿐.
// 폰에서 복권을 쓰면(폰이 닫힌 직후) 가운데 작은 판이 튀어나와 "복권 / 결과 한 줄"을 1.5초 보이고 사라진다.
// 당첨 = 금색 + 높은 소리, 꽝 = 흐린 회색 + 낮은 소리. 바뀐 곳(내 잔고·레버리지·내 폰)을 금색 틀로 세 번 반짝인다.
public class LotteryPopupView : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] AudioManager audioManager;
    [SerializeField] GameObject root;            // 판 전체(켜고 끄는 단위)
    [SerializeField] RectTransform panel;        // 튀어나오는 판(크기 연출)
    [SerializeField] TMP_Text resultText;        // "잔고 +1" 등
    [SerializeField] Color winColor = new Color32(0xD9, 0xB4, 0x5A, 255);    // gold2
    [SerializeField] Color loseColor = new Color32(0x77, 0x73, 0x6B, 255);   // 흐린 회색
    [SerializeField] float showSeconds = 1.5f;
    [SerializeField] float popSeconds = 0.15f;

    [Header("소리 (클릭음 높낮이만 바꿔 씀)")]
    [SerializeField] float winPitch = 1.6f;
    [SerializeField] float losePitch = 0.6f;
    [SerializeField, Range(0f, 1f)] float soundVolume = 0.8f;

    [Header("바뀐 곳 반짝임")]
    [SerializeField] RectTransform frame;        // 금색 틀(선 4개를 코드로 붙임)
    [SerializeField] RectTransform myBalanceArea;
    [SerializeField] RectTransform leverageArea;
    [SerializeField] RectTransform phoneArea;
    [SerializeField] float framePadding = 2f;

    int lastCount;
    Image[] edges;

    void Awake()
    {
        root.SetActive(false);
        BuildEdges();
        frame.gameObject.SetActive(false);
    }

    void Start() { lastCount = game.PlayerLotteryCount; }

    void Update()
    {
        if (game.PlayerLotteryCount == lastCount) return;
        lastCount = game.PlayerLotteryCount;
        StopAllCoroutines();
        StartCoroutine(Show());
    }

    IEnumerator Show()
    {
        LotteryPrize got = game.LastLotteryOutcome;          // 실제로 된 것(못 받으면 Nothing)
        bool won = got != LotteryPrize.Nothing;
        // "잔고 +1" / "레버리지 +1" / "아이템 +1 (찌라시)" / "꽝 (…)". 레버리지 기록의 "(현재 N)"은 화면에선 뺌(상자에 보이므로)
        string detail = game.LastLotteryDetail;
        resultText.text = got == LotteryPrize.Leverage ? "레버리지 +1" : detail;
        resultText.color = won ? winColor : loseColor;
        audioManager.PlayVoice(soundVolume, won ? winPitch : losePitch);

        root.SetActive(true);
        // 튀어나옴: 0.6 → 1.1 → 1.0 (EaseOutBack)
        for (float t = 0f; t < popSeconds; t += Time.deltaTime)
        {
            panel.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, Tween.EaseOutBack(t / popSeconds));
            yield return null;
        }
        panel.localScale = Vector3.one;

        RectTransform target = got == LotteryPrize.Heal ? myBalanceArea : got == LotteryPrize.Leverage ? leverageArea : got == LotteryPrize.Item ? phoneArea : null;
        float rest = showSeconds - popSeconds;
        if (target != null) yield return Blink(target, rest);
        else yield return new WaitForSeconds(rest);
        root.SetActive(false);
    }

    // 대상 위에 금색 틀을 세 번 깜빡임(켜짐·꺼짐 반복) — 시간은 남은 표시 시간에 맞춤
    IEnumerator Blink(RectTransform target, float seconds)
    {
        frame.position = target.TransformPoint(target.rect.center);
        frame.sizeDelta = target.rect.size * (target.lossyScale.x / frame.lossyScale.x) + Vector2.one * framePadding * 2f;
        frame.gameObject.SetActive(true);
        float step = seconds / 6f;
        for (int i = 0; i < 6; i++)
        {
            foreach (Image e in edges) e.enabled = i % 2 == 0;
            yield return new WaitForSeconds(step);
        }
        frame.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (root != null) root.SetActive(false);
        if (frame != null) frame.gameObject.SetActive(false);
    }

    // 금색 선 4개(튜토리얼 강조 틀과 같은 방법 — 그림 없는 단색 Image)
    void BuildEdges()
    {
        Vector2[,] a =
        {
            { new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1) },
            { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0) },
            { new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f) },
            { new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f) },
        };
        edges = new Image[4];
        for (int i = 0; i < 4; i++)
        {
            var go = new GameObject("Edge" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = frame.gameObject.layer;
            var r = (RectTransform)go.transform;
            r.SetParent(frame, false);
            r.anchorMin = a[i, 0]; r.anchorMax = a[i, 1]; r.pivot = a[i, 2];
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta = i < 2 ? new Vector2(0f, 1f) : new Vector2(1f, 0f);
            edges[i] = go.GetComponent<Image>();
            edges[i].color = winColor;
            edges[i].raycastTarget = false;
        }
    }
}
