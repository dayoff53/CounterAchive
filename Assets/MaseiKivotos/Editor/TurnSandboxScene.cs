using System.IO;
using MaseiKivotos.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MaseiKivotos.Editor
{
    /// <summary>
    /// 독립 턴 검증 씬을 생성하거나 여는 에디터 도구. 기존 전투 씬과 빌드 씬 목록은 수정하지 않는다.
    /// </summary>
    public static class TurnSandboxScene
    {
        /// <summary>
        /// 독립 턴 검증 씬을 생성하고 여는 프로젝트 상대 경로.
        /// </summary>
        public const string ScenePath = "Assets/MaseiKivotos/Scenes/TurnSandbox.unity";

        /// <summary>
        /// 검증 씬이 없을 때 빈 씬에 카메라와 턴 컨트롤러를 만들고 저장한다. 이미 존재하는 파일은 덮어쓰지 않는다.
        /// </summary>
        public static void Create()
        {
            if (File.Exists(ScenePath)) { Debug.Log("Turn sandbox already exists: " + ScenePath); return; }
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            // scene: 검증용 카메라와 컨트롤러를 저장할 새 빈 씬.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // camera: 씬을 렌더링할 카메라.
            var camera = new GameObject("Main Camera", typeof(Camera));
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 0, -10);
            camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            camera.GetComponent<Camera>().backgroundColor = new Color32(14, 22, 36, 255);
            new GameObject("Masei Turn Sandbox", typeof(TurnSandboxController));
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Could not save turn sandbox.");
            AssetDatabase.Refresh();
            Debug.Log("Created independent turn sandbox: " + ScenePath);
        }

        /// <summary>
        /// 현재 씬의 저장 여부를 확인하고, 필요하면 검증 씬을 생성한 뒤 연다. 재생 중에는 동작하지 않는다.
        /// </summary>
        [MenuItem("마키보/턴 검증 씬 열기")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Create();
            EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
