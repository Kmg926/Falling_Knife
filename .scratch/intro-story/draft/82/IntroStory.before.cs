using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// 시작 스토리 시네마틱 (intro-story 77). 타이틀 `시작`으로 왔을 때만 GameScene 처음에 한 번 보인다(다시하기 땐 없음).
// 검은 화면 → 1초 밝아짐 → 아래 대사 상자에 광대 대사가 한 글자씩 → 끝나면 GameManager에 알려 1라운드(덱 공개 연출)부터.
// 판정은 하지 않는다: 시네마틱 동안 GameManager가 게임 입력을 무시하게 하고(IsIntro), 끝나면 풀어 줄 뿐.
// 게임 판정과 상관없는 연출이라 Input Actions를 거치지 않고 키보드·마우스를 직접 읽는다(TitleMenu와 같은 방식).
public class IntroStory : MonoBehaviour
{
    // 타이틀의 `시작`이 켠다 → 다음에 열리는 GameScene이 시네마틱을 보이고 끈다.
    // static = Scene이 바뀌어도 남는 값(오브젝트에 붙은 값은 Scene이 바뀌면 사라짐)
    public static bool PlayNext;

    [SerializeField] GameManager game;
    [SerializeField] AudioManager audioManager;
    [SerializeField] GameObject root;            // 시네마틱 전체(검정·대사 상자). 끝나면 끔
    [SerializeField] Image black;                // 화면 전체 검정 → 밝아짐
    [SerializeField] GameObject dialogBox;       // 대사 상자(창 그림 + 이름 + 대사 + ▼)
    [SerializeField] TMP_Text lineText;          // 대사 한 줄
    [SerializeField] GameObject nextMark;        // 줄이 다 나오면 오른쪽 아래 ▼
    // 시네마틱 동안 숨길 게임 글자 UI(잔고·라운드·레버리지 등, Spec 제안 1). 탁자 위 폰·모니터는 그대로
    [SerializeField] RectTransform[] hideDuringIntro;

    [Header("에디터에서 GameScene을 바로 Play할 때도 보기 (Spec 제안 3, 평소 끔)")]
    [SerializeField] bool playInEditorWithoutTitle = false;

    [Header("대사 (Grilling Q3)")]
    [SerializeField, TextArea(1, 3)] string[] lines =
    {
        "……또 한 명 왔네.",
        "너도 나도 더 잃을 게 없으니까 여기 앉아 있는 거겠지.",
        "규칙은 간단해. 다음 캔들을 들고 있을지, 나한테 넘길지.",
        "떨어지는 칼은 잡는 게 아니야. 남한테 넘기는 거지.",
    };

    [Header("준비됐나? 예/아니오 (intro-story 78, Grilling Q3·Q8·Q9)")]
    [SerializeField] string question = "준비됐나?";
    [SerializeField] GameObject choicesRoot;     // 대사 상자 오른쪽 아래 `예` `아니오`
    [SerializeField] TMP_Text[] choiceTexts;     // 0 = 예, 1 = 아니오
    [SerializeField] string[] choiceLabels = { "예", "아니오" };
    // 무엇을 골라도 시작한다. 고른 뒤 광대의 대답만 다르다
    [SerializeField] string[] replies = { "좋아. 시작하자.", "상관없어. 칼은 이미 떨어지고 있거든." };
    [SerializeField] Color selectedColor = new Color32(0xD9, 0xB4, 0x5A, 255);   // gold2 (타이틀 메뉴와 같음)
    [SerializeField] Color normalColor = new Color32(0xA3, 0x9A, 0x8C, 255);     // cream0

    [Header("시간 (Grilling Q4)")]
    [SerializeField] float fadeInSeconds = 1f;
    [SerializeField] float charSeconds = 0.03f;          // 한 글자 나오는 간격
    [SerializeField] int charsPerSound = 2;              // 글자 소리: 2글자마다 한 번(Spec 제안 4)
    [SerializeField, Range(0f, 1f)] float typeVolume = 0.3f;   // 클릭음의 30%

