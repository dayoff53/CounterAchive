using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MaseiKivotos.Core;
using MaseiKivotos.Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace MaseiKivotos.Tests
{
    /// <summary>
    /// 저장된 독립 검증 씬을 로드해 실제 UI 클릭과 턴 진행을 확인하는 PlayMode 테스트.
    /// </summary>
    public sealed class TurnSandboxTests
    {
        /// <summary>
        /// 검증 씬 버튼으로 차례와 다음 턴을 진행하고 COST 증가·중복 입력 방지·처리 중 초기화를 검증한다.
        /// </summary>
        [UnityTest]
        public IEnumerator SceneButtonsAdvanceRealTurnAndResetCancelsPendingWork()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/MaseiKivotos/Scenes/TurnSandbox.unity", new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("This smoke test loads the editor sandbox asset.");
            yield break;
#endif
            yield return null;
            // controller: 로드된 씬에서 실제로 동작하는 턴 진행 컨트롤러.
            var controller = Object.FindAnyObjectByType<TurnSandboxController>();
            Assert.IsNotNull(controller);
            Assert.AreEqual("TURN 01", controller.View.TurnText.text);
            Assert.AreEqual(3, controller.Battle.Costs[1]);
            Assert.AreEqual(TurnPhase.Planning, controller.Battle.Phase);
            yield return Capture(controller, "makibo-turn-01.png");

            Click(controller.View.StepButton);
            Assert.AreEqual(TurnPhase.Execution, controller.Battle.Phase);
            Click(controller.View.StepButton);
            Assert.AreEqual("A", controller.Battle.ActiveUnitId);
            StringAssert.Contains("아군 A", controller.View.VisibleActor);
            Assert.AreEqual(1, controller.Battle.TurnNumber);

            controller.StepDelay = 0;
            Click(controller.View.SkipButton);
            Assert.IsTrue(controller.IsAdvancing);
            Assert.IsFalse(controller.View.SkipButton.interactable);
            controller.SkipTurn(); // UI를 거치지 않는 직접 호출도 중복 진행을 막아야 한다.
            for (int i = 0; i < 64 && controller.IsAdvancing; i++) yield return null;
            Assert.IsFalse(controller.IsAdvancing);
            Assert.AreEqual(2, controller.Battle.TurnNumber);
            Assert.AreEqual("TURN 02", controller.View.TurnText.text);
            Assert.AreEqual(5, controller.Battle.Costs[1]);
            Assert.AreEqual(TurnPhase.Planning, controller.Battle.Phase);
            Assert.AreEqual(4, controller.Battle.Events.Count(e => e.Kind == TurnEventKind.Finished));
            yield return Capture(controller, "makibo-turn-02.png");

            controller.StepDelay = 0.1f;
            Click(controller.View.SkipButton);
            Click(controller.View.ResetButton);
            yield return new WaitForSecondsRealtime(0.3f);
            Assert.AreEqual(1, controller.Battle.TurnNumber);
            Assert.IsFalse(controller.IsAdvancing);
            Assert.AreEqual(3, controller.Battle.Costs[1]);
            Assert.IsTrue(controller.View.SkipButton.interactable);
        }

        /// <summary>
        /// 버튼 중앙에 실제 UI raycast를 수행해 가림이 없는지 확인한 뒤 왼쪽 클릭 이벤트를 전달한다.
        /// </summary>
        /// <param name="button">클릭하거나 새 HUD에 다시 연결할 버튼.</param>
        private static void Click(Button button)
        {
            Canvas.ForceUpdateCanvases();
            // pointer: 목표 버튼 중앙을 왼쪽 클릭하는 테스트용 포인터 이벤트.
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(null, button.transform.TransformPoint(((RectTransform)button.transform).rect.center)),
                button = PointerEventData.InputButton.Left
            };
            // hits: 포인터 위치의 UI raycast 결과. 첫 항목이 목표 버튼인지 확인한다.
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.IsNotEmpty(hits, "The button must be reachable through the canvas raycaster.");
            Assert.AreEqual(button.gameObject, hits[0].gameObject, "Another UI element blocks the button.");
            ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        }

        /// <summary>
        /// 실제 씬을 임시 RenderTexture에 렌더링해 Logs에 PNG로 저장한다. 그래픽 장치가 없는 실행은 건너뛰며 변경한 Canvas/카메라 설정을 복구한다.
        /// </summary>
        /// <param name="controller">전투 상태와 사용자 조작을 제공하는 공용 컨트롤러.</param>
        /// <param name="filename">Logs 폴더에 저장할 PNG 파일 이름.</param>
        private static IEnumerator Capture(TurnSandboxController controller, string filename)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) yield break;
            // canvas: 촬영 대상 UI를 포함하는 Canvas.
            var canvas = controller.GetComponentInChildren<Canvas>();
            // scaler: 해상도에 따른 UI 크기를 조절하는 CanvasScaler.
            var scaler = canvas.GetComponent<CanvasScaler>();
            // camera: 씬을 렌더링할 카메라.
            var camera = Camera.main;
            // target: 실제 씬과 UI를 지정 해상도로 촬영하는 임시 RenderTexture.
            var target = new RenderTexture(1280, 800, 24);
            // texture: 렌더링 결과를 읽어 PNG로 변환할 CPU 측 Texture2D.
            var texture = new Texture2D(1280, 800, TextureFormat.RGB24, false);
            // previous: 캡처 종료 후 복구할 기존 활성 RenderTexture.
            var previous = RenderTexture.active;
            // previousScale: 캡처용 배율로 바꾸기 전에 보관하는 Canvas 배율.
            float previousScale = canvas.scaleFactor;
            try
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; scaler.scaleFactor = 1;
                canvas.scaleFactor = 1;
                camera.targetTexture = target;

                // CanvasScaler와 폰트 메시가 새 캡처 영역으로 갱신될 때까지 한 프레임 기다린다.
                yield return null;
                Canvas.ForceUpdateCanvases(); camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, 1280, 800), 0, 0); texture.Apply();
                // folder: 캡처 PNG를 저장할 프로젝트 Logs 폴더의 절대 경로.
                var folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs"));
                Directory.CreateDirectory(folder);
                File.WriteAllBytes(Path.Combine(folder, filename), texture.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous; camera.targetTexture = null;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null;
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvas.scaleFactor = previousScale;
                Object.Destroy(target); Object.Destroy(texture);
                Canvas.ForceUpdateCanvases();
            }
        }
    }
}
