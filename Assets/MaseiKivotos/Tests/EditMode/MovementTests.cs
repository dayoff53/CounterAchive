using System;
using System.Linq;
using MaseiKivotos.Core;
using NUnit.Framework;

namespace MaseiKivotos.Tests
{
    /// <summary>이동 예약·실행·미리보기의 상태 분리와 규칙 경계를 검증한다.</summary>
    public sealed class MovementTests
    {
        /// <summary>잔여 1회 중 반복 시도해도 연장하지 않고 자기 종료에서 정상 해제한다.</summary>
        [Test]
        public void BlockedRetriesPreserveOneRemainingLockAndExpireAtEnd()
        {
            // battle: 첫 턴에 이동을 마친 뒤 다음 턴 두 번 재시도하는 전투.
            var battle = Create(); Plan(battle); battle.ReserveMove("A", 4); Confirm(battle); Plan(battle);
            Assert.AreEqual(1, battle.Unit("A").MovementLockTurns);
            battle.ReserveMove("A", 5); battle.ReserveMove("A", 6); battle.ReserveMainSkill("A", "hit", "X");
            Assert.AreEqual(4, battle.PreviewMovement(1)["A"]);
            Assert.AreEqual(1, battle.Unit("A").MovementLockTurns);
            Confirm(battle); Start(battle, "A");
            battle.Advance(); Assert.AreEqual(1, battle.Unit("A").MovementLockTurns);
            battle.Advance(); Assert.AreEqual(1, battle.Unit("A").MovementLockTurns);
            Assert.AreEqual(0, battle.Unit("A").RemainingSubActions); Assert.AreEqual(4, battle.Unit("A").Slot);
            battle.Advance(); Assert.AreEqual(70, battle.Unit("X").CurrentHp);
            battle.Advance(); Assert.AreEqual(0, battle.Unit("A").MovementLockTurns);
            Plan(battle); battle.ReserveMove("A", 5); Confirm(battle); Start(battle, "A"); battle.Advance();
            Assert.AreEqual(5, battle.Unit("A").Slot); Assert.AreEqual(2, battle.Unit("A").MovementLockTurns);
        }

        /// <summary>외부 이동 금지 중 시도는 새 2회를 만들지 않으며 해제 후 다음 이동을 실행한다.</summary>
        [Test]
        public void ExternalBlockDoesNotCreateLockAndLaterUnblockedMoveCanRun()
        {
            // battle: 초기화 시 외부 금지를 부여하고 실행 중 해제하는 전투.
            var battle = Create(); battle.Advance(); battle.SetMovementBlocked("A", true); Plan(battle);
            battle.ReserveMove("A", 3); battle.ReserveMove("A", 4);
            Assert.AreEqual(1, battle.PreviewMovement(1)["A"]);
            Confirm(battle); Start(battle, "A"); battle.Advance();
            Assert.AreEqual(0, battle.Unit("A").MovementLockTurns); Assert.AreEqual(1, battle.Unit("A").RemainingSubActions);
            battle.SetMovementBlocked("A", false);
            battle.Advance(); battle.Advance(); battle.Advance();
            Assert.AreEqual(4, battle.Unit("A").Slot); Assert.AreEqual(2, battle.Unit("A").MovementLockTurns);
        }

        /// <summary>아군 교환만 당한 유닛은 같은 차례의 자기 이동을 정상 실행할 수 있다.</summary>
        [Test]
        public void SwappedPassengerCanExecuteItsOwnMoveWithoutInheritingLock()
        {
            // battle: A1→4로 B가 2→1 교환된 뒤 B가 자기 차례에 1→3 이동하는 전투.
            var battle = Create(); Plan(battle);
            battle.ReserveMove("A", 4); battle.ReserveMove("B", 3);
            Assert.AreEqual(3, battle.PreviewMovement(1)["B"]);
            Assert.AreEqual(0, battle.Unit("B").MovementLockTurns);
            Confirm(battle); Start(battle, "B");
            Assert.AreEqual(1, battle.Unit("B").Slot);
            Assert.AreEqual(0, battle.Unit("B").MovementLockTurns);
            Assert.AreEqual(2, battle.Unit("B").RemainingSubActions);
            battle.Advance(); Assert.AreEqual(2, battle.Unit("B").Slot);
            battle.Advance(); Assert.AreEqual(3, battle.Unit("B").Slot);
            Assert.AreEqual(2, battle.Unit("B").MovementLockTurns, "자기 이동을 시도한 뒤에만 부여한다.");
        }

