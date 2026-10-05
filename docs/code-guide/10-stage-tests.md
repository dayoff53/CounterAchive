# 10. 실제 씬 연결 검증 — StageBattleTests

기준: 2026-10-01 · 커밋 f69ceba · 검토 대기

[소스 파일 열기](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/PlayMode/StageBattleTests.cs)

Stage_Battle을 PlayMode로 불러와 기존 리소스와 새 턴 코드가 함께 작동하는지 검사한다. Core 상태뿐 아니라 버튼 가림, 화면 문구, COST 바, 캐릭터 렌더러까지 확인한다.

## 한 테스트가 따라가는 시나리오

1. 실제 Stage_Battle 씬을 단일 씬으로 불러온다.
2. StageBattleView와 공용 Controller를 찾고, SandboxView가 겹쳐 생성되지 않았는지 확인한다.
3. 9슬롯, 기존 버튼 이름, TURN 01, COST 바 0.3, 바닥과 네 캐릭터를 검사한다.
4. 첫 아군 슬롯을 두 번 눌러 예약 추가/취소를 확인한다.
5. Step을 두 번 눌러 확정 → A 차례 시작을 확인한다.
6. Skip과 직접 중복 Skip 호출 뒤 TURN 02, Planning, 게이지 0.5, Finished 4개를 검사한다.
7. 자동 진행 중 Reset을 누르고 기다려도 1턴에 남는지 검사한다.
8. 시작/다음 턴 화면을 PNG로 남긴다.

## Click이 단순 onClick 호출보다 확인하는 것

Canvas를 갱신하고 버튼 중앙을 화면 좌표로 계산한다. EventSystem.RaycastAll의 첫 결과가 목표 버튼인지 확인한 다음 포인터 클릭 이벤트를 전달한다. 투명하거나 오래된 UI가 버튼을 가리는 문제를 잡기 위한 검사다. 실제 운영체제 마우스 장치를 움직이는 테스트는 아니다.

## Capture의 범위

1280×720 RenderTexture와 Texture2D로 실제 씬/UI를 렌더링한다. 두 프레임 대기 후 글자 메시를 다시 만들고 PNG로 저장한다. 그래픽 장치가 Null이면 촬영을 건너뛴다.

finally에서 활성 RenderTexture와 캡처 장치를 정리하고 Canvas를 이 검증 화면의 Overlay/ScaleWithScreenSize 설정으로 돌린다. 변경한 모든 속성의 원래 값을 일반적으로 백업·복원하는 도구는 아니다. 이 씬 테스트를 위해 만든 촬영 보조 함수다.

UNITY_EDITOR가 아닌 분기에서는 Assert.Ignore로 건너뛴다. 따라서 통과 결과는 실제 플레이어 빌드 검증을 대신하지 않는다.

## 변수·함수 상세

소스 선언 순서로 정리했다. 원본 링크에서 실제 코드 위치를 바로 열 수 있다. 함수 내부 변수는 해당 함수 아래에 묶었다.

## StageBattleTests

기존 InGame 리소스와 새 턴 로직의 연결을 실제 씬에서 확인하는 PlayMode 테스트. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/PlayMode/StageBattleTests.cs#L22)

### StageBattleTests.RetainedSlotsAndButtonsDriveTheSharedTurnBattle

실제 9슬롯·재사용 버튼·캐릭터·COST 바를 확인하고 예약 토글부터 다음 턴과 초기화까지 검증한다.

선언: `public IEnumerator RetainedSlotsAndButtonsDriveTheSharedTurnBattle()`. [원본 27행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/PlayMode/StageBattleTests.cs#L27)

반환: 테스트 러너가 프레임별로 실행하는 IEnumerator. 씬 로드·화면 생성 후 검사한다. controller.StepDelay를 0으로 줄여 자동 진행을 빠르게 확인하고, 마지막 리셋 검사는 0.1초 지연으로 실제 대기 취소를 확인한다.

별도의 입력 매개변수는 없다.

**함수 내부의 변수·반복/람다 이름**

- `view` (37행): 실제 Stage_Battle 씬에 연결된 새 턴 화면.

- `controller` (40행): 로드된 씬에서 실제로 동작하는 턴 진행 컨트롤러.

- `t` (50행): 함수식이 검사/변환 중인 슬롯 Transform 한 개다. 적용하는 판정/변환은 `t.GetComponentsInChildren<SpriteRenderer>()`다.

- `r` (50행): 함수식이 검사/변환 중인 SpriteRenderer 한 개다. 적용하는 판정/변환은 `r.name == "UnitCharactorSprite" && r.sprite != null`다.

- `i` (64행): 0부터 증가하는 반복 인덱스다. 이 함수의 배열/목록 순회 또는 테스트의 최대 대기 프레임 수를 센다. 각 for문마다 별개다.

- `e` (70행): 함수식이 검사/변환 중인 진행 기록 한 개다. 적용하는 판정/변환은 `e.Kind == TurnEventKind.Finished`다.

### StageBattleTests.Capture

실제 씬을 임시 RenderTexture에 렌더링해 Logs에 PNG로 저장한다. 그래픽 장치가 없는 실행은 건너뛰며 변경한 Canvas/카메라 설정을 복구한다.

선언: `private static IEnumerator Capture(StageBattleView view, string filename)`. [원본 84행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/PlayMode/StageBattleTests.cs#L84)

view로 Canvas를 찾고 filename으로 Logs 저장명을 정한다. RenderTexture.active는 previous로 복원하고 카메라 targetTexture는 null로 되돌린다. 임시 GPU/CPU 텍스처를 Destroy한다.

**입력값**

- `StageBattleView view`: 촬영할 Stage_Battle 화면.

- `string filename`: Logs 폴더에 저장할 PNG 파일 이름.

**함수 내부의 변수·반복/람다 이름**

- `canvas` (88행): 촬영 대상 UI를 포함하는 Canvas.

- `scaler` (90행): 해상도에 따른 UI 크기를 조절하는 CanvasScaler.

- `camera` (92행): 씬을 렌더링할 카메라.

- `target` (94행): 실제 씬과 UI를 지정 해상도로 촬영하는 임시 RenderTexture.

- `texture` (96행): 렌더링 결과를 읽어 PNG로 변환할 CPU 측 Texture2D.

- `previous` (98행): 캡처 종료 후 복구할 기존 활성 RenderTexture.

- `label` (107행): 이번 반복의 캡처용 메시를 갱신할 Text다. 순회 대상: `canvas.GetComponentsInChildren<Text>()`.

### StageBattleTests.Click

버튼 중앙에 실제 UI raycast를 수행해 가림이 없는지 확인한 뒤 왼쪽 클릭 이벤트를 전달한다.

선언: `private static void Click(Button button)`. [원본 127행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Tests/PlayMode/StageBattleTests.cs#L127)

반환값 없음. button의 RectTransform 중심을 계산하고 PointerEventData의 왼쪽 버튼 이벤트를 만든다. hits가 비거나 첫 대상이 다른 UI이면 Assert 실패다.

**입력값**

- `Button button`: 클릭하거나 새 HUD에 다시 연결할 버튼.

**함수 내부의 변수·반복/람다 이름**

- `rect` (131행): UI 위치·크기 또는 버튼 중앙 좌표 계산에 사용하는 RectTransform.

- `pointer` (133행): 목표 버튼 중앙을 왼쪽 클릭하는 테스트용 포인터 이벤트.

- `hits` (138행): 포인터 위치의 UI raycast 결과. 첫 항목이 목표 버튼인지 확인한다.
