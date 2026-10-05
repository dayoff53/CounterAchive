using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MaseiKivotos.Core
{
    /// <summary>절대 슬롯을 목표로 하는 자발적 이동 예약. 실제 시작 위치는 실행 직전에 확인한다.</summary>
    public sealed class MoveReservation
    {
        /// <summary>도착하려는 1~9 슬롯 번호.</summary>
        public int Destination { get; }
        /// <summary>검증된 목적지를 고정한다.</summary>
        /// <param name="destination">목표 슬롯.</param>
        internal MoveReservation(int destination) { Destination = destination; }
    }

    /// <summary>서브 이동 예약, 실제 한 칸씩 실행, 상태를 변경하지 않는 4단계 위치 예측.</summary>
    public sealed partial class TurnBattle
    {
        /// <summary>유닛별 서브 이동 목록. 목록 순서대로 메인 이전에 실행한다.</summary>
        private readonly Dictionary<string, List<MoveReservation>> moveReservations = new Dictionary<string, List<MoveReservation>>();
        /// <summary>이번 턴에 이미 처리한 이동 예약 수.</summary>
        private readonly Dictionary<string, int> movesExecuted = new Dictionary<string, int>();
        /// <summary>여러 Advance 호출에 걸쳐 한 칸씩 이동 중인 사용자 ID.</summary>
        private string movingUnitId;
        /// <summary>진행 중 이동의 고정 목적지와 남은 한 칸 진행 횟수.</summary>
        private int moveDestination, remainingMoveSteps;

        /// <summary>유닛의 이동 목록을 읽기 전용으로 조회한다. 상대 예약을 UI에 공개하지 않는다.</summary>
        /// <param name="unitId">예약자 ID.</param>
        public IReadOnlyList<MoveReservation> MovementReservations(string unitId)
            => moveReservations.TryGetValue(unitId, out var list) ? list.AsReadOnly() : (IReadOnlyList<MoveReservation>)Array.Empty<MoveReservation>();

        /// <summary>추가 이동 예약이 가능한지 검사한다. null이면 가능하며 실제 상태나 기존 예약을 바꾸지 않는다.</summary>
        /// <param name="unitId">필드 예약자.</param>
        /// <param name="destination">추가할 목적지.</param>
        public string MovementReservationError(string unitId, int destination)
        {
            if (Phase != TurnPhase.Planning) return "이동은 4단계에서 예약합니다.";
            // actor/positions/origin: 자기 이전 아군과 기존 자기 이동까지 반영한 예상 시작 위치.
            var actor = Unit(unitId);
            if (actor.Location != UnitLocation.Field || !order.Contains(unitId)) return "현재 차례가 있는 필드 유닛만 이동을 예약합니다.";
            if (MovementReservations(unitId).Count >= actor.RemainingSubActions) return "남은 서브 예약 횟수가 부족합니다.";
            if (destination < 1 || destination > 9) return "목적지는 1~9번 슬롯입니다.";
            var positions = ProjectMovement(actor.FactionId, unitId);
            int origin = positions[unitId];
            if (destination == origin) return "현재 예상 위치와 다른 목적지를 선택하세요.";
            if (Math.Abs(destination - origin) > actor.Definition.MoveRange) return "유닛의 이동 거리를 초과했습니다.";
            // cursor: 예약할 때 보이는 경로의 적을 검사한다. 예상 아군은 교환할 수 있다.
            for (int cursor = origin + Math.Sign(destination - origin); ; cursor += Math.Sign(destination - origin))
            {
                if (positions.Any(p => p.Value == cursor && Unit(p.Key).FactionId != actor.FactionId)) return "적이 있는 칸을 목표로 하거나 지나갈 수 없습니다.";
                if (cursor == destination) break;
            }
            return null;
        }

        /// <summary>서브 이동 하나를 목록 끝에 추가하고 마지막 턴 종료를 보완한다. 메인 스킬은 유지한다.</summary>
        /// <param name="unitId">필드 예약자.</param>
        /// <param name="destination">목표 슬롯.</param>
        public void ReserveMove(string unitId, int destination)
        {
            // error: 검증 실패 시 기존 예약을 전혀 바꾸지 않는다.
            string error = MovementReservationError(unitId, destination);
            if (error != null) throw new InvalidOperationException(error);
            if (!moveReservations.TryGetValue(unitId, out var list)) moveReservations[unitId] = list = new List<MoveReservation>();
            list.Add(new MoveReservation(destination)); reserved.Add(unitId);
            Record(TurnEventKind.Reserved, Unit(unitId).Definition.Name + " · 이동 → " + destination + "번 예약", unitId);
        }

        /// <summary>이동만 모두 취소한다. 메인 스킬과 종료 예약은 유지한다.</summary>
        /// <param name="unitId">취소할 사용자.</param>
        public void ClearMovementReservations(string unitId)
        {
            RequirePhase(TurnPhase.Planning); moveReservations.Remove(unitId);
        }

        /// <summary>공개된 현재 필드와 지정 세력의 이동만으로 예상 배치를 만든다. 스킬 피해·상대 비공개 예약은 반영하지 않는다.</summary>
        /// <param name="factionId">자기 예약을 볼 세력.</param>
        public IReadOnlyDictionary<string, int> PreviewMovement(int factionId)
        {
            RequirePhase(TurnPhase.Planning);
            return new ReadOnlyDictionary<string, int>(ProjectMovement(factionId, null));
        }

        /// <summary>선택 유닛의 메인 직전 예상 위치. 나중 차례 아군의 이동으로 사거리를 잘못 표시하지 않게 한다.</summary>
        /// <param name="unitId">현재 필드 유닛.</param>
        public int ProjectedSlotBeforeMain(string unitId)
        {
            RequirePhase(TurnPhase.Planning);
            return ProjectMovement(Unit(unitId).FactionId, unitId)[unitId];
        }

        /// <summary>외부 효과에서 자발적 이동 금지만 적용/해제한다. 메인 행동 금지나 구체적인 상태 지속 시간과 별개다.</summary>
        /// <param name="unitId">효과 대상.</param>
        /// <param name="blocked">금지 여부.</param>
        public void SetMovementBlocked(string unitId, bool blocked)
        {
            if (Phase != TurnPhase.Initialization && Phase != TurnPhase.Execution) throw new InvalidOperationException("Movement effects require initialization or execution.");
            Unit(unitId).IsMovementBlocked = blocked;
        }

        /// <summary>필드 좌표만 복사한다. 이 복사본 수정은 실제 유닛 상태를 변경하지 않는다.</summary>
        private Dictionary<string, int> CopyFieldPositions() => units.Where(u => u.Location == UnitLocation.Field).ToDictionary(u => u.Id, u => u.Slot.Value);

        /// <summary>서브 이동의 성공/실패와 교환을 복사본에서만 순서대로 계산한다.</summary>
        /// <param name="factionId">확인 가능한 아군 예약 세력.</param>
        /// <param name="stopAfterId">지정하면 해당 유닛까지의 이동만 예측한다.</param>
        private Dictionary<string, int> ProjectMovement(int factionId, string stopAfterId)
        {
            // positions/locks: 좌표와 이동 금지 횟수의 복사본. HP·자원·로그·난수는 읽거나 변경하지 않는다.
            var positions = CopyFieldPositions();
            var locks = units.ToDictionary(u => u.Id, u => u.MovementLockTurns);
            foreach (string id in order)
            {
                // actor/remaining: 이 차례의 이동 주체와 복사된 서브 잔여량.
                var actor = Unit(id);
                int remaining = actor.RemainingSubActions;
                if (actor.FactionId == factionId && positions.ContainsKey(id))
                    foreach (var plan in MovementReservations(id))
                    {
                        // D-009: 이동 금지로 막힌 시도는 기존 횟수를 보존하며 새 디버프를 만들지 않는다.
                        bool hasSub = remaining-- > 0;
                        if (locks[id] > 0 || actor.IsMovementBlocked) continue;
                        if (hasSub && Math.Abs(positions[id] - plan.Destination) <= actor.Definition.MoveRange)
                            while (positions[id] != plan.Destination && TryMoveOneSlot(id, plan.Destination, positions)) { }
                        locks[id] = 2;
                    }
                if (id == stopAfterId) break;
            }
            return positions;
        }

        /// <summary>좌표 복사본에서 한 칸 진행한다. 아군은 교환하며 적이 있으면 변경 없이 false를 반환한다.</summary>
        /// <param name="unitId">이동 주체.</param>
        /// <param name="destination">목표 슬롯.</param>
        /// <param name="positions">변경할 좌표 복사본.</param>
        private bool TryMoveOneSlot(string unitId, int destination, Dictionary<string, int> positions)
        {
            // from/next/occupant: 이번 한 칸 진행의 출발지, 목적 칸과 점유자.
            int from = positions[unitId], next = from + Math.Sign(destination - from);
            string occupant = positions.FirstOrDefault(p => p.Value == next && p.Key != unitId).Key;
            if (occupant != null && Unit(occupant).FactionId != Unit(unitId).FactionId) return false;
            if (occupant != null) positions[occupant] = from;
            positions[unitId] = next; return true;
        }

        /// <summary>남은 서브 이동을 한 칸 또는 실패 한 번 처리한다. 이동이 없을 때만 false로 메인 실행에 넘긴다.</summary>
        /// <param name="actor">현재 차례 유닛.</param>
        private bool TryExecuteMovement(BattleUnit actor)
        {
            if (movingUnitId != actor.Id)
            {
                // index/plans: 다음에 시도할 확정 이동.
                int index = movesExecuted.TryGetValue(actor.Id, out int done) ? done : 0;
                var plans = MovementReservations(actor.Id);
                if (index >= plans.Count) return false;
                movingUnitId = actor.Id; moveDestination = plans[index].Destination; remainingMoveSteps = actor.Definition.MoveRange;
                // blocked: 실행 시 이미 이동 금지라면 서브는 소비하되 이동 불가를 재부여하지 않는다.
                bool blocked = actor.MovementLockTurns > 0 || actor.IsMovementBlocked;
                // allowed: 상태/거리/잔여 횟수는 이동 시작 직전에 재검사한다.
                bool allowed = actor.RemainingSubActions > 0 && actor.MovementLockTurns == 0 && !actor.IsMovementBlocked
                    && actor.Slot != moveDestination && Math.Abs(actor.Slot.Value - moveDestination) <= remainingMoveSteps;
                actor.RemainingSubActions = Math.Max(0, actor.RemainingSubActions - 1);
                if (blocked) { FinishMovement(actor, "이동 금지로 실행 불가", false); return true; }
                if (!allowed) { FinishMovement(actor, "이동 실패 (이동 불가/거리/서브 재검사)"); return true; }
            }
            // positions/from: 실제 상태를 한 칸 교환 후 원자적으로 반영하기 위한 좌표 묶음.
            var positions = CopyFieldPositions();
            int from = actor.Slot.Value;
            if (remainingMoveSteps <= 0 || !TryMoveOneSlot(actor.Id, moveDestination, positions))
            { FinishMovement(actor, "경로 차단 · " + from + "번에서 종료"); return true; }
            foreach (var position in positions) Unit(position.Key).Slot = position.Value;
            remainingMoveSteps--;
            Record(TurnEventKind.MovementStep, actor.Definition.Name + " · 이동 " + from + " → " + actor.Slot + "번 (아군은 순차 교환)", actor.Id);
            if (actor.Slot == moveDestination) FinishMovement(actor, "목적지 도착");
            return true;
        }

        /// <summary>시도를 마치고 다음 예약으로 넘긴다. 기존 이동 금지로 막혔으면 횟수를 유지하고, 그 외에는 2회를 부여한다.</summary>
        /// <param name="actor">시도한 유닛.</param>
        /// <param name="reason">도착/실패/부분 이동 안내.</param>
        /// <param name="applyMovementLock">기존 이동 금지에 막힌 경우 false로 재부여를 생략한다.</param>
        private void FinishMovement(BattleUnit actor, string reason, bool applyMovementLock = true)
        {
            if (applyMovementLock) actor.MovementLockTurns = 2;
            movesExecuted[actor.Id] = (movesExecuted.TryGetValue(actor.Id, out int count) ? count : 0) + 1;
            movingUnitId = null;
            Record(TurnEventKind.MovementFinished, actor.Definition.Name + " · " + reason
                + (applyMovementLock ? " · 이동 불가 2회" : " · 이동 불가 재부여 없음 (잔여 " + actor.MovementLockTurns + "회)"), actor.Id);
        }

        /// <summary>새 턴/전투 종료에서 이동 예약과 진행 중인 경로를 모두 비운다. 유닛의 디버프는 별도로 보존한다.</summary>
        private void ResetMovementReservations()
        {
            moveReservations.Clear(); movesExecuted.Clear(); movingUnitId = null;
        }
    }
}
