# 01. 전투 데이터를 읽는 법 — BattleRoster

기준: 2026-10-01 · 커밋 f69ceba · 검토 대기

[소스 파일 열기](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs)

전투에 들어갈 유닛의 정의와, 전투 중 바뀌는 상태를 분리하는 파일이다. 화면을 만들거나 턴을 진행하지 않는다. 생성자가 잘못된 입력을 먼저 거르기 때문에 이후 턴 로직은 유효한 편성이 들어왔다고 가정할 수 있다.

## 데이터가 만들어지는 순서

1. CombatStats에 기본 HP·두 공격력·두 방어력·Speed를 넣는다.
2. UnitDefinition이 그 능력치에 고유 ID, 표시 이름, 기본 서브 횟수를 붙인다.
3. BattleRoster가 세력별 유닛 목록과 출전 구역을 복사해 보관한다.
4. TurnBattle 생성자가 두 BattleRoster를 받아 앞의 두 명은 필드, 나머지는 대기로 BattleUnit을 만든다.
5. 이후 Slot·Location·남은 행동 횟수는 BattleUnit에서 바뀌고 UnitDefinition은 그대로 남는다.

## A를 예로 읽기

아군 A의 ID는 "A", 화면 이름은 "아군 A", Speed는 150이다. Definition은 이 기본 정보를 가리킨다. 전투가 시작되면 FactionId 1, Slot 1, Location Field인 BattleUnit이 생긴다. 초기화 단계 뒤에는 RemainingMainActions가 1이다. A의 차례를 끝내면 남은 행동 횟수는 0이 되지만 기본 Speed 150은 변하지 않는다.

## 읽을 때 구분할 것

- Hp는 현재 체력이 아니라 정의에 들어 있는 기본값이다. 이 클래스에 피해 누적이나 회복 함수는 없다.
- Id는 전투 전체에서 유일해야 한다. Name은 표시용이다. ID 중복의 최종 검사는 TurnBattle이 양측을 합친 뒤 한다.
- Slot의 null은 숫자 0번 칸이 아니다. 필드에 놓이지 않았다는 뜻이다.
- Reserve와 Retreated는 다르다. 대기는 편성에 남아 있는 상태, 퇴각은 전투에서 제외된 상태다.
- BattleRoster는 배열을 복사하고 읽기 전용으로 감싼다. 원래 입력 배열의 순서를 바꿔도 편성은 바뀌지 않는다.
- BattleUnit의 internal set은 같은 Core 어셈블리에서 상태를 바꿀 수 있다는 뜻이다. Unity 화면은 해당 값을 읽는 역할이다.
- ITurnRandom은 난수 공급 방법을 바꿀 수 있는 약속이다. 실제 예제는 SeededTurnRandom, 동률 테스트는 ScriptedRandom을 사용한다.
- 기본값 baseSubActions = 1과 seed는 다른 개념이다. 전자는 행동 횟수, 후자는 난수 재현용 시작값이다.

## 변수·함수 상세

소스 선언 순서로 정리했다. 원본 링크에서 실제 코드 위치를 바로 열 수 있다. 함수 내부 변수는 해당 함수 아래에 묶었다.

## UnitLocation

전투 중 유닛의 소속 영역. 필드·대기·퇴각을 구분하며 대기 복귀와 퇴각은 다른 상태다. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L10)

- **UnitLocation.Field**: 현재 9칸 전투 필드에 배치된 상태. [원본 15행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L15)

- **UnitLocation.Reserve**: 편성에는 포함되지만 필드에 출전하지 않은 대기 상태. [원본 19행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L19)

- **UnitLocation.Retreated**: 전투에서 퇴각한 상태. 현재 전투의 행동 순서와 필드 인원에서 제외한다. [원본 23행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L23)

## CombatStats

유닛 정의에 보관하는 변경 불가능한 기본 능력치. 현재 HP나 버프에 의한 능력치 변화는 아직 별도로 구현하지 않았다. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L29)

- **CombatStats.Hp**: 유닛 정의의 기본 HP. 생성 시 양수여야 하며 현재 체력이나 피해 누적값은 아니다. [원본 34행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L34)

- **CombatStats.TAtk**: 태크 계열 공격력. 현재 턴 검증에서는 보관만 하고 피해 계산에는 연결하지 않는다. [원본 38행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L38)

- **CombatStats.MAtk**: 신비 계열 공격력. 현재 턴 검증에서는 보관만 하고 피해 계산에는 연결하지 않는다. [원본 42행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L42)

- **CombatStats.TDef**: 태크 계열 방어력. 0 이상을 허용한다. [원본 46행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L46)

