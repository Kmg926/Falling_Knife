using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 타이틀 화면 (TitleScene). title-menu 53: 세로 메뉴 시작·규칙·설정·크레딧·종료.
// ↑/↓·W/S 이동(끝에서 반대쪽으로), Space·Enter·클릭 선택, 마우스를 올리면 그 줄을 고름.
// Esc = 창 닫기(첫 화면에선 아무 일 없음 — 실수로 꺼지지 않게, Grilling Q2).
// 게임 판정과 상관없는 화면 이동이라 Input Actions를 거치지 않고 키보드·마우스를 직접 읽는다(전과 같음).
public class TitleMenu : MonoBehaviour
{
    public enum Panel { None, Rules, Settings, Credits, Placeholder }

    [Header("메뉴 (위에서부터 시작·규칙·설정·크레딧·종료)")]
    [SerializeField] TMP_Text[] items;
    [SerializeField] string[] labels = { "시작", "규칙", "설정", "크레딧", "종료" };
    [SerializeField] Color selectedColor = new Color32(0xD9, 0xB4, 0x5A, 255);   // gold2
    [SerializeField] Color normalColor = new Color32(0xA3, 0x9A, 0x8C, 255);     // cream0
    [SerializeField] string gameSceneName = "GameScene";

    [Header("창 (규칙 3쪽 / 준비 중)")]
    [SerializeField] GameObject panelRoot;          // 뒤 어둡게 + 창
    [SerializeField] TMP_Text panelTitle;
    [SerializeField] TMP_Text panelBody;
    [SerializeField] TMP_Text panelFooter;
    [SerializeField] GameObject itemRows;           // 규칙 2쪽: 아이템 4줄(아이콘 + 글)
    [SerializeField] TMP_Text[] itemTexts;
    [SerializeField] ItemData[] itemData;           // 아이템 글은 ItemData에서 읽음 → 게임 속 설명과 늘 같음

    // 규칙 글(title-menu 56 다듬기): 짧은 이름표 + 줄 맞춘 표. TMP 태그 — <color=#..> 색, <pos=n%> 그 자리에서 시작.
    // 색: 금색 = 이름표·키, 초록 ▲ = 상승, 빨강 ▼ = 하락·피해, 흐린 크림 = 표 머리
    [SerializeField, TextArea(6, 12)] string rulesPage1 =
        "<color=#D9B45A>목표</color><pos=22%>상대 잔고를 먼저 0으로 (잔고 10, 3라운드)\n" +
        "<color=#D9B45A>패배</color><pos=22%>내 잔고가 0이 되면\n" +
        "<color=#D9B45A>내 차례</color><pos=22%>숨겨진 캔들 한 장에 매수 또는 매도\n" +
        "<color=#A39A8C>행동<pos=22%>캔들<pos=44%>결과</color>\n" +
        "매수<pos=22%><color=#6CC27F>▲ 상승</color><pos=44%>레버리지 +1, <color=#D9B45A>한 번 더</color>\n" +
        "매수<pos=22%><color=#E46A58>▼ 하락</color><pos=44%><color=#E46A58>내 잔고 −(1+레버리지)</color>\n" +
        "매도<pos=22%><color=#E46A58>▼ 하락</color><pos=44%><color=#E46A58>상대 잔고 −(1+레버리지)</color>\n" +
        "매도<pos=22%><color=#6CC27F>▲ 상승</color><pos=44%>변화 없음";
    [SerializeField, TextArea(2, 4)] string rulesPage1Footer =
        "\"한 번 더\"가 아니면 차례가 넘어가요.";
    [SerializeField, TextArea(2, 4)] string rulesPage2 =
        "<color=#D9B45A>레버리지</color>  0~3 공용 · 다음 피해 +1씩 · 피해 후 0";
    [SerializeField, TextArea(2, 4)] string rulesPage2Footer =
        "아이템은 라운드 시작·덱을 다시 채울 때 2개씩 (최대 4개)";
    [SerializeField, TextArea(4, 8)] string rulesPage3 =
        "<color=#D9B45A>Space · 클릭</color><pos=36%>모니터 열기\n" +
        "<color=#D9B45A>← → · A D</color><pos=36%>매수·매도 고르기\n" +
        "<color=#D9B45A>1 ~ 4</color><pos=36%>아이템 확인\n" +
        "<color=#D9B45A>Esc</color><pos=36%>뒤로 · 일시정지";

