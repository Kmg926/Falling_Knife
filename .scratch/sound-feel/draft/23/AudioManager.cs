using UnityEngine;

// 소리를 트는 곳(방송실). 판정은 하지 않는다. GameManager가 사건이 일어난 자리에서 부른다.
public class AudioManager : MonoBehaviour
{
    [Header("효과음")]
    [SerializeField] AudioClip impactClip;
    [SerializeField] AudioClip tickClip;
    [SerializeField] AudioClip clickClip;
    [SerializeField] AudioClip shuffleClip;

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

    // 클립이나 재생기가 비어 있거나 크기가 0이면 아무것도 하지 않는다.
    void Play(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        if (masterVolume <= 0f) return;
        sfxSource.PlayOneShot(clip, masterVolume);
    }
}
