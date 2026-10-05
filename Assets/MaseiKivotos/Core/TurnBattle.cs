using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MaseiKivotos.Core
{
    /// <summary>
    /// 한 턴의 5단계와 전투 종료 상태. 숫자는 화면의 단계 번호와 대응한다.
    /// </summary>
    public enum TurnPhase
    {
        /// <summary>
        /// 1단계: 현재 필드를 기준으로 승패를 확인한다.
        /// </summary>
        TurnStart = 1,
        /// <summary>
        /// 2단계: 필드 유닛의 행동 횟수를 초기화하고 공유 COST를 보충한다.
        /// </summary>
        Initialization = 2,
        /// <summary>
        /// 3단계: 필드 유닛의 Speed와 동률 추첨으로 공통 순서를 정한다.
        /// </summary>
        Order = 3,
        /// <summary>
        /// 4단계: 서브 이동·메인 스킬과 마지막 턴 종료를 예약하고 확정한다.
        /// </summary>
        Planning = 4,
        /// <summary>
        /// 5단계: 확정된 순서대로 각 유닛의 차례를 진행한다.
        /// </summary>
        Execution = 5,
        /// <summary>
        /// 승패가 확정되어 더 이상 턴을 진행하지 않는 상태. 여섯 번째 턴 단계는 아니다.
        /// </summary>
        Ended = 6
    }
    /// <summary>
    /// 현재 2세력 PvE 전투의 진행 또는 최종 결과.
    /// </summary>
    public enum BattleOutcome
    {
        /// <summary>
        /// 아직 전투 결과가 확정되지 않은 상태.
        /// </summary>
        Ongoing,
        /// <summary>
        /// 플레이어 필드 유닛이 남고 적 필드 유닛이 전멸한 결과.
        /// </summary>
        PlayerVictory,
        /// <summary>
        /// 플레이어 필드 전멸로 인한 패배. PvE 동시 전멸도 포함한다.
        /// </summary>
        PlayerDefeat
    }
    /// <summary>
    /// 화면 진행 기록과 테스트에서 사건을 구분하는 종류.
    /// </summary>
    public enum TurnEventKind
    {
        /// <summary>
        /// 전투 단계가 시작되거나 바뀐 기록.
        /// </summary>
        Phase,
        /// <summary>
        /// 행동 횟수와 COST 초기화 처리 기록.
        /// </summary>
        Initialized,
        /// <summary>
        /// 이번 턴의 공통 행동 순서 확정 기록.
        /// </summary>
        Ordered,
        /// <summary>
        /// 개별 유닛의 스킬/턴 종료 예약 기록. 상대 예약은 UI에서 숨긴다.
        /// </summary>
        Reserved,
        /// <summary>
        /// 전체 예약 확정 및 실행 단계 진입 기록.
        /// </summary>
        Confirmed,
        /// <summary>
        /// 개별 유닛의 행동 차례 시작 기록.
        /// </summary>
        Started,
        /// <summary>
        /// 개별 유닛의 차례 종료 기록.
        /// </summary>
        Finished,
        /// <summary>
        /// 모든 필드 유닛의 차례를 마친 전체 턴 종료 기록.
        /// </summary>
        RoundFinished,
        /// <summary>
        /// 유닛 퇴각 결과를 반영한 기록.
        /// </summary>
        Retreated,
        /// <summary>
        /// 전투 승패 확정 기록.
        /// </summary>
        BattleEnded,
        /// <summary>일반 스킬 또는 대체 스킬 적중과 HP 변화.</summary>
        SkillHit,
        /// <summary>실행한 스킬의 명중 실패.</summary>
        SkillMissed,
        /// <summary>실행 조건 불충족으로 메인 스킬 취소.</summary>
        SkillCancelled,
        /// <summary>원래 스킬 대신 헤일로 이상 발동.</summary>
        SkillReplaced,
        /// <summary>필수 후속 HP 소모와 헤일로 이상 자체 BP 완충.</summary>
        SkillFollowUp,
        /// <summary>자발적 이동의 한 칸 진행과 아군 교환.</summary>
        MovementStep,
        /// <summary>이동 시도 종료와 이동 불가 부여.</summary>
        MovementFinished
    }

    /// <summary>
    /// 턴 번호·사건 종류·관련 유닛·표시 문구를 보관하는 변경 불가능한 진행 기록.
    /// </summary>
    public sealed class TurnEvent
    {
        /// <summary>
        /// 사건이 발생한 전체 턴 번호. 개별 유닛의 차례 번호와 다르다.
        /// </summary>
        public int Turn { get; }
        /// <summary>
        /// 이 기록이 나타내는 사건 종류.
        /// </summary>
        public TurnEventKind Kind { get; }
        /// <summary>
        /// 사건과 관련된 유닛 ID. 단계 전환 등 특정 유닛이 없는 기록에서는 null이다.
        /// </summary>
        public string UnitId { get; }
        /// <summary>
        /// UI와 디버깅에 표시할 진행 설명 문구.
        /// </summary>
        public string Message { get; }
        /// <summary>
        /// 발생 시점과 사건 정보를 하나의 진행 기록으로 묶는다.
        /// </summary>
        /// <param name="turn">사건이 발생한 전체 턴 번호.</param>
        /// <param name="kind">기록할 사건 종류.</param>
        /// <param name="message">화면에 표시할 사건 설명.</param>
        /// <param name="unitId">관련 유닛 ID. 특정 유닛과 무관한 사건이면 null.</param>
        internal TurnEvent(int turn, TurnEventKind kind, string message, string unitId)
        { Turn = turn; Kind = kind; Message = message; UnitId = unitId; }
    }

    /// <summary>
    /// Unity 연출과 분리된 2세력 PvE 턴 로직. 이동·일반 공격·턴 종료를 지원하며 일반 상태 효과·턴 도중 Speed 변경은 미구현이다.
    /// </summary>
    public sealed partial class TurnBattle
    {
        /// <summary>
        /// 필드·대기·퇴각을 모두 포함한 이번 전투의 유닛 상태 목록.
        /// </summary>
        private readonly List<BattleUnit> units = new List<BattleUnit>();
        /// <summary>
        /// 이번 턴의 공통 행동 순서를 나타내는 유닛 ID 목록.
        /// </summary>
        private readonly List<string> order = new List<string>();
        /// <summary>
        /// 현재 턴 종료가 예약된 유닛 ID 집합. 중복 예약을 방지한다.
        /// </summary>
        private readonly HashSet<string> reserved = new HashSet<string>();
        /// <summary>
        /// 이번 턴에서 차례를 끝낸 유닛 ID 집합. 동일 유닛의 재행동을 막는다.
        /// </summary>
        private readonly HashSet<string> completed = new HashSet<string>();
        /// <summary>
        /// 세력 ID별 공유 COST. 초기값 3, 상한 10으로 관리한다.
        /// </summary>
        private readonly Dictionary<int, int> costs = new Dictionary<int, int>();
        /// <summary>
        /// 최근 진행 기록 목록. 장시간 실행 시 무한히 쌓이지 않도록 최대 256개를 유지한다.
        /// </summary>
        private readonly List<TurnEvent> events = new List<TurnEvent>();
        /// <summary>
        /// 동일 Speed 유닛의 순서와 스킬 명중을 추첨하는 외부 주입 난수 공급기.
        /// </summary>
        private readonly ITurnRandom random;

        /// <summary>
        /// 전체 턴 번호. 모든 유효 차례를 처리한 뒤에만 1 증가한다.
        /// </summary>
        public int TurnNumber { get; private set; } = 1;
        /// <summary>
        /// 현재 진행 단계. 예약 단계에서는 확정 호출 없이 실행으로 넘어갈 수 없다.
        /// </summary>
        public TurnPhase Phase { get; private set; } = TurnPhase.TurnStart;
        /// <summary>
        /// 현재 전투 결과. 승패 확정 전에는 Ongoing이다.
        /// </summary>
        public BattleOutcome Outcome { get; private set; }
        /// <summary>
        /// 플레이어로 취급할 세력 ID. 승패와 아군 UI 판정의 기준이다.
        /// </summary>
        public int PlayerFactionId { get; }
        /// <summary>
        /// 현재 차례를 진행 중인 유닛 ID. 예약 중이나 차례 사이에는 null이다.
        /// </summary>
        public string ActiveUnitId { get; private set; }
        /// <summary>
        /// 외부에서 목록 구성을 바꿀 수 없도록 공개하는 전체 유닛 상태 목록.
        /// </summary>
        public IReadOnlyList<BattleUnit> Units { get; }
        /// <summary>
        /// UI와 테스트에 공개하는 이번 턴의 공통 행동 순서.
        /// </summary>
        public IReadOnlyList<string> Order { get; }
        /// <summary>
        /// 외부에서 직접 차감·증가할 수 없도록 공개하는 세력별 COST 조회용 사전.
        /// </summary>
        public IReadOnlyDictionary<int, int> Costs { get; }
        /// <summary>
        /// 최근 진행 기록의 읽기 전용 목록. 상대 예약을 숨기는 처리는 화면에서 담당한다.
        /// </summary>
        public IReadOnlyList<TurnEvent> Events { get; }
        /// <summary>
        /// 현재 턴에서 차례를 마친 유닛 수. 다음 턴 준비 시 0으로 초기화한다.
        /// </summary>
        public int CompletedCount => completed.Count;

        /// <summary>
        /// 두 세력의 중복 ID와 출전 구역 충돌을 검사하고, 각 편성의 앞 두 명을 출전시켜 1턴을 시작한다.
        /// </summary>
        /// <param name="player">플레이어 세력의 편성과 출전 구역.</param>
        /// <param name="enemy">상대 세력의 편성과 출전 구역.</param>
        /// <param name="random">동률과 명중 추첨에 사용할 난수 공급기.</param>
        public TurnBattle(BattleRoster player, BattleRoster enemy, ITurnRandom random)
        {
            if (player == null || enemy == null) throw new ArgumentNullException(nameof(player));
            if (player.FactionId == enemy.FactionId) throw new ArgumentException("Distinct factions required.");
            if (player.DeploymentSlots.Intersect(enemy.DeploymentSlots).Any())
                throw new ArgumentException("Deployment zones must not overlap.");
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            PlayerFactionId = player.FactionId;
            foreach (var roster in new[] { player, enemy })
            {
                costs.Add(roster.FactionId, 3);
                for (int i = 0; i < roster.Units.Count; i++)
                    units.Add(new BattleUnit(roster.Units[i], roster.FactionId, i < 2 ? (int?)roster.DeploymentSlots[i] : null));
            }
            if (units.Select(u => u.Id).Distinct().Count() != units.Count)
                throw new ArgumentException("Unit IDs must be unique across factions.");
            Units = units.AsReadOnly(); Order = order.AsReadOnly(); Events = events.AsReadOnly();
            Costs = new ReadOnlyDictionary<int, int>(costs);
            Record(TurnEventKind.Phase, "1단계 · 턴 시작");
        }

        /// <summary>
        /// 고유 ID에 대응하는 전투 유닛을 찾는다. 존재하지 않으면 예외를 발생시킨다.
        /// </summary>
        /// <param name="id">전투에서 유닛을 식별할 고유 ID.</param>
        public BattleUnit Unit(string id) => units.FirstOrDefault(u => u.Id == id)
            ?? throw new ArgumentException("Unknown unit ID: " + id);
        /// <summary>
        /// 해당 유닛의 턴 종료 예약이 현재 저장되어 있는지 조회한다.
        /// </summary>
        /// <param name="id">전투에서 유닛을 식별할 고유 ID.</param>
        public bool HasEndTurnReservation(string id) => reserved.Contains(id);
        /// <summary>
        /// 해당 유닛이 이번 턴에서 이미 차례를 마쳤는지 조회한다.
        /// </summary>
        /// <param name="id">전투에서 유닛을 식별할 고유 ID.</param>
        public bool HasCompleted(string id) => completed.Contains(id);

        /// <summary>
        /// 예약 단계에 이번 턴 필드 유닛의 턴 종료를 예약한다. 같은 예약을 중복 추가하지 않는다.
        /// </summary>
        /// <param name="id">전투에서 유닛을 식별할 고유 ID.</param>
        public void ReserveEndTurn(string id)
        {
            RequirePhase(TurnPhase.Planning);
            if (!order.Contains(id) || Unit(id).Location != UnitLocation.Field)
                throw new InvalidOperationException("Only this turn's field units may reserve.");
            if (reserved.Add(id)) Record(TurnEventKind.Reserved, Unit(id).Definition.Name + " · 턴 종료 예약", id);
        }

        /// <summary>
        /// 예약 단계에서 해당 유닛의 이동/스킬/턴 종료 예약을 모두 취소한다. 실행 단계에서는 변경을 허용하지 않는다.
        /// </summary>
        /// <param name="id">전투에서 유닛을 식별할 고유 ID.</param>
        public void ClearReservation(string id)
        {
            RequirePhase(TurnPhase.Planning);
            reserved.Remove(id);
            mainReservations.Remove(id);
            moveReservations.Remove(id);
        }

        /// <summary>
        /// 차례가 있는 모든 필드 유닛의 예약을 확인한 뒤 실행 단계로 전환한다. 미예약 유닛이 있으면 예외를 발생시킨다.
        /// </summary>
        public void ConfirmReservations()
        {
            RequirePhase(TurnPhase.Planning);
            if (order.Any(id => Unit(id).Location == UnitLocation.Field && !reserved.Contains(id)))
                throw new InvalidOperationException("Every field unit must finish planning before execution.");
            // entry: 확정 시점의 BP 0 여부를 이후 대체 판정에 사용하도록 저장한다.
            foreach (var entry in mainReservations)
                entry.Value.ZeroBpAtConfirmation = Unit(entry.Key).SkillBp(entry.Value.SkillId) == 0;
            Record(TurnEventKind.Confirmed, "예약 확정 · 실행 중 변경 불가");
            ChangePhase(TurnPhase.Execution);
        }

        /// <summary>
        /// 초기 단계 하나 또는 차례 시작/이동 한 칸/스킬/종료를 한 번 진행한다. 예약 단계는 별도 확정이 필요하며 전투 종료 상태에서는 false를 반환한다.
        /// </summary>
        public bool Advance()
        {
            switch (Phase)
            {
                case TurnPhase.TurnStart:
                    if (!EvaluateVictory()) ChangePhase(TurnPhase.Initialization);
                    break;
                case TurnPhase.Initialization:

                    foreach (var unit in units)
                    {
                        // field: 초기화 대상이 현재 필드 유닛인지 여부. 대기·퇴각 유닛에는 행동 횟수를 주지 않는다.
                        bool field = unit.Location == UnitLocation.Field;
                        unit.RemainingMainActions = field ? 1 : 0;
                        unit.RemainingSubActions = field ? unit.Definition.BaseSubActions : 0;
                    }
                    if (TurnNumber > 1)
                        foreach (int faction in costs.Keys.ToArray())
                            costs[faction] = Math.Min(10, costs[faction] + units.Count(u => u.FactionId == faction && u.Location == UnitLocation.Field));
                    Record(TurnEventKind.Initialized, TurnNumber == 1 ? "행동 횟수 초기화 · 첫 턴 COST 획득 생략" : "행동 횟수 초기화 · 필드 인원만큼 COST 획득 (최대 10)");
                    ChangePhase(TurnPhase.Order);
                    break;
                case TurnPhase.Order:
                    BuildOrder();
                    ChangePhase(TurnPhase.Planning);
                    break;
                case TurnPhase.Planning:
                    throw new InvalidOperationException("Confirm reservations before advancing.");
                case TurnPhase.Execution:
                    AdvanceExecution();
                    break;
                case TurnPhase.Ended:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 외부 효과 처리가 끝난 한 묶음의 퇴각 결과를 반영한 뒤 승패를 판정한다. 피해 계산 자체는 하지 않으며 모든 ID를 먼저 검증해 부분 적용을 막는다.
        /// </summary>
        /// <param name="unitIds">필수 후속 효과까지 끝난 한 묶음의 퇴각 유닛 ID 목록.</param>
        public void ApplyCompletedRetreatBatch(IEnumerable<string> unitIds)
        {
            if (Phase == TurnPhase.Ended) return;
            if (Phase != TurnPhase.Execution && Phase != TurnPhase.Initialization)
                throw new InvalidOperationException("Retreat results belong to effect execution.");
            // retiring: 중복 ID를 제거하고 전체 유닛 조회를 끝낸 퇴각 대상 배열. 검증 완료 전에는 상태를 바꾸지 않는다.
            var retiring = unitIds?.Distinct().Select(Unit).ToArray() ?? throw new ArgumentNullException(nameof(unitIds));
            RetireWithoutVictory(retiring);
            EvaluateVictory();
        }

        /// <summary>유닛의 필드 이탈과 예약 정리만 수행한다. 필수 후속 처리가 끝날 때까지 승패를 확정하지 않는다.</summary>
        /// <param name="retiring">모든 입력 검증이 끝난 퇴각 대상.</param>
        private void RetireWithoutVictory(IEnumerable<BattleUnit> retiring)
        {
            foreach (var unit in retiring)
            {
                if (unit.Location == UnitLocation.Retreated) continue;
                unit.Location = UnitLocation.Retreated; unit.Slot = null;
                unit.CurrentHp = 0;
                unit.RemainingMainActions = 0; unit.RemainingSubActions = 0;
                reserved.Remove(unit.Id);
                mainReservations.Remove(unit.Id);
                moveReservations.Remove(unit.Id); movesExecuted.Remove(unit.Id);
                if (movingUnitId == unit.Id) movingUnitId = null;
                if (ActiveUnitId == unit.Id) ActiveUnitId = null;
                Record(TurnEventKind.Retreated, unit.Definition.Name + " · 퇴각", unit.Id);
            }
        }

        /// <summary>
        /// 필드 유닛을 Speed 내림차순으로 정렬하고 동률 그룹을 매 턴 새로 섞는다. 이전 예약·완료 상태도 비운다.
        /// </summary>
        private void BuildOrder()
        {
            order.Clear(); reserved.Clear(); completed.Clear(); ActiveUnitId = null;
            mainReservations.Clear(); mainExecuted.Clear();
            ResetMovementReservations();
            foreach (var group in units.Where(u => u.Location == UnitLocation.Field).GroupBy(u => u.Definition.Stats.Speed).OrderByDescending(g => g.Key))
            {
                // tied: Speed가 같은 유닛 그룹의 복사본. 이 목록 안에서만 균등하게 순서를 섞는다.
                var tied = group.ToList();
                // Fisher-Yates 방식으로 같은 Speed 그룹 안에서만 매 턴 균등 추첨한다.
                for (int i = tied.Count - 1; i > 0; i--)
                {
                    // j: 아직 섞지 않은 구간에서 뽑은 교환 대상 인덱스. 현재 위치 i도 포함한다.
                    int j = random.Next(i + 1);
                    if (j < 0 || j > i) throw new InvalidOperationException("Random source returned an invalid index.");
                    // swap: 동률 그룹의 두 원소를 교환할 때 잠시 보관하는 유닛.
                    var swap = tied[i]; tied[i] = tied[j]; tied[j] = swap;
                }
                order.AddRange(tied.Select(u => u.Id));
            }
            Record(TurnEventKind.Ordered, "공통 순서 · " + string.Join(" → ", order.Select(id => Unit(id).Definition.Name)));
        }

        /// <summary>
        /// 현재 차례의 서브 이동 → 메인 스킬 → 종료를 처리하고, 차례가 없으면 다음 유효 유닛을 시작한다. 남은 차례가 없으면 전체 턴을 끝낸다.
        /// </summary>
        private void AdvanceExecution()
        {
            if (EvaluateVictory()) return;
            if (ActiveUnitId != null)
            {
                // unit: 현재 스킬 또는 종료를 처리할 유닛.
                var unit = Unit(ActiveUnitId);
                if (TryExecuteMovement(unit)) return;
                if (mainReservations.TryGetValue(unit.Id, out var main) && !mainExecuted.Contains(unit.Id))
                {
                    ExecuteMainSkill(unit, main);
                    return;
                }
                completed.Add(unit.Id);
                if (unit.MovementLockTurns > 0) unit.MovementLockTurns--;
                unit.RemainingMainActions = 0; unit.RemainingSubActions = 0;
                Record(TurnEventKind.Finished, unit.Definition.Name + " · 턴 종료 (차례 소모)", unit.Id);
                ActiveUnitId = null;
                if (!order.Any(IsPending)) FinishRound();
                return;
            }
            ActiveUnitId = order.FirstOrDefault(IsPending);
            if (ActiveUnitId == null) { FinishRound(); return; }
            if (!reserved.Contains(ActiveUnitId)) throw new InvalidOperationException("Missing confirmed EndTurn reservation.");
            Record(TurnEventKind.Started, Unit(ActiveUnitId).Definition.Name + " · 행동 차례 시작", ActiveUnitId);
        }

        /// <summary>
        /// 해당 유닛이 아직 차례를 끝내지 않았고 필드에 남아 있는지 판정한다.
        /// </summary>
        /// <param name="id">전투에서 유닛을 식별할 고유 ID.</param>
        private bool IsPending(string id) => !completed.Contains(id) && Unit(id).Location == UnitLocation.Field;

        /// <summary>
        /// 전체 턴 종료를 기록하고 턴 번호를 증가시킨 뒤 다음 턴 시작 단계로 돌아간다.
        /// </summary>
        private void FinishRound()
        {
            Record(TurnEventKind.RoundFinished, "전체 턴 완료 · 모든 필드 유닛의 차례 처리");
            TurnNumber++;
            order.Clear(); reserved.Clear(); completed.Clear(); ActiveUnitId = null;
            mainReservations.Clear(); mainExecuted.Clear();
            ResetMovementReservations();
            ChangePhase(TurnPhase.TurnStart);
        }

        /// <summary>
        /// 양측 필드 생존 여부로 2세력 PvE 승패를 확정한다. 대기 유닛은 생존 판정에 포함하지 않으며 양측 전멸은 플레이어 패배다. 종료 확정 시 true를 반환한다.
        /// </summary>
        private bool EvaluateVictory()
        {
            // playerAlive: 플레이어 필드 유닛이 한 명 이상 남아 있는지 나타내는 승패 조건.
            bool playerAlive = units.Any(u => u.FactionId == PlayerFactionId && u.Location == UnitLocation.Field);
            // enemyAlive: 적 세력의 필드 유닛이 한 명 이상 남아 있는지 나타내는 승패 조건.
            bool enemyAlive = units.Any(u => u.FactionId != PlayerFactionId && u.Location == UnitLocation.Field);
            if (playerAlive && enemyAlive) return false;

            Outcome = playerAlive ? BattleOutcome.PlayerVictory : BattleOutcome.PlayerDefeat;
            ActiveUnitId = null; reserved.Clear(); Phase = TurnPhase.Ended;
            mainReservations.Clear();
            ResetMovementReservations();
            Record(TurnEventKind.BattleEnded, Outcome == BattleOutcome.PlayerVictory ? "전투 종료 · 승리" : "전투 종료 · 패배");
            return true;
        }

        /// <summary>
        /// 현재 단계가 요구 단계와 다르면 예외를 발생시켜 잘못된 시점의 조작을 차단한다.
        /// </summary>
        /// <param name="expected">현재 조작을 허용하는 단계.</param>
        private void RequirePhase(TurnPhase expected)
        { if (Phase != expected) throw new InvalidOperationException("Expected phase " + expected + ", got " + Phase); }
        /// <summary>
        /// 현재 단계를 바꾸고 해당 단계의 시작 문구를 진행 기록에 남긴다.
        /// </summary>
        /// <param name="phase">전환하거나 표시할 턴 단계.</param>
        private void ChangePhase(TurnPhase phase)
        {
            Phase = phase;
            // names: 턴 단계 번호에 대응하는 화면 표시 문구 배열.
            string[] names = { "", "턴 시작", "초기화", "행동 순서", "행동 예약", "행동 실행", "전투 종료" };
            Record(TurnEventKind.Phase, ((int)phase) + "단계 · " + names[(int)phase]);
        }
        /// <summary>
        /// 현재 턴 번호로 사건을 기록한다. 256개를 넘기기 전 가장 오래된 기록을 제거한다.
        /// </summary>
        /// <param name="kind">기록할 사건 종류.</param>
        /// <param name="message">화면에 표시할 사건 설명.</param>
        /// <param name="unitId">관련 유닛 ID. 특정 유닛과 무관한 사건이면 null.</param>
        private void Record(TurnEventKind kind, string message, string unitId = null)
        {
            if (events.Count == 256) events.RemoveAt(0);
            events.Add(new TurnEvent(TurnNumber, kind, message, unitId));
        }
    }
}
