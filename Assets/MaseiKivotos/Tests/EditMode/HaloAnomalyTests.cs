using System;
using System.Collections.Generic;
using System.Linq;
using MaseiKivotos.Core;
using NUnit.Framework;

namespace MaseiKivotos.Tests
{
    /// <summary>D-007로 확정된 BP 0 대체·범위 피해·필수 후속 처리·승패 경계를 검증한다.</summary>
    public sealed class HaloAnomalyTests
    {
        /// <summary>각 대상의 80% 경계를 재현하는 난수 공급기. 동률 없는 편성에서 명중에만 사용한다.</summary>
        private sealed class HitRandom : ITurnRandom
        {
            /// <summary>다음 대상부터 차례로 사용할 0~99 값.</summary>
            private readonly Queue<int> rolls;
            /// <summary>예정된 명중값을 복사한다.</summary>
            /// <param name="values">슬롯 순서로 소비할 추첨 값.</param>
            public HitRandom(params int[] values) { rolls = new Queue<int>(values); }
            /// <summary>기대한 명중 추첨만 허용하고 다음 값을 반환한다.</summary>
            /// <param name="exclusiveMax">명중 추첨의 상한 100.</param>
            public int Next(int exclusiveMax) { Assert.AreEqual(100, exclusiveMax); return rolls.Dequeue(); }
        }

        /// <summary>능력치와 원래 스킬을 가진 테스트 유닛을 만든다.</summary>
        /// <param name="id">유닛 ID.</param>
        /// <param name="speed">중복 없는 행동 속도.</param>
        /// <param name="hp">최대 HP.</param>
        /// <param name="mAtk">신비 공격력.</param>
        /// <param name="mDef">신비 방어력.</param>
        private static UnitDefinition U(string id, int speed, int hp = 100, int mAtk = 10, int mDef = 10)
            => new UnitDefinition(id, id, new CombatStats(hp, 10, mAtk, 10, mDef, speed), skills: new[] {
                new MainSkill("hit", "원래 스킬", SkillType.Tech, CombatTrait.Neutral, 20, 100, 3, 1, 0, 0)
            });

        /// <summary>원래 사거리 0인 스킬의 BP를 비우고 A의 실행 직전까지 진행한다.</summary>
        /// <param name="battle">준비할 전투.</param>
        private static void PrepareZero(TurnBattle battle)
        {
            battle.Advance(); battle.ApplySkillBpChange("A", "hit", -3); battle.Advance(); battle.Advance();
            battle.ReserveMainSkill("A", "hit", "X");
            foreach (string id in battle.Order) battle.ReserveEndTurn(id);
            battle.ConfirmReservations(); battle.Advance();
        }

        /// <summary>양 끝 슬롯의 범위 절단, 자신/아군 타격과 원래 BP/COST 보존을 확인한다.</summary>
        /// <param name="center">시전자 끝 슬롯.</param>
        /// <param name="neighbor">인접 아군 슬롯.</param>
        /// <param name="enemySlot">먼 적 시작 슬롯.</param>
        /// <param name="otherEnemy">나머지 적 슬롯.</param>
        [TestCase(1, 2, 9, 8)]
        [TestCase(9, 8, 1, 2)]
        public void EdgesIncludeSelfAndAllyWithoutSpendingOriginalCost(int center, int neighbor, int enemySlot, int otherEnemy)
        {
            // battle: 원래 타깃은 사거리 밖이지만 대체 범위는 자신과 인접 아군이다.
            var battle = new TurnBattle(new BattleRoster(1, new[] { U("A", 150, 50), U("B", 80) }, new[] { center, neighbor }),
                new BattleRoster(2, new[] { U("X", 120), U("Y", 50) }, new[] { enemySlot, otherEnemy }), new HitRandom(0, 0));
            PrepareZero(battle); Assert.AreEqual(1, battle.ReservedCost(1)); battle.Advance();
            Assert.AreEqual(UnitLocation.Retreated, battle.Unit("A").Location);
            Assert.AreEqual(25, battle.Unit("B").CurrentHp); Assert.AreEqual(100, battle.Unit("X").CurrentHp);
            Assert.AreEqual(0, battle.Unit("A").SkillBp("hit")); Assert.AreEqual(3, battle.Unit("B").SkillBp("hit"));
            Assert.AreEqual(40, battle.Unit("A").HaloBp); Assert.AreEqual(3, battle.Costs[1]); Assert.AreEqual(0, battle.ReservedCost(1));
            Assert.AreEqual(1, battle.Events.Count(e => e.Kind == TurnEventKind.SkillReplaced));
            Assert.Less(battle.Events.ToList().FindIndex(e => e.Kind == TurnEventKind.Retreated && e.UnitId == "A"),
                battle.Events.ToList().FindIndex(e => e.Kind == TurnEventKind.SkillFollowUp));
            battle.Advance(); Assert.AreEqual("X", battle.ActiveUnitId);
        }

