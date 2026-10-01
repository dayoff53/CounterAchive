using System;
using System.Collections.Generic;
using System.Linq;
using MaseiKivotos.Core;
using NUnit.Framework;

namespace MaseiKivotos.Tests
{
    /// <summary>
    /// Unity 연출 없이 턴 규칙의 초기화·정렬·예약·COST·퇴각 경계를 검증하는 EditMode 테스트.
    /// </summary>
    public sealed class TurnBattleTests
    {
        /// <summary>
        /// 테스트에서 사용할 유닛 정의를 간단히 생성한다. 속도와 서브 횟수 이외의 능력치는 고정한다.
        /// </summary>
        /// <param name="id">전투에서 유닛을 식별할 고유 ID.</param>
        /// <param name="speed">0 이상인 기본 Speed.</param>
        /// <param name="sub">테스트 유닛의 기본 서브 행동 횟수.</param>
        private static UnitDefinition D(string id, int speed, int sub = 1) =>
            new UnitDefinition(id, id, new CombatStats(100, 10, 10, 10, 10, speed), sub);

        /// <summary>
        /// 양측 필드 2명·대기 1명의 테스트 전투를 만든다. 대기 유닛의 높은 Speed가 순서에 섞이지 않는지도 확인할 수 있다.
        /// </summary>
        /// <param name="speedA">아군 A의 테스트 Speed.</param>
        /// <param name="speedB">아군 B의 테스트 Speed.</param>
        /// <param name="speedX">적군 X의 테스트 Speed.</param>
        /// <param name="speedY">적군 Y의 테스트 Speed.</param>
        /// <param name="random">동률 추첨에 사용할 난수 공급기.</param>
        private static TurnBattle Create(int speedA = 150, int speedB = 80, int speedX = 120, int speedY = 50, ITurnRandom random = null) =>
            new TurnBattle(new BattleRoster(1, new[] { D("A", speedA), D("B", speedB, 2), D("C", 999) }, new[] { 1, 2, 3 }),
                new BattleRoster(2, new[] { D("X", speedX), D("Y", speedY), D("Z", 999) }, new[] { 9, 8, 7 }), random ?? new SeededTurnRandom(53));

        /// <summary>
        /// 테스트 전투를 예약 단계까지 진행하고 제한 횟수 안에 도달했는지 확인한다.
        /// </summary>
        /// <param name="battle">테스트에서 진행할 전투 인스턴스.</param>
        private static void Plan(TurnBattle battle)
        {
            // limit: 테스트 진행이 멈췄을 때 무한 반복하지 않도록 제한하는 남은 진행 횟수.
            int limit = 10;
            while (battle.Phase != TurnPhase.Planning && --limit > 0) battle.Advance();
            Assert.AreEqual(TurnPhase.Planning, battle.Phase);
        }

        /// <summary>
        /// 순서에 포함된 필드 유닛 모두에게 턴 종료를 예약하고 확정한다.
        /// </summary>
        /// <param name="battle">테스트에서 진행할 전투 인스턴스.</param>
        private static void Confirm(TurnBattle battle)
        {
            foreach (string id in battle.Order) if (battle.Unit(id).Location == UnitLocation.Field) battle.ReserveEndTurn(id);
            battle.ConfirmReservations();
        }

        /// <summary>
        /// 현재 전체 턴이 끝나거나 전투가 종료될 때까지 진행한다. 무한 반복을 막는 상한도 검증한다.
        /// </summary>
        /// <param name="battle">테스트에서 진행할 전투 인스턴스.</param>
        private static void Finish(TurnBattle battle)
        {
            // turn: 테스트 시작 당시 전체 턴 번호. 해당 턴의 종료 여부를 판단한다.
            int turn = battle.TurnNumber;
            // limit: 테스트 진행이 멈췄을 때 무한 반복하지 않도록 제한하는 남은 진행 횟수.
            int limit = 32;
            while (battle.TurnNumber == turn && battle.Phase != TurnPhase.Ended && --limit > 0) battle.Advance();
            Assert.Greater(limit, 0, "No progress / possible infinite loop.");
        }

