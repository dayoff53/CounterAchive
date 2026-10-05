using System;
using System.Linq;

namespace MaseiKivotos.Core
{
    /// <summary>스킬 분류. 변화의 표시 값은 제공하지만 전투 효과 실행은 아직 지원하지 않는다.</summary>
    public enum SkillType
    {
        /// <summary>TAtk/TDef로 계산하는 태크 공격.</summary>
        Tech = 0,
        /// <summary>MAtk/MDef로 계산하는 신비 공격.</summary>
        Mystic = 1,
        /// <summary>변화 계열의 표시용 값. 일반 공격 정의로 생성할 수 없다.</summary>
        Change = 2
    }

    /// <summary>규칙 원문의 11속성. 분류(Type)와 별개이며 숫자 값을 고정한다.</summary>
    public enum CombatTrait
    {
        /// <summary>염.</summary>
        Flame = 0,
        /// <summary>습.</summary>
        Wet = 1,
        /// <summary>전.</summary>
        Electric = 2,
        /// <summary>폭.</summary>
        Blast = 3,
        /// <summary>금.</summary>
        Metal = 4,
        /// <summary>신.</summary>
        Divine = 5,
        /// <summary>성.</summary>
        Holy = 6,
        /// <summary>암.</summary>
        Dark = 7,
        /// <summary>순.</summary>
        Pure = 8,
        /// <summary>사.</summary>
        Death = 9,
        /// <summary>무.</summary>
        Neutral = 10
    }

    /// <summary>유닛 지정·적 단일 공격의 불변 정의. 수치는 콘텐츠별로 입력한다.</summary>
    public sealed class MainSkill
    {
        /// <summary>유닛 내에서 중복되지 않는 스킬 식별자.</summary>
        public string Id { get; }
        /// <summary>화면과 결과 기록에 표시하는 이름.</summary>
        public string Name { get; }
        /// <summary>피해 계산 능력치를 선택하는 분류.</summary>
        public SkillType Type { get; }
        /// <summary>상성 및 자속 보정에 사용하는 속성.</summary>
        public CombatTrait Trait { get; }
        /// <summary>공격력/방어력 비율에 곱하는 위력. 중간 올림은 하지 않는다.</summary>
        public decimal Power { get; }
        /// <summary>0~100의 정수 명중률.</summary>
        public int Accuracy { get; }
        /// <summary>이 스킬의 최대 BP. 한 번의 시도에 1을 사용한다.</summary>
        public int MaxBp { get; }
        /// <summary>실행 성공 또는 명중 실패 시 소비할 공유 COST.</summary>
        public int Cost { get; }
        /// <summary>포함되는 최소 슬롯 거리.</summary>
        public int MinRange { get; }
        /// <summary>포함되는 최대 슬롯 거리.</summary>
        public int MaxRange { get; }
        /// <summary>작성된 스킬 효과 설명. 비어 있으면 UI가 현재 공격 수치로 설명을 만든다.</summary>
        public string EffectDescription { get; }

