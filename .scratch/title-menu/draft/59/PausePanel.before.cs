using UnityEngine;
using UnityEngine.UI;

// 일시정지 패널: 멈춘 동안 켜지고, 계속 / 타이틀 버튼. 판정은 하지 않는다(GameManager 함수만 부른다).
public class PausePanel : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] GameObject root;     // 어둡게 + 패널 전체
    [SerializeField] Button resumeButton;
    [SerializeField] Button titleButton;

    void Awake()
    {
        resumeButton.onClick.AddListener(() => game.Resume());
        titleButton.onClick.AddListener(() => game.GoToTitle());
    }

    public void Refresh(GameManager game)
    {
        root.SetActive(game.IsPaused);
    }
}