        /// <summary>
        /// 편성 앞의 두 명이 지정 구역에 배치되고, 대기 유닛은 행동 순서에서 제외되는지 검증한다.
        /// </summary>
        [Test]
        public void StartsWithFirstTwoInZonesAndReserveExcluded()
        {
            // battle: 진행하거나 표시·검증할 전투 상태 인스턴스.
            var battle = Create(); Plan(battle);
            Assert.AreEqual(1, battle.Unit("A").Slot); Assert.AreEqual(2, battle.Unit("B").Slot);
            Assert.AreEqual(9, battle.Unit("X").Slot); Assert.AreEqual(8, battle.Unit("Y").Slot);
            Assert.AreEqual(UnitLocation.Reserve, battle.Unit("C").Location);
            Assert.IsNull(battle.Unit("Z").Slot);
            CollectionAssert.AreEqual(new[] { "A", "X", "B", "Y" }, battle.Order);
        }

        /// <summary>
        /// 첫 턴에도 다섯 단계를 거치고 행동 횟수를 초기화하되 COST 획득만 생략하는지 검증한다.
        /// </summary>
        [Test]
        public void FirstTurnSkipsOnlyCostGainAndRunsAllFivePhases()
        {
            // battle: 진행하거나 표시·검증할 전투 상태 인스턴스.
            var battle = Create();
            Assert.AreEqual(TurnPhase.TurnStart, battle.Phase);
            battle.Advance(); Assert.AreEqual(TurnPhase.Initialization, battle.Phase);
            battle.Advance(); Assert.AreEqual(TurnPhase.Order, battle.Phase);
            Assert.AreEqual(3, battle.Costs[1]); Assert.AreEqual(3, battle.Costs[2]);
            Assert.AreEqual(1, battle.Unit("A").RemainingMainActions);
            Assert.AreEqual(2, battle.Unit("B").RemainingSubActions);
            battle.Advance(); Assert.AreEqual(TurnPhase.Planning, battle.Phase);
            Confirm(battle); Assert.AreEqual(TurnPhase.Execution, battle.Phase);
        }

        /// <summary>
        /// 속도 차이와 무관하게 유닛별 한 차례만 부여하고 마지막 유닛 종료 후 전체 턴 번호가 증가하는지 검증한다.
        /// </summary>
        [Test]
        public void OneChanceEachDespiteDifferentSpeedsAndTurnIncrementsOnlyAfterLastUnit()
        {
            // battle: 진행하거나 표시·검증할 전투 상태 인스턴스.
            var battle = Create(); Plan(battle); Confirm(battle);
            foreach (string expected in new[] { "A", "X", "B", "Y" })
            {
                battle.Advance(); Assert.AreEqual(expected, battle.ActiveUnitId);
                Assert.IsFalse(battle.HasCompleted(expected)); Assert.AreEqual(1, battle.TurnNumber);
                battle.Advance(); Assert.IsNull(battle.ActiveUnitId);
                Assert.AreEqual(expected == "Y" ? 2 : 1, battle.TurnNumber);
            }
            CollectionAssert.AreEqual(new[] { "A", "X", "B", "Y" }, battle.Events.Where(e => e.Kind == TurnEventKind.Finished).Select(e => e.UnitId));
            Assert.AreEqual(TurnPhase.TurnStart, battle.Phase);
        }

        /// <summary>
        /// 필드 인원에 따른 COST 획득·상한 10·예약과 행동 횟수 미이월을 여러 턴에 걸쳐 검증한다.
        /// </summary>
        [Test]
        public void CostUsesFieldCountAndCapsAtTenWithoutCarryOverActions()
        {
            // battle: 진행하거나 표시·검증할 전투 상태 인스턴스.
            var battle = Create(); Plan(battle);
            foreach (int expected in new[] { 5, 7, 9, 10, 10 })
            {
                Confirm(battle); Finish(battle); Plan(battle);
                Assert.AreEqual(expected, battle.Costs[1]); Assert.AreEqual(expected, battle.Costs[2]);
                Assert.AreEqual(1, battle.Unit("A").RemainingMainActions);
                Assert.AreEqual(2, battle.Unit("B").RemainingSubActions);
                Assert.IsFalse(battle.HasEndTurnReservation("A"));
            }
        }

