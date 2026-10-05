# 03. 버튼에서 다음 턴까지 — TurnSandboxController

기준: 2026-10-01 · 커밋 f69ceba · 검토 대기

[소스 파일 열기](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs)

Unity 입력을 Core 호출로 번역하는 공용 진행자다. 이름에 Sandbox가 있지만 Stage_Battle과 독립 검증 씬이 함께 사용한다. 전투 규칙을 화면 클래스마다 중복해서 만들지 않도록 가운데에 둔 것이다.

## 처음 Play를 누르면

Awake → 화면 찾기 → View.Build(this) → ResetBattle → 임시 편성 생성 → TurnBattle 생성 → Planning까지 Advance → 적 종료 예약 → View.Refresh 순서다.

아군 세력은 1, A/B/C의 기본 Speed는 150/80/40이고 슬롯 구역은 1/2/3이다. 적은 세력 2, X/Y/Z의 Speed는 120/50/30이고 구역은 9/8/7이다. 앞 두 명만 배치되어 A1·B2·Y8·X9가 보인다. B의 기본 서브 횟수는 2, 나머지는 1이다. HP 100과 두 공격·방어 10은 테스트용 값이다.

## 세 가지 조작을 구분하기

- 아군 슬롯 클릭: ToggleEndTurnReservation으로 해당 유닛의 종료 예약만 켜거나 끈다. 턴은 진행하지 않는다.
- 다음 단계 / 차례: Step → AdvanceOne. 한 번만 진행한다. 예약 화면에서는 전원 종료 예약을 채우고 확정한다.
- 턴 넘기기: SkipTurn → AdvanceToNextPlanning → AdvanceOne 반복. 다음 전체 턴의 Planning까지 진행한다.

예약이 비어 있어도 Step/Skip가 진행되는 이유는 현재 유일한 행동인 턴 종료를 Controller가 모두 채워 주기 때문이다. Core의 ConfirmReservations 자체는 미예약을 거부한다. 두 동작은 모순이 아니며 호출 계층이 다르다.

## 자동 진행의 종료 조건

sourceTurn은 시작한 턴 번호를 기억한다. 번호가 바뀌었어도 아직 TurnStart·Initialization·Order라면 계속 진행한다. 다음 번호의 Planning에 도착해야 멈춘다. 따라서 화면은 TURN 02와 COST 5, 다시 예약할 수 있는 상태를 함께 보여 준다.

stepDelay 0.22초는 과정을 볼 수 있게 하는 연출 대기다. WaitForSecondsRealtime을 사용한다. 속도 능력치나 전투 시간 비용과 무관하다. remainingSteps는 무한 반복 방지용이며 감소 후 0을 검사하므로 최대 63회의 AdvanceOne을 허용한다.

## 초기화와 중복 입력

SkipTurn은 코루틴 시작 전에 IsAdvancing을 true로 만들고 화면을 갱신한다. 버튼을 빠르게 연속 클릭하거나 함수를 직접 두 번 불러도 중복 진행을 막는다. ResetBattle은 StopAllCoroutines를 먼저 호출하므로 이전 전투의 남은 진행이 새 전투에 끼어들지 않는다.

동률 예제는 EqualSpeedExample을 바꾼 뒤 전투를 처음부터 만든다. ResetBattle만 누르면 현재 예제 모드는 유지된다. 같은 randomSeed로 다시 만들기 때문에 해당 모드의 추첨을 재현할 수 있다.

## 변수·함수 상세

소스 선언 순서로 정리했다. 원본 링크에서 실제 코드 위치를 바로 열 수 있다. 함수 내부 변수는 해당 함수 아래에 묶었다.

## TurnSandboxController

두 화면에서 공유하는 개발용 턴 진행 컨트롤러. 임시 편성을 만들고 UI 입력을 Core에 전달하며 코루틴으로 턴 넘기기를 표시한다. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L11)

- **TurnSandboxController.stepDelay**: 자동 턴 넘기기에서 한 단계 처리 후 기다리는 실제 시간(초). 전투 규칙의 시간 비용은 아니다. [원본 16행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L16)

- **TurnSandboxController.randomSeed**: 예제의 동률 추첨을 재현하기 위한 난수 시드. 전투 초기화 시 같은 시드로 다시 시작한다. [원본 20행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L20)

