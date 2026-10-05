using System.Collections;
using System.Linq;
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
    /// <summary>실제 Stage_Battle에서 미리보기 이미지 제거와 순차 이동을 검증한다.</summary>
    public sealed class MovementSceneTests
    {
        /// <summary>실제 화면에서 잔여 1회 중 재시도 후 0회가 되고 다음 턴 이동이 가능한지 확인한다.</summary>
        [UnityTest]
        public IEnumerator BlockedRetryDoesNotRefreshLockInStageBattle()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Stage/Battle/BattleType/Stage_Battle.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Requires editor scene asset."); yield break;
#endif
            yield return null; yield return null;
            // view/controller/panel: 실제 이동 창에서 A의 이동과 금지 중 재시도를 예약한다.
            var view = Object.FindAnyObjectByType<StageBattleView>();
            var controller = view.GetComponent<TurnSandboxController>(); var panel = controller.MovePanel;
            controller.StepDelay = 0; controller.ReserveMove("A", 4); Click(view.SkipButton);
            for (int i = 0; i < 100 && controller.IsAdvancing; i++) yield return null;
            Assert.AreEqual(1, controller.Battle.Unit("A").MovementLockTurns);
            Click(panel.OpenButton); yield return null;
            Click(panel.UnitButtons.Single(b => b.gameObject.activeSelf && b.GetComponentInChildren<Text>().text.Contains("아군 A")));
            Click(panel.TargetButtons[4]); Click(panel.CloseButton);
            Click(view.StepButton); Click(view.StepButton); Click(view.StepButton);
            Assert.AreEqual(4, controller.Battle.Unit("A").Slot);
            Assert.AreEqual(1, controller.Battle.Unit("A").MovementLockTurns);
            StringAssert.Contains("재부여 없음", controller.Battle.Events.Last().Message);
            Click(view.SkipButton);
            for (int i = 0; i < 100 && controller.IsAdvancing; i++) yield return null;
            Assert.AreEqual(0, controller.Battle.Unit("A").MovementLockTurns);
            controller.ReserveMove("A", 5); Click(view.SkipButton);
            for (int i = 0; i < 100 && controller.IsAdvancing; i++) yield return null;
            Assert.AreEqual(5, controller.Battle.Unit("A").Slot);
        }

        /// <summary>교환당한 B의 다음 턴 상태와 목적지 버튼, 실제 자발적 이동까지 검사한다.</summary>
        [UnityTest]
        public IEnumerator SwappedAllyHasZeroLockAndCanReserveAndMoveNextTurn()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Stage/Battle/BattleType/Stage_Battle.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Requires editor scene asset."); yield break;
#endif
            yield return null; yield return null;
            // view/controller/panel: 실제 씬과 이동 창에서 A만 이동시킨다.
            var view = Object.FindAnyObjectByType<StageBattleView>();
            var controller = view.GetComponent<TurnSandboxController>(); var panel = controller.MovePanel;
            Click(panel.OpenButton); yield return null;
            Click(panel.TargetButtons[3]); Click(panel.CloseButton);
            controller.StepDelay = 0; Click(view.SkipButton);
            for (int i = 0; i < 100 && controller.IsAdvancing; i++) yield return null;
            Assert.AreEqual(2, controller.Battle.TurnNumber);
            Assert.AreEqual(1, controller.Battle.Unit("A").MovementLockTurns);
            Assert.AreEqual(0, controller.Battle.Unit("B").MovementLockTurns);
            Assert.AreEqual(1, controller.Battle.Unit("B").Slot);
            Click(panel.OpenButton); yield return null;
            // passengerButton: 버튼 순서 대신 이름으로 B를 선택하여 이동 후 순서 변경과 구분한다.
            var passengerButton = panel.UnitButtons.Single(b => b.gameObject.activeSelf && b.GetComponentInChildren<Text>().text.Contains("아군 B"));
            Click(passengerButton);
            StringAssert.Contains("이동 불가 0회", panel.GetComponentsInChildren<Text>().Single(t => t.name == "Status").text);
            Assert.IsTrue(panel.TargetButtons[2].interactable);
            yield return Capture(view, "movement-passenger-unlocked.png");
            Click(panel.TargetButtons[2]); Click(panel.CloseButton); Click(view.SkipButton);
            for (int i = 0; i < 100 && controller.IsAdvancing; i++) yield return null;
            Assert.AreEqual(3, controller.Battle.Unit("B").Slot);
            Assert.AreEqual(1, controller.Battle.Unit("B").MovementLockTurns);
            Assert.AreEqual(1, controller.Battle.Events.Count(e => e.Kind == TurnEventKind.MovementFinished && e.UnitId == "B"));
        }

        /// <summary>실제 버튼으로 예약하고 취소·재예약·확정·실행·초기화를 확인한다.</summary>
        [UnityTest]
        public IEnumerator PreviewIsReplacedByActualSequentialMovement()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Stage/Battle/BattleType/Stage_Battle.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Requires editor scene asset."); yield break;
