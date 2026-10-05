# 06. 독립 검증 화면 — TurnSandboxView

기준: 2026-10-01 · 커밋 f69ceba · 검토 대기

[소스 파일 열기](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs)

기존 게임 리소스 없이 턴 로직만 확인하기 위한 uGUI 화면이다. StageBattleView와 동일한 Controller/Core를 사용하므로 규칙과 실제 씬 연결 문제를 나누어 확인할 수 있다.

## 화면이 만들어지는 순서

Build가 1280×800 기준 Overlay Canvas를 만들고 전체 배경, 5단계 표시, 턴 번호, 현재 차례, COST를 배치한다. 이어 9개 슬롯 버튼, 대기 유닛 안내, 순서, 진행 기록, 네 조작 버튼을 만든다. EventSystem이 하나도 없으면 StandaloneInputModule과 함께 추가한다.

slotNumber는 반복문의 i+1을 복사한 값이다. 클릭 콜백이 각자의 슬롯 번호를 기억하도록 한다. 해당 슬롯 유닛을 현재 전투에서 찾아 Controller의 예약 토글을 호출한다.

## 화면 갱신 순서

Refresh는 전투 객체를 받아 턴 번호와 단계명을 쓰고 현재 차례를 표시한다. 5단계 중 현재 단계만 강조한다. 아군 예약 가능 여부를 검사하고 빈 슬롯·아군·적군 문구를 다르게 쓴다.

대기 명단은 Location.Reserve에서 가져온다. 공개 순서는 Order, 최근 기록은 Events에서 읽되 상대 Reserved 기록을 숨기고 최근 6개를 보여 준다. 자동 진행 동안 Step/Skip와 예약 슬롯 입력을 막지만 Reset/Example은 그대로 사용할 수 있다.

## Stage 화면과의 차이

여기서는 슬롯을 화면 안의 카드로 만들고 실제 캐릭터 SpriteRenderer는 사용하지 않는다. StageBattleView의 월드 좌표 변환도 없다. 턴 규칙은 동일하다. Stage는 기록 2개, Sandbox는 6개를 표시한다.

색상 필드 background/panel/muted/ally/enemy/accent는 게임 상태가 아니라 UI 팔레트다. '필드 최대 3명' 문구가 있어도 이 화면에서 추가 배치 버튼을 제공하는 것은 아니다.

Box·Label·MakeButton·Place는 반복되는 UI 생성을 줄이는 보조 함수다. 숫자 x/y/w/h는 해당 Canvas 기준의 위치·크기이며 전투 슬롯 번호나 사거리가 아니다.

## 변수·함수 상세

소스 선언 순서로 정리했다. 원본 링크에서 실제 코드 위치를 바로 열 수 있다. 함수 내부 변수는 해당 함수 아래에 묶었다.

## TurnSandboxView

기존 리소스 없이 턴 규칙을 확인할 수 있도록 독립적인 uGUI 화면을 생성하는 검증용 화면. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L14)

- **TurnSandboxView.background**: 검증 화면 전체의 배경색. [원본 19행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L19)

- **TurnSandboxView.panel**: 카드·정보 영역·보조 버튼에 쓰는 기본 패널 색상. [원본 23행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L23)

- **TurnSandboxView.muted**: 보조 설명과 비어 있는 슬롯에 쓰는 약한 강조 색상. [원본 27행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L27)

- **TurnSandboxView.ally**: 아군 유닛 정보를 표시하는 색상. [원본 31행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L31)

- **TurnSandboxView.enemy**: 적군 유닛 정보를 표시하는 색상. [원본 35행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L35)

- **TurnSandboxView.accent**: 현재 단계와 턴 넘기기 버튼을 강조하는 색상. [원본 39행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L39)

- **TurnSandboxView.font**: 런타임에 생성하는 한국어 표시용 폰트. 화면 파괴 시 함께 해제한다. [원본 43행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L43)

- **TurnSandboxView.canvasRoot**: 독립 검증 화면의 Canvas 루트. 생성한 UI 요소의 부모로 사용한다. [원본 47행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L47)

- **TurnSandboxView.phaseText**: 현재 턴 단계의 설명 텍스트. [원본 57행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L57)

- **TurnSandboxView.actorText**: 현재 행동 중인 유닛 또는 대기 상태를 보여 주는 텍스트. [원본 57행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L57)

