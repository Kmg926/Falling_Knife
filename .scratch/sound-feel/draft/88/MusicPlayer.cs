using UnityEngine;

// 배경음악 재생기 (sound-feel 88, 사용자 "타이틀에서 노래가 흘러나오게, 시작을 해도 노래가 안 끊기게").
// 씬이 바뀌어도 사라지지 않는 오브젝트(DontDestroyOnLoad) 하나가 타이틀 → 게임 → 타이틀 내내 같은 곡을 이어 튼다.
// 처음 켜진 하나만 남고, 나중에 열린 씬에 놓인 MusicPlayer는 스스로 없어진다(곡이 두 번 겹치거나 처음부터 다시 나오지 않게).
// 곡은 "인트로 한 번 + 끝없는 루프" 두 조각(79): 인트로가 끝나는 샘플에 맞춰 루프를 예약한다.
public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }   // 지금 살아 있는 하나

    [SerializeField] AudioClip introClip;        // 0 ~ 62.93초, 한 번
    [SerializeField] AudioClip loopClip;         // 62.93 ~ 151.54초, 반복
    // 79·82: 곡 자체가 커서 설정 100%일 때 이 비율로(0.5 × 0.7 = 0.35)
    [SerializeField, Range(0f, 1f)] float musicBase = 0.35f;

    AudioSource introSource;
    AudioSource loopSource;
    float duck = 1f;                             // AudioManager가 공개 중 줄일 때 쓰는 비율(지금은 1 = 안 줄임, #75)

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);                 // 이미 타이틀에서 켜진 것이 이어 트는 중 → 이 씬의 것은 필요 없음
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);           // 씬이 바뀌어도 남음(맨 위 오브젝트여야 함)

        introSource = gameObject.AddComponent<AudioSource>();
        loopSource = gameObject.AddComponent<AudioSource>();
        foreach (AudioSource s in new[] { introSource, loopSource })
        {
            s.playOnAwake = false;
            s.spatialBlend = 0f;                 // 2D 소리(카메라 위치와 상관없이 같은 크기)
        }
        ApplyVolume();
        Play();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // 79: 인트로를 0.1초 뒤에 예약하고, 루프는 "인트로 시작 + 인트로 길이"에 예약 → 두 조각 사이 빈틈·겹침 없음
    void Play()
    {
        if (introClip == null || loopClip == null) return;
        introSource.clip = introClip;
        introSource.loop = false;
        loopSource.clip = loopClip;
        loopSource.loop = true;

        double start = AudioSettings.dspTime + 0.1;
        double introLength = (double)introClip.samples / introClip.frequency;   // 초 = 샘플 수 ÷ 1초 샘플 수
        introSource.PlayScheduled(start);
        loopSource.PlayScheduled(start + introLength);
    }

    // 음악 크기 = 전체 × 음악(타이틀 설정, 저장값) × 기준 비율 × 줄임 비율. 설정을 바꾸면 타이틀이 불러서 바로 반영
    public void ApplyVolume()
    {
        float volume = GameSettings.MasterVolume * GameSettings.MusicVolume * musicBase * duck;
        if (introSource != null) introSource.volume = volume;
        if (loopSource != null) loopSource.volume = volume;
    }

    public void SetDuck(float ratio)
    {
        duck = ratio;
        ApplyVolume();
    }

    // 테스트·확인용
    public float Volume => introSource != null ? introSource.volume : 0f;
    public bool IsPlaying => introSource != null && (introSource.isPlaying || loopSource.isPlaying);
}