    [Header("설정 (title-menu 54) — 소리를 바꾸면 클릭음으로 바로 들려 줌")]
    [SerializeField] AudioSource sfx;
    [SerializeField] AudioClip clickClip;
    const int SettingRows = 4;   // 전체 소리 · 음악 · 화면 흔들림 · 전체화면
    public int SettingRow { get; private set; }

    [Header("크레딧 (title-menu 55) — 공개 저장소라 본명·학번·학교는 쓰지 않음(GitHub 이름만)")]
    [SerializeField, TextArea(8, 16)] string credits =
        "Falling Knife\n" +
        "만든 사람   Kmg926\n" +
        "엔진   Unity 6.6 (URP 2D)\n" +
        "글꼴   Galmuri11 — Lee Minseo (quiple)\n" +
        "        SIL Open Font License 1.1\n" +
        "        github.com/quiple/galmuri\n" +
        "그림·효과음   Claude(Anthropic)가 작성한 코드로 생성,\n" +
        "        고르기·고칠 곳 지시는 만든 사람\n" +
        "코드 도움   Claude(Anthropic)\n" +
        "배경음악   (정해지면 추가)\n" +
        "비영리 학교 과제";
    public int CreditPage { get; private set; } = 1;   // TMP 쪽 번호(1부터)
    public int CreditPages => panelBody != null ? Mathf.Max(1, panelBody.textInfo.pageCount) : 1;
    TextOverflowModes bodyOverflow;   // 크레딧만 쪽 나누기(Page) — 닫으면 원래 방식으로

    static readonly string[] PageTitles = { "규칙 1/3 · 목표와 턴", "규칙 2/3 · 레버리지와 아이템", "규칙 3/3 · 조작" };
    const int PageCount = 3;

    public int Selected { get; private set; }
    public Panel Open { get; private set; } = Panel.None;
    public int Page { get; private set; }

    Camera uiCamera;

    void Awake()
    {
        Time.timeScale = 1f;   // 일시정지 중에 돌아와도 정상 속도
        Canvas canvas = GetComponentInParent<Canvas>();
        uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        if (panelRoot != null) panelRoot.SetActive(false);
        if (panelBody != null) bodyOverflow = panelBody.overflowMode;
        for (int i = 0; i < itemTexts.Length && i < itemData.Length; i++)
            itemTexts[i].text = $"<color=#D9B45A>{itemData[i].displayName}</color>  {itemData[i].description}";   // 이름은 금색
        RefreshMenu();
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (Open != Panel.None)
        {
            if (kb != null && kb.escapeKey.wasPressedThisFrame) { Back(); return; }
            if (Open == Panel.Settings) { UpdateSettings(kb, mouse); return; }
            if (Open == Panel.Credits) { UpdateCredits(kb, mouse); return; }
            if (Open == Panel.Rules)
            {
                if (kb != null && (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)) Turn(-1);
                else if (kb != null && (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)) Turn(1);
                else if (mouse != null && mouse.leftButton.wasPressedThisFrame) Turn(1);
            }
            else if ((kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)) || (mouse != null && mouse.leftButton.wasPressedThisFrame))
                Back();
            return;
        }

        // 마우스를 올린 줄을 고름(움직였을 때만 — 키보드로 고른 줄을 가만히 있는 마우스가 덮어쓰지 않게)
        int hovered = -1;
        if (mouse != null)
        {
            Vector2 pos = mouse.position.ReadValue();
            for (int i = 0; i < items.Length; i++)
                if (RectTransformUtility.RectangleContainsScreenPoint(items[i].rectTransform, pos, uiCamera)) hovered = i;
            if (hovered >= 0 && mouse.delta.ReadValue().sqrMagnitude > 0f && hovered != Selected) { Selected = hovered; RefreshMenu(); }
        }

