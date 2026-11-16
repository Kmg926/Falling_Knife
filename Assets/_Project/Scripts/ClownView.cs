using UnityEngine;

// 광대 그림: 생각 중 / 평소(조명 받음) / 어둡게(내 차례라 조명이 비켜남, ui-revamp 30 제안 4). 읽기만 한다.
public class ClownView : MonoBehaviour
{
    [SerializeField] SpriteRenderer body;
    [SerializeField] Sprite idle;
    [SerializeField] Sprite thinking;
    [SerializeField] Sprite dimmed;

    public void Refresh(GameManager game)
    {
        // 광대 차례이거나 라운드가 끝났으면 광대에게 조명
        bool lit = game.Current == game.Clown || game.IsRoundOver;
        if (game.IsClownThinking) body.sprite = thinking;
        else body.sprite = lit ? idle : dimmed;
    }
}
