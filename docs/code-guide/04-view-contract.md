# 04. 두 화면의 공통 약속 — TurnBattleView

기준: 2026-10-01 · 커밋 f69ceba · 검토 대기

[소스 파일 열기](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnBattleView.cs)

컨트롤러가 어떤 화면을 사용하든 동일한 방식으로 구성하고 갱신하도록 만든 추상 클래스다. 실제 텍스트 생성이나 배치는 두 파생 클래스가 담당한다.

## 공통으로 요구하는 것

Build는 화면 오브젝트를 만들고 버튼을 Controller에 연결한다. Refresh는 이미 만들어 둔 화면에 현재 Battle 값을 반영한다. 이 둘을 분리했으므로 차례가 바뀔 때마다 전체 UI를 새로 만들 필요가 없다.

TurnText와 네 버튼은 테스트와 컨트롤러가 공통으로 접근하는 창구다. protected set이라 파생 화면은 참조를 지정할 수 있지만 일반 외부 코드는 다른 버튼으로 바꿀 수 없다.

VisibleCost와 VisibleActor는 화면에 실제 표시한 문구를 읽는 속성이다. 테스트가 Core 값만 맞는지 검사하는 데 그치지 않고, 화면 표시도 맞는지 비교할 수 있다.

## 호출 관계

TurnSandboxController.Awake가 Build를 호출한다. ResetBattle·예약 토글·단계 진행·자동 진행 시작과 종료·재활성화에서 Refresh가 호출된다. TurnBattleView 자신은 턴을 계산하지 않는다.

StageBattleView는 기존 InGame 리소스를 연결하고, TurnSandboxView는 검증 UI를 새로 만든다. 두 화면 모두 이 계약을 만족하므로 Controller는 구체적인 화면 배치를 알 필요가 없다.

## 변수·함수 상세

소스 선언 순서로 정리했다. 원본 링크에서 실제 코드 위치를 바로 열 수 있다. 함수 내부 변수는 해당 함수 아래에 묶었다.

## TurnBattleView

턴 화면이 제공해야 할 공통 표시와 조작 연결 계약. 게임 규칙은 컨트롤러와 Core에서 처리한다. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnBattleView.cs#L10)

- **TurnBattleView.TurnText**: 현재 전체 턴 번호를 표시하는 텍스트. [원본 15행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnBattleView.cs#L15)

- **TurnBattleView.SkipButton**: 다음 턴의 예약 단계까지 자동 진행하는 턴 넘기기 버튼. [원본 19행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnBattleView.cs#L19)

- **TurnBattleView.StepButton**: 예약 확정 또는 다음 단계/차례를 한 번 실행하는 버튼. [원본 23행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnBattleView.cs#L23)

- **TurnBattleView.ResetButton**: 진행 중인 전투를 취소하고 첫 턴으로 초기화하는 버튼. [원본 27행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnBattleView.cs#L27)

- **TurnBattleView.ExampleButton**: 기본 Speed 예제와 동률 예제를 바꿔 시작하는 버튼. [원본 31행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnBattleView.cs#L31)

- **TurnBattleView.VisibleCost**: 화면에 표시한 COST 문구. UI 연결 검증에서도 조회한다. [원본 35행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnBattleView.cs#L35)

- **TurnBattleView.VisibleActor**: 화면에 표시한 현재 행동 차례 문구. 실제 Core 상태와의 일치를 검증할 때 조회한다. [원본 39행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnBattleView.cs#L39)

### TurnBattleView.Build

화면 리소스를 구성하고 각 버튼을 전달받은 컨트롤러에 연결한다.

선언: `public abstract void Build(TurnSandboxController controller)`. [원본 44행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnBattleView.cs#L44)

추상 함수이므로 이 파일에는 실행 본문이 없다. controller를 받아 버튼 이벤트를 연결하는 구현을 파생 클래스에서 제공해야 한다.

**입력값**

- `TurnSandboxController controller`: 전투 상태와 사용자 조작을 제공하는 공용 컨트롤러.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnBattleView.Refresh

컨트롤러의 최신 전투 상태를 읽어 화면 표시와 입력 가능 여부를 갱신한다.

선언: `public abstract void Refresh(TurnSandboxController controller)`. [원본 49행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnBattleView.cs#L49)

추상 함수. controller.Battle과 IsAdvancing 등을 읽어 표시와 입력 가능 상태를 동기화한다.

**입력값**

- `TurnSandboxController controller`: 전투 상태와 사용자 조작을 제공하는 공용 컨트롤러.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.