        /// <summary>80% 명중 경계와 모든 대상 빗나감에서도 최대 HP 기준 올림 소모·자체 BP 회복을 확인한다.</summary>
        /// <param name="roll">0~99 추첨값.</param>
        /// <param name="selfHp">시전자 최종 HP.</param>
        /// <param name="allyHp">아군 최종 HP.</param>
        [TestCase(79, 0, 26)]
        [TestCase(80, 45, 101)]
        public void AccuracyBoundaryStillRunsMandatoryEffectsOnMiss(int roll, int selfHp, int allyHp)
        {
            // battle: 최대 HP 101이므로 55% 소모는 56이다.
            var battle = new TurnBattle(new BattleRoster(1, new[] { U("A", 150, 101), U("B", 80, 101) }, new[] { 1, 2 }),
                new BattleRoster(2, new[] { U("X", 120), U("Y", 50) }, new[] { 9, 8 }), new HitRandom(roll, roll));
            PrepareZero(battle); battle.Advance();
            Assert.AreEqual(selfHp, battle.Unit("A").CurrentHp); Assert.AreEqual(allyHp, battle.Unit("B").CurrentHp);
            Assert.AreEqual(40, battle.Unit("A").HaloBp); Assert.AreEqual(0, battle.Unit("A").RemainingMainActions);
            StringAssert.Contains("소모 56", battle.Events.Last(e => e.Kind == TurnEventKind.SkillFollowUp).Message);
            if (selfHp > 0) { Assert.AreEqual("A", battle.ActiveUnitId); battle.Advance(); Assert.IsTrue(battle.HasCompleted("A")); }
        }

        /// <summary>대상별 명중 추첨은 공유하지 않으며 이미 퇴각한 원래 대상 때문에 대체를 취소하지 않는다.</summary>
        [Test]
        public void MissingOriginalTargetStillReplacesAndHitsEachSlotIndependently()
        {
            // battle: A2를 중심으로 자신은 빗나가고 B3만 적중한다. X9 퇴각 후에도 Y8이 생존한다.
            var battle = new TurnBattle(new BattleRoster(1, new[] { U("A", 150), U("B", 80) }, new[] { 2, 3 }),
                new BattleRoster(2, new[] { U("X", 120), U("Y", 50) }, new[] { 9, 8 }), new HitRandom(80, 79));
            PrepareZero(battle); battle.ApplyCompletedRetreatBatch(new[] { "X" }); battle.Advance();
            Assert.AreEqual(45, battle.Unit("A").CurrentHp); Assert.AreEqual(25, battle.Unit("B").CurrentHp);
            Assert.AreEqual(1, battle.Events.Count(e => e.Kind == TurnEventKind.SkillMissed));
            Assert.AreEqual(1, battle.Events.Count(e => e.Kind == TurnEventKind.SkillHit));
            Assert.AreEqual(BattleOutcome.Ongoing, battle.Outcome);
        }

