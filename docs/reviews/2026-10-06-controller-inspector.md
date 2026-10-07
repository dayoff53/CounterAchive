# TurnSandboxController Inspector 표시 복구

2026-10-06 / 상태: **검토 대기** / 검토자: dayoff53

## 확인한 증상과 원인

원본 Unity `6000.5.10f1`에서 `Stage_Battle / Makibo Battle`을 선택한 화면을 확인했다. `Turn Sandbox Controller (Script)`가 펼쳐져 있지만 필드 영역이 충분한 높이를 확보하지 못해 `Add Component` 버튼이 겹쳐 있었다.

동시에 Console에 `MissingMethodException`이 반복되었다. 누락된 메서드는 `UnityEngine.UIElements.UIElementsUtility.GetCurrentIMGUIContainer()`이며 호출 경로는 `Sirenix.OdinInspector.Editor.Internal.UIToolkitIntegration.ImguiElementUtils.EmbedIMGUI`다. 이 프로젝트의 Odin Inspector가 Unity 6.5의 내부 UI API와 맞지 않는 그리기 경로를 사용한다. 이전 로그의 `s_BeginContainerCallback` 초기화 오류와 이번 펼침 시의 `MissingMethodException`을 구분한다.

## 수정

`Assets/MaseiKivotos/Editor/TurnSandboxControllerEditor.cs`에 해당 컴포넌트 전용 `CustomEditor`를 추가한다. `UnityEditor.Editor`를 상속하고 `DrawDefaultInspector()`로 표시해 Odin의 해당 그리기 경로를 사용하지 않는다. `CanEditMultipleObjects`도 지정한다.

- 표시 대상은 기존 직렬화 필드인 `Step Delay`, `Random Seed`와 스크립트 참조다.
- Unity 기본 Inspector가 Min 제한, Undo, 다중 선택, 프리팹 오버라이드를 처리한다.
- 런타임 컨트롤러/전투 규칙, 씬의 설정값과 참조, Odin 플러그인 및 전역 설정은 변경하지 않는다.
- `Battle`, `View` 같은 실행 중 프로퍼티를 새로 Inspector에 노출하는 작업은 아니다.

## 검증

- 원본 에디터가 새 스크립트를 컴파일한 뒤 실제 화면에서 `Script`, `Step Delay = 0.3`, `Random Seed = 530`이 각각 정상 배치되고 `Add Component` 버튼이 그 아래로 분리되는 것을 확인했다. 값은 씬에 저장된 기존 값과 일치한다.
- 펼쳐진 화면에서 수정 전의 반복 `MissingMethodException` 대신 기존 Odin 초기화 오류 4건만 남은 Console 상태를 확인했다. 전체 Odin 오류 해소를 의미하지 않는다.
- 검증 사본 `Logs/StageValidation`에서 **컴파일 성공, PlayMode 11/11 통과**. 로그/XML은 `Logs/inspector-fix-playmode.log`, `Logs/inspector-fix-playmode.xml`이다.
- 원본과 사본의 새 에디터 코드 SHA-256이 일치한다. `Stage_Battle.unity`와 런타임 `TurnSandboxController.cs`는 HEAD 대비 변경이 없다. [검증 요약](evidence/2026-10-06-controller-inspector.json).
- 이번 변경은 에디터 표시만 수정하므로 EditMode 규칙 테스트 및 플레이어 빌드는 재실행하지 않았다. Undo/다중 선택/프리팹 오버라이드별 수동 조작은 별도 검증하지 않았다.

## dayoff53 확인 순서

1. Unity의 스크립트 컴파일이 끝나면 `Stage_Battle`의 `Makibo Battle`을 선택한다.
2. `Turn Sandbox Controller (Script)` 왼쪽 삼각형을 접었다가 다시 펼친다.
3. `Script`, `Step Delay`, `Random Seed`가 각각 보이고 `Add Component`는 컴포넌트 아래에 배치되는지 확인한다.

## 남은 범위

이 수정은 해당 컴포넌트의 Inspector 표시를 복구한다. Odin 전체의 Unity 6.5 호환성을 고친 것은 아니므로 다른 Odin 화면이나 에디터 초기화의 별도 오류는 남을 수 있다. 플러그인 업그레이드 및 플레이어 빌드는 이번 범위가 아니다.
