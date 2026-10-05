# 05. 기존 InGame 리소스를 연결하는 법 — StageBattleView

기준: 2026-10-01 · 커밋 f69ceba · 검토 대기

[소스 파일 열기](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs)

Stage_Battle의 배경과 아홉 슬롯을 새 턴 로직에 연결한다. 기존 카브 스크립트는 호출하지 않는다. Play 중 씬 인스턴스의 UI를 재배치하고, 기존 캐릭터 렌더러에 임시 스프라이트를 지정한다.

## Build의 실제 작업 순서

1. hud가 이미 있으면 재구성을 생략한다.
2. Inspector의 inGame 또는 씬 루트 이름 InGame으로 리소스를 찾는다.
3. UnitSlots 자식을 x좌표 순으로 정렬한다. 정확히 9개인지 확인한다. 배열 인덱스 0은 규칙의 1번 슬롯이다.
4. MainCamera와 UICanvas, 한국어 폰트를 확보한다. Canvas 기준은 1280×720이며 화면 전체에 겹치는 Overlay 방식이다.
5. Makibo Battle HUD를 새로 만든다. 기존 TurnEndButton, ActionPointSkipNutton, COST 바를 찾는다.
6. 기존 Canvas 자식 UI와 ProdutionCanvas를 숨긴다. 재사용 버튼과 COST 배경은 새 HUD로 옮겨 다시 활성화한다.
7. 턴·단계·순서·현재 차례·진행 기록·COST 문구와 초기화/동률 버튼을 추가한다.
8. 스프라이트 세 개가 모두 연결되었는지 검사한다.
9. 각 슬롯의 바닥/캐릭터 렌더러를 보관하고 옛 Animator와 유닛 Canvas를 끈다. 클릭 영역과 설명판을 덧붙인다.

## 기존 이름과 새 기능의 대응

TurnEndButton은 턴 넘기기, ActionPointSkipNutton은 단계별 진행에 연결된다. 철자가 어색한 기존 이름도 리소스 경로이므로 임의로 바꾸지 않았다. Rebind가 이전 클릭 이벤트를 새 이벤트로 교체한다.

allyA는 아군 A, allyB는 나머지 임시 아군 B, enemy는 적 X/Y가 공유한다. 이 연결은 캐릭터 데이터 시스템이 아니다. 현재 A/B/X/Y 임시 편성에 맞춘 표시이며, PNG 프레임을 Inspector에서 직접 지정해 같은 이름의 Aseprite 에셋과 구분했다.

## Refresh와 LateUpdate의 차이

Refresh는 전투 상태가 바뀔 때 내용을 바꾼다. COST 3이면 게이지는 0.3, COST 5이면 0.5다. 현재 차례 슬롯은 초록색, 완료한 캐릭터는 회색으로 표시한다. 적 예약 이벤트는 진행 기록에서 제외하고 최근 공개 기록 2개만 보여 준다.

LateUpdate는 매 프레임 월드 슬롯 위에 클릭 영역을 맞춘다. 카메라로 슬롯 위치를 화면 좌표로 바꾼 뒤, HUD 안의 좌표로 다시 바꿔 버튼을 놓는다. 화면 비율이 달라져도 클릭 영역과 캐릭터 위치를 맞추기 위한 처리다. 전투 진행 함수가 아니다.

## 값과 리소스의 수명

font와 hud는 실행 중 생성된다. grounds/unitSprites는 기존 리소스의 컴포넌트를 참조한다. 스프라이트 프레임은 에셋 참조이며 OnDestroy가 지우는 대상은 동적 font뿐이다.

현재 코드는 필요한 이름·경로·컴포넌트가 있다고 가정한다. Required는 필수 경로 누락을 예외로 알리고, Single은 해당 객체가 하나여야 한다고 요구한다. UICanvas·CanvasScaler·EventSystem 등의 구조를 바꾸면 연결도 함께 검토해야 한다.

