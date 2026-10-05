using System;
using System.Linq;
using MaseiKivotos.Core;
using NUnit.Framework;

namespace MaseiKivotos.Tests
{
    /// <summary>메인 스킬의 예약·피해·BP·COST·퇴각 경계를 검증한다.</summary>
    public sealed class MainSkillTests
    {
        /// <summary>테스트용 무속성 단일 공격을 만든다.</summary>
        /// <param name="id">스킬 ID.</param>
        /// <param name="power">위력.</param>
        /// <param name="accuracy">명중률.</param>
        /// <param name="cost">COST 비용.</param>
        /// <param name="min">최소 사거리.</param>
        /// <param name="max">최대 사거리.</param>
        private static MainSkill S(string id = "hit", decimal power = 20, int accuracy = 100, int cost = 1, int min = 0, int max = 8)
            => new MainSkill(id, id, SkillType.Tech, CombatTrait.Neutral, power, accuracy, 3, cost, min, max);

        /// <summary>HP 100의 테스트 유닛 정의를 만든다.</summary>
        /// <param name="id">유닛 ID.</param>
        /// <param name="speed">행동 순서용 Speed.</param>
        /// <param name="skills">소유 스킬.</param>
        private static UnitDefinition U(string id, int speed, params MainSkill[] skills)
            => new UnitDefinition(id, id, new CombatStats(100, 10, 20, 10, 20, speed), skills: skills);

        /// <summary>두 세력에 필드 두 명씩 만든다. 같은 스킬 정의를 공유해도 현재 BP는 독립적이다.</summary>
        /// <param name="skills">아군이 소유할 공격들.</param>
        private static TurnBattle Create(params MainSkill[] skills) => new TurnBattle(
            new BattleRoster(1, new[] { U("A", 150, skills), U("B", 80, skills) }, new[] { 1, 2 }),
            new BattleRoster(2, new[] { U("X", 120), U("Y", 50) }, new[] { 9, 8 }), new SeededTurnRandom(53));

        /// <summary>초기 세 단계를 진행한다.</summary>
        /// <param name="battle">대상 전투.</param>
        private static void Plan(TurnBattle battle) { battle.Advance(); battle.Advance(); battle.Advance(); Assert.AreEqual(TurnPhase.Planning, battle.Phase); }

        /// <summary>기존 스킬을 유지하면서 마무리 예약을 채우고 확정한다.</summary>
        /// <param name="battle">대상 전투.</param>
        private static void Confirm(TurnBattle battle)
        {
            foreach (string id in battle.Order) battle.ReserveEndTurn(id);
            battle.ConfirmReservations();
        }

        /// <summary>예약은 자원을 소비하지 않으며 메인 실행 뒤에도 종료 행동이 남는지 검사한다.</summary>
        [Test]
        public void ReservationPreservesResourcesAndSkillExecutesOnceBeforeEndTurn()
        {
            // battle: 예약/실행과 별도 종료를 검사할 전투.
            var battle = Create(S()); Plan(battle);
            battle.ReserveMainSkill("A", "hit", "X");
            Assert.AreEqual(3, battle.Costs[1]); Assert.AreEqual(3, battle.Unit("A").SkillBp("hit"));
            Assert.AreEqual(1, battle.ReservedCost(1));
            Confirm(battle); battle.Advance(); battle.Advance();
            Assert.AreEqual(70, battle.Unit("X").CurrentHp); Assert.AreEqual(2, battle.Unit("A").SkillBp("hit"));
            Assert.AreEqual(3, battle.Unit("B").SkillBp("hit")); Assert.AreEqual(2, battle.Costs[1]);
            Assert.AreEqual("A", battle.ActiveUnitId); Assert.AreEqual(0, battle.Unit("A").RemainingMainActions);
            Assert.IsFalse(battle.HasCompleted("A")); Assert.AreEqual(0, battle.ReservedCost(1));
            battle.Advance(); Assert.IsTrue(battle.HasCompleted("A")); Assert.AreEqual(70, battle.Unit("X").CurrentHp);
            Assert.AreEqual(1, battle.Events.Count(e => e.Kind == TurnEventKind.SkillHit));
            Assert.Throws<InvalidOperationException>(() => battle.ReserveMainSkill("B", "hit", "Y"));
        }