        /// <summary>대체가 행동 금지를 우회하지 않으며 BP는 음수가 되지 않는다.</summary>
        [Test]
        public void MainSkillRestrictionPrecedesReplacement()
        {
            // battle: 난수는 소비되면 안 되므로 빈 큐를 사용한다.
            var battle = new TurnBattle(new BattleRoster(1, new[] { U("A", 150), U("B", 80) }, new[] { 1, 2 }),
                new BattleRoster(2, new[] { U("X", 120), U("Y", 50) }, new[] { 9, 8 }), new HitRandom());
            PrepareZero(battle); battle.SetMainSkillBlocked("A", true); battle.Advance();
            Assert.AreEqual(100, battle.Unit("A").CurrentHp); Assert.AreEqual(0, battle.Unit("A").SkillBp("hit"));
            Assert.AreEqual(0, battle.Unit("A").RemainingMainActions); Assert.AreEqual(3, battle.Costs[1]);
            Assert.AreEqual(TurnEventKind.SkillCancelled, battle.Events.Last().Kind);
            Assert.IsFalse(battle.Events.Any(e => e.Kind == TurnEventKind.SkillReplaced));
            battle.Advance(); Assert.IsTrue(battle.HasCompleted("A"));
        }

        /// <summary>직전 범위 피해로 적이 전멸해도 HP 소모와 자체 BP 회복이 끝나기 전에는 승리를 확정하지 않는다.</summary>
        /// <param name="hp">시전자 최대 HP.</param>
        /// <param name="remaining">범위 피해 8과 55% 소모 후 기대 HP.</param>
        /// <param name="outcome">모든 후속 효과 뒤의 승패.</param>
        [TestCase(10, 0, BattleOutcome.PlayerDefeat)]
        [TestCase(50, 14, BattleOutcome.PlayerVictory)]
        public void EnemyWipeWaitsForSelfCostBeforeChoosingWinner(int hp, int remaining, BattleOutcome outcome)
        {
            // battle: X4/Y6는 75 피해로 퇴각한다. A5는 방어력 100으로 피해 8을 받고 이후 최대 HP 기준량을 소모한다.
            var battle = new TurnBattle(new BattleRoster(1, new[] { U("A", 150, hp, 10, 100), U("B", 80) }, new[] { 5, 1 }),
                new BattleRoster(2, new[] { U("X", 120, 60), U("Y", 50, 60) }, new[] { 4, 6 }), new HitRandom(0, 0, 0));
            PrepareZero(battle);
            battle.ApplyCompletedRetreatBatch(new[] { "B" });
            battle.Advance();
            Assert.AreEqual(remaining, battle.Unit("A").CurrentHp);
            Assert.AreEqual(40, battle.Unit("A").HaloBp);
            Assert.AreEqual(outcome, battle.Outcome);
            Assert.AreEqual(TurnEventKind.BattleEnded, battle.Events.Last().Kind);
            Assert.Less(battle.Events.ToList().FindIndex(e => e.Kind == TurnEventKind.SkillFollowUp), battle.Events.Count - 1);
            Assert.IsFalse(battle.Advance());
        }

        /// <summary>실행 전 퇴각한 사용자는 헤일로 이상을 발동하지 않고 다음 유효 유닛으로 넘어간다.</summary>
        [Test]
        public void RetreatedCasterNeverTriggersReplacement()
        {
            // battle: A가 사라져도 다른 양측 필드 유닛은 생존한다.
            var battle = new TurnBattle(new BattleRoster(1, new[] { U("A", 150), U("B", 80) }, new[] { 1, 2 }),
                new BattleRoster(2, new[] { U("X", 120), U("Y", 50) }, new[] { 9, 8 }), new HitRandom());
            PrepareZero(battle); battle.ApplyCompletedRetreatBatch(new[] { "A" }); battle.Advance();
            Assert.AreEqual("X", battle.ActiveUnitId);
            Assert.IsFalse(battle.Events.Any(e => e.Kind == TurnEventKind.SkillReplaced));
        }
    }
}