    CanvasGroup[] hidden;          // hideDuringIntro를 투명하게 만드는 칸(실행 중에만 붙임 — Scene은 안 바뀜)
    bool advance;                  // 이번 프레임에 Space·Enter·클릭을 눌렀나
    bool skip;                     // Esc를 눌렀나(어디서든 바로 시작)

    public bool IsPlaying { get; private set; }
    public bool IsChoosing { get; private set; }   // 예/아니오를 고르는 중
    public int Choice { get; private set; }        // 지금 고른 칸(0 = 예)

    Camera uiCamera;          // 마우스가 글자 위에 있는지 잴 때 쓰는 카메라(HUD는 Screen Space - Camera)
    float arrowWidth = -1f;   // "▶ " 폭(타이틀 메뉴와 같은 방법으로 처음 한 번 잼)

    void Awake()
    {
        bool play = PlayNext;
#if UNITY_EDITOR
        play |= playInEditorWithoutTitle;
#endif
        PlayNext = false;                       // 한 번 보였으면 끔(다시하기·같은 Scene에서 다시 안 나오게)
        if (!play)
        {
            root.SetActive(false);
            return;
        }
        // Awake는 모든 Start보다 먼저 → GameManager.Start(1라운드 준비)가 시네마틱 중이라는 걸 알고 덱 공개 연출을 미룬다
        IsPlaying = true;
        game.BeginIntro();
    }

    void Start()
    {
        if (!IsPlaying) return;
        // 숨길 UI에 투명도 칸을 붙여 0으로(SetActive를 쓰면 각 View가 Refresh 때 다시 켤 수 있어서 투명도로 숨김)
        hidden = new CanvasGroup[hideDuringIntro.Length];
        for (int i = 0; i < hideDuringIntro.Length; i++)
        {
            CanvasGroup g = hideDuringIntro[i].GetComponent<CanvasGroup>();
            if (g == null) g = hideDuringIntro[i].gameObject.AddComponent<CanvasGroup>();
            g.alpha = 0f;
            hidden[i] = g;
        }
        Canvas canvas = GetComponentInParent<Canvas>();
        uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        if (choicesRoot != null) choicesRoot.SetActive(false);
        root.SetActive(true);
        StartCoroutine(Play());
    }

