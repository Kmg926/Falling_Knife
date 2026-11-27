using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 튜토리얼 (tutorial 97). 타이틀의 `튜토리얼`로 GameScene에 왔을 때만 켜진다.
// 광대가 대화창으로 한두 줄 설명 → 그 행동만 할 수 있게 열어 줌 → 해내면 다음 단계(해 보면서 배우기, Grilling Q2).
// 판정은 하지 않는다: GameManager 상태를 읽고, "지금 말하는 중(입력 막기)"과 "허용할 행동"만 바꾼다.
// 덱 순서·아이템·광대 행동은 GameManager의 튜토리얼 값(각본, Q3)이라 매번 같은 결과가 나온다.
public class TutorialGuide : MonoBehaviour
{
    // 타이틀의 `튜토리얼`(또는 처음 권유)이 켠다 → 다음에 열리는 GameScene이 튜토리얼로 시작하고 끈다.
    public static bool PlayNext;

    [SerializeField] GameManager game;
    [SerializeField] AudioManager audioManager;

    [Header("대화창 (시작 스토리 것을 같이 씀)")]
    [SerializeField] GameObject introRoot;        // Intro/Root (검정·대화창·건너뛰기 안내)
    [SerializeField] GameObject black;            // 시작 스토리 검정 화면 — 튜토리얼에선 끔
    [SerializeField] GameObject dialogBox;
    [SerializeField] TMP_Text lineText;
    [SerializeField] GameObject nextMark;         // ▼
    [SerializeField] GameObject choicesRoot;      // 끝 선택(시작 / 타이틀)
    [SerializeField] TMP_Text[] choiceTexts;
    [SerializeField] CanvasGroup skipHint;
    [SerializeField] Image skipFill;

    [Header("할 일 (Q7)")]
    [SerializeField] GameObject taskRoot;         // 위쪽 가운데 할 일 한 줄
    [SerializeField] TMP_Text taskText;
    [SerializeField] RectTransform highlight;     // 금색 테두리 틀(시작할 때 선 4개를 코드로 붙임)
    [SerializeField] float highlightPadding = 3f;
    [SerializeField] float highlightThickness = 1f;   // 선 굵기(HUD 1칸 = 화면 4px)
    [SerializeField] Color highlightColor = new Color32(0xD9, 0xB4, 0x5A, 255);   // gold2
    Image[] edges;                                // 위·아래·왼쪽·오른쪽 선
    [SerializeField] RectTransform monitorButton;
    [SerializeField] RectTransform buyButton;
    [SerializeField] RectTransform sellButton;
    [SerializeField] RectTransform phoneButton;
    [SerializeField] RectTransform firstSlot;     // 폰 목록 첫 칸(차트 분석)
    [SerializeField] RectTransform useButton;     // 폰 상세의 사용 버튼
    // "여기를 봐"(Look): 대화창이 화면 아래를 가려서, 잠깐 숨기고 이 자리를 가리킨다
    [SerializeField] RectTransform leverageArea;
    [SerializeField] RectTransform myBalanceArea;
    [SerializeField] RectTransform clownBalanceArea;

    [Header("글자 (시작 스토리와 같은 값)")]
    [SerializeField] float charSeconds = 0.03f;
    [SerializeField] float punctSeconds = 0.2f;
    [SerializeField] float ellipsisSeconds = 0.4f;
    [SerializeField] float dotSeconds = 0.12f;
    [SerializeField] int charsPerSound = 2;
    [SerializeField, Range(0f, 1f)] float typeVolume = 0.3f;
    [SerializeField] float voicePitch = 0.75f;
    [SerializeField] float voicePitchJitter = 0.05f;
    [SerializeField] Color selectedColor = new Color32(0xD9, 0xB4, 0x5A, 255);   // gold2
    [SerializeField] Color normalColor = new Color32(0xA3, 0x9A, 0x8C, 255);     // cream0

    [Header("Space 길게 = 건너뛰기 (Q8, 대사 중에만)")]
    [SerializeField] float holdSeconds = 1f;
    [SerializeField] float tapSeconds = 0.25f;
    [SerializeField, Range(0f, 1f)] float idleAlpha = 0.4f;

