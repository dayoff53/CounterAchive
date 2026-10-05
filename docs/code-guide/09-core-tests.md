# 09. 규칙 검증을 읽는 법 — TurnBattleTests

기준: 2026-10-01 · 커밋 f69ceba · 검토 대기

[소스 파일 열기](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs)

화면 없이 Core만 검사하는 NUnit 테스트다. '준비 → 진행 → 기대 결과 비교' 순서로 읽으면 각 규칙을 어떤 사례로 보장하는지 이해할 수 있다.

## 테스트의 공통 준비

D는 간단한 유닛 정의를 만든다. Create는 양측 세 명씩 전투를 만든다. 실제 Controller 예제와 달리 대기 C/Z의 Speed를 999로 높여 두었다. 대기의 Speed가 아무리 높아도 순서에 끼어들면 안 된다는 검증 의도다.

Plan은 Planning까지, Confirm은 전원 종료 예약 후 확정까지, Finish는 현재 턴 끝까지 진행하는 도우미다. Plan/Finish에는 반복 제한과 Assert가 있어 정체를 테스트 실패로 알린다.

## 12개 결과와 함수 수의 차이

실제 테스트 함수는 11개다. CompletedEffectBatchEndsBattleEvenWithReservesAndStopsNextTurn에 TestCase 두 개가 있어 실행 결과는 12개다. private 도우미나 ScriptedRandom.Next는 별도 테스트 케이스로 집계되지 않는다.

## 동률 테스트가 우연에 기대지 않는 이유

ScriptedRandom은 3,2,1,0,0,0을 순서대로 돌려준다. 전원 Speed 0인 네 명을 두 턴 정렬할 때 요청 상한이 4,3,2,4,3,2인지 확인한다. 첫 턴은 자기 자리 교환을, 다음 턴은 다른 교환을 유도한다. '매 턴 새 추첨'을 검사하는 것이지 현실 난수에서 반드시 다른 순서가 나와야 한다는 규칙은 아니다.

## 실패가 의미하는 것

Assert.AreEqual은 기대값과 실제값을 비교한다. Assert.Throws는 잘못된 호출이 예외로 거부되는지 확인한다. CollectionAssert.AreEquivalent는 구성원 일치, AreEqual/SequenceEqual은 순서까지 비교한다.

기존 실행 기록에서 12/12가 통과했다. 문서 작성 때문에 테스트를 다시 실행한 것은 아니다. 테스트가 없는 스킬·피해·상태 규칙까지 검증되었다는 의미는 아니다.

## 변수·함수 상세

소스 선언 순서로 정리했다. 원본 링크에서 실제 코드 위치를 바로 열 수 있다. 함수 내부 변수는 해당 함수 아래에 묶었다.

## TurnBattleTests

Unity 연출 없이 턴 규칙의 초기화·정렬·예약·COST·퇴각 경계를 검증하는 EditMode 테스트. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L12)

### TurnBattleTests.D

테스트에서 사용할 유닛 정의를 간단히 생성한다. 속도와 서브 횟수 이외의 능력치는 고정한다.

선언: `private static UnitDefinition D(string id, int speed, int sub = 1)`. [원본 20행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L20)

반환: ID를 표시 이름으로도 쓰는 UnitDefinition. HP 100과 공격·방어 10을 고정하고 speed/sub만 다르게 만든다.

**입력값**

- `string id`: 전투에서 유닛을 식별할 고유 ID.

- `int speed`: 0 이상인 기본 Speed.

- `int sub = 1`: 테스트 유닛의 기본 서브 행동 횟수.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnBattleTests.Create

양측 필드 2명·대기 1명의 테스트 전투를 만든다. 대기 유닛의 높은 Speed가 순서에 섞이지 않는지도 확인할 수 있다.

선언: `private static TurnBattle Create(int speedA = 150, int speedB = 80, int speedX = 120, int speedY = 50, ITurnRandom random = null)`. [원본 31행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L31)

반환: TurnBattle. random을 넘기지 않으면 시드 53을 사용한다. Controller의 시드 530과 달라도 테스트 목적에는 문제가 없다.

