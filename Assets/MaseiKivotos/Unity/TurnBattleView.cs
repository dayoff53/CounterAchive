using UnityEngine;
using UnityEngine.UI;

namespace MaseiKivotos.Unity
{

    /// <summary>
    /// 턴 화면이 제공해야 할 공통 표시와 조작 연결 계약. 게임 규칙은 컨트롤러와 Core에서 처리한다.
    /// </summary>
    public abstract class TurnBattleView : MonoBehaviour
    {
        /// <summary>
        /// 현재 전체 턴 번호를 표시하는 텍스트.
        /// </summary>
        public Text TurnText { get; protected set; }
        /// <summary>
        /// 다음 턴의 예약 단계까지 자동 진행하는 턴 넘기기 버튼.
        /// </summary>
        public Button SkipButton { get; protected set; }
        /// <summary>
        /// 예약 확정 또는 다음 단계/차례를 한 번 실행하는 버튼.
        /// </summary>
        public Button StepButton { get; protected set; }
        /// <summary>
        /// 진행 중인 전투를 취소하고 첫 턴으로 초기화하는 버튼.
        /// </summary>
        public Button ResetButton { get; protected set; }
        /// <summary>
        /// 기본 Speed 예제와 동률 예제를 바꿔 시작하는 버튼.
        /// </summary>
        public Button ExampleButton { get; protected set; }
        /// <summary>
        /// 화면에 표시한 COST 문구. UI 연결 검증에서도 조회한다.
        /// </summary>
        public abstract string VisibleCost { get; }
        /// <summary>
        /// 화면에 표시한 현재 행동 차례 문구. 실제 Core 상태와의 일치를 검증할 때 조회한다.
        /// </summary>
        public abstract string VisibleActor { get; }
        /// <summary>
        /// 화면 리소스를 구성하고 각 버튼을 전달받은 컨트롤러에 연결한다.
        /// </summary>
        /// <param name="controller">전투 상태와 사용자 조작을 제공하는 공용 컨트롤러.</param>
        public abstract void Build(TurnSandboxController controller);
        /// <summary>
        /// 컨트롤러의 최신 전투 상태를 읽어 화면 표시와 입력 가능 여부를 갱신한다.
        /// </summary>
        /// <param name="controller">전투 상태와 사용자 조작을 제공하는 공용 컨트롤러.</param>
        public abstract void Refresh(TurnSandboxController controller);
    }
}
