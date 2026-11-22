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
    // 79: 곡을 "처음 한 번(인트로) + 끝없이 반복(루프)" 두 조각으로 잘라 이어 튼다.
    // 두 칸이 비어 있으면 예전처럼 Music Source에 넣은 곡을 그대로 튼다.
    [SerializeField] AudioClip introClip;        // 0 ~ 62.93초, 한 번
    [SerializeField] AudioClip loopClip;         // 62.93 ~ 151.54초, 반복
    [SerializeField] AudioSource loopSource;     // 루프 전용 재생기(인트로가 끝나는 샘플에 맞춰 시작)

    [Header("크기 (0~1)")]
    [SerializeField, Range(0f, 1f)] float masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] float musicVolume = 0.6f;
    // 79: 곡 자체가 커서(사용자 "기본 음량 너무 커") 설정 100%일 때 이 비율로 튼다 → 설정 100% = 전의 50%
    // 82: 사용자 "음악 사운드 70%로" → 0.5 × 0.7 = 0.35
    [SerializeField, Range(0f, 1f)] float musicBase = 0.35f;
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
        PlayMusic();
    }

    // 79: 인트로를 조금 뒤(0.1초)에 시작하도록 예약하고, 루프는 "인트로 시작 + 인트로 길이"에 예약한다.
    // PlayScheduled = 오디오 시계(dspTime) 기준으로 샘플 단위 정확히 시작 → 두 조각 사이에 빈틈·겹침이 없다.
    // (Update에서 "끝났으면 다음 곡"으로 바꾸면 프레임 간격만큼 틈이 생겨 '툭' 끊긴다)
    void PlayMusic()
    {
        if (introClip == null || loopClip == null || musicSource == null || loopSource == null) return;
        musicSource.Stop();
        musicSource.clip = introClip;
        musicSource.loop = false;
        loopSource.clip = loopClip;
        loopSource.loop = true;                  // 루프 조각은 끝에서 처음으로 바로 돌아감(자른 자리가 서로 이어지도록 찾아 둠)

        double start = AudioSettings.dspTime + 0.1;
        double introLength = (double)introClip.samples / introClip.frequency;   // 초 = 샘플 수 ÷ 1초 샘플 수
        musicSource.PlayScheduled(start);
        loopSource.PlayScheduled(start + introLength);
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
        float volume = masterVolume * musicVolume * musicBase;   // 79: × 기준 비율
        if (ducking) volume *= revealDuck;
        musicSource.volume = volume;
        if (loopSource != null) loopSource.volume = volume;   // 79: 두 조각 같은 크기
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