COST 표시의 세력 번호 1/2와 필드 2명+대기 1명 안내는 현재 예제에 맞춰 고정되어 있다. StageBattleView의 Ended 시 actorText는 일반적인 '다음 단계 대기'로 남을 수 있어, 실제 승패 연출은 후속 작업이다.

## 변수·함수 상세

소스 선언 순서로 정리했다. 원본 링크에서 실제 코드 위치를 바로 열 수 있다. 함수 내부 변수는 해당 함수 아래에 묶었다.

## StageBattleView

Stage_Battle의 InGame 시각 리소스를 새 턴 화면에 연결한다. 격리된 카브 스크립트를 호출하거나 프리팹 원본을 변경하지 않는다. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L14)

- **StageBattleView.inGame**: 배경·슬롯·기존 UI가 있는 씬의 InGame 루트. Inspector에서 미지정이면 같은 씬의 InGame 이름으로 찾는다. [원본 19행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L19)

- **StageBattleView.allyA**: 임시 아군 A에 사용할 정확한 PNG 스프라이트 프레임. 같은 이름의 Aseprite와 혼동하지 않도록 직접 연결한다. [원본 23행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L23)

- **StageBattleView.allyB**: 임시 아군 B에 사용할 PNG 스프라이트 프레임. [원본 27행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L27)

- **StageBattleView.enemy**: 임시 적군 X/Y에 함께 사용할 PNG 스프라이트 프레임. [원본 31행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L31)

- **StageBattleView.font**: 런타임에 생성하는 한국어 표시용 폰트. 화면 파괴 시 함께 해제한다. [원본 35행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L35)

- **StageBattleView.canvas**: 기존 InGame의 UICanvas. 새 HUD와 재사용 버튼을 표시한다. [원본 39행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L39)

- **StageBattleView.battleCamera**: 필드를 비추는 이 씬의 MainCamera. 슬롯 위치를 화면 좌표로 바꿀 때도 사용한다. [원본 43행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L43)

- **StageBattleView.hud**: 기존 Canvas 아래에 런타임으로 생성하는 새 턴 정보 및 조작 UI의 루트. [원본 47행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L47)

- **StageBattleView.slots**: 기존 UnitSlots 자식들을 왼쪽부터 정렬한 배열. 인덱스 0~8은 규칙의 슬롯 1~9에 대응한다. [원본 51행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L51)

- **StageBattleView.slotButtons**: 실제 슬롯 위에 겹쳐 놓는 클릭 영역. 아군 종료 예약과 취소 입력을 받는다. [원본 55행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L55)

- **StageBattleView.slotLabels**: 각 슬롯의 번호·유닛·Speed·예약/실행 상태를 표시하는 텍스트 목록. [원본 59행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L59)

- **StageBattleView.unitSprites**: 기존 슬롯에서 재사용하는 캐릭터 SpriteRenderer 목록. 슬롯 배열과 같은 순서다. [원본 63행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L63)

- **StageBattleView.grounds**: 기존 슬롯 바닥 SpriteRenderer 목록. 아군·적군·현재 차례 색상을 표시한다. [원본 67행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L67)

- **StageBattleView.phaseText**: 현재 턴 단계의 설명 텍스트. [원본 75행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L75)

- **StageBattleView.actorText**: 현재 행동 중인 유닛 또는 대기 상태를 보여 주는 텍스트. [원본 75행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L75)

- **StageBattleView.costText**: 아군과 적군의 공유 COST를 보여 주는 텍스트. [원본 75행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L75)

- **StageBattleView.orderText**: 모든 세력이 공유하는 행동 순서와 차례 진행 상태를 표시하는 텍스트. [원본 75행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L75)

- **StageBattleView.historyText**: 최근 공개 가능한 진행 기록을 표시하는 텍스트. [원본 75행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L75)

- **StageBattleView.costFill**: 기존 COST 바의 Image. 아군 COST/10을 가로 채움 비율로 표시한다. [원본 79행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L79)

- **StageBattleView.portraits**: 아군 A, 아군 B, 공용 적군 순서로 참조하는 임시 캐릭터 프레임 배열. [원본 83행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L83)

