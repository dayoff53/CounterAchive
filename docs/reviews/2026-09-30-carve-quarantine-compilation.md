# 카브 격리 후 Unity 컴파일 증거

실행일: 2026-09-30 (Asia/Seoul). Unity `6000.5.10f1 (3bd4f66ad299)`.

실행 중인 Unity가 없는 상태를 확인하고 숨김 배치 프로세스를 실행했다. 첫 제한 환경 실행은 Unity 라이선스 연결 실패로 컴파일 결과를 얻지 못해 해당 검증 프로세스를 종료했다. 정상 권한 환경에서 다시 실행한 결과는 다음과 같다.

```text
Unity.exe -batchmode -nographics -quit -projectPath C:\Fork\CounterAchive
  -logFile C:\Fork\CounterAchive\Logs\carve-quarantine-compile-full.log
Exit code: 0
Log SHA-256: 0061302379EB5ED331CFBAC5D09CB08CB6DD29A43E30970CA727C7F889E2C030
```

전체 로그는 로컬 `Logs/carve-quarantine-compile-full.log`에 있다. Logs는 Git 제외 대상이며 아래에 결과 판단에 필요한 발췌를 남긴다.

## 실제 컴파일

```text
[ScriptCompilation] Requested script compilation because: AssetDatabase observed changes in script compilation related files
DisplayProgressbar: Compiling Scripts
Finished compiling graph: 1100 nodes, 22290 flattened edges (18572 ToBuild, 176 ToUse), maximum node priority 1200
[1093/1097  4s] Csc Library/Bee/artifacts/1900b0aE.dag/Assembly-CSharp.dll (+2 others)
Begin MonoManager ReloadAssembly
Exiting batchmode successfully now!
Exiting without the bug reporter. Application will terminate with return code 0
```

`error CS` / `Scripts have compiler errors` 보고 없음. C# 경고는 중복 로그를 제외하여 5개다.

| 경고 | 위치 | 내용 |
| --- | --- | --- |
| CS0114 | Assets/Script/Production/FadeManager.cs:27 | Awake가 Singleton의 Awake를 숨김 |
| CS0618 | Assets/Script/System/Singleton.cs:18 | FindFirstObjectByType 폐기 예정 API |
| CS0414 | Assets/Script/Production/PlayerCameraController.cs:21 | minZoom 미사용 |
| CS0414 | Assets/Script/Production/PlayerCameraController.cs:22 | maxZoom 미사용 |
| CS0414 | Assets/Script/Production/PlayerCameraController.cs:24 | cameraZoomSpeed 미사용 |

## 컴파일과 별개의 에디터 초기화 문제

```text
Exception while executing InitializeOnLoad for GUITimeHelper.Init
Exception while executing InitializeOnLoad for ImguiElementUtils.Init
MissingFieldException: Field not found: System.Action`1<UnityEngine.UIElements.IMGUIContainer>
  UnityEngine.UIElements.UIElementsUtility.s_BeginContainerCallback
```

스택은 Sirenix.Utilities.Editor 코드에서 발생한다. 기존 플러그인과 Unity 버전의 호환성 문제로 추정되지만 격리 전 재현 검증은 하지 않았다. 이 예외가 있으므로 종료 코드 0과 C# 컴파일 성공만으로 에디터 정상 동작을 주장하지 않는다.

## 보존 검사

컴파일 종료 후 `Tools/Verify-CarveQuarantine.ps1` 실행 결과:

```text
QuarantinedScripts      : 49
PreservedAssetFiles     : 3978
ArchiveHashes           : PASS
PreservedAssetHashes    : PASS
RetiredSourceReferences : NONE (static scan)
```

이번 작업에서 Play Mode, 새 규칙 테스트, 플레이어 빌드는 수행하지 않았다. 기존 씬은 Missing Script를 포함한 카브 리소스 보존본이며 새 마키보 전투는 아직 구현 전이다.
