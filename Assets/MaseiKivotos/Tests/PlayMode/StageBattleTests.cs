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
        private static IEnumerator Capture(StageBattleView view, string filename)
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
            try
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; scaler.scaleFactor = 1; canvas.scaleFactor = 1;
                camera.targetTexture = target;
                yield return null; yield return null;

                // 작은 Game 뷰에서 만든 글자 메시를 늘리지 않고 캡처 해상도에 맞춰 다시 생성한다.
                foreach (var label in canvas.GetComponentsInChildren<Text>()) label.SetAllDirty();
                Canvas.ForceUpdateCanvases(); camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); texture.Apply();
                Directory.CreateDirectory("Logs"); File.WriteAllBytes("Logs/" + filename, texture.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous; camera.targetTexture = null;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null;
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                Object.Destroy(target); Object.Destroy(texture);
            }
            yield return null;
        }

        /// <summary>
        /// 버튼 중앙에 실제 UI raycast를 수행해 가림이 없는지 확인한 뒤 왼쪽 클릭 이벤트를 전달한다.
        /// </summary>
        /// <param name="button">클릭하거나 새 HUD에 다시 연결할 버튼.</param>
        private static void Click(Button button)
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
            Assert.IsNotEmpty(hits);
            Assert.AreEqual(button.gameObject, hits[0].gameObject, "A retained UI resource blocks this button.");
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }
    }
}
