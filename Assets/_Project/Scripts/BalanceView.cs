using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 잔고 전광판의 한 줄(balance-board 112, 2026-11-27). 위 줄 = 광대, 아래 줄 = 나 — Scene에 둘 두고 isPlayer로 구분. 읽기만 한다.
// 줄마다: 7세그먼트 숫자 2자리(그림 바꿔 끼움, 몸통 그림에 꺼진 "88"이 비침) + 칸 막대(잔고 1 = 1칸, 칸 수 = 라운드 시작 잔고)
//         + "▼n"/"▲n" 변화 글자 + "정지"(거래 정지 당함).
// - 잔고가 줄면: 잃은 칸이 빨강으로 깜빡이다 꺼지고, "▼n"이 잠깐. 늘면(회복): 새 칸이 밝게 깜빡이다 켜지고 "▲n".
// - 내 잔고 3 이하: 숫자가 빨강으로 천천히 깜빡임(긴장 색 — 전처럼 플레이어 쪽만, 12주차 Ticket 22).
// (전: 매트 위 동전 탑 + "7/10" 글자, 줄면 동전이 떨어짐)
public class BalanceView : MonoBehaviour
{
    [SerializeField] bool isPlayer;

    [Header("전광판 한 줄")]
    [SerializeField] Image tensDigit;          // 십의 자리(10 이상일 때만)
    [SerializeField] Image onesDigit;          // 일의 자리
    [SerializeField] Image[] cells;            // 칸 10개(왼쪽부터)
    [SerializeField] TMP_Text deltaText;       // "▼2" / "▲1"
    [SerializeField] TMP_Text haltText;        // "정지"

    [Header("그림")]
    [SerializeField] Sprite[] digits;          // 0~9 평소 색
    [SerializeField] Sprite[] dangerDigits;    // 0~9 빨강
    [SerializeField] Sprite cellOn;
    [SerializeField] Sprite cellOff;
    [SerializeField] Sprite cellHit;           // 잃는 칸 깜빡임
    [SerializeField] Sprite cellHeal;          // 회복 칸 깜빡임

    [Header("연출")]
    [SerializeField] int dangerBalance = 3;
    [SerializeField] float flashSeconds = 0.6f;     // 칸 깜빡임 시간
    [SerializeField] float flashStep = 0.1f;        // 켜짐·꺼짐 한 번
    [SerializeField] float deltaSeconds = 1.2f;     // "▼n" 보이는 시간
    [SerializeField] float dangerPeriod = 1.2f;     // 위험 깜빡임 한 주기(빨강 0.75 · 평소 0.25)
    [SerializeField] Color downColor = new Color32(0xE4, 0x6A, 0x58, 255);   // red4
    [SerializeField] Color upColor = new Color32(0x6C, 0xC2, 0x7F, 255);     // green2

    Participant who;
    Participant lastWho;       // 라운드가 바뀌면 다른 사람(새 Participant) → 깜빡임 없이 새 값
    int lastBalance;
    int flashLo, flashHi;      // 깜빡이는 칸 [lo, hi)
    bool flashDown;            // true = 잃은 칸(빨강), false = 회복 칸
    float flashUntil = -1f, deltaUntil = -1f;

    public void Refresh(GameManager game)
    {
        who = isPlayer ? game.Player : game.Clown;
        if (who == null) return;
        if (who == lastWho && who.Balance != lastBalance)
        {
            int diff = who.Balance - lastBalance;
            flashDown = diff < 0;
            flashLo = Mathf.Min(who.Balance, lastBalance);
            flashHi = Mathf.Max(who.Balance, lastBalance);
            flashUntil = Time.time + flashSeconds;
            deltaUntil = Time.time + deltaSeconds;
            if (deltaText != null)
            {
                deltaText.text = (flashDown ? "▼" : "▲") + Mathf.Abs(diff);
                deltaText.color = flashDown ? downColor : upColor;
            }
        }
        lastWho = who;
        lastBalance = who.Balance;
        Draw();
    }

    void Update()
    {
        if (who != null) Draw();   // 깜빡임은 시간에 따라 바뀌므로 매 프레임(그림 몇 장만 바꿈)
    }

    void Draw()
    {
        int b = Mathf.Max(0, who.Balance);
        int cap = Mathf.Min(who.StartingBalance, cells.Length);
        float now = Time.time;

        // 숫자: 위험이면 빨강으로 천천히 깜빡임
        bool danger = isPlayer && b <= dangerBalance;
        bool red = danger && (now % dangerPeriod) < dangerPeriod * 0.75f;
        Sprite[] set = red ? dangerDigits : digits;
        tensDigit.enabled = b >= 10;
        if (b >= 10) tensDigit.sprite = set[(b / 10) % 10];
        onesDigit.sprite = set[b % 10];

        // 칸: 시작 잔고만큼 보이고, 남은 만큼 켜짐. 방금 바뀐 칸은 깜빡임
        bool flashing = now < flashUntil;
        bool blinkOn = (now % (flashStep * 2f)) < flashStep;
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].enabled = i < cap;
            if (i >= cap) continue;
            if (flashing && i >= flashLo && i < flashHi)
                cells[i].sprite = blinkOn ? (flashDown ? cellHit : cellHeal) : cellOff;
            else
                cells[i].sprite = i < b ? cellOn : cellOff;
        }

        // 작은 기계라 "▼n"과 "정지"가 한 자리를 나눠 씀 — 변화 글자가 떠 있는 동안은 그쪽이 먼저
        bool showDelta = now < deltaUntil;
        if (deltaText != null) deltaText.enabled = showDelta;
        if (haltText != null) haltText.enabled = who.IsHalted && !showDelta;
    }
}
