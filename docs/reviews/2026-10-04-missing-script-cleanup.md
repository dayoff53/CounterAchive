# Missing Script 컴포넌트 정리

2026-10-04 / 상태: **검토 대기** / 검토자: dayoff53

## 목적과 경계

Inspector의 Mono Script 연결이 비어 있는 컴포넌트를 제거해 달라는 사용자 요청을 적용한다. 2026-09-30 격리 기록의 “Missing Script를 일괄 제거하지 않는다”는 당시 보존 방침이며, 이번 명시적 요청에 따라 씬/프리팹의 누락 컴포넌트를 정리한다. 격리 스크립트와 당시 manifest는 보존한다.

정상 컴포넌트의 Inspector 필드가 `None`인 것은 제거 조건이 아니다. Unity의 `GameObjectUtility.GetMonoBehavioursWithMissingScriptCount` 및 `RemoveMonoBehavioursWithMissingScript`로 실제 누락된 MonoBehaviour만 판정하고 제거한다. 비활성 GameObject도 검사한다.

ScriptableObject `.asset`은 GameObject 컴포넌트가 아니므로 삭제하지 않는다. 격리된 스크립트를 참조하는 구형 데이터도 추후 이행을 위해 유지한다. GameObject, Transform, 정상 컴포넌트, 이미지, 애니메이션, 프리팹/씬 에셋 자체를 삭제하는 작업이 아니다.

## 실행 방법

- 원본 에디터와 분리된 `Logs/StageValidation`에 최신 파일을 동기화했다.
- `Tools/MissingScriptCleanup.cs`를 사본의 `Assets/MaseiKivotos/Editor`에 복사하고 `-executeMethod MakiboMaintenance.MissingScriptCleanup.Run`으로 실행한다. 원본 프로젝트에서 실행하지 못하도록 경로를 제한했다.
- `Assets` 아래 씬/프리팹을 모두 검사하고, 원본 프리팹 → 의존 프리팹 → 씬 순서로 정리한다. 정상 에셋은 저장하지 않는다. 프리팹 상속으로 여러 곳에서 보이는 동일한 누락은 원본에서 제거한다.
- 정리 전후 파일과 제거 위치를 기록하고, 작업 시작 해시와 현재 원본 해시가 일치하는 파일만 적용한다. 사용자 기존 변경이 포함된 현재 파일을 백업하며 Git HEAD로 덮어쓰지 않는다.

## 검증 및 최종 결과

- **231개 씬/프리팹 전수 검사 → 32개 파일 수정(프리팹 9개, 씬 23개) → 직접 누락 컴포넌트 167개 제거 → 재검사 0개.** 정리 전 989개는 중첩/상속 인스턴스별 누락을 중복 집계한 값이다.
- 사용자 리소스와 구형 씬 외에 실제 누락이 있던 `Dark UI/DarkUI.unity`, TMP의 `24 - Surface Shader Example URP.unity`, `28 - HDRP Shader Example.unity`도 포함한다. 정상 서드파티 예제는 저장하지 않았다.
- 모든 검사 에셋에서 정리 전후 GameObject 계층·정상 컴포넌트 유형/개수·활성 상태·태그/레이어·로컬 위치/회전/크기·SpriteRenderer/UI Image 스프라이트·Animator 컨트롤러 참조 지문이 일치했다.
- **Unity 사본 컴파일 성공, PlayMode 11/11 통과.** 기존 턴/스킬/이동/HP UI 테스트를 실행했다. Core 로직 변경이 없어 EditMode는 재실행하지 않았다. 원본 에디터의 수동 플레이와 플레이어 빌드는 미실행이다.
- 원본 `Assets` 파일 **4,040개**를 작업 전 해시와 대조했다. 승인된 32개 수정 외의 변경/삭제는 0개다. `.meta`와 GUID, ScriptableObject·이미지·애니메이션 파일도 보존했다. 씬/프리팹의 `m_Script`에서 격리된 카브 GUID를 직접 참조하는 항목은 0개다.
- Unity 저장에 따른 직렬화 버전/신규 기본 필드와 사용하지 않는 `stripped` 블록 정리도 diff에 포함된다. 실제 오브젝트 삭제와 구분하며, 원본은 [백업과 manifest](../../Legacy/MissingScripts/2026-10-04/README.md)에 보관했다. 사본 검증 후 현재 원본 해시를 다시 확인하고 적용했다.
- [Unity 제거 위치 목록](../../Legacy/MissingScripts/2026-10-04/unity-report.json), [최종 검증 증거](evidence/2026-10-04-missing-script-cleanup.json). 실행 로그/XML: `Logs/missing-script-cleanup-verified.log`, `Logs/missing-script-playmode.log`, `Logs/missing-script-playmode.xml`.

## dayoff53 확인 순서

1. Unity가 변경된 에셋을 임포트한 뒤 `Stage_Battle` 씬을 다시 연다. 열린 씬에 별도 미저장 편집이 있다면 먼저 보존하고 디스크 변경을 다시 읽는다.
2. `InGame/UnitSlots` 아래 슬롯, `SlotGroundSprite`, `UnitBase` 및 캐릭터 자식을 선택해 Inspector에 `Missing (Mono Script)`가 없는지 확인한다.
3. Play 후 슬롯/HP UI, 스킬 예약/피해, 이동 미리보기/실행, 턴 넘기기와 초기화를 확인한다.

## 남은 사항

카브의 예전 게임 진행 기능을 복구하는 작업은 아니다. Sirenix/Odin의 에디터 초기화 예외는 누락 컴포넌트와 별도 문제다.

PlayMode 로그에는 오브젝트 이름이 없는 `The referenced script (Unknown) on this Behaviour is missing!` 경고 24건이 남았다. 전체 씬/프리팹의 GameObject 컴포넌트 재검사는 0개이며, 이 잔여 경고의 발생 에셋은 이번에 특정하지 못했다. 보존한 구형 ScriptableObject 등의 원인이라고 단정하지 않는다. 따라서 **Inspector 컴포넌트 정리 완료와 콘솔 경고 전체 해소를 구분**한다. 원본 에디터 직접 조작과 플레이어 빌드는 별도 검증이 필요하다.
