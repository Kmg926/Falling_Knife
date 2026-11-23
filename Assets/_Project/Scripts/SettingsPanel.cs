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

    [Header("닫기 X 버튼 (game-settings 95)")]
    [SerializeField] RectTransform closeButton;      // 창 오른쪽 위 X(누르면 각 화면이 창을 닫음)
    [SerializeField] TMP_Text closeLabel;            // X 글자(마우스를 올리면 금색)
    [SerializeField] Color closeNormal = new Color32(0xA3, 0x9A, 0x8C, 255);    // cream0

    public const int Rows = 5;        // 전체 소리 · 효과음 · 음악 · 화면 흔들림 · 전체화면 (96: 효과음 줄 추가)
    public const int VolumeRows = 3;  // 96: 앞의 3줄이 소리 막대(줄 번호 = GameSettings.GetVolume 번호)
    public int Row { get; private set; }
    // 95: 이번 프레임에 X를 눌렀나 — HandleInput 뒤에 각 화면(타이틀·일시정지)이 읽고 자기 방식으로 닫는다
    public bool CloseClicked { get; private set; }

    Camera uiCamera;   // 마우스가 어느 줄 위인지 잴 때(HUD·타이틀 Canvas는 Screen Space - Camera)
    int dragRow = -1;  // 95: 음량 막대를 누른 채 끄는 중인 줄(0 전체 소리, 1 효과음, 2 음악, -1 = 아님)
    bool wasDown = true;  // 96: 지난번 왼쪽 버튼 상태. 처음은 "눌려 있었음"으로 → 창을 연 그 클릭이 바로 값을 바꾸지 않게

    void Awake()
    {
        Canvas canvas = body != null ? body.GetComponentInParent<Canvas>() : null;
        uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
    }

    // 창을 열 때: 첫 줄부터
    public void Begin()
    {
        Row = 0;
        dragRow = -1;
        wasDown = true;   // 96: 창을 연 클릭(톱니·메뉴)을 떼기 전엔 누름으로 치지 않음
        CloseClicked = false;
        if (closeLabel != null) closeLabel.color = closeNormal;
        if (closeButton != null) closeButton.gameObject.SetActive(true);
        Show();
    }

    // 95: 창을 닫을 때(타이틀은 규칙·크레딧과 창을 같이 써서 X를 숨겨야 함)
    public void End()
    {
        dragRow = -1;
        if (closeButton != null) closeButton.gameObject.SetActive(false);
    }

    // 열려 있는 동안 매 프레임(각 화면이 부름): 마우스 올리면 그 줄, 클릭하면 그 줄 한 단계 오른쪽, 키보드 ↑↓ 줄 / ←→ 값
    // 95: 음량 줄은 막대를 누른 자리의 크기로 바로 가고, 누른 채 끌면 따라감. X를 누르면 CloseClicked
    // Esc(닫기)는 여기서 보지 않는다 — 닫는 길은 각 화면이 정함
    public void HandleInput(Keyboard kb, Mouse mouse)
    {
        CloseClicked = false;
        if (mouse != null)
        {
            Vector2 pos = mouse.position.ReadValue();
            // 96: "막 누름" = 지금 눌림 + 지난번엔 안 눌림(일시정지 메뉴와 같은 방식). 멈춘 동안(timeScale 0)에도 확실하게
            bool down = mouse.leftButton.isPressed;
            bool pressed = down && !wasDown;
            wasDown = down;

            // 95: 끄는 중 — 버튼을 누르고 있는 동안 마우스 x 위치의 크기로(막대 밖으로 나가도 0·100%에서 멈춤)
            if (dragRow >= 0)
            {
                if (mouse.leftButton.isPressed) SetVolume(dragRow, StepAt(dragRow, pos, true));
                else dragRow = -1;
                return;
            }

            // 95: X — 올리면 금색, 누르면 닫기 요청
            bool overClose = closeButton != null && closeButton.gameObject.activeInHierarchy &&
                             RectTransformUtility.RectangleContainsScreenPoint(closeButton, pos, uiCamera);
            if (closeLabel != null) closeLabel.color = overClose ? selectedColor : closeNormal;
            if (overClose && pressed) { CloseClicked = true; return; }

            int line = TMP_TextUtilities.FindIntersectingLine(body, pos, uiCamera);
            if (line >= 0 && line < Rows && mouse.delta.ReadValue().sqrMagnitude > 0f && line != Row) { Row = line; Show(); }
            if (pressed && line >= 0 && line < Rows)
            {
                Row = line;
                int step = line < VolumeRows ? StepAt(line, pos, false) : -1;
                if (step >= 0) { dragRow = line; SetVolume(line, step); Show(); }   // 95: 막대 위 → 그 자리 크기 + 끌기 시작
                else Change(1);                                                     // 이름 쪽·켬/끔 줄 → 전과 같이 한 단계
                return;
            }
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
        if (Row < VolumeRows) GameSettings.SetVolume(Row, GameSettings.GetVolume(Row) + dir);   // 96: 전체·효과음·음악
        else if (Row == VolumeRows) GameSettings.Shake = !GameSettings.Shake;
        else GameSettings.Fullscreen = !GameSettings.Fullscreen;
        PlayClick();
        Show();
    }

    // 소리 크기를 바로 들려 줌(효과음이 실제로 들리는 크기 = 전체 × 효과음). 시간이 멈춰 있어도 PlayOneShot은 들림
    void PlayClick()
    {
        if (sfx != null && clickClip != null && GameSettings.EffectsVolume > 0f) sfx.PlayOneShot(clickClip, GameSettings.EffectsVolume);
    }

    // 95: 마우스 x가 음량 막대(■□ 10칸)의 몇 칸째인지 → 0~10. 누른 칸까지 켜짐(7번째 칸 = 70%), 막대 왼쪽 바깥 = 0
    // clamp가 false면 막대 근처(왼쪽 1칸 ~ 오른쪽 % 글자까지)가 아닐 때 -1(= 막대를 누른 게 아님)
    int StepAt(int row, Vector2 screenPos, bool clamp)
    {
        TMP_TextInfo info = body.textInfo;
        if (row >= info.lineCount) return -1;
        TMP_LineInfo li = info.lineInfo[row];
        float left = float.NaN, right = 0f;
        for (int i = li.firstCharacterIndex; i <= li.lastCharacterIndex; i++)
        {
            char c = info.characterInfo[i].character;
            if (c != '■' && c != '□') continue;
            if (float.IsNaN(left)) left = info.characterInfo[i].origin;   // 첫 칸 왼쪽
            right = info.characterInfo[i].xAdvance;                        // 마지막 칸 오른쪽
        }
        if (float.IsNaN(left)) return -1;
        float cell = (right - left) / GameSettings.Steps;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(body.rectTransform, screenPos, uiCamera, out Vector2 local);
        if (!clamp && (local.x < left - cell || local.x > right + cell * 4f)) return -1;
        if (local.x < left) return 0;
        return Mathf.Clamp(Mathf.FloorToInt((local.x - left) / cell) + 1, 0, GameSettings.Steps);
    }

    // 95: 음량을 정한 크기로. 바뀔 때만 저장·클릭음(끄는 동안 칸이 넘어갈 때마다 딸깍)
    void SetVolume(int row, int step)
    {
        if (step == GameSettings.GetVolume(row)) return;
        GameSettings.SetVolume(row, step);   // 96: 0 전체 · 1 효과음 · 2 음악
        PlayClick();
        Show();
    }

    public void Show()
    {
        // 96: 사용자 "메인 볼륨, 효과음 볼륨, 배경 음악 볼륨으로"(2026-11-23)
        string[] names = { "메인 볼륨", "효과음 볼륨", "배경 음악 볼륨", "화면 흔들림", "전체화면" };
        string[] values =
        {
            Bar(GameSettings.Master), Bar(GameSettings.Sfx), Bar(GameSettings.Music),
            GameSettings.Shake ? "켬" : "끔", GameSettings.Fullscreen ? "켬" : "끔",
        };
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < Rows; i++)
        {
            bool on = i == Row;
            // <pos=45%>: 이름 길이와 상관없이 값이 같은 자리에서 시작(TMP 태그)
            // 96: 40 → 45% — "▶ 배경 음악 볼륨"과 막대 사이가 4칸뿐이라 넓힘
            string line = $"{(on ? "▶ " : "   ")}{names[i]}<pos=45%>{values[i]}";
            sb.Append(on ? $"<color=#{ColorUtility.ToHtmlStringRGB(selectedColor)}>{line}</color>" : line);
            if (i < Rows - 1) sb.Append('\n');
        }
        body.text = sb.ToString();
    }

    // ■■■■■■□□□□ 60%
    static string Bar(int step) => new string('■', step) + new string('□', GameSettings.Steps - step) + $" {step * 10}%";
}
