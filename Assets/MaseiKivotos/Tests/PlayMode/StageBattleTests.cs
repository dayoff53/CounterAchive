using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MaseiKivotos.Core;
using MaseiKivotos.Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace MaseiKivotos.Tests
{
    /// <summary>
    /// 기존 InGame 리소스와 새 턴 로직의 연결을 실제 씬에서 확인하는 PlayMode 테스트.
    /// </summary>
    public sealed class StageBattleTests
    {
        /// <summary>BP 0 원래 스킬을 UI로 예약하여 헤일로 이상·필수 후속 효과·다음 턴·초기화까지 진행한다.</summary>
        [UnityTest]
        public IEnumerator ZeroBpUsesHaloAnomalyAndContinuesWithoutStalling()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Stage/Battle/BattleType/Stage_Battle.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Requires the editor scene asset."); yield break;
#endif
            yield return null;
            // view/controller: 실제 씬에서 BP가 고갈된 다음 턴을 준비하는 진행자.
            var view = Object.FindAnyObjectByType<StageBattleView>();
            var controller = view.GetComponent<TurnSandboxController>();
            controller.Step(); controller.Battle.ApplySkillBpChange("A", "shot", -5);
            controller.StepDelay = 0; controller.SkipTurn();
            for (int i = 0; i < 64 && controller.IsAdvancing; i++) yield return null;
            Assert.AreEqual(TurnPhase.Planning, controller.Battle.Phase);
            // panel: BP 0 안내와 원래 스킬 예약을 확인한다. 대체 스킬을 직접 고르지 않는다.
            var panel = controller.SkillPanel;
            Click(panel.OpenButton); yield return null;
            Click(panel.UnitButtons[0]); Click(panel.SkillButtons[0]); Click(panel.TargetButtons[8]); Click(panel.ApplyButton);
            StringAssert.Contains("BP 0이면 헤일로 이상", panel.VisibleInfo);
            yield return Capture(view, "halo-anomaly-planning.png");
            Click(panel.CloseButton);
            Click(view.StepButton); Click(view.StepButton); Click(view.StepButton);
            Assert.AreEqual(1, controller.Battle.Events.Count(e => e.Kind == TurnEventKind.SkillReplaced));
            Assert.AreEqual(1, controller.Battle.Events.Count(e => e.Kind == TurnEventKind.SkillFollowUp));
            Assert.AreEqual(5, controller.Battle.Costs[1]); Assert.AreEqual(100, controller.Battle.Unit("X").CurrentHp);
            Assert.LessOrEqual(controller.Battle.Unit("A").CurrentHp, 45);
            Assert.AreEqual(40, controller.Battle.Unit("A").HaloBp); Assert.AreEqual(0, controller.Battle.Unit("A").SkillBp("shot"));
            yield return Capture(view, "halo-anomaly-executed.png");
            Click(view.SkipButton);
            for (int i = 0; i < 64 && controller.IsAdvancing; i++) yield return null;
            Assert.IsFalse(controller.IsAdvancing); Assert.AreEqual(3, controller.Battle.TurnNumber);
            Assert.IsTrue(view.SkipButton.interactable); Assert.IsTrue(view.StepButton.interactable);
            Click(view.ResetButton);
            Assert.AreEqual(TurnPhase.Planning, controller.Battle.Phase);
            Assert.AreEqual(100, controller.Battle.Unit("A").CurrentHp); Assert.IsTrue(view.SkipButton.interactable);
        }

        /// <summary>실제 스킬 선택 창의 사용자·스킬·대상 버튼으로 예약하고 HP/BP 변화를 확인한다.</summary>
        [UnityTest]
        public IEnumerator MainSkillPanelReservesAndExecutesDamageWithoutOverwritingPlans()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Stage/Battle/BattleType/Stage_Battle.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Requires the editor scene asset."); yield break;
#endif
            yield return null;
            // view/controller/panel: 실제 씬에 구성된 화면·진행자·스킬 선택 창.
            var view = Object.FindAnyObjectByType<StageBattleView>();
            var controller = view.GetComponent<TurnSandboxController>();
            var panel = controller.SkillPanel;
            Click(panel.OpenButton);
            yield return null;
            Click(panel.UnitButtons[0]); Click(panel.SkillButtons[1]);
            Click(panel.TargetButtons[8]); Click(panel.ApplyButton);
            Assert.AreEqual("mystic", controller.Battle.MainReservation("A").SkillId);
            Assert.AreEqual(1, controller.Battle.ReservedCost(1));
            Click(panel.EndOnlyButton); Assert.IsNull(controller.Battle.MainReservation("A"));
            Assert.AreEqual(0, controller.Battle.ReservedCost(1));
            Click(panel.SkillButtons[0]); Click(panel.ApplyButton);
            Click(panel.UnitButtons[1]); Click(panel.SkillButtons[0]); Click(panel.TargetButtons[7]); Click(panel.ApplyButton);
            yield return Capture(view, "main-skill-planning.png");
            Click(panel.CloseButton);
            Click(view.StepButton); Click(view.StepButton); Click(view.StepButton);
            Assert.AreEqual("A", controller.Battle.ActiveUnitId);
            Assert.AreEqual(70, controller.Battle.Unit("X").CurrentHp);
            Assert.AreEqual(4, controller.Battle.Unit("A").SkillBp("shot"));
            Assert.AreEqual(0, controller.Battle.Unit("A").RemainingMainActions);
            yield return Capture(view, "main-skill-hit.png");
            controller.StepDelay = 0; Click(view.SkipButton);
            for (int i = 0; i < 64 && controller.IsAdvancing; i++) yield return null;
            Assert.AreEqual(2, controller.Battle.TurnNumber); Assert.AreEqual(70, controller.Battle.Unit("Y").CurrentHp);
            Assert.AreEqual(2, controller.Battle.Events.Count(e => e.Kind == TurnEventKind.SkillHit));
            Click(view.ResetButton);
            Assert.AreEqual(100, controller.Battle.Unit("X").CurrentHp); Assert.AreEqual(5, controller.Battle.Unit("A").SkillBp("shot"));
            Assert.IsNull(controller.Battle.MainReservation("A"));
        }

        /// <summary>
        /// 실제 9슬롯·재사용 버튼·캐릭터·COST 바를 확인하고 예약 토글부터 다음 턴과 초기화까지 검증한다.
        /// </summary>
        [UnityTest]
        public IEnumerator RetainedSlotsAndButtonsDriveTheSharedTurnBattle()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Stage/Battle/BattleType/Stage_Battle.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Requires the editor scene asset."); yield break;
