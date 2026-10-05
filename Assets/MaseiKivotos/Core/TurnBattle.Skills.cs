using System;
using System.Collections.Generic;
using System.Linq;

namespace MaseiKivotos.Core
{
    /// <summary>공통 턴 로직의 메인 스킬 예약·자원·실행 부분.</summary>
    public sealed partial class TurnBattle
    {
        /// <summary>유닛별 메인 스킬 한 개. 종료 예약과 독립적으로 보관한다.</summary>
        private readonly Dictionary<string, MainSkillReservation> mainReservations = new Dictionary<string, MainSkillReservation>();
        /// <summary>이번 차례에서 스킬 시도를 이미 처리한 유닛. 다음 진행은 턴 종료로 이어진다.</summary>
        private readonly HashSet<string> mainExecuted = new HashSet<string>();

        /// <summary>해당 유닛의 메인 예약을 조회한다. 상대 예약을 숨기는 것은 View의 책임이다.</summary>
        /// <param name="unitId">예약한 유닛 ID.</param>
        public MainSkillReservation MainReservation(string unitId) => mainReservations.TryGetValue(unitId, out var plan) ? plan : null;

        /// <summary>스킬 ID를 소유 유닛의 정의에서 찾는다.</summary>
        /// <param name="unitId">소유 유닛 ID.</param>
        /// <param name="skillId">소유 스킬 ID.</param>
        public MainSkill Skill(string unitId, string skillId) => Unit(unitId).Definition.Skills.FirstOrDefault(s => s.Id == skillId)
            ?? throw new ArgumentException("Unit does not own skill: " + skillId);

        /// <summary>예약에 확보했지만 아직 시도하지 않은 공유 COST 합계.</summary>
        /// <param name="factionId">조회할 세력.</param>
        public int ReservedCost(int factionId) => mainReservations.Where(p => Unit(p.Key).FactionId == factionId && !mainExecuted.Contains(p.Key))
            .Sum(p => Skill(p.Key, p.Value.SkillId).Cost);

        /// <summary>메인 한 개와 마지막 턴 종료를 함께 예약한다. 수정 시 이전 확보분을 돌려 계산하며 실패하면 기존 예약은 유지한다.</summary>
        /// <param name="unitId">현재 필드에 있는 사용자.</param>
        /// <param name="skillId">사용자가 가진 공격.</param>
        /// <param name="targetId">현재 필드의 적 대상. 사거리는 실행 직전에 다시 판정한다.</param>
        public void ReserveMainSkill(string unitId, string skillId, string targetId)
        {
            RequirePhase(TurnPhase.Planning);
            // actor/target/skill: 입력 전체를 검증한 다음 예약을 변경하기 위한 참조.
            var actor = Unit(unitId);
            var target = Unit(targetId);
            var skill = Skill(unitId, skillId);
            if (!order.Contains(unitId) || actor.Location != UnitLocation.Field || actor.RemainingMainActions < 1)
                throw new InvalidOperationException("Only a field unit with a main action may reserve.");
            if (target.Location != UnitLocation.Field || target.FactionId == actor.FactionId)
                throw new InvalidOperationException("Select an enemy field unit.");
            // previousCost: 예약 수정 중 자기 자신의 기존 확보분은 중복 합산하지 않는다.
            int previousCost = mainReservations.TryGetValue(unitId, out var previous) ? Skill(unitId, previous.SkillId).Cost : 0;
            if (ReservedCost(actor.FactionId) - previousCost + skill.Cost > costs[actor.FactionId])
                throw new InvalidOperationException("공유 COST가 부족합니다. 다른 예약을 변경하거나 취소하세요.");
            mainReservations[unitId] = new MainSkillReservation(skillId, targetId);
            reserved.Add(unitId);
            Record(TurnEventKind.Reserved, actor.Definition.Name + " · " + skill.Name + " → " + target.Definition.Name + " 예약", unitId);
        }