**입력값**

- `int speedA = 150`: 아군 A의 테스트 Speed.

- `int speedB = 80`: 아군 B의 테스트 Speed.

- `int speedX = 120`: 적군 X의 테스트 Speed.

- `int speedY = 50`: 적군 Y의 테스트 Speed.

- `ITurnRandom random = null`: 동률 추첨에 사용할 난수 공급기.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnBattleTests.Plan

테스트 전투를 예약 단계까지 진행하고 제한 횟수 안에 도달했는지 확인한다.

선언: `private static void Plan(TurnBattle battle)`. [원본 39행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L39)

반환값 없음. 최대 반복 범위 안에서 예약 단계에 도달하지 못하면 마지막 Assert가 실패한다.

**입력값**

- `TurnBattle battle`: 테스트에서 진행할 전투 인스턴스.

**함수 내부의 변수·반복/람다 이름**

- `limit` (42행): 테스트 진행이 멈췄을 때 무한 반복하지 않도록 제한하는 남은 진행 횟수.

### TurnBattleTests.Confirm

순서에 포함된 필드 유닛 모두에게 턴 종료를 예약하고 확정한다.

선언: `private static void Confirm(TurnBattle battle)`. [원본 51행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L51)

Order의 현재 Field 유닛만 종료 예약 후 ConfirmReservations를 호출한다.

**입력값**

- `TurnBattle battle`: 테스트에서 진행할 전투 인스턴스.

**함수 내부의 변수·반복/람다 이름**

- `id` (53행): 이번 반복의 예약할 유닛 ID다. 순회 대상: `battle.Order`.

### TurnBattleTests.Finish

현재 전체 턴이 끝나거나 전투가 종료될 때까지 진행한다. 무한 반복을 막는 상한도 검증한다.

선언: `private static void Finish(TurnBattle battle)`. [원본 61행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L61)

시작 시점의 turn을 기억하고 번호가 바뀌거나 Ended가 될 때 멈춘다. 반복 제한에 닿으면 실패한다.

**입력값**

- `TurnBattle battle`: 테스트에서 진행할 전투 인스턴스.

**함수 내부의 변수·반복/람다 이름**

- `turn` (64행): 테스트 시작 당시 전체 턴 번호. 해당 턴의 종료 여부를 판단한다.

- `limit` (66행): 테스트 진행이 멈췄을 때 무한 반복하지 않도록 제한하는 남은 진행 횟수.

### TurnBattleTests.StartsWithFirstTwoInZonesAndReserveExcluded

편성 앞의 두 명이 지정 구역에 배치되고, 대기 유닛은 행동 순서에서 제외되는지 검증한다.

선언: `public void StartsWithFirstTwoInZonesAndReserveExcluded()`. [원본 74행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L74)

A1/B2/X9/Y8 배치, C 대기, Z 슬롯 null, 순서 A-X-B-Y를 확인한다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `battle` (78행): 진행하거나 표시·검증할 전투 상태 인스턴스.

### TurnBattleTests.FirstTurnSkipsOnlyCostGainAndRunsAllFivePhases

첫 턴에도 다섯 단계를 거치고 행동 횟수를 초기화하되 COST 획득만 생략하는지 검증한다.

선언: `public void FirstTurnSkipsOnlyCostGainAndRunsAllFivePhases()`. [원본 89행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L89)

각 단계 전환, 양측 COST 3 유지, A 메인 1/B 서브 2를 검사한다. 첫 턴 초기화 자체를 생략하지 않는다는 증거다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `battle` (93행): 진행하거나 표시·검증할 전투 상태 인스턴스.

### TurnBattleTests.OneChanceEachDespiteDifferentSpeedsAndTurnIncrementsOnlyAfterLastUnit

속도 차이와 무관하게 유닛별 한 차례만 부여하고 마지막 유닛 종료 후 전체 턴 번호가 증가하는지 검증한다.

