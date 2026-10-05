# 02. 턴 규칙의 중심 — TurnBattle

기준: 2026-10-01 · 커밋 f69ceba · 검토 대기

[소스 파일 열기](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs)

현재 마키보 턴 기능의 중심이다. UnityEngine을 사용하지 않으며, 화면·애니메이션 없이도 전투 상태를 한 단계씩 진행할 수 있다. 현재 지원하는 예약 행동은 턴 종료 하나다.

## 다섯 단계와 함수의 관계

TurnStart → Initialization → Order → Planning → Execution 순서다. Ended는 승패 확정 후 정지 상태이며 여섯 번째 진행 단계가 아니다.

1. TurnStart에서 Advance를 호출하면 EvaluateVictory로 승패를 확인하고, 계속 중이면 Initialization으로 이동한다.
2. Initialization에서 Advance를 호출하면 행동 횟수를 초기화하고 2턴부터 COST를 얻는다. 그 뒤 Order로 이동한다.
3. Order에서 Advance를 호출하면 BuildOrder가 순서를 만들고 Planning으로 이동한다.
4. Planning에서는 Advance를 호출할 수 없다. ReserveEndTurn으로 필드 유닛의 예약을 채운 뒤 ConfirmReservations를 호출해야 한다.
5. Execution에서 Advance를 호출하면 AdvanceExecution이 현재 차례를 시작하거나 끝낸다. 마지막 유닛까지 끝나면 FinishRound가 다음 턴의 TurnStart로 돌아간다.

## 한 유닛에 두 번 진행하는 이유

실행 단계에서 ActiveUnitId가 null이면 다음 유닛의 차례를 시작한다. 다음 호출에서 ActiveUnitId가 있으면 그 차례를 끝낸다. 시작과 종료 사이에 화면으로 현재 유닛을 확인할 수 있다. 지금은 공격이 없으므로 이것이 곧 턴 종료 예약 실행 과정이다.

기본 예제의 실행 순서는 A 시작 → A 종료 → X 시작 → X 종료 → B 시작 → B 종료 → Y 시작 → Y 종료다. Y 종료에서 TurnNumber가 2가 되고 TurnStart로 이동한다. 유닛 한 명이 끝날 때마다 TurnNumber가 오르는 구조가 아니다.

## 내부 목록을 읽는 법

units는 전투 전체의 유닛 상태, order는 이번 턴의 행동 순서다. reserved는 종료 예약을 가진 ID 집합, completed는 차례를 마친 ID 집합이다. 같은 유닛은 여러 목록에 동시에 나타날 수 있지만 각 목록이 답하는 질문은 다르다.

Order·Units·Costs·Events는 읽기 전용 접근 창구다. 별도의 과거 스냅샷은 아니므로 다음 진행 후 조회하면 최신 내용이 보인다. 이전 순서를 비교하고 싶으면 테스트처럼 ToArray로 복사해야 한다.

## COST와 퇴각 예시

첫 턴 양측 COST는 3이다. 필드 두 명이 유지되면 다음 초기화에서 5, 이후 7·9·10이 된다. 남은 COST는 유지되지만 남은 메인·서브 횟수와 예약은 다음 턴으로 이월되지 않는다.

X가 퇴각하면 Location은 Retreated, Slot은 null, 남은 행동 횟수는 0이다. 이번 순서 목록에 X의 ID가 남아 있더라도 IsPending이 필드 여부를 확인해 차례를 건너뛴다. 다음 턴에는 순서를 새로 만들므로 X가 목록에서도 빠진다.

## 확장 전 알아둘 경계

ApplyCompletedRetreatBatch는 피해 계산 함수가 아니다. 호출자가 후속 효과까지 끝낸 뒤 최종 퇴각 ID 묶음을 넘겨야 한다. 먼저 모든 ID를 조회해 검사하고 나서 상태를 바꾸므로 ["A", "unknown"]처럼 잘못된 ID가 있으면 A도 퇴각시키지 않는다.

승패는 필드 유닛 유무만 본다. 대기 유닛이 남아도 필드 전멸이면 종료한다. 현재 2세력 PvE에서 양측 동시 전멸은 플레이어 패배다. UI에는 퇴각 입력이 연결되어 있지 않다. 스킬·피해·상태·턴 도중 Speed 변경·비용 소비가 구현되어 있다고 읽으면 안 된다.

## 변수·함수 상세

