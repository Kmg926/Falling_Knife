using UnityEngine;

// title-menu 54: 플레이어 설정(소리·화면 흔들림·전체화면). 컴퓨터에 저장(PlayerPrefs)해서 게임을 다시 켜도 남는다.
// static이라 씬마다 오브젝트를 둘 필요 없이 타이틀(바꾸기)·GameScene(시작할 때 읽기)이 같은 값을 본다.
// 소리는 0~10 단계(10% 단위). 기본값 = 설정이 생기기 전 값(전체 100%, 음악 60%, 흔들림 켬).
public static class GameSettings
{
    const string MasterKey = "fk.master";
    const string MusicKey = "fk.music";
    const string ShakeKey = "fk.shake";
    const string FullscreenKey = "fk.fullscreen";

    public const int Steps = 10;

    // game-settings 91: 값이 바뀌면 알림 — 음악(MusicPlayer)·효과음(AudioManager)·흔들림(UIManager)이 듣고 바로 반영.
    // 타이틀에서 바꾸든 게임 중에 바꾸든 같은 길(바꾸는 쪽은 듣는 쪽을 몰라도 됨)
    public static event System.Action Changed;
    const int DefaultMaster = 10;
    const int DefaultMusic = 6;

    public static int Master
    {
        get => PlayerPrefs.GetInt(MasterKey, DefaultMaster);
        set { PlayerPrefs.SetInt(MasterKey, Mathf.Clamp(value, 0, Steps)); PlayerPrefs.Save(); Changed?.Invoke(); }
    }

    public static int Music
    {
        get => PlayerPrefs.GetInt(MusicKey, DefaultMusic);
        set { PlayerPrefs.SetInt(MusicKey, Mathf.Clamp(value, 0, Steps)); PlayerPrefs.Save(); Changed?.Invoke(); }
    }

    public static bool Shake
    {
        get => PlayerPrefs.GetInt(ShakeKey, 1) == 1;
        set { PlayerPrefs.SetInt(ShakeKey, value ? 1 : 0); PlayerPrefs.Save(); Changed?.Invoke(); }
    }

    // 저장값이 없으면 지금 창 상태. 바꾸면 바로 창에도 적용(에디터에서는 효과 없음 — 빌드에서 확인)
    public static bool Fullscreen
    {
        get => PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        set { PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0); PlayerPrefs.Save(); Screen.fullScreen = value; Changed?.Invoke(); }
    }

    // 0~1 크기(AudioManager가 씀)
    public static float MasterVolume => Master / (float)Steps;
    public static float MusicVolume => Music / (float)Steps;
}
