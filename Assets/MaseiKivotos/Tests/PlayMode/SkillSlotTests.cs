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
            Assert.AreEqual("SkillNameText", card.NameText.name); Assert.AreEqual("SkillCostText", card.BpText.name);
            Assert.AreEqual("기본 사격", card.NameText.text); Assert.AreEqual("5/5", card.BpText.text);
            StringAssert.Contains("적 1명", card.FlavorText.text); StringAssert.Contains("COST 0", card.FlavorText.text);
            Assert.AreEqual("사거리 0~8칸", card.RangeText.text); Assert.AreEqual("범위 단일(1명)", card.AreaText.text);
            Assert.IsNull(card.Icon.sprite); Assert.IsTrue(card.Icon.transform.Find("Icon Placeholder").gameObject.activeSelf);
            // 빈 카드 두 장도 표시/클릭하여 4스킬 편성에서 영역 중첩이 없는지 검사한다.
            for (int i = 2; i < 4; i++)
            {
                panel.SkillSlots[i].gameObject.SetActive(true);
                panel.SkillSlots[i].Show("표시 예제 " + i, "효과 설명 예제\n두 번째 줄도 표시합니다.", 13, 15, "1~3칸", "-1~1칸", SkillType.Change, i == 2 ? CombatTrait.Wet : CombatTrait.Death);
            }
            yield return Capture(view, "skill-slot-four-cards.png");
            Click(panel.SkillButtons[2]); Click(panel.SkillButtons[3]);
            panel.Refresh(); Assert.IsFalse(panel.SkillSlots[2].gameObject.activeSelf);
            Click(panel.SkillButtons[0]); Click(panel.TargetButtons[8]); Click(panel.ApplyButton); Click(panel.CloseButton);
            controller.StepDelay = 0; Click(view.SkipButton);
            for (int i = 0; i < 100 && controller.IsAdvancing; i++) yield return null;
            Click(panel.OpenButton); yield return null;
            Assert.AreEqual("4/5", card.BpText.text); Assert.AreEqual(70, controller.Battle.Unit("X").CurrentHp);
            yield return Capture(view, "skill-slot-current-bp.png");
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
                    card.Show("테스트", "보호 효과 설명", 13, 15, "1~3칸", "-1~1칸", types[i], CombatTrait.Flame, sprite);
                    Assert.AreEqual(typeNames[i], card.TypeBadge.GetComponentInChildren<TMP_Text>().text);
                    Assert.AreEqual(typeColors[i], ColorUtility.ToHtmlStringRGB(card.TypeBadge.color));
                }
                // names/colors: 원문의 enum 순서와 사용자가 지정한 11속성 팔레트.
                string[] names = { "염", "습", "전", "폭", "금", "신", "성", "암", "순", "사", "무" };
                string[] colors = { "F59437", "263A85", "24AAA5", "DA4141", "C0C0C0", "87CEEB", "FFEF99", "844CB3", "B1DE69", "4E3426", "FFC6DA" };
                for (int i = 0; i < names.Length; i++)
                {
                    card.Show("테스트", "보호 효과 설명", 13, 15, "1~3칸", "-1~1칸", SkillType.Tech, (CombatTrait)i, sprite);
                    Assert.AreEqual(names[i], card.TraitBadge.GetComponentInChildren<TMP_Text>().text);
                    Assert.AreEqual(colors[i], ColorUtility.ToHtmlStringRGB(card.TraitBadge.color));
                }
                Assert.AreEqual("13/15", card.BpText.text); Assert.AreEqual("보호 효과 설명", card.FlavorText.text);
                Assert.AreEqual("사거리 1~3칸", card.RangeText.text); Assert.AreEqual("범위 -1~1칸", card.AreaText.text);
                Assert.AreSame(sprite, card.Icon.sprite); Assert.IsFalse(card.Icon.transform.Find("Icon Placeholder").gameObject.activeSelf);
                card.Show("다음 스킬", "다음 효과", 0, 5, "0칸", "단일", SkillType.Mystic, CombatTrait.Neutral);
                Assert.IsNull(card.Icon.sprite); Assert.IsTrue(card.Icon.transform.Find("Icon Placeholder").gameObject.activeSelf);
                Assert.AreEqual("0/5", card.BpText.text);
                Assert.IsTrue(card.GetComponentsInChildren<UnityEngine.UI.Graphic>().Where(g => g.gameObject != card.gameObject).All(g => !g.raycastTarget));
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(sprite); Object.DestroyImmediate(texture); }
        }
    }
}