        /// <summary>교환만 당하고 자기 이동을 예약하지 않은 유닛은 다음 턴에도 이동 불가가 없다.</summary>
        [Test]
        public void PassiveSwapLeavesPassengerUnlockedIntoNextTurn()
        {
            // battle: A만 이동을 예약한 경우의 다음 턴 상태.
            var battle = Create(); Plan(battle); battle.ReserveMove("A", 4); Confirm(battle);
            Plan(battle);
            Assert.AreEqual(1, battle.Unit("B").Slot);
            Assert.AreEqual(1, battle.Unit("A").MovementLockTurns);
            Assert.AreEqual(0, battle.Unit("B").MovementLockTurns);
            Assert.IsFalse(battle.Unit("B").IsMovementBlocked);
            Assert.IsNull(battle.MovementReservationError("B", 4));
            Assert.IsFalse(battle.Events.Any(e => e.Kind == TurnEventKind.MovementFinished && e.UnitId == "B"));
        }

        /// <summary>각 난수 요청 횟수를 기록하여 미리보기의 비침투성을 검사한다.</summary>
        private sealed class CountingRandom : ITurnRandom
        {
            /// <summary>난수 요청 누계.</summary>
            public int Calls;
            /// <summary>유효한 첫 인덱스를 반환한다.</summary>
            /// <param name="maxExclusive">상한.</param>
            public int Next(int maxExclusive) { Calls++; return 0; }
        }
        /// <summary>이동 거리 3, 서브 2회, 근거리 공격을 가진 테스트 유닛.</summary>
        /// <param name="id">유닛 ID.</param>
        /// <param name="speed">순서.</param>
        private static UnitDefinition U(string id, int speed) => new UnitDefinition(id, id,
            new CombatStats(100, 10, 10, 10, 10, speed), 2,
            new[] { new MainSkill("hit", "공격", SkillType.Tech, CombatTrait.Neutral, 20, 100, 5, 0, 1, 5) }, moveRange: 3);
        /// <summary>원하는 초기 위치와 적 선행 여부로 전투를 구성한다.</summary>
        /// <param name="a">아군 A 위치.</param>
        /// <param name="b">아군 B 위치.</param>
        /// <param name="x">적 X 위치.</param>
        /// <param name="y">적 Y 위치.</param>
        /// <param name="enemyFirst">적을 먼저 움직일지 여부.</param>
        /// <param name="random">난수 추적기.</param>
        private static TurnBattle Create(int a = 1, int b = 2, int x = 9, int y = 8, bool enemyFirst = false, ITurnRandom random = null)
            => new TurnBattle(new BattleRoster(1, new[] { U("A", 150), U("B", 80) }, new[] { a, b }),
                new BattleRoster(2, new[] { U("X", enemyFirst ? 200 : 120), U("Y", 50) }, new[] { x, y }), random ?? new SeededTurnRandom(53));
        /// <summary>첫 턴 또는 다음 턴의 4단계까지 진행한다.</summary>
        /// <param name="battle">대상.</param>
        private static void Plan(TurnBattle battle) { for (int i = 0; i < 100 && battle.Phase != TurnPhase.Planning; i++) battle.Advance(); Assert.AreEqual(TurnPhase.Planning, battle.Phase); }
        /// <summary>필드 전원의 종료를 보완하고 확정한다.</summary>
        /// <param name="battle">대상.</param>
        private static void Confirm(TurnBattle battle) { foreach (string id in battle.Order) battle.ReserveEndTurn(id); battle.ConfirmReservations(); }
        /// <summary>지정 유닛의 행동 시작까지 진행한다.</summary>
        /// <param name="battle">대상.</param>
        /// <param name="id">기다릴 유닛.</param>
        private static void Start(TurnBattle battle, string id) { for (int i = 0; i < 100 && battle.ActiveUnitId != id; i++) battle.Advance(); Assert.AreEqual(id, battle.ActiveUnitId); }