    // 입력은 Update에서 받아 두고, 연출(코루틴)이 그 값을 읽는다
    void Update()
    {
        if (!IsPlaying) return;
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame) skip = true;
        if ((kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)) ||
            (mouse != null && mouse.leftButton.wasPressedThisFrame))
            advance = true;
        if (skip) Finish();
    }

    IEnumerator Play()
    {
        // ① 검정 → 밝아짐. 대사 상자는 밝아진 뒤에 나온다
        dialogBox.SetActive(false);
        black.gameObject.SetActive(true);
        for (float t = 0f; t < fadeInSeconds; t += Time.deltaTime)
        {
            black.color = new Color(0f, 0f, 0f, 1f - t / fadeInSeconds);
            yield return null;
        }
        black.gameObject.SetActive(false);

        // ② 대사를 한 줄씩
        dialogBox.SetActive(true);
        foreach (string line in lines)
            yield return Say(line);

        // ③ 78: "준비됐나?" → 예/아니오 → 광대 대답 한 줄 → 시작
        if (choicesRoot != null && choiceTexts != null && choiceTexts.Length >= 2)
        {
            yield return Type(question);
            yield return Choose();
            yield return Say(replies[Choice]);
        }

        Finish();
    }

    // 예/아니오 고르기. ←/→·A/D·↑/↓·W/S 이동, Space·Enter 결정, 마우스를 올리면 그 칸, 클릭하면 그 칸으로 결정.
    // (Esc는 Update에서 언제든 Finish = "예"와 같이 바로 시작)
    IEnumerator Choose()
    {
        IsChoosing = true;
        Choice = 0;                    // 처음은 `예`(Q9)
        nextMark.SetActive(false);     // 고르는 동안 ▼ 대신 선택지
        choicesRoot.SetActive(true);
        RefreshChoices();
        yield return null;             // 질문 줄을 넘긴 누름이 바로 결정되지 않게 한 프레임 쉼

        confirmed = false;
        while (!confirmed)
        {
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;

            int hovered = -1;
            if (mouse != null)
            {
                Vector2 pos = mouse.position.ReadValue();
                for (int i = 0; i < choiceTexts.Length; i++)
                    if (RectTransformUtility.RectangleContainsScreenPoint(choiceTexts[i].rectTransform, pos, uiCamera)) hovered = i;
                // 마우스를 움직였을 때만 올린 칸을 고름(키보드로 고른 칸을 가만히 있는 마우스가 덮어쓰지 않게 — 타이틀 메뉴와 같음)
                if (hovered >= 0 && hovered != Choice && mouse.delta.ReadValue().sqrMagnitude > 0f) Pick(hovered);
                if (hovered >= 0 && mouse.leftButton.wasPressedThisFrame) { Pick(hovered); break; }
            }
            if (kb != null)
            {
                if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) Pick(0);
                else if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) Pick(1);
                else if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame) break;
            }
            yield return null;
        }

        IsChoosing = false;
        choicesRoot.SetActive(false);
        audioManager.PlayClick();
    }

    // 그 칸으로 결정(테스트·다른 코드에서 부를 수 있게 — 타이틀 메뉴의 Choose와 같은 역할)
    bool confirmed;
    public void Confirm(int index)
    {
        if (!IsChoosing) return;
        Pick(index);
        confirmed = true;
    }

    // 칸 고르기(테스트에서도 부름). 바뀔 때만 클릭음
    public void Pick(int index)
    {
        index = Mathf.Clamp(index, 0, choiceTexts.Length - 1);
        if (index != Choice) audioManager.PlayClick(typeVolume);
        Choice = index;
        RefreshChoices();
    }

    // 고른 칸 = 금색 + ▶(▶는 글자 왼쪽 바깥에 걸침 — 타이틀 메뉴 76과 같은 방법), 나머지 = 흐린 크림
    void RefreshChoices()
    {
        if (arrowWidth < 0f)
            arrowWidth = choiceTexts[0].GetPreferredValues("▶ 가").x - choiceTexts[0].GetPreferredValues("가").x;
        for (int i = 0; i < choiceTexts.Length; i++)
        {
            bool on = i == Choice;
            choiceTexts[i].text = (on ? "▶ " : "") + choiceLabels[i];
            choiceTexts[i].margin = new Vector4(on ? -arrowWidth : 0f, 0f, 0f, 0f);
            choiceTexts[i].color = on ? selectedColor : normalColor;
        }
    }

    // 한 줄을 보이고 ▼ + 넘김을 기다린다
    IEnumerator Say(string line)
    {
        yield return Type(line);
        nextMark.SetActive(true);
        while (!advance) yield return null;
        advance = false;
        audioManager.PlayClick();
    }

    // 한 줄을 한 글자씩 보인다. 나오는 중에 누르면 그 줄 전체를 바로 보인다(Grilling Q4).
    IEnumerator Type(string line)
    {
        nextMark.SetActive(false);
        lineText.text = line;
        lineText.maxVisibleCharacters = 0;   // 글은 다 넣어 두고 보이는 글자 수만 늘림(줄바꿈 자리가 처음부터 정해져 흔들리지 않음)
        advance = false;

        int sounded = 0;
        for (int shown = 1; shown <= line.Length; shown++)
        {
            if (advance) break;
            lineText.maxVisibleCharacters = shown;
            if (!char.IsWhiteSpace(line[shown - 1]) && sounded++ % charsPerSound == 0)
                audioManager.PlayClick(typeVolume);
            yield return new WaitForSeconds(charSeconds);
        }
        lineText.maxVisibleCharacters = line.Length;
        advance = false;                     // 줄 전체를 보이게 한 누름은 다음 줄로 넘기지 않음
    }

    // 끝(마지막 줄 다음이나 Esc): 숨긴 UI를 되돌리고 게임을 시작하게 한다
    void Finish()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        StopAllCoroutines();
        root.SetActive(false);
        foreach (CanvasGroup g in hidden) g.alpha = 1f;
        game.EndIntro();
    }
}
