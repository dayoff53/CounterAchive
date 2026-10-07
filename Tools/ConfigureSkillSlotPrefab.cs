#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using MaseiKivotos.Unity;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>검증 사본에서만 명시적으로 실행하는 SkillSlot 1회 이행 도구.</summary>
public static class ConfigureSkillSlotPrefab
{
    /// <summary>사용자 계층과 외형을 보존하고 슬롯 중첩 해제 및 표시 참조를 저장한다.</summary>
    public static void Run()
    {
        const string path = "Assets/Resources/prefab/UI/SkillSlot.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var rect = (RectTransform)root.transform;
            var diagram = root.transform.Find("Area&RangeSlots") as RectTransform;
            if (diagram == null) throw new InvalidOperationException("Area&RangeSlots missing.");
            var areas = diagram.Cast<Transform>().Where(t => t.name.Contains("AreaSlot")).ToArray();
            if (areas.Length != 9) throw new InvalidOperationException("Expected nine area slots.");
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            var layout = diagram.GetComponent<HorizontalLayoutGroup>();
            // 격리된 프리팹 내용은 Canvas 갱신 주기가 없으므로 기존 그룹의 배치를 명시적으로 계산한다.
            if (layout != null)
            {
                layout.CalculateLayoutInputHorizontal(); layout.CalculateLayoutInputVertical();
                layout.SetLayoutHorizontal(); layout.SetLayoutVertical();
            }
            // corners: 레이아웃 적용 후의 위치를 기록하고, 형제 분리 후 같은 위치인지 검증한다.
            var areaCorners = areas.Select(t => Corners((RectTransform)t)).ToArray();
            var rangeCorners = areas.Select(t => Corners((RectTransform)t.Find("RangeSlot"))).ToArray();
            if (layout != null) UnityEngine.Object.DestroyImmediate(layout);
            var ranges = new Image[9];
            for (int i = 0; i < 9; i++)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(areas[i]))
                    PrefabUtility.UnpackPrefabInstance(areas[i].gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                areas[i].name = "AreaSlot" + (i + 1);
                var range = areas[i].Find("RangeSlot");
                if (range == null) throw new InvalidOperationException("RangeSlot missing.");
                range.name = "RangeSlot" + (i + 1);
                range.SetParent(diagram, true);
                ranges[i] = range.GetComponent<Image>();
                RequireSameCorners(areaCorners[i], Corners((RectTransform)areas[i]));
                RequireSameCorners(rangeCorners[i], Corners((RectTransform)range));
            }
            for (int i = 0; i < 9; i++) { areas[i].SetSiblingIndex(i); ranges[i].transform.SetSiblingIndex(9 + i); }

