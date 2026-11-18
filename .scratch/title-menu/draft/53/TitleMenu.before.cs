using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 타이틀 화면 (TitleScene). Space·시작 버튼 → GameScene, Esc·종료 → 게임 끝(에디터에서는 Play 멈춤).
// 게임 판정과 상관없는 화면 이동이라 Input Actions를 거치지 않고 키보드를 직접 읽는다(F1과 같은 방식).
public class TitleMenu : MonoBehaviour
{
    [SerializeField] Button startButton;
    [SerializeField] Button quitButton;
    [SerializeField] string gameSceneName = "GameScene";

    void Awake()
    {
        Time.timeScale = 1f;   // 일시정지 중에 돌아와도 정상 속도
        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(Quit);
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard.spaceKey.wasPressedThisFrame) StartGame();
        else if (keyboard.escapeKey.wasPressedThisFrame) Quit();
    }

    void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    void Quit()
    {
#if UNITY_EDITOR
        // 에디터에는 "프로그램 끄기"가 없어서 Play를 멈춘다
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