        /// <summary>지원하는 공격 정의의 값과 수치 범위를 검사한다.</summary>
        /// <param name="id">유닛 내 고유 ID.</param>
        /// <param name="name">표시 이름.</param>
        /// <param name="type">태크 또는 신비.</param>
        /// <param name="trait">공격 속성.</param>
        /// <param name="power">0 이상 int 최대값 이하의 위력. 계산 오버플로 방지를 위한 데이터 범위다.</param>
        /// <param name="accuracy">0~100 명중률.</param>
        /// <param name="maxBp">양수인 스킬별 최대 BP.</param>
        /// <param name="cost">0~10 공유 COST 비용.</param>
        /// <param name="minRange">0~8 최소 거리.</param>
        /// <param name="maxRange">최소 거리 이상, 8 이하 최대 거리.</param>
        /// <param name="effectDescription">효과 안내 문구. 생략 시 표시 계층에서 기본 설명을 사용한다.</param>
        public MainSkill(string id, string name, SkillType type, CombatTrait trait, decimal power,
            int accuracy, int maxBp, int cost = 0, int minRange = 0, int maxRange = 8, string effectDescription = null)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Skill ID and name required.");
            if (!Enum.IsDefined(typeof(SkillType), type) || !Enum.IsDefined(typeof(CombatTrait), trait)) throw new ArgumentException("Unknown skill type or trait.");
            if (type == SkillType.Change) throw new NotSupportedException("Change skills have display support only; their battle effects are not implemented.");
            if (power < 0 || power > int.MaxValue || accuracy < 0 || accuracy > 100 || maxBp < 1 || cost < 0 || cost > 10 || minRange < 0 || maxRange > 8 || minRange > maxRange)
                throw new ArgumentOutOfRangeException(nameof(power), "Invalid skill data.");
            Id = id; Name = name; Type = type; Trait = trait; Power = power;
            Accuracy = accuracy; MaxBp = maxBp; Cost = cost; MinRange = minRange; MaxRange = maxRange;
            EffectDescription = effectDescription ?? string.Empty;
        }
    }

    /// <summary>메인 공격 한 개와 대상의 예약. 종료 예약은 TurnBattle이 별도로 관리한다.</summary>
    public sealed class MainSkillReservation
    {
        /// <summary>예약자가 가진 원래 스킬 ID.</summary>
        public string SkillId { get; }
        /// <summary>실행 시에도 추적하는 대상 유닛 ID.</summary>
        public string TargetId { get; }
        /// <summary>예약 확정 순간 원래 스킬 BP가 0이었는지 여부.</summary>
        public bool ZeroBpAtConfirmation { get; internal set; }
        /// <summary>스킬·대상을 고정한 새 예약을 만든다.</summary>
        /// <param name="skillId">스킬 ID.</param>
        /// <param name="targetId">대상 ID.</param>
        internal MainSkillReservation(string skillId, string targetId) { SkillId = skillId; TargetId = targetId; }
    }

    /// <summary>BP 부족으로만 발동하는 대체 스킬의 고정 정의. 일반 소유 스킬 목록에는 넣지 않는다.</summary>
    public static class HaloAnomaly
    {
        /// <summary>원문에서 정한 자체 BP 상한. 사용 후 이 값까지 완충한다.</summary>
        public const int MaxBp = 40;
        /// <summary>범위 내 각 유닛의 명중·피해 계산에 사용하는 무속성 신비 공격.</summary>
        internal static MainSkill Attack { get; } = new MainSkill("halo-anomaly", "헤일로 이상", SkillType.Mystic, CombatTrait.Neutral, 50, 80, MaxBp, 0, 0, 0);
    }

    /// <summary>원문 상성표와 단일 최종 올림을 적용하는 순수 피해 계산.</summary>
    public static class SkillDamage
    {
        /// <summary>공격 속성별 강세 대상. 역방향 관계를 자동 생성하지 않는다.</summary>
        private static readonly CombatTrait[][] strengths = {
            new[] { CombatTrait.Electric, CombatTrait.Metal }, // 염 → 전·금
            new[] { CombatTrait.Flame, CombatTrait.Blast }, // 습 → 염·폭
            new[] { CombatTrait.Wet, CombatTrait.Divine }, // 전 → 습·신
            new[] { CombatTrait.Metal, CombatTrait.Flame }, // 폭 → 금·염
            new[] { CombatTrait.Divine, CombatTrait.Electric }, // 금 → 신·전
            new[] { CombatTrait.Blast, CombatTrait.Wet }, // 신 → 폭·습
            new[] { CombatTrait.Dark, CombatTrait.Death }, // 성 → 암·사
            new[] { CombatTrait.Holy, CombatTrait.Death }, // 암 → 성·사
            new[] { CombatTrait.Holy, CombatTrait.Dark, CombatTrait.Neutral }, // 순 → 성·암·무
            new[] { CombatTrait.Pure }, // 사 → 순
            Array.Empty<CombatTrait>() // 무: 강세 없음
        };
        /// <summary>공격 속성별 약세 대상. 순/무와 성/암 예외도 표 그대로 보관한다.</summary>
        private static readonly CombatTrait[][] weaknesses = {
            new[] { CombatTrait.Wet, CombatTrait.Blast }, // 염 → 습·폭
            new[] { CombatTrait.Electric, CombatTrait.Divine }, // 습 → 전·신
            new[] { CombatTrait.Flame, CombatTrait.Metal }, // 전 → 염·금
            new[] { CombatTrait.Wet, CombatTrait.Divine }, // 폭 → 습·신
            new[] { CombatTrait.Flame, CombatTrait.Blast }, // 금 → 염·폭
            new[] { CombatTrait.Electric, CombatTrait.Metal }, // 신 → 전·금
            new[] { CombatTrait.Pure }, // 성 → 순
            new[] { CombatTrait.Pure }, // 암 → 순
            new[] { CombatTrait.Flame, CombatTrait.Wet, CombatTrait.Electric, CombatTrait.Blast, CombatTrait.Metal, CombatTrait.Divine, CombatTrait.Death }, // 순 → 6속성·사
            new[] { CombatTrait.Holy, CombatTrait.Dark }, // 사 → 성·암
            Array.Empty<CombatTrait>() // 무: 약세 없음
        };

        /// <summary>공격/방어 속성 한 쌍의 2·1·0.5 배율을 반환한다.</summary>
        /// <param name="attack">공격 속성.</param>
        /// <param name="defense">방어 속성.</param>
        public static decimal Matchup(CombatTrait attack, CombatTrait defense)
        {
            if (!Enum.IsDefined(typeof(CombatTrait), attack) || !Enum.IsDefined(typeof(CombatTrait), defense)) throw new ArgumentException("Unknown trait.");
            return strengths[(int)attack].Contains(defense) ? 2m : weaknesses[(int)attack].Contains(defense) ? .5m : 1m;
        }

        /// <summary>상성 곱을 0.5~2로 제한하고 자속을 별도로 곱한 후 최종 한 번만 올림한다.</summary>
        /// <param name="attacker">공격 유닛 정의.</param>
        /// <param name="defender">피격 유닛 정의.</param>
        /// <param name="skill">계산할 공격 정의.</param>
        public static int Calculate(UnitDefinition attacker, UnitDefinition defender, MainSkill skill)
        {
            if (attacker == null || defender == null || skill == null) throw new ArgumentNullException(nameof(skill));
            // multiplier: 대상의 속성별 상성을 곱한 뒤 제한할 배율.
            decimal multiplier = 1m;
            foreach (var trait in defender.Traits) multiplier *= Matchup(skill.Trait, trait);
            multiplier = Math.Max(.5m, Math.Min(2m, multiplier));
            // attack/defense: Type에 따른 원래 능력치. 방어력 원본은 변경하지 않는다.
            decimal attack = skill.Type == SkillType.Tech ? attacker.Stats.TAtk : attacker.Stats.MAtk;
            decimal defense = skill.Type == SkillType.Tech ? defender.Stats.TDef : defender.Stats.MDef;
            // damage: 중간 절삭 없는 최종 피해. int HP를 넘는 값은 표현 상한으로 제한한다.
            decimal damage = Math.Ceiling(attack / Math.Max(1m, defense) * skill.Power * multiplier * (attacker.Traits.Contains(skill.Trait) ? 1.5m : 1m));
            return (int)Math.Min(int.MaxValue, damage);
        }
    }
}
