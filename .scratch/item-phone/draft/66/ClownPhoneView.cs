using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// item-phone 66: 테이블 위 접힌 광대 폰(내 폰과 같은 모양). 읽기만 한다(판정 없음).
// - 바깥 작은 화면에 광대 아이템 개수, 열 수 있을 때(내 차례·공개 중 아님) 금색 테두리, 클릭 = 같은 확대 화면에 광대 아이템(보기 전용)
// - 광대가 아이템을 쓰면(GameManager.ClownItemUseCount가 바뀌면) 금색 테두리를 3번 반짝이고 개수가 줄어든다
// (64에서는 펼쳐 세운 폰에 아이콘 2×2 + 쓴 칸 반짝임이었다 → Grilling Q13·Q16으로 바꿈)
public class ClownPhoneView : MonoBehaviour
{
    [SerializeField] GameManager game;
    [SerializeField] Button closedButton;      // 접힌 광대 폰 그림(클릭 = 열기)
    [SerializeField] Image outline;            // 금색 테두리(열 수 있을 때 + 광대 사용 반짝임)
    [SerializeField] TMP_Text countText;       // 바깥 작은 화면의 개수(아래 그림 5장이 있으면 끄고 그림으로)
    // item-phone 66: 광대 폰은 비스듬히 놓여 있어 글자를 따로 쓰면 각도가 안 맞고 칸 밖으로 넘친다 →
    // 개수(0~4)를 화면 칸에 그려 넣은 폰 그림 5장 중 하나로 바꿔 끼운다(숫자도 폰과 같은 원근·각도)
    [SerializeField] Image phoneImage;
    [SerializeField] Sprite[] countSprites;    // [0] = 0개 … [4] = 4개

    [Header("사용 반짝임 (Grilling Q16)")]
    [SerializeField] int blinks = 3;
    [SerializeField] float blinkSeconds = 0.1f;   // 켜짐 0.05 + 꺼짐 0.05 → 3번 = 0.3초

    int lastUseCount = -1;      // 처음 Refresh 때 지금 값으로 맞춤(시작부터 반짝이지 않게)
    bool flashing;              // 반짝이는 동안은 테두리를 코루틴이 맡는다

    void Awake()
    {
        closedButton.onClick.AddListener(() => game.OpenClownPhone());
    }

    public void Refresh(GameManager game)
    {
        int count = game.Clown.Items.Count;
        bool useSprites = phoneImage != null && countSprites != null && count < countSprites.Length;
        if (useSprites) phoneImage.sprite = countSprites[count];
        countText.enabled = !useSprites;
        countText.text = count.ToString();
        bool can = game.CanOpenPhone && !game.IsPhoneOpen;
        closedButton.interactable = can;

        if (lastUseCount < 0) lastUseCount = game.ClownItemUseCount;
        if (game.ClownItemUseCount != lastUseCount)
        {
            lastUseCount = game.ClownItemUseCount;
            if (isActiveAndEnabled)
            {
                StopAllCoroutines();
                StartCoroutine(Flash());
            }
        }
        if (!flashing) outline.enabled = can;
    }

    IEnumerator Flash()
    {
        flashing = true;
        for (int i = 0; i < blinks; i++)
        {
            outline.enabled = true;
            yield return new WaitForSeconds(blinkSeconds / 2f);
            outline.enabled = false;
            yield return new WaitForSeconds(blinkSeconds / 2f);
        }
        flashing = false;
        outline.enabled = game.CanOpenPhone && !game.IsPhoneOpen;
    }

    void OnDisable()
    {
        flashing = false;
    }
}
