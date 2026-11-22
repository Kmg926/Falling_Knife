using UnityEngine;

// 소리를 트는 곳(방송실). 판정은 하지 않는다. GameManager가 사건이 일어난 자리에서 부른다.
public class AudioManager : MonoBehaviour
{
    [Header("효과음")]
    [SerializeField] AudioClip impactClip;
    [SerializeField] AudioClip tickClip;
    [SerializeField] AudioClip clickClip;
    [SerializeField] AudioClip shuffleClip;
    [SerializeField] AudioClip buzzClip;          // item-phone 65: 아이템을 받아 폰이 울릴 때(진동음 B, 볼륨 절반)

    [Header("재생기")]
    [SerializeField] AudioSource sfxSource;

    // 88: 배경음악은 MusicPlayer(씬이 바뀌어도 남는 재생기)가 맡는다 — 타이틀에서 시작해 게임까지 끊기지 않게.
    //     여기(GameScene)는 효과음만 튼다. 음악 크기 기준(0.35)·인트로/루프 예약도 MusicPlayer로 옮김.

    [Header("크기 (0~1)")]
    [SerializeField, Range(0f, 1f)] float masterVolume = 1f;
    // 캔들 공개 중 음악 크기 비율 (GDD "공개 직전 잠깐 무음"을 음악이 있는 상태로)
    // 2026-11-22 사용자 "모든 상황에서 음악 사운드 일정하게, 특정 행동 때 줄이니 정신없어" → 0.2에서 1(줄이지 않음). 기능은 남겨 둠(값만 1)
    [SerializeField, Range(0f, 1f)] float revealDuck = 1f;

    // title-menu 54: 타이틀 설정 창에서 저장한 소리 크기를 시작할 때 읽음(Inspector 값 대신)
    void Awake()
    {
        masterVolume = GameSettings.MasterVolume;
    }

    // 공개가 시작되면 true, 끝나면 false. 음악만 줄인다(효과음은 그대로). 88: 실제로는 MusicPlayer에 비율을 넘김
    public void SetRevealDuck(bool on)
    {
        if (MusicPlayer.Instance != null) MusicPlayer.Instance.SetDuck(on ? revealDuck : 1f);
    }

    public void PlayImpact() { Play(impactClip); }
    public void PlayTick() { Play(tickClip); }
    public void PlayClick() { Play(clickClip); }
    // intro-story 77: 대사 글자 소리 = 같은 클릭음을 작게(scale 0~1, 전체 소리에 곱함)
    public void PlayClick(float scale) { Play(clickClip, scale); }
    public void PlayShuffle() { Play(shuffleClip); }

    // item-phone 65: 두 폰이 같은 순간에 울려도 진동음은 한 번만(0.1초 안의 두 번째 요청은 무시)
    float lastBuzzTime = -1f;
    public void PlayBuzz()
    {
        if (Time.unscaledTime - lastBuzzTime < 0.1f) return;
        lastBuzzTime = Time.unscaledTime;
        Play(buzzClip);
    }

    // 클립이나 재생기가 비어 있거나 크기가 0이면 아무것도 하지 않는다.
    void Play(AudioClip clip, float scale = 1f)   // intro-story 77: scale = 이번 소리만 작게(기본 1 = 전과 같음)
    {
        if (clip == null || sfxSource == null) return;
        if (masterVolume <= 0f) return;
        sfxSource.PlayOneShot(clip, masterVolume * scale);
    }
}
