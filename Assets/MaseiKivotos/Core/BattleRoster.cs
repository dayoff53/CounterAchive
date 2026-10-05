using System;
using System.Collections.Generic;
using System.Linq;

namespace MaseiKivotos.Core
{
    /// <summary>
    /// 전투 중 유닛의 소속 영역. 필드·대기·퇴각을 구분하며 대기 복귀와 퇴각은 다른 상태다.
    /// </summary>
    public enum UnitLocation
    {
        /// <summary>
        /// 현재 9칸 전투 필드에 배치된 상태.
        /// </summary>
        Field,
        /// <summary>
        /// 편성에는 포함되지만 필드에 출전하지 않은 대기 상태.
        /// </summary>
        Reserve,
        /// <summary>
        /// 전투에서 퇴각한 상태. 현재 전투의 행동 순서와 필드 인원에서 제외한다.
        /// </summary>
        Retreated
    }

    /// <summary>
    /// 유닛 정의에 보관하는 기본 능력치. 현재 HP는 BattleUnit에 보관하며 능력치 버프는 아직 미구현이다.
    /// </summary>
    public sealed class CombatStats
    {
        /// <summary>
        /// 유닛 정의의 기본 HP. 생성 시 양수여야 하며 현재 체력이나 피해 누적값은 아니다.
        /// </summary>
        public int Hp { get; }
        /// <summary>
        /// 태크 계열 공격력. 태크 스킬 피해 계산에 사용한다.
        /// </summary>
        public int TAtk { get; }
        /// <summary>
        /// 신비 계열 공격력. 신비 스킬 피해 계산에 사용한다.
        /// </summary>
        public int MAtk { get; }
        /// <summary>
        /// 태크 계열 방어력. 0 이상을 허용한다.
        /// </summary>
        public int TDef { get; }
        /// <summary>
        /// 신비 계열 방어력. 0 이상을 허용한다.
        /// </summary>
        public int MDef { get; }
        /// <summary>
        /// 공통 행동 순서를 정하는 속도. 높은 값부터 차례를 배정하며 0도 허용한다.
        /// </summary>
        public int Speed { get; }

        /// <summary>
        /// HP는 양수, 나머지 능력치는 0 이상인지 검사한 뒤 기본 능력치를 저장한다.
        /// </summary>
        /// <param name="hp">양수인 기본 HP.</param>
        /// <param name="tAtk">0 이상인 태크 공격력.</param>
        /// <param name="mAtk">0 이상인 신비 공격력.</param>
        /// <param name="tDef">0 이상인 태크 방어력.</param>
        /// <param name="mDef">0 이상인 신비 방어력.</param>
        /// <param name="speed">0 이상인 기본 Speed.</param>
        public CombatStats(int hp, int tAtk, int mAtk, int tDef, int mDef, int speed)
        {
            if (hp <= 0 || tAtk < 0 || mAtk < 0 || tDef < 0 || mDef < 0 || speed < 0)
                throw new ArgumentOutOfRangeException(nameof(hp), "Invalid combat stats.");
            Hp = hp; TAtk = tAtk; MAtk = mAtk; TDef = tDef; MDef = mDef; Speed = speed;
        }
    }

    /// <summary>
    /// 한 유닛의 식별자·기본 능력치·행동 횟수·스킬·속성을 묶은 정의 데이터.
    /// </summary>
    public sealed class UnitDefinition
    {
        /// <summary>
        /// 전투 전체에서 유닛을 식별하는 고유 문자열. 화면 이름과 독립적으로 사용한다.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// UI와 진행 기록에 표시할 유닛 이름.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// 턴 순서와 공격 피해 계산에 사용할 기본 능력치.
        /// </summary>
        public CombatStats Stats { get; }
        /// <summary>
        /// 턴 초기화 때 필드 유닛에 지급할 기본 서브 행동 횟수.
        /// </summary>
        public int BaseSubActions { get; }
        /// <summary>한 번의 자발적 이동에서 허용하는 최대 칸 수. 콘텐츠에서 지정하며 0이면 이동할 수 없다.</summary>
        public int MoveRange { get; }

