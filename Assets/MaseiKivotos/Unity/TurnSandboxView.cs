using System;
using System.Collections.Generic;
using System.Linq;
using MaseiKivotos.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MaseiKivotos.Unity
{
    /// <summary>
    /// 기존 리소스 없이 턴 규칙을 확인할 수 있도록 독립적인 uGUI 화면을 생성하는 검증용 화면.
    /// </summary>
    public sealed class TurnSandboxView : TurnBattleView
    {
        /// <summary>
        /// 검증 화면 전체의 배경색.
        /// </summary>
        private readonly Color background = new Color32(14, 22, 36, 255);
        /// <summary>
        /// 카드·정보 영역·보조 버튼에 쓰는 기본 패널 색상.
        /// </summary>
        private readonly Color panel = new Color32(25, 38, 57, 255);
        /// <summary>
        /// 보조 설명과 비어 있는 슬롯에 쓰는 약한 강조 색상.
        /// </summary>
        private readonly Color muted = new Color32(155, 176, 199, 255);
        /// <summary>
        /// 아군 유닛 정보를 표시하는 색상.
        /// </summary>
        private readonly Color ally = new Color32(103, 189, 255, 255);
        /// <summary>
        /// 적군 유닛 정보를 표시하는 색상.
        /// </summary>
        private readonly Color enemy = new Color32(255, 145, 137, 255);
        /// <summary>
        /// 현재 단계와 턴 넘기기 버튼을 강조하는 색상.
        /// </summary>
        private readonly Color accent = new Color32(115, 236, 195, 255);
        /// <summary>
        /// 런타임에 생성하는 한국어 표시용 폰트. 화면 파괴 시 함께 해제한다.
        /// </summary>
        private Font font;
        /// <summary>
        /// 독립 검증 화면의 Canvas 루트. 생성한 UI 요소의 부모로 사용한다.
        /// </summary>
        private Transform canvasRoot;
        /// <summary>
        /// phaseText: 현재 턴 단계의 설명 텍스트.
        /// actorText: 현재 행동 중인 유닛 또는 대기 상태를 보여 주는 텍스트.
        /// costText: 아군과 적군의 공유 COST를 보여 주는 텍스트.
        /// orderText: 모든 세력이 공유하는 행동 순서와 차례 진행 상태를 표시하는 텍스트.
        /// historyText: 최근 공개 가능한 진행 기록을 표시하는 텍스트.
        /// reserveText: 필드에 나와 있지 않은 대기 유닛 안내 텍스트.
        /// modeText: 임시 데이터·미구현 범위와 현재 속도 예제를 안내하는 텍스트.
        /// </summary>
        private Text phaseText, actorText, costText, orderText, historyText, reserveText, modeText;
        /// <summary>
        /// 상단에 표시하는 다섯 단계의 텍스트 목록. 현재 단계만 강조한다.
        /// </summary>
        private readonly List<Text> phases = new List<Text>();
        /// <summary>
        /// 슬롯 번호 순서대로 보관하는 9개 클릭 버튼. 예약 가능한 아군만 활성화한다.
        /// </summary>
        private readonly List<Button> slots = new List<Button>();
        /// <summary>
        /// 각 슬롯의 번호·유닛·Speed·예약/실행 상태를 표시하는 텍스트 목록.
        /// </summary>
        private readonly List<Text> slotLabels = new List<Text>();
        /// <summary>
        /// 화면에 표시한 COST 문구. UI 연결 검증에서도 조회한다.
        /// </summary>
        public override string VisibleCost => costText.text;
        /// <summary>
        /// 화면에 표시한 현재 행동 차례 문구. 실제 Core 상태와의 일치를 검증할 때 조회한다.
        /// </summary>
        public override string VisibleActor => actorText.text;

        /// <summary>
        /// Canvas·9칸 버튼·진행 정보·조작 버튼을 한 번 생성하고 필요한 EventSystem을 준비한다.
        /// </summary>
        /// <param name="controller">전투 상태와 사용자 조작을 제공하는 공용 컨트롤러.</param>
        public override void Build(TurnSandboxController controller)
        {
            if (canvasRoot != null) return;
            font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Arial" }, 18);
            // root: 독립 검증 화면용 Canvas 게임 오브젝트.
            var root = new GameObject("Turn Sandbox Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvasRoot = root.transform;
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            // scaler: 해상도에 따른 UI 크기를 조절하는 CanvasScaler.
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800);

            // Game 뷰의 가로세로 비율이 달라도 조작 버튼이 화면 밖으로 잘리지 않게 한다.
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            // backdrop: 전체 화면 배경. 입력은 아래의 실제 버튼으로 전달한다.
            var backdrop = Box("Background", canvasRoot, 0, 0, 1280, 800, background);
            backdrop.raycastTarget = false;
            Label("Title", canvasRoot, "마세이 키보토스  /  턴 진행", 32, 24, 1000, 42, 30, Color.white);
            Label("Subtitle", canvasRoot, "턴을 넘기며 공통 행동 순서와 COST 변화를 확인하세요.", 34, 72, 1180, 28, 16, muted);
            // names: 턴 단계 번호에 대응하는 화면 표시 문구 배열.
            string[] names = { "1  턴 시작", "2  초기화", "3  행동 순서", "4  행동 예약", "5  행동 실행" };
            for (int i = 0; i < 5; i++)
            {
                // box: 새로 생성한 UI 배경 Image.
                var box = Box("Phase " + i, canvasRoot, 32 + i * 246, 112, 232, 40, panel);
                phases.Add(Label("Label", box.transform, names[i], 12, 0, 210, 40, 17, muted));
            }
            // turnPanel: 턴 번호와 현재 단계 설명을 담는 패널.
            var turnPanel = Box("Turn", canvasRoot, 32, 170, 190, 85, panel);
            TurnText = Label("Turn Number", turnPanel.transform, "", 16, 6, 172, 44, 30, accent);
            phaseText = Label("Current Phase", turnPanel.transform, "", 16, 51, 170, 26, 15, muted);
            // actorPanel: 현재 행동 차례 정보를 담는 패널.
            var actorPanel = Box("Active Unit", canvasRoot, 236, 170, 548, 85, panel);
            Label("Caption", actorPanel.transform, "현재 행동 차례", 16, 10, 480, 22, 14, muted);
            actorText = Label("Actor", actorPanel.transform, "", 16, 36, 510, 40, 22, Color.white);
            // costPanel: COST 설명과 수치를 묶는 정보 패널.
            var costPanel = Box("Cost", canvasRoot, 798, 170, 450, 85, panel);
            Label("Caption", costPanel.transform, "공유 COST   ·   첫 턴 3 / 상한 10", 16, 10, 418, 22, 14, muted);
            costText = Label("Values", costPanel.transform, "", 16, 38, 418, 38, 23, accent);

            Label("Field Caption", canvasRoot, "전투 필드  /  아군 카드를 눌러 턴 종료 예약·취소", 34, 276, 1200, 28, 17, Color.white);
            for (int i = 0; i < 9; i++)
            {
                // slotNumber: 배열 인덱스를 규칙의 1~9 슬롯 번호로 바꾼 값. 클릭 콜백에 고정해 전달한다.
                int slotNumber = i + 1;
                // button: 클릭 이벤트를 연결하거나 검증할 UI 버튼.
                var button = MakeButton("Slot " + slotNumber, "", 32 + i * 137, 316, 120, 138, panel, () =>
                {
                    // unit: 현재 처리하거나 표시할 유닛. 슬롯 조회 결과일 때는 빈 슬롯이면 null이다.
                    var unit = controller.Battle.Units.FirstOrDefault(u => u.Slot == slotNumber);
                    if (unit != null) controller.ToggleEndTurnReservation(unit.Id);
                });
                slots.Add(button); slotLabels.Add(button.GetComponentInChildren<Text>());
            }
            reserveText = Label("Reserves", canvasRoot, "", 34, 464, 1210, 26, 15, muted);
            // sequence: 공통 행동 순서를 표시할 패널.
            var sequence = Box("Order", canvasRoot, 32, 502, 1216, 58, panel);
            orderText = Label("Sequence", sequence.transform, "", 16, 0, 1184, 58, 19, Color.white);
            Box("Log", canvasRoot, 32, 576, 818, 178, panel);
            Label("Log Caption", canvasRoot, "진행 기록", 48, 584, 180, 25, 15, muted);
            historyText = Label("History", canvasRoot, "", 48, 615, 786, 132, 14, Color.white);
            historyText.alignment = TextAnchor.UpperLeft;
            SkipButton = MakeButton("Skip Turn", "턴 넘기기  →", 870, 576, 378, 54, accent, controller.SkipTurn);
            StepButton = MakeButton("Step", "예약 확정", 870, 642, 378, 44, panel, controller.Step);
            ResetButton = MakeButton("Reset", "처음부터", 870, 698, 182, 44, panel, controller.ResetBattle);
            ExampleButton = MakeButton("Example", "동률 예제", 1066, 698, 182, 44, panel, controller.ToggleExample);
            modeText = Label("Scope", canvasRoot, "", 34, 768, 1210, 24, 13, muted);
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                // input: 검증 씬에 EventSystem이 없을 때 생성하는 UI 입력 오브젝트.
                var input = new GameObject("Turn Sandbox Input", typeof(EventSystem), typeof(StandaloneInputModule));
                input.transform.SetParent(transform, false);
            }
        }

        /// <summary>
        /// 턴 번호·단계·순서·슬롯·COST·최근 기록을 갱신한다. 상대 예약 기록은 걸러서 표시한다.
        /// </summary>
        /// <param name="controller">전투 상태와 사용자 조작을 제공하는 공용 컨트롤러.</param>
        public override void Refresh(TurnSandboxController controller)
        {
            // battle: 진행하거나 표시·검증할 전투 상태 인스턴스.
            var battle = controller.Battle;
            TurnText.text = "TURN " + battle.TurnNumber.ToString("00");
            phaseText.text = PhaseName(battle.Phase);
            actorText.text = battle.ActiveUnitId != null
                ? battle.Unit(battle.ActiveUnitId).Definition.Name + "  ·  턴 종료 실행 대기"
                : battle.Phase == TurnPhase.Planning ? "행동 예약 중" : battle.Phase == TurnPhase.Ended ? "전투 종료" : "다음 단계 / 차례 대기";
            costText.text = "아군  " + battle.Costs[1] + " / 10     적군  " + battle.Costs[2] + " / 10";
            for (int i = 0; i < phases.Count; i++) phases[i].color = (int)battle.Phase == i + 1 ? accent : muted;
            for (int i = 0; i < 9; i++)
            {
                // unit: 현재 처리하거나 표시할 유닛. 슬롯 조회 결과일 때는 빈 슬롯이면 null이다.
                var unit = battle.Units.FirstOrDefault(u => u.Slot == i + 1);
                // friendly: 현재 슬롯 유닛이 플레이어 소속인지 여부. 색상과 예약 입력 허용에 사용한다.
                bool friendly = unit != null && unit.FactionId == battle.PlayerFactionId;
                // active: 이 슬롯의 유닛이 현재 차례 소유자인지 나타내는 강조 표시 조건.
                bool active = unit != null && unit.Id == battle.ActiveUnitId;
                slots[i].interactable = friendly && battle.Phase == TurnPhase.Planning && !controller.IsAdvancing;
                slots[i].GetComponent<Image>().color = active ? new Color32(39, 89, 81, 255) : panel;
                slotLabels[i].color = unit == null ? muted : friendly ? ally : enemy;
                if (unit == null) slotLabels[i].text = (i + 1) + "번 슬롯\n\n—";
                else
                {
                    // state: 유닛의 현재 차례·완료·예약 여부에 따라 선택한 슬롯 안내 문구.
                    string state = active ? "▶ 현재 차례" : battle.HasCompleted(unit.Id) ? "차례 완료" : !friendly ? "예약 비공개" : battle.HasEndTurnReservation(unit.Id) ? "종료 예약됨" : "눌러서 예약";
                    slotLabels[i].text = (i + 1) + "번 슬롯\n" + unit.Definition.Name + "\nSpeed " + unit.Definition.Stats.Speed + "\n" + state;
                }
            }
            reserveText.text = "대기  ·  " + string.Join(" / ", battle.Units.Where(u => u.Location == UnitLocation.Reserve).Select(u => u.Definition.Name)) + "    |    시작 2명 · 필드 최대 3명 · 전체 9슬롯";
            orderText.text = battle.Order.Count == 0 ? "행동 순서  ·  초기화 후 결정됩니다" : "행동 순서  ·  " + string.Join("   →   ", battle.Order.Select(id => (id == battle.ActiveUnitId ? "▶ " : battle.HasCompleted(id) ? "✓ " : "") + battle.Unit(id).Definition.Name));

            // visible: 상대의 비공개 예약을 제외하고 화면에 보여 줄 진행 기록.
            var visible = battle.Events.Where(e => e.Kind != TurnEventKind.Reserved || battle.Unit(e.UnitId).FactionId == battle.PlayerFactionId).ToList();
            historyText.text = string.Join("\n", visible.Skip(Math.Max(0, visible.Count - 6)).Select(e => "T" + e.Turn.ToString("00") + "  " + e.Message));
            SkipButton.interactable = !controller.IsAdvancing && battle.Phase != TurnPhase.Ended;
            StepButton.interactable = SkipButton.interactable;
            SkipButton.GetComponentInChildren<Text>().text = controller.IsAdvancing ? "차례 처리 중…" : "턴 넘기기  →";
            StepButton.GetComponentInChildren<Text>().text = battle.Phase == TurnPhase.Planning ? "예약 확정 · 모두 차례 생략" : "다음 단계 / 차례";
            ExampleButton.GetComponentInChildren<Text>().text = controller.EqualSpeedExample ? "기본 예제" : "동률 예제";
            modeText.text = "턴 검증용 임시 유닛 · 적은 차례 생략 · 스킬/상태 효과 미연결   |   " + (controller.EqualSpeedExample ? "전원 Speed 100 · 매 턴 동률 재추첨" : "Speed 150 → 120 → 80 → 50");
        }

        /// <summary>
        /// 턴 단계 enum을 검증 화면에서 읽을 수 있는 한국어 문구로 변환한다.
        /// </summary>
        /// <param name="phase">전환하거나 표시할 턴 단계.</param>
        private static string PhaseName(TurnPhase phase)
        {
            // names: 턴 단계 번호에 대응하는 화면 표시 문구 배열.
            string[] names = { "", "1단계 · 턴 시작", "2단계 · 초기화", "3단계 · 행동 순서", "4단계 · 행동 예약", "5단계 · 행동 실행", "전투 종료" };
            return names[(int)phase];
        }

        /// <summary>
        /// 지정한 부모와 영역에 단색 Image 패널을 생성해 반환한다.
        /// </summary>
        /// <param name="name">생성할 UI 오브젝트의 이름.</param>
        /// <param name="parent">생성하거나 조회할 오브젝트의 부모 Transform.</param>
        /// <param name="x">부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.</param>
        /// <param name="y">부모 위쪽에서 아래쪽으로 떨어진 UI 거리.</param>
        /// <param name="w">UI 영역의 가로 크기.</param>
        /// <param name="h">UI 영역의 세로 크기.</param>
        /// <param name="color">패널 또는 글자에 적용할 색상.</param>
        private Image Box(string name, Transform parent, float x, float y, float w, float h, Color color)
        {
            // go: UI 컴포넌트를 담기 위해 생성한 게임 오브젝트.
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            Place(go, parent, x, y, w, h);
            // image: 생성한 UI 배경의 Image 컴포넌트.
            var image = go.GetComponent<Image>(); image.color = color; return image;
        }

        /// <summary>
        /// 폰트·글자 크기·색을 적용한 텍스트를 생성한다. 텍스트 자체는 마우스 입력을 가로채지 않는다.
        /// </summary>
        /// <param name="name">생성할 UI 오브젝트의 이름.</param>
        /// <param name="parent">생성하거나 조회할 오브젝트의 부모 Transform.</param>
        /// <param name="text">표시할 본문 문자열.</param>
        /// <param name="x">부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.</param>
        /// <param name="y">부모 위쪽에서 아래쪽으로 떨어진 UI 거리.</param>
        /// <param name="w">UI 영역의 가로 크기.</param>
        /// <param name="h">UI 영역의 세로 크기.</param>
        /// <param name="size">글자 크기.</param>
        /// <param name="color">패널 또는 글자에 적용할 색상.</param>
        private Text Label(string name, Transform parent, string text, float x, float y, float w, float h, int size, Color color)
        {
            // go: UI 컴포넌트를 담기 위해 생성한 게임 오브젝트.
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            Place(go, parent, x, y, w, h);
            // label: 문구·폰트·정렬을 설정할 UI Text 컴포넌트.
            var label = go.GetComponent<Text>();
            label.font = font; label.fontSize = size; label.color = color; label.text = text;
            label.alignment = TextAnchor.MiddleLeft; label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        /// <summary>
        /// 검증 화면용 Image 버튼과 가운데 문구를 만들고 클릭 콜백을 연결한다.
        /// </summary>
        /// <param name="name">생성할 UI 오브젝트의 이름.</param>
        /// <param name="caption">버튼 또는 텍스트에 표시할 문구.</param>
        /// <param name="x">부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.</param>
        /// <param name="y">부모 위쪽에서 아래쪽으로 떨어진 UI 거리.</param>
        /// <param name="w">UI 영역의 가로 크기.</param>
        /// <param name="h">UI 영역의 세로 크기.</param>
        /// <param name="color">패널 또는 글자에 적용할 색상.</param>
        /// <param name="action">버튼 클릭 시 실행할 콜백.</param>
        private Button MakeButton(string name, string caption, float x, float y, float w, float h, Color color, Action action)
        {
            // box: 새로 생성한 UI 배경 Image.
            var box = Box(name, canvasRoot, x, y, w, h, color);
            // button: 클릭 이벤트를 연결하거나 검증할 UI 버튼.
            var button = box.gameObject.AddComponent<Button>(); button.targetGraphic = box;
            // colors: 버튼 상태별 색 설정의 복사본. 수정 후 버튼에 다시 대입한다.
            var colors = button.colors;

            // 클릭할 수 없는 적군 카드도 정보는 읽을 수 있도록 과도하게 어둡게 만들지 않는다.
            colors.disabledColor = new Color(0.8f, 0.8f, 0.8f, 1f); button.colors = colors;
            // label: 문구·폰트·정렬을 설정할 UI Text 컴포넌트.
            var label = Label("Label", box.transform, caption, 8, 4, w - 16, h - 8, 16, color == accent ? background : Color.white);
            label.alignment = TextAnchor.MiddleCenter;
            button.onClick.AddListener(() => action());
            return button;
        }

        /// <summary>
        /// 부모의 왼쪽 위를 원점으로 UI 위치와 크기를 지정한다. x는 오른쪽, y는 아래쪽 방향의 거리다.
        /// </summary>
        /// <param name="go">부모·위치·크기를 설정할 UI 오브젝트.</param>
        /// <param name="parent">생성하거나 조회할 오브젝트의 부모 Transform.</param>
        /// <param name="x">부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.</param>
        /// <param name="y">부모 위쪽에서 아래쪽으로 떨어진 UI 거리.</param>
        /// <param name="w">UI 영역의 가로 크기.</param>
        /// <param name="h">UI 영역의 세로 크기.</param>
        private static void Place(GameObject go, Transform parent, float x, float y, float w, float h)
        {
            go.transform.SetParent(parent, false);
            // rect: UI 위치·크기 또는 버튼 중앙 좌표 계산에 사용하는 RectTransform.
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h);
        }

        /// <summary>
        /// 이 화면이 런타임에 생성한 폰트를 해제한다. 원본 에셋 폰트는 수정하지 않는다.
        /// </summary>
        private void OnDestroy() { if (font != null) Destroy(font); }
    }
}
