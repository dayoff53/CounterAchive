using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MaseiKivotos.Core;
using UnityEngine;

namespace MaseiKivotos.Unity
{
    /// <summary>
    /// 두 화면에서 공유하는 개발용 턴 진행 컨트롤러. 임시 편성을 만들고 UI 입력을 Core에 전달하며 코루틴으로 턴 넘기기를 표시한다.
    /// </summary>
    public sealed class TurnSandboxController : MonoBehaviour
    {
        /// <summary>
        /// 자동 턴 넘기기에서 한 단계 처리 후 기다리는 실제 시간(초). 전투 규칙의 시간 비용은 아니다.
        /// </summary>
        [SerializeField, Min(0f)] private float stepDelay = 0.22f;
        /// <summary>
        /// 예제의 동률 추첨을 재현하기 위한 난수 시드. 전투 초기화 시 같은 시드로 다시 시작한다.
        /// </summary>
        [SerializeField] private int randomSeed = 530;
        /// <summary>
        /// 현재 실행 중인 전투 로직. 처음부터 버튼을 누르면 새 인스턴스로 교체한다.
        /// </summary>
        public TurnBattle Battle { get; private set; }
        /// <summary>
        /// 자동 턴 넘기기 코루틴의 실행 여부. 중복 진행과 예약 변경을 차단하는 데 사용한다.
        /// </summary>
        public bool IsAdvancing { get; private set; }
        /// <summary>
        /// 모든 임시 유닛의 Speed를 100으로 맞추는 동률 검증 모드 여부.
        /// </summary>
        public bool EqualSpeedExample { get; private set; }
        /// <summary>
        /// 단계 간 연출 대기 시간의 외부 접근 창구. 음수는 0으로 보정한다.
        /// </summary>
        public float StepDelay { get => stepDelay; set => stepDelay = Mathf.Max(0f, value); }
        /// <summary>
        /// 현재 씬이 사용하는 화면 구현. Stage_Battle과 독립 검증 씬을 같은 조작 코드로 갱신한다.
        /// </summary>
        public TurnBattleView View { get; private set; }
        /// <summary>두 씬에서 함께 사용하는 메인 스킬 선택 창.</summary>
        public MainSkillPanel SkillPanel { get; private set; }
        /// <summary>필드를 가리지 않고 이동을 추가하는 공용 예약 창.</summary>
        public MovementPanel MovePanel { get; private set; }
        /// <summary>4단계 표시 전용 좌표. null이면 실제 필드를 표시한다.</summary>
        public IReadOnlyDictionary<string, int> MovementPreview { get; private set; }

        /// <summary>
        /// 연결된 화면을 찾거나 기본 검증 화면을 만들고, 화면 구성 후 첫 전투를 초기화한다.
        /// </summary>
        private void Awake()
        {
            View = GetComponent<TurnBattleView>();
            if (View == null) View = gameObject.AddComponent<TurnSandboxView>();
            View.Build(this);
            SkillPanel = gameObject.AddComponent<MainSkillPanel>();
            SkillPanel.Build(this);
            MovePanel = gameObject.AddComponent<MovementPanel>();
            MovePanel.Build(this);
            ResetBattle();
        }

        /// <summary>
        /// 진행 중인 코루틴을 중단하고 임시 편성·COST·턴 번호를 처음 상태로 되돌린 뒤 1턴 예약 화면을 연다.
        /// </summary>
        public void ResetBattle()
        {
            StopAllCoroutines(); IsAdvancing = false;
            if (SkillPanel != null) SkillPanel.Close();
            if (MovePanel != null) MovePanel.Close();

            // 임시 편성으로 전투를 새로 만든다. 임시 유닛 정의는 Definition()에서 생성한다.
            Battle = new TurnBattle(
                new BattleRoster(1, new[] { Definition("A", "아군 A", 150), Definition("B", "아군 B", 80, 2), Definition("C", "대기 C", 40) }, new[] { 1, 2, 3 }),
                new BattleRoster(2, new[] { Definition("X", "적군 X", 120), Definition("Y", "적군 Y", 50), Definition("Z", "대기 Z", 30) }, new[] { 9, 8, 7 }),
                new SeededTurnRandom(randomSeed));

            while (Battle.Phase != TurnPhase.Planning) Battle.Advance();
            PlanSandboxEnemy();
            RefreshView();
        }

