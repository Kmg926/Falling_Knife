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

    [Header("배경음악 (곡은 내가 구해서 Music Source의 AudioClip 칸에 넣는다)")]
    [SerializeField] AudioSource musicSource;

    [Header("크기 (0~1)")]
    [SerializeField, Range(0f, 1f)] float masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] float musicVolume = 0.6f;
    // 캔들 공개 중 음악 크기 비율 (GDD "공개 직전 잠깐 무음"을 음악이 있는 상태로)
    [SerializeField, Range(0f, 1f)] float revealDuck = 0.2f;

    bool ducking;

    // title-menu 54: 타이틀 설정 창에서 저장한 소리 크기를 시작할 때 읽음(Inspector 값 대신)
    void Awake()
    {
        masterVolume = GameSettings.MasterVolume;
        musicVolume = GameSettings.MusicVolume;
    }

    void Start()
    {
        ApplyMusicVolume();
    }

    // Inspector에서 슬라이더를 움직이면(Play 중에도) 바로 반영
    void OnValidate()
    {
        ApplyMusicVolume();
    }

    // 공개가 시작되면 true, 끝나면 false. 음악만 줄인다(효과음은 그대로).
    public void SetRevealDuck(bool on)
    {
        ducking = on;
        ApplyMusicVolume();
    }

    // 음악 크기 = 전체 × 음악 × (공개 중이면 비율). 재생기가 비어 있으면 아무것도 안 함.
    void ApplyMusicVolume()
    {
        if (musicSource == null) return;
        float volume = masterVolume * musicVolume;
        if (ducking) volume *= revealDuck;
        musicSource.volume = volume;
    }

    public void PlayImpact() { Play(impactClip); }
    public void PlayTick() { Play(tickClip); }
    public void PlayClick() { Play(clickClip); }
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
    void Play(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        if (masterVolume <= 0f) return;
        sfxSource.PlayOneShot(clip, masterVolume);
    }
}