#endif
            yield return null;
            // view: 실제 Stage_Battle 씬에 연결된 새 턴 화면.
            var view = Object.FindAnyObjectByType<StageBattleView>();
            Assert.IsNotNull(view);
            // controller: 로드된 씬에서 실제로 동작하는 턴 진행 컨트롤러.
            var controller = view.GetComponent<TurnSandboxController>();
            Assert.AreSame(view, controller.View);
            Assert.IsNull(controller.GetComponent<TurnSandboxView>(), "Do not draw the sandbox over the retained scene.");
            Assert.AreEqual(9, view.Slots.Count);
            Assert.AreEqual("UnitSlot_0", view.Slots[0].name);
            Assert.AreEqual("TurnEndButton", view.SkipButton.name);
            Assert.AreEqual("ActionPointSkipNutton", view.StepButton.name);
            Assert.AreEqual("TURN 01", view.TurnText.text);
            Assert.AreEqual(.3f, view.CostFill.fillAmount, .001f);
            Assert.IsNotNull(view.Slots[0].Find("SlotGroundSprite").GetComponent<SpriteRenderer>().sprite);
            Assert.AreEqual(4, view.Slots.SelectMany(t => t.GetComponentsInChildren<SpriteRenderer>()).Count(r => r.name == "UnitCharactorSprite" && r.sprite != null));
            yield return Capture(view, "stage-battle-turn-01.png");
            Click(view.SlotButtons[0]);
            Assert.IsTrue(controller.Battle.HasEndTurnReservation("A"));
            Click(view.SlotButtons[0]);
            Assert.IsFalse(controller.Battle.HasEndTurnReservation("A"));
            Click(view.StepButton);
            Click(view.StepButton);
            Assert.AreEqual("A", controller.Battle.ActiveUnitId);
            StringAssert.Contains("아군 A", view.VisibleActor);
            controller.StepDelay = 0;
            Click(view.SkipButton);
            controller.SkipTurn();
            Assert.IsFalse(view.SkipButton.interactable);
            for (int i = 0; i < 64 && controller.IsAdvancing; i++) yield return null;
            Assert.IsFalse(controller.IsAdvancing);
            Assert.AreEqual(2, controller.Battle.TurnNumber);
            Assert.AreEqual(TurnPhase.Planning, controller.Battle.Phase);
            Assert.AreEqual("TURN 02", view.TurnText.text);
            Assert.AreEqual(.5f, view.CostFill.fillAmount, .001f);
            Assert.AreEqual(4, controller.Battle.Events.Count(e => e.Kind == TurnEventKind.Finished));
            yield return Capture(view, "stage-battle-turn-02.png");
            controller.StepDelay = .1f;
            Click(view.SkipButton); Click(view.ResetButton);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.AreEqual(1, controller.Battle.TurnNumber);
            Assert.IsTrue(view.SkipButton.interactable);
        }

        /// <summary>
        /// 실제 씬을 임시 RenderTexture에 렌더링해 Logs에 PNG로 저장한다. 그래픽 장치가 없는 실행은 건너뛰며 변경한 Canvas/카메라 설정을 복구한다.
        /// </summary>
        /// <param name="view">촬영할 Stage_Battle 화면.</param>
        /// <param name="filename">Logs 폴더에 저장할 PNG 파일 이름.</param>
        internal static IEnumerator Capture(StageBattleView view, string filename)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) yield break;
            // canvas: 촬영 대상 UI를 포함하는 Canvas.
            var canvas = view.SkipButton.GetComponentInParent<Canvas>();
            // scaler: 해상도에 따른 UI 크기를 조절하는 CanvasScaler.
            var scaler = canvas.GetComponent<CanvasScaler>();
            // camera: 씬을 렌더링할 카메라.
            var camera = Camera.main;
            // target: 실제 씬과 UI를 지정 해상도로 촬영하는 임시 RenderTexture.
            var target = new RenderTexture(1280, 720, 24);
            // texture: 렌더링 결과를 읽어 PNG로 변환할 CPU 측 Texture2D.
            var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            // previous: 캡처 종료 후 복구할 기존 활성 RenderTexture.
            var previous = RenderTexture.active;
            // skillCanvas: 별도 오버레이인 스킬 창도 같은 이미지에 렌더링한다.
            var skillCanvas = view.GetComponent<TurnSandboxController>().SkillPanel.OpenButton.GetComponentInParent<Canvas>();
            var skillScaler = skillCanvas.GetComponent<CanvasScaler>();
            // moveCanvas/moveScaler: 필드 위 이동 예약 창도 캡처한다.
            var moveCanvas = view.GetComponent<TurnSandboxController>().MovePanel.OpenButton.GetComponentInParent<Canvas>();
            var moveScaler = moveCanvas.GetComponent<CanvasScaler>();
            try
            {
                moveCanvas.renderMode = RenderMode.ScreenSpaceCamera; moveCanvas.worldCamera = camera; moveCanvas.planeDistance = .7f;
                moveScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; moveScaler.scaleFactor = 1; moveCanvas.scaleFactor = 1;
                skillCanvas.renderMode = RenderMode.ScreenSpaceCamera; skillCanvas.worldCamera = camera; skillCanvas.planeDistance = .5f;
                skillScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; skillScaler.scaleFactor = 1; skillCanvas.scaleFactor = 1;
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; scaler.scaleFactor = 1; canvas.scaleFactor = 1;
                camera.targetTexture = target;
                yield return null; yield return null;

                // 작은 Game 뷰에서 만든 글자 메시를 늘리지 않고 캡처 해상도에 맞춰 다시 생성한다.
                foreach (var label in canvas.GetComponentsInChildren<Text>()) label.SetAllDirty();
                foreach (var label in skillCanvas.GetComponentsInChildren<Text>()) label.SetAllDirty();
                foreach (var label in moveCanvas.GetComponentsInChildren<Text>()) label.SetAllDirty();
                Canvas.ForceUpdateCanvases();
                // TMP 카드도 캡처 해상도에 맞춰 메시를 갱신한다.
                foreach (var label in skillCanvas.GetComponentsInChildren<TMPro.TMP_Text>()) label.ForceMeshUpdate();
                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); texture.Apply();
                Directory.CreateDirectory("Logs"); File.WriteAllBytes("Logs/" + filename, texture.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous; camera.targetTexture = null;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null;
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                skillCanvas.renderMode = RenderMode.ScreenSpaceOverlay; skillCanvas.worldCamera = null;
                skillScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                moveCanvas.renderMode = RenderMode.ScreenSpaceOverlay; moveCanvas.worldCamera = null;
                moveScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                Object.Destroy(target); Object.Destroy(texture);
            }
            yield return null;
        }

        /// <summary>
        /// 버튼 중앙에 실제 UI raycast를 수행해 가림이 없는지 확인한 뒤 왼쪽 클릭 이벤트를 전달한다.
        /// </summary>
        /// <param name="button">클릭하거나 새 HUD에 다시 연결할 버튼.</param>
        internal static void Click(Button button)
        {
            Canvas.ForceUpdateCanvases();
            // rect: UI 위치·크기 또는 버튼 중앙 좌표 계산에 사용하는 RectTransform.
            var rect = (RectTransform)button.transform;
            // pointer: 목표 버튼 중앙을 왼쪽 클릭하는 테스트용 포인터 이벤트.
            var pointer = new PointerEventData(EventSystem.current) {
                position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center)),
                button = PointerEventData.InputButton.Left
            };
            // hits: 포인터 위치의 UI raycast 결과. 첫 항목이 목표 버튼인지 확인한다.
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
            Assert.IsNotEmpty(hits, "No UI raycast hit: " + button.name + " at " + pointer.position + " screen " + Screen.width + "x" + Screen.height);
            Assert.AreEqual(button.gameObject, hits[0].gameObject, "A retained UI resource blocks this button.");
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }
    }
}