소스 선언 순서로 정리했다. 원본 링크에서 실제 코드 위치를 바로 열 수 있다. 함수 내부 변수는 해당 함수 아래에 묶었다.

## TurnPhase

한 턴의 5단계와 전투 종료 상태. 숫자는 화면의 단계 번호와 대응한다. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L11)

- **TurnPhase.TurnStart**: 1단계: 현재 필드를 기준으로 승패를 확인한다. [원본 16행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L16)

- **TurnPhase.Initialization**: 2단계: 행동 횟수와 COST를 갱신한다. 상태 효과·쿨다운은 아직 미연결이다. [원본 20행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L20)

- **TurnPhase.Order**: 3단계: 필드 유닛의 Speed와 동률 추첨으로 공통 순서를 정한다. [원본 24행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L24)

- **TurnPhase.Planning**: 4단계: 행동을 예약하고 확정한다. 현재는 턴 종료 예약만 지원한다. [원본 28행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L28)

- **TurnPhase.Execution**: 5단계: 확정된 순서대로 각 유닛의 차례를 진행한다. [원본 32행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L32)

- **TurnPhase.Ended**: 승패가 확정되어 더 이상 턴을 진행하지 않는 상태. 여섯 번째 턴 단계는 아니다. [원본 36행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L36)

## BattleOutcome

현재 2세력 PvE 전투의 진행 또는 최종 결과. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L41)

- **BattleOutcome.Ongoing**: 아직 전투 결과가 확정되지 않은 상태. [원본 46행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L46)

- **BattleOutcome.PlayerVictory**: 플레이어 필드 유닛이 남고 적 필드 유닛이 전멸한 결과. [원본 50행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L50)

- **BattleOutcome.PlayerDefeat**: 플레이어 필드 전멸로 인한 패배. PvE 동시 전멸도 포함한다. [원본 54행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L54)

## TurnEventKind

화면 진행 기록과 테스트에서 사건을 구분하는 종류. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L59)

- **TurnEventKind.Phase**: 전투 단계가 시작되거나 바뀐 기록. [원본 64행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L64)

- **TurnEventKind.Initialized**: 행동 횟수와 COST 초기화 처리 기록. [원본 68행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L68)

- **TurnEventKind.Ordered**: 이번 턴의 공통 행동 순서 확정 기록. [원본 72행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L72)

- **TurnEventKind.Reserved**: 개별 유닛의 턴 종료 예약 기록. 상대 예약은 UI에서 숨긴다. [원본 76행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L76)

- **TurnEventKind.Confirmed**: 전체 예약 확정 및 실행 단계 진입 기록. [원본 80행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L80)

- **TurnEventKind.Started**: 개별 유닛의 행동 차례 시작 기록. [원본 84행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L84)

- **TurnEventKind.Finished**: 개별 유닛의 차례 종료 기록. [원본 88행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L88)

- **TurnEventKind.RoundFinished**: 모든 필드 유닛의 차례를 마친 전체 턴 종료 기록. [원본 92행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L92)

- **TurnEventKind.Retreated**: 유닛 퇴각 결과를 반영한 기록. [원본 96행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L96)

- **TurnEventKind.BattleEnded**: 전투 승패 확정 기록. [원본 100행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L100)

## TurnEvent

턴 번호·사건 종류·관련 유닛·표시 문구를 보관하는 변경 불가능한 진행 기록. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L106)

- **TurnEvent.Turn**: 사건이 발생한 전체 턴 번호. 개별 유닛의 차례 번호와 다르다. [원본 111행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L111)

- **TurnEvent.Kind**: 이 기록이 나타내는 사건 종류. [원본 115행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L115)

- **TurnEvent.UnitId**: 사건과 관련된 유닛 ID. 단계 전환 등 특정 유닛이 없는 기록에서는 null이다. [원본 119행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L119)

- **TurnEvent.Message**: UI와 디버깅에 표시할 진행 설명 문구. [원본 123행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L123)

### TurnEvent.TurnEvent

발생 시점과 사건 정보를 하나의 진행 기록으로 묶는다.

선언: `internal TurnEvent(int turn, TurnEventKind kind, string message, string unitId)`. [원본 131행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L131)

반환: 변경 불가능한 진행 기록 객체. 전달받은 turn/kind/message/unitId를 그대로 저장한다. 보통 TurnBattle.Record에서 생성하며 특정 유닛이 없는 사건의 unitId는 null이다.