#endif
            yield return null;
            // view/controller/panel: 사용자 씬에 생성된 실제 UI와 진행자.
            var view = Object.FindAnyObjectByType<StageBattleView>();
            var controller = view.GetComponent<TurnSandboxController>(); var panel = controller.MovePanel;
            Click(panel.OpenButton); yield return null;
            Click(panel.UnitButtons[0]); Click(panel.TargetButtons[3]);
            Assert.IsTrue(view.PreviewVisible); Assert.AreEqual("A", controller.DisplayedUnitAtSlot(4).Id);
            Assert.AreEqual("B", controller.DisplayedUnitAtSlot(1).Id);
            Assert.AreEqual(1, controller.Battle.Unit("A").Slot); Assert.AreEqual(2, controller.Battle.Unit("B").Slot);
            Assert.AreEqual(1, controller.Battle.Unit("A").RemainingSubActions);
            Assert.AreEqual(view.Slots[3].position.x, view.VisibleUnitSprite(4).transform.position.x, .001f);
            Assert.IsTrue(view.VisibleUnitSprite(4).gameObject.activeInHierarchy);
            yield return Capture(view, "movement-preview.png");
            Click(panel.ClearButton); Assert.IsFalse(view.PreviewVisible); Assert.AreEqual("A", controller.DisplayedUnitAtSlot(1).Id);
            Click(panel.TargetButtons[3]); Click(panel.CloseButton);
            // 스킬 예약을 추가해 이동 → 메인 → 종료의 연결도 실제 UI로 확인한다.
            Click(controller.SkillPanel.OpenButton); yield return null;
            Click(controller.SkillPanel.TargetButtons[8]); Click(controller.SkillPanel.ApplyButton); Click(controller.SkillPanel.CloseButton);
            Click(view.StepButton);
            Assert.IsFalse(view.PreviewVisible); Assert.IsNull(controller.MovementPreview);
            Assert.AreEqual("A", controller.DisplayedUnitAtSlot(1).Id); Assert.IsTrue(view.VisibleUnitSprite(1).gameObject.activeInHierarchy);
            yield return Capture(view, "movement-execution-start.png");
            Click(view.StepButton); Click(view.StepButton);
            Assert.AreEqual(2, controller.Battle.Unit("A").Slot); Assert.AreEqual(1, controller.Battle.Unit("B").Slot);
            Assert.AreEqual(view.Slots[1].position.x, view.VisibleUnitSprite(2).transform.position.x, .001f);
            Assert.AreEqual(0, controller.Battle.Unit("A").RemainingSubActions);
            Click(view.StepButton); Assert.AreEqual(3, controller.Battle.Unit("A").Slot);
            Click(view.StepButton); Assert.AreEqual(4, controller.Battle.Unit("A").Slot); Assert.AreEqual(2, controller.Battle.Unit("A").MovementLockTurns);
            yield return Capture(view, "movement-arrived.png");
            Click(view.StepButton); Assert.AreEqual(70, controller.Battle.Unit("X").CurrentHp);
            Click(view.StepButton); Assert.AreEqual(1, controller.Battle.Unit("A").MovementLockTurns);
            controller.StepDelay = 0; Click(view.SkipButton);
            for (int i = 0; i < 100 && controller.IsAdvancing; i++) yield return null;
            Assert.AreEqual(2, controller.Battle.TurnNumber); Assert.IsFalse(view.PreviewVisible); Assert.AreEqual(4, controller.Battle.Unit("A").Slot);
            Click(view.ResetButton); Assert.AreEqual(1, controller.Battle.Unit("A").Slot); Assert.AreEqual(0, controller.Battle.Unit("A").MovementLockTurns);
        }

        /// <summary>미리보기에서 교환된 아군 클릭은 표시된 유닛만 취소하며 초기화는 진행 중 경로도 중단한다.</summary>
        [UnityTest]
        public IEnumerator PreviewSelectionAndResetDoNotMutateWrongUnitOrContinueOldPath()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Stage/Battle/BattleType/Stage_Battle.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Requires editor scene asset."); yield break;
#endif
            yield return null;
            // view/controller: 4단계 표시 대상과 실제 예약 ID의 연결을 검사한다.
            var view = Object.FindAnyObjectByType<StageBattleView>(); var controller = view.GetComponent<TurnSandboxController>();
            controller.ReserveMove("A", 4); controller.ReserveSkill("B", "shot", "Y");
            // 씬 로드 직후 LateUpdate의 카메라/월드 슬롯 UI 정렬을 기다린다.
            yield return null;
            Click(view.SlotButtons[0]);
            Assert.IsNull(controller.Battle.MainReservation("B")); Assert.AreEqual(1, controller.Battle.MovementReservations("A").Count);
            Click(view.SlotButtons[3]); Assert.IsFalse(view.PreviewVisible); Assert.IsEmpty(controller.Battle.MovementReservations("A"));
            controller.ReserveMove("A", 4); controller.Step(); controller.Step(); controller.Step(); Assert.AreEqual(2, controller.Battle.Unit("A").Slot);
            controller.StepDelay = .1f; Click(view.SkipButton); Click(view.ResetButton);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.IsFalse(controller.IsAdvancing); Assert.AreEqual(TurnPhase.Planning, controller.Battle.Phase);
            Assert.AreEqual(1, controller.Battle.Unit("A").Slot); Assert.IsFalse(view.PreviewVisible); Assert.IsEmpty(controller.Battle.MovementReservations("A"));
        }
    }
}