- **CombatStats.MDef**: 신비 계열 방어력. 0 이상을 허용한다. [원본 50행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L50)

- **CombatStats.Speed**: 공통 행동 순서를 정하는 속도. 높은 값부터 차례를 배정하며 0도 허용한다. [원본 54행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L54)

### CombatStats.CombatStats

HP는 양수, 나머지 능력치는 0 이상인지 검사한 뒤 기본 능력치를 저장한다.

선언: `public CombatStats(int hp, int tAtk, int mAtk, int tDef, int mDef, int speed)`. [원본 65행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L65)

반환: 생성된 기본 능력치 객체. HP가 0 이하이거나 나머지 값이 음수면 생성하지 않고 예외를 던진다. 현재 예외의 매개변수명은 어느 능력치가 잘못되어도 hp로 지정되어 있다.

**입력값**

- `int hp`: 양수인 기본 HP.

- `int tAtk`: 0 이상인 태크 공격력.

- `int mAtk`: 0 이상인 신비 공격력.

- `int tDef`: 0 이상인 태크 방어력.

- `int mDef`: 0 이상인 신비 방어력.

- `int speed`: 0 이상인 기본 Speed.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

## UnitDefinition

한 유닛의 식별자·표시 이름·기본 능력치·기본 서브 횟수를 묶은 정의 데이터. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L76)

- **UnitDefinition.Id**: 전투 전체에서 유닛을 식별하는 고유 문자열. 화면 이름과 독립적으로 사용한다. [원본 81행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L81)

- **UnitDefinition.Name**: UI와 진행 기록에 표시할 유닛 이름. [원본 85행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L85)

- **UnitDefinition.Stats**: 턴 순서와 향후 전투 계산에 사용할 기본 능력치. [원본 89행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L89)

- **UnitDefinition.BaseSubActions**: 턴 초기화 때 필드 유닛에 지급할 기본 서브 행동 횟수. [원본 93행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L93)

### UnitDefinition.UnitDefinition

식별자·이름·능력치와 서브 행동 횟수를 검증해 유닛 정의를 만든다.

선언: `public UnitDefinition(string id, string name, CombatStats stats, int baseSubActions = 1)`. [원본 102행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L102)

반환: 유닛 정의 객체. 빈 ID·이름, 음수 서브 횟수, null 능력치를 거부한다. 여기서는 다른 유닛과의 ID 중복까지 알 수 없으므로 검사하지 않는다.

**입력값**

- `string id`: 전투에서 유닛을 식별할 고유 ID.

- `string name`: UI와 진행 기록에 표시할 유닛 이름.

- `CombatStats stats`: 유닛의 기본 능력치 정의.

- `int baseSubActions = 1`: 턴마다 초기화할 기본 서브 행동 횟수.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

## BattleRoster

한 세력의 편성 순서와 스테이지 출전 구역. 현재 구현은 편성 2~6명과 출전 슬롯 2칸 이상을 요구한다. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L115)

- **BattleRoster.FactionId**: 유닛 또는 편성이 속한 세력의 식별 번호. 전투의 COST도 이 번호로 구분한다. [원본 120행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L120)

- **BattleRoster.Units**: 편성 순서대로 보관한 유닛 정의. 시작 시 앞의 두 명이 출전한다. [원본 124행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L124)

- **BattleRoster.DeploymentSlots**: 1~9 범위의 중복 없는 출전 슬롯 번호. 처음 두 칸은 시작 유닛 두 명의 배치 위치다. [원본 128행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L128)

### BattleRoster.BattleRoster

입력 편성과 출전 구역을 복사·검증해 외부 목록 변경이 전투 구성에 영향을 주지 않게 한다.

선언: `public BattleRoster(int factionId, IEnumerable<UnitDefinition> units, IEnumerable<int> deploymentSlots)`. [원본 136행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L136)

반환: 세력 편성 객체. 2~6명, null 유닛 없음, 출전 슬롯 2개 이상, 1~9 범위, 슬롯 중복 없음이 조건이다. 두 세력 간 구역 충돌은 TurnBattle에서 검사한다. 필드 최대 3명이라는 향후 배치 규칙을 이 생성자가 완전히 구현한 것은 아니다.

**입력값**

- `int factionId`: 해당 편성 또는 유닛이 속할 세력 ID.

- `IEnumerable<UnitDefinition> units`: 편성 순서대로 제공하는 유닛 정의 목록.

- `IEnumerable<int> deploymentSlots`: 1~9 사이의 중복 없는 출전 구역. 앞 두 칸에 시작 유닛을 놓는다.

**함수 내부의 변수·반복/람다 이름**

- `party` (139행): 입력 편성을 복사한 배열. 외부 목록 수정의 영향을 막고 시작 배치 순서를 유지한다.