    public bool IsActive { get; private set; }
    public int Step { get; private set; }          // 지금 단계(테스트·기록용)
    public int Choice { get; private set; }        // 끝 선택: 0 = 시작, 1 = 타이틀
    public bool IsChoosing { get; private set; }

    // 할 일 종류: 없음 / 매수·매도(모니터에서) / 폰에서 차트 분석
    enum Task { None, Trade, Phone }
    Task task;
    PlayerAction taskAction;

    bool talking;      // 대화창이 떠 있고 입력을 받는 중
    bool advance;      // 다음 줄(Enter·클릭 누름, Space 짧게 뗌)
    bool skip;
    float holdTime, progress;
    Camera uiCamera;
    // 테스트용: Unity 창에 포커스가 없으면 MCP 키 입력이 안 들어가서 — 켜면 "다음 줄"을 한 번 누른 것으로 침
    [NonSerialized] public bool simulateAdvance;
    // 테스트용: 켜 두면 대사를 매 프레임 넘김(끝까지 빨리 돌려 보기). 저장되지 않음
    [NonSerialized] public bool autoAdvance;

    void Awake()
    {
        if (!PlayNext)
        {
            enabled = false;
            if (taskRoot != null) taskRoot.SetActive(false);
            if (highlight != null) highlight.gameObject.SetActive(false);
            return;
        }
        PlayNext = false;
        IntroStory.PlayNext = false;   // 튜토리얼엔 시작 스토리 없음
        IsActive = true;
        // Awake는 모든 Start보다 먼저 → GameManager.Start(첫 라운드)가 튜토리얼 값으로 시작한다
        game.BeginTutorial();
    }

    void Start()
    {
        if (!IsActive) return;
        Canvas canvas = GetComponentInParent<Canvas>();
        uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        if (black != null) black.SetActive(false);
        if (choicesRoot != null) choicesRoot.SetActive(false);
        BuildEdges();
        ShowTask(false);
        StartCoroutine(Run());
    }