        /// <summary>예상 좌표 계산은 실제 좌표·행동·상태·난수·로그를 바꾸지 않는다.</summary>
        [Test]
        public void PreviewSwapsAlliesWithoutMutatingBattleOrRandom()
        {
            // random/battle/events: 미리보기 전 기준 상태.
            var random = new CountingRandom(); var battle = Create(random: random); Plan(battle);
            battle.ReserveMove("A", 4); int events = battle.Events.Count, calls = random.Calls;
            for (int i = 0; i < 5; i++)
            {
                var preview = battle.PreviewMovement(1);
                Assert.AreEqual(4, preview["A"]); Assert.AreEqual(1, preview["B"]);
            }
            Assert.AreEqual(1, battle.Unit("A").Slot); Assert.AreEqual(2, battle.Unit("B").Slot);
            Assert.AreEqual(2, battle.Unit("A").RemainingSubActions); Assert.AreEqual(0, battle.Unit("A").MovementLockTurns);
            Assert.AreEqual(100, battle.Unit("A").CurrentHp); Assert.AreEqual(3, battle.Costs[1]);
            Assert.AreEqual(events, battle.Events.Count); Assert.AreEqual(calls, random.Calls);
            battle.ClearMovementReservations("A"); Assert.AreEqual(1, battle.PreviewMovement(1)["A"]);
        }
        /// <summary>확정 시 좌표가 유지되고 실행은 한 칸씩 진행한다. 자원은 시도당 한 번 소비한다.</summary>
        [Test]
        public void ExecutionMovesOneCellAtATimeBeforeMainAndEnd()
        {
            var battle = Create(); Plan(battle); battle.ReserveMove("A", 4); battle.ReserveMainSkill("A", "hit", "X"); Confirm(battle);
            Assert.AreEqual(1, battle.Unit("A").Slot); Assert.Throws<InvalidOperationException>(() => battle.PreviewMovement(1));
            Start(battle, "A"); battle.Advance();
            Assert.AreEqual(2, battle.Unit("A").Slot); Assert.AreEqual(1, battle.Unit("B").Slot);
            Assert.AreEqual(1, battle.Unit("A").RemainingSubActions); Assert.AreEqual(0, battle.Unit("B").MovementLockTurns);
            Assert.AreEqual(100, battle.Unit("X").CurrentHp);
            battle.Advance(); Assert.AreEqual(3, battle.Unit("A").Slot);
            battle.Advance(); Assert.AreEqual(4, battle.Unit("A").Slot); Assert.AreEqual(2, battle.Unit("A").MovementLockTurns);
            Assert.AreEqual(1, battle.Unit("A").RemainingSubActions);
            battle.Advance(); Assert.AreEqual(70, battle.Unit("X").CurrentHp); Assert.AreEqual(2, battle.Unit("A").MovementLockTurns);
            battle.Advance(); Assert.AreEqual(1, battle.Unit("A").MovementLockTurns); Assert.IsTrue(battle.HasCompleted("A"));
        }
        /// <summary>이동 불가는 초기화가 아니라 자기 종료에서만 감소한다.</summary>
        [Test]
        public void MovementLockExpiresOnlyAtOwnerEndTurn()
        {
            var battle = Create(); Plan(battle); battle.ReserveMove("A", 4); Confirm(battle); Start(battle, "A");
            battle.Advance(); battle.Advance(); battle.Advance(); battle.Advance(); Plan(battle);
            Assert.AreEqual(1, battle.Unit("A").MovementLockTurns);
            Confirm(battle); Start(battle, "A"); Assert.AreEqual(1, battle.Unit("A").MovementLockTurns);
            battle.Advance(); Assert.AreEqual(0, battle.Unit("A").MovementLockTurns);
            Plan(battle); battle.ReserveMove("A", 5); Confirm(battle); Start(battle, "A"); battle.Advance(); Assert.AreEqual(5, battle.Unit("A").Slot);
        }
        /// <summary>같은 차례의 추가 이동은 실패하고 서브를 소비하지만 기존 2회를 재부여하지 않는다.</summary>
        [Test]
        public void SecondMoveFailsButConsumesOneSubAndStillAllowsMain()
        {
            var battle = Create(); Plan(battle); battle.ReserveMove("A", 4); battle.ReserveMove("A", 5); battle.ReserveMainSkill("A", "hit", "X");
            Assert.AreEqual(4, battle.PreviewMovement(1)["A"]); Confirm(battle); Start(battle, "A");
            battle.Advance(); battle.Advance(); battle.Advance(); battle.Advance();
            Assert.AreEqual(4, battle.Unit("A").Slot); Assert.AreEqual(0, battle.Unit("A").RemainingSubActions);
            Assert.AreEqual(2, battle.Unit("A").MovementLockTurns); Assert.AreEqual(2, battle.Events.Count(e => e.Kind == TurnEventKind.MovementFinished));
            StringAssert.Contains("재부여 없음", battle.Events.Last().Message);
            battle.Advance(); Assert.AreEqual(70, battle.Unit("X").CurrentHp);
        }
        /// <summary>적의 비공개 예약은 예측하지 않고 실행 시 새로 막힌 경로에서 부분 이동한다.</summary>
        [Test]
        public void HiddenEnemyMoveBlocksExecutionWithoutLeakingIntoPreview()
        {
            var battle = Create(2, 1, 7, 9, true); Plan(battle);
            battle.ReserveMove("A", 5); var before = battle.PreviewMovement(1);
            battle.ReserveMove("X", 4); Assert.AreEqual(before["A"], battle.PreviewMovement(1)["A"]); Assert.AreEqual(7, battle.PreviewMovement(1)["X"]);
            Confirm(battle); Start(battle, "A"); Assert.AreEqual(4, battle.Unit("X").Slot);
            battle.Advance(); Assert.AreEqual(3, battle.Unit("A").Slot);
            battle.Advance(); Assert.AreEqual(3, battle.Unit("A").Slot); Assert.AreEqual(2, battle.Unit("A").MovementLockTurns);
            Assert.AreEqual(1, battle.Unit("A").RemainingSubActions);
        }
        /// <summary>예약 후 바로 옆에 적이 오면 한 칸도 움직이지 않고 시도를 끝낸다.</summary>
        [Test]
        public void AdjacentEnemyAtExecutionPreventsAllMovement()
        {
            var battle = Create(3, 1, 7, 9, true); Plan(battle); battle.ReserveMove("A", 5); battle.ReserveMove("X", 4);
            Confirm(battle); Start(battle, "A"); battle.Advance();
            Assert.AreEqual(3, battle.Unit("A").Slot); Assert.AreEqual(2, battle.Unit("A").MovementLockTurns);
            Assert.IsFalse(battle.Events.Any(e => e.Kind == TurnEventKind.MovementStep && e.UnitId == "A"));
        }
        /// <summary>예약 입력의 경계 오류는 기존 계획과 실제 상태를 보존한다.</summary>
        [TestCase(0)] [TestCase(10)] [TestCase(1)] [TestCase(5)]
        public void InvalidDestinationDoesNotChangeReservations(int destination)
        {
            var battle = Create(); Plan(battle);
            Assert.Throws<InvalidOperationException>(() => battle.ReserveMove("A", destination));
            Assert.AreEqual(0, battle.MovementReservations("A").Count); Assert.IsFalse(battle.HasEndTurnReservation("A"));
        }
        /// <summary>보이는 적의 칸과 그 너머는 예약할 수 없다.</summary>
        [TestCase(3)] [TestCase(4)]
        public void VisibleEnemyCannotBeTargetedOrCrossed(int destination)
        {
            var battle = Create(1, 2, 3, 9); Plan(battle); Assert.Throws<InvalidOperationException>(() => battle.ReserveMove("A", destination));
        }
        /// <summary>예약 횟수 상한과 선택적 취소가 메인 및 종료와 독립적인지 검사한다.</summary>
        [Test]
        public void CancellationAndCapacityPreserveUnrelatedPlans()
        {
            var battle = Create(); Plan(battle); battle.ReserveMainSkill("A", "hit", "X"); battle.ReserveMove("A", 3); battle.ReserveMove("A", 4);
            Assert.Throws<InvalidOperationException>(() => battle.ReserveMove("A", 5)); Assert.AreEqual(2, battle.MovementReservations("A").Count);
            battle.ClearMovementReservations("A"); Assert.IsNotNull(battle.MainReservation("A")); Assert.IsTrue(battle.HasEndTurnReservation("A"));
            battle.ReserveMove("A", 4); battle.ClearReservation("A");
            Assert.IsNull(battle.MainReservation("A")); Assert.IsEmpty(battle.MovementReservations("A")); Assert.IsFalse(battle.HasEndTurnReservation("A"));
        }
        /// <summary>메인 금지와 이동 금지는 서로 독립적으로 판정한다.</summary>
        [TestCase(true)] [TestCase(false)]
        public void MainAndMovementRestrictionsAreIndependent(bool blockMain)
        {
            var battle = Create(1, 2, 6, 9); Plan(battle); battle.ReserveMove("A", 3); battle.ReserveMainSkill("A", "hit", "X"); Confirm(battle);
            battle.SetMainSkillBlocked("A", blockMain); battle.SetMovementBlocked("A", !blockMain); Start(battle, "A");
            for (int i = 0; i < 10 && !battle.HasCompleted("A"); i++) battle.Advance();
            Assert.AreEqual(blockMain ? 3 : 1, battle.Unit("A").Slot); Assert.AreEqual(blockMain ? 100 : 70, battle.Unit("X").CurrentHp);
            Assert.AreEqual(blockMain ? 1 : 0, battle.Unit("A").MovementLockTurns);
        }
        /// <summary>이동 도중 퇴각하면 다른 유닛이 남은 경로를 이어서 실행하지 않는다.</summary>
        [Test]
        public void RetreatDiscardsUnfinishedPath()
        {
            var battle = Create(); Plan(battle); battle.ReserveMove("A", 4); Confirm(battle); Start(battle, "A"); battle.Advance();
            battle.ApplyCompletedRetreatBatch(new[] { "A" }); Assert.IsEmpty(battle.MovementReservations("A"));
            Plan(battle); Assert.AreEqual(1, battle.Unit("B").Slot); Assert.IsNull(battle.Unit("A").Slot); Assert.AreEqual(0, battle.Unit("A").MovementLockTurns);
        }
        /// <summary>예측 사거리는 본인 메인 직전 위치이며 나중 아군 이동으로 바뀌지 않는다.</summary>
        [Test]
        public void SkillOriginExcludesLaterAllyMovement()
        {
            var battle = Create(); Plan(battle); battle.ReserveMove("A", 4); battle.ReserveMove("B", 4);
            Assert.AreEqual(4, battle.ProjectedSlotBeforeMain("A")); Assert.AreEqual(3, battle.PreviewMovement(1)["A"]);
        }
        /// <summary>대상은 이동한 슬롯이 아닌 유닛 ID를 추적하며 실행 때 사거리를 재검사한다.</summary>
        [Test]
        public void SkillTracksEnemyAfterMovementAndRechecksRange()
        {
            var battle = Create(1, 2, 6, 9, true); Plan(battle); battle.ReserveMainSkill("A", "hit", "X"); battle.ReserveMove("X", 8);
            Confirm(battle); Start(battle, "A"); battle.Advance(); Assert.AreEqual(100, battle.Unit("X").CurrentHp);
            Assert.IsTrue(battle.Events.Any(e => e.Kind == TurnEventKind.SkillCancelled)); Assert.AreEqual(4, battle.Unit("A").SkillBp("hit"));
        }
    }
}
