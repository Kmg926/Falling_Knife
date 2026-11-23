using System.Collections.Generic;
using UnityEngine;

// 캔들 한 장의 종류. D = Down(하락), U = Up(상승)
public enum Candle { Down, Up }

public class CandleDeck : MonoBehaviour
{
    [Header("테스트용")]
    [SerializeField] bool useFixedOrder = false;

    // 이번 라운드의 총 장수 범위와 고정 순서. GameManager가 SetComposition으로 넘겨준다.
    int minCards;
    int maxCards;
    string fixedOrder = "";

    // 0번이 덱 맨 위 (다음에 공개될 캔들)
    readonly List<Candle> cards = new List<Candle>();

    public int RemainingDown { get; private set; }
    public int RemainingUp { get; private set; }
    public bool IsEmpty => cards.Count == 0;
    // 남은 장수 (단말기 `남은 N`)                                                      // ← 25 추가
    public int Remaining => cards.Count;                                                // ← 25 추가
    // 방금 채웠고 아직 한 장도 안 뽑았으면 true → 단말기가 하락/상승 개수를 보여 준다(#40) // ← 25 추가
    public bool JustFilled { get; private set; }                                        // ← 25 추가
    // 지금까지 덱을 채운 횟수 (ui-revamp 31). 화면이 "방금 새로 채워졌다"를 놓치지 않고 알아차리는 데 쓴다
    public int FillCount { get; private set; }

    // 라운드 시작 때 GameManager가 부른다. 저장해 둔 값은 재장전(Fill)도 그대로 쓴다.
    public void SetComposition(int min, int max, string order)
    {
        // 안전장치: 하락·상승을 1장씩 넣으려면 최소 2장이 필요하다.
        if (min < 2)
        {
            Debug.LogWarning($"최소 장수 {min}은(는) 너무 작아 2로 맞춥니다.");
            min = 2;
        }
        if (max < min)
        {
            Debug.LogWarning($"최대 장수 {max}이(가) 최소 장수 {min}보다 작아 {min}으로 맞춥니다.");
            max = min;
        }

        minCards = min;
        maxCards = max;
        fixedOrder = order ?? "";
    }

    // 라운드 시작과 재장전 때 부른다. 부를 때마다 구성을 새로 뽑는다.
    // tutorial 97: 튜토리얼 판은 Inspector 값과 상관없이 고정 순서(각본)
    public bool ForceFixedOrder { get; set; }

    public void Fill()
    {
        if (!(useFixedOrder || ForceFixedOrder) || !TryFillFixedOrder())
        {
            FillRandom();
        }
        Recount();
        JustFilled = true;                                                              // ← 25 추가
        FillCount++;
        Debug.Log("덱 순서(위→아래): " + string.Join(" ", cards));
    }

    // 맨 위 한 장을 꺼내 공개한다. 비어 있을 때 부르면 안 된다(IsEmpty 먼저 확인).
    public Candle Draw()
    {
        Candle top = cards[0];
        cards.RemoveAt(0);
        Recount();
        JustFilled = false;                                                             // ← 25 추가
        return top;
    }

    // flip-item 101: 맨 위 한 장을 반대로(하락 ↔ 상승). 개수도 다시 셈. 비어 있으면 아무 일 없음
    public void FlipTop()
    {
        if (cards.Count == 0) return;
        cards[0] = cards[0] == Candle.Down ? Candle.Up : Candle.Down;
        Recount();
    }

    // 맨 위 한 장을 보기만 한다(꺼내지 않음). 차트 분석용. 비어 있을 때 부르면 안 된다.
    public Candle Peek()
    {
        return cards[0];
    }

    // 총 장수와 하락 수를 뽑아 채우고 섞는다.
    // Random.Range(int, int)는 끝 값을 포함하지 않는다 → 끝에 +1, 하락은 1 ~ total-1.
    void FillRandom()
    {
        int total = Random.Range(minCards, maxCards + 1);
        int down = Random.Range(1, total);
        int up = total - down;

        cards.Clear();
        for (int i = 0; i < down; i++) cards.Add(Candle.Down);
        for (int i = 0; i < up; i++) cards.Add(Candle.Up);

        // Fisher-Yates: 뒤에서부터 한 장씩, 자기 자리 또는 앞쪽 아무 자리와 맞바꾼다
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Candle temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }
    }

    // 문자열이 빈칸이 아니고 D/U/공백만 있으면 그대로 덱이 된다(true).
    // 빈칸이면 조용히 false, 다른 글자가 있으면 경고하고 false (→ 랜덤).
    bool TryFillFixedOrder()
    {
        if (string.IsNullOrWhiteSpace(fixedOrder)) return false;

        cards.Clear();
        foreach (char c in fixedOrder.ToUpper())
        {
            if (c == 'D') cards.Add(Candle.Down);
            else if (c == 'U') cards.Add(Candle.Up);
            else if (c != ' ')
            {
                Debug.LogWarning($"고정 순서에 D/U가 아닌 글자 '{c}'가 있어 랜덤으로 채웁니다.");
                return false;
            }
        }
        return true;
    }

    void Recount()
    {
        RemainingDown = 0;
        RemainingUp = 0;
        foreach (Candle card in cards)
        {
            if (card == Candle.Down) RemainingDown++;
            else RemainingUp++;
        }
    }
}