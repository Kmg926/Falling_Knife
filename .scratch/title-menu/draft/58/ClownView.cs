using UnityEngine;

// 광대 (clown-idle 46): 그림을 직접 바꾸지 않고 Animator에 "지금 상태"만 알려 준다. 읽기만 한다(판정 없음).
// - 몸 레이어: 생각 중 / 평소(조명 받음) / 어둡게(내 차례, ui-revamp 30) — 각 상태가 숨쉬기 클립(2.4초)을 돈다
// - 눈 레이어: 열림 / 깜빡임(0.12초) / 힐끗(0.6초). 불규칙한 간격은 Animator만으로 만들기 어려워서 여기서 신호(trigger)를 보낸다
public class ClownView : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] Animator animator;

    [Header("깜빡임 (Spec 제안 3)")]
    [SerializeField] float blinkMin = 3f;
    [SerializeField] float blinkMax = 6f;
    [SerializeField] float doubleBlinkChance = 0.15f;  // 가끔 두 번 연속
    [SerializeField] float doubleBlinkGap = 0.25f;

    [Header("힐끗 (Spec 제안 4)")]
    [SerializeField] float glanceMin = 15f;
    [SerializeField] float glanceMax = 25f;

    static readonly int IsThinking = Animator.StringToHash("IsThinking");
    static readonly int IsLit = Animator.StringToHash("IsLit");
    static readonly int Blink = Animator.StringToHash("Blink");
    static readonly int Glance = Animator.StringToHash("Glance");

    float blinkTimer;
    float glanceTimer;

    void Awake()
    {
        blinkTimer = Random.Range(blinkMin, blinkMax);
        glanceTimer = Random.Range(glanceMin, glanceMax);
    }

    // 타이틀 광대(title-menu 58): 게임이 없으면 Refresh가 불리지 않으므로, 처음부터 밝은 평소 상태로 숨쉬고 깜빡임
    void Start()
    {
        if (game == null) animator.SetBool(IsLit, true);
    }

    public void Refresh(GameManager game)
    {
        // 광대 차례이거나 라운드가 끝났으면 광대에게 조명(전과 같은 규칙) — 어떤 그림을 쓸지는 Animator가 정함
        bool lit = game.Current == game.Clown || game.IsRoundOver;
        animator.SetBool(IsThinking, game.IsClownThinking);
        animator.SetBool(IsLit, lit);
    }

    // 매 프레임: 깜빡임·힐끗 타이머. Time.deltaTime이라 일시정지(timeScale 0)면 같이 멈춤
    void Update()
    {
        blinkTimer -= Time.deltaTime;
        if (blinkTimer <= 0f)
        {
            animator.SetTrigger(Blink);
            bool twice = Random.value < doubleBlinkChance;
            blinkTimer = twice ? doubleBlinkGap : Random.Range(blinkMin, blinkMax);
        }

        glanceTimer -= Time.deltaTime;
        if (glanceTimer <= 0f)
        {
            // 생각 중(눈동자가 위)·캔들 공개 중에는 힐끗하지 않고 조금 뒤로 미룸
            if (game != null && (game.IsClownThinking || game.IsRevealing)) glanceTimer = 1f;
            else
            {
                animator.SetTrigger(Glance);
                glanceTimer = Random.Range(glanceMin, glanceMax);
            }
        }
    }
}