- **StageBattleView.VisibleCost**: 화면에 표시한 COST 문구. UI 연결 검증에서도 조회한다. [원본 87행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L87)

- **StageBattleView.VisibleActor**: 화면에 표시한 현재 행동 차례 문구. 실제 Core 상태와의 일치를 검증할 때 조회한다. [원본 91행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L91)

- **StageBattleView.Slots**: 검증과 외부 조회에 공개하는 왼쪽부터 정렬된 실제 슬롯 Transform 목록. [원본 95행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L95)

- **StageBattleView.SlotButtons**: UI 검증에서 실제 raycast와 클릭을 수행할 수 있도록 공개하는 슬롯 버튼 목록. [원본 99행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L99)

- **StageBattleView.CostFill**: COST 수치와 실제 게이지 채움량의 일치를 확인하기 위한 Image 참조. [원본 103행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L103)

### StageBattleView.Build

기존 슬롯·Canvas·턴 종료/SKIP 버튼·COST 바를 연결하고 부족한 HUD를 생성한다. 구형 메뉴는 현재 실행 인스턴스에서만 숨긴다.

선언: `public override void Build(TurnSandboxController owner)`. [원본 109행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L109)

반환값 없음. 필수 리소스를 찾고 HUD를 구성하는 최초 1회 작업이다. 슬롯 번호 자체가 아니라 실제 x 위치로 정렬한다. 슬롯 클릭 콜백은 Build 당시 유닛을 고정하지 않고 클릭 시점의 owner.Battle에서 조회한다.

**입력값**

- `TurnSandboxController owner`: 이 화면을 사용하는 공용 턴 컨트롤러.

**함수 내부의 변수·반복/람다 이름**

- `g` (113행): 함수식이 검사/변환 중인 씬 루트 GameObject 한 개다. 적용하는 판정/변환은 `g.name == "InGame"`다.

- `t` (114행): 함수식이 검사/변환 중인 슬롯 Transform 한 개다. 적용하는 판정/변환은 `t.localPosition.x`다.

- `g` (116행): 함수식이 검사/변환 중인 씬 루트 GameObject 한 개다. 적용하는 판정/변환은 `g.GetComponentsInChildren<Camera>()`다.

- `c` (116행): 함수식이 검사/변환 중인 카메라 한 개다. 적용하는 판정/변환은 `c.CompareTag("MainCamera")`다.

- `scaler` (120행): 해상도에 따른 UI 크기를 조절하는 CanvasScaler.

- `originalBattle` (129행): 기존 턴 종료/SKIP 버튼과 COST 바가 배치된 Battle_UI 루트.

- `costBack` (134행): 재사용 COST 바의 배경 Image. 새 HUD에 옮겨 표시한다.

- `child` (135행): 이번 반복의 숨김 여부를 정할 자식 Transform다. 순회 대상: `canvas.transform.Cast<Transform>().ToArray()`.

- `fillRect` (151행): COST 채움 Image의 RectTransform. 배경 영역 전체에 맞춘다.

- `sprite` (166행): 함수식이 검사/변환 중인 스프라이트 한 개다. 적용하는 판정/변환은 `sprite == null`다.

- `i` (167행): 0부터 증가하는 반복 인덱스다. 이 함수의 배열/목록 순회 또는 테스트의 최대 대기 프레임 수를 센다. 각 for문마다 별개다.

- `index` (170행): 클릭 콜백이 반복 종료 후에도 같은 슬롯을 가리키도록 복사한 배열 인덱스.

- `slot` (172행): 현재 반복에서 화면을 연결할 기존 슬롯 Transform.

- `model` (175행): 해당 슬롯에 원래 배치된 UnitBase 리소스 루트.

- `animator` (176행): 이번 반복의 비활성화할 Animator다. 순회 대상: `model.GetComponentsInChildren<Animator>(true)`.

- `c` (177행): 이번 반복의 비활성화할 유닛 Canvas다. 순회 대상: `model.GetComponentsInChildren<Canvas>(true)`.