        /// <summary>최대 네 개의 일반 공격 정의. 빈 목록이면 턴 종료만 사용할 수 있다.</summary>
        public IReadOnlyList<MainSkill> Skills { get; }
        /// <summary>유닛 속성 한두 개. 기존 호출에서 생략하면 임시 무속성으로 구성한다.</summary>
        public IReadOnlyList<CombatTrait> Traits { get; }

        /// <summary>
        /// 식별자·이름·능력치와 서브 행동 횟수를 검증해 유닛 정의를 만든다.
        /// </summary>
        /// <param name="id">전투에서 유닛을 식별할 고유 ID.</param>
        /// <param name="name">UI와 진행 기록에 표시할 유닛 이름.</param>
        /// <param name="stats">유닛의 기본 능력치 정의.</param>
        /// <param name="baseSubActions">턴마다 초기화할 기본 서브 행동 횟수.</param>
        /// <param name="skills">최대 네 개의 중복 없는 일반 스킬. 생략하면 빈 목록.</param>
        /// <param name="traits">중복 없는 한두 속성. 생략하면 무.</param>
        /// <param name="moveRange">유닛별 이동 거리 0~8. 기존 정의에서 생략하면 이동하지 않는다.</param>
        public UnitDefinition(string id, string name, CombatStats stats, int baseSubActions = 1,
            IEnumerable<MainSkill> skills = null, IEnumerable<CombatTrait> traits = null, int moveRange = 0)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A stable ID and a display name are required.");
            if (baseSubActions < 0) throw new ArgumentOutOfRangeException(nameof(baseSubActions));
            if (moveRange < 0 || moveRange > 8) throw new ArgumentOutOfRangeException(nameof(moveRange));
            Id = id; Name = name; Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            BaseSubActions = baseSubActions;
            MoveRange = moveRange;
            // skillList/traitList: 외부 배열 수정에 영향받지 않도록 복사한 정의 목록.
            var skillList = (skills ?? Enumerable.Empty<MainSkill>()).ToArray();
            var traitList = (traits ?? new[] { CombatTrait.Neutral }).ToArray();
            if (skillList.Length > 4 || skillList.Any(s => s == null) || skillList.Select(s => s.Id).Distinct().Count() != skillList.Length)
                throw new ArgumentException("At most four unique skills required.");
            if (traitList.Length < 1 || traitList.Length > 2 || traitList.Distinct().Count() != traitList.Length || traitList.Any(t => !Enum.IsDefined(typeof(CombatTrait), t)))
                throw new ArgumentException("One or two distinct traits required.");
            Skills = Array.AsReadOnly(skillList); Traits = Array.AsReadOnly(traitList);
        }
    }

    /// <summary>
    /// 한 세력의 편성 순서와 스테이지 출전 구역. 현재 구현은 편성 2~6명과 출전 슬롯 2칸 이상을 요구한다.
    /// </summary>
    public sealed class BattleRoster
    {
        /// <summary>
        /// 유닛 또는 편성이 속한 세력의 식별 번호. 전투의 COST도 이 번호로 구분한다.
        /// </summary>
        public int FactionId { get; }
        /// <summary>
        /// 편성 순서대로 보관한 유닛 정의. 시작 시 앞의 두 명이 출전한다.
        /// </summary>
        public IReadOnlyList<UnitDefinition> Units { get; }
        /// <summary>
        /// 1~9 범위의 중복 없는 출전 슬롯 번호. 처음 두 칸은 시작 유닛 두 명의 배치 위치다.
        /// </summary>
        public IReadOnlyList<int> DeploymentSlots { get; }

        /// <summary>
        /// 입력 편성과 출전 구역을 복사·검증해 외부 목록 변경이 전투 구성에 영향을 주지 않게 한다.
        /// </summary>
        /// <param name="factionId">해당 편성 또는 유닛이 속할 세력 ID.</param>
        /// <param name="units">편성 순서대로 제공하는 유닛 정의 목록.</param>
        /// <param name="deploymentSlots">1~9 사이의 중복 없는 출전 구역. 앞 두 칸에 시작 유닛을 놓는다.</param>
        public BattleRoster(int factionId, IEnumerable<UnitDefinition> units, IEnumerable<int> deploymentSlots)
        {
            // party: 입력 편성을 복사한 배열. 외부 목록 수정의 영향을 막고 시작 배치 순서를 유지한다.
            var party = units?.ToArray() ?? throw new ArgumentNullException(nameof(units));
            // slots: 출전 구역 입력을 복사한 배열. 범위와 중복을 검사한 뒤 편성에 보관한다.
            var slots = deploymentSlots?.ToArray() ?? throw new ArgumentNullException(nameof(deploymentSlots));

            if (party.Length < 2 || party.Length > 6 || party.Any(u => u == null))
                throw new ArgumentException("This battle requires a roster of 2 to 6 units.");
            if (slots.Length < 2 || slots.Any(s => s < 1 || s > 9) || slots.Distinct().Count() != slots.Length)
                throw new ArgumentException("Deployment slots must be distinct positions in 1..9.");
            FactionId = factionId;
            Units = Array.AsReadOnly(party);
            DeploymentSlots = Array.AsReadOnly(slots);
        }
    }

    /// <summary>
    /// 전투 한 판에서 변하는 유닛의 HP·BP·배치 상태와 남은 행동 횟수. 정의 데이터와 분리한다.
    /// </summary>
    public sealed class BattleUnit
    {
        /// <summary>유닛마다 독립적으로 유지하는 스킬별 현재 BP. 턴마다 초기화하지 않는다.</summary>
        private readonly Dictionary<string, int> skillBp = new Dictionary<string, int>();
        /// <summary>이번 전투의 현재 HP. 0이면 퇴각하며 다음 턴에도 회복되지 않는다.</summary>
        public int CurrentHp { get; internal set; }
        /// <summary>헤일로 이상 자체의 BP. 일반 스킬 BP와 독립적이며 사용 후 완충한다.</summary>
        public int HaloBp { get; internal set; } = HaloAnomaly.MaxBp;
        /// <summary>외부 효과가 일반 메인 스킬 사용을 금지했는지 여부. 지속 시간 처리는 후속 상태 시스템의 책임이다.</summary>
        public bool IsMainSkillBlocked { get; internal set; }
        /// <summary>자발적 이동 시도 후 부여하는 이동 불가 잔여 횟수. 자신의 턴 종료에서만 1 감소한다.</summary>
        public int MovementLockTurns { get; internal set; }
        /// <summary>외부 상태 효과의 자발적 이동 금지 여부. 메인 스킬 금지와 독립적이다.</summary>
        public bool IsMovementBlocked { get; internal set; }
        /// <summary>현재 스킬 BP를 읽는다. 알 수 없는 스킬 ID는 거부한다.</summary>
        /// <param name="skillId">이 유닛이 가진 스킬 ID.</param>
        public int SkillBp(string skillId) => skillBp.TryGetValue(skillId, out int value) ? value : throw new ArgumentException("Unknown skill: " + skillId);
        /// <summary>Core 효과 처리에서만 BP를 설정한다. 0~최대 범위를 벗어나는 값은 거부한다.</summary>
        /// <param name="skillId">소유 스킬 ID.</param>
        /// <param name="value">새 BP.</param>
        internal void SetSkillBp(string skillId, int value)
        {
            // skill: BP 상한을 제공하는 소유 스킬 정의.
            var skill = Definition.Skills.FirstOrDefault(s => s.Id == skillId) ?? throw new ArgumentException("Unknown skill.");
            if (value < 0 || value > skill.MaxBp) throw new ArgumentOutOfRangeException(nameof(value));
            skillBp[skillId] = value;
        }
        /// <summary>
        /// 이 전투 유닛의 이름·기본 능력치·서브 횟수를 제공하는 정의.
        /// </summary>
        public UnitDefinition Definition { get; }
        /// <summary>
        /// 전투 전체에서 유닛을 식별하는 고유 문자열. 화면 이름과 독립적으로 사용한다.
        /// </summary>
        public string Id => Definition.Id;
        /// <summary>
        /// 유닛 또는 편성이 속한 세력의 식별 번호. 전투의 COST도 이 번호로 구분한다.
        /// </summary>
        public int FactionId { get; }
        /// <summary>
        /// 현재 필드 슬롯 번호(1~9). 대기 또는 퇴각 상태에서는 null이다.
        /// </summary>
        public int? Slot { get; internal set; }
        /// <summary>
        /// 유닛이 현재 필드·대기·퇴각 중 어디에 속하는지 나타내는 상태.
        /// </summary>
        public UnitLocation Location { get; internal set; }
        /// <summary>
        /// 현재 턴에 남은 메인 행동 횟수. 턴 초기화 시 필드 유닛에 1회를 지급하고 차례 종료 시 남은 횟수를 버린다.
        /// </summary>
        public int RemainingMainActions { get; internal set; }
        /// <summary>
        /// 현재 턴에 남은 서브 행동 횟수. 기본 횟수로 초기화하며 사용하지 않은 횟수는 이월하지 않는다.
        /// </summary>
        public int RemainingSubActions { get; internal set; }

        /// <summary>
        /// 유닛 정의와 세력을 연결하고 슬롯 유무에 따라 필드 또는 대기 상태로 시작한다.
        /// </summary>
        /// <param name="definition">전투 유닛의 원본 정의 데이터.</param>
        /// <param name="factionId">해당 편성 또는 유닛이 속할 세력 ID.</param>
        /// <param name="slot">시작 필드 슬롯 번호. null이면 대기 상태로 시작한다.</param>
        internal BattleUnit(UnitDefinition definition, int factionId, int? slot)
        {
            Definition = definition; FactionId = factionId; Slot = slot;
            Location = slot.HasValue ? UnitLocation.Field : UnitLocation.Reserve;
            CurrentHp = definition.Stats.Hp;
            foreach (var skill in definition.Skills) skillBp.Add(skill.Id, skill.MaxBp);
        }
    }

    /// <summary>
    /// 동률 순서 추첨에 사용할 난수 공급 계약. 테스트에서는 정해진 값을 반환하는 구현으로 교체할 수 있다.
    /// </summary>
    public interface ITurnRandom
    {
        /// <summary>
        /// 0 이상 exclusiveMaximum 미만의 정수를 반환한다.
        /// </summary>
        /// <param name="exclusiveMaximum">난수 범위의 제외 상한. 반환값은 이 값보다 작다.</param>
        int Next(int exclusiveMaximum);
    }

    /// <summary>
    /// 같은 시드에서 같은 난수열을 재현하는 전투용 난수 공급기.
    /// </summary>
    public sealed class SeededTurnRandom : ITurnRandom
    {
        /// <summary>
        /// 시드로 초기화한 난수 생성기. 턴마다 새로 만들지 않고 이어서 추첨한다.
        /// </summary>
        private readonly Random random;
        /// <summary>
        /// 재현 가능한 난수열을 주어진 시드로 초기화한다.
        /// </summary>
        /// <param name="seed">난수열을 재현할 초기 시드.</param>
        public SeededTurnRandom(int seed) { random = new Random(seed); }
        /// <summary>
        /// 상한을 제외한 범위에서 다음 난수를 뽑는다.
        /// </summary>
        /// <param name="exclusiveMaximum">난수 범위의 제외 상한. 반환값은 이 값보다 작다.</param>
        public int Next(int exclusiveMaximum) => random.Next(exclusiveMaximum);
    }
}
