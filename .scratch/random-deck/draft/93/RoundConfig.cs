using System;

// 라운드 표의 한 줄. [Serializable]이라 GameManager Inspector에 칸으로 보인다.
[Serializable]
public class RoundConfig
{
    // random-deck 93: 시작 잔고 범위. 라운드를 시작할 때마다 이 안(양 끝 포함)에서 하나 뽑아 나·광대 둘 다 그 값으로 시작한다.
    // 2026-11-23 사용자 "체력을 라운드마다 랜덤, 마지막 라운드만 10, 나머지 5~7". 고정하려면 최소 = 최대
    public int minBalance = 5;
    public int maxBalance = 7;
    // 덱 총 장수 범위. 채울 때마다(라운드 시작·재장전) 이 안에서 뽑는다.
    public int minCards = 4;
    public int maxCards = 6;
    // 테스트용 고정 순서 (CandleDeck의 Use Fixed Order를 켰을 때만 사용). 빈칸이 아니면 이 문자열이 그대로 덱이 된다.
    public string fixedOrder = "";
}
