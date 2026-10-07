#if UNITY_EDITOR
using System;
using System.IO;
using MaseiKivotos.Unity;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>검증 사본에서 Over 안내를 Canvas용 텍스트로 이행하는 명시적 도구.</summary>
public static class ConfigureSkillSlotOverflow
{
    /// <summary>새 사용자 계층을 보존하며 Over 참조와 그리기 순서를 저장한다.</summary>
    public static void Run()
    {
        const string path = "Assets/Resources/prefab/UI/SkillSlot.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var diagram = root.transform.Find("Area&RangeSlots");
            var area = ConvertLabel(diagram.Find("AreaOverText"));
            var range = ConvertLabel(diagram.Find("RangeOverText"));
            // 같은 Canvas에서 두 텍스트가 슬롯 그룹보다 나중에 그려지도록 에셋에 순서를 저장한다.
            area.transform.SetAsLastSibling(); range.transform.SetAsLastSibling();
            var view = root.GetComponent<SkillSlotView>();
            var so = new SerializedObject(view);
            so.FindProperty("areaOverText").objectReferenceValue = area;
            so.FindProperty("rangeOverText").objectReferenceValue = range;
            so.ApplyModifiedPropertiesWithoutUndo();
            view.ValidateReferences(); view.PreviewDiagram();
            // 기존 예시의 숫자는 유지하고 표기만 통일한다.
            view.CostText.text = "Cost 0"; view.BpText.text = "BP 5/5";
            // 0×0 영역의 무제한 넘침 때문에 접두어가 옆 열을 침범한다. 위치는 유지하고 편집 가능한 영역만 지정한다.
            FitValue(view.CostText, view.BpText.rectTransform.anchoredPosition.x - view.CostText.rectTransform.anchoredPosition.x - 12);
            FitValue(view.AreaText, view.RangeText.rectTransform.anchoredPosition.x - view.AreaText.rectTransform.anchoredPosition.x - 12);
            FitValue(view.BpText, ((RectTransform)root.transform).rect.width - view.BpText.rectTransform.anchoredPosition.x - 24);
            FitValue(view.RangeText, ((RectTransform)root.transform).rect.width - view.RangeText.rectTransform.anchoredPosition.x - 24);
            PrefabUtility.SaveAsPrefabAsset(root, path, out bool saved);
            if (!saved) throw new InvalidOperationException("SkillSlot save failed.");
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/skill-slot-over-migration.txt", "TextMeshPro + MeshRenderer converted to TextMeshProUGUI + CanvasRenderer.\nAreaSlots, RangeSlots, AreaOverText, RangeOverText sibling order.\nExisting text, font, color, transform position and scale retained; zero-sized over label rectangles assigned UI bounds.\nOver references assigned; normal preview hides both.\n");
            Debug.Log("SKILL_SLOT_OVER_MIGRATION_OK");
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    /// <summary>위치·색·폰트는 유지하고 두 열 안에 값이 들어가도록 최초 프리팹 설정만 보완한다.</summary>
    /// <param name="text">값을 표시하는 텍스트.</param>
    /// <param name="width">옆 열이나 카드 오른쪽 여백까지의 너비.</param>
    private static void FitValue(TMP_Text text, float width)
    {
        if (text.rectTransform.sizeDelta == Vector2.zero) text.rectTransform.sizeDelta = new Vector2(width, 70);
        text.fontSizeMax = text.fontSize; text.fontSizeMin = 12; text.enableAutoSizing = true;
        text.textWrappingMode = TextWrappingModes.NoWrap;
    }


    /// <summary>3D 글자를 같은 오브젝트의 uGUI 글자로 바꾸고 작성된 문구와 색을 보존한다.</summary>
    /// <param name="target">기존 OverText 오브젝트.</param>
    private static TextMeshProUGUI ConvertLabel(Transform target)
    {
        if (target == null) throw new InvalidOperationException("OverText is missing.");
        var existing = target.GetComponent<TextMeshProUGUI>();
        if (existing != null) { existing.raycastTarget = false; return existing; }
        var old = target.GetComponent<TextMeshPro>();
        if (old == null) throw new InvalidOperationException("Expected TextMeshPro on " + target.name);
        // data: 컴포넌트 교체 전에 작성된 스타일을 보관한다.
        string text = old.text; var font = old.font; var material = old.fontSharedMaterial;
        var color = old.color; var alignment = old.alignment; var style = old.fontStyle;
        float size = old.fontSize * .1f, spacing = old.characterSpacing;
        UnityEngine.Object.DestroyImmediate(old);
        var renderer = target.GetComponent<MeshRenderer>();
        if (renderer != null) UnityEngine.Object.DestroyImmediate(renderer);
        var filter = target.GetComponent<MeshFilter>();
        if (filter != null) UnityEngine.Object.DestroyImmediate(filter);
        var label = target.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text; label.font = font; label.fontSharedMaterial = material;
        label.color = color; label.alignment = alignment; label.fontStyle = style; label.characterSpacing = spacing;
        label.raycastTarget = false; label.textWrappingMode = TextWrappingModes.NoWrap;
        label.fontSize = size; label.enableAutoSizing = true; label.fontSizeMin = 1; label.fontSizeMax = size;
        var rect = label.rectTransform;
        // rect: 위치와 스케일은 보존하고 0×0이었던 텍스트에 실제 슬롯 행 크기를 준다.
        rect.sizeDelta = new Vector2(((RectTransform)target.parent).rect.width / Mathf.Abs(rect.localScale.x), 24 / Mathf.Abs(rect.localScale.y));
        return label;
    }
}
#endif