- `slots` (141행): 출전 구역 입력을 복사한 배열. 범위와 중복을 검사한 뒤 편성에 보관한다.

- `u` (143행): 함수식이 검사/변환 중인 유닛 한 개다. 적용하는 판정/변환은 `u == null`다.

- `s` (145행): 함수식이 검사/변환 중인 항목 한 개다. 적용하는 판정/변환은 `s < 1 || s > 9`다.

## BattleUnit

전투 한 판에서 변하는 유닛의 배치 상태와 남은 행동 횟수. 정의 데이터와 분리한다. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L156)

- **BattleUnit.Definition**: 이 전투 유닛의 이름·기본 능력치·서브 횟수를 제공하는 정의. [원본 161행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L161)

- **BattleUnit.Id**: 전투 전체에서 유닛을 식별하는 고유 문자열. 화면 이름과 독립적으로 사용한다. [원본 165행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L165)

- **BattleUnit.FactionId**: 유닛 또는 편성이 속한 세력의 식별 번호. 전투의 COST도 이 번호로 구분한다. [원본 169행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L169)

- **BattleUnit.Slot**: 현재 필드 슬롯 번호(1~9). 대기 또는 퇴각 상태에서는 null이다. [원본 173행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L173)

- **BattleUnit.Location**: 유닛이 현재 필드·대기·퇴각 중 어디에 속하는지 나타내는 상태. [원본 177행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L177)

- **BattleUnit.RemainingMainActions**: 현재 턴에 남은 메인 행동 횟수. 턴 초기화 시 필드 유닛에 1회를 지급하고 차례 종료 시 남은 횟수를 버린다. [원본 181행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L181)

- **BattleUnit.RemainingSubActions**: 현재 턴에 남은 서브 행동 횟수. 기본 횟수로 초기화하며 사용하지 않은 횟수는 이월하지 않는다. [원본 185행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L185)

### BattleUnit.BattleUnit

유닛 정의와 세력을 연결하고 슬롯 유무에 따라 필드 또는 대기 상태로 시작한다.

선언: `internal BattleUnit(UnitDefinition definition, int factionId, int? slot)`. [원본 193행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L193)

반환: 전투 중 유닛 상태. slot이 있으면 Field, 없으면 Reserve로 시작한다. 행동 횟수는 생성 직후 기본값 0이며 이후 Initialization에서 지급한다.

**입력값**

- `UnitDefinition definition`: 전투 유닛의 원본 정의 데이터.

- `int factionId`: 해당 편성 또는 유닛이 속할 세력 ID.

- `int? slot`: 시작 필드 슬롯 번호. null이면 대기 상태로 시작한다.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

## ITurnRandom

동률 순서 추첨에 사용할 난수 공급 계약. 테스트에서는 정해진 값을 반환하는 구현으로 교체할 수 있다. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L203)

### ITurnRandom.Next

0 이상 exclusiveMaximum 미만의 정수를 반환한다.

선언: `int Next(int exclusiveMaximum)`. [원본 209행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L209)

함수의 형태만 정의한다. 구현체가 정수를 제공하고 TurnBattle.BuildOrder가 유효한 교환 인덱스인지 한 번 더 검사한다.

**입력값**

- `int exclusiveMaximum`: 난수 범위의 제외 상한. 반환값은 이 값보다 작다.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

## SeededTurnRandom

같은 시드에서 같은 난수열을 재현하는 전투용 난수 공급기. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L215)

- **SeededTurnRandom.random**: 시드로 초기화한 난수 생성기. 턴마다 새로 만들지 않고 이어서 추첨한다. [원본 220행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L220)

### SeededTurnRandom.SeededTurnRandom

재현 가능한 난수열을 주어진 시드로 초기화한다.

선언: `public SeededTurnRandom(int seed)`. [원본 225행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L225)

System.Random을 한 번 만든다. 턴마다 초기화하지 않고 다음 난수를 계속 꺼낸다. 동일 시드로 전투를 다시 만들면 같은 추첨 과정을 재현할 수 있다.

**입력값**

- `int seed`: 난수열을 재현할 초기 시드.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### SeededTurnRandom.Next

상한을 제외한 범위에서 다음 난수를 뽑는다.

선언: `public int Next(int exclusiveMaximum)`. [원본 230행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Core/BattleRoster.cs#L230)

반환 범위는 0 이상 exclusiveMaximum 미만이다. 예를 들어 Next(4)는 0·1·2·3 중 하나를 반환한다.

**입력값**

- `int exclusiveMaximum`: 난수 범위의 제외 상한. 반환값은 이 값보다 작다.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.