            // 이름이 다른 구형 요소는 역할이 확인된 별명만 허용한다.
            var name = Find<TMP_Text>(root, "NameText", "SkillNameText");
            var cost = Find<TMP_Text>(root, "CostText", "Cost");
            var bp = Find<TMP_Text>(root, "BpText", "BPText", "SkillCostText");
            var area = Find<TMP_Text>(root, "AreaText", "SkillAreaText");
            var rangeText = Find<TMP_Text>(root, "RangeText", "SkillRangeText");
            var flavorBox = root.transform.Find("FlavorTextBox");
            if (flavorBox == null) throw new InvalidOperationException("FlavorTextBox missing.");
            var flavor = Find<TMP_Text>(flavorBox.gameObject, "FlavorText", "SkillFlavorText");
            var icon = Find<Image>(root, "IconText", "SkillIcon");
            var type = Find<Image>(root, "TypeText", "SkillType");
            var trait = Find<Image>(root, "TraitText", "SkillTrait");
            var typeLabel = Label(type.transform, "TypeLabel", name.font);
            var traitLabel = Label(trait.transform, "TraitLabel", name.font);
            var placeholder = Label(icon.transform, "Icon Placeholder", name.font);
            placeholder.text = "미정"; typeLabel.text = "타입"; traitLabel.text = "속성";
            placeholder.color = new Color32(28, 31, 38, 255);
            icon.preserveAspect = true;
            var button = root.GetComponent<Button>() ?? root.AddComponent<Button>();
            button.targetGraphic = root.GetComponent<Image>(); button.transition = Selectable.Transition.None;
            var outline = root.GetComponent<Outline>() ?? root.AddComponent<Outline>();
            outline.effectColor = new Color32(28, 173, 133, 255); outline.effectDistance = new Vector2(3, -3); outline.enabled = false;
            // 설명 스크롤은 입력을 유지하고 다른 장식은 카드 클릭을 가로채지 않는다.
            foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = graphic.gameObject == root || graphic.GetComponentInParent<ScrollRect>() != null;
            var view = root.GetComponent<SkillSlotView>() ?? root.AddComponent<SkillSlotView>();
            var so = new SerializedObject(view);
            Set(so, "button", button); Set(so, "selection", outline);
            Set(so, "nameText", name); Set(so, "costText", cost); Set(so, "bpText", bp);
            Set(so, "areaText", area); Set(so, "rangeText", rangeText); Set(so, "flavorText", flavor);
            Set(so, "icon", icon); Set(so, "typeBadge", type); Set(so, "traitBadge", trait);
            Set(so, "typeLabel", typeLabel); Set(so, "traitLabel", traitLabel); Set(so, "iconPlaceholder", placeholder);
            var areaArray = so.FindProperty("areaSlots"); var rangeArray = so.FindProperty("rangeSlots");
            areaArray.arraySize = rangeArray.arraySize = 9;
            for (int i = 0; i < 9; i++)
            {
                areaArray.GetArrayElementAtIndex(i).objectReferenceValue = areas[i].GetComponent<Image>();
                rangeArray.GetArrayElementAtIndex(i).objectReferenceValue = ranges[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            view.ValidateReferences(); view.PreviewDiagram();
            name.text = "스킬 이름"; cost.text = "COST 0"; bp.text = "5/5";
            area.text = "-1~1칸"; rangeText.text = "3~6칸";
            flavor.text = "스킬 효과 설명이 표시됩니다.\n실행하면 선택한 스킬의 실제 데이터로 갱신됩니다.";
            PrefabUtility.SaveAsPrefabAsset(root, path, out bool saved);
            if (!saved) throw new InvalidOperationException("Saving SkillSlot failed.");
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/skill-slot-migration.txt", "References assigned; 9 area + 9 range siblings; nested instances unpacked; all 18 world corner sets preserved.\n" +
                string.Join("\n", root.GetComponentsInChildren<Transform>(true).Select(t => t.name)));
            Debug.Log("SKILL_SLOT_MIGRATION_OK");
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    /// <summary>같은 역할의 허용된 이름만 찾아 요청된 이름으로 정규화한다.</summary>
    private static T Find<T>(GameObject root, string expected, params string[] aliases) where T : Component
    {
        var matches = root.GetComponentsInChildren<T>(true).Where(t => t.name == expected || aliases.Contains(t.name)).ToArray();
        if (matches.Length != 1) throw new InvalidOperationException("Expected one " + expected);
        matches[0].name = expected; return matches[0];
    }
    /// <summary>새 이름 표시만 이미지 안에 작성한다. 기존 요소의 배치는 건드리지 않는다.</summary>
    private static TMP_Text Label(Transform parent, string name, TMP_FontAsset font)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); go.transform.SetParent(parent, false);
        var label = go.GetComponent<TextMeshProUGUI>(); label.font = font; label.fontSize = 24;
        label.alignment = TextAlignmentOptions.Center; label.color = Color.white; label.raycastTarget = false;
        var rect = label.rectTransform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        return label;
    }
    /// <summary>직렬화 참조를 설정한다.</summary>
    private static void Set(SerializedObject so, string name, UnityEngine.Object value) => so.FindProperty(name).objectReferenceValue = value;
    /// <summary>계층 이동 전후의 시각 영역을 기록한다.</summary>
    private static Vector3[] Corners(RectTransform rect) { var result = new Vector3[4]; rect.GetWorldCorners(result); return result; }
    /// <summary>월드 배치가 유지되지 않으면 저장하지 않는다.</summary>
    private static void RequireSameCorners(Vector3[] before, Vector3[] after)
    {
        for (int i = 0; i < 4; i++) if (Vector3.Distance(before[i], after[i]) > .01f) throw new InvalidOperationException("Slot layout changed during unpack/reparent.");
    }
}
#endif
