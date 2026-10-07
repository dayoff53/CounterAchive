using System.Collections;
using System.Linq;
using MaseiKivotos.Core;
using MaseiKivotos.Unity;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using static MaseiKivotos.Tests.StageBattleTests;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace MaseiKivotos.Tests
{
    /// <summary>기존 프리팹의 실제 컴포넌트에 데이터·색·이미지가 연결되는지 검사한다.</summary>
    public sealed class SkillSlotTests
    {
        /// <summary>초과 경계, 일부/전체 초과, 독립 안내와 정상 복귀를 실제 프리팹으로 검사한다.</summary>
        [Test]
        public void OverflowLabelsFollowIntervalsAndUseCanvasText()
        {
            var root = new GameObject("Overflow Test", typeof(RectTransform));
            var card = SkillSlotView.Create(Resources.Load<GameObject>(SkillSlotView.PrefabPath), root.transform, null);
            try
            {
                Assert.IsInstanceOf<TextMeshProUGUI>(card.AreaOverText);
                Assert.IsInstanceOf<TextMeshProUGUI>(card.RangeOverText);
                Assert.IsNull(card.AreaOverText.GetComponent<MeshRenderer>());
                Assert.IsNull(card.RangeOverText.GetComponent<MeshRenderer>());
                Assert.IsFalse(card.AreaOverText.raycastTarget); Assert.IsFalse(card.RangeOverText.raycastTarget);
                var diagram = card.transform.Find("Area&RangeSlots");
                Assert.Greater(card.AreaOverText.transform.GetSiblingIndex(), diagram.Find("RangeSlots").GetSiblingIndex());
                Assert.Greater(card.RangeOverText.transform.GetSiblingIndex(), diagram.Find("AreaSlots").GetSiblingIndex());
                card.Show("표기 예제", "검증", 3, 5, 4, 3, 6, -1, 1, SkillType.Tech, CombatTrait.Flame);
                Assert.AreEqual("Cost 4", card.CostText.text); Assert.AreEqual("BP 3/5", card.BpText.text);
                Assert.AreEqual("Area -1~1", card.AreaText.text); Assert.AreEqual("Range 3~6", card.RangeText.text);
                Assert.IsFalse(card.AreaOverText.gameObject.activeSelf); Assert.IsFalse(card.RangeOverText.gameObject.activeSelf);
                card.ShowDiagram(0, 8, -4, 4);
                Assert.IsFalse(card.AreaOverText.gameObject.activeSelf); Assert.IsFalse(card.RangeOverText.gameObject.activeSelf);
                card.ShowDiagram(0, 8, -5, 4);
                Assert.IsTrue(card.AreaOverText.gameObject.activeSelf); Assert.IsFalse(card.RangeOverText.gameObject.activeSelf);
                card.ShowDiagram(0, 8, -4, 5);
                Assert.IsTrue(card.AreaOverText.gameObject.activeSelf);
                card.ShowDiagram(3, 9, -4, 4);
                Assert.IsFalse(card.AreaOverText.gameObject.activeSelf); Assert.IsTrue(card.RangeOverText.gameObject.activeSelf);
                // 전체 구간이 도식 밖인 경우에도 실제 입력 문구와 초과 안내를 유지한다.
                card.ShowDiagram(9, 12, 5, 8);
                Assert.IsTrue(card.AreaOverText.gameObject.activeSelf); Assert.IsTrue(card.RangeOverText.gameObject.activeSelf);
                Assert.AreEqual("Area 5~8", card.AreaText.text); Assert.AreEqual("Range 9~12", card.RangeText.text);
                Assert.AreEqual(1, card.RangeSlots.Count(s => s.color.a == 1));
                card.ShowDiagram(0, 0, 0, 0);
                Assert.IsFalse(card.AreaOverText.gameObject.activeSelf); Assert.IsFalse(card.RangeOverText.gameObject.activeSelf);
                Assert.AreEqual("Area 0", card.AreaText.text); Assert.AreEqual("Range 0", card.RangeText.text);
            }
            finally { Object.DestroyImmediate(root); }
        }

        /// <summary>좌우 범위·최소 사거리·필드 경계·재표시와 편집한 배치 보존을 실제 프리팹에서 검사한다.</summary>
        [Test]
        public void DiagramIntervalsResetAndAuthoredLayoutSurvivesBinding()
        {
            var root = new GameObject("Diagram Test", typeof(RectTransform));
            var card = SkillSlotView.Create(Resources.Load<GameObject>(SkillSlotView.PrefabPath), root.transform, null);
            try
            {
                var diagram = card.transform.Find("Area&RangeSlots");
                Assert.AreEqual(4, diagram.childCount);
                for (int i = 0; i < 9; i++)
                {
                    Assert.AreEqual("AreaSlot" + (i + 1), card.AreaSlots[i].name);
                    Assert.AreEqual("RangeSlot" + (i + 1), card.RangeSlots[i].name);
                    Assert.AreSame(diagram.Find("AreaSlots"), card.AreaSlots[i].transform.parent);
                    Assert.AreSame(diagram.Find("RangeSlots"), card.RangeSlots[i].transform.parent);
                    if (i > 0)
                    {
                        Assert.Greater(card.AreaSlots[i].rectTransform.anchoredPosition.x, card.AreaSlots[i - 1].rectTransform.anchoredPosition.x + 23);
                        Assert.Greater(card.RangeSlots[i].rectTransform.anchoredPosition.x, card.RangeSlots[i - 1].rectTransform.anchoredPosition.x + 23);
                    }
                }
                Assert.IsTrue(card.FlavorText.transform.IsChildOf(card.transform.Find("FlavorTextBox")));
                // 실행 전에 편집한 위치와 글자 크기가 데이터 갱신 후에도 유지되는지 검사한다.
                card.NameText.rectTransform.anchoredPosition += new Vector2(13, -7);
                card.NameText.fontSize = 31;
                var rects = card.GetComponentsInChildren<RectTransform>(true);
                var positions = rects.Select(r => r.anchoredPosition).ToArray();
                var sizes = rects.Select(r => r.sizeDelta).ToArray();
                card.Show("광역 표시 예제", "표시 전용", 4, 7, 3, 3, 6, -2, 2, SkillType.Tech, CombatTrait.Flame);
                CollectionAssert.AreEqual(new[] { "808080", "808080", "F59437", "F59437", "DA4141", "F59437", "F59437", "808080", "808080" },
                    card.AreaSlots.Select(s => ColorUtility.ToHtmlStringRGB(s.color)).ToArray());
                CollectionAssert.AreEqual(new[] { "48C370", "808080", "808080", "87CEEB", "87CEEB", "87CEEB", "87CEEB", "808080", "808080" },
                    card.RangeSlots.Select(s => ColorUtility.ToHtmlStringRGB(s.color)).ToArray());
                card.ShowDiagram(0, 0, 0, 0);
                Assert.AreEqual(1, card.AreaSlots.Count(s => s.color.a == 1));
                Assert.AreEqual(1, card.RangeSlots.Count(s => s.color.a == 1));
                card.ShowDiagram(8, 8, -8, 8);
                Assert.AreEqual("87CEEB", ColorUtility.ToHtmlStringRGB(card.RangeSlots[8].color));
                Assert.AreEqual(7, card.RangeSlots.Count(s => s.color.a < 1));
                Assert.AreEqual(8, card.AreaSlots.Count(s => ColorUtility.ToHtmlStringRGB(s.color) == "F59437"));
                Assert.Throws<System.ArgumentOutOfRangeException>(() => card.ShowDiagram(6, 3, 0, 0));
                Assert.Throws<System.ArgumentOutOfRangeException>(() => card.ShowDiagram(-1, 8, 0, 0));
                Assert.Throws<System.ArgumentOutOfRangeException>(() => card.ShowDiagram(0, 8, 2, -2));
                CollectionAssert.AreEqual(positions, rects.Select(r => r.anchoredPosition).ToArray());
                CollectionAssert.AreEqual(sizes, rects.Select(r => r.sizeDelta).ToArray());
                Assert.AreEqual(31, card.NameText.fontSize);
                card.PreviewDiagram();
                Assert.AreEqual("Range 3~6", card.RangeText.text);
                Assert.AreEqual("Area -1~1", card.AreaText.text);
            }
            finally { Object.DestroyImmediate(root); }
        }

        /// <summary>최대 4장의 카드, 선택 버튼, 실행 후 실제 BP 갱신을 검증한다.</summary>
        [UnityTest]
        public IEnumerator PrefabCardsShowDataAndRefreshBpAfterExecution()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Stage/Battle/BattleType/Stage_Battle.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Requires editor scene asset."); yield break;
#endif
            yield return null; yield return null;
            // view/controller/panel: 실제 씬의 새 프리팹 카드 선택 창.
            var view = Object.FindAnyObjectByType<StageBattleView>();
            var controller = view.GetComponent<TurnSandboxController>(); var panel = controller.SkillPanel;
            Click(panel.OpenButton); yield return null;
            Assert.AreEqual(4, panel.SkillSlots.Count);
            // card: 첫 번째 소유 스킬의 실제 프리팹 인스턴스.
            var card = panel.SkillSlots[0];
            Assert.AreEqual("NameText", card.NameText.name); Assert.AreEqual("BpText", card.BpText.name);
            Assert.AreEqual("기본 사격", card.NameText.text); Assert.AreEqual("BP 5/5", card.BpText.text);
            StringAssert.Contains("적 1명", card.FlavorText.text); StringAssert.Contains("COST 0", card.FlavorText.text);
            Assert.AreEqual("Range 0~8", card.RangeText.text); Assert.AreEqual("Area 0", card.AreaText.text);
            Assert.IsNull(card.Icon.sprite); Assert.IsTrue(card.Icon.transform.Find("Icon Placeholder").gameObject.activeSelf);
            // 빈 카드 두 장도 표시/클릭하여 4스킬 편성에서 영역 중첩이 없는지 검사한다.
            for (int i = 2; i < 4; i++)
            {
                panel.SkillSlots[i].transform.parent.gameObject.SetActive(true); panel.SkillSlots[i].gameObject.SetActive(true);
                panel.SkillSlots[i].Show("표시 예제 " + i, "효과 설명 예제\n두 번째 줄도 표시합니다.", 13, 15, 2, 1, 3, -1, 1, SkillType.Change, i == 2 ? CombatTrait.Wet : CombatTrait.Death);
            }
            yield return Capture(view, "skill-slot-four-cards.png");
            var scroll = card.GetComponentInParent<UnityEngine.UI.ScrollRect>();
            scroll.content.sizeDelta = new Vector2(2400, 294);
            scroll.horizontalNormalizedPosition = 1;
            yield return null;
            Click(panel.SkillButtons[2]); Click(panel.SkillButtons[3]);
            scroll.horizontalNormalizedPosition = 0;
            panel.Refresh(); Assert.IsFalse(panel.SkillSlots[2].gameObject.activeSelf);
            Click(panel.SkillButtons[0]); Click(panel.TargetButtons[8]); Click(panel.ApplyButton); Click(panel.CloseButton);
            controller.StepDelay = 0; Click(view.SkipButton);
            for (int i = 0; i < 100 && controller.IsAdvancing; i++) yield return null;
            Click(panel.OpenButton); yield return null;
            Assert.AreEqual("BP 4/5", card.BpText.text); Assert.AreEqual(70, controller.Battle.Unit("X").CurrentHp);
            yield return Capture(view, "skill-slot-current-bp.png");
            // detailRoot: 에디터 원본 크기에서 글자·도식·스크롤을 확인하는 표시 전용 카드.
            var detailRoot = new GameObject("SkillSlot Detail", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            detailRoot.transform.SetParent(panel.OpenButton.transform.parent, false);
            var detailRect = (RectTransform)detailRoot.transform;
            detailRect.anchorMin = Vector2.zero; detailRect.anchorMax = Vector2.one; detailRect.offsetMin = detailRect.offsetMax = Vector2.zero;
            detailRoot.GetComponent<UnityEngine.UI.Image>().color = new Color32(30, 36, 48, 255);
            var detail = SkillSlotView.Create(Resources.Load<GameObject>(SkillSlotView.PrefabPath), detailRoot.transform, null);
            ((RectTransform)detail.transform).anchoredPosition = new Vector2(296, -160);
            detail.Show("범위 예제", "목표는 빨강, 주변 영향 범위는 주황입니다.\n사용자는 초록, 사거리 3~6칸은 하늘색입니다.\n이 카드는 전투 효과가 아닌 UI 검증용 예제입니다.",
                13, 15, 2, 3, 6, -2, 2, SkillType.Mystic, CombatTrait.Flame);
            yield return Capture(view, "skill-slot-detail.png");
            detail.Show("초과 예제", "도식에 들어오지 않는 구간은 Over 안내를 표시합니다.\nArea와 Range 문구에는 생략 없이 원래 값을 표시합니다.",
                3, 5, 4, 3, 12, -6, 6, SkillType.Mystic, CombatTrait.Flame);
            yield return Capture(view, "skill-slot-over-detail.png");
            Assert.LessOrEqual(detail.AreaText.textBounds.size.x, detail.AreaText.rectTransform.rect.width + 1);
            Assert.LessOrEqual(detail.RangeText.textBounds.size.x, detail.RangeText.rectTransform.rect.width + 1);
            // canvasRenderer 깊이를 비교해 실제 렌더링 순서도 검증한다.
            var maxSlotDepth = detail.AreaSlots.Concat(detail.RangeSlots).Max(s => s.canvasRenderer.absoluteDepth);
            Assert.Greater(((TextMeshProUGUI)detail.AreaOverText).canvasRenderer.absoluteDepth, maxSlotDepth);
            Assert.Greater(((TextMeshProUGUI)detail.RangeOverText).canvasRenderer.absoluteDepth, maxSlotDepth);
            Object.Destroy(detailRoot);
        }

        /// <summary>3타입·11속성의 이름/색, BP 형식, 이미지 지정/해제를 실제 프리팹으로 대조한다.</summary>
        [Test]
        public void PrefabFieldsCoverEveryTypeTraitAndOptionalIcon()
        {
            // root/card: 전투 없이 표시 기능만 검증하는 프리팹 복제본.
            var root = new GameObject("Skill Slot Test", typeof(RectTransform));
            var card = SkillSlotView.Create(Resources.Load<GameObject>(SkillSlotView.PrefabPath), root.transform, null);
            // texture/sprite: 아이콘 설정 후 null 재연결 시 오래된 이미지가 남지 않는지 확인할 임시 이미지.
            var texture = new Texture2D(2, 2); var sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.one * .5f);
            try
            {
                // types/typeNames/typeColors: 사용자 타입 요구의 독립 기대값.
                var types = new[] { SkillType.Tech, SkillType.Mystic, SkillType.Change };
                string[] typeNames = { "태크", "신비", "변화" }; string[] typeColors = { "F59437", "87CEEB", "919191" };
                for (int i = 0; i < types.Length; i++)
                {
                    card.Show("테스트", "보호 효과 설명", 13, 15, 2, 1, 3, -1, 1, types[i], CombatTrait.Flame, sprite);
                    Assert.AreEqual(typeNames[i], card.TypeBadge.GetComponentInChildren<TMP_Text>().text);
                    Assert.AreEqual(typeColors[i], ColorUtility.ToHtmlStringRGB(card.TypeBadge.color));
                }
                // names/colors: 원문의 enum 순서와 사용자가 지정한 11속성 팔레트.
                string[] names = { "염", "습", "전", "폭", "금", "신", "성", "암", "순", "사", "무" };
                string[] colors = { "F59437", "263A85", "24AAA5", "DA4141", "C0C0C0", "87CEEB", "FFEF99", "844CB3", "B1DE69", "4E3426", "FFC6DA" };
                for (int i = 0; i < names.Length; i++)
                {
                    card.Show("테스트", "보호 효과 설명", 13, 15, 2, 1, 3, -1, 1, SkillType.Tech, (CombatTrait)i, sprite);
                    Assert.AreEqual(names[i], card.TraitBadge.GetComponentInChildren<TMP_Text>().text);
                    Assert.AreEqual(colors[i], ColorUtility.ToHtmlStringRGB(card.TraitBadge.color));
                }
                Assert.AreEqual("Cost 2", card.CostText.text); Assert.AreEqual("BP 13/15", card.BpText.text); Assert.AreEqual("보호 효과 설명", card.FlavorText.text);
                Assert.AreEqual("Range 1~3", card.RangeText.text); Assert.AreEqual("Area -1~1", card.AreaText.text);
                Assert.AreSame(sprite, card.Icon.sprite); Assert.IsFalse(card.Icon.transform.Find("Icon Placeholder").gameObject.activeSelf);
                card.Show("다음 스킬", "다음 효과", 0, 5, 0, 0, 0, 0, 0, SkillType.Mystic, CombatTrait.Neutral);
                Assert.IsNull(card.Icon.sprite); Assert.IsTrue(card.Icon.transform.Find("Icon Placeholder").gameObject.activeSelf);
                Assert.AreEqual("BP 0/5", card.BpText.text);
                Assert.IsTrue(card.GetComponentsInChildren<UnityEngine.UI.Graphic>().Where(g => g.gameObject != card.gameObject && g.GetComponentInParent<UnityEngine.UI.ScrollRect>() == null).All(g => !g.raycastTarget));
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(sprite); Object.DestroyImmediate(texture); }
        }
    }
}