        /// <summary>공유 자원의 과다 예약을 막고 수정 실패 시 이전 예약을 유지한다.</summary>
        [Test]
        public void SharedCostReservationIsAtomicAndCancellationReleasesIt()
        {
            // battle: 비용이 다른 두 스킬을 가진 전투.
            var battle = Create(S("cheap"), S("heavy", cost: 2)); Plan(battle);
            battle.ReserveMainSkill("A", "heavy", "X"); battle.ReserveMainSkill("B", "cheap", "Y");
            Assert.Throws<InvalidOperationException>(() => battle.ReserveMainSkill("B", "heavy", "Y"));
            Assert.AreEqual("cheap", battle.MainReservation("B").SkillId); Assert.AreEqual(3, battle.ReservedCost(1));
            battle.ReserveMainSkill("A", "cheap", "X"); Assert.AreEqual(2, battle.ReservedCost(1));
            battle.ClearReservation("A"); Assert.IsNull(battle.MainReservation("A")); Assert.AreEqual(1, battle.ReservedCost(1));
            Assert.AreEqual(3, battle.Costs[1]);
        }

        /// <summary>명중 실패도 실제 실행이므로 BP/COST/메인을 소비한다.</summary>
        [Test]
        public void MissConsumesResourcesWithoutDamage()
        {
            // battle: 반드시 빗나가는 스킬 전투.
            var battle = Create(S(accuracy: 0)); Plan(battle); battle.ReserveMainSkill("A", "hit", "X"); Confirm(battle);
            battle.Advance(); battle.Advance();
            Assert.AreEqual(100, battle.Unit("X").CurrentHp); Assert.AreEqual(2, battle.Costs[1]);
            Assert.AreEqual(2, battle.Unit("A").SkillBp("hit")); Assert.AreEqual(0, battle.Unit("A").RemainingMainActions);
            Assert.AreEqual(TurnEventKind.SkillMissed, battle.Events.Last().Kind);
        }

        /// <summary>최소/최대 거리를 포함하고 범위 밖에서는 BP/메인만 소비한다.</summary>
        /// <param name="min">최소 거리.</param>
        /// <param name="max">최대 거리.</param>
        /// <param name="hit">거리 8의 대상이 범위 안인지 여부.</param>
        [TestCase(8, 8, true)]
        [TestCase(0, 7, false)]
        [TestCase(0, 8, true)]
        public void RangeIsRecheckedAndInvalidSkillDoesNotSpendCost(int min, int max, bool hit)
        {
            // battle: A1에서 X9로 공격하는 전투.
            var battle = Create(S(min: min, max: max)); Plan(battle); battle.ReserveMainSkill("A", "hit", "X"); Confirm(battle);
            battle.Advance(); battle.Advance();
            Assert.AreEqual(hit ? 70 : 100, battle.Unit("X").CurrentHp);
            Assert.AreEqual(hit ? 2 : 3, battle.Costs[1]); Assert.AreEqual(2, battle.Unit("A").SkillBp("hit"));
        }

        /// <summary>확정 뒤 대상이 퇴각하면 다른 적으로 바꾸지 않고 취소한다.</summary>
        [Test]
        public void RetiredTargetCancelsMainButEndTurnStillRuns()
        {
            // battle: 대상 X가 실행 전에 사라지는 전투.
            var battle = Create(S()); Plan(battle); battle.ReserveMainSkill("A", "hit", "X"); Confirm(battle);
            battle.ApplyCompletedRetreatBatch(new[] { "X" }); battle.Advance(); battle.Advance();
            Assert.AreEqual(100, battle.Unit("Y").CurrentHp); Assert.AreEqual(3, battle.Costs[1]);
            Assert.AreEqual(2, battle.Unit("A").SkillBp("hit"));
            battle.Advance(); Assert.IsTrue(battle.HasCompleted("A"));
        }

