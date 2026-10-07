using MaseiKivotos.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace MaseiKivotos.Editor
{
    /// <summary>프리팹 편집 중 참조와 색상을 편집하고 설명용 도식을 갱신한다.</summary>
    [CustomEditor(typeof(SkillSlotView))]
    public sealed class SkillSlotViewEditor : UnityEditor.Editor
    {
        /// <summary>기본 필드 편집 후 에디터 전용 예제를 표시한다.</summary>
        public override void OnInspectorGUI()
        {
            bool changed = DrawDefaultInspector();
            EditorGUILayout.HelpBox("목표 5번(빨강), 사용자 1번(초록). Area -4~4 / Range 0~8을 넘으면 Over 안내를 표시합니다. Preview는 전투 데이터가 아닙니다.", MessageType.Info);
            if (!Application.isPlaying && (GUILayout.Button("슬롯 색상 미리보기") || changed))
            {
                var view = (SkillSlotView)target; // view: 현재 편집 중인 프리팹 또는 씬 인스턴스.
                try
                {
                    view.ValidateReferences();
                    var graphics = view.GetComponentsInChildren<Graphic>(true);
                    // objects: 색/문구와 Over 오브젝트의 활성 상태를 함께 Undo로 복구한다.
                    var objects = graphics.SelectMany(g => new Object[] { g, g.gameObject }).Distinct().ToArray();
                    Undo.RecordObjects(objects, "Preview skill diagram"); view.PreviewDiagram();
                    foreach (var graphic in graphics)
                    {
                        EditorUtility.SetDirty(graphic);
                        EditorUtility.SetDirty(graphic.gameObject);
                        if (PrefabUtility.IsPartOfPrefabInstance(graphic)) PrefabUtility.RecordPrefabInstancePropertyModifications(graphic);
                        if (PrefabUtility.IsPartOfPrefabInstance(graphic.gameObject)) PrefabUtility.RecordPrefabInstancePropertyModifications(graphic.gameObject);
                    }
                }
                catch (System.InvalidOperationException exception) { Debug.LogWarning(exception.Message, view); }
            }
        }
    }
}