        /// <summary>후속 효과용 BP 변경 입구. 스킬별 상한을 지키며 턴 진행과 별도로 적용한다.</summary>
        /// <param name="unitId">BP가 바뀔 유닛.</param>
        /// <param name="skillId">소유 스킬.</param>
        /// <param name="delta">회복은 양수, 소모는 음수.</param>
        public void ApplySkillBpChange(string unitId, string skillId, int delta)
        {
            if (Phase != TurnPhase.Initialization && Phase != TurnPhase.Execution) throw new InvalidOperationException("BP effects require initialization or execution.");
            // actor/skill/value: 원래 BP에 변화량을 더한 뒤 범위를 제한한다. long으로 덧셈 오버플로를 막는다.
            var actor = Unit(unitId);
            var skill = Skill(unitId, skillId);
            long value = (long)actor.SkillBp(skillId) + delta;
            actor.SetSkillBp(skillId, (int)Math.Max(0, Math.Min(skill.MaxBp, value)));
        }

        /// <summary>외부 효과의 메인 스킬 사용 금지 여부를 반영한다. 효과 지속 시간이나 특정 상태 이상을 정의하지는 않는다.</summary>
        /// <param name="unitId">제한을 적용할 유닛.</param>
        /// <param name="blocked">금지는 true, 해제는 false.</param>
        public void SetMainSkillBlocked(string unitId, bool blocked)
        {
            if (Phase != TurnPhase.Initialization && Phase != TurnPhase.Execution) throw new InvalidOperationException("Skill restrictions require initialization or execution.");
            Unit(unitId).IsMainSkillBlocked = blocked;
        }

        /// <summary>확정된 스킬을 한 번 시도하고 현재 차례를 유지한다. 다음 Advance에서 마지막 턴 종료를 실행한다.</summary>
        /// <param name="actor">현재 행동권 소유 유닛.</param>
        /// <param name="plan">확정된 스킬과 대상.</param>
        private void ExecuteMainSkill(BattleUnit actor, MainSkillReservation plan)
        {
            // skill/bp: 실행 시점의 원래 스킬과 현재 BP. 원래 대상보다 사용자 제한과 대체 여부를 먼저 검사한다.
            var skill = Skill(actor.Id, plan.SkillId);
            int bp = actor.SkillBp(skill.Id);
            mainExecuted.Add(actor.Id);
            actor.RemainingMainActions = Math.Max(0, actor.RemainingMainActions - 1);
            if (actor.Location != UnitLocation.Field || actor.IsMainSkillBlocked)
            {
                if (bp > 0) actor.SetSkillBp(skill.Id, bp - 1);
                Record(TurnEventKind.SkillCancelled, actor.Definition.Name + " · 메인 스킬 사용 불가 (사용자 제한)", actor.Id);
                return;
            }
            if (bp == 0 && plan.ZeroBpAtConfirmation)
            {
                ExecuteHaloAnomaly(actor);
                return;
            }
            if (bp > 0) actor.SetSkillBp(skill.Id, bp - 1);
            // target: 유닛 지정은 슬롯 번호 대신 원래 ID로 추적한다.
            var target = Unit(plan.TargetId);
            // distance: 현재 슬롯의 절댓값 거리. 퇴각 대상에는 계산하지 않는다.
            int distance = target.Slot.HasValue && actor.Slot.HasValue ? Math.Abs(actor.Slot.Value - target.Slot.Value) : int.MaxValue;
            if (bp == 0 || target.Location != UnitLocation.Field || target.FactionId == actor.FactionId || distance < skill.MinRange || distance > skill.MaxRange || costs[actor.FactionId] < skill.Cost)
            {
                Record(TurnEventKind.SkillCancelled, actor.Definition.Name + " · " + skill.Name + " 취소 (BP/대상/사거리/COST 재검사)", actor.Id);
                return;
            }
            costs[actor.FactionId] -= skill.Cost;
            if (!RollHit(skill.Accuracy))
            {
                Record(TurnEventKind.SkillMissed, actor.Definition.Name + " · " + skill.Name + " 빗나감 (BP·COST 소비)", actor.Id);
                return;
            }
            // damage: 모든 상성·자속 보정을 마친 정수 피해.
            int damage = SkillDamage.Calculate(actor.Definition, target.Definition, skill);
            target.CurrentHp = Math.Max(0, target.CurrentHp - damage);
            Record(TurnEventKind.SkillHit, actor.Definition.Name + " · " + skill.Name + " → " + target.Definition.Name + " " + damage + " 피해 · HP " + target.CurrentHp, actor.Id);
            if (target.CurrentHp == 0) ApplyCompletedRetreatBatch(new[] { target.Id });
        }

