using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// 설정 창의 내용 (game-settings 91): 전체 소리 · 음악 · 화면 흔들림 · 전체화면 4줄.
// 타이틀(TitleMenu)과 게임 중 일시정지(PausePanel)가 같은 이 부품을 쓴다 — 줄 고르기·값 바꾸기·저장·글 그리기가 한 곳에.
// 창을 열고 닫는 일(어느 창, 제목·아래 안내)은 각 화면이 맡고, 이 부품은 글 상자(body)에 설정 줄만 그린다.
// 값은 GameSettings에 저장 → GameSettings.Changed 알림으로 음악·효과음·흔들림이 바로 따라온다.
public class SettingsPanel : MonoBehaviour
{
    [SerializeField] TMP_Text body;                  // 설정 4줄을 그릴 글 상자
    [SerializeField] Color selectedColor = new Color32(0xD9, 0xB4, 0x5A, 255);   // gold2
    [SerializeField] AudioSource sfx;                // 값을 바꾸면 클릭음으로 크기를 바로 들려 줌
    [SerializeField] AudioClip clickClip;

    public const int Rows = 4;   // 전체 소리 · 음악 · 화면 흔들림 · 전체화면
    public int Row { get; private set; }

    Camera uiCamera;   // 마우스가 어느 줄 위인지 잴 때(HUD·타이틀 Canvas는 Screen Space - Camera)

    void Awake()
    {
        Canvas canvas = body != null ? body.GetComponentInParent<Canvas>() : null;
        uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
    }

    // 창을 열 때: 첫 줄부터
    public void Begin()
    {
        Row = 0;
        Show();
    }

    // 열려 있는 동안 매 프레임(각 화면이 부름): 마우스 올리면 그 줄, 클릭하면 그 줄 한 단계 오른쪽, 키보드 ↑↓ 줄 / ←→ 값
    // Esc(닫기)는 여기서 보지 않는다 — 닫는 길은 각 화면이 정함
    public void HandleInput(Keyboard kb, Mouse mouse)
    {
        if (mouse != null)
        {
            int line = TMP_TextUtilities.FindIntersectingLine(body, mouse.position.ReadValue(), uiCamera);
            if (line >= 0 && line < Rows && mouse.delta.ReadValue().sqrMagnitude > 0f && line != Row) { Row = line; Show(); }
            if (mouse.leftButton.wasPressedThisFrame && line >= 0 && line < Rows) { Row = line; Change(1); return; }
        }
        if (kb == null) return;
        if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) Move(-1);
        else if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) Move(1);
        else if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) Change(-1);
        else if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame) Change(1);
    }

    // 줄 위·아래(끝에서 돎)
    public void Move(int dir)
    {
        Row = (Row + dir + Rows) % Rows;
        Show();
    }

    // 고른 줄의 값을 바꿈: 소리는 10% 단위(0~100%에서 멈춤), 켬/끔은 방향과 상관없이 바뀜. 바꿀 때마다 저장(GameSettings가 알림을 보냄)
    public void Change(int dir)
    {
        switch (Row)
        {
            case 0: GameSettings.Master += dir; break;
            case 1: GameSettings.Music += dir; break;
            case 2: GameSettings.Shake = !GameSettings.Shake; break;
            default: GameSettings.Fullscreen = !GameSettings.Fullscreen; break;
        }
        // 소리 크기를 바로 들려 줌(전체 소리 크기로). 시간이 멈춰 있어도 PlayOneShot은 들림
        if (sfx != null && clickClip != null && GameSettings.MasterVolume > 0f) sfx.PlayOneShot(clickClip, GameSettings.MasterVolume);
        Show();
    }

    public void Show()
    {
        string[] names = { "전체 소리", "음악", "화면 흔들림", "전체화면" };
        string[] values =
        {
            Bar(GameSettings.Master), Bar(GameSettings.Music),
            GameSettings.Shake ? "켬" : "끔", GameSettings.Fullscreen ? "켬" : "끔",
        };
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < Rows; i++)
        {
            bool on = i == Row;
            // <pos=40%>: 이름 길이와 상관없이 값이 같은 자리에서 시작(TMP 태그)
            string line = $"{(on ? "▶ " : "   ")}{names[i]}<pos=40%>{values[i]}";
            sb.Append(on ? $"<color=#{ColorUtility.ToHtmlStringRGB(selectedColor)}>{line}</color>" : line);
            if (i < Rows - 1) sb.Append('\n');
        }
        body.text = sb.ToString();
    }

    // ■■■■■■□□□□ 60%
    static string Bar(int step) => new string('■', step) + new string('□', GameSettings.Steps - step) + $" {step * 10}%";
}