        /// <summary>
        /// 예약 수정 가능 시점과 예약 누락·대기 유닛·확정 후 변경 거부를 검증한다.
        /// </summary>
        [Test]
        public void PlanningCanBeEditedButCannotBeChangedAfterConfirmation()
        {
            // battle: 진행하거나 표시·검증할 전투 상태 인스턴스.
            var battle = Create(); Plan(battle);
            battle.ReserveEndTurn("A"); battle.ClearReservation("A");
            Assert.IsFalse(battle.HasEndTurnReservation("A"));
            Assert.Throws<InvalidOperationException>(() => battle.ConfirmReservations());
            Assert.Throws<InvalidOperationException>(() => battle.Advance());
            Assert.Throws<InvalidOperationException>(() => battle.ReserveEndTurn("C"));
            Confirm(battle);
            Assert.Throws<InvalidOperationException>(() => battle.ReserveEndTurn("A"));
            Assert.Throws<InvalidOperationException>(() => battle.ClearReservation("A"));
            Assert.Throws<InvalidOperationException>(() => battle.ConfirmReservations());
        }

        /// <summary>
        /// 동률 테스트가 우연한 결과에 의존하지 않도록 정해진 난수와 호출 이력을 제공하는 테스트 대역.
        /// </summary>
        private sealed class ScriptedRandom : ITurnRandom
        {
            /// <summary>
            /// 첫 턴과 다음 턴의 순서를 다르게 만드는 미리 정한 난수열.
            /// </summary>
            private readonly Queue<int> values = new Queue<int>(new[] { 3, 2, 1, 0, 0, 0 });
            /// <summary>
            /// 추첨 때 요청받은 제외 상한의 기록. 매 턴 추첨이 새로 이루어졌는지 검사한다.
            /// </summary>
            public readonly List<int> Bounds = new List<int>();
            /// <summary>
            /// 요청받은 상한을 기록하고 준비된 난수열에서 다음 값을 꺼낸다.
            /// </summary>
            /// <param name="bound">테스트 대역에 요청된 난수 제외 상한.</param>
            public int Next(int bound) { Bounds.Add(bound); return values.Dequeue(); }
        }

        /// <summary>
        /// Speed가 모두 0이어도 매 턴 동률 추첨이 새로 이루어지고 유닛 누락이 없는지 검증한다.
        /// </summary>
        [Test]
        public void TiesUseFreshFisherYatesDrawsEveryTurnIncludingZeroSpeed()
        {
            // random: 정해진 추첨 결과와 호출 상한을 기록하는 동률 테스트 대역.
            var random = new ScriptedRandom();
            // battle: 진행하거나 표시·검증할 전투 상태 인스턴스.
            var battle = Create(0, 0, 0, 0, random); Plan(battle);
            // first: 다음 턴의 재추첨 결과와 비교하기 위해 복사한 첫 턴 순서.
            var first = battle.Order.ToArray();
            Confirm(battle); Finish(battle); Plan(battle);
            CollectionAssert.AreEqual(new[] { 4, 3, 2, 4, 3, 2 }, random.Bounds);
            CollectionAssert.AreEquivalent(first, battle.Order);
            Assert.IsFalse(first.SequenceEqual(battle.Order));
        }

        /// <summary>
        /// 퇴각한 유닛의 미실행 차례가 사라지고 다음 턴 COST 획득 인원에서도 빠지는지 검증한다.
        /// </summary>
        [Test]
        public void RetreatedUnitLosesPendingChanceAndDoesNotGenerateCost()
        {
            // battle: 진행하거나 표시·검증할 전투 상태 인스턴스.
            var battle = Create(); Plan(battle); Confirm(battle);
            battle.ApplyCompletedRetreatBatch(new[] { "X" }); Finish(battle); Plan(battle);
            Assert.IsFalse(battle.Events.Any(e => e.Kind == TurnEventKind.Started && e.UnitId == "X"));
            Assert.IsFalse(battle.Order.Contains("X")); Assert.AreEqual(4, battle.Costs[2]);
        }

