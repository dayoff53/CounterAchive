using System.Collections;
using MaseiKivotos.Core;
using MaseiKivotos.Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using static MaseiKivotos.Tests.StageBattleTests;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace MaseiKivotos.Tests
{
    /// <summary>기존 슬롯/HP 리소스가 실제 씬의 피해·이동·퇴각·초기화를 따라가는지 검증한다.</summary>
    public sealed class UnitHudTests
    {
        /// <summary>동일 스프라이트의 UI 전환과 HP 갱신, 미리보기/실제 교환 중 유닛 추적을 검사한다.</summary>
        [UnityTest]
        public IEnumerator RetainedResourcesFollowHpAcrossPreviewExecutionRetreatAndReset()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Stage/Battle/BattleType/Stage_Battle.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Requires the editor scene asset."); yield break;
#endif
            yield return null; yield return null;
            // view/controller: 실제 Stage_Battle의 리소스 화면과 전투 진행자.
            var view = Object.FindAnyObjectByType<StageBattleView>();
            var controller = view.GetComponent<TurnSandboxController>();
            // unitPrefab: 원본과 같은 HP 배경/채움 스프라이트인지 비교할 프리팹 에셋.
            var unitPrefab = Resources.Load<GameObject>("prefab/Unit");
            for (int i = 0; i < 9; i++)
            {
                // source: UI로 옮겨 표시한 슬롯 바닥의 원래 월드 렌더러.
                var source = view.Slots[i].Find("SlotGroundSprite").GetComponent<SpriteRenderer>();
                Assert.AreSame(source.sprite, view.SlotGrounds[i].sprite);
                Assert.IsFalse(source.enabled); Assert.IsFalse(view.SlotGrounds[i].raycastTarget);
                Assert.AreSame(unitPrefab.transform.Find("UnitCanvas/HpBarBackground").GetComponent<Image>().sprite, view.HpBackgrounds[i].sprite);
                Assert.AreSame(unitPrefab.transform.Find("UnitCanvas/HpBarBackground/HpBar").GetComponent<Image>().sprite, view.HpFills[i].sprite);
                Assert.IsFalse(view.HpBackgrounds[i].transform.Find("ApBar").gameObject.activeSelf);
                Assert.IsFalse(view.HpBackgrounds[i].transform.Find("HpText").gameObject.activeSelf);
                Assert.IsFalse(view.HpBackgrounds[i].raycastTarget); Assert.IsFalse(view.HpFills[i].raycastTarget);
            }
            AssertHp(view, 1, 100); AssertHp(view, 2, 100); AssertHp(view, 3, null);
            // 새 장식이 입력을 가로채지 않고 같은 슬롯의 예약을 켜고 끌 수 있어야 한다.
            Click(view.SlotButtons[0]); Click(view.SlotButtons[0]);
            yield return Capture(view, "unit-hud-full.png");
            // 적 AI는 미구현이므로 Core의 공개 예약 API로 A에게 반드시 명중하는 공격을 준비한다.
            controller.Battle.ReserveMainSkill("X", "shot", "A");
            controller.StepDelay = 0; Click(view.SkipButton);
            for (int i = 0; i < 100 && controller.IsAdvancing; i++) yield return null;
            Assert.AreEqual(70, controller.Battle.Unit("A").CurrentHp);
            AssertHp(view, 1, 70); AssertHp(view, 2, 100);
            yield return Capture(view, "unit-hud-damaged.png");
            controller.ReserveMove("A", 4);
            Assert.IsTrue(view.PreviewVisible);
            AssertHp(view, 4, 70); AssertHp(view, 1, 100); AssertHp(view, 2, null);
            yield return Capture(view, "unit-hud-preview.png");
            Click(view.StepButton);
            Assert.IsFalse(view.PreviewVisible); AssertHp(view, 1, 70); AssertHp(view, 2, 100); AssertHp(view, 4, null);
            Click(view.StepButton); Click(view.StepButton);
            Assert.AreEqual(2, controller.Battle.Unit("A").Slot);
            AssertHp(view, 2, 70); AssertHp(view, 1, 100);
            yield return Capture(view, "unit-hud-swap.png");
            // 퇴각은 효과 완료 API로 주입하고 화면 갱신으로 빈 슬롯의 잔상 여부를 확인한다.
            controller.Battle.ApplyCompletedRetreatBatch(new[] { "A" }); view.Refresh(controller);
            AssertHp(view, 2, null); AssertHp(view, 1, 100);
            Click(view.ResetButton);
            AssertHp(view, 1, 100); AssertHp(view, 2, 100); AssertHp(view, 4, null);
        }

        /// <summary>슬롯의 HP 표시 유무·정확한 숫자·비율을 함께 확인한다.</summary>
        /// <param name="view">확인할 실제 전투 화면.</param>
        /// <param name="slot">1~9 슬롯 번호.</param>
        /// <param name="hp">최대 HP 100 예제의 기대 체력. null이면 빈 슬롯.</param>
        private static void AssertHp(StageBattleView view, int slot, int? hp)
        {
            Assert.AreEqual(hp.HasValue, view.HpBackgrounds[slot - 1].gameObject.activeInHierarchy);
            Assert.AreEqual(hp.HasValue, view.HpLabels[slot - 1].gameObject.activeInHierarchy);
            Assert.AreEqual(hp.HasValue ? "HP " + hp + "/100" : "", view.HpLabels[slot - 1].text);
            Assert.AreEqual(hp.GetValueOrDefault() / 100f, view.HpFills[slot - 1].fillAmount, .001f);
        }
    }
}
