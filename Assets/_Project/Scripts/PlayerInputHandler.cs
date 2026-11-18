using UnityEngine;
using UnityEngine.InputSystem;

// 키 입력을 받아 GameManager에 전달만 한다. 규칙 판정은 하지 않는다.
// 광대 차례 입력은 GameManager가 무시한다.
public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] InputActionReference selectAction;
    [SerializeField] InputActionReference executeAction;

    [Header("아이템 (1~4 확인, Esc 취소)")]
    [SerializeField] InputActionReference item1Action;
    [SerializeField] InputActionReference item2Action;
    [SerializeField] InputActionReference item3Action;
    [SerializeField] InputActionReference item4Action;
    [SerializeField] InputActionReference cancelAction;

    void OnEnable()
    {
        selectAction.action.performed += OnSelect;
        executeAction.action.performed += OnExecute;
        item1Action.action.performed += OnItem1;
        item2Action.action.performed += OnItem2;
        item3Action.action.performed += OnItem3;
        item4Action.action.performed += OnItem4;
        cancelAction.action.performed += OnCancel;

        selectAction.action.Enable();
        executeAction.action.Enable();
        item1Action.action.Enable();
        item2Action.action.Enable();
        item3Action.action.Enable();
        item4Action.action.Enable();
        cancelAction.action.Enable();
    }

    void OnDisable()
    {
        selectAction.action.performed -= OnSelect;
        executeAction.action.performed -= OnExecute;
        item1Action.action.performed -= OnItem1;
        item2Action.action.performed -= OnItem2;
        item3Action.action.performed -= OnItem3;
        item4Action.action.performed -= OnItem4;
        cancelAction.action.performed -= OnCancel;

        selectAction.action.Disable();
        executeAction.action.Disable();
        item1Action.action.Disable();
        item2Action.action.Disable();
        item3Action.action.Disable();
        item4Action.action.Disable();
        cancelAction.action.Disable();
    }

    // A·← = -1 (매수), D·→ = +1 (매도)
    void OnSelect(InputAction.CallbackContext context)
    {
        float direction = context.ReadValue<float>();
        if (direction < 0) game.Select(PlayerAction.Buy);
        else if (direction > 0) game.Select(PlayerAction.Sell);
    }

    void OnExecute(InputAction.CallbackContext context)
    {
        game.Execute();
    }

    // 1~4 키 → 슬롯 0~3 (화면 번호는 1부터, 코드 번호는 0부터)
    void OnItem1(InputAction.CallbackContext context) { game.SelectItem(0); }
    void OnItem2(InputAction.CallbackContext context) { game.SelectItem(1); }
    void OnItem3(InputAction.CallbackContext context) { game.SelectItem(2); }
    void OnItem4(InputAction.CallbackContext context) { game.SelectItem(3); }

    // Esc
    // Esc·우클릭: 아이템 확인 중이면 취소. 아니면 Esc만 일시정지(우클릭은 취소 전용 — 실수로 멈추지 않게).
    void OnCancel(InputAction.CallbackContext context)
    {
        if (game.IsConfirming)
        {
            game.CancelItem();      // item-phone 63: 상세 → 폰 목록
            return;
        }
        // item-phone 63: 폰 목록이 열려 있으면 Esc·우클릭 모두 "폰 닫기"
        if (game.IsPhoneOpen)
        {
            game.ClosePhone();
            return;
        }
        // monitor-trade 43: 모니터 확대 화면이 열려 있으면 Esc·우클릭 모두 "닫기"
        if (game.IsMonitorOpen)
        {
            game.CloseMonitor();
            return;
        }
        if (context.control.device is Keyboard) game.TogglePause();
    }
}
