# 11. 검증 화면의 회귀 검사 — TurnSandboxTests

기준: 2026-10-01 · 커밋 f69ceba · 검토 대기

[소스 파일 열기](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/PlayMode/TurnSandboxTests.cs)

독립 TurnSandbox 씬에서 같은 턴 흐름을 확인한다. 실제 Stage 리소스가 바뀌더라도 공용 Controller와 기본 화면이 정상인지 따로 확인하는 회귀 테스트다.

## 실행 순서

씬 로드 → Controller 존재와 1턴/COST 3/Planning 확인 → 초기 화면 캡처 → Step으로 예약 확정 → Step으로 A 시작 → Skip으로 다음 턴 → 중복 입력 차단 확인 → 2턴/COST 5/Planning과 Finished 4개 확인 → 다음 턴 캡처 → 자동 진행 도중 Reset → 1턴/COST 3 복귀 확인 순서다.

코루틴을 기다리는 반복문은 64프레임 한도다. 무한히 기다리는 대신 IsAdvancing이 꺼졌는지 Assert로 검사한다. 검증 과정은 StepDelay=0으로 빨리 진행하지만 한 번의 yield는 유지된다.

## 두 보조 함수

Click은 Stage 테스트와 같은 목적으로 raycast의 첫 대상이 버튼인지 확인한다. Capture는 1280×800으로 검증 UI를 저장한다. previousScale을 추가로 저장하고 캡처 뒤 Canvas.scaleFactor를 복구한다.

캡처 파일은 Application.dataPath의 상위 Logs에 저장되며 Git에 자동 포함되는 게임 리소스가 아니다. finally는 실패 시에도 임시 텍스처를 정리하려는 구조다.

이 테스트도 UNITY_EDITOR 외에서는 Ignore된다. 이전 실행에서 이 테스트와 Stage 테스트가 각각 통과해 PlayMode 결과가 2/2다.

## 변수·함수 상세

소스 선언 순서로 정리했다. 원본 링크에서 실제 코드 위치를 바로 열 수 있다. 함수 내부 변수는 해당 함수 아래에 묶었다.

## TurnSandboxTests

저장된 독립 검증 씬을 로드해 실제 UI 클릭과 턴 진행을 확인하는 PlayMode 테스트. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/PlayMode/TurnSandboxTests.cs#L22)

### TurnSandboxTests.SceneButtonsAdvanceRealTurnAndResetCancelsPendingWork

검증 씬 버튼으로 차례와 다음 턴을 진행하고 COST 증가·중복 입력 방지·처리 중 초기화를 검증한다.

선언: `public IEnumerator SceneButtonsAdvanceRealTurnAndResetCancelsPendingWork()`. [원본 27행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/PlayMode/TurnSandboxTests.cs#L27)

반환: 프레임에 걸쳐 실행하는 IEnumerator. 버튼 비활성화와 직접 중복 호출을 모두 확인한다. Reset 이후 0.3초가 지나도 이전 코루틴이 턴을 밀어 버리지 않아야 한다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `controller` (38행): 로드된 씬에서 실제로 동작하는 턴 진행 컨트롤러.

- `i` (57행): 0부터 증가하는 반복 인덱스다. 이 함수의 배열/목록 순회 또는 테스트의 최대 대기 프레임 수를 센다. 각 for문마다 별개다.

- `e` (63행): 함수식이 검사/변환 중인 진행 기록 한 개다. 적용하는 판정/변환은 `e.Kind == TurnEventKind.Finished`다.

### TurnSandboxTests.Click

버튼 중앙에 실제 UI raycast를 수행해 가림이 없는지 확인한 뒤 왼쪽 클릭 이벤트를 전달한다.

선언: `private static void Click(Button button)`. [원본 80행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/PlayMode/TurnSandboxTests.cs#L80)

반환값 없음. 버튼 중앙의 PointerEventData, RaycastResult 목록을 만들고 목표 버튼 도달 가능성을 검사한 뒤 클릭을 전달한다.

**입력값**

- `Button button`: 클릭하거나 새 HUD에 다시 연결할 버튼.

**함수 내부의 변수·반복/람다 이름**

- `pointer` (84행): 목표 버튼 중앙을 왼쪽 클릭하는 테스트용 포인터 이벤트.

- `hits` (90행): 포인터 위치의 UI raycast 결과. 첫 항목이 목표 버튼인지 확인한다.

### TurnSandboxTests.Capture

실제 씬을 임시 RenderTexture에 렌더링해 Logs에 PNG로 저장한다. 그래픽 장치가 없는 실행은 건너뛰며 변경한 Canvas/카메라 설정을 복구한다.

선언: `private static IEnumerator Capture(TurnSandboxController controller, string filename)`. [원본 102행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/PlayMode/TurnSandboxTests.cs#L102)

controller 아래 Canvas를 촬영한다. filename은 Logs의 PNG 이름이다. 활성 RenderTexture와 Canvas 배율을 저장하고 마지막에 복구한다. 카메라/Canvas는 검증 화면의 알려진 설정으로 돌린다.

**입력값**

- `TurnSandboxController controller`: 전투 상태와 사용자 조작을 제공하는 공용 컨트롤러.

- `string filename`: Logs 폴더에 저장할 PNG 파일 이름.

**함수 내부의 변수·반복/람다 이름**

- `canvas` (106행): 촬영 대상 UI를 포함하는 Canvas.

- `scaler` (108행): 해상도에 따른 UI 크기를 조절하는 CanvasScaler.

- `camera` (110행): 씬을 렌더링할 카메라.

- `target` (112행): 실제 씬과 UI를 지정 해상도로 촬영하는 임시 RenderTexture.

- `texture` (114행): 렌더링 결과를 읽어 PNG로 변환할 CPU 측 Texture2D.

- `previous` (116행): 캡처 종료 후 복구할 기존 활성 RenderTexture.

- `previousScale` (118행): 캡처용 배율로 바꾸기 전에 보관하는 Canvas 배율.

- `folder` (132행): 캡처 PNG를 저장할 프로젝트 Logs 폴더의 절대 경로.
