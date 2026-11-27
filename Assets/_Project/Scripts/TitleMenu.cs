using System.Collections;
using System.Collections.Generic;
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
    public enum Panel { None, Rules, Settings, Credits, Placeholder, AskTutorial }   // tutorial 98: 처음 권유 창

    [Header("메뉴 (위에서부터 시작·튜토리얼·규칙·설정·크레딧·종료)")]
    [SerializeField] TMP_Text[] items;
    [SerializeField] string[] labels = { "시작", "튜토리얼", "규칙", "설정", "크레딧", "종료" };   // tutorial 98: 튜토리얼 추가
    const string TutorialAskedKey = "fk.tutorialAsked";   // tutorial 98: 처음 권유를 이미 했나(저장)

    [Header("고른 줄 꾸밈 (title-polish 104, 레퍼런스 Halo 띠 · Hollow Knight 장식)")]
    [SerializeField] RectTransform selectBar;    // 고른 줄 뒤 금색 띠(오른쪽으로 사라짐). Menu 안, 글자보다 뒤
    [SerializeField] Vector2 barOffset = new Vector2(-6f, 1f);    // 줄 왼쪽 위 기준 띠 자리
    // 2026-11-27 사용자 "칼은 없애자": 칼(knife) 칸·자리 값 삭제. 띠가 연결돼 있으면 띠만으로 고른 줄을 보여 줌
    public int AskRow { get; private set; }               // 권유 창: 0 = 튜토리얼 먼저, 1 = 바로 시작
    [SerializeField] Color selectedColor = new Color32(0xD9, 0xB4, 0x5A, 255);   // gold2
    [SerializeField] Color normalColor = new Color32(0xA3, 0x9A, 0x8C, 255);     // cream0
    [SerializeField] string gameSceneName = "GameScene";
    // intro-story 77: 시작 → 화면 전체 검정이 0.5초 동안 짙어진 뒤 GameScene(거기서 검정 → 밝아지며 시작 스토리)
    [SerializeField] Image fade;
    [SerializeField] float fadeOutSeconds = 0.5f;
    bool starting;   // 어두워지는 중 = 입력 무시(두 번 눌러도 한 번만)

    [Header("창 (규칙 3쪽 · 설정 · 크레딧 · 권유)")]
    [SerializeField] GameObject panelRoot;          // 뒤 어둡게 + 창
    [SerializeField] TMP_Text panelTitle;
    [SerializeField] TMP_Text panelBody;
    [SerializeField] TMP_Text panelFooter;
    [SerializeField] GameObject itemRows;           // 규칙 3쪽: 아이템 7줄(아이콘 + 글)
    [SerializeField] TMP_Text[] itemTexts;
    [SerializeField] ItemData[] itemData;           // 아이템 글은 ItemData에서 읽음 → 게임 속 설명과 늘 같음

    // rules-card 105(2026-11-27): 규칙 = 잊었을 때 찾아보는 카드 3쪽, 배우는 건 튜토리얼(Grilling Q1).
    // 튜토리얼과 같은 말을 씀: 피해는 수식 대신 레버리지 상자의 "다음 피해"(Q2), 아이템은 "폰"에서 고름.
    // 필드 이름을 새로 지음 → Scene에 저장된 옛 글(rulesPage1 등)이 새 글을 덮어쓰지 않음.
    // TMP 태그 — <color=#..> 색, <pos=n%> 그 자리에서 시작. 금색 = 이름표·키, 초록 ▲ 상승, 빨강 ▼ 하락·피해, 흐린 크림 = 표 머리

    [Header("규칙 1쪽 · 화면 (레퍼런스: 지도 위 번호 + 옆 이름표)")]
    [SerializeField] GameObject rulesScreen;        // 1쪽에서만 켬: 번호 달린 게임 화면 그림 + 이름표 + 아래 글
    [SerializeField] TMP_Text rulesScreenLegend;    // 그림 오른쪽 이름표(번호 = 그림의 배지 숫자, tools/make_rules_screen.py SPOTS와 같은 순서)
    [SerializeField] TMP_Text rulesScreenNotes;     // 그림 아래 목표 한 줄 + 조작 두 줄
    [SerializeField, TextArea(7, 9)] string rulesScreenLegendText =
        "<color=#D9B45A>1</color> 모니터\n" +
        "<color=#D9B45A>2</color> 레버리지\n" +
        "<color=#D9B45A>3</color> 내 잔고\n" +
        "<color=#D9B45A>4</color> 광대 잔고\n" +
        "<color=#D9B45A>5</color> 내 폰\n" +
        "<color=#D9B45A>6</color> 광대 폰\n" +
        "<color=#D9B45A>7</color> 덱";
    [SerializeField, TextArea(3, 5)] string rulesScreenNotesText =
        "<color=#D9B45A>목표</color>  상대 잔고를 0으로. 내 잔고가 0이면 패배\n" +
        "<color=#D9B45A>Space·클릭</color> 모니터 열기·실행   <color=#D9B45A>← →</color> 매수·매도\n" +
        "<color=#D9B45A>폰·1~4</color> 아이템   <color=#D9B45A>Esc</color> 닫기·일시정지";

    [Header("규칙 2쪽 · 행동")]
    [SerializeField, TextArea(8, 12)] string rulesActions =
        // 9줄이라 줄 높이를 조금 줄임(<line-height>) — 아래 안내 줄과 겹치지 않게
        "<line-height=88%><color=#D9B45A>내 차례</color>에 숨겨진 캔들을 두고 매수나 매도를 고른다.\n" +
        "<color=#A39A8C>행동<pos=20%>캔들<pos=40%>결과</color>\n" +
        "매수<pos=20%><color=#6CC27F>▲ 상승</color><pos=40%>레버리지 +1, <color=#D9B45A>한 번 더</color>\n" +
        "매수<pos=20%><color=#E46A58>▼ 하락</color><pos=40%><color=#E46A58>내가 다음 피해만큼 잃음</color>\n" +
        "매도<pos=20%><color=#E46A58>▼ 하락</color><pos=40%><color=#E46A58>상대가 다음 피해만큼 잃음</color>\n" +
        "매도<pos=20%><color=#6CC27F>▲ 상승</color><pos=40%>변화 없음\n" +
        "<color=#D9B45A>레버리지</color>가 쌓일수록 다음 피해가 커진다(최대 3).\n" +
        "피해가 나거나 라운드가 바뀌면 레버리지는 0.\n" +
        "<color=#D9B45A>덱</color> 개수는 채울 때만 보인다. 남은 수는 직접 센다.";
    [SerializeField, TextArea(1, 2)] string rulesActionsFooter =
        "\"한 번 더\"가 아니면 차례가 상대에게 넘어간다.";

    [Header("설정 (title-menu 54) — game-settings 91: 내용은 공통 부품 SettingsPanel(panelBody에 그림)")]
    [SerializeField] SettingsPanel settings;

    [Header("크레딧 (title-menu 55) — 공개 저장소라 본명·학번·학교는 쓰지 않음(GitHub 이름만)")]
    [SerializeField, TextArea(8, 16)] string credits =
        "Falling Knife\n" +
        "만든 사람   Kmg926\n" +
        "엔진   Unity 6.6 (URP 2D)\n" +
        "글꼴   Galmuri11 — Lee Minseo (quiple)\n" +
        "        SIL Open Font License 1.1\n" +
        "        github.com/quiple/galmuri\n" +
        "그림·효과음   Claude(Anthropic)가 작성한 코드로 생성\n" +
        "        선택·수정 지시: 만든 사람\n" +
        "코드 도움   Claude(Anthropic)\n" +
        "배경음악   xjj9 — prod. by cryalxt\n" +
        "비영리 학교 과제";
    public int CreditPage { get; private set; } = 1;   // TMP 쪽 번호(1부터)
    public int CreditPages => panelBody != null ? Mathf.Max(1, panelBody.textInfo.pageCount) : 1;
    TextOverflowModes bodyOverflow;   // 크레딧만 쪽 나누기(Page) — 닫으면 원래 방식으로

    // rules-card 105: 5쪽 → 3쪽(화면 · 행동 · 아이템). rules-card 123: 아이템 설명을 줄이지 않으므로 아이템 쪽은 필요한 만큼 늘어난다
    static readonly string[] FixedTitles = { "화면", "행동" };
    const int ItemPage = 2;   // 첫 아이템 쪽 번호(이 뒤로 아이템 쪽이 이어짐)
    readonly List<int> itemPageStart = new List<int>();   // 아이템 쪽마다 첫 아이템 번호(OpenRules 때 계산)
    int PageCount => ItemPage + Mathf.Max(1, itemPageStart.Count);

    [Header("규칙 아이템 쪽 나누기 (rules-card 123)")]
    [SerializeField] float itemRowsTopFirst = -50f;   // 첫 아이템 쪽: 소개 글 아래에서 시작
    [SerializeField] float itemRowsTopRest = -23f;    // 둘째 쪽부터: 소개 글 없이 본문 자리에서 시작
    [SerializeField] float itemRowsBottom = -138f;    // 이 아래는 쪽 안내 글 자리
    [SerializeField] float itemRowGap = 3f;           // 아이템 사이 빈칸
    [SerializeField] float itemRowMinHeight = 10f;    // 한 줄짜리도 아이콘(8) 높이는 넘게

    [Header("규칙 3쪽 · 아이템 (줄 7개, heal-items 89 아이콘 그림)")]
    [SerializeField] Image[] itemIcons;             // 줄마다 아이콘(7개)
    [SerializeField] Sprite[] itemIconSprites;      // ItemType 순서 7개
    [SerializeField, TextArea(2, 3)] string rulesItemsIntro =
        "라운드 시작·덱 재장전 때 2개씩 받는다(최대 4개).\n" +
        "내 폰을 열어 고르고 Space로 쓴다.";

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
        if (fade != null) fade.gameObject.SetActive(false);
        if (panelBody != null) bodyOverflow = panelBody.overflowMode;
        for (int i = 0; i < itemTexts.Length && i < itemData.Length; i++)
            itemTexts[i].text = $"<color=#D9B45A>{itemData[i].displayName}</color>  {itemData[i].description}";   // 이름은 금색
        RefreshMenu();
    }

    void Update()
    {
        if (starting) return;
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (Open != Panel.None)
        {
            if (kb != null && kb.escapeKey.wasPressedThisFrame) { Back(); return; }
            if (Open == Panel.Settings) { UpdateSettings(kb, mouse); return; }
            if (Open == Panel.AskTutorial) { UpdateAsk(kb, mouse); return; }   // tutorial 98
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
            // tutorial 98: 처음 `시작`이면 한 번만 튜토리얼을 권함(Grilling Q10)
            case 0:
                if (PlayerPrefs.GetInt(TutorialAskedKey, 0) == 0) OpenAsk();
                else StartCoroutine(StartGame(false));
                break;
            case 1: StartCoroutine(StartGame(true)); break;   // tutorial 98
            case 2: OpenRules(); break;
            case 3: OpenSettings(); break;
            case 4: OpenCredits(); break;
            default: Quit(); break;
        }
    }

    // ---------- 튜토리얼 권유 창 (tutorial 98) ----------
    void OpenAsk()
    {
        Open = Panel.AskTutorial;
        AskRow = 0;
        panelRoot.SetActive(true);
        itemRows.SetActive(false);
        panelTitle.text = "처음이라면";
        panelFooter.text = "↑↓ 고르기   Space 결정   Esc 닫기";
        ShowAsk();
    }

    void ShowAsk()
    {
        string[] rows = { "튜토리얼 먼저 (약 2분)", "바로 시작" };
        var sb = new System.Text.StringBuilder("광대가 규칙을 알려 주는 짧은 연습 판이 있다.\n\n");
        for (int i = 0; i < rows.Length; i++)
        {
            bool on = i == AskRow;
            string line = (on ? "▶ " : "   ") + rows[i];
            sb.Append(on ? $"<color=#{ColorUtility.ToHtmlStringRGB(selectedColor)}>{line}</color>" : line);
            if (i < rows.Length - 1) sb.Append('\n');
        }
        panelBody.text = sb.ToString();
    }

    // 2·3번째 줄(빈 줄 다음)이 선택지. 마우스를 올리면 그 줄, 클릭·Space·Enter = 결정
    void UpdateAsk(Keyboard kb, Mouse mouse)
    {
        if (mouse != null)
        {
            int line = TMP_TextUtilities.FindIntersectingLine(panelBody, mouse.position.ReadValue(), uiCamera) - 2;
            if (line >= 0 && line <= 1 && mouse.delta.ReadValue().sqrMagnitude > 0f && line != AskRow) { AskRow = line; ShowAsk(); }
            if (mouse.leftButton.wasPressedThisFrame && line >= 0 && line <= 1) { AnswerAsk(line); return; }
        }
        if (kb == null) return;
        if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame)
        {
            AskRow = 1 - AskRow;
            ShowAsk();
        }
        else if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) AnswerAsk(AskRow);
    }

    // 답하면 저장(다시 안 물음) → 튜토리얼 또는 실전
    public void AnswerAsk(int row)
    {
        if (Open != Panel.AskTutorial) return;
        PlayerPrefs.SetInt(TutorialAskedKey, 1);
        PlayerPrefs.Save();
        Back();
        StartCoroutine(StartGame(row == 0));
    }

    // intro-story 77: 검정이 짙어진 뒤 시작 스토리를 켜고 GameScene으로
    // tutorial 98: tutorial = true면 시작 스토리 대신 튜토리얼
    IEnumerator StartGame(bool tutorial)
    {
        starting = true;
        if (fade != null)
        {
            fade.gameObject.SetActive(true);
            for (float t = 0f; t < fadeOutSeconds; t += Time.unscaledDeltaTime)
            {
                fade.color = new Color(0f, 0f, 0f, t / fadeOutSeconds);
                yield return null;
            }
            fade.color = Color.black;
        }
        if (tutorial) TutorialGuide.PlayNext = true;
        else IntroStory.PlayNext = true;
        SceneManager.LoadScene(gameSceneName);
    }

    // 창 닫기(Esc). 첫 화면에선 아무 일 없음
    public void Back()
    {
        if (Open == Panel.None) return;
        if (Open == Panel.Settings) settings.End();   // 95: X 숨김(규칙·크레딧 창엔 X 없음)
        Open = Panel.None;
        panelBody.overflowMode = bodyOverflow;   // 크레딧의 쪽 나누기를 되돌림
        panelBody.pageToDisplay = 1;
        if (rulesScreen != null) rulesScreen.SetActive(false);   // rules-card 105: 다른 창(설정·크레딧)에 그림이 남지 않게
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
        LayoutItemPages();
        ShowPage();
    }

    // rules-card 123: 아이템 한 줄(이름 금색 + 게임 안 설명 그대로)
    string ItemLine(int i) => $"<color=#D9B45A>{itemData[i].displayName}</color>  {itemData[i].description}";

    // 아이템 줄 높이 = 글이 실제로 차지하는 높이(줄바꿈 포함)
    float ItemRowHeight(int i)
    {
        TMP_Text t = itemTexts[0];
        float h = t.GetPreferredValues(ItemLine(i), t.rectTransform.rect.width, 0f).y;
        return Mathf.Max(h, itemRowMinHeight);
    }

    // rules-card 123: 아이템을 위에서부터 쌓다가 아래 한계를 넘으면 다음 쪽으로 — 쪽마다 첫 아이템 번호를 남김
    void LayoutItemPages()
    {
        itemPageStart.Clear();
        int count = Mathf.Min(itemTexts.Length, itemData.Length);
        float y = 0f, room = itemRowsTopFirst - itemRowsBottom;   // 첫 쪽은 소개 글 때문에 자리가 좁다
        for (int i = 0; i < count; i++)
        {
            float h = ItemRowHeight(i);
            if (itemPageStart.Count == 0 || (y > 0f && y + h > room))
            {
                if (itemPageStart.Count > 0) room = itemRowsTopRest - itemRowsBottom;
                itemPageStart.Add(i);
                y = 0f;
            }
            y += h + itemRowGap;
        }
    }

    // ---------- 설정 창 (title-menu 54 → game-settings 91: 내용은 공통 SettingsPanel이 맡음) ----------
    // 이 화면은 창을 열고 닫는 일(제목·아래 안내)만. 줄 고르기·값 바꾸기·저장·글은 SettingsPanel(게임 중 일시정지와 같은 부품)
    void UpdateSettings(Keyboard kb, Mouse mouse)
    {
        settings.HandleInput(kb, mouse);
        if (settings.CloseClicked) Back();   // 95: X = Esc와 같은 닫기
    }

    public int SettingRow => settings.Row;

    public void MoveSetting(int dir)
    {
        if (Open != Panel.Settings) return;
        settings.Move(dir);
    }

    public void ChangeSetting(int dir)
    {
        if (Open != Panel.Settings) return;
        settings.Change(dir);
    }

    void OpenSettings()
    {
        Open = Panel.Settings;
        panelRoot.SetActive(true);
        itemRows.SetActive(false);
        panelTitle.text = "설정";
        panelFooter.text = "↑↓ 고르기   ←→ 바꾸기   Esc 닫기";
        settings.Begin();
    }

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
        panelBody.text = "준비 중이다.";
        panelFooter.text = "Esc 닫기";
    }

    void ShowPage()
    {
        string section = Page < ItemPage ? FixedTitles[Page] : "아이템";
        panelTitle.text = $"규칙 {Page + 1}/{PageCount} · {section}";
        // rules-card 105: 1쪽 = 그림 묶음(본문 글은 비움), 3쪽부터 = 아이템 줄
        if (rulesScreen != null) rulesScreen.SetActive(Page == 0);
        if (Page == 0)
        {
            if (rulesScreenLegend != null) rulesScreenLegend.text = rulesScreenLegendText;
            if (rulesScreenNotes != null) rulesScreenNotes.text = rulesScreenNotesText;
        }
        bool itemPage = Page >= ItemPage;
        itemRows.SetActive(itemPage);
        if (itemPage)
        {
            // rules-card 123: 이 쪽에 들어갈 아이템만. 첫 아이템 쪽만 소개 글이 있고 줄은 그 아래에서 시작
            int k = Page - ItemPage;
            int first = k < itemPageStart.Count ? itemPageStart[k] : 0;
            int next = k + 1 < itemPageStart.Count ? itemPageStart[k + 1] : itemData.Length;
            var box = (RectTransform)itemRows.transform;
            box.anchoredPosition = new Vector2(box.anchoredPosition.x, k == 0 ? itemRowsTopFirst : itemRowsTopRest);
            ShowItemRows(first, next - first);
        }
        panelBody.text = Page == 1 ? rulesActions : Page == ItemPage ? rulesItemsIntro : "";
        string tail = Page == 1 ? rulesActionsFooter + "\n" : "";
        panelFooter.text = tail + (Page < PageCount - 1 ? "←/→ 쪽 넘기기   Esc 닫기" : "← 앞 쪽   Esc 닫기");
    }

    // heal-items 89: 아이템 줄에 first번째부터 count개(ItemType 순서) — 남는 줄은 숨김.
    // rules-card 123: 줄 높이가 설명 길이마다 다르므로 위에서부터 차례로 쌓는다(글·아이콘 위치를 코드로 정함)
    void ShowItemRows(int first, int count)
    {
        float y = 0f;
        for (int row = 0; row < itemTexts.Length; row++)
        {
            int i = first + row;
            bool show = row < count && i < itemData.Length;
            itemTexts[row].gameObject.SetActive(show);
            if (itemIcons != null && row < itemIcons.Length) itemIcons[row].gameObject.SetActive(show);
            if (!show) continue;
            float h = ItemRowHeight(i);
            RectTransform t = itemTexts[row].rectTransform;
            itemTexts[row].text = ItemLine(i);   // 이름은 금색, 설명은 게임 안 그대로
            t.anchoredPosition = new Vector2(t.anchoredPosition.x, -y);
            t.sizeDelta = new Vector2(t.sizeDelta.x, h);
            if (itemIcons != null && row < itemIcons.Length)
            {
                RectTransform r = itemIcons[row].rectTransform;
                r.anchoredPosition = new Vector2(r.anchoredPosition.x, -y - 1f);   // 아이콘은 첫 줄 높이에 맞춤
                if (itemIconSprites != null && i < itemIconSprites.Length) itemIcons[row].sprite = itemIconSprites[i];
            }
            y += h + itemRowGap;
        }
    }

    float arrowWidth = -1f;   // "▶ " 폭(처음 한 번 잼)

    void RefreshMenu()
    {
        // 사용자(2026-11-19): 메뉴 글자를 부제 "떨어지는"과 같은 왼쪽 선에 → 앞 빈칸 없이 글자만 두고,
        // 고른 줄의 "▶ "는 그 폭만큼 왼쪽 여백(margin)을 음수로 줘서 선 바깥(왼쪽)에 매단다 → 글자 시작은 늘 같은 선
        // 끝 빈칸은 폭을 잴 때 빠지므로 "▶ 가" − "가"로 잼(처음엔 "▶ "만 재서 고른 줄 글자가 조금 오른쪽에 남았음)
        if (arrowWidth < 0f && items.Length > 0)
            arrowWidth = items[0].GetPreferredValues("▶ 가").x - items[0].GetPreferredValues("가").x;
        bool useBar = selectBar != null;   // 104: 띠가 있으면 ▶ 글자는 안 씀(글자 시작 선은 그대로)
        for (int i = 0; i < items.Length; i++)
        {
            bool on = i == Selected;
            bool arrow = on && !useBar;
            items[i].text = (arrow ? "▶ " : "") + labels[i];
            items[i].margin = new Vector4(arrow ? -arrowWidth : 0f, 0f, 0f, 0f);
            items[i].color = on ? selectedColor : normalColor;
        }
        // 104: 띠를 고른 줄 높이로 옮김(줄과 같은 부모 Menu 안이라 줄의 anchoredPosition을 그대로 씀)
        if (useBar && Selected >= 0 && Selected < items.Length)
            selectBar.anchoredPosition = items[Selected].rectTransform.anchoredPosition + barOffset;
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
