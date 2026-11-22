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
}