**입력값**

- `int turn`: 사건이 발생한 전체 턴 번호.

- `TurnEventKind kind`: 기록할 사건 종류.

- `string message`: 화면에 표시할 사건 설명.

- `string unitId`: 관련 유닛 ID. 특정 유닛과 무관한 사건이면 null.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

## TurnBattle

Unity 연출과 분리된 2세력 PvE 턴 로직. 턴 종료 예약만 지원하며 AP 누적·스킬 실행·상태 효과·턴 도중 Speed 변경은 구현하지 않았다. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L138)

- **TurnBattle.units**: 필드·대기·퇴각을 모두 포함한 이번 전투의 유닛 상태 목록. [원본 143행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L143)

- **TurnBattle.order**: 이번 턴의 공통 행동 순서를 나타내는 유닛 ID 목록. [원본 147행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L147)

- **TurnBattle.reserved**: 현재 턴 종료가 예약된 유닛 ID 집합. 중복 예약을 방지한다. [원본 151행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L151)

- **TurnBattle.completed**: 이번 턴에서 차례를 끝낸 유닛 ID 집합. 동일 유닛의 재행동을 막는다. [원본 155행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L155)

- **TurnBattle.costs**: 세력 ID별 공유 COST. 초기값 3, 상한 10으로 관리한다. [원본 159행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L159)

- **TurnBattle.events**: 최근 진행 기록 목록. 장시간 실행 시 무한히 쌓이지 않도록 최대 256개를 유지한다. [원본 163행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L163)

- **TurnBattle.random**: 동일 Speed 유닛의 순서를 추첨하는 외부 주입 난수 공급기. [원본 167행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L167)

- **TurnBattle.TurnNumber**: 전체 턴 번호. 모든 유효 차례를 처리한 뒤에만 1 증가한다. [원본 172행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L172)

- **TurnBattle.Phase**: 현재 진행 단계. 예약 단계에서는 확정 호출 없이 실행으로 넘어갈 수 없다. [원본 176행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L176)

- **TurnBattle.Outcome**: 현재 전투 결과. 승패 확정 전에는 Ongoing이다. [원본 180행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L180)

- **TurnBattle.PlayerFactionId**: 플레이어로 취급할 세력 ID. 승패와 아군 UI 판정의 기준이다. [원본 184행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L184)

- **TurnBattle.ActiveUnitId**: 현재 차례를 진행 중인 유닛 ID. 예약 중이나 차례 사이에는 null이다. [원본 188행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L188)

- **TurnBattle.Units**: 외부에서 목록 구성을 바꿀 수 없도록 공개하는 전체 유닛 상태 목록. [원본 192행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L192)

- **TurnBattle.Order**: UI와 테스트에 공개하는 이번 턴의 공통 행동 순서. [원본 196행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L196)

- **TurnBattle.Costs**: 외부에서 직접 차감·증가할 수 없도록 공개하는 세력별 COST 조회용 사전. [원본 200행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L200)

- **TurnBattle.Events**: 최근 진행 기록의 읽기 전용 목록. 상대 예약을 숨기는 처리는 화면에서 담당한다. [원본 204행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L204)

- **TurnBattle.CompletedCount**: 현재 턴에서 차례를 마친 유닛 수. 다음 턴 준비 시 0으로 초기화한다. [원본 208행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L208)

### TurnBattle.TurnBattle

두 세력의 중복 ID와 출전 구역 충돌을 검사하고, 각 편성의 앞 두 명을 출전시켜 1턴을 시작한다.

선언: `public TurnBattle(BattleRoster player, BattleRoster enemy, ITurnRandom random)`. [원본 216행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L216)

player/enemy의 null, 동일 세력 ID, 출전 구역 겹침, null 난수, 전투 전체 ID 중복을 검사한다. 각 세력 COST를 3으로 만들고 앞 두 명만 출전시킨다. 생성 직후 상태는 1턴 TurnStart이며 첫 단계 기록을 남긴다. Planning까지 진행하는 것은 Controller의 몫이다.

**입력값**

- `BattleRoster player`: 플레이어 세력의 편성과 출전 구역.

- `BattleRoster enemy`: 상대 세력의 편성과 출전 구역.

- `ITurnRandom random`: 동률 추첨에 사용할 난수 공급기.

**함수 내부의 변수·반복/람다 이름**

- `roster` (224행): 이번 반복의 초기 배치할 편성다. 순회 대상: `new[] { player, enemy }`.