        /// <summary>
        /// 현재 차례 소유자가 퇴각하면 그 유닛의 종료 예약을 실행하지 않고 다음 유닛으로 넘어가는지 검증한다.
        /// </summary>
        [Test]
        public void RetreatOfCurrentActorDoesNotRunItsEndTurnReservation()
        {
            // battle: 진행하거나 표시·검증할 전투 상태 인스턴스.
            var battle = Create(); Plan(battle); Confirm(battle); battle.Advance();
            Assert.AreEqual("A", battle.ActiveUnitId);
            battle.ApplyCompletedRetreatBatch(new[] { "A" }); battle.Advance();
            Assert.AreEqual("X", battle.ActiveUnitId);
            Assert.IsFalse(battle.Events.Any(e => e.Kind == TurnEventKind.Finished && e.UnitId == "A"));
        }

        /// <summary>
        /// 대기 유닛이 남아도 필드 전멸이면 종료하고, 양측 동시 전멸 시 플레이어가 패배하는지 검증한다.
        /// </summary>
        /// <param name="both">true이면 양측 모두, false이면 적군만 동시에 퇴각시킨다.</param>
        /// <param name="outcome">해당 퇴각 조합에서 기대하는 전투 결과.</param>
        [TestCase(false, BattleOutcome.PlayerVictory)]
        [TestCase(true, BattleOutcome.PlayerDefeat)]
        public void CompletedEffectBatchEndsBattleEvenWithReservesAndStopsNextTurn(bool both, BattleOutcome outcome)
        {
            // battle: 진행하거나 표시·검증할 전투 상태 인스턴스.
            var battle = Create(); Plan(battle); Confirm(battle);
            battle.ApplyCompletedRetreatBatch(both ? new[] { "A", "B", "X", "Y" } : new[] { "X", "Y" });
            Assert.AreEqual(outcome, battle.Outcome); Assert.AreEqual(TurnPhase.Ended, battle.Phase);
            Assert.IsFalse(battle.Advance()); Assert.AreEqual(1, battle.TurnNumber);
        }

        /// <summary>
        /// 퇴각 묶음에 알 수 없는 ID가 하나라도 있으면 앞의 유효 유닛도 퇴각 처리하지 않는지 검증한다.
        /// </summary>
        [Test]
        public void InvalidRetreatBatchIsAtomic()
        {
            // battle: 진행하거나 표시·검증할 전투 상태 인스턴스.
            var battle = Create(); Plan(battle); Confirm(battle);
            Assert.Throws<ArgumentException>(() => battle.ApplyCompletedRetreatBatch(new[] { "A", "unknown" }));
            Assert.AreEqual(UnitLocation.Field, battle.Unit("A").Location);
        }

        /// <summary>
        /// 음수 속도·편성 초과·범위 밖 슬롯·중복 ID·양측 출전 구역 충돌을 거부하는지 검증한다.
        /// </summary>
        [Test]
        public void RejectsInvalidStatsRosterZonesAndDuplicateIds()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => D("A", -1));
            Assert.Throws<ArgumentException>(() => new BattleRoster(1, Enumerable.Range(0, 7).Select(i => D(i.ToString(), 10)), new[] { 1, 2 }));
            Assert.Throws<ArgumentException>(() => new BattleRoster(1, new[] { D("A", 1), D("B", 2) }, new[] { 0, 2 }));
            // first: 중복 ID와 출전 구역 충돌을 검사할 때 기준이 되는 플레이어 편성.
            var first = new BattleRoster(1, new[] { D("A", 1), D("B", 2) }, new[] { 1, 2 });
            Assert.Throws<ArgumentException>(() => new TurnBattle(first, new BattleRoster(2, new[] { D("A", 3), D("X", 4) }, new[] { 8, 9 }), new SeededTurnRandom(0)));
            Assert.Throws<ArgumentException>(() => new TurnBattle(first, new BattleRoster(2, new[] { D("X", 3), D("Y", 4) }, new[] { 2, 9 }), new SeededTurnRandom(0)));
        }
    }
}
