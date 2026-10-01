using System;
using System.Collections.Generic;
using System.Linq;
using MaseiKivotos.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MaseiKivotos.Unity
{

    /// <summary>
    /// Stage_Battle의 InGame 시각 리소스를 새 턴 화면에 연결한다. 격리된 카브 스크립트를 호출하거나 프리팹 원본을 변경하지 않는다.
    /// </summary>
    public sealed class StageBattleView : TurnBattleView
    {
        /// <summary>
        /// 배경·슬롯·기존 UI가 있는 씬의 InGame 루트. Inspector에서 미지정이면 같은 씬의 InGame 이름으로 찾는다.
        /// </summary>
        [SerializeField] private Transform inGame;
        /// <summary>
        /// 임시 아군 A에 사용할 정확한 PNG 스프라이트 프레임. 같은 이름의 Aseprite와 혼동하지 않도록 직접 연결한다.
        /// </summary>
        [SerializeField] private Sprite allyA;
        /// <summary>
        /// 임시 아군 B에 사용할 PNG 스프라이트 프레임.
        /// </summary>
        [SerializeField] private Sprite allyB;
        /// <summary>
        /// 임시 적군 X/Y에 함께 사용할 PNG 스프라이트 프레임.
        /// </summary>
        [SerializeField] private Sprite enemy;
        /// <summary>
        /// 런타임에 생성하는 한국어 표시용 폰트. 화면 파괴 시 함께 해제한다.
        /// </summary>
        private Font font;
        /// <summary>
        /// 기존 InGame의 UICanvas. 새 HUD와 재사용 버튼을 표시한다.
        /// </summary>
        private Canvas canvas;
        /// <summary>
        /// 필드를 비추는 이 씬의 MainCamera. 슬롯 위치를 화면 좌표로 바꿀 때도 사용한다.
        /// </summary>
        private Camera battleCamera;
        /// <summary>
        /// 기존 Canvas 아래에 런타임으로 생성하는 새 턴 정보 및 조작 UI의 루트.
        /// </summary>
        private RectTransform hud;
        /// <summary>
        /// 기존 UnitSlots 자식들을 왼쪽부터 정렬한 배열. 인덱스 0~8은 규칙의 슬롯 1~9에 대응한다.
        /// </summary>
        private Transform[] slots;
        /// <summary>
        /// 실제 슬롯 위에 겹쳐 놓는 클릭 영역. 아군 종료 예약과 취소 입력을 받는다.
        /// </summary>
        private readonly List<Button> slotButtons = new List<Button>();
        /// <summary>
        /// 각 슬롯의 번호·유닛·Speed·예약/실행 상태를 표시하는 텍스트 목록.
        /// </summary>
        private readonly List<Text> slotLabels = new List<Text>();
        /// <summary>
        /// 기존 슬롯에서 재사용하는 캐릭터 SpriteRenderer 목록. 슬롯 배열과 같은 순서다.
        /// </summary>
        private readonly List<SpriteRenderer> unitSprites = new List<SpriteRenderer>();
        /// <summary>
        /// 기존 슬롯 바닥 SpriteRenderer 목록. 아군·적군·현재 차례 색상을 표시한다.
        /// </summary>
        private readonly List<SpriteRenderer> grounds = new List<SpriteRenderer>();
        /// <summary>
        /// phaseText: 현재 턴 단계의 설명 텍스트.
        /// actorText: 현재 행동 중인 유닛 또는 대기 상태를 보여 주는 텍스트.
        /// costText: 아군과 적군의 공유 COST를 보여 주는 텍스트.
        /// orderText: 모든 세력이 공유하는 행동 순서와 차례 진행 상태를 표시하는 텍스트.
        /// historyText: 최근 공개 가능한 진행 기록을 표시하는 텍스트.
        /// </summary>
        private Text phaseText, actorText, costText, orderText, historyText;
        /// <summary>
        /// 기존 COST 바의 Image. 아군 COST/10을 가로 채움 비율로 표시한다.
        /// </summary>
        private Image costFill;
        /// <summary>
        /// 아군 A, 아군 B, 공용 적군 순서로 참조하는 임시 캐릭터 프레임 배열.
        /// </summary>
        private Sprite[] portraits;
        /// <summary>
        /// 화면에 표시한 COST 문구. UI 연결 검증에서도 조회한다.
        /// </summary>
        public override string VisibleCost => costText.text;
        /// <summary>
        /// 화면에 표시한 현재 행동 차례 문구. 실제 Core 상태와의 일치를 검증할 때 조회한다.
        /// </summary>
        public override string VisibleActor => actorText.text;
        /// <summary>
        /// 검증과 외부 조회에 공개하는 왼쪽부터 정렬된 실제 슬롯 Transform 목록.
        /// </summary>
        public IReadOnlyList<Transform> Slots => slots;
        /// <summary>
        /// UI 검증에서 실제 raycast와 클릭을 수행할 수 있도록 공개하는 슬롯 버튼 목록.
        /// </summary>
        public IReadOnlyList<Button> SlotButtons => slotButtons;
        /// <summary>
        /// COST 수치와 실제 게이지 채움량의 일치를 확인하기 위한 Image 참조.
        /// </summary>
        public Image CostFill => costFill;

        /// <summary>
        /// 기존 슬롯·Canvas·턴 종료/SKIP 버튼·COST 바를 연결하고 부족한 HUD를 생성한다. 구형 메뉴는 현재 실행 인스턴스에서만 숨긴다.
        /// </summary>
        /// <param name="owner">이 화면을 사용하는 공용 턴 컨트롤러.</param>
        public override void Build(TurnSandboxController owner)
        {
            if (hud != null) return;
            if (inGame == null)
                inGame = gameObject.scene.GetRootGameObjects().Single(g => g.name == "InGame").transform;
            slots = Required(inGame, "UnitSlots").Cast<Transform>().OrderBy(t => t.localPosition.x).ToArray();
            if (slots.Length != 9) throw new InvalidOperationException("Stage_Battle needs exactly nine UnitSlots.");
            battleCamera = gameObject.scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Camera>()).Single(c => c.CompareTag("MainCamera"));
            canvas = Required(inGame, "UICanvas").GetComponent<Canvas>();
            font = Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Arial" }, 18);
            // scaler: 해상도에 따른 UI 크기를 조절하는 CanvasScaler.
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            hud = (RectTransform)new GameObject("Makibo Battle HUD", typeof(RectTransform)).transform;
            hud.SetParent(canvas.transform, false);
            hud.anchorMin = Vector2.zero; hud.anchorMax = Vector2.one; hud.offsetMin = hud.offsetMax = Vector2.zero;
            // originalBattle: 기존 턴 종료/SKIP 버튼과 COST 바가 배치된 Battle_UI 루트.
            var originalBattle = Required(inGame, "UICanvas/Play_UI/Battle_UI");
            SkipButton = Required(originalBattle, "TurnEndButton").GetComponent<Button>();
            StepButton = Required(originalBattle, "ActionPointSkipNutton").GetComponent<Button>();
            costFill = Required(originalBattle, "CostGauge/CostBarBackground/CostBar").GetComponent<Image>();
            // costBack: 재사용 COST 바의 배경 Image. 새 HUD에 옮겨 표시한다.
            var costBack = costFill.transform.parent.GetComponent<Image>();
            foreach (Transform child in canvas.transform.Cast<Transform>().ToArray())
                if (child != hud) child.gameObject.SetActive(false);
            Required(inGame, "ProdutionCanvas").gameObject.SetActive(false);

            Panel("Header", 20, 16, 1240, 86, new Color32(16, 26, 42, 235));
            TurnText = Label("Turn", "", 38, 25, 180, 38, 28);
            phaseText = Label("Phase", "", 230, 28, 980, 28, 18);
            orderText = Label("Public Order", "", 38, 66, 1190, 27, 18);
            Panel("Commands", 20, 514, 1240, 184, new Color32(16, 26, 42, 245));
            actorText = Label("Actor", "", 40, 529, 840, 28, 21);
            historyText = Label("History", "", 40, 568, 790, 54, 15);
            historyText.alignment = TextAnchor.UpperLeft;
            costText = Label("Cost", "", 40, 632, 590, 30, 18);
            Place(costBack.gameObject, hud, 650, 641, 190, 14);
            costBack.gameObject.SetActive(true); costBack.raycastTarget = false;
            // fillRect: COST 채움 Image의 RectTransform. 배경 영역 전체에 맞춘다.
            var fillRect = costFill.rectTransform;
            fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = fillRect.offsetMax = Vector2.zero;
            costFill.type = Image.Type.Filled; costFill.fillMethod = Image.FillMethod.Horizontal;
            costFill.fillOrigin = 0; costFill.raycastTarget = false;
            Rebind(SkipButton, "턴 넘기기  →", 884, 529, 352, 48, owner.SkipTurn);
            Rebind(StepButton, "예약 확정", 884, 585, 352, 44, owner.Step);
            SkipButton.GetComponentInChildren<Text>().color = new Color32(16, 26, 42, 255);
            StepButton.GetComponentInChildren<Text>().color = new Color32(16, 26, 42, 255);
            SkipButton.GetComponent<Image>().color = new Color32(126, 235, 196, 255);
            ResetButton = NewButton("Reset", "처음부터", 884, 639, 170, 40, owner.ResetBattle);
            ExampleButton = NewButton("Equal Speed", "동률 예제", 1066, 639, 170, 40, owner.ToggleExample);
            Label("Scope", "턴 진행 확인용 임시 유닛 · 스킬/피해 미연결  |  아군 슬롯 클릭: 종료 예약/취소", 26, 699, 1220, 21, 13);

            portraits = new[] { allyA, allyB, enemy };
            if (portraits.Any(sprite => sprite == null)) throw new InvalidOperationException("Assign the three preview character sprites in Stage_Battle.");
            for (int i = 0; i < slots.Length; i++)
            {
                // index: 클릭 콜백이 반복 종료 후에도 같은 슬롯을 가리키도록 복사한 배열 인덱스.
                int index = i;
                // slot: 현재 반복에서 화면을 연결할 기존 슬롯 Transform.
                var slot = slots[i];
                grounds.Add(Required(slot, "SlotGroundSprite").GetComponent<SpriteRenderer>());
                // model: 해당 슬롯에 원래 배치된 UnitBase 리소스 루트.
                var model = Required(slot, "UnitBaseBase/UnitBase");
                foreach (var animator in model.GetComponentsInChildren<Animator>(true)) animator.enabled = false;
                foreach (var c in model.GetComponentsInChildren<Canvas>(true)) c.gameObject.SetActive(false);
                // sprite: 해당 슬롯에서 캐릭터 프레임과 방향·크기를 표시할 기존 SpriteRenderer.
                var sprite = model.GetComponentsInChildren<SpriteRenderer>(true).Single(r => r.name == "UnitCharactorSprite");
                unitSprites.Add(sprite);
                // button: 클릭 이벤트를 연결하거나 검증할 UI 버튼.
                var button = NewButton("Slot " + (i + 1), "", 0, 0, 112, 260, () =>
                {
                    // unit: 현재 처리하거나 표시할 유닛. 슬롯 조회 결과일 때는 빈 슬롯이면 null이다.
                    var unit = owner.Battle.Units.FirstOrDefault(u => u.Location == UnitLocation.Field && u.Slot == index + 1);
                    if (unit != null) owner.ToggleEndTurnReservation(unit.Id);
                });
                button.GetComponent<Image>().color = new Color(0, 0, 0, 0.01f);
                slotButtons.Add(button);
                // label: 문구·폰트·정렬을 설정할 UI Text 컴포넌트.
                var label = button.GetComponentInChildren<Text>();
                label.fontSize = 15; label.alignment = TextAnchor.LowerCenter;
                // captionBack: 밝은 배경 위에서도 슬롯 설명을 읽을 수 있게 하는 어두운 패널.
                var captionBack = Panel("Slot Caption Background", 0, 188, 112, 72, new Color32(16, 26, 42, 240));
                Place(captionBack.gameObject, button.transform, 0, 188, 112, 72);
                captionBack.transform.SetAsFirstSibling();
                slotLabels.Add(label);
            }
        }

        /// <summary>
        /// Core 상태에 맞춰 재사용 리소스의 색·캐릭터·COST와 새 정보 문구를 갱신한다. 현재는 임시 편성과 턴 종료만 표시한다.
        /// </summary>
        /// <param name="owner">이 화면을 사용하는 공용 턴 컨트롤러.</param>
        public override void Refresh(TurnSandboxController owner)
        {
            // b: 화면에 반영할 현재 전투 상태.
            var b = owner.Battle;
            TurnText.text = "TURN " + b.TurnNumber.ToString("00");
            // phaseNames: 현재 단계 번호로 조회할 한국어 단계 설명.
            string[] phaseNames = { "", "1 · 턴 시작", "2 · 초기화", "3 · 행동 순서", "4 · 행동 예약", "5 · 행동 실행", "전투 종료" };
            phaseText.text = phaseNames[(int)b.Phase] + "    |    필드 2명 + 대기 1명씩";
            actorText.text = b.ActiveUnitId == null ? "현재 차례  ·  " + (b.Phase == TurnPhase.Planning ? "행동 예약 중" : "다음 단계 대기") : "현재 차례  ▶  " + b.Unit(b.ActiveUnitId).Definition.Name;
            costText.text = "COST  아군 " + b.Costs[1] + "/10   ·   적군 " + b.Costs[2] + "/10";
            costFill.fillAmount = b.Costs[1] / 10f;
            orderText.text = "행동 순서  " + string.Join("  →  ", b.Order.Select(id => (id == b.ActiveUnitId ? "▶ " : b.HasCompleted(id) ? "✓ " : "") + b.Unit(id).Definition.Name));
            // visible: 상대의 비공개 예약을 제외하고 화면에 보여 줄 진행 기록.
            var visible = b.Events.Where(e => e.Kind != TurnEventKind.Reserved || b.Unit(e.UnitId).FactionId == b.PlayerFactionId).ToArray();
            historyText.text = string.Join("\n", visible.Skip(Math.Max(0, visible.Length - 2)).Select(e => "T" + e.Turn.ToString("00") + "  " + e.Message));
            for (int i = 0; i < slots.Length; i++)
            {
                // u: 현재 슬롯에 배치된 필드 유닛. 빈 슬롯이면 null이다.
                var u = b.Units.FirstOrDefault(unit => unit.Location == UnitLocation.Field && unit.Slot == i + 1);
                // friendly: 현재 슬롯 유닛이 플레이어 소속인지 여부. 색상과 예약 입력 허용에 사용한다.
                bool friendly = u != null && u.FactionId == b.PlayerFactionId;
                // active: 이 슬롯의 유닛이 현재 차례 소유자인지 나타내는 강조 표시 조건.
                bool active = u != null && u.Id == b.ActiveUnitId;
                // color: 빈 슬롯·아군·적군 상태에 따라 선택한 기본 표시 색상.
                Color color = u == null ? new Color32(195, 207, 217, 255) : friendly ? new Color32(118, 217, 255, 255) : new Color32(255, 167, 157, 255);
                grounds[i].color = active ? new Color32(126, 255, 179, 220) : new Color(color.r, color.g, color.b, u == null ? 0.14f : 0.32f);
                unitSprites[i].gameObject.SetActive(u != null);
                if (u != null)
                {
                    // sprite: 해당 슬롯에서 캐릭터 프레임과 방향·크기를 표시할 기존 SpriteRenderer.
                    var sprite = unitSprites[i];
                    sprite.sprite = portraits[friendly ? u.Id == "A" ? 0 : 1 : 2];
                    sprite.flipX = !friendly; sprite.sortingOrder = 5;
                    sprite.transform.position = new Vector3(slots[i].position.x, 2.1f, 0);

                    // 원본 128픽셀 셀에는 투명 여백이 있다. 셀 전체 높이가 아닌 원본 PPU를 기준으로 크기를 맞춘다.
                    sprite.transform.localScale = Vector3.one * .85f;
                    sprite.color = b.HasCompleted(u.Id) ? new Color(.7f, .7f, .7f) : Color.white;
                }
                slotButtons[i].interactable = friendly && b.Phase == TurnPhase.Planning && !owner.IsAdvancing;
                slotLabels[i].color = active ? new Color32(126, 255, 179, 255) : color;
                slotLabels[i].text = (i + 1) + "번" + (u == null ? "\n빈 슬롯" : "  " + u.Definition.Name + "\nSpeed " + u.Definition.Stats.Speed + "\n" + (active ? "▶ 현재 차례" : b.HasCompleted(u.Id) ? "차례 완료" : !friendly ? "예약 비공개" : b.HasEndTurnReservation(u.Id) ? "종료 예약됨" : "눌러서 예약"));
            }
            SkipButton.interactable = StepButton.interactable = !owner.IsAdvancing && b.Phase != TurnPhase.Ended;
            SkipButton.GetComponentInChildren<Text>().text = owner.IsAdvancing ? "차례 처리 중…" : "턴 넘기기  →";
            StepButton.GetComponentInChildren<Text>().text = b.Phase == TurnPhase.Planning ? "예약 확정 · 차례 생략" : "다음 단계 / 차례";
            ExampleButton.GetComponentInChildren<Text>().text = owner.EqualSpeedExample ? "기본 예제" : "동률 예제";
        }

        /// <summary>
        /// 화면 비율에 맞춰 카메라 범위를 확보하고 각 월드 슬롯 위에 클릭 영역과 설명을 정렬한다.
        /// </summary>
        private void LateUpdate()
        {
            if (hud == null) return;
            battleCamera.orthographicSize = Mathf.Max(7.2f, 12.8f / battleCamera.aspect);
            for (int i = 0; i < slotButtons.Count; i++)
            {
                // screen: 월드 슬롯의 위치를 카메라로 투영한 화면 좌표.
                var screen = battleCamera.WorldToScreenPoint(new Vector3(slots[i].position.x, 2.0f, 0));
                RectTransformUtility.ScreenPointToLocalPointInRectangle(hud, screen, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out var local);
                // rect: UI 위치·크기 또는 버튼 중앙 좌표 계산에 사용하는 RectTransform.
                var rect = (RectTransform)slotButtons[i].transform;
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.pivot = new Vector2(.5f, .5f);
                rect.anchoredPosition = local;
            }
        }

        /// <summary>
        /// 부모 아래의 필수 리소스를 상대 경로로 찾는다. 없으면 누락된 경로가 드러나는 예외를 발생시킨다.
        /// </summary>
        /// <param name="parent">생성하거나 조회할 오브젝트의 부모 Transform.</param>
        /// <param name="path">부모 기준으로 찾을 자식의 상대 경로.</param>
        private static Transform Required(Transform parent, string path) => parent.Find(path) ?? throw new InvalidOperationException("Missing stage resource: " + path);
        /// <summary>
        /// 새 HUD에 입력을 가로채지 않는 색상 패널을 생성해 반환한다.
        /// </summary>
        /// <param name="name">생성할 UI 오브젝트의 이름.</param>
        /// <param name="x">부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.</param>
        /// <param name="y">부모 위쪽에서 아래쪽으로 떨어진 UI 거리.</param>
        /// <param name="w">UI 영역의 가로 크기.</param>
        /// <param name="h">UI 영역의 세로 크기.</param>
        /// <param name="color">패널 또는 글자에 적용할 색상.</param>
        private Image Panel(string name, float x, float y, float w, float h, Color color)
        {
            // go: UI 컴포넌트를 담기 위해 생성한 게임 오브젝트.
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); Place(go, hud, x, y, w, h);
            // image: 생성한 UI 배경의 Image 컴포넌트.
            var image = go.GetComponent<Image>(); image.color = color; image.raycastTarget = false; return image;
        }
        /// <summary>
        /// 새 HUD에 한국어 텍스트를 생성한다. 클릭 판정은 버튼에서 처리하도록 raycast를 끈다.
        /// </summary>
        /// <param name="name">생성할 UI 오브젝트의 이름.</param>
        /// <param name="caption">버튼 또는 텍스트에 표시할 문구.</param>
        /// <param name="x">부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.</param>
        /// <param name="y">부모 위쪽에서 아래쪽으로 떨어진 UI 거리.</param>
        /// <param name="w">UI 영역의 가로 크기.</param>
        /// <param name="h">UI 영역의 세로 크기.</param>
        /// <param name="size">글자 크기.</param>
        private Text Label(string name, string caption, float x, float y, float w, float h, int size)
        {
            // go: UI 컴포넌트를 담기 위해 생성한 게임 오브젝트.
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); Place(go, hud, x, y, w, h);
            // label: 문구·폰트·정렬을 설정할 UI Text 컴포넌트.
            var label = go.GetComponent<Text>(); label.font = font; label.text = caption; label.fontSize = size;
            label.color = Color.white; label.alignment = TextAnchor.MiddleLeft; label.raycastTarget = false; return label;
        }
        /// <summary>
        /// 부족한 조작에 사용할 버튼 배경을 생성하고 문구와 이벤트를 연결한다.
        /// </summary>
        /// <param name="name">생성할 UI 오브젝트의 이름.</param>
        /// <param name="caption">버튼 또는 텍스트에 표시할 문구.</param>
        /// <param name="x">부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.</param>
        /// <param name="y">부모 위쪽에서 아래쪽으로 떨어진 UI 거리.</param>
        /// <param name="w">UI 영역의 가로 크기.</param>
        /// <param name="h">UI 영역의 세로 크기.</param>
        /// <param name="action">버튼 클릭 시 실행할 콜백.</param>
        private Button NewButton(string name, string caption, float x, float y, float w, float h, Action action)
        {
            // image: 생성한 UI 배경의 Image 컴포넌트.
            var image = Panel(name, x, y, w, h, new Color32(36, 64, 87, 255));
            // button: 클릭 이벤트를 연결하거나 검증할 UI 버튼.
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            Rebind(button, caption, x, y, w, h, action); return button;
        }
        /// <summary>
        /// 버튼을 새 HUD로 옮기고 구형 문구를 숨긴 뒤 클릭 이벤트를 새 콜백으로 교체한다. 변경은 현재 실행 인스턴스에만 적용한다.
        /// </summary>
        /// <param name="button">클릭하거나 새 HUD에 다시 연결할 버튼.</param>
        /// <param name="caption">버튼 또는 텍스트에 표시할 문구.</param>
        /// <param name="x">부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.</param>
        /// <param name="y">부모 위쪽에서 아래쪽으로 떨어진 UI 거리.</param>
        /// <param name="w">UI 영역의 가로 크기.</param>
        /// <param name="h">UI 영역의 세로 크기.</param>
        /// <param name="action">버튼 클릭 시 실행할 콜백.</param>
        private void Rebind(Button button, string caption, float x, float y, float w, float h, Action action)
        {
            Place(button.gameObject, hud, x, y, w, h); button.gameObject.SetActive(true);
            foreach (Transform child in button.transform) child.gameObject.SetActive(false);
            button.onClick = new Button.ButtonClickedEvent(); button.onClick.AddListener(() => action());
            button.GetComponent<Image>().raycastTarget = true;
            // label: 문구·폰트·정렬을 설정할 UI Text 컴포넌트.
            var label = Label(button.name + " Caption", caption, 0, 0, w, h, 18);
            label.transform.SetParent(button.transform, false); label.alignment = TextAnchor.MiddleCenter;
            // colors: 버튼 상태별 색 설정의 복사본. 수정 후 버튼에 다시 대입한다.
            var colors = button.colors; colors.disabledColor = new Color(.8f, .8f, .8f); button.colors = colors;
            // navigation: 버튼 키보드 탐색 설정의 복사본. 슬롯 사이의 의도치 않은 자동 탐색을 끈다.
            var navigation = button.navigation; navigation.mode = Navigation.Mode.None; button.navigation = navigation;
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
            var rect = (RectTransform)go.transform; rect.localScale = Vector3.one;
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(w, h);
        }
        /// <summary>
        /// 이 화면이 런타임에 생성한 폰트를 해제한다. 원본 에셋 폰트는 수정하지 않는다.
        /// </summary>
        private void OnDestroy() { if (font != null) Destroy(font); }
    }
}
