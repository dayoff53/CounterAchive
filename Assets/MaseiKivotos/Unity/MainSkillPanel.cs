using System;
using System.Collections.Generic;
using System.Linq;
using MaseiKivotos.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MaseiKivotos.Unity
{
    /// <summary>기존 두 턴 화면에 공통으로 덧붙이는 메인 스킬 예약 창. 규칙 계산은 Core에 위임한다.</summary>
    public sealed class MainSkillPanel : MonoBehaviour
    {
        /// <summary>실제 전투와 예약 명령을 제공하는 컨트롤러.</summary>
        private TurnSandboxController owner;
        /// <summary>기존 화면에서 빌려 쓰는 한국어 폰트. 이 컴포넌트가 해제하지 않는다.</summary>
        private Font font;
        /// <summary>독립 정렬 순서로 조작 창을 표시하는 Canvas 루트.</summary>
        private Transform canvasRoot;
        /// <summary>열려 있을 때 배경 입력을 막는 전체 화면 패널.</summary>
        private GameObject popup;
        /// <summary>현재 선택한 사용자와 원래 스킬, 대상의 ID. 확정 전 편집용이며 예약 자체가 아니다.</summary>
        private string selectedUnit, selectedSkill, selectedTarget;
        /// <summary>선택값·자원·확보 COST를 설명하는 텍스트.</summary>
        private Text info;
        /// <summary>예약 성공 또는 거부 이유를 표시하는 텍스트.</summary>
        private Text feedback;
        /// <summary>선택 가능한 아군 버튼 목록.</summary>
        private readonly List<Button> unitButtons = new List<Button>();
        /// <summary>최대 네 개의 소유 스킬 버튼.</summary>
        private readonly List<Button> skillButtons = new List<Button>();
        /// <summary>기존 프리팹으로 생성한 스킬 데이터 카드.</summary>
        private readonly List<SkillSlotView> skillSlots = new List<SkillSlotView>();
        /// <summary>현재 사용자 데이터가 연결된 카드 목록.</summary>
        public IReadOnlyList<SkillSlotView> SkillSlots => skillSlots;
        /// <summary>1~9 슬롯에 대응하는 대상 버튼. 빈칸·아군은 비활성화한다.</summary>
        private readonly List<Button> targetButtons = new List<Button>();
        /// <summary>메인 스킬 선택 창을 여는 버튼.</summary>
        public Button OpenButton { get; private set; }
        /// <summary>편집 선택값을 실제 예약에 반영하는 버튼.</summary>
        public Button ApplyButton { get; private set; }
        /// <summary>선택 유닛의 스킬을 지우고 종료만 예약하는 버튼.</summary>
        public Button EndOnlyButton { get; private set; }
        /// <summary>창을 닫는 버튼. 닫아도 이미 적용한 예약은 유지한다.</summary>
        public Button CloseButton { get; private set; }
        /// <summary>UI 클릭 검증용 읽기 전용 아군 버튼 목록.</summary>
        public IReadOnlyList<Button> UnitButtons => unitButtons;
        /// <summary>UI 클릭 검증용 읽기 전용 스킬 버튼 목록.</summary>
        public IReadOnlyList<Button> SkillButtons => skillButtons;
        /// <summary>UI 클릭 검증용 읽기 전용 대상 버튼 목록.</summary>
        public IReadOnlyList<Button> TargetButtons => targetButtons;
        /// <summary>현재 창에 표시된 선택/자원 안내.</summary>
        public string VisibleInfo => info.text;

        /// <summary>기존 화면의 폰트를 이용해 공용 예약 UI를 한 번 구성한다.</summary>
        /// <param name="controller">공용 진행 컨트롤러.</param>
        public void Build(TurnSandboxController controller)
        {
            if (canvasRoot != null) return;
            owner = controller; font = owner.View.TurnText.font;
            // root/scaler: 기존 화면과 독립적인 오버레이. 해상도가 달라도 조작 창을 화면 안에 유지한다.
            var root = new GameObject("Main Skill Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false); canvasRoot = root.transform;
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            root.GetComponent<Canvas>().sortingOrder = 100;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            OpenButton = ButtonAt(canvasRoot, "Main Skill", "메인 스킬", 1100, 20, 150, 44, Open);
            // popupRect: 모든 배경 입력을 막도록 Canvas 전체를 덮는다.
            popup = new GameObject("Skill Planning", typeof(RectTransform), typeof(Image));
            popup.transform.SetParent(canvasRoot, false);
            var popupRect = (RectTransform)popup.transform;
            popupRect.anchorMin = Vector2.zero; popupRect.anchorMax = Vector2.one; popupRect.offsetMin = popupRect.offsetMax = Vector2.zero;
            popup.GetComponent<Image>().color = new Color32(14, 22, 36, 250);
            Label(popup.transform, "Title", "메인 스킬 예약  ·  사용자 → 스킬 → 대상 → 예약 적용", 40, 14, 1050, 32, 25);
            CloseButton = ButtonAt(popup.transform, "Close", "닫기", 1130, 14, 110, 36, Close);
            Label(popup.transform, "Unit Caption", "1. 사용할 아군", 40, 45, 1100, 24, 16);
            for (int i = 0; i < 3; i++)
            {
                // index: 콜백이 고유한 아군 순서를 기억하도록 복사한다.
                int index = i;
                unitButtons.Add(ButtonAt(popup.transform, "Caster " + i, "", 40 + i * 400, 72, 380, 42, () => SelectUnit(index)));
            }
            Label(popup.transform, "Skill Caption", "2. 스킬 선택  ·  카드의 숫자는 현재/최대 BP  ·  수치는 임시 데이터", 40, 120, 1160, 26, 17);
            // skillPrefab: 사용자가 구성한 Resources 프리팹 원본. 선택 카드마다 복제한다.
            var skillPrefab = Resources.Load<GameObject>(SkillSlotView.PrefabPath);
            for (int i = 0; i < 4; i++)
            {
                // index: 각 버튼이 선택할 소유 스킬 위치.
                int index = i;
                // slot: 원본의 이름/설명/BP/사거리/범위/이미지/타입/속성 영역을 재사용한다.
                var slot = SkillSlotView.Create(skillPrefab, popup.transform, () => SelectSkill(index));
                Place(slot.gameObject, popup.transform, 40 + (i % 2) * 610, 150 + (i / 2) * 152, 590, 140);
                skillSlots.Add(slot); skillButtons.Add(slot.Button);
            }
            Label(popup.transform, "Target Caption", "3. 대상 적군  ·  예약 후에도 유닛을 추적하며 실행 직전 다시 판정", 40, 450, 1160, 24, 17);
            for (int i = 0; i < 9; i++)
            {
                // slot: 화면의 대상 버튼에 대응하는 규칙 슬롯 번호.
                int slot = i + 1;
                targetButtons.Add(ButtonAt(popup.transform, "Target " + slot, "", 40 + i * 134, 482, 122, 60, () => SelectTarget(slot)));
                targetButtons[i].GetComponentInChildren<Text>().fontSize = 15;
            }
            info = Label(popup.transform, "Plan Info", "", 40, 550, 1190, 66, 16);
            feedback = Label(popup.transform, "Feedback", "", 40, 618, 1190, 24, 15);
            ApplyButton = ButtonAt(popup.transform, "Apply Skill", "예약 적용", 40, 653, 280, 44, Apply);
            EndOnlyButton = ButtonAt(popup.transform, "End Only", "모든 행동 취소 · 종료만 예약", 340, 653, 360, 44, EndOnly);
            Label(popup.transform, "Hint", "닫은 뒤 예약 확정 / 턴 넘기기로 실행합니다.", 730, 653, 500, 44, 17);
            Close();
        }

        /// <summary>예약 단계에서 아군 첫 유닛을 기본 선택해 창을 연다.</summary>
        public void Open()
        {
            if (owner.IsAdvancing || owner.Battle.Phase != TurnPhase.Planning) return;
            owner.MovePanel.Close();
            popup.SetActive(true); feedback.text = ""; SelectUnit(0);
        }

        /// <summary>임시 선택을 지우고 창을 숨긴다. Core 예약은 유지한다.</summary>
        public void Close()
        {
            if (popup != null) popup.SetActive(false);
            selectedUnit = selectedSkill = selectedTarget = null;
        }

        /// <summary>현재 출전 아군을 슬롯 순서대로 가져온다.</summary>
        private BattleUnit[] Allies() => owner.Battle.Units.Where(u => u.Location == UnitLocation.Field && u.FactionId == owner.Battle.PlayerFactionId).OrderBy(u => u.Slot).ToArray();

        /// <summary>사용자 변경 시 기존 예약을 편집 시작값으로 불러온다.</summary>
        /// <param name="index">출전 아군 배열 위치.</param>
        private void SelectUnit(int index)
        {
            // allies/actor/plan: 현재 필드 아군과 그 유닛의 기존 예약.
            var allies = Allies(); if (index >= allies.Length) return;
            var actor = allies[index]; var plan = owner.Battle.MainReservation(actor.Id);
            selectedUnit = actor.Id; selectedSkill = plan?.SkillId ?? actor.Definition.Skills.FirstOrDefault()?.Id;
            selectedTarget = plan?.TargetId; feedback.text = ""; Refresh();
        }

        /// <summary>선택한 유닛의 소유 스킬을 편집값으로 지정한다.</summary>
        /// <param name="index">소유 스킬 배열 위치.</param>
        private void SelectSkill(int index)
        {
            if (selectedUnit == null) return;
            // skills: 선택한 유닛의 소유 공격 정의.
            var skills = owner.Battle.Unit(selectedUnit).Definition.Skills;
            if (index >= skills.Count) return;
            selectedSkill = skills[index].Id; feedback.text = ""; Refresh();
        }

        /// <summary>클릭한 슬롯의 현재 적 유닛을 지정 대상으로 저장한다.</summary>
        /// <param name="slot">1~9 슬롯 번호.</param>
        private void SelectTarget(int slot)
        {
            // target: 클릭 시점에 해당 슬롯에 남아 있는 유닛.
            var target = owner.Battle.Units.FirstOrDefault(u => u.Location == UnitLocation.Field && u.Slot == slot);
            if (target == null || target.FactionId == owner.Battle.PlayerFactionId) return;
            selectedTarget = target.Id; feedback.text = ""; Refresh();
        }

        /// <summary>선택값을 검증하고 적용한다. 자원 부족 등 거부 사유는 창에 남긴다.</summary>
        private void Apply()
        {
            if (selectedUnit == null || selectedSkill == null || selectedTarget == null) return;
            try { owner.ReserveSkill(selectedUnit, selectedSkill, selectedTarget); feedback.text = "스킬 → 턴 종료 예약을 적용했습니다. 다른 아군도 선택할 수 있습니다."; }
            catch (InvalidOperationException exception) { feedback.text = exception.Message; }
        }

        /// <summary>선택 유닛을 스킬 없는 종료 예약으로 바꾼다.</summary>
        private void EndOnly()
        {
            if (selectedUnit == null) return;
            owner.ReserveOnlyEndTurn(selectedUnit); feedback.text = "이동·스킬과 확보 COST를 취소하고 턴 종료만 예약했습니다.";
        }

        /// <summary>BP·HP·공유 자원과 예약 표시를 현재 상태에 맞춘다.</summary>
        public void Refresh()
        {
            if (owner.Battle == null) return;
            // battle/canPlan: 이번 전투와 현재 편집 허용 여부.
            var battle = owner.Battle;
            bool canPlan = battle.Phase == TurnPhase.Planning && !owner.IsAdvancing;
            OpenButton.interactable = canPlan;
            if (!canPlan) { Close(); return; }
            if (!popup.activeSelf) return;
            // allies/actor/skill: 현재 버튼과 설명을 구성할 출전 아군·선택 사용자·스킬.
            var allies = Allies();
            var actor = allies.FirstOrDefault(u => u.Id == selectedUnit);
            var skill = actor?.Definition.Skills.FirstOrDefault(s => s.Id == selectedSkill);
            // zeroBp: 현재 BP가 0이면 실행 시 대체 가능성을 표시하고 원래 사거리로 선택을 제한하지 않는다.
            bool zeroBp = actor != null && skill != null && actor.SkillBp(skill.Id) == 0;
            for (int i = 0; i < unitButtons.Count; i++)
            {
                unitButtons[i].gameObject.SetActive(i < allies.Length);
                if (i < allies.Length) Caption(unitButtons[i], (allies[i] == actor ? "▶ " : "") + allies[i].Definition.Name + "  HP " + allies[i].CurrentHp);
            }
            for (int i = 0; i < skillButtons.Count; i++)
            {
                skillButtons[i].gameObject.SetActive(actor != null && i < actor.Definition.Skills.Count);
                if (actor == null || i >= actor.Definition.Skills.Count) continue;
                // item: 이 버튼에 연결된 일반 공격.
                var item = actor.Definition.Skills[i];
                skillSlots[i].Bind(actor, item, item == skill);
            }
            for (int i = 0; i < targetButtons.Count; i++)
            {
                // target/distance: 해당 슬롯의 현재 유닛과 선택 사용자까지의 거리.
                var target = battle.Units.FirstOrDefault(u => u.Location == UnitLocation.Field && u.Slot == i + 1);
                int distance = actor != null && target != null ? Math.Abs(battle.ProjectedSlotBeforeMain(actor.Id) - target.Slot.Value) : 99;
                targetButtons[i].interactable = target != null && target.FactionId != battle.PlayerFactionId && skill != null && (zeroBp || (distance >= skill.MinRange && distance <= skill.MaxRange));
                Caption(targetButtons[i], (i + 1) + "번\n" + (target == null ? "빈 슬롯" : (target.Id == selectedTarget ? "▶ " : "") + target.Definition.Name + "\nHP " + target.CurrentHp));
            }
            // plan: 현재 Core에 적용된 예약. 편집 선택값과 구분해 표시한다.
            var plan = actor == null ? null : battle.MainReservation(actor.Id);
            info.text = "공유 COST " + battle.Costs[battle.PlayerFactionId] + "/10 · 예약 확보 " + battle.ReservedCost(battle.PlayerFactionId)
                + (skill == null ? "" : " · 사거리 " + skill.MinRange + "~" + skill.MaxRange + " · 헤일로 BP " + actor.HaloBp + "/" + HaloAnomaly.MaxBp)
                + "\n현재 예약: " + (plan == null ? "스킬 없음" : battle.Skill(actor.Id, plan.SkillId).Name + " → " + battle.Unit(plan.TargetId).Definition.Name);
            if (zeroBp)
                info.text += "\nBP 0이면 헤일로 이상: 자신과 양옆 피아 모두 공격 · 명중과 무관하게 최대 HP 55% 소모";
            // chosenTarget/chosenDistance: 스킬을 변경한 뒤 이전 선택 대상이 새 사거리 밖이면 적용 버튼도 막는다.
            var chosenTarget = battle.Units.FirstOrDefault(u => u.Id == selectedTarget && u.Location == UnitLocation.Field);
            int chosenDistance = actor != null && chosenTarget != null ? Math.Abs(battle.ProjectedSlotBeforeMain(actor.Id) - chosenTarget.Slot.Value) : 99;
            ApplyButton.interactable = actor != null && skill != null && chosenTarget != null && chosenTarget.FactionId != battle.PlayerFactionId
                && (zeroBp || (chosenDistance >= skill.MinRange && chosenDistance <= skill.MaxRange));
            EndOnlyButton.interactable = actor != null;
        }

        /// <summary>버튼의 기존 문구를 바꾼다.</summary>
        /// <param name="button">대상 버튼.</param>
        /// <param name="text">새 문구.</param>
        private static void Caption(Button button, string text) { button.GetComponentInChildren<Text>().text = text; }

        /// <summary>정해진 영역에 한국어 텍스트를 만든다. 장식 문구는 클릭을 막지 않는다.</summary>
        /// <param name="parent">UI 부모.</param>
        /// <param name="name">오브젝트 이름.</param>
        /// <param name="text">문구.</param>
        /// <param name="x">왼쪽 거리.</param>
        /// <param name="y">위쪽 거리.</param>
        /// <param name="width">너비.</param>
        /// <param name="height">높이.</param>
        /// <param name="size">글자 크기.</param>
        private Text Label(Transform parent, string name, string text, float x, float y, float width, float height, int size)
        {
            // go/label: 생성한 텍스트 오브젝트와 컴포넌트.
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); Place(go, parent, x, y, width, height);
            var label = go.GetComponent<Text>(); label.font = font; label.fontSize = size; label.text = text;
            label.color = Color.white; label.raycastTarget = false; label.alignment = TextAnchor.MiddleLeft; return label;
        }

        /// <summary>입력 가능한 버튼을 생성하고 콜백을 연결한다.</summary>
        /// <param name="parent">UI 부모.</param>
        /// <param name="name">오브젝트 이름.</param>
        /// <param name="text">문구.</param>
        /// <param name="x">왼쪽 거리.</param>
        /// <param name="y">위쪽 거리.</param>
        /// <param name="width">너비.</param>
        /// <param name="height">높이.</param>
        /// <param name="action">클릭할 때 실행하는 명령.</param>
        private Button ButtonAt(Transform parent, string name, string text, float x, float y, float width, float height, Action action)
        {
            // go/button/label: 배경과 버튼, 그 안의 캡션.
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); Place(go, parent, x, y, width, height);
            go.GetComponent<Image>().color = new Color32(37, 70, 98, 255);
            var button = go.GetComponent<Button>(); button.targetGraphic = go.GetComponent<Image>(); button.onClick.AddListener(() => action());
            var label = Label(go.transform, "Caption", text, 6, 2, width - 12, height - 4, 17); label.alignment = TextAnchor.MiddleCenter;
            return button;
        }

        /// <summary>좌상단 기준으로 UI를 배치한다.</summary>
        /// <param name="go">배치할 UI.</param>
        /// <param name="parent">부모.</param>
        /// <param name="x">오른쪽 거리.</param>
        /// <param name="y">아래쪽 거리.</param>
        /// <param name="width">너비.</param>
        /// <param name="height">높이.</param>
        private static void Place(GameObject go, Transform parent, float x, float y, float width, float height)
        {
            go.transform.SetParent(parent, false);
            // rect: 위치와 크기를 담는 UI 변환.
            var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height);
        }
    }
}