        /// <summary>확정 당시 BP가 있었지만 실행 전에 고갈되면 대체 없이 취소한다.</summary>
        [Test]
        public void BpLostAfterConfirmationCancelsWithoutNegativeBp()
        {
            // battle: 실행 직전 BP 소모 효과가 생긴 전투.
            var battle = Create(S()); Plan(battle); battle.ReserveMainSkill("A", "hit", "X"); Confirm(battle);
            Assert.IsFalse(battle.MainReservation("A").ZeroBpAtConfirmation);
            battle.ApplySkillBpChange("A", "hit", -3); battle.Advance(); battle.Advance();
            Assert.AreEqual(0, battle.Unit("A").SkillBp("hit")); Assert.AreEqual(3, battle.Costs[1]);
            Assert.AreEqual(100, battle.Unit("X").CurrentHp); Assert.AreEqual(TurnEventKind.SkillCancelled, battle.Events.Last().Kind);
        }

        /// <summary>확정 때 BP가 0이어도 실행 전 회복하면 원래 스킬을 실행한다.</summary>
        [Test]
        public void RecoveredBpUsesOriginalSkillAndHpBpPersistAcrossTurns()
        {
            // battle: 첫 초기화 단계에서 BP를 비운 뒤 실행 전에 회복하는 전투.
            var battle = Create(S()); battle.Advance(); battle.ApplySkillBpChange("A", "hit", -3); battle.Advance(); battle.Advance();
            battle.ReserveMainSkill("A", "hit", "X"); Confirm(battle); Assert.IsTrue(battle.MainReservation("A").ZeroBpAtConfirmation);
            battle.ApplySkillBpChange("A", "hit", 1); battle.Advance(); battle.Advance();
            Assert.AreEqual(70, battle.Unit("X").CurrentHp); Assert.AreEqual(0, battle.Unit("A").SkillBp("hit"));
            for (int i = 0; i < 20 && battle.TurnNumber == 1; i++) battle.Advance();
            Plan(battle);
            Assert.AreEqual(70, battle.Unit("X").CurrentHp); Assert.AreEqual(0, battle.Unit("A").SkillBp("hit"));
            Assert.IsNull(battle.MainReservation("A")); Assert.AreEqual(4, battle.Costs[1]);
        }

        /// <summary>치명적인 공격으로 퇴각한 적의 차례를 건너뛰고 전멸 즉시 다음 턴 없이 끝낸다.</summary>
        [Test]
        public void LethalSkillsRetireTargetsAndStopBattleBeforeNextTurn()
        {
            // battle: 아군 각자가 다른 적을 한 번에 처치하는 전투.
            var battle = Create(S(power: 100, cost: 0)); Plan(battle);
            battle.ReserveMainSkill("A", "hit", "X"); battle.ReserveMainSkill("B", "hit", "Y"); Confirm(battle);
            for (int i = 0; i < 20 && battle.Phase != TurnPhase.Ended; i++) battle.Advance();
            Assert.AreEqual(BattleOutcome.PlayerVictory, battle.Outcome); Assert.AreEqual(1, battle.TurnNumber);
            Assert.AreEqual(UnitLocation.Retreated, battle.Unit("X").Location); Assert.AreEqual(0, battle.Unit("Y").CurrentHp);
            Assert.IsFalse(battle.Events.Any(e => e.Kind == TurnEventKind.Started && e.UnitId == "X"));
            Assert.IsFalse(battle.Advance());
        }

        /// <summary>분류별 능력치, 복합 상성 제한, 자속, 최종 올림과 피해 0을 검사한다.</summary>
        [Test]
        public void DamageUsesTypeTraitCapStabAndOnlyFinalCeiling()
        {
            // attacker/defender: 태크 기본 피해 20에 상성 2와 자속 1.5를 적용하는 정의.
            var attacker = new UnitDefinition("A", "A", new CombatStats(100, 100, 30, 1, 1, 1), traits: new[] { CombatTrait.Flame });
            var defender = new UnitDefinition("X", "X", new CombatStats(100, 1, 1, 200, 0, 1), traits: new[] { CombatTrait.Electric, CombatTrait.Metal });
            Assert.AreEqual(60, SkillDamage.Calculate(attacker, defender, new MainSkill("t", "t", SkillType.Tech, CombatTrait.Flame, 40, 100, 1)));
            Assert.AreEqual(1, SkillDamage.Calculate(attacker, defender, new MainSkill("m", "m", SkillType.Mystic, CombatTrait.Neutral, .01m, 100, 1)));
            Assert.AreEqual(0, SkillDamage.Calculate(attacker, defender, S(power: 0)));
            Assert.AreEqual(1m, SkillDamage.Matchup(CombatTrait.Neutral, CombatTrait.Pure));
            Assert.AreEqual(2m, SkillDamage.Matchup(CombatTrait.Pure, CombatTrait.Neutral));
            Assert.AreEqual(2m, SkillDamage.Matchup(CombatTrait.Holy, CombatTrait.Dark));
            Assert.AreEqual(2m, SkillDamage.Matchup(CombatTrait.Dark, CombatTrait.Holy));
            Assert.AreEqual(1m, SkillDamage.Matchup(CombatTrait.Flame, CombatTrait.Pure));
        }