        /// <summary>
        /// 검증용 유닛 정의를 만든다. HP·공격·방어와 기본 편성은 임시 값이며 확정된 콘텐츠 데이터가 아니다.
        /// </summary>
        /// <param name="id">전투에서 유닛을 식별할 고유 ID.</param>
        /// <param name="name">UI와 진행 기록에 표시할 유닛 이름.</param>
        /// <param name="speed">0 이상인 기본 Speed.</param>
        /// <param name="subActions">임시 유닛의 기본 서브 행동 횟수.</param>
        private UnitDefinition Definition(string id, string name, int speed, int subActions = 1)
        {

            return new UnitDefinition(id, name, new CombatStats(100, 10, 10, 10, 10, EqualSpeedExample ? 100 : speed), subActions,
                new[] {
                    new MainSkill("shot", "기본 사격", SkillType.Tech, CombatTrait.Neutral, 20, 100, 5),
                    new MainSkill("mystic", "신비탄", SkillType.Mystic, CombatTrait.Neutral, 32, 85, 3, 1)
                }, moveRange: 3);
        }

        /// <summary>
        /// 기본 속도 예제와 전원 동률 예제를 전환하고 전투를 처음부터 다시 시작한다.
        /// </summary>
        public void ToggleExample()
        {
            EqualSpeedExample = !EqualSpeedExample;
            ResetBattle();
        }

        /// <summary>
        /// 예약 단계에서 아군 필드 유닛의 턴 종료 예약을 토글한다. 자동 진행 중에는 입력을 무시한다.
        /// </summary>
        /// <param name="id">전투에서 유닛을 식별할 고유 ID.</param>
        public void ToggleEndTurnReservation(string id)
        {
            if (IsAdvancing || Battle.Phase != TurnPhase.Planning) return;
            // unit: 종료 예약을 토글할 유닛. 알 수 없는 ID는 Battle.Unit에서 거부한다.
            var unit = Battle.Unit(id);
            if (unit.FactionId != Battle.PlayerFactionId || unit.Location != UnitLocation.Field) return;
            if (Battle.HasEndTurnReservation(id)) Battle.ClearReservation(id);
            else Battle.ReserveEndTurn(id);
            RefreshView();
        }

        /// <summary>
        /// 자동 진행 중이 아니면 현재 단계 또는 유닛 차례를 한 번만 진행한다.
        /// </summary>
        public void Step()
        {
            if (IsAdvancing || Battle.Phase == TurnPhase.Ended) return;
            AdvanceOne();
        }

        /// <summary>
        /// 중복 실행을 막은 뒤 다음 턴의 예약 단계까지 자동으로 진행한다. 전투가 종료되면 진행하지 않는다.
        /// </summary>
        public void SkipTurn()
        {
            if (IsAdvancing || Battle.Phase == TurnPhase.Ended) return;
            IsAdvancing = true;
            if (SkillPanel != null) SkillPanel.Close();
            if (MovePanel != null) MovePanel.Close();
            RefreshView();
            StartCoroutine(AdvanceToNextPlanning());
        }

        /// <summary>
        /// 단계별 대기 시간을 두며 다음 턴 예약까지 진행하는 코루틴. 진행 정체에 대비해 반복 횟수를 제한한다.
        /// </summary>
        private IEnumerator AdvanceToNextPlanning()
        {
            // sourceTurn: 자동 진행 시작 당시 전체 턴 번호. 다음 턴 예약 단계에 도달했는지 판정하는 기준이다.
            int sourceTurn = Battle.TurnNumber;

            // remainingSteps: 자동 진행의 무한 반복을 막는 횟수 예산. 전투 규칙의 행동 횟수와 무관하다.
            long remainingSteps = 32 + Battle.Units.Sum(u => 3L + (long)Battle.MovementReservations(u.Id).Count * (u.Definition.MoveRange + 1));
            while (Battle.Phase != TurnPhase.Ended && (Battle.TurnNumber == sourceTurn || Battle.Phase != TurnPhase.Planning))
            {
                if (--remainingSteps == 0) { Debug.LogError("Turn advancement did not reach its boundary."); break; }
                if (!AdvanceOne()) break;
                yield return new WaitForSecondsRealtime(stepDelay);
            }
            IsAdvancing = false;
            RefreshView();
        }

        /// <summary>
        /// 기존 스킬 예약을 유지한 채 마지막 턴 종료를 보완·확정하고, 다음 호출부터 Core를 한 번씩 진행한다. 전투 종료 시 false를 반환한다.
        /// </summary>
        private bool AdvanceOne()
        {
            // advanced: 전투가 이미 끝나 더 진행하지 않았으면 자동 반복을 즉시 끝내는 신호.
            bool advanced = true;
            if (Battle.Phase == TurnPhase.Planning)
            {

                // 스킬 예약과 종료 예약은 별개다. 여기서는 마무리만 추가하며 메인 스킬을 지우지 않는다.
                foreach (var unit in Battle.Units.Where(u => u.Location == UnitLocation.Field))
                    Battle.ReserveEndTurn(unit.Id);
                Battle.ConfirmReservations();
            }
            else advanced = Battle.Advance();
            if (Battle.Phase == TurnPhase.Planning) PlanSandboxEnemy();
            RefreshView();
            return advanced;
        }