- **TurnSandboxController.Battle**: 현재 실행 중인 전투 로직. 처음부터 버튼을 누르면 새 인스턴스로 교체한다. [원본 24행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L24)

- **TurnSandboxController.IsAdvancing**: 자동 턴 넘기기 코루틴의 실행 여부. 중복 진행과 예약 변경을 차단하는 데 사용한다. [원본 28행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L28)

- **TurnSandboxController.EqualSpeedExample**: 모든 임시 유닛의 Speed를 100으로 맞추는 동률 검증 모드 여부. [원본 32행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L32)

- **TurnSandboxController.StepDelay**: 단계 간 연출 대기 시간의 외부 접근 창구. 음수는 0으로 보정한다. [원본 36행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L36)

- **TurnSandboxController.View**: 현재 씬이 사용하는 화면 구현. Stage_Battle과 독립 검증 씬을 같은 조작 코드로 갱신한다. [원본 40행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L40)

### TurnSandboxController.Awake

연결된 화면을 찾거나 기본 검증 화면을 만들고, 화면 구성 후 첫 전투를 초기화한다.

선언: `private void Awake()`. [원본 45행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L45)

GetComponent로 같은 GameObject의 TurnBattleView 파생 컴포넌트를 찾는다. 없으면 TurnSandboxView를 추가한다. StageBattleView가 이미 있으면 검증 화면을 덧그리지 않는다.

별도의 입력 매개변수는 없다.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnSandboxController.ResetBattle

진행 중인 코루틴을 중단하고 임시 편성·COST·턴 번호를 처음 상태로 되돌린 뒤 1턴 예약 화면을 연다.

선언: `public void ResetBattle()`. [원본 56행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L56)

전투 객체와 상태를 새로 만든다. while문은 Planning까지 초기 세 단계를 즉시 진행한다. 지금은 유효한 고정 편성이므로 도달한다. 외부 데이터로 확장하면 초기 종료나 실패를 고려한 보호가 필요하다.

별도의 입력 매개변수는 없다.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnSandboxController.Definition

검증용 유닛 정의를 만든다. HP·공격·방어와 기본 편성은 임시 값이며 확정된 콘텐츠 데이터가 아니다.

선언: `private UnitDefinition Definition(string id, string name, int speed, int subActions = 1)`. [원본 76행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L76)

반환: 검증용 UnitDefinition. EqualSpeedExample이 true이면 전달받은 speed 대신 100을 사용한다.

**입력값**

- `string id`: 전투에서 유닛을 식별할 고유 ID.

- `string name`: UI와 진행 기록에 표시할 유닛 이름.

- `int speed`: 0 이상인 기본 Speed.

- `int subActions = 1`: 임시 유닛의 기본 서브 행동 횟수.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnSandboxController.ToggleExample

기본 속도 예제와 전원 동률 예제를 전환하고 전투를 처음부터 다시 시작한다.

선언: `public void ToggleExample()`. [원본 85행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L85)

EqualSpeedExample 반전 후 ResetBattle. 진행 중에도 초기화가 코루틴을 취소하므로 새 예제로 돌아간다.

별도의 입력 매개변수는 없다.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnSandboxController.ToggleEndTurnReservation

예약 단계에서 아군 필드 유닛의 턴 종료 예약을 토글한다. 자동 진행 중에는 입력을 무시한다.

선언: `public void ToggleEndTurnReservation(string id)`. [원본 95행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L95)

자동 진행 중이거나 Planning이 아니면 무시한다. 플레이어 소속 Field만 조작한다. 알 수 없는 ID는 Battle.Unit의 예외로 드러난다. 변경 뒤 화면을 다시 그린다.

**입력값**

- `string id`: 전투에서 유닛을 식별할 고유 ID.

**함수 내부의 변수·반복/람다 이름**

- `unit` (99행): 종료 예약을 토글할 유닛. 알 수 없는 ID는 Battle.Unit에서 거부한다.

### TurnSandboxController.Step

자동 진행 중이 아니면 현재 단계 또는 유닛 차례를 한 번만 진행한다.

선언: `public void Step()`. [원본 109행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L109)

자동 진행 중 또는 Ended이면 아무 것도 하지 않는다. 나머지는 AdvanceOne을 딱 한 번 호출한다.

