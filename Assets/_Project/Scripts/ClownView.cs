using UnityEngine;

// 광대 그림: 생각 중이면 '생각 중' 그림, 아니면 평소 그림. 읽기만 한다.
public class ClownView : MonoBehaviour
{
    [SerializeField] SpriteRenderer body;
    [SerializeField] Sprite idle;
    [SerializeField] Sprite thinking;

    public void Refresh(GameManager game)
    {
        body.sprite = game.IsClownThinking ? thinking : idle;
    }
}