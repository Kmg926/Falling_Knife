using System.Collections;
using TMPro;
using UnityEngine;

// 오른쪽 위 "ROUND 1/3" + "내 턴/광대 턴", 가운데 안내(라운드 클리어), 턴이 바뀔 때 가운데 띠(ui-revamp 30). 읽기만 한다.
public class RoundTurnView : MonoBehaviour
{
    [SerializeField] TMP_Text roundText;
    [SerializeField] TMP_Text turnText;
    [SerializeField] TMP_Text noticeText;
    [SerializeField] Color myTurnColor = new Color32(0x3F, 0x8F, 0x55, 255);     // 팔레트 green
    [SerializeField] Color clownTurnColor = new Color32(0xC4, 0x3A, 0x3A, 255);  // 팔레트 red3

    [Header("턴 띠 (제안 5: 0.8초, 입력 막지 않음)")]
    [SerializeField] RectTransform banner;       // 띠 전체(바둑판 + 글자). 평소엔 화면 밖 왼쪽
    [SerializeField] TMP_Text bannerText;
    [SerializeField] float slideSeconds = 0.2f;  // 들어오기·나가기 각각
    [SerializeField] float holdSeconds = 0.4f;   // 가운데 멈춤 (0.2 + 0.4 + 0.2 = 0.8초)

    [Header("라운드 클리어 판 (round-feedback 99)")]
    [SerializeField] GameObject clearPanel;      // 뒤 어둡게 + 가운데 판(제목·SPACE 키·안내)
    [SerializeField] TMP_Text clearTitle;        // "라운드 1 클리어"
    [SerializeField] CanvasGroup clearHint;      // SPACE 키 + "다음 라운드"(깜빡임)
    [SerializeField] float hintBlinkSeconds = 1.2f;   // 밝음 → 흐림 → 밝음 한 번

    bool? lastMine;          // 지난번 Refresh 때 내 차례였나. 처음엔 값 없음(null) → 첫 화면에서는 띠 안 띄움
    Coroutine bannerRoutine;

    public void Refresh(GameManager game)
    {
        roundText.text = game.IsTutorial ? "TUTORIAL" : $"ROUND {game.RoundNumber}/{game.RoundCount}";   // tutorial 97

        bool mine = game.Current == game.Player;
        turnText.text = mine ? "내 턴" : "광대 턴";
        turnText.color = mine ? myTurnColor : clownTurnColor;

        // 차례가 바뀐 순간에만 띠 (라운드가 끝난 화면은 제외)
        if (lastMine.HasValue && lastMine.Value != mine && !game.IsRoundOver)
        {
            if (bannerRoutine != null) StopCoroutine(bannerRoutine);
            bannerRoutine = StartCoroutine(BannerRoutine(mine));
        }
        lastMine = mine;

        // 한 판의 끝(파산·생존)은 결과 화면(Ticket 28)이 보여 준다. 여기는 라운드 클리어만.
        // 99: 금색 글자만 광대 얼굴 위에 떠서 안 보임 → 가운데 판(clearPanel)으로. 판이 연결 안 됐으면 전처럼 글자
        bool cleared = game.IsRoundCleared;
        if (clearPanel != null)
        {
            noticeText.text = "";
            clearPanel.SetActive(cleared);
            if (cleared) clearTitle.text = $"라운드 {game.RoundNumber} 클리어";
        }
        else noticeText.text = cleared ? $"라운드 {game.RoundNumber} 클리어! Space로 다음 라운드" : "";
    }

    // 99: 클리어 판이 떠 있는 동안 "SPACE 다음 라운드"가 천천히 밝아졌다 흐려졌다(1 ↔ 0.35)
    void Update()
    {
        if (clearPanel == null || !clearPanel.activeSelf || clearHint == null) return;
        float t = Mathf.PingPong(Time.unscaledTime * 2f / hintBlinkSeconds, 1f);   // 0 → 1 → 0
        clearHint.alpha = Mathf.Lerp(1f, 0.35f, t);
    }

    // 왼쪽 밖 → 가운데(빠르게 들어와 천천히 멈춤) → 잠깐 → 오른쪽 밖(천천히 시작해 빠르게 나감)
    IEnumerator BannerRoutine(bool mine)
    {
        bannerText.text = mine ? "내 턴" : "광대 턴";
        bannerText.color = mine ? myTurnColor : clownTurnColor;
        float y = banner.anchoredPosition.y;
        yield return Tween.MoveAnchored(banner, new Vector2(-320f, y), new Vector2(0f, y), slideSeconds, Tween.EaseOutCubic);
        yield return new WaitForSeconds(holdSeconds);
        yield return Tween.MoveAnchored(banner, new Vector2(0f, y), new Vector2(320f, y), slideSeconds, Tween.EaseInCubic);
        bannerRoutine = null;
    }
}
