using UnityEditor;
using UnityEditor.SceneManagement;

namespace MaseiKivotos.Editor
{
    /// <summary>
    /// Unity 메뉴에서 기존 리소스를 연결한 마키보 전투 씬을 여는 에디터 도구.
    /// </summary>
    public static class StageBattleScene
    {
        /// <summary>
        /// 마키보 전투 실행 기준인 Stage_Battle 씬의 프로젝트 상대 경로.
        /// </summary>
        public const string ScenePath = "Assets/Scenes/Stage/Battle/BattleType/Stage_Battle.unity";
        /// <summary>
        /// 재생 중에는 동작하지 않는다. 편집 중인 씬의 저장 여부를 확인한 뒤 Stage_Battle을 연다.
        /// </summary>
        [MenuItem("마키보/Stage_Battle 열기")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
