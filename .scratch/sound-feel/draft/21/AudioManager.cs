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

    [Header("크기 (0~1)")]
    [SerializeField, Range(0f, 1f)] float masterVolume = 1f;

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
