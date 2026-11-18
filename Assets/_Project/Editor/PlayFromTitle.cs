#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

// 에디터 전용(Editor 폴더 → 빌드에 안 들어감). Play 버튼을 누르면 지금 열린 Scene이 아니라 항상 TitleScene부터 시작.
// Unity는 기본적으로 "에디터에 열려 있는 Scene"에서 Play를 시작한다 → GameScene을 열어 두고 작업하면 타이틀을 건너뜀.
// [InitializeOnLoad]: 에디터가 켜지거나 스크립트가 다시 컴파일될 때마다 이 설정을 다시 넣음(설정이 프로젝트에 저장되지 않아서).
[InitializeOnLoad]
static class PlayFromTitle
{
    const string TitleScenePath = "Assets/_Project/Scenes/TitleScene.unity";

    static PlayFromTitle()
    {
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(TitleScenePath);
    }
}
#endif