- `sprite` (179행): 해당 슬롯에서 캐릭터 프레임과 방향·크기를 표시할 기존 SpriteRenderer.

- `r` (179행): 함수식이 검사/변환 중인 SpriteRenderer 한 개다. 적용하는 판정/변환은 `r.name == "UnitCharactorSprite"`다.

- `button` (182행): 클릭 이벤트를 연결하거나 검증할 UI 버튼.

- `unit` (185행): 현재 처리하거나 표시할 유닛. 슬롯 조회 결과일 때는 빈 슬롯이면 null이다.

- `u` (185행): 함수식이 검사/변환 중인 유닛 한 개다. 적용하는 판정/변환은 `u.Location == UnitLocation.Field && u.Slot == index + 1`다.

- `label` (191행): 문구·폰트·정렬을 설정할 UI Text 컴포넌트.

- `captionBack` (194행): 밝은 배경 위에서도 슬롯 설명을 읽을 수 있게 하는 어두운 패널.

### StageBattleView.Refresh

Core 상태에 맞춰 재사용 리소스의 색·캐릭터·COST와 새 정보 문구를 갱신한다. 현재는 임시 편성과 턴 종료만 표시한다.

선언: `public override void Refresh(TurnSandboxController owner)`. [원본 205행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L205)

반환값 없음. TurnNumber/Phase/ActiveUnitId/Costs/Order/Events 및 유닛 상태를 화면에 반영한다. reserved 비공개는 표시에서 걸러 주는 것이며 Core 데이터에 상대 예약이 없는 것은 아니다.

**입력값**

- `TurnSandboxController owner`: 이 화면을 사용하는 공용 턴 컨트롤러.

**함수 내부의 변수·반복/람다 이름**

- `b` (208행): 화면에 반영할 현재 전투 상태.

- `phaseNames` (211행): 현재 단계 번호로 조회할 한국어 단계 설명.

- `id` (216행): 함수식이 검사/변환 중인 순서의 유닛 ID 한 개다. 적용하는 판정/변환은 `(id == b.ActiveUnitId ? "▶ " : b.HasCompleted(id) ? "✓ " : "") + b.Unit(id).Definition.Name`다.

- `visible` (218행): 상대의 비공개 예약을 제외하고 화면에 보여 줄 진행 기록.

- `e` (218행): 함수식이 검사/변환 중인 진행 기록 한 개다. 적용하는 판정/변환은 `e.Kind != TurnEventKind.Reserved || b.Unit(e.UnitId).FactionId == b.PlayerFactionId`다.

- `e` (219행): 함수식이 검사/변환 중인 진행 기록 한 개다. 적용하는 판정/변환은 `"T" + e.Turn.ToString("00") + " " + e.Message`다.

- `i` (220행): 0부터 증가하는 반복 인덱스다. 이 함수의 배열/목록 순회 또는 테스트의 최대 대기 프레임 수를 센다. 각 for문마다 별개다.

- `u` (223행): 현재 슬롯에 배치된 필드 유닛. 빈 슬롯이면 null이다.

- `unit` (223행): 함수식이 검사/변환 중인 전투 유닛 한 개다. 적용하는 판정/변환은 `unit.Location == UnitLocation.Field && unit.Slot == i + 1`다.

- `friendly` (225행): 현재 슬롯 유닛이 플레이어 소속인지 여부. 색상과 예약 입력 허용에 사용한다.

- `active` (227행): 이 슬롯의 유닛이 현재 차례 소유자인지 나타내는 강조 표시 조건.

- `color` (229행): 빈 슬롯·아군·적군 상태에 따라 선택한 기본 표시 색상.

- `sprite` (235행): 해당 슬롯에서 캐릭터 프레임과 방향·크기를 표시할 기존 SpriteRenderer.

### StageBattleView.LateUpdate

화면 비율에 맞춰 카메라 범위를 확보하고 각 월드 슬롯 위에 클릭 영역과 설명을 정렬한다.