선언: `public void OneChanceEachDespiteDifferentSpeedsAndTurnIncrementsOnlyAfterLastUnit()`. [원본 107행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L107)

각 유닛마다 시작/종료를 따로 호출한다. Y 종료 전에는 1턴, 뒤에는 2턴이며 Finished 기록 순서도 일치해야 한다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `battle` (111행): 진행하거나 표시·검증할 전투 상태 인스턴스.

- `expected` (112행): 이번 반복의 기대하는 순서 또는 COST다. 순회 대상: `new[] { "A", "X", "B", "Y" }`.

- `e` (119행): 함수식이 검사/변환 중인 진행 기록 한 개다. 적용하는 판정/변환은 `e.Kind == TurnEventKind.Finished`다.

- `e` (119행): 함수식이 검사/변환 중인 진행 기록 한 개다. 적용하는 판정/변환은 `e.UnitId`다.

### TurnBattleTests.CostUsesFieldCountAndCapsAtTenWithoutCarryOverActions

필드 인원에 따른 COST 획득·상한 10·예약과 행동 횟수 미이월을 여러 턴에 걸쳐 검증한다.

선언: `public void CostUsesFieldCountAndCapsAtTenWithoutCarryOverActions()`. [원본 126행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L126)

연속 다섯 턴에서 COST 5/7/9/10/10, 행동 횟수 재지급, 이전 예약 제거를 확인한다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `battle` (130행): 진행하거나 표시·검증할 전투 상태 인스턴스.

- `expected` (131행): 이번 반복의 기대하는 순서 또는 COST다. 순회 대상: `new[] { 5, 7, 9, 10, 10 }`.

### TurnBattleTests.PlanningCanBeEditedButCannotBeChangedAfterConfirmation

예약 수정 가능 시점과 예약 누락·대기 유닛·확정 후 변경 거부를 검증한다.

선언: `public void PlanningCanBeEditedButCannotBeChangedAfterConfirmation()`. [원본 144행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L144)

예약/취소 성공과 미예약 확정 거부, Planning의 Advance 거부, 대기 C 예약 거부, 확정 후 수정 거부를 검사한다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `battle` (148행): 진행하거나 표시·검증할 전투 상태 인스턴스.

## ScriptedRandom

동률 테스트가 우연한 결과에 의존하지 않도록 정해진 난수와 호출 이력을 제공하는 테스트 대역. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L163)

- **ScriptedRandom.values**: 첫 턴과 다음 턴의 순서를 다르게 만드는 미리 정한 난수열. [원본 168행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L168)

- **ScriptedRandom.Bounds**: 추첨 때 요청받은 제외 상한의 기록. 매 턴 추첨이 새로 이루어졌는지 검사한다. [원본 172행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L172)

### ScriptedRandom.Next

요청받은 상한을 기록하고 준비된 난수열에서 다음 값을 꺼낸다.

선언: `public int Next(int bound)`. [원본 177행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L177)

요청 상한을 Bounds에 추가한 뒤 values 큐의 맨 앞 값을 꺼낸다. 시험에 필요한 개수만 준비되어 있으므로 과도한 난수 호출도 실패로 드러난다.

**입력값**

- `int bound`: 테스트 대역에 요청된 난수 제외 상한.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### TurnBattleTests.TiesUseFreshFisherYatesDrawsEveryTurnIncludingZeroSpeed

Speed가 모두 0이어도 매 턴 동률 추첨이 새로 이루어지고 유닛 누락이 없는지 검증한다.

선언: `public void TiesUseFreshFisherYatesDrawsEveryTurnIncludingZeroSpeed()`. [원본 183행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L183)

전원 Speed 0으로 만들고 첫 순서를 복사한다. 다음 턴의 난수 호출 상한·같은 구성원·다른 순서를 검사한다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `random` (187행): 정해진 추첨 결과와 호출 상한을 기록하는 동률 테스트 대역.

- `battle` (189행): 진행하거나 표시·검증할 전투 상태 인스턴스.

- `first` (191행): 다음 턴의 재추첨 결과와 비교하기 위해 복사한 첫 턴 순서.