    // 강조 틀 = 금색 선 4개. 창 테두리 그림은 안쪽이 어두운 판이라 대상을 가려서, 그림 없는 단색 선으로(Image에 그림이 없으면 단색 사각형)
    void BuildEdges()
    {
        Image own = highlight.GetComponent<Image>();
        if (own != null) own.enabled = false;
        // (anchorMin, anchorMax, pivot) — 위·아래는 가로로 꽉, 왼쪽·오른쪽은 세로로 꽉
        Vector2[,] a =
        {
            { new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1) },
            { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0) },
            { new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f) },
            { new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f) },
        };
        edges = new Image[4];
        for (int i = 0; i < 4; i++)
        {
            var go = new GameObject("Edge" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = highlight.gameObject.layer;
            var r = (RectTransform)go.transform;
            r.SetParent(highlight, false);
            r.anchorMin = a[i, 0]; r.anchorMax = a[i, 1]; r.pivot = a[i, 2];
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta = i < 2 ? new Vector2(0f, highlightThickness) : new Vector2(highlightThickness, 0f);
            edges[i] = go.GetComponent<Image>();
            edges[i].color = highlightColor;
            edges[i].raycastTarget = false;
        }
    }

    // ───────────── 단계 (Spec 2) ─────────────
    IEnumerator Run()
    {
        yield return null;   // GameManager.Start(튜토리얼 라운드 준비)가 먼저 돌게 한 프레임

        Step = 0;
        yield return Talk("연습이다. 오늘은 잃을 게 없어.",
                          "규칙은 직접 해 보면서 배운다. 먼저 이번 덱을 보여 주지.");
        Release();                                       // → 덱 공개 연출이 시작됨
        yield return null;
        while (game.IsPresenting) yield return null;

        Step = 1;
        // 사용자 정정(2026-11-23): 모니터는 남은 개수를 보여 주지 않는다 — 개수는 덱을 채울 때 공개 연출로만
        yield return Talk($"방금 본 게 이번 덱이다. {game.Deck.Remaining}장, 하락 {game.Deck.RemainingDown}장·상승 {game.Deck.RemainingUp}장.",
                          "개수는 덱을 채울 때 한 번만 보여 준다. 순서는 아무도 모르지. 몇 장 남았는지는 직접 세어 둬.");
        yield return Look(monitorButton, "모니터: 공개된 캔들이 여기 그려진다");
        yield return Talk("매수·매도도 이 모니터를 열어서 고른다.");

        Step = 2;
        yield return Talk("먼저 <color=#D9B45A>매수</color>. 다음 캔들을 네가 들고 간다.",
                          "오르면 레버리지가 1 쌓이고, 네 차례가 계속된다.");
        yield return Act(Task.Trade, PlayerAction.Buy, 1);
        yield return Talk("올랐군.");
        yield return Look(leverageArea, $"레버리지 {game.Leverage.Value} · 다음 피해 {1 + game.Leverage.Value}");
        // 사용자: 피해를 수식(1 + 레버리지)으로 말하지 않기 → 레버리지 상자의 "다음 피해"를 가리키고 "판이 커진다"로
        yield return Talk("오를 때마다 레버리지가 쌓인다. 저 <color=#D9B45A>다음 피해</color>가 떨어졌을 때 맞는 양이다.",
                          "아직 네 차례다. 한 번 더 <color=#D9B45A>매수</color>해 봐.");

        Step = 3;
        int before = game.Player.Balance;
        yield return Act(Task.Trade, PlayerAction.Buy, 1);
        yield return Talk("떨어졌군. 들고 있던 네가 맞았다. 커진 판만큼 아프지.");
        yield return Look(myBalanceArea, $"내 잔고 {before} → {game.Player.Balance}");
        yield return Talk("맞으면 레버리지는 0으로 돌아가고, 차례는 나한테 넘어온다. 구경해.");

        Step = 4;
        yield return Watch();                            // 광대: 매도 → 상승
        yield return Talk("나는 매도했다. 그런데 올랐지. 매도한 캔들이 오르면 아무 일도 없다.",
                          "이번엔 네가 <color=#D9B45A>매도</color>해 봐. 캔들을 나한테 넘기는 거다.");

        Step = 5;
        int clownBefore = game.Clown.Balance;
        yield return Act(Task.Trade, PlayerAction.Sell, 1);
        yield return Talk("……넘긴 캔들이 떨어지면 받은 쪽이 맞는다.");
        yield return Look(clownBalanceArea, $"광대 잔고 {clownBefore} → {game.Clown.Balance}");
        yield return Talk("매도는 결과와 상관없이 차례를 넘긴다. 다시 내 차례.");

        Step = 6;
        yield return Watch();                            // 광대: 매수 → 하락
        yield return Talk("이번엔 들고 있던 내가 맞았군.",
                          "레버리지를 쌓아봐라. 오르는 동안은 계속 매수할 수 있다. <color=#D9B45A>매수</color> 두 번.");

        Step = 7;
        yield return Act(Task.Trade, PlayerAction.Buy, 2);
        yield return Talk($"판이 꽤 커졌군. 지금 떨어지면 {1 + game.Leverage.Value}이다.",
                          "레버리지는 3까지만 쌓인다.",   // rules-card 106: 규칙 창에만 있던 한도
                          "다음 캔들이 궁금하지? 네 폰을 열어 <color=#D9B45A>차트 분석</color>을 써라.");

        Step = 8;
        yield return Act(Task.Phone, PlayerAction.Buy, 0);
        yield return Talk("다음은 하락. 너만 봤다.",
                          "떨어질 걸 알았으면…… 나한테 넘겨라.");

        Step = 9;
        clownBefore = game.Clown.Balance;
        yield return Act(Task.Trade, PlayerAction.Sell, 1);
        yield return Talk("……키운 판을 그대로 받았군.");
        yield return Look(clownBalanceArea, $"광대 잔고 {clownBefore} → {game.Clown.Balance}");
        yield return Talk("내 잔고가 바닥났다. 상대 잔고를 0으로 만들면 이긴다. 진짜 판은 3라운드다.");

        Step = 10;
        yield return Talk("아이템은 라운드 시작과 덱을 다시 채울 때 2개씩 받는다. 4개까지.",   // rules-card 106
                          "아이템은 더 있다. <color=#D9B45A>찌라시</color>는 다음 캔들을 모두에게 보여 주고 버린다.",
                          "<color=#D9B45A>거래 정지</color>는 상대 차례를 한 번 건너뛰게 한다.",
                          "<color=#D9B45A>복권</color>은 운에 맡기는 보상. <color=#D9B45A>배당금</color>·<color=#D9B45A>구제 금융</color>은 잔고를 되찾는다.",
                          "<color=#D9B45A>작전</color>은 다음 캔들을 뒤집는다. 하락은 상승으로, 상승은 하락으로.",   // flip-item 101
                          "자세한 건 폰에서 아이템을 고르면 나온다.");

        Step = 11;
        yield return Type("이제 진짜다.");
        yield return Choose();
        if (Choice == 0)
        {
            IntroStory.PlayNext = true;                  // 실전은 시작 스토리부터(Q9)
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else game.GoToTitle();
    }

    // ───────────── 대화창 ─────────────
    // 여러 줄을 차례로: 줄이 다 나오면 ▼ → 넘기면 다음 줄. 말하는 동안 게임 입력은 막힌다(GameManager.IsGuideTalking).
    IEnumerator Talk(params string[] lines)
    {
        foreach (string line in lines)
        {
            yield return Type(line);
            nextMark.SetActive(true);
            while (!advance) yield return null;
            advance = false;
            audioManager.PlayClick();
        }
    }

    // 한 줄을 한 글자씩(시작 스토리와 같은 속도·문장부호 멈춤·광대 목소리). 나오는 중에 누르면 줄 전체
    IEnumerator Type(string line)
    {
        Hold();
        introRoot.SetActive(true);    // Look 뒤에도 다시 보이게
        dialogBox.SetActive(true);
        nextMark.SetActive(false);
        lineText.text = line;
        lineText.ForceMeshUpdate();
        int count = lineText.textInfo.characterCount;   // 색 태그를 뺀 실제 글자 수
        lineText.maxVisibleCharacters = 0;
        advance = false;
        int sounded = 0;
        for (int shown = 1; shown <= count; shown++)
        {
            if (advance) break;
            lineText.maxVisibleCharacters = shown;
            char c = lineText.textInfo.characterInfo[shown - 1].character;
            if (!char.IsWhiteSpace(c) && ".,?!…".IndexOf(c) < 0 && sounded++ % charsPerSound == 0)
                audioManager.PlayVoice(typeVolume, voicePitch + UnityEngine.Random.Range(-voicePitchJitter, voicePitchJitter));
            float wait = IntroStory.PauseAfter(Plain(count), shown - 1, charSeconds, punctSeconds, ellipsisSeconds, dotSeconds);
            for (float t = 0f; t < wait && !advance; t += Time.deltaTime) yield return null;
        }
        lineText.maxVisibleCharacters = count;
        advance = false;
    }

    // 태그를 뺀 실제 글자들(문장부호 멈춤 계산용)
    string Plain(int count)
    {
        var chars = new char[count];
        for (int i = 0; i < count; i++) chars[i] = lineText.textInfo.characterInfo[i].character;
        return new string(chars);
    }

    // 말하기 시작: 대화창을 켜고 게임 입력을 막음
    void Hold()
    {
        if (talking) return;
        talking = true;
        ShowTask(false);
        introRoot.SetActive(true);
        dialogBox.SetActive(true);
        game.SetGuideTalking(true);
    }

    // 말 끝: 대화창을 끄고 게임을 움직이게
    void Release()
    {
        talking = false;
        holdTime = 0f;
        progress = 0f;
        introRoot.SetActive(false);
        game.SetGuideTalking(false);
    }

    // ───────────── 할 일 ─────────────
    // 그 행동만 열어 주고, 공개가 reveals번 끝날 때까지(폰은 차트 분석을 쓸 때까지) 기다린다.
    // 공개가 끝날 때마다 GameManager가 "말할 차례"로 바꿔 두므로(광대가 먼저 움직이지 않게), 더 해야 하면 다시 풀어 준다.
    IEnumerator Act(Task kind, PlayerAction action, int reveals)
    {
        task = kind;
        taskAction = action;
        game.SetTutorialGate(kind == Task.Trade, kind == Task.Phone, kind == Task.Trade ? action : (PlayerAction?)null);
        Release();
        ShowTask(true);

        if (kind == Task.Phone)
        {
            while (game.PlayerKnownNext == null) yield return null;
        }
        else
        {
            for (int i = 0; i < reveals; i++)
            {
                int before = game.RevealCount;
                while (game.RevealCount == before) yield return null;
                if (i < reveals - 1) game.SetGuideTalking(false);   // 더 해야 함 → 다시 움직이게
            }
        }

        task = Task.None;
        game.SetTutorialGate(false, false, null);
        ShowTask(false);
    }

    // "여기를 봐": 대화창을 잠깐 숨기고 그 자리를 금색 틀로 가리키며 위쪽에 한 줄 요약. 넘기면(Space·Enter·클릭) 대화창으로.
    // 게임은 계속 멈춰 있다(말하는 중 그대로).
    RectTransform lookTarget;
    IEnumerator Look(RectTransform target, string summary)
    {
        Hold();
        introRoot.SetActive(false);
        lookTarget = target;
        taskText.text = summary + "   <color=#A39A8C>Space ▶</color>";
        ShowTask(true);
        advance = false;
        while (!advance) yield return null;
        advance = false;
        audioManager.PlayClick();
        lookTarget = null;
        ShowTask(false);
    }

    // 광대 차례 구경: 풀어 두고 광대의 공개 한 번이 끝날 때까지
    IEnumerator Watch()
    {
        Release();
        int before = game.RevealCount;
        while (game.RevealCount == before) yield return null;
    }

    void ShowTask(bool on)
    {
        if (taskRoot != null) taskRoot.SetActive(on);
        if (highlight != null) highlight.gameObject.SetActive(on);
    }

    // 할 일 글과 강조 틀: 지금 상태(모니터·폰이 열렸나)에 따라 다음 한 걸음을 가리킴
    // 할 일 칸 자리: 평소 위쪽 가운데(Scene 값), 폰이 열리면 폰이 위까지 덮어서 왼쪽 빈자리로(두 줄)
    [SerializeField] Vector2 phoneTaskPos = new Vector2(6f, -40f);
    [SerializeField] Vector2 phoneTaskSize = new Vector2(100f, 28f);
    Vector2 taskPos, taskSize;
    bool taskPlaced;

    void PlaceTask()
    {
        var r = (RectTransform)taskRoot.transform;
        if (!taskPlaced) { taskPos = r.anchoredPosition; taskSize = r.sizeDelta; taskPlaced = true; }
        bool side = game.IsPhoneOpen;
        r.anchoredPosition = side ? phoneTaskPos : taskPos;
        r.sizeDelta = side ? phoneTaskSize : taskSize;
        taskText.textWrappingMode = side ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
    }

    void UpdateTask()
    {
        if (taskRoot != null && taskRoot.activeSelf) PlaceTask();
        if (lookTarget != null) { Point(lookTarget); return; }   // "여기를 봐" 중
        if (task == Task.None || taskText == null) return;
        RectTransform target = null;
        string text = "";
        bool myMove = game.CanPlayerAct || game.IsPhoneOpen;
        if (task == Task.Trade)
        {
            string act = taskAction == PlayerAction.Buy ? "매수" : "매도";
            if (!game.IsMonitorOpen) { target = monitorButton; text = $"모니터를 눌러 열기 (Space) → {act}"; }
            else { target = taskAction == PlayerAction.Buy ? buyButton : sellButton; text = $"{act} 버튼 누르기 (Space)"; }
        }
        else
        {
            if (!game.IsPhoneOpen) { target = phoneButton; text = "내 폰을 눌러 열기"; }
            else if (!game.IsConfirming) { target = firstSlot; text = "차트 분석 칸 고르기 (1)"; }
            else { target = useButton; text = "사용 (Space)"; }
        }
        if (!myMove && !game.IsPhoneOpen) target = null;   // 공개 중·광대 차례엔 가리키지 않음
        taskText.text = text;
        Point(target);
    }

    // 강조 틀을 대상 위에(없거나 멈춤이면 숨김)
    void Point(RectTransform target)
    {
        bool show = target != null && target.gameObject.activeInHierarchy && !game.IsPaused;
        highlight.gameObject.SetActive(show);
        if (!show) return;
        // 대상과 같은 자리·크기(+여백). 두 칸 모두 같은 Canvas라 월드 좌표로 맞춤
        highlight.position = target.TransformPoint(target.rect.center);
        Vector2 size = target.rect.size * (target.lossyScale.x / highlight.lossyScale.x);
        highlight.sizeDelta = size + Vector2.one * highlightPadding * 2f;
        // 0.5초 주기 깜빡임(켜짐 0.35 · 꺼짐 0.15)
        bool on = (Time.unscaledTime % 0.5f) < 0.35f;
        foreach (Image e in edges) e.enabled = on;
    }

    // ───────────── 입력 (대사 중에만) ─────────────
    void Update()
    {
        if (!IsActive) return;
        UpdateTask();
        if (!talking || game.IsPaused) return;

        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (simulateAdvance || autoAdvance) { simulateAdvance = false; advance = true; }
        if ((kb != null && kb.enterKey.wasPressedThisFrame) || (mouse != null && mouse.leftButton.wasPressedThisFrame && !IsChoosing))
            advance = true;

        // Space: 짧게 = 다음 줄, 1초 꾹 = 건너뛰기(→ 타이틀). 시작 스토리와 같은 표시
        float dt = Time.unscaledDeltaTime;
        bool held = kb != null && kb.spaceKey.isPressed;
        if (held)
        {
            holdTime += dt;
            progress = Mathf.Max(progress, holdTime / holdSeconds);
            if (progress >= 1f) skip = true;
        }
        else
        {
            if (holdTime > 0f && holdTime < tapSeconds) { advance = true; spaceTap = true; }
            holdTime = 0f;
            progress = Mathf.Max(0f, progress - dt / 0.3f);
        }
        if (skipHint != null) skipHint.alpha = progress > 0f ? 1f : idleAlpha;
        if (skipFill != null) skipFill.fillAmount = progress;
        if (skip)
        {
            skip = false;
            IsActive = false;
            StopAllCoroutines();
            game.GoToTitle();
        }
    }

    bool spaceTap;   // 이번 프레임 Space 짧게 → 끝 선택 결정에 씀(코루틴이 같은 프레임에 읽음)
    void LateUpdate() { spaceTap = false; }

    // ───────────── 끝 선택 (Q9) ─────────────
    IEnumerator Choose()
    {
        IsChoosing = true;
        Choice = 0;
        nextMark.SetActive(false);
        choiceTexts[0].text = "시작";
        choiceTexts[1].text = "타이틀";
        choicesRoot.SetActive(true);
        RefreshChoices();
        yield return null;
        confirmed = false;
        while (!confirmed)
        {
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                Vector2 pos = mouse.position.ReadValue();
                for (int i = 0; i < choiceTexts.Length; i++)
                {
                    if (!RectTransformUtility.RectangleContainsScreenPoint(choiceTexts[i].rectTransform, pos, uiCamera)) continue;
                    if (i != Choice && mouse.delta.ReadValue().sqrMagnitude > 0f) Pick(i);
                    if (mouse.leftButton.wasPressedThisFrame) { Pick(i); confirmed = true; }
                }
            }
            if (kb != null)
            {
                if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) Pick(0);
                else if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) Pick(1);
                else if (spaceTap || kb.enterKey.wasPressedThisFrame) confirmed = true;
            }
            yield return null;
        }
        IsChoosing = false;
        audioManager.PlayClick();
    }

    bool confirmed;
    // 테스트·다른 코드에서 결정
    public void Confirm(int index)
    {
        if (!IsChoosing) return;
        Pick(index);
        confirmed = true;
    }

    void Pick(int index)
    {
        index = Mathf.Clamp(index, 0, choiceTexts.Length - 1);
        if (index != Choice) audioManager.PlayClick(typeVolume);
        Choice = index;
        RefreshChoices();
    }

    float arrowWidth = -1f;   // "▶ " 폭 — ▶를 글자 왼쪽 바깥에 걸침(시작 스토리 예/아니오와 같은 방법)

    void RefreshChoices()
    {
        string[] labels = { "시작", "타이틀" };
        if (arrowWidth < 0f)
            arrowWidth = choiceTexts[0].GetPreferredValues("▶ 가").x - choiceTexts[0].GetPreferredValues("가").x;
        for (int i = 0; i < choiceTexts.Length && i < labels.Length; i++)
        {
            bool on = i == Choice;
            choiceTexts[i].text = (on ? "▶ " : "") + labels[i];
            choiceTexts[i].margin = new Vector4(on ? -arrowWidth : 0f, 0f, 0f, 0f);
            choiceTexts[i].color = on ? selectedColor : normalColor;
        }
    }
}