- `i` (227행): 0부터 증가하는 반복 인덱스다. 이 함수의 배열/목록 순회 또는 테스트의 최대 대기 프레임 수를 센다. 각 for문마다 별개다.

- `u` (230행): 함수식이 검사/변환 중인 유닛 한 개다. 적용하는 판정/변환은 `u.Id`다.

### TurnBattle.Unit

고유 ID에 대응하는 전투 유닛을 찾는다. 존재하지 않으면 예외를 발생시킨다.

선언: `public BattleUnit Unit(string id)`. [원본 241행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L241)

반환: 해당 BattleUnit 참조. 없으면 ArgumentException. 복사본이 아니라 현재 전투 객체를 찾는다.

**입력값**

- `string id`: 전투에서 유닛을 식별할 고유 ID.

**함수 내부의 변수·반복/람다 이름**

- `u` (241행): 함수식이 검사/변환 중인 유닛 한 개다. 적용하는 판정/변환은 `u.Id == id`다.

### TurnBattle.HasEndTurnReservation

해당 유닛의 턴 종료 예약이 현재 저장되어 있는지 조회한다.

선언: `public bool HasEndTurnReservation(string id)`. [원본 247행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L247)

반환: reserved 집합에 ID가 있는지 여부. 이 조회 자체는 알려지지 않은 ID를 예외로 검사하지 않는다.

**입력값**

- `string id`: 전투에서 유닛을 식별할 고유 ID.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnBattle.HasCompleted

해당 유닛이 이번 턴에서 이미 차례를 마쳤는지 조회한다.

선언: `public bool HasCompleted(string id)`. [원본 252행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L252)

반환: completed 집합 포함 여부. 전체 턴 종료 뒤 집합이 비워지므로 과거 모든 턴의 완료 이력이 아니다.

**입력값**

- `string id`: 전투에서 유닛을 식별할 고유 ID.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnBattle.ReserveEndTurn

예약 단계에 이번 턴 필드 유닛의 턴 종료를 예약한다. 같은 예약을 중복 추가하지 않는다.

선언: `public void ReserveEndTurn(string id)`. [원본 258행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L258)

Planning에서만 동작한다. 이번 order에 있고 Field인 유닛만 허용한다. HashSet.Add가 처음 추가에 성공할 때만 Reserved 이벤트를 남긴다. 중복 호출은 예약이나 기록을 추가하지 않는다.

**입력값**

- `string id`: 전투에서 유닛을 식별할 고유 ID.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnBattle.ClearReservation

예약 단계에서 해당 유닛의 턴 종료 예약을 취소한다. 실행 단계에서는 변경을 허용하지 않는다.

선언: `public void ClearReservation(string id)`. [원본 270행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L270)

Planning에서 reserved에서 ID를 제거한다. 존재하지 않는 예약 제거는 아무 변화가 없고 별도 취소 이벤트도 남기지 않는다.

**입력값**

- `string id`: 전투에서 유닛을 식별할 고유 ID.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnBattle.ConfirmReservations

차례가 있는 모든 필드 유닛의 예약을 확인한 뒤 실행 단계로 전환한다. 미예약 유닛이 있으면 예외를 발생시킨다.

선언: `public void ConfirmReservations()`. [원본 279행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L279)

현재 필드 유닛 중 미예약자가 있으면 예외다. 모두 준비되면 Confirmed 기록을 남기고 Execution으로 전환한다. 이 호출만으로 첫 유닛의 차례가 시작되지는 않는다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `id` (282행): 함수식이 검사/변환 중인 순서의 유닛 ID 한 개다. 적용하는 판정/변환은 `Unit(id).Location == UnitLocation.Field && !reserved.Contains(id)`다.

### TurnBattle.Advance

초기 단계 하나 또는 유닛의 차례 시작/종료 한 번을 진행한다. 예약 단계는 별도 확정이 필요하며 전투 종료 상태에서는 false를 반환한다.

선언: `public bool Advance()`. [원본 291행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L291)

반환: Ended에 이미 들어와 있을 때 false, 그 외 처리한 호출은 true다. 이 호출 도중 승패가 확정되어도 반환값은 true일 수 있으므로 최종 상태는 Phase/Outcome으로 확인한다. Planning에서 무조건 진행하면 예외다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `unit` (300행): 이번 반복의 초기화·예약·퇴각 처리할 유닛다. 순회 대상: `units`.

