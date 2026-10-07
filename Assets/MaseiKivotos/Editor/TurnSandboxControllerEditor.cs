using MaseiKivotos.Unity;
using UnityEditor;

namespace MaseiKivotos.Editor
{
    /// <summary>
    /// 턴 컨트롤러를 Unity 기본 Inspector로 그린다.
    /// 현재 Odin의 UI Toolkit/IMGUI 연결 오류로 필드 높이가 무너지는 경로를 이 컴포넌트에서 우회한다.
    /// </summary>
    [CustomEditor(typeof(TurnSandboxController)), CanEditMultipleObjects]
    public sealed class TurnSandboxControllerEditor : UnityEditor.Editor
    {
        /// <summary>
        /// 직렬화된 필드를 기본 그리기로 표시한다. Min 제한, Undo, 다중 선택과 프리팹 오버라이드는 Unity가 처리한다.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}
