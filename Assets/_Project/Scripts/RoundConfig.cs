using System;

// 라운드 표의 한 줄. [Serializable]이라 GameManager Inspector에 칸으로 보인다.
[Serializable]
public class RoundConfig
{
    public int startingBalance = 2;
    public int downCount = 2;
    public int upCount = 3;
    // 테스트용 고정 순서 (CandleDeck의 Use Fixed Order를 켰을 때만 사용)
    public string fixedOrder = "";
}