- `field` (303행): 초기화 대상이 현재 필드 유닛인지 여부. 대기·퇴각 유닛에는 행동 횟수를 주지 않는다.

- `faction` (308행): 이번 반복의 COST를 갱신할 세력 ID다. 순회 대상: `costs.Keys.ToArray()`.

- `u` (309행): 함수식이 검사/변환 중인 유닛 한 개다. 적용하는 판정/변환은 `u.FactionId == faction && u.Location == UnitLocation.Field`다.

### TurnBattle.ApplyCompletedRetreatBatch

외부 효과 처리가 끝난 한 묶음의 퇴각 결과를 반영한 뒤 승패를 판정한다. 피해 계산 자체는 하지 않으며 모든 ID를 먼저 검증해 부분 적용을 막는다.

선언: `public void ApplyCompletedRetreatBatch(IEnumerable<string> unitIds)`. [원본 332행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L332)

Ended에서는 즉시 반환한다. Initialization/Execution 이외 단계에서는 예외다. 중복 ID는 제거한다. 전체 조회를 끝낸 다음 퇴각·예약 제거·현재 차례 해제를 반영하고 묶음 끝에 승패를 판정한다. 이미 퇴각한 ID의 중복 재호출을 별도로 거부하는 코드는 없다.

**입력값**

- `IEnumerable<string> unitIds`: 필수 후속 효과까지 끝난 한 묶음의 퇴각 유닛 ID 목록.

**함수 내부의 변수·반복/람다 이름**

- `retiring` (338행): 중복 ID를 제거하고 전체 유닛 조회를 끝낸 퇴각 대상 배열. 검증 완료 전에는 상태를 바꾸지 않는다.

- `unit` (339행): 이번 반복의 초기화·예약·퇴각 처리할 유닛다. 순회 대상: `retiring`.

### TurnBattle.BuildOrder

필드 유닛을 Speed 내림차순으로 정렬하고 동률 그룹을 매 턴 새로 섞는다. 이전 예약·완료 상태도 비운다.

선언: `private void BuildOrder()`. [원본 353행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L353)

Field만 골라 Speed별로 묶고 그룹을 내림차순 정렬한다. 같은 그룹 안에서 Fisher–Yates 교환을 수행한다. 이전 순서를 복사하지 않고 매 턴 난수를 새로 소비하지만, 우연히 같은 순서가 다시 나올 수는 있다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `group` (356행): 이번 반복의 같은 Speed의 유닛 그룹다. 순회 대상: `units.Where(u => u.Location == UnitLocation.Field).GroupBy(u => u.Definition.Stats.Speed).OrderByDescending(g => g.Key)`.

- `u` (356행): 함수식이 검사/변환 중인 유닛 한 개다. 적용하는 판정/변환은 `u.Location == UnitLocation.Field`다.

- `u` (356행): 함수식이 검사/변환 중인 유닛 한 개다. 적용하는 판정/변환은 `u.Definition.Stats.Speed`다.

- `g` (356행): 함수식이 검사/변환 중인 동률 그룹 한 개다. 적용하는 판정/변환은 `g.Key`다.

- `tied` (359행): Speed가 같은 유닛 그룹의 복사본. 이 목록 안에서만 균등하게 순서를 섞는다.

- `i` (361행): Fisher-Yates 방식으로 같은 Speed 그룹 안에서만 매 턴 균등 추첨한다.

- `j` (364행): 아직 섞지 않은 구간에서 뽑은 교환 대상 인덱스. 현재 위치 i도 포함한다.

- `swap` (367행): 동률 그룹의 두 원소를 교환할 때 잠시 보관하는 유닛.

- `u` (369행): 함수식이 검사/변환 중인 유닛 한 개다. 적용하는 판정/변환은 `u.Id`다.

- `id` (371행): 함수식이 검사/변환 중인 순서의 유닛 ID 한 개다. 적용하는 판정/변환은 `Unit(id).Definition.Name`다.

### TurnBattle.AdvanceExecution

현재 차례가 있으면 종료하고, 없으면 다음 유효 유닛의 차례를 시작한다. 남은 차례가 없으면 전체 턴을 끝낸다.

선언: `private void AdvanceExecution()`. [원본 377행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L377)

