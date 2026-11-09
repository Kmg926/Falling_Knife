using UnityEngine;
using UnityEngine.InputSystem;

// 키 입력을 받아 GameManager에 전달만 한다. 규칙 판정은 하지 않는다.
// 핫시트라서 광대 차례에도 같은 입력을 쓴다.
public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] InputActionReference selectAction;
    [SerializeField] InputActionReference executeAction;
    [SerializeField] InputActionReference restartAction;

    void OnEnable()
    {
        selectAction.action.performed += OnSelect;
        executeAction.action.performed += OnExecute;
        restartAction.action.performed += OnRestart;
        selectAction.action.Enable();
        executeAction.action.Enable();
        restartAction.action.Enable();
    }

    void OnDisable()
    {
        selectAction.action.performed -= OnSelect;
        executeAction.action.performed -= OnExecute;
        restartAction.action.performed -= OnRestart;
        selectAction.action.Disable();
        executeAction.action.Disable();
        restartAction.action.Disable();
    }

    // A·← = -1 (보유), D·→ = +1 (매도)
    void OnSelect(InputAction.CallbackContext context)
    {
        float direction = context.ReadValue<float>();
        if (direction < 0) game.Select(PlayerAction.Hold);
        else if (direction > 0) game.Select(PlayerAction.Sell);
    }

    void OnExecute(InputAction.CallbackContext context)
    {
        game.Execute();
    }

    // R (테스트용, 최종 빌드 전 제거)
    void OnRestart(InputAction.CallbackContext context)
    {
        game.Restart();
    }
}
