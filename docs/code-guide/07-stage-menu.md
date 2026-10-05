# 07. 실제 전투 씬 열기 — StageBattleScene

기준: 2026-10-01 · 커밋 f69ceba · 검토 대기

[소스 파일 열기](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Editor/StageBattleScene.cs)

Unity 상단 메뉴의 '마키보 → Stage_Battle 열기'를 제공하는 작은 에디터 전용 도구다. 게임 실행 중 턴을 처리하는 클래스가 아니다.

## 작동 순서

Open → 재생 중인지 확인 → 수정된 씬 저장 여부 대화상자 → 사용자가 계속 진행하면 ScenePath 열기 순서다.

SaveCurrentModifiedScenesIfUserWantsTo는 Unity의 기존 저장 확인 절차를 사용한다. 사용자가 취소하면 false이므로 현재 씬에 머문다. 저장하지 않기를 선택해 진행하는 것과 취소는 다르다.

이 클래스에는 유닛 데이터, 상태 변수, 반복문 내부 변수는 없다. ScenePath 상수 하나와 Open 함수 하나가 핵심이다. Editor 어셈블리에만 들어가므로 플레이어 빌드의 런타임 메뉴가 아니다.

## 변수·함수 상세

소스 선언 순서로 정리했다. 원본 링크에서 실제 코드 위치를 바로 열 수 있다. 함수 내부 변수는 해당 함수 아래에 묶었다.

## StageBattleScene

Unity 메뉴에서 기존 리소스를 연결한 마키보 전투 씬을 여는 에디터 도구. [원본](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Editor/StageBattleScene.cs#L9)

- **StageBattleScene.ScenePath**: 마키보 전투 실행 기준인 Stage_Battle 씬의 프로젝트 상대 경로. [원본 14행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Editor/StageBattleScene.cs#L14)

### StageBattleScene.Open

재생 중에는 동작하지 않는다. 편집 중인 씬의 저장 여부를 확인한 뒤 Stage_Battle을 연다.

선언: `public static void Open()`. [원본 18행](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Assets/MaseiKivotos/Editor/StageBattleScene.cs#L18)

반환값 없음. EditorApplication.isPlaying이면 바로 반환한다. 저장 확인이 취소되면 중단하고, 그렇지 않으면 EditorSceneManager.OpenScene(ScenePath)을 호출한다.

별도의 입력 매개변수는 없다.

이 함수에 이름을 붙여 선언한 지역 변수는 없다.