        /// <summary>원문 11절 표를 독립 행렬로 대조한다. 각 문자는 2배/1배/0.5배를 뜻한다.</summary>
        /// <param name="attack">공격 속성.</param>
        /// <param name="expected">염·습·전·폭·금·신·성·암·순·사·무 순서의 기대 배율.</param>
        [TestCase(CombatTrait.Flame,   "1h2h2111111")]
        [TestCase(CombatTrait.Wet,     "21h21h11111")]
        [TestCase(CombatTrait.Electric,"h211h211111")]
        [TestCase(CombatTrait.Blast,   "2h112h11111")]
        [TestCase(CombatTrait.Metal,   "h12h1211111")]
        [TestCase(CombatTrait.Divine,  "12h2h111111")]
        [TestCase(CombatTrait.Holy,    "11111112h21")]
        [TestCase(CombatTrait.Dark,    "11111121h21")]
        [TestCase(CombatTrait.Pure,    "hhhhhh221h2")]
        [TestCase(CombatTrait.Death,   "111111hh211")]
        [TestCase(CombatTrait.Neutral, "11111111111")]
        public void EntireTraitTableMatchesSource(CombatTrait attack, string expected)
        {
            for (int i = 0; i < expected.Length; i++)
                Assert.AreEqual(expected[i] == 'h' ? .5m : expected[i] == '2' ? 2m : 1m,
                    SkillDamage.Matchup(attack, (CombatTrait)i), attack + " → " + (CombatTrait)i);
        }

        /// <summary>이중 약세의 하한과 자속 불일치, 나눗셈의 중간 올림 방지를 확인한다.</summary>
        [Test]
        public void DamageKeepsFractionsUntilTheEndAndClampsDoubleResistance()
        {
            // attacker/defender: 태크 10/3 × 위력 3 × 이중 약세 하한 0.5 = 최종 5.
            var attacker = new UnitDefinition("A", "A", new CombatStats(100, 10, 99, 1, 1, 1));
            var defender = new UnitDefinition("X", "X", new CombatStats(100, 1, 1, 3, 1, 1), traits: new[] { CombatTrait.Wet, CombatTrait.Blast });
            Assert.AreEqual(5, SkillDamage.Calculate(attacker, defender, new MainSkill("fire", "염", SkillType.Tech, CombatTrait.Flame, 3, 100, 1)));
        }

        /// <summary>스킬 수·중복 ID·잘못된 소유/대상 및 속성 입력을 거부한다.</summary>
        [Test]
        public void InvalidDefinitionsAndReservationsAreRejected()
        {
            Assert.Throws<ArgumentException>(() => U("A", 1, S(), S()));
            Assert.Throws<ArgumentException>(() => U("A", 1, Enumerable.Range(0, 5).Select(i => S(i.ToString())).ToArray()));
            Assert.Throws<ArgumentOutOfRangeException>(() => S(accuracy: 101));
            Assert.Throws<NotSupportedException>(() => new MainSkill("change", "변화", SkillType.Change, CombatTrait.Neutral, 0, 100, 1));
            Assert.AreEqual("직접 작성한 효과", new MainSkill("named", "효과", SkillType.Tech, CombatTrait.Neutral, 1, 100, 1, effectDescription: "직접 작성한 효과").EffectDescription);
            // battle: 입력 거부가 기존 자원에 영향 없는지 확인할 전투.
            var battle = Create(S()); Plan(battle);
            Assert.Throws<ArgumentException>(() => battle.ReserveMainSkill("A", "unknown", "X"));
            Assert.Throws<InvalidOperationException>(() => battle.ReserveMainSkill("A", "hit", "B"));
            Assert.IsNull(battle.MainReservation("A")); Assert.AreEqual(0, battle.ReservedCost(1));
        }
    }
}
