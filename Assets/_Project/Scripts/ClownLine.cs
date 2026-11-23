// 광대가 말할 대사의 종류 (clown-tell 80). 판정 코드(GameManager)는 "어떤 순간인지"만 이 이름으로 알리고,
// 실제 글은 ClownSpeechView의 표(Inspector)에 있다 — 판정과 글을 섞지 않으려고(Spec 제안 1).
public enum ClownLine
{
    None,          // 말풍선 없음(내가 행동했거나 새 라운드)
    Buy,           // "매수. 들고 간다."
    Sell,          // "매도. 너한테 넘기지."
    UseChart,      // "차트 분석을 쓰지."
    UseRumor,      // "찌라시를 쓰지."
    UseHalt,       // "거래 정지를 쓰지."
    UseLottery,    // "복권을 쓰지."
    Halted,        // 광대가 거래 정지로 차례를 건너뜀 "…한 번 쉬지." (Spec 제안 3)

    // ── clown-tell 81: 결과 대사 (Grilling Q6·Q8) ──
    OneMore,       // 광대 매수 + 상승 "한 번 더."
    Hurt,          // 광대가 맞음(광대 매수+하락, 내 매도+하락) "…칫."
    Gotcha,        // 광대 매도 + 하락 = 내가 맞음 "잘 받았지?"
    NothingHappened, // 광대 매도 + 상승 "흥. 아무 일도 없군."
    Laugh,         // 내 매수 + 하락 = 내가 맞음 "하하."
    ChartDone,     // 차트 분석 뒤 "…그렇군."
    RumorDone,     // 찌라시 뒤 "다들 봤지?"
    HaltDone,      // 거래 정지 뒤 "넌 한 번 쉬어."
    LotteryHeal,   // 복권 잔고 +1 "운이 좋군."
    LotteryLeverage, // 복권 레버리지 +1 "판을 키우지."
    LotteryItem,   // 복권 아이템 +1 "하나 더."
    LotteryNothing,  // 복권 꽝(이미 가득이라 못 받은 것도) "…꽝이군."

    // ── heal-items 90: 회복 아이템 (뒤에 추가 — 앞 번호 그대로) ──
    UseDividend,   // "배당금을 받지."
    UseBailout,    // "구제 금융을 쓰지."
    Healed,        // 회복 뒤 "숨 좀 돌리지."

    // ── flip-item 101: 작전 (뒤에 추가 — 앞 번호 그대로) ──
    UseFlip,       // "작전을 쓰겠다."
    FlipDone,      // 쓴 뒤 "흐름을 바꿨다."
}