### TurnBattleTests.RetreatedUnitLosesPendingChanceAndDoesNotGenerateCost

퇴각한 유닛의 미실행 차례가 사라지고 다음 턴 COST 획득 인원에서도 빠지는지 검증한다.

선언: `public void RetreatedUnitLosesPendingChanceAndDoesNotGenerateCost()`. [원본 201행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L201)

X 퇴각 후 X 차례 시작 이벤트 없음, 다음 순서에서 X 없음, 적 COST는 3+1=4임을 확인한다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `battle` (205행): 진행하거나 표시·검증할 전투 상태 인스턴스.

- `e` (207행): 함수식이 검사/변환 중인 진행 기록 한 개다. 적용하는 판정/변환은 `e.Kind == TurnEventKind.Started && e.UnitId == "X"`다.

### TurnBattleTests.RetreatOfCurrentActorDoesNotRunItsEndTurnReservation

현재 차례 소유자가 퇴각하면 그 유닛의 종료 예약을 실행하지 않고 다음 유닛으로 넘어가는지 검증한다.

선언: `public void RetreatOfCurrentActorDoesNotRunItsEndTurnReservation()`. [원본 214행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L214)

A가 현재 차례일 때 퇴각시킨다. 다음 진행에서 X가 시작하고 A의 Finished 기록은 없어야 한다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `battle` (218행): 진행하거나 표시·검증할 전투 상태 인스턴스.

- `e` (222행): 함수식이 검사/변환 중인 진행 기록 한 개다. 적용하는 판정/변환은 `e.Kind == TurnEventKind.Finished && e.UnitId == "A"`다.

### TurnBattleTests.CompletedEffectBatchEndsBattleEvenWithReservesAndStopsNextTurn

대기 유닛이 남아도 필드 전멸이면 종료하고, 양측 동시 전멸 시 플레이어가 패배하는지 검증한다.

선언: `public void CompletedEffectBatchEndsBattleEvenWithReservesAndStopsNextTurn(bool both, BattleOutcome outcome)`. [원본 230행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L230)

both=false는 적 X/Y 퇴각으로 승리, true는 양측 필드 네 명 퇴각으로 패배다. Ended 이후 Advance=false, TurnNumber=1 유지도 검사한다.

**입력값**

- `bool both`: true이면 양측 모두, false이면 적군만 동시에 퇴각시킨다.

- `BattleOutcome outcome`: 해당 퇴각 조합에서 기대하는 전투 결과.

**함수 내부의 변수·반복/람다 이름**

- `battle` (235행): 진행하거나 표시·검증할 전투 상태 인스턴스.

### TurnBattleTests.InvalidRetreatBatchIsAtomic

퇴각 묶음에 알 수 없는 ID가 하나라도 있으면 앞의 유효 유닛도 퇴각 처리하지 않는지 검증한다.

선언: `public void InvalidRetreatBatchIsAtomic()`. [원본 244행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L244)

A와 unknown을 묶어서 넘긴다. 예외 뒤에도 A가 Field여야 하므로 잘못된 묶음이 일부만 적용되지 않음을 확인한다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `battle` (248행): 진행하거나 표시·검증할 전투 상태 인스턴스.

### TurnBattleTests.RejectsInvalidStatsRosterZonesAndDuplicateIds

음수 속도·편성 초과·범위 밖 슬롯·중복 ID·양측 출전 구역 충돌을 거부하는지 검증한다.

선언: `public void RejectsInvalidStatsRosterZonesAndDuplicateIds()`. [원본 256행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/EditMode/TurnBattleTests.cs#L256)

음수 Speed, 7명 편성, 0번 슬롯, 세력 간 같은 ID, 겹치는 출전 구역에 대한 예외를 각각 확인한다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `i` (260행): 함수식이 검사/변환 중인 테스트 유닛 번호 한 개다. 적용하는 판정/변환은 `D(i.ToString(), 10)`다.

- `first` (263행): 중복 ID와 출전 구역 충돌을 검사할 때 기준이 되는 플레이어 편성.