선언: `private void LateUpdate()`. [원본 257행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L257)

카메라 orthographicSize를 max(7.2, 12.8/aspect)로 맞추고 슬롯 x좌표와 기준 y=2.0을 화면에 투영한다. out local은 HUD 내부 좌표다. 버튼 중심 앵커에 해당 좌표를 지정한다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `i` (261행): 0부터 증가하는 반복 인덱스다. 이 함수의 배열/목록 순회 또는 테스트의 최대 대기 프레임 수를 센다. 각 for문마다 별개다.

- `screen` (264행): 월드 슬롯의 위치를 카메라로 투영한 화면 좌표.

- `local` (265행): 화면 좌표를 HUD 내부 좌표로 변환한 결과다. out var로 받아 버튼의 anchoredPosition에 넣는다.

- `rect` (267행): UI 위치·크기 또는 버튼 중앙 좌표 계산에 사용하는 RectTransform.

### StageBattleView.Required

부모 아래의 필수 리소스를 상대 경로로 찾는다. 없으면 누락된 경로가 드러나는 예외를 발생시킨다.

선언: `private static Transform Required(Transform parent, string path)`. [원본 278행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L278)

반환: parent.Find(path) 결과. 없으면 누락 경로를 담은 InvalidOperationException이다. 컴포넌트 존재까지 검사하는 함수는 아니다.

**입력값**

- `Transform parent`: 생성하거나 조회할 오브젝트의 부모 Transform.

- `string path`: 부모 기준으로 찾을 자식의 상대 경로.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.

### StageBattleView.Panel

새 HUD에 입력을 가로채지 않는 색상 패널을 생성해 반환한다.

선언: `private Image Panel(string name, float x, float y, float w, float h, Color color)`. [원본 288행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L288)

반환: 새 Image. RectTransform/Image가 있는 GameObject를 만들고 Place로 배치한다. raycastTarget=false라 장식 패널이 입력을 가로채지 않는다.

**입력값**

- `string name`: 생성할 UI 오브젝트의 이름.

- `float x`: 부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.

- `float y`: 부모 위쪽에서 아래쪽으로 떨어진 UI 거리.

- `float w`: UI 영역의 가로 크기.

- `float h`: UI 영역의 세로 크기.

- `Color color`: 패널 또는 글자에 적용할 색상.

**함수 내부의 변수·반복/람다 이름**

- `go` (291행): UI 컴포넌트를 담기 위해 생성한 게임 오브젝트.

- `image` (293행): 생성한 UI 배경의 Image 컴포넌트.

### StageBattleView.Label

새 HUD에 한국어 텍스트를 생성한다. 클릭 판정은 버튼에서 처리하도록 raycast를 끈다.

선언: `private Text Label(string name, string caption, float x, float y, float w, float h, int size)`. [원본 305행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L305)

반환: 새 Text. 폰트·본문·크기·기본 흰색·왼쪽 정렬을 지정하고 raycastTarget을 끈다.

**입력값**

- `string name`: 생성할 UI 오브젝트의 이름.

- `string caption`: 버튼 또는 텍스트에 표시할 문구.

- `float x`: 부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.

- `float y`: 부모 위쪽에서 아래쪽으로 떨어진 UI 거리.

- `float w`: UI 영역의 가로 크기.

- `float h`: UI 영역의 세로 크기.

- `int size`: 글자 크기.

**함수 내부의 변수·반복/람다 이름**

- `go` (308행): UI 컴포넌트를 담기 위해 생성한 게임 오브젝트.

- `label` (310행): 문구·폰트·정렬을 설정할 UI Text 컴포넌트.

### StageBattleView.NewButton

부족한 조작에 사용할 버튼 배경을 생성하고 문구와 이벤트를 연결한다.

선언: `private Button NewButton(string name, string caption, float x, float y, float w, float h, Action action)`. [원본 323행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L323)

반환: 새 Button. Panel의 Image에 Button을 붙인 뒤 Rebind로 클릭 가능 상태, 텍스트, 이벤트를 완성한다.

