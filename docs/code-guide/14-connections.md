# 14. 어셈블리·씬·메타·검증 기록의 연결

기준: 2026-10-01 · 커밋 f69ceba · 검토 대기

C# 파일 밖에서도 어떤 코드를 함께 컴파일하고 어떤 씬이 실행 진입점인지 결정하는 파일이 있다. 함수가 있는 스크립트와 설정 파일을 구분해서 읽으면 된다.

## 다섯 asmdef 파일

**MaseiKivotos.Core.asmdef** — name/rootNamespace는 MaseiKivotos.Core다. noEngineReferences=true로 UnityEngine 의존을 막는다. BattleRoster/TurnBattle이 여기 들어간다.

**MaseiKivotos.Unity.asmdef** — Core와 UnityEngine.UI를 참조한다. Controller와 두 화면 및 공통 View가 들어간다.

**MaseiKivotos.Editor.asmdef** — Unity 어셈블리를 참조하고 includePlatforms는 Editor뿐이다. 두 씬 메뉴 도구가 들어간다.

**MaseiKivotos.EditModeTests.asmdef** — Core를 참조한다. Editor 한정이며 optionalUnityReferences의 TestAssemblies가 테스트 어셈블리임을 나타낸다.

**MaseiKivotos.PlayModeTests.asmdef** — Core, Unity, UnityEngine.UI와 TestAssemblies를 참조한다. Editor 한정 필드는 없지만 실제 씬 로드 코드는 UNITY_EDITOR 조건으로 감싸고 그 외에는 테스트를 Ignore한다.

name은 어셈블리 식별자, rootNamespace는 기본 네임스페이스 설정, references는 다른 어셈블리 의존 목록, includePlatforms는 포함 플랫폼, noEngineReferences는 Unity 엔진 참조 금지, optionalUnityReferences는 테스트 관련 참조 설정이다. 이 설정에는 직접 호출할 함수가 없다.

## 실행 씬과 메뉴

실제 리소스를 확인할 진입점은 Assets/Scenes/Stage/Battle/BattleType/Stage_Battle.unity다. 추가된 Makibo Battle 오브젝트에 공용 Controller와 StageBattleView가 연결된다. StageBattleView는 기존 InGame을 찾아 슬롯·버튼·COST 바를 재사용한다.

독립 확인용 Assets/MaseiKivotos/Scenes/TurnSandbox.unity에는 Controller가 있다. 연결된 View가 없으면 Awake에서 TurnSandboxView를 붙여 UI를 만든다.

기존 LoadingScene과 카브 메뉴를 거쳐 새 전투로 이동하는 게임 전체 흐름은 아직 연결하지 않았다. 새 턴 기능 확인은 위 두 씬을 직접 열어야 한다.

## meta와 GUID

.meta는 Unity가 에셋을 식별하는 GUID와 임포트 정보를 보관한다. 씬/프리팹은 파일 이름만으로 C#이나 Sprite를 찾는 것이 아니라 직렬화된 GUID 참조를 사용한다. 따라서 파일 이동 시 메타를 함께 보존한다.

Legacy/Carve의 .cs.meta는 과거 GUID의 기록이다. Assets 밖에 있어 현재 런타임 스크립트로 임포트되지 않는다. 기존 리소스의 Missing Script를 무조건 삭제하면 과거 참조를 추적할 단서도 사라질 수 있다.

## 검증 기록을 찾는 법

docs/reviews/2026-09-30-turn-foundation.md는 최초 턴 기반, 2026-10-01-stage-battle-resources.md는 기존 씬 연결, 2026-10-01-code-comments.md는 주석 정비를 설명한다. evidence에는 실행 결과 요약 JSON과 실제 씬 캡처 PNG가 있다.

TurnBattleTests는 로직, PlayMode 테스트는 실제 씬과 UI를 검사한다. 주석 정비의 코드 토큰 비교는 실행 코드가 같다는 증거이며, 새로 플레이한 결과가 아니다. 이 세 증거를 서로 대신하는 것으로 읽지 않는다.

## 개발용 임시 파일과 원본 구분

Tools 아래 두 PowerShell 파일은 재사용할 검증 도구다. Logs 아래 주석 비교·캡처·복사본 파일은 당시 검증 보조 자료이며 제품 런타임 코드가 아니다. Library/Temp/Obj와 csproj는 Unity가 생성한다.

이 해설의 전체 상세 대상은 마키보 C# 11개와 Tools의 PowerShell 2개다. 격리된 카브 원본 49개, 서드파티, 기존 독립 유틸리티, 자동 생성 프로젝트 파일의 모든 변수를 새로 해설하는 범위는 아니다. 새 턴 로직이 어디에 있는지를 혼동하지 않도록 구분했다.
