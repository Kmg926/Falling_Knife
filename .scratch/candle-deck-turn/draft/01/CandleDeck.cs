using System.Collections.Generic;
using UnityEngine;

// 캔들 한 장의 종류. D = Down(하락), U = Up(상승)
public enum Candle { Down, Up }

public class CandleDeck : MonoBehaviour
{
    [Header("라운드 구성")]
    [SerializeField] int downCount = 2;
    [SerializeField] int upCount = 3;

    [Header("테스트용 고정 순서")]
    [SerializeField] bool useFixedOrder = false;
    [SerializeField] string fixedOrder = "D U U D U";

    // 0번이 덱 맨 위 (다음에 공개될 캔들)
    readonly List<Candle> cards = new List<Candle>();

    public int RemainingDown { get; private set; }
    public int RemainingUp { get; private set; }

    // 라운드 시작 때 부른다. 재장전(04)도 이 함수를 다시 쓴다.
    public void Fill()
    {
        if (!useFixedOrder || !TryFillFixedOrder())
        {
            FillShuffled();
        }
        Recount();
        Debug.Log("덱 순서(위→아래): " + string.Join(" ", cards));
    }

    void FillShuffled()
    {
        cards.Clear();
        for (int i = 0; i < downCount; i++) cards.Add(Candle.Down);
        for (int i = 0; i < upCount; i++) cards.Add(Candle.Up);

        // Fisher-Yates: 뒤에서부터 한 장씩, 자기 자리 또는 앞쪽 아무 자리와 맞바꾼다
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Candle temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }
    }

    // 성공하면 true. 구성과 안 맞으면 경고하고 false (→ 무작위로 섞음)
    bool TryFillFixedOrder()
    {
        cards.Clear();
        int downs = 0;
        int ups = 0;

        foreach (char c in fixedOrder.ToUpper())
        {
            if (c == 'D') { cards.Add(Candle.Down); downs++; }
            else if (c == 'U') { cards.Add(Candle.Up); ups++; }
            else if (c != ' ')
            {
                Debug.LogWarning($"고정 순서에 D/U가 아닌 글자 '{c}'가 있어 무작위로 섞습니다.");
                return false;
            }
        }

        if (downs != downCount || ups != upCount)
        {
            Debug.LogWarning($"고정 순서(하락{downs}/상승{ups})가 덱 구성(하락{downCount}/상승{upCount})과 달라 무작위로 섞습니다.");
            return false;
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