**입력값**

- `string name`: 생성할 UI 오브젝트의 이름.

- `string caption`: 버튼 또는 텍스트에 표시할 문구.

- `float x`: 부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.

- `float y`: 부모 위쪽에서 아래쪽으로 떨어진 UI 거리.

- `float w`: UI 영역의 가로 크기.

- `float h`: UI 영역의 세로 크기.

- `Action action`: 버튼 클릭 시 실행할 콜백.

**함수 내부의 변수·반복/람다 이름**

- `image` (326행): 생성한 UI 배경의 Image 컴포넌트.

- `button` (328행): 클릭 이벤트를 연결하거나 검증할 UI 버튼.

### StageBattleView.Rebind

버튼을 새 HUD로 옮기고 구형 문구를 숨긴 뒤 클릭 이벤트를 새 콜백으로 교체한다. 변경은 현재 실행 인스턴스에만 적용한다.

선언: `private void Rebind(Button button, string caption, float x, float y, float w, float h, Action action)`. [원본 341행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L341)

기존/신규 버튼 모두에 쓰인다. HUD로 이동, 자식 숨김, 새 클릭 이벤트 지정, Image raycast 활성화, 새 캡션, 비활성 색, 키보드 자동 탐색 끄기를 수행한다. 구형 자식은 삭제하지 않는다.

**입력값**

- `Button button`: 클릭하거나 새 HUD에 다시 연결할 버튼.

- `string caption`: 버튼 또는 텍스트에 표시할 문구.

- `float x`: 부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.

- `float y`: 부모 위쪽에서 아래쪽으로 떨어진 UI 거리.

- `float w`: UI 영역의 가로 크기.

- `float h`: UI 영역의 세로 크기.

- `Action action`: 버튼 클릭 시 실행할 콜백.

**함수 내부의 변수·반복/람다 이름**

- `child` (344행): 이번 반복의 숨김 여부를 정할 자식 Transform다. 순회 대상: `button.transform`.

- `label` (348행): 문구·폰트·정렬을 설정할 UI Text 컴포넌트.

- `colors` (351행): 버튼 상태별 색 설정의 복사본. 수정 후 버튼에 다시 대입한다.

- `navigation` (353행): 버튼 키보드 탐색 설정의 복사본. 슬롯 사이의 의도치 않은 자동 탐색을 끈다.

### StageBattleView.Place

부모의 왼쪽 위를 원점으로 UI 위치와 크기를 지정한다. x는 오른쪽, y는 아래쪽 방향의 거리다.

선언: `private static void Place(GameObject go, Transform parent, float x, float y, float w, float h)`. [원본 364행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L364)

좌상단 앵커·피벗을 사용한다. 입력 y는 아래쪽 거리이므로 anchoredPosition에는 -y를 넣는다. SetParent(false)와 localScale=1로 부모 변경 뒤 UI 배율을 맞춘다.

**입력값**

- `GameObject go`: 부모·위치·크기를 설정할 UI 오브젝트.

- `Transform parent`: 생성하거나 조회할 오브젝트의 부모 Transform.

- `float x`: 부모 왼쪽에서 오른쪽으로 떨어진 UI 거리.

- `float y`: 부모 위쪽에서 아래쪽으로 떨어진 UI 거리.

- `float w`: UI 영역의 가로 크기.

- `float h`: UI 영역의 세로 크기.

**함수 내부의 변수·반복/람다 이름**

- `rect` (368행): UI 위치·크기 또는 버튼 중앙 좌표 계산에 사용하는 RectTransform.

### StageBattleView.OnDestroy

이 화면이 런타임에 생성한 폰트를 해제한다. 원본 에셋 폰트는 수정하지 않는다.

선언: `private void OnDestroy()`. [원본 375행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Unity/StageBattleView.cs#L375)

동적으로 생성한 font가 있으면 Destroy한다. Inspector 스프라이트 에셋을 지우지 않는다.

별도의 입력 매개변수는 없다.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.