- **TurnSandboxView.costText**: 아군과 적군의 공유 COST를 보여 주는 텍스트. [원본 57행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L57)

- **TurnSandboxView.orderText**: 모든 세력이 공유하는 행동 순서와 차례 진행 상태를 표시하는 텍스트. [원본 57행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L57)

- **TurnSandboxView.historyText**: 최근 공개 가능한 진행 기록을 표시하는 텍스트. [원본 57행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L57)

- **TurnSandboxView.reserveText**: 필드에 나와 있지 않은 대기 유닛 안내 텍스트. [원본 57행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L57)

- **TurnSandboxView.modeText**: 임시 데이터·미구현 범위와 현재 속도 예제를 안내하는 텍스트. [원본 57행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L57)

- **TurnSandboxView.phases**: 상단에 표시하는 다섯 단계의 텍스트 목록. 현재 단계만 강조한다. [원본 61행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L61)

- **TurnSandboxView.slots**: 슬롯 번호 순서대로 보관하는 9개 클릭 버튼. 예약 가능한 아군만 활성화한다. [원본 65행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L65)

- **TurnSandboxView.slotLabels**: 각 슬롯의 번호·유닛·Speed·예약/실행 상태를 표시하는 텍스트 목록. [원본 69행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L69)

- **TurnSandboxView.VisibleCost**: 화면에 표시한 COST 문구. UI 연결 검증에서도 조회한다. [원본 73행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L73)

- **TurnSandboxView.VisibleActor**: 화면에 표시한 현재 행동 차례 문구. 실제 Core 상태와의 일치를 검증할 때 조회한다. [원본 77행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L77)

### TurnSandboxView.Build

Canvas·9칸 버튼·진행 정보·조작 버튼을 한 번 생성하고 필요한 EventSystem을 준비한다.

선언: `public override void Build(TurnSandboxController controller)`. [원본 83행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L83)

canvasRoot가 있으면 즉시 반환한다. 버튼의 action으로 Controller 메서드를 넘긴다. 새 전투마다 Canvas를 다시 만들지 않는다.

**입력값**

- `TurnSandboxController controller`: 전투 상태와 사용자 조작을 제공하는 공용 컨트롤러.

**함수 내부의 변수·반복/람다 이름**

- `root` (88행): 독립 검증 화면용 Canvas 게임 오브젝트.

- `scaler` (93행): 해상도에 따른 UI 크기를 조절하는 CanvasScaler.

- `backdrop` (100행): 전체 화면 배경. 입력은 아래의 실제 버튼으로 전달한다.

- `names` (105행): 턴 단계 번호에 대응하는 화면 표시 문구 배열.

- `i` (106행): 0부터 증가하는 반복 인덱스다. 이 함수의 배열/목록 순회 또는 테스트의 최대 대기 프레임 수를 센다. 각 for문마다 별개다.

- `box` (109행): 새로 생성한 UI 배경 Image.

- `turnPanel` (113행): 턴 번호와 현재 단계 설명을 담는 패널.

- `actorPanel` (117행): 현재 행동 차례 정보를 담는 패널.

- `costPanel` (121행): COST 설명과 수치를 묶는 정보 패널.

- `i` (126행): 0부터 증가하는 반복 인덱스다. 이 함수의 배열/목록 순회 또는 테스트의 최대 대기 프레임 수를 센다. 각 for문마다 별개다.

- `slotNumber` (129행): 배열 인덱스를 규칙의 1~9 슬롯 번호로 바꾼 값. 클릭 콜백에 고정해 전달한다.

- `button` (131행): 클릭 이벤트를 연결하거나 검증할 UI 버튼.

- `unit` (134행): 현재 처리하거나 표시할 유닛. 슬롯 조회 결과일 때는 빈 슬롯이면 null이다.

- `u` (134행): 함수식이 검사/변환 중인 유닛 한 개다. 적용하는 판정/변환은 `u.Slot == slotNumber`다.

- `sequence` (141행): 공통 행동 순서를 표시할 패널.

- `input` (155행): 검증 씬에 EventSystem이 없을 때 생성하는 UI 입력 오브젝트.

### TurnSandboxView.Refresh

턴 번호·단계·순서·슬롯·COST·최근 기록을 갱신한다. 상대 예약 기록은 걸러서 표시한다.

