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
    /// 유닛 정의에 보관하는 변경 불가능한 기본 능력치. 현재 HP나 버프에 의한 능력치 변화는 아직 별도로 구현하지 않았다.
    /// </summary>
    public sealed class CombatStats
    {
        /// <summary>
        /// 유닛 정의의 기본 HP. 생성 시 양수여야 하며 현재 체력이나 피해 누적값은 아니다.
        /// </summary>
        public int Hp { get; }
        /// <summary>
        /// 태크 계열 공격력. 현재 턴 검증에서는 보관만 하고 피해 계산에는 연결하지 않는다.
        /// </summary>
        public int TAtk { get; }
        /// <summary>
        /// 신비 계열 공격력. 현재 턴 검증에서는 보관만 하고 피해 계산에는 연결하지 않는다.
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
    /// 한 유닛의 식별자·표시 이름·기본 능력치·기본 서브 횟수를 묶은 정의 데이터.
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
        /// 턴 순서와 향후 전투 계산에 사용할 기본 능력치.
        /// </summary>
        public CombatStats Stats { get; }
        /// <summary>
        /// 턴 초기화 때 필드 유닛에 지급할 기본 서브 행동 횟수.
        /// </summary>
        public int BaseSubActions { get; }

        /// <summary>
        /// 식별자·이름·능력치와 서브 행동 횟수를 검증해 유닛 정의를 만든다.
        /// </summary>
        /// <param name="id">전투에서 유닛을 식별할 고유 ID.</param>
        /// <param name="name">UI와 진행 기록에 표시할 유닛 이름.</param>
        /// <param name="stats">유닛의 기본 능력치 정의.</param>
        /// <param name="baseSubActions">턴마다 초기화할 기본 서브 행동 횟수.</param>
        public UnitDefinition(string id, string name, CombatStats stats, int baseSubActions = 1)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A stable ID and a display name are required.");
            if (baseSubActions < 0) throw new ArgumentOutOfRangeException(nameof(baseSubActions));
            Id = id; Name = name; Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            BaseSubActions = baseSubActions;
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
    /// 전투 한 판에서 변하는 유닛의 배치 상태와 남은 행동 횟수. 정의 데이터와 분리한다.
    /// </summary>
    public sealed class BattleUnit
    {
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
