# 08. 검증 씬 생성과 열기 — TurnSandboxScene

기준: 2026-10-01 · 커밋 f69ceba · 검토 대기

[소스 파일 열기](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Editor/TurnSandboxScene.cs)

저장된 독립 검증 씬을 열고, 파일이 없을 때만 최소한의 카메라와 컨트롤러를 만들어 저장한다. 기존 Stage_Battle이나 빌드 씬 목록을 바꾸지 않는다.

## 메뉴 실행 흐름

Open → 재생 중이면 중단 → 현재 수정 씬 저장 확인 → Create → OpenScene 순서다.

Create는 ScenePath에 파일이 있으면 로그를 남기고 반환한다. 파일이 없으면 폴더를 만들고 빈 단일 씬에 Main Camera와 Masei Turn Sandbox를 만든다. 새 컨트롤러는 Play의 Awake에서 기본 TurnSandboxView를 자동으로 생성한다.

Create를 도구 코드에서 직접 호출할 경우 Open의 재생/저장 확인을 거치지 않는다. 저장된 씬이 없을 때 NewSceneMode.Single로 새 씬을 열기 때문에, 일반 사용자는 Open 메뉴를 통해 접근하는 것이 현재 코드의 안전한 사용 순서다.

## 변수·함수 상세

소스 선언 순서로 정리했다. 원본 링크에서 실제 코드 위치를 바로 열 수 있다. 함수 내부 변수는 해당 함수 아래에 묶었다.

## TurnSandboxScene

독립 턴 검증 씬을 생성하거나 여는 에디터 도구. 기존 전투 씬과 빌드 씬 목록은 수정하지 않는다. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Editor/TurnSandboxScene.cs#L12)

- **TurnSandboxScene.ScenePath**: 독립 턴 검증 씬을 생성하고 여는 프로젝트 상대 경로. [원본 17행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Editor/TurnSandboxScene.cs#L17)

### TurnSandboxScene.Create

검증 씬이 없을 때 빈 씬에 카메라와 턴 컨트롤러를 만들고 저장한다. 이미 존재하는 파일은 덮어쓰지 않는다.

선언: `public static void Create()`. [원본 22행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Editor/TurnSandboxScene.cs#L22)

반환값 없음. 이미 존재하는 씬을 덮어쓰지 않는다. 새 카메라는 MainCamera 태그, 위치 (0,0,-10), 단색 배경으로 설정한다. SaveScene 실패 시 IOException. 저장 뒤 AssetDatabase.Refresh와 생성 로그를 남긴다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `scene` (27행): 검증용 카메라와 컨트롤러를 저장할 새 빈 씬.

- `camera` (29행): 씬을 렌더링할 카메라.

### TurnSandboxScene.Open

현재 씬의 저장 여부를 확인하고, 필요하면 검증 씬을 생성한 뒤 연다. 재생 중에는 동작하지 않는다.

선언: `public static void Open()`. [원본 43행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Editor/TurnSandboxScene.cs#L43)

Unity 메뉴 진입점. 저장 확인이 끝나면 Create를 호출해 파일 존재를 보장하고 ScenePath를 연다.

별도의 입력 매개변수는 없다.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.