선언: `public override void Refresh(TurnSandboxController controller)`. [원본 164행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L164)

상태를 읽어서 기존 UI를 갱신한다. Ended일 때 현재 차례 문구를 '전투 종료'로 표시한다. COST 표시는 현재 예제의 세력 1/2를 직접 조회한다.

**입력값**

- `TurnSandboxController controller`: 전투 상태와 사용자 조작을 제공하는 공용 컨트롤러.

**함수 내부의 변수·반복/람다 이름**

- `battle` (167행): 진행하거나 표시·검증할 전투 상태 인스턴스.

- `i` (174행): 0부터 증가하는 반복 인덱스다. 이 함수의 배열/목록 순회 또는 테스트의 최대 대기 프레임 수를 센다. 각 for문마다 별개다.

- `i` (175행): 0부터 증가하는 반복 인덱스다. 이 함수의 배열/목록 순회 또는 테스트의 최대 대기 프레임 수를 센다. 각 for문마다 별개다.

- `unit` (178행): 현재 처리하거나 표시할 유닛. 슬롯 조회 결과일 때는 빈 슬롯이면 null이다.

- `u` (178행): 함수식이 검사/변환 중인 유닛 한 개다. 적용하는 판정/변환은 `u.Slot == i + 1`다.

- `friendly` (180행): 현재 슬롯 유닛이 플레이어 소속인지 여부. 색상과 예약 입력 허용에 사용한다.

- `active` (182행): 이 슬롯의 유닛이 현재 차례 소유자인지 나타내는 강조 표시 조건.

- `state` (190행): 유닛의 현재 차례·완료·예약 여부에 따라 선택한 슬롯 안내 문구.

- `u` (194행): 함수식이 검사/변환 중인 유닛 한 개다. 적용하는 판정/변환은 `u.Location == UnitLocation.Reserve`다.

- `u` (194행): 함수식이 검사/변환 중인 유닛 한 개다. 적용하는 판정/변환은 `u.Definition.Name`다.

- `id` (195행): 함수식이 검사/변환 중인 순서의 유닛 ID 한 개다. 적용하는 판정/변환은 `(id == battle.ActiveUnitId ? "▶ " : battle.HasCompleted(id) ? "✓ " : "") + battle.Unit(id).Definition.Name`다.

- `visible` (198행): 상대의 비공개 예약을 제외하고 화면에 보여 줄 진행 기록.

- `e` (198행): 함수식이 검사/변환 중인 진행 기록 한 개다. 적용하는 판정/변환은 `e.Kind != TurnEventKind.Reserved || battle.Unit(e.UnitId).FactionId == battle.PlayerFactionId`다.

- `e` (199행): 함수식이 검사/변환 중인 진행 기록 한 개다. 적용하는 판정/변환은 `"T" + e.Turn.ToString("00") + " " + e.Message`다.

### TurnSandboxView.PhaseName

턴 단계 enum을 검증 화면에서 읽을 수 있는 한국어 문구로 변환한다.

선언: `private static string PhaseName(TurnPhase phase)`. [원본 212행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L212)

반환: enum 숫자에 대응하는 한국어 단계명. 배열 0번은 비워 두어 TurnPhase의 1부터 시작하는 값과 맞춘다.

**입력값**

- `TurnPhase phase`: 전환하거나 표시할 턴 단계.

**함수 내부의 변수·반복/람다 이름**

- `names` (215행): 턴 단계 번호에 대응하는 화면 표시 문구 배열.

### TurnSandboxView.Box

지정한 부모와 영역에 단색 Image 패널을 생성해 반환한다.

선언: `private Image Box(string name, Transform parent, float x, float y, float w, float h, Color color)`. [원본 229행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L229)

반환: 단색 Image. Stage의 Panel과 달리 이 함수 안에서 raycastTarget을 일괄 끄지는 않는다. 전체 배경은 호출부에서 끄고 버튼 배경은 입력에 사용한다.

**입력값**

- `string name`: 생성할 UI 오브젝트의 이름.

- `Transform parent`: 생성하거나 조회할 오브젝트의 부모 Transform.

- `float x`: 부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.

- `float y`: 부모 위쪽에서 아래쪽으로 떨어진 UI 거리.

- `float w`: UI 영역의 가로 크기.

- `float h`: UI 영역의 세로 크기.