        /// <summary>아군 스킬을 예약하고 즉시 화면을 갱신한다.</summary>
        /// <param name="unitId">현재 필드 아군 ID.</param>
        /// <param name="skillId">소유 스킬 ID.</param>
        /// <param name="targetId">필드 적군 ID.</param>
        public void ReserveSkill(string unitId, string skillId, string targetId)
        {
            if (IsAdvancing || Battle.Phase != TurnPhase.Planning) return;
            if (Battle.Unit(unitId).FactionId != Battle.PlayerFactionId) return;
            Battle.ReserveMainSkill(unitId, skillId, targetId);
            RefreshView();
        }

        /// <summary>선택 아군의 스킬을 지우고 턴 종료만 예약한다. 확보 COST도 해제한다.</summary>
        /// <param name="unitId">아군 ID.</param>
        public void ReserveOnlyEndTurn(string unitId)
        {
            if (IsAdvancing || Battle.Phase != TurnPhase.Planning || Battle.Unit(unitId).FactionId != Battle.PlayerFactionId) return;
            Battle.ClearReservation(unitId); Battle.ReserveEndTurn(unitId); RefreshView();
        }

        /// <summary>기본 화면과 스킬 선택 창에 동일한 최신 전투 상태를 반영한다.</summary>
        private void RefreshView()
        {
            MovementPreview = Battle.Phase == TurnPhase.Planning && Battle.Units.Any(u => u.FactionId == Battle.PlayerFactionId && Battle.MovementReservations(u.Id).Count > 0)
                ? Battle.PreviewMovement(Battle.PlayerFactionId) : null;
            View.Refresh(this);
            if (SkillPanel != null) SkillPanel.Refresh();
            if (MovePanel != null) MovePanel.Refresh();
        }

        /// <summary>미리보기 또는 실제 좌표로 해당 슬롯의 표시 유닛을 찾는다. 클릭 대상도 같은 좌표를 사용한다.</summary>
        /// <param name="slot">표시 슬롯 번호.</param>
        public BattleUnit DisplayedUnitAtSlot(int slot) => Battle.Units.FirstOrDefault(u => u.Location == UnitLocation.Field
            && (MovementPreview != null ? MovementPreview[u.Id] : u.Slot) == slot);

        /// <summary>아군 서브 이동을 추가하고 미리보기를 다시 계산한다.</summary>
        /// <param name="unitId">아군 ID.</param>
        /// <param name="destination">도착 슬롯.</param>
        public void ReserveMove(string unitId, int destination)
        {
            if (IsAdvancing || Battle.Phase != TurnPhase.Planning || Battle.Unit(unitId).FactionId != Battle.PlayerFactionId) return;
            Battle.ReserveMove(unitId, destination); RefreshView();
        }

        /// <summary>선택 아군의 이동만 취소하고 표시 좌표를 복구한다.</summary>
        /// <param name="unitId">아군 ID.</param>
        public void ClearMoves(string unitId)
        {
            if (IsAdvancing || Battle.Phase != TurnPhase.Planning || Battle.Unit(unitId).FactionId != Battle.PlayerFactionId) return;
            Battle.ClearMovementReservations(unitId); RefreshView();
        }

        /// <summary>
        /// 적 필드 유닛에 턴 종료를 자동 예약한다. 스킬을 선택하는 적 AI는 아직 구현하지 않았다.
        /// </summary>
        private void PlanSandboxEnemy()
        {
            foreach (var unit in Battle.Units.Where(u => u.FactionId != Battle.PlayerFactionId && u.Location == UnitLocation.Field))
                Battle.ReserveEndTurn(unit.Id);
        }

        /// <summary>
        /// 컴포넌트가 비활성화되면 자동 진행을 중단하고 중복 입력 차단 상태를 해제한다.
        /// </summary>
        private void OnDisable()
        {
            StopAllCoroutines(); IsAdvancing = false;
        }

        /// <summary>
        /// 이미 전투가 생성되어 있으면 재활성화 시 현재 상태로 화면을 다시 그린다.
        /// </summary>
        private void OnEnable()
        {
            if (Battle != null && View != null) RefreshView();
        }
    }
}