별도의 입력 매개변수는 없다.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnSandboxController.SkipTurn

중복 실행을 막은 뒤 다음 턴의 예약 단계까지 자동으로 진행한다. 전투가 종료되면 진행하지 않는다.

선언: `public void SkipTurn()`. [원본 118행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L118)

입력 잠금 → 화면 갱신 → 코루틴 시작 순서다. 반환 즉시 전체 턴이 끝나는 동기 함수가 아니다.

별도의 입력 매개변수는 없다.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnSandboxController.AdvanceToNextPlanning

단계별 대기 시간을 두며 다음 턴 예약까지 진행하는 코루틴. 진행 정체에 대비해 반복 횟수를 제한한다.

선언: `private IEnumerator AdvanceToNextPlanning()`. [원본 129행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L129)

IEnumerator를 반환해 프레임에 걸쳐 실행된다. 각 진행 뒤 실제 시간으로 대기한다. 종료 또는 반복 한도에 도달하면 IsAdvancing을 해제하고 화면을 갱신한다. 한도 초과 시 로그 오류를 남기며 성공한 턴 전환으로 간주하지 않는다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `sourceTurn` (132행): 자동 진행 시작 당시 전체 턴 번호. 다음 턴 예약 단계에 도달했는지 판정하는 기준이다.

- `remainingSteps` (135행): 자동 진행의 무한 반복을 막는 횟수 예산. 전투 규칙의 행동 횟수와 무관하다.

### TurnSandboxController.AdvanceOne

예약 단계에서는 전원 턴 종료를 예약·확정하고, 나머지 단계에서는 Core를 한 번 진행한 후 화면을 갱신한다. 현재 턴 종료만 지원하는 개발용 동작이다.

선언: `private void AdvanceOne()`. [원본 149행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L149)

Planning에서는 모든 Field 유닛에 ReserveEndTurn 후 ConfirmReservations. 그 외에는 Advance. 결과가 Planning이면 적의 종료 예약을 채운 뒤 Refresh한다. 향후 스킬 예약을 넣을 때 전원 종료 예약을 강제로 추가하는 이 부분을 재설계해야 한다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `unit` (155행): 이번 반복의 초기화·예약·퇴각 처리할 유닛다. 순회 대상: `Battle.Units.Where(u => u.Location == UnitLocation.Field)`.

- `u` (155행): 함수식이 검사/변환 중인 유닛 한 개다. 적용하는 판정/변환은 `u.Location == UnitLocation.Field`다.

### TurnSandboxController.PlanSandboxEnemy

적 필드 유닛에 턴 종료를 자동 예약한다. 스킬을 선택하는 적 AI는 아직 구현하지 않았다.

선언: `private void PlanSandboxEnemy()`. [원본 167행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L167)

적 Field 유닛 모두 종료 예약. 스킬을 고르는 적 AI가 아니라 턴 흐름을 확인하기 위한 임시 행동이다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `unit` (169행): 이번 반복의 초기화·예약·퇴각 처리할 유닛다. 순회 대상: `Battle.Units.Where(u => u.FactionId != Battle.PlayerFactionId && u.Location == UnitLocation.Field)`.

- `u` (169행): 함수식이 검사/변환 중인 유닛 한 개다. 적용하는 판정/변환은 `u.FactionId != Battle.PlayerFactionId && u.Location == UnitLocation.Field`다.

### TurnSandboxController.OnDisable

컴포넌트가 비활성화되면 자동 진행을 중단하고 중복 입력 차단 상태를 해제한다.

선언: `private void OnDisable()`. [원본 176행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L176)

이 컴포넌트의 코루틴을 멈추고 IsAdvancing을 false로 만든다. 전투 객체 자체는 새로 만들지 않는다.

별도의 입력 매개변수는 없다.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnSandboxController.OnEnable

이미 전투가 생성되어 있으면 재활성화 시 현재 상태로 화면을 다시 그린다.

선언: `private void OnEnable()`. [원본 184행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxController.cs#L184)

Battle과 View가 이미 있을 때 현재 상태를 다시 표시한다. 첫 활성화에서 Awake가 만든 상태와 재활성화 상황을 모두 고려한다.

별도의 입력 매개변수는 없다.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.
