using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] TMP_Text statusText;

    [Header("플레이 화면 조각 (Ticket 25)")]                                             // ← 25 추가
    [SerializeField] TerminalView terminalView;                                        // ← 25 추가
    [SerializeField] LeverageView leverageView;                                        // ← 25 추가
    [SerializeField] ClownView clownView;                                              // ← 25 추가

    [Header("정식 UI 조각 (Ticket 26)")]
    [SerializeField] BalanceView playerBalance;
    [SerializeField] BalanceView clownBalance;
    [SerializeField] RoundTurnView roundTurnView;
    [SerializeField] ActionButtonsView actionButtons;
    [SerializeField] ItemSlotsView itemSlots;
    [SerializeField] ItemCloseupPanel closeupPanel;   // Ticket 27
    [SerializeField] ResultPanel resultPanel;          // Ticket 28
    [SerializeField] PausePanel pausePanel;            // Ticket 29
    // 피해 때 화면 전체를 덮는 반투명 빨강
    [SerializeField] Image damageFlash;

    [Header("개발용 글자 (F1로 켜고 끔, 최종 빌드 전 제거 #41)")]
    [SerializeField] bool showDevText = false;

    [Header("화면 반응")]
    [SerializeField] float flashSeconds = 0.2f;
    [SerializeField] Color flashColor = new Color(0.85f, 0.25f, 0.22f);
    // 내 잔고가 이 값 이하면 잔고 줄을 어두운 빨강으로 (긴장 연출)
    [SerializeField] int dangerBalance = 3;
    [SerializeField] string dangerColorHex = "#8A3A34";

    Color normalColor;
    Coroutine flashRoutine;

    // 원래 글자 색을 기억해 둔다(깜빡임 뒤 되돌릴 색).
    void Awake()
    {
        normalColor = statusText.color;
        statusText.enabled = showDevText;
        damageFlash.enabled = false;
    }

    // F1: 개발용 글자 켜고 끄기. 판정과 상관없는 개발 도구라 PlayerInputHandler를 거치지 않고 키보드를 직접 읽는다.
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            showDevText = !showDevText;
            statusText.enabled = showDevText;
        }
    }

    // 피해: 글자를 잠깐 빨강으로. 이미 깜빡이는 중이면 처음부터 다시.
    public void FlashDamage()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        statusText.color = flashColor;
        damageFlash.enabled = true;
        yield return new WaitForSeconds(flashSeconds);
        statusText.color = normalColor;
        damageFlash.enabled = false;
        flashRoutine = null;
    }

    // 상태를 읽어서 글자로만 보여준다. 값을 바꾸지 않는다.
    public void Refresh(GameManager game)
    {
        // 그림 쪽 화면 조각들도 같은 상태를 읽어 다시 그린다.                           // ← 25 추가
        terminalView.Refresh(game);                                                    // ← 25 추가
        leverageView.Refresh(game);                                                    // ← 25 추가
        clownView.Refresh(game);                                                       // ← 25 추가
        playerBalance.Refresh(game);
        clownBalance.Refresh(game);
        roundTurnView.Refresh(game);
        actionButtons.Refresh(game);
        itemSlots.Refresh(game);
        closeupPanel.Refresh(game);
        resultPanel.Refresh(game);
        pausePanel.Refresh(game);

        string choice = game.Selected == PlayerAction.Hold ? "[보유]  매도" : " 보유  [매도]";
        string message = game.IsRevealing ? "공개 중..." : game.LastResult;

        // 결과 줄을 덮지 않고 덧붙인다: 내 직전 결과를 읽을 수 있게.
        if (game.IsClownThinking)
        {
            message += "\n광대 생각 중...";
        }

        // 차트 분석으로 아는 다음 캔들 (플레이어만). 그 캔들이 나오면 GameManager가 지운다.
        if (game.PlayerKnownNext.HasValue)
        {
            string next = game.PlayerKnownNext.Value == Candle.Down ? "하락" : "상승";
            message += $"\n차트 분석: 다음 캔들은 {next}";
        }

        // 아이템 확인 중: 이름·설명·조작 안내
        if (game.IsConfirming)
        {
            ItemType item = game.Player.Items[game.ConfirmingSlot];
            message += $"\n확인: {game.ItemName(item)} — {game.ItemDescription(item)} / Space 사용, Esc 취소";

            // 거래 정지는 상대가 이미 정지면 쓸 수 없다고 미리 알려 준다.
            if (item == ItemType.TradingHalt && game.Clown.IsHalted)
            {
                message += " (사용 불가: 상대가 이미 정지 상태)";
            }
        }

        if (game.IsRoundCleared)
        {
            message += $"\n라운드 {game.RoundNumber} 클리어! - Space로 다음 라운드";
        }
        else if (game.IsFinalVictory)
        {
            message += "\n최종 승리! - 다시하기 버튼";
        }
        else if (game.IsRoundOver)
        {
            message += "\n패배... - 다시하기 버튼";
        }

        // 내 잔고가 위험하면 잔고 줄 전체를 어두운 빨강으로 감싼다(TMP 색 태그).
        string balanceLine = $"잔고  플레이어 {game.Player.Balance}{HaltMark(game.Player)} / 광대 {game.Clown.Balance}{HaltMark(game.Clown)}";
        if (game.Player.Balance <= dangerBalance)
        {
            balanceLine = $"<color={dangerColorHex}>{balanceLine}</color>";
        }

        statusText.text =
            $"라운드 {game.RoundNumber} / {game.RoundCount}\n" +
            $"차례: {game.Current.Name}\n" +
            $"선택: {choice}\n" +
            $"{balanceLine}\n" +
            $"레버리지 {game.Leverage.Value}\n" +
            $"남은 캔들  하락 {game.Deck.RemainingDown} / 상승 {game.Deck.RemainingUp}\n" +
            $"아이템  나: {ItemList(game, game.Player, true)} / 광대: {ItemList(game, game.Clown, false)}\n" +
            $"{message}";
    }

    // 거래 정지 상태면 잔고 옆에 붙일 표시.
    string HaltMark(Participant who)
    {
        return who.IsHalted ? " (정지)" : "";
    }

    // 아이템 이름을 쉼표로 잇는다. 내 것은 키 번호(1~4)를 앞에 붙인다. 없으면 "-".
    string ItemList(GameManager game, Participant who, bool withKeys)
    {
        if (who.Items.Count == 0) return "-";

        List<string> names = new List<string>();
        for (int i = 0; i < who.Items.Count; i++)
        {
            string name = game.ItemName(who.Items[i]);
            names.Add(withKeys ? $"{i + 1} {name}" : name);
        }
        return string.Join(", ", names);
    }
}