- `Color color`: 패널 또는 글자에 적용할 색상.

**함수 내부의 변수·반복/람다 이름**

- `go` (232행): UI 컴포넌트를 담기 위해 생성한 게임 오브젝트.

- `image` (235행): 생성한 UI 배경의 Image 컴포넌트.

### TurnSandboxView.Label

폰트·글자 크기·색을 적용한 텍스트를 생성한다. 텍스트 자체는 마우스 입력을 가로채지 않는다.

선언: `private Text Label(string name, Transform parent, string text, float x, float y, float w, float h, int size, Color color)`. [원본 250행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L250)

반환: Text. 가로는 줄바꿈, 세로는 영역 초과 시 자르기로 설정한다. 클릭 입력은 텍스트 대신 버튼이 받는다.

**입력값**

- `string name`: 생성할 UI 오브젝트의 이름.

- `Transform parent`: 생성하거나 조회할 오브젝트의 부모 Transform.

- `string text`: 표시할 본문 문자열.

- `float x`: 부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.

- `float y`: 부모 위쪽에서 아래쪽으로 떨어진 UI 거리.

- `float w`: UI 영역의 가로 크기.

- `float h`: UI 영역의 세로 크기.

- `int size`: 글자 크기.

- `Color color`: 패널 또는 글자에 적용할 색상.

**함수 내부의 변수·반복/람다 이름**

- `go` (253행): UI 컴포넌트를 담기 위해 생성한 게임 오브젝트.

- `label` (256행): 문구·폰트·정렬을 설정할 UI Text 컴포넌트.

### TurnSandboxView.MakeButton

검증 화면용 Image 버튼과 가운데 문구를 만들고 클릭 콜백을 연결한다.

선언: `private Button MakeButton(string name, string caption, float x, float y, float w, float h, Color color, Action action)`. [원본 274행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L274)

반환: Button. 배경 Image 생성 → Button 추가 → disabledColor 설정 → 중앙 텍스트 생성 → action 리스너 연결 순서다.

**입력값**

- `string name`: 생성할 UI 오브젝트의 이름.

- `string caption`: 버튼 또는 텍스트에 표시할 문구.

- `float x`: 부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.

- `float y`: 부모 위쪽에서 아래쪽으로 떨어진 UI 거리.

- `float w`: UI 영역의 가로 크기.

- `float h`: UI 영역의 세로 크기.

- `Color color`: 패널 또는 글자에 적용할 색상.

- `Action action`: 버튼 클릭 시 실행할 콜백.

**함수 내부의 변수·반복/람다 이름**

- `box` (277행): 새로 생성한 UI 배경 Image.

- `button` (279행): 클릭 이벤트를 연결하거나 검증할 UI 버튼.

- `colors` (281행): 버튼 상태별 색 설정의 복사본. 수정 후 버튼에 다시 대입한다.

- `label` (286행): 문구·폰트·정렬을 설정할 UI Text 컴포넌트.

### TurnSandboxView.Place

부모의 왼쪽 위를 원점으로 UI 위치와 크기를 지정한다. x는 오른쪽, y는 아래쪽 방향의 거리다.

선언: `private static void Place(GameObject go, Transform parent, float x, float y, float w, float h)`. [원본 301행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L301)

RectTransform을 부모 좌상단 기준으로 배치한다. y는 anchoredPosition에서 음수로 바꿔 아래로 내려가게 한다.

**입력값**

- `GameObject go`: 부모·위치·크기를 설정할 UI 오브젝트.

- `Transform parent`: 생성하거나 조회할 오브젝트의 부모 Transform.

- `float x`: 부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.

- `float y`: 부모 위쪽에서 아래쪽으로 떨어진 UI 거리.

- `float w`: UI 영역의 가로 크기.

- `float h`: UI 영역의 세로 크기.

**함수 내부의 변수·반복/람다 이름**

- `rect` (305행): UI 위치·크기 또는 버튼 중앙 좌표 계산에 사용하는 RectTransform.

### TurnSandboxView.OnDestroy

이 화면이 런타임에 생성한 폰트를 해제한다. 원본 에셋 폰트는 수정하지 않는다.

선언: `private void OnDestroy()`. [원본 313행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/TurnSandboxView.cs#L313)

동적으로 만든 font를 해제한다.

별도의 입력 매개변수는 없다.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.
