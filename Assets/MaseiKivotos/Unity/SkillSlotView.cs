using System;
using MaseiKivotos.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MaseiKivotos.Unity
{
    /// <summary>기존 SkillSlot 프리팹의 표시 전용 연결부. 전투 계산이나 BP 소비는 하지 않는다.</summary>
    public sealed class SkillSlotView : MonoBehaviour
    {
        /// <summary>Resources에서 읽는 기존 프리팹 경로.</summary>
        public const string PrefabPath = "prefab/UI/SkillSlot";
        /// <summary>염·습·전·폭·금·신·성·암·순·사·무의 표시 이름.</summary>
        private static readonly string[] traitNames = { "염", "습", "전", "폭", "금", "신", "성", "암", "순", "사", "무" };
        /// <summary>사용자 지정 색 계열을 구체적인 RGB 값으로 표현한 팔레트.</summary>
        private static readonly Color32[] traitColors = {
            new Color32(245, 148, 55, 255), new Color32(38, 58, 133, 255),
            new Color32(36, 170, 165, 255), new Color32(218, 65, 65, 255),
            new Color32(192, 192, 192, 255), new Color32(135, 206, 235, 255),
            new Color32(255, 239, 153, 255), new Color32(132, 76, 179, 255),
            new Color32(177, 222, 105, 255), new Color32(78, 52, 38, 255),
            new Color32(255, 198, 218, 255)
        };
        /// <summary>선택된 스킬의 외곽선. 타입/속성 색상과 독립적이다.</summary>
        private Outline selection;
        /// <summary>타입·속성 배경 내부에 런타임으로 추가하는 이름.</summary>
        private TMP_Text typeLabel, traitLabel;
        /// <summary>미지정 아이콘임을 알리는 문구.</summary>
        private TMP_Text iconPlaceholder;
        /// <summary>프리팹에 원래 존재하는 선택 버튼.</summary>
        public Button Button { get; private set; }
        /// <summary>SkillNameText: 스킬명.</summary>
        public TMP_Text NameText { get; private set; }
        /// <summary>SkillFlavorText: 효과 설명.</summary>
        public TMP_Text FlavorText { get; private set; }
        /// <summary>SkillCostText: 공유 COST가 아닌 현재/최대 BP.</summary>
        public TMP_Text BpText { get; private set; }
        /// <summary>SkillRangeText: 사거리.</summary>
        public TMP_Text RangeText { get; private set; }
        /// <summary>SkillAreaText: 효과 범위.</summary>
        public TMP_Text AreaText { get; private set; }
        /// <summary>SkillIcon: 지정된 스킬 이미지 또는 미정 표시 영역.</summary>
        public Image Icon { get; private set; }
        /// <summary>SkillType: 타입 색 배경.</summary>
        public Image TypeBadge { get; private set; }
        /// <summary>SkillTrait: 속성 색 배경.</summary>
        public Image TraitBadge { get; private set; }

        /// <summary>원본 프리팹을 복제하고 참조를 연결한다. 원본 에셋은 수정하지 않는다.</summary>
        /// <param name="prefab">Resources에서 읽은 SkillSlot 원본.</param>
        /// <param name="parent">표시 부모.</param>
        /// <param name="selected">스킬 선택 콜백.</param>
        public static SkillSlotView Create(GameObject prefab, Transform parent, Action selected)
        {
            if (prefab == null) throw new InvalidOperationException("SkillSlot prefab not found at Resources/" + PrefabPath);
            // instance/view: 구형 전투 코드를 연결하지 않는 프리팹 복제본과 새 표시 컴포넌트.
            var instance = Instantiate(prefab, parent, false);
            var view = instance.GetComponent<SkillSlotView>() ?? instance.AddComponent<SkillSlotView>();
            view.Initialize(); view.Button.onClick = new Button.ButtonClickedEvent();
            if (selected != null) view.Button.onClick.AddListener(() => selected());
            return view;
        }

        /// <summary>필수 자식들을 이름으로 연결하고 읽기 쉬운 카드 배치를 적용한다.</summary>
        private void Initialize()
        {
            if (Button != null) return;
            Button = GetComponent<Button>();
            NameText = Required<TMP_Text>("SkillNameText"); FlavorText = Required<TMP_Text>("SkillFlavorText");
            BpText = Required<TMP_Text>("SkillCostText"); RangeText = Required<TMP_Text>("SkillRangeText"); AreaText = Required<TMP_Text>("SkillAreaText");
            Icon = Required<Image>("SkillIcon"); TypeBadge = Required<Image>("SkillType"); TraitBadge = Required<Image>("SkillTrait");
            typeLabel = BadgeLabel(TypeBadge.transform, "Type Label"); traitLabel = BadgeLabel(TraitBadge.transform, "Trait Label");
            iconPlaceholder = BadgeLabel(Icon.transform, "Icon Placeholder"); iconPlaceholder.text = "미정";
            foreach (var graphic in GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = graphic.gameObject == gameObject;
            selection = gameObject.AddComponent<Outline>(); selection.effectColor = new Color32(28, 173, 133, 255); selection.effectDistance = new Vector2(3, -3);
            Button.targetGraphic = GetComponent<Image>();
            SetRect((RectTransform)transform, 0, 0, 590, 140);
            SetRect(Icon.rectTransform, 10, 10, 78, 78);
            SetRect(TypeBadge.rectTransform, 10, 94, 78, 18); SetRect(TraitBadge.rectTransform, 10, 116, 78, 18);
            ConfigureText(NameText, 102, 5, 472, 30, 25);
            ConfigureText(FlavorText, 102, 37, 472, 62, 19);
            ConfigureText(BpText, 102, 107, 108, 26, 20);
            ConfigureText(RangeText, 214, 107, 162, 26, 19);
            ConfigureText(AreaText, 380, 107, 196, 26, 19);
        }

        /// <summary>현재 소유자의 BP와 공격 정의를 프리팹 필드에 표시한다.</summary>
        /// <param name="actor">BP를 조회할 소유자.</param>
        /// <param name="skill">표시할 소유 스킬.</param>
        /// <param name="isSelected">현재 편집 선택 여부.</param>
        public void Bind(BattleUnit actor, MainSkill skill, bool isSelected)
        {
            // description: 직접 작성한 설명을 우선하고 없으면 실제 공격 데이터로 기본 안내를 만든다.
            string description = string.IsNullOrWhiteSpace(skill.EffectDescription)
                ? "적 1명에게 " + TypeName(skill.Type) + " 피해를 줍니다.\n위력 " + skill.Power + " · 명중 " + skill.Accuracy + "% · COST " + skill.Cost
                : skill.EffectDescription;
            // icon: 미정인 동안 비워 두고, 향후 Resources/SkillIcons/<스킬 ID>의 Sprite를 연결한다.
            var icon = Resources.Load<Sprite>("SkillIcons/" + skill.Id);
            Show(skill.Name, description, actor.SkillBp(skill.Id), skill.MaxBp,
                skill.MinRange == skill.MaxRange ? skill.MinRange + "칸" : skill.MinRange + "~" + skill.MaxRange + "칸",
                "단일(1명)", skill.Type, skill.Trait, icon, isSelected);
        }

        /// <summary>표시 데이터를 연결한다. 변화/광역 안내도 표현할 수 있지만 해당 전투 기능을 실행하지 않는다.</summary>
        /// <param name="skillName">스킬명.</param>
        /// <param name="description">효과 설명.</param>
        /// <param name="currentBp">현재 BP.</param>
        /// <param name="maxBp">최대 BP.</param>
        /// <param name="range">사거리 설명.</param>
        /// <param name="area">범위 설명.</param>
        /// <param name="type">스킬 타입.</param>
        /// <param name="trait">스킬 속성.</param>
        /// <param name="icon">미정이면 null.</param>
        /// <param name="isSelected">선택 강조 여부.</param>
        public void Show(string skillName, string description, int currentBp, int maxBp, string range, string area,
            SkillType type, CombatTrait trait, Sprite icon = null, bool isSelected = false)
        {
            Initialize();
            NameText.text = skillName; FlavorText.text = description; BpText.text = currentBp + "/" + maxBp;
            RangeText.text = "사거리 " + range; AreaText.text = "범위 " + area;
            TypeBadge.color = TypeColor(type); typeLabel.text = TypeName(type); typeLabel.color = Contrast(TypeBadge.color);
            TraitBadge.color = TraitColor(trait); traitLabel.text = TraitName(trait); traitLabel.color = Contrast(TraitBadge.color);
            Icon.sprite = icon; Icon.preserveAspect = true; Icon.color = icon == null ? new Color32(225, 229, 235, 255) : Color.white;
            iconPlaceholder.gameObject.SetActive(icon == null); selection.enabled = isSelected;
        }

        /// <summary>타입을 한국어로 표시한다.</summary>
        /// <param name="type">표시 타입.</param>
        public static string TypeName(SkillType type) => type == SkillType.Tech ? "태크" : type == SkillType.Mystic ? "신비" : type == SkillType.Change ? "변화" : throw new ArgumentOutOfRangeException(nameof(type));
        /// <summary>태크 주황, 신비 하늘, 변화 회색.</summary>
        /// <param name="type">표시 타입.</param>
        public static Color32 TypeColor(SkillType type) => type == SkillType.Tech ? traitColors[0] : type == SkillType.Mystic ? traitColors[5] : type == SkillType.Change ? new Color32(145, 145, 145, 255) : throw new ArgumentOutOfRangeException(nameof(type));
        /// <summary>11속성 중 해당 이름을 가져온다.</summary>
        /// <param name="trait">속성.</param>
        public static string TraitName(CombatTrait trait) => traitNames[TraitIndex(trait)];
        /// <summary>11속성 중 해당 배경색을 가져온다.</summary>
        /// <param name="trait">속성.</param>
        public static Color32 TraitColor(CombatTrait trait) => traitColors[TraitIndex(trait)];
        /// <summary>잘못된 속성을 조용히 다른 색으로 표시하지 않도록 검사한다.</summary>
        /// <param name="trait">속성.</param>
        private static int TraitIndex(CombatTrait trait) => Enum.IsDefined(typeof(CombatTrait), trait) ? (int)trait : throw new ArgumentOutOfRangeException(nameof(trait));
        /// <summary>짙은 배경에는 흰 글자, 밝은 배경에는 검은 글자를 사용한다.</summary>
        /// <param name="color">배경색.</param>
        private static Color Contrast(Color color) => .2126f * color.r + .7152f * color.g + .0722f * color.b < .52f ? Color.white : new Color32(28, 31, 38, 255);
        /// <summary>프리팹의 필수 자식을 찾아 참조 누락을 명확히 알린다.</summary>
        /// <typeparam name="T">필요 컴포넌트.</typeparam>
        /// <param name="child">직접 자식 이름.</param>
        private T Required<T>(string child) where T : Component => transform.Find(child)?.GetComponent<T>() ?? throw new InvalidOperationException("SkillSlot requires " + child);
        /// <summary>기존 폰트를 재사용해 색상 배경 안에 이름을 표시한다.</summary>
        /// <param name="parent">색상/아이콘 영역.</param>
        /// <param name="name">자식 이름.</param>
        private TMP_Text BadgeLabel(Transform parent, string name)
        {
            // label/rect: 배경 전체를 채우는 텍스트.
            var label = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.transform.SetParent(parent, false); label.font = NameText.font; label.fontSize = 18; label.color = new Color32(28, 31, 38, 255);
            label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false; label.richText = false;
            var rect = label.rectTransform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            return label;
        }
        /// <summary>프리팹 글자의 크기와 줄바꿈을 카드 영역에 맞춘다.</summary>
        /// <param name="text">텍스트.</param>
        /// <param name="x">왼쪽 거리.</param>
        /// <param name="y">위쪽 거리.</param>
        /// <param name="width">너비.</param>
        /// <param name="height">높이.</param>
        /// <param name="size">최대 글자 크기.</param>
        private static void ConfigureText(TMP_Text text, float x, float y, float width, float height, int size)
        {
            SetRect(text.rectTransform, x, y, width, height); text.fontSize = size;
            text.enableAutoSizing = true; text.fontSizeMin = 15; text.fontSizeMax = size;
            text.alignment = TextAlignmentOptions.MidlineLeft; text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis; text.richText = false; text.color = new Color32(32, 36, 44, 255);
        }
        /// <summary>루트와 자식을 좌상단 기준으로 배치한다.</summary>
        /// <param name="rect">대상 RectTransform.</param>
        /// <param name="x">왼쪽 거리.</param>
        /// <param name="y">위쪽 거리.</param>
        /// <param name="width">너비.</param>
        /// <param name="height">높이.</param>
        private static void SetRect(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height); rect.localScale = Vector3.one;
        }
    }
}
