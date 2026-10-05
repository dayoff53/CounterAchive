using System;
using System.Collections.Generic;
using System.Linq;
using MaseiKivotos.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MaseiKivotos.Unity
{
    /// <summary>필드를 보면서 아군과 목적지를 선택하는 서브 이동 예약 창.</summary>
    public sealed class MovementPanel : MonoBehaviour
    {
        /// <summary>예약 명령과 전투 상태를 제공하는 진행자.</summary>
        private TurnSandboxController owner;
        /// <summary>기존 화면에서 빌려 쓰는 한국어 폰트.</summary>
        private Font font;
        /// <summary>화면 하단에만 표시하는 선택 영역.</summary>
        private GameObject drawer;
        /// <summary>현재 선택 아군 ID.</summary>
        private string selectedUnit;
        /// <summary>선택 유닛 상태, 예약 결과와 안내 문구.</summary>
        private Text status, feedback, plans;
        /// <summary>출전 아군 선택 버튼.</summary>
        private readonly List<Button> unitButtons = new List<Button>();
        /// <summary>1~9 목적지 선택 버튼.</summary>
        private readonly List<Button> targetButtons = new List<Button>();
        /// <summary>이동 예약 창을 연다.</summary>
        public Button OpenButton { get; private set; }
        /// <summary>적용한 예약을 유지하고 창만 닫는다.</summary>
        public Button CloseButton { get; private set; }
        /// <summary>선택 아군의 이동 예약만 비운다.</summary>
        public Button ClearButton { get; private set; }
        /// <summary>입력 검증용 아군 버튼 목록.</summary>
        public IReadOnlyList<Button> UnitButtons => unitButtons;
        /// <summary>입력 검증용 목적지 버튼 목록.</summary>
        public IReadOnlyList<Button> TargetButtons => targetButtons;

        /// <summary>배경 필드가 보이는 하단 창과 상단 진입 버튼을 만든다.</summary>
        /// <param name="controller">공용 진행자.</param>
        public void Build(TurnSandboxController controller)
        {
            owner = controller; font = owner.View.TurnText.font;
            // root/scaler: 기존 HUD보다 위에 표시하는 독립 Canvas.
            var root = new GameObject("Movement Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            root.GetComponent<Canvas>().sortingOrder = 90;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            OpenButton = ButtonAt(root.transform, "Open Movement", "서브 이동", 920, 20, 150, 44, Open);
            drawer = new GameObject("Movement Planning", typeof(RectTransform), typeof(Image));
            Place(drawer, root.transform, 20, 472, 1240, 226);
            drawer.GetComponent<Image>().color = new Color32(14, 22, 36, 255);
            Label(drawer.transform, "Title", "이동 예약 · 아군 선택 → 도착할 칸 선택", 18, 0, 1000, 32, 21);
            CloseButton = ButtonAt(drawer.transform, "Close Movement", "닫기", 1110, 4, 110, 32, Close);
            for (int i = 0; i < 3; i++)
            {
                // index: 콜백에서 사용할 아군 순서.
                int index = i;
                unitButtons.Add(ButtonAt(drawer.transform, "Mover " + i, "", 18 + i * 222, 39, 210, 34, () => SelectUnit(index)));
            }
            status = Label(drawer.transform, "Status", "", 700, 35, 520, 42, 16);
            for (int i = 0; i < 9; i++)
            {
                // slot: 각 버튼의 목적지 번호.
                int slot = i + 1;
                targetButtons.Add(ButtonAt(drawer.transform, "Destination " + slot, "", 18 + i * 134, 80, 122, 44, () => Reserve(slot)));
            }
            feedback = Label(drawer.transform, "Feedback", "", 18, 128, 815, 46, 16);
            ClearButton = ButtonAt(drawer.transform, "Clear Movement", "선택 아군 이동 모두 취소", 864, 133, 352, 38, Clear);
            plans = Label(drawer.transform, "Plans", "", 18, 175, 1200, 44, 15);
            Close();
        }

        /// <summary>기본 아군을 선택하고 창을 연다. 메인 선택 창은 닫는다.</summary>
        public void Open()
        {
            if (owner.IsAdvancing || owner.Battle.Phase != TurnPhase.Planning) return;
            owner.SkillPanel.Close(); drawer.SetActive(true); SelectUnit(0);
        }
        /// <summary>창과 선택값을 지운다. 이미 저장한 이동은 유지한다.</summary>
        public void Close() { if (drawer != null) drawer.SetActive(false); selectedUnit = null; }
        /// <summary>아군 버튼 순서를 실제 좌표로 고정한다. 미리보기로 선택 순서가 바뀌지 않는다.</summary>
        private BattleUnit[] Allies() => owner.Battle.Units.Where(u => u.Location == UnitLocation.Field && u.FactionId == owner.Battle.PlayerFactionId).OrderBy(u => u.Slot).ToArray();
        /// <summary>예약할 아군을 변경한다.</summary>
        /// <param name="index">필드 아군 순서.</param>
        private void SelectUnit(int index)
        {
            // allies: 현재 선택 가능한 아군 목록.
            var allies = Allies(); if (index >= allies.Length) return;
            selectedUnit = allies[index].Id; feedback.text = "도착할 칸을 누르면 이동이 추가됩니다. 파란 캐릭터는 예상 배치입니다."; Refresh();
        }
        /// <summary>목적지를 추가하고 결과를 안내한다.</summary>
        /// <param name="slot">목적지 번호.</param>
        private void Reserve(int slot)
        {
            if (selectedUnit == null) return;
            try { owner.ReserveMove(selectedUnit, slot); feedback.text = slot + "번으로 이동 예약 · 확정하면 원래 위치부터 차례대로 실행합니다."; }
            catch (InvalidOperationException exception) { feedback.text = exception.Message; }
        }
        /// <summary>선택 아군의 이동만 취소한다.</summary>
        private void Clear()
        {
            if (selectedUnit == null) return;
            owner.ClearMoves(selectedUnit); feedback.text = "이동을 취소했습니다. 메인 스킬과 턴 종료 예약은 유지합니다.";
        }
        /// <summary>가능한 목적지, 남은 서브와 이동 불가 상태를 반영한다.</summary>
        public void Refresh()
        {
            if (owner.Battle == null) return;
            // battle/canPlan: 현재 전투와 예약 편집 가능 여부.
            var battle = owner.Battle;
            bool canPlan = battle.Phase == TurnPhase.Planning && !owner.IsAdvancing;
            OpenButton.interactable = canPlan;
            if (!canPlan) { Close(); return; }
            if (!drawer.activeSelf) return;
            // allies/actor/moves: 선택 대상과 이미 추가한 이동 목록.
            var allies = Allies(); var actor = allies.FirstOrDefault(u => u.Id == selectedUnit);
            for (int i = 0; i < unitButtons.Count; i++)
            {
                unitButtons[i].gameObject.SetActive(i < allies.Length);
                if (i < allies.Length) Caption(unitButtons[i], (allies[i] == actor ? "▶ " : "") + allies[i].Definition.Name);
            }
            if (actor == null) return;
            var moves = battle.MovementReservations(actor.Id);
            status.text = "이동 거리 " + actor.Definition.MoveRange + " · 서브 예약 " + moves.Count + "/" + actor.RemainingSubActions
                + "\n이동 불가 " + actor.MovementLockTurns + "회" + (actor.IsMovementBlocked ? " · 외부 이동 금지" : "");
            for (int i = 0; i < targetButtons.Count; i++)
            {
                // error: 예상 시작점 기준의 거리·적 점유·잔여 횟수 검사 결과.
                string error = battle.MovementReservationError(actor.Id, i + 1);
                targetButtons[i].interactable = error == null;
                Caption(targetButtons[i], (i + 1) + "번" + (error == null ? " · 이동" : ""));
            }
            ClearButton.interactable = moves.Count > 0;
            plans.text = "이동 예약: " + (moves.Count == 0 ? "없음" : string.Join(" → ", moves.Select(m => m.Destination + "번")))
                + "  |  이동 → 메인 → 종료\n아군 이동만 예상 · 적 예약/피해 미반영 · 이동 금지 중 재시도는 불가 횟수를 갱신하지 않음";
        }
        /// <summary>버튼 문구를 교체한다.</summary>
        /// <param name="button">버튼.</param>
        /// <param name="text">문구.</param>
        private static void Caption(Button button, string text) { button.GetComponentInChildren<Text>().text = text; }
        /// <summary>좌상단 기준 텍스트를 만든다.</summary>
        /// <param name="parent">부모.</param>
        /// <param name="name">오브젝트명.</param>
        /// <param name="text">문구.</param>
        /// <param name="x">왼쪽 거리.</param>
        /// <param name="y">위쪽 거리.</param>
        /// <param name="width">너비.</param>
        /// <param name="height">높이.</param>
        /// <param name="size">글자 크기.</param>
        private Text Label(Transform parent, string name, string text, float x, float y, float width, float height, int size)
        {
            // go/label: 표시 오브젝트와 글자 속성.
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); Place(go, parent, x, y, width, height);
            var label = go.GetComponent<Text>(); label.font = font; label.fontSize = size; label.text = text;
            label.color = Color.white; label.raycastTarget = false; label.alignment = TextAnchor.MiddleLeft; return label;
        }
        /// <summary>버튼과 가운데 캡션을 만든다.</summary>
        /// <param name="parent">부모.</param>
        /// <param name="name">오브젝트명.</param>
        /// <param name="text">문구.</param>
        /// <param name="x">왼쪽 거리.</param>
        /// <param name="y">위쪽 거리.</param>
        /// <param name="width">너비.</param>
        /// <param name="height">높이.</param>
        /// <param name="action">클릭 명령.</param>
        private Button ButtonAt(Transform parent, string name, string text, float x, float y, float width, float height, Action action)
        {
            // go/button/label: 클릭 영역과 버튼, 안내 글자.
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); Place(go, parent, x, y, width, height);
            go.GetComponent<Image>().color = new Color32(37, 70, 98, 255);
            var button = go.GetComponent<Button>(); button.targetGraphic = go.GetComponent<Image>(); button.onClick.AddListener(() => action());
            var label = Label(go.transform, "Caption", text, 6, 2, width - 12, height - 4, 16); label.alignment = TextAnchor.MiddleCenter; return button;
        }
        /// <summary>좌상단을 기준으로 UI 위치와 크기를 정한다.</summary>
        /// <param name="go">대상.</param>
        /// <param name="parent">부모.</param>
        /// <param name="x">오른쪽 거리.</param>
        /// <param name="y">아래쪽 거리.</param>
        /// <param name="width">너비.</param>
        /// <param name="height">높이.</param>
        private static void Place(GameObject go, Transform parent, float x, float y, float width, float height)
        {
            go.transform.SetParent(parent, false);
            // rect: 고정 기준점과 표시 영역.
            var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height);
        }
    }
}
