using System;
using TMPro;
using UnityEngine;

// 광대 말풍선 (clown-tell 80). 광대 머리 왼쪽에 짧은 대사를 띄운다. 판정은 하지 않는다.
// GameManager가 대사 번호(ClownLineCount)를 올리면 → 새 대사를 표에서 찾아 한 번에 보이고, holdSeconds 뒤 사라진다.
// 광대 폰 깜빡임(item-phone 64)과 같은 "번호가 바뀌었나" 방식: Refresh가 여러 번 불려도 한 번만 반응한다.
public class ClownSpeechView : MonoBehaviour
{
    [Serializable]
    public struct Entry
    {
        public ClownLine line;
        [TextArea(1, 2)] public string text;   // <color=#D9B45A>…</color> = 금색 강조
    }

    [SerializeField] AudioManager audioManager;
    [SerializeField] GameObject root;            // 말풍선 전체(틀·꼬리·글자). 켜고 끄는 단위
    [SerializeField] RectTransform box;          // 틀. 오른쪽 위가 기준(pivot 1,1) → 폭이 늘면 왼쪽으로 커짐(꼬리는 광대 쪽에 고정)
    [SerializeField] TMP_Text text;
    [SerializeField] float padding = 7f;         // 글자 양옆 여백(HUD 단위)
    [SerializeField] float holdSeconds = 1.5f;   // Grilling Q9
    [SerializeField, Range(0f, 1f)] float soundVolume = 0.3f;   // Q10: 클릭음 30% 한 번

    // 대사 표 (Grilling Q6). 같은 상황엔 늘 같은 대사
    [SerializeField] Entry[] entries =
    {
        new Entry { line = ClownLine.Buy,        text = "<color=#D9B45A>매수</color>. 들고 간다." },
        new Entry { line = ClownLine.Sell,       text = "<color=#D9B45A>매도</color>. 너한테 넘기지." },
        new Entry { line = ClownLine.UseChart,   text = "<color=#D9B45A>차트 분석</color>을 쓰지." },
        new Entry { line = ClownLine.UseRumor,   text = "<color=#D9B45A>찌라시</color>를 쓰지." },
        new Entry { line = ClownLine.UseHalt,    text = "<color=#D9B45A>거래 정지</color>를 쓰지." },
        new Entry { line = ClownLine.UseLottery, text = "<color=#D9B45A>복권</color>을 쓰지." },
        new Entry { line = ClownLine.Halted,     text = "…한 번 쉬지." },
        // clown-tell 81: 결과 대사
        new Entry { line = ClownLine.OneMore,         text = "한 번 더." },
        new Entry { line = ClownLine.Hurt,            text = "…칫." },
        new Entry { line = ClownLine.Gotcha,          text = "잘 받았지?" },
        new Entry { line = ClownLine.NothingHappened, text = "흥. 아무 일도 없군." },
        new Entry { line = ClownLine.Laugh,           text = "하하." },
        new Entry { line = ClownLine.ChartDone,       text = "…그렇군." },
        new Entry { line = ClownLine.RumorDone,       text = "다들 봤지?" },
        new Entry { line = ClownLine.HaltDone,        text = "넌 한 번 쉬어." },
        new Entry { line = ClownLine.LotteryHeal,     text = "운이 좋군." },
        new Entry { line = ClownLine.LotteryLeverage, text = "판을 키우지." },
        new Entry { line = ClownLine.LotteryItem,     text = "하나 더." },
        new Entry { line = ClownLine.LotteryNothing,  text = "…꽝이군." },
        // heal-items 90
        new Entry { line = ClownLine.UseDividend,     text = "<color=#D9B45A>배당금</color>을 받지." },
        new Entry { line = ClownLine.UseBailout,      text = "<color=#D9B45A>구제 금융</color>을 쓰지." },
        new Entry { line = ClownLine.Healed,          text = "숨 좀 돌리지." },
    };

    int lastCount = -1;
    float hideAt;

    public bool IsShowing => root.activeSelf;
    public string ShownText => text.text;

    void Awake()
    {
        root.SetActive(false);
    }

    public void Refresh(GameManager game)
    {
        if (game.ClownLineCount == lastCount) return;   // 새 대사가 없으면 그대로
        lastCount = game.ClownLineCount;

        string line = Find(game.LastClownLine);
        if (line == null)                               // None(또는 표에 없음) = 말풍선 닫기
        {
            root.SetActive(false);
            return;
        }

        text.text = line;
        // 틀 폭 = 글자 폭 + 양옆 여백. 태그(<color>)는 폭에 안 들어감(TMP가 빼고 잼)
        float width = text.GetPreferredValues(line).x + padding * 2f;
        box.sizeDelta = new Vector2(Mathf.Ceil(width), box.sizeDelta.y);   // 도트에 맞게 올림
        root.SetActive(true);
        hideAt = Time.time + holdSeconds;
        if (audioManager != null) audioManager.PlayClick(soundVolume);
    }

    // 보인 뒤 holdSeconds가 지나면 닫음(일시정지 중엔 Time.time이 멈춰 그대로 남음)
    void Update()
    {
        if (root.activeSelf && Time.time >= hideAt) root.SetActive(false);
    }

    string Find(ClownLine line)
    {
        if (line == ClownLine.None) return null;
        foreach (Entry e in entries)
            if (e.line == line) return e.text;
        return null;
    }
}