먼저 승패를 검사한다. 현재 차례가 있으면 completed에 추가하고 남은 행동을 0으로 만들며 Finished를 기록한다. 현재 차례가 없으면 미완료 Field 중 순서상 첫 유닛을 고른다. 그 유닛의 확정 예약이 없으면 예외다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `unit` (383행): 현재 차례를 종료할 유닛. ActiveUnitId로 조회하므로 없는 ID는 예외다.

### TurnBattle.IsPending

해당 유닛이 아직 차례를 끝내지 않았고 필드에 남아 있는지 판정한다.

선언: `private bool IsPending(string id)`. [원본 401행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L401)

반환: 아직 completed에 없고 Location이 Field인 경우 true. 이 조건으로 퇴각 유닛의 남은 차례가 사라진다.

**입력값**

- `string id`: 전투에서 유닛을 식별할 고유 ID.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnBattle.FinishRound

전체 턴 종료를 기록하고 턴 번호를 증가시킨 뒤 다음 턴 시작 단계로 돌아간다.

선언: `private void FinishRound()`. [원본 406행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L406)

전체 턴 완료를 기존 턴 번호로 기록하고 TurnNumber를 1 올린다. order/reserved/completed와 ActiveUnitId를 비운 뒤 TurnStart로 이동한다. COST 획득은 다음 Initialization에서 한다.

별도의 입력 매개변수는 없다.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnBattle.EvaluateVictory

양측 필드 생존 여부로 2세력 PvE 승패를 확정한다. 대기 유닛은 생존 판정에 포함하지 않으며 양측 전멸은 플레이어 패배다. 종료 확정 시 true를 반환한다.

선언: `private bool EvaluateVictory()`. [원본 417행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L417)

반환: 전투 종료를 확정했으면 true. 양측 필드가 모두 남으면 false. 종료 시 Outcome과 Phase를 바꾸고 ActiveUnitId와 예약을 비운다. 전체 유닛 목록·COST·완료 집합까지 모두 지우지는 않는다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `playerAlive` (420행): 플레이어 필드 유닛이 한 명 이상 남아 있는지 나타내는 승패 조건.

- `u` (420행): 함수식이 검사/변환 중인 유닛 한 개다. 적용하는 판정/변환은 `u.FactionId == PlayerFactionId && u.Location == UnitLocation.Field`다.

- `enemyAlive` (422행): 적 세력의 필드 유닛이 한 명 이상 남아 있는지 나타내는 승패 조건.

- `u` (422행): 함수식이 검사/변환 중인 유닛 한 개다. 적용하는 판정/변환은 `u.FactionId != PlayerFactionId && u.Location == UnitLocation.Field`다.

### TurnBattle.RequirePhase

현재 단계가 요구 단계와 다르면 예외를 발생시켜 잘못된 시점의 조작을 차단한다.

선언: `private void RequirePhase(TurnPhase expected)`. [원본 435행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L435)

현재 Phase가 expected와 다르면 InvalidOperationException을 던진다. 잘못된 순서의 외부 호출을 조용히 처리하지 않고 드러낸다.

**입력값**

- `TurnPhase expected`: 현재 조작을 허용하는 단계.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnBattle.ChangePhase

현재 단계를 바꾸고 해당 단계의 시작 문구를 진행 기록에 남긴다.

선언: `private void ChangePhase(TurnPhase phase)`. [원본 441행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L441)

Phase를 바꾸고 번호에 대응하는 한국어 단계명을 Phase 이벤트로 기록한다. 승패 종료는 EvaluateVictory에서 BattleEnded 이벤트로 직접 처리한다.

**입력값**

- `TurnPhase phase`: 전환하거나 표시할 턴 단계.

**함수 내부의 변수·반복/람다 이름**

- `names` (445행): 턴 단계 번호에 대응하는 화면 표시 문구 배열.

### TurnBattle.Record

현재 턴 번호로 사건을 기록한다. 256개를 넘기기 전 가장 오래된 기록을 제거한다.

선언: `private void Record(TurnEventKind kind, string message, string unitId = null)`. [원본 454행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/TurnBattle.cs#L454)

현재 TurnNumber를 포함한 TurnEvent를 추가한다. 이미 256개라면 가장 오래된 한 개를 먼저 지워 최근 256개만 보존한다.

**입력값**

- `TurnEventKind kind`: 기록할 사건 종류.

- `string message`: 화면에 표시할 사건 설명.

- `string unitId = null`: 관련 유닛 ID. 특정 유닛과 무관한 사건이면 null.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.