        /// <summary>자신 중심 ±1칸의 피아 모두에 피해를 동시에 적용한 뒤 HP 소모·자체 BP 회복·승패를 처리한다.</summary>
        /// <param name="actor">이미 메인 1회를 소비한 시전자. 원래 스킬 BP/COST는 소비하지 않는다.</param>
        private void ExecuteHaloAnomaly(BattleUnit actor)
        {
            // skill/center: 대체 공격의 고정 정의와 실행 순간 시전자 위치.
            var skill = HaloAnomaly.Attack;
            int center = actor.Slot.Value;
            actor.HaloBp = Math.Max(0, actor.HaloBp - 1);
            Record(TurnEventKind.SkillReplaced, actor.Definition.Name + " · BP 0 → 헤일로 이상 (자신 중심 ±1칸)", actor.Id);
            // targets/hits/damages: 동일 공격 직전 상태에서 대상별 명중과 피해를 확정하는 스냅샷. 슬롯 순서로 난수를 소비한다.
            var targets = units.Where(u => u.Location == UnitLocation.Field && Math.Abs(u.Slot.Value - center) <= 1).OrderBy(u => u.Slot).ToArray();
            var hits = new bool[targets.Length];
            var damages = new int[targets.Length];
            for (int i = 0; i < targets.Length; i++)
            {
                hits[i] = RollHit(skill.Accuracy);
                damages[i] = hits[i] ? SkillDamage.Calculate(actor.Definition, targets[i].Definition, skill) : 0;
            }
            for (int i = 0; i < targets.Length; i++)
            {
                // target: 피해 계산이 모두 끝난 뒤 HP를 반영할 현재 대상.
                var target = targets[i];
                target.CurrentHp = Math.Max(0, target.CurrentHp - damages[i]);
                Record(hits[i] ? TurnEventKind.SkillHit : TurnEventKind.SkillMissed,
                    "헤일로 이상 → " + target.Definition.Name + (hits[i] ? " " + damages[i] + " 피해 · HP " + target.CurrentHp : " 빗나감"), actor.Id);
            }
            RetireWithoutVictory(targets.Where(u => u.CurrentHp == 0));
            // hpCost: 현재 HP가 아닌 최대 HP의 55%를 올림한다. 명중 실패·시전자 퇴각에도 필수 후속 처리를 끝낸다.
            int hpCost = (int)Math.Ceiling(actor.Definition.Stats.Hp * .55m);
            actor.CurrentHp = Math.Max(0, actor.CurrentHp - hpCost);
            actor.HaloBp = HaloAnomaly.MaxBp;
            Record(TurnEventKind.SkillFollowUp, actor.Definition.Name + " · 최대 HP 55% 소모 " + hpCost + " · HP " + actor.CurrentHp + " · 헤일로 BP " + actor.HaloBp, actor.Id);
            if (actor.CurrentHp == 0) RetireWithoutVictory(new[] { actor });
            EvaluateVictory();
        }

        /// <summary>정수 퍼센트 명중을 판정한다. 확정 명중/실패는 불필요한 난수를 소비하지 않는다.</summary>
        /// <param name="accuracy">0~100의 명중률.</param>
        private bool RollHit(int accuracy)
        {
            if (accuracy == 100) return true;
            if (accuracy == 0) return false;
            // roll: 0~99 균등 추첨. 잘못된 외부 난수 공급 결과는 예외로 알린다.
            int roll = random.Next(100);
            if (roll < 0 || roll >= 100) throw new InvalidOperationException("Invalid hit random result.");
            return roll < accuracy;
        }

    }
}
