using System;
using System.Collections.Generic;
using MaseiKivotos.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MaseiKivotos.Unity
{
    /// <summary>프리팹 배치를 보존하며 스킬 데이터와 설명용 도식을 표시한다. 전투 판정은 하지 않는다.</summary>
    public sealed class SkillSlotView : MonoBehaviour
    {
        /// <summary>Resources의 스킬 카드 경로.</summary>
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
        /// <summary>선택 입력을 받는 루트 버튼.</summary>
        [SerializeField] private Button button;
        /// <summary>이름, COST, BP, 사거리, 범위, 효과 설명의 Inspector 연결.</summary>
        [SerializeField] private TMP_Text nameText, costText, bpText, rangeText, areaText, flavorText;
        /// <summary>9칸 도식에 전체 구간을 표현할 수 없을 때만 표시하는 안내.</summary>
        [SerializeField] private TMP_Text areaOverText, rangeOverText;
        /// <summary>IconText, TypeText, TraitText의 이미지 영역.</summary>
        [SerializeField] private Image icon, typeBadge, traitBadge;
        /// <summary>타입명, 속성명, 미지정 아이콘 안내.</summary>
        [SerializeField] private TMP_Text typeLabel, traitLabel, iconPlaceholder;
        /// <summary>선택 외곽선. 외형은 Inspector에서 편집한다.</summary>
        [SerializeField] private Outline selection;
        /// <summary>왼쪽부터 1~9번 효과 범위와 사거리 이미지.</summary>
        [SerializeField] private Image[] areaSlots = new Image[9], rangeSlots = new Image[9];
        /// <summary>효과 없음 색.</summary>
        [SerializeField] private Color inactiveColor = new Color32(128, 128, 128, 128);
        /// <summary>목표 위치 색.</summary>
        [SerializeField] private Color targetColor = new Color32(218, 65, 65, 255);
        /// <summary>추가 영향 범위 색.</summary>
        [SerializeField] private Color areaColor = new Color32(245, 148, 55, 255);
        /// <summary>사용자 위치 색.</summary>
        [SerializeField] private Color userColor = new Color32(72, 195, 112, 255);
        /// <summary>사용 가능 사거리 색.</summary>
        [SerializeField] private Color rangeColor = new Color32(135, 206, 235, 255);
        /// <summary>에디터 도식 예제. 실제 스킬 데이터나 전투 규칙을 변경하지 않는다.</summary>
        [SerializeField, Min(0)] private int previewMinRange = 3, previewMaxRange = 6;
        /// <summary>에디터 도식의 목표 기준 영향 범위 예제.</summary>
        [SerializeField] private int previewAreaStart = -1, previewAreaEnd = 1;
        /// <summary>카드 선택 버튼.</summary>
        public Button Button => button;
        /// <summary>스킬명.</summary>
        public TMP_Text NameText => nameText;
        /// <summary>공유 COST 비용. BP와 별개다.</summary>
        public TMP_Text CostText => costText;
        /// <summary>현재/최대 BP.</summary>
        public TMP_Text BpText => bpText;
        /// <summary>사거리 설명.</summary>
        public TMP_Text RangeText => rangeText;
        /// <summary>목표 기준 효과 범위 설명.</summary>
        public TMP_Text AreaText => areaText;
        /// <summary>효과 범위가 도식 밖으로 나가는 경우의 안내.</summary>
        public TMP_Text AreaOverText => areaOverText;
        /// <summary>사거리 구간이 도식 밖으로 나가는 경우의 안내.</summary>
        public TMP_Text RangeOverText => rangeOverText;
        /// <summary>FlavorTextBox의 Scroll View 안에 있는 효과 설명.</summary>
        public TMP_Text FlavorText => flavorText;
        /// <summary>스킬 이미지.</summary>
        public Image Icon => icon;
        /// <summary>타입 색상 배경.</summary>
        public Image TypeBadge => typeBadge;
        /// <summary>속성 색상 배경.</summary>
        public Image TraitBadge => traitBadge;
        /// <summary>범위 도식의 9개 칸.</summary>
        public IReadOnlyList<Image> AreaSlots => areaSlots;
        /// <summary>사거리 도식의 9개 칸.</summary>
        public IReadOnlyList<Image> RangeSlots => rangeSlots;

        /// <summary>작성된 프리팹을 복제하고 선택 콜백만 연결한다.</summary>
        /// <param name="prefab">카드 원본.</param>
        /// <param name="parent">카드를 배치할 부모.</param>
        /// <param name="selected">스킬 선택 콜백.</param>
        public static SkillSlotView Create(GameObject prefab, Transform parent, Action selected)
        {
            if (prefab == null) throw new InvalidOperationException("SkillSlot prefab not found at Resources/" + PrefabPath);
            var instance = Instantiate(prefab, parent, false); // instance: 작성된 크기와 자식 배치를 유지한다.
            var view = instance.GetComponent<SkillSlotView>();
            if (view == null) throw new InvalidOperationException("SkillSlot prefab needs serialized SkillSlotView references.");
            view.ValidateReferences();
            if (selected != null) view.button.onClick.AddListener(() => selected());
            return view;
        }

        /// <summary>누락된 Inspector 연결을 명확한 오류로 알린다.</summary>
        public void ValidateReferences()
        {
            if (button == null || nameText == null || costText == null || bpText == null || rangeText == null || areaText == null || flavorText == null
                || areaOverText == null || rangeOverText == null || icon == null || typeBadge == null || traitBadge == null || typeLabel == null || traitLabel == null || iconPlaceholder == null || selection == null)
                throw new InvalidOperationException("SkillSlot text/image/button references are incomplete.");
            if (areaSlots.Length != 9 || rangeSlots.Length != 9) throw new InvalidOperationException("SkillSlot needs nine area and nine range slots.");
            for (int i = 0; i < 9; i++)
                if (areaSlots[i] == null || rangeSlots[i] == null) throw new InvalidOperationException("SkillSlot has an unassigned diagram slot: " + (i + 1));
        }

        /// <summary>현재 일반 단일 공격의 데이터와 소유자의 BP를 표시한다.</summary>
        /// <param name="actor">스킬 소유자.</param>
        /// <param name="skill">표시할 공격.</param>
        /// <param name="isSelected">선택 강조 여부.</param>
        public void Bind(BattleUnit actor, MainSkill skill, bool isSelected)
        {
            string description = string.IsNullOrWhiteSpace(skill.EffectDescription) // description: 작성된 설명 우선.
                ? "적 1명에게 " + TypeName(skill.Type) + " 피해를 줍니다.\n위력 " + skill.Power + " · 명중 " + skill.Accuracy + "% · COST " + skill.Cost
                : skill.EffectDescription;
            // 일반 MainSkill은 현재 단일 대상이다. 도식 때문에 광역 전투 판정을 추가하지 않는다.
            Show(skill.Name, description, actor.SkillBp(skill.Id), skill.MaxBp, skill.Cost,
                skill.MinRange, skill.MaxRange, 0, 0, skill.Type, skill.Trait,
                Resources.Load<Sprite>("SkillIcons/" + skill.Id), isSelected);
        }

        /// <summary>수치로 문구와 도식을 함께 갱신한다. 광역 표시는 전투 광역 실행 지원을 뜻하지 않는다.</summary>
        /// <param name="skillName">스킬명.</param>
        /// <param name="description">효과 설명.</param>
        /// <param name="currentBp">현재 BP.</param>
        /// <param name="maxBp">최대 BP.</param>
        /// <param name="cost">공유 COST 비용.</param>
        /// <param name="minRange">최소 거리.</param>
        /// <param name="maxRange">최대 거리.</param>
        /// <param name="areaStart">목표 기준 영향 범위의 시작 오프셋.</param>
        /// <param name="areaEnd">목표 기준 영향 범위의 끝 오프셋.</param>
        /// <param name="type">스킬 타입.</param>
        /// <param name="trait">속성.</param>
        /// <param name="sprite">아이콘. 미지정이면 null.</param>
        /// <param name="isSelected">선택 강조 여부.</param>
        public void Show(string skillName, string description, int currentBp, int maxBp, int cost,
            int minRange, int maxRange, int areaStart, int areaEnd, SkillType type, CombatTrait trait,
            Sprite sprite = null, bool isSelected = false)
        {
            ValidateReferences();
            ShowDiagram(minRange, maxRange, areaStart, areaEnd);
            nameText.text = skillName; flavorText.text = description; bpText.text = "BP " + currentBp + "/" + maxBp;
            costText.text = "Cost " + cost;
            typeBadge.color = TypeColor(type); typeLabel.text = TypeName(type); typeLabel.color = Contrast(typeBadge.color);
            traitBadge.color = TraitColor(trait); traitLabel.text = TraitName(trait); traitLabel.color = Contrast(traitBadge.color);
            icon.sprite = sprite; iconPlaceholder.gameObject.SetActive(sprite == null); selection.enabled = isSelected;
        }

        /// <summary>목표 5번과 사용자 1번 기준 도식을 표시한다. 잘린 구간은 Over 안내를 켜고 원래 수치를 텍스트에 남긴다.</summary>
        /// <param name="minRange">최소 거리.</param>
        /// <param name="maxRange">최대 거리.</param>
        /// <param name="areaStart">목표 기준 시작 오프셋.</param>
        /// <param name="areaEnd">목표 기준 끝 오프셋.</param>
        public void ShowDiagram(int minRange, int maxRange, int areaStart, int areaEnd)
        {
            if (minRange < 0 || minRange > maxRange || areaStart > areaEnd)
                throw new ArgumentOutOfRangeException(nameof(minRange), "Invalid skill diagram interval.");
            rangeText.text = "Range " + Interval(minRange, maxRange);
            areaText.text = "Area " + Interval(areaStart, areaEnd);
            // 일부만 잘리는 경우와 전체가 보이지 않는 경우 모두 안내한다. 전투 Core의 사거리 제한은 바꾸지 않는다.
            areaOverText.gameObject.SetActive(areaStart < -4 || areaEnd > 4);
            rangeOverText.gameObject.SetActive(maxRange > 8);
            for (int i = 0; i < 9; i++)
            {
                int offset = i - 4; // offset: 목표 5번 기준 상대 칸. i는 사용자 1번과의 거리다.
                areaSlots[i].color = i == 4 ? targetColor : offset >= areaStart && offset <= areaEnd ? areaColor : inactiveColor;
                rangeSlots[i].color = i == 0 ? userColor : i >= minRange && i <= maxRange ? rangeColor : inactiveColor;
            }
        }

        /// <summary>Inspector의 예제 값으로 도식만 미리 본다. 전투 중에는 호출하지 않는다.</summary>
        public void PreviewDiagram()
        {
            ValidateReferences();
            int min = Math.Min(previewMinRange, previewMaxRange), max = Math.Max(previewMinRange, previewMaxRange);
            int start = Math.Min(previewAreaStart, previewAreaEnd), end = Math.Max(previewAreaStart, previewAreaEnd);
            ShowDiagram(min, max, start, end);
        }
        /// <summary>같은 끝점은 하나의 값, 나머지는 시작~끝으로 표시한다.</summary>
        /// <param name="start">시작값.</param>
        /// <param name="end">끝값.</param>
        private static string Interval(int start, int end) => start == end ? start.ToString() : start + "~" + end;
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
    }
}
