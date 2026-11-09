using System;

// 라운드 표의 한 줄. [Serializable]이라 GameManager Inspector에 칸으로 보인다.
[Serializable]
public class RoundConfig
{
    public int startingBalance = 10;
    // 덱 총 장수 범위. 채울 때마다(라운드 시작·재장전) 이 안에서 뽑는다.
    public int minCards = 4;
    public int maxCards = 6;
    // 테스트용 고정 순서 (CandleDeck의 Use Fixed Order를 켰을 때만 사용). 빈칸이 아니면 이 문자열이 그대로 덱이 된다.
    public string fixedOrder = "";
}