        if (kb != null)
        {
            if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) Move(-1);
            else if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) Move(1);
            else if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) Choose();
        }
        if (mouse != null && mouse.leftButton.wasPressedThisFrame && hovered >= 0)
        {
            Selected = hovered;
            Choose();
        }
    }

    // 위·아래로 한 줄(끝에서 반대쪽으로 돌아감)
    public void Move(int dir)
    {
        Selected = (Selected + dir + items.Length) % items.Length;
        RefreshMenu();
    }

    // 고른 줄 실행
    public void Choose()
    {
        switch (Selected)
        {
            case 0: SceneManager.LoadScene(gameSceneName); break;
            case 1: OpenRules(); break;
            case 2: OpenSettings(); break;
            case 3: OpenCredits(); break;
            default: Quit(); break;
        }
    }

    // 창 닫기(Esc). 첫 화면에선 아무 일 없음
    public void Back()
    {
        if (Open == Panel.None) return;
        Open = Panel.None;
        panelBody.overflowMode = bodyOverflow;   // 크레딧의 쪽 나누기를 되돌림
        panelBody.pageToDisplay = 1;
        panelRoot.SetActive(false);
    }

    // 규칙 쪽 넘기기(처음·끝에서 멈춤)
    public void Turn(int dir)
    {
        if (Open != Panel.Rules) return;
        Page = Mathf.Clamp(Page + dir, 0, PageCount - 1);
        ShowPage();
    }

    void OpenRules()
    {
        Open = Panel.Rules;
        Page = 0;
        panelRoot.SetActive(true);
        ShowPage();
    }

    // ---------- 설정 창 (title-menu 54) ----------
    void UpdateSettings(Keyboard kb, Mouse mouse)
    {
        // 마우스를 올린 줄을 고름(움직였을 때만), 클릭하면 그 줄을 한 단계(오른쪽) 바꿈
        if (mouse != null)
        {
            int line = TMP_TextUtilities.FindIntersectingLine(panelBody, mouse.position.ReadValue(), uiCamera);
            if (line >= 0 && line < SettingRows && mouse.delta.ReadValue().sqrMagnitude > 0f && line != SettingRow) { SettingRow = line; ShowSettings(); }
            if (mouse.leftButton.wasPressedThisFrame && line >= 0 && line < SettingRows) { SettingRow = line; ChangeSetting(1); return; }
        }
        if (kb == null) return;
        if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) MoveSetting(-1);
        else if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) MoveSetting(1);
        else if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) ChangeSetting(-1);
        else if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame) ChangeSetting(1);
    }

    // 설정 줄 위·아래(돎)
    public void MoveSetting(int dir)
    {
        if (Open != Panel.Settings) return;
        SettingRow = (SettingRow + dir + SettingRows) % SettingRows;
        ShowSettings();
    }

    // 고른 줄의 값을 바꿈: 소리는 10% 단위(0~100%에서 멈춤), 켬/끔은 방향과 상관없이 바뀜. 바꿀 때마다 저장
    public void ChangeSetting(int dir)
    {
        if (Open != Panel.Settings) return;
        switch (SettingRow)
        {
            case 0: GameSettings.Master += dir; break;
            case 1: GameSettings.Music += dir; break;
            case 2: GameSettings.Shake = !GameSettings.Shake; break;
            default: GameSettings.Fullscreen = !GameSettings.Fullscreen; break;
        }
        // 소리 크기를 바로 들려 줌(전체 소리 크기로)
        if (sfx != null && clickClip != null && GameSettings.MasterVolume > 0f) sfx.PlayOneShot(clickClip, GameSettings.MasterVolume);
        ShowSettings();
    }

    void OpenSettings()
    {
        Open = Panel.Settings;
        SettingRow = 0;
        panelRoot.SetActive(true);
        itemRows.SetActive(false);
        panelTitle.text = "설정";
        panelFooter.text = "↑↓ 고르기   ←→ 바꾸기   Esc 닫기";
        ShowSettings();
    }

    void ShowSettings()
    {
        string[] names = { "전체 소리", "음악", "화면 흔들림", "전체화면" };
        string[] values =
        {
            Bar(GameSettings.Master), Bar(GameSettings.Music),
            GameSettings.Shake ? "켬" : "끔", GameSettings.Fullscreen ? "켬" : "끔",
        };
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < SettingRows; i++)
        {
            bool on = i == SettingRow;
            // <pos=40%>: 이름 길이와 상관없이 값이 같은 자리에서 시작(TMP 태그)
            string line = $"{(on ? "▶ " : "   ")}{names[i]}<pos=40%>{values[i]}";
            sb.Append(on ? $"<color=#{ColorUtility.ToHtmlStringRGB(selectedColor)}>{line}</color>" : line);
            if (i < SettingRows - 1) sb.Append('\n');
        }
        panelBody.text = sb.ToString();
    }

    // 소리 막대: ■ = 켜진 칸, □ = 꺼진 칸, 뒤에 %
    static string Bar(int step) => new string('■', step) + new string('□', GameSettings.Steps - step) + $" {step * 10}%";

    // ---------- 크레딧 창 (title-menu 55) ----------
    void UpdateCredits(Keyboard kb, Mouse mouse)
    {
        if (kb != null && (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame)) TurnCredits(-1);
        else if (kb != null && (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)) TurnCredits(1);
        else if (mouse != null)
        {
            float wheel = mouse.scroll.ReadValue().y;
            if (wheel < 0f) TurnCredits(1);
            else if (wheel > 0f) TurnCredits(-1);
            else if (mouse.leftButton.wasPressedThisFrame) TurnCredits(1);
        }
    }

    // 크레딧 쪽 넘기기(처음·끝에서 멈춤). 글이 창보다 길면 TMP가 쪽을 나눠 줌(overflowMode = Page)
    public void TurnCredits(int dir)
    {
        if (Open != Panel.Credits) return;
        CreditPage = Mathf.Clamp(CreditPage + dir, 1, CreditPages);
        panelBody.pageToDisplay = CreditPage;
        ShowCreditsFooter();
    }

    void OpenCredits()
    {
        Open = Panel.Credits;
        panelRoot.SetActive(true);
        itemRows.SetActive(false);
        panelTitle.text = "크레딧";
        panelBody.overflowMode = TextOverflowModes.Page;
        panelBody.text = credits;
        CreditPage = 1;
        panelBody.pageToDisplay = 1;
        panelBody.ForceMeshUpdate();   // 몇 쪽인지 바로 알기 위해
        ShowCreditsFooter();
    }

    void ShowCreditsFooter()
    {
        panelFooter.text = CreditPages > 1
            ? $"↑↓ 넘기기 ({CreditPage}/{CreditPages})   Esc 닫기"
            : "Esc 닫기";
    }

    void OpenPlaceholder(string title)
    {
        Open = Panel.Placeholder;
        panelRoot.SetActive(true);
        itemRows.SetActive(false);
        panelTitle.text = title;
        panelBody.text = "준비 중이에요.";
        panelFooter.text = "Esc 닫기";
    }

    void ShowPage()
    {
        panelTitle.text = PageTitles[Page];
        itemRows.SetActive(Page == 1);
        panelBody.text = Page == 0 ? rulesPage1 : Page == 1 ? rulesPage2 : rulesPage3;
        string tail = Page == 0 ? rulesPage1Footer + "\n" : Page == 1 ? rulesPage2Footer + "\n" : "";
        panelFooter.text = tail + (Page < PageCount - 1 ? "←/→ 쪽 넘기기   Esc 닫기" : "← 앞 쪽   Esc 닫기");
    }

    void RefreshMenu()
    {
        for (int i = 0; i < items.Length; i++)
        {
            bool on = i == Selected;
            items[i].text = (on ? "▶ " : "   ") + labels[i];
            items[i].color = on ? selectedColor : normalColor;
        }
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
