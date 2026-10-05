# 개발 환경과 검토 절차

기준일: 2026-10-04. 설치 유무와 실행 성공을 구분한다.

2026-10-04 Missing Script 정리 후 사본 컴파일·PlayMode **11개** 통과. 231개 씬/프리팹의 GameObject 누락 컴포넌트는 재검사에서 **0개**다. 콘솔의 Unknown 경고 24건 및 기존 Odin 초기화 예외가 남아 있으므로 경고 없는 실행을 의미하지 않는다. [대상·보존·백업·검증](reviews/2026-10-04-missing-script-cleanup.md).

2026-10-04 슬롯 바닥/HP UI를 검증했다. 사본 컴파일·PlayMode **11개** 통과. 피해 수치/게이지와 이동 미리보기·실제 교환·퇴각·초기화를 확인했다. Core 수정이 없어 이번에는 EditMode를 재실행하지 않았다. [화면과 재현 절차](reviews/2026-10-04-unit-slot-hp-ui.md).

2026-10-04 SkillSlot 프리팹 표시를 검증했다. 사본 컴파일·EditMode **67개**·PlayMode **10개** 통과. 타입/속성 색과 데이터, 이미지 지정/해제, 선택과 실행 후 BP 갱신을 포함한다. [최신 화면과 검증 기록](reviews/2026-10-04-skill-slot.md).

2026-10-02 서브 이동과 4단계 미리보기를 사본에서 검증했다. 컴파일·EditMode **63개**·PlayMode **6개** 통과. 실제 Stage_Battle의 이동 선택, 예시 제거, 한 칸 실행, 스킬 연결, 취소와 초기화를 포함한다. [최신 이동 검증과 재현 순서](reviews/2026-10-02-movement.md).

2026-10-02 일반 단일 공격과 D-007 헤일로 이상을 별도 사본에서 통합 검증했다. 컴파일·EditMode **45개**·PlayMode **4개** 통과. Stage_Battle의 스킬 선택/예약/피해/자원/초기화와 두 씬의 턴 진행을 포함한다. 변화/다단/일반 광역까지 포함한 전체 스킬 기능 완료를 뜻하지 않는다. [최신 결과·기능 범위·확인 순서](reviews/2026-10-02-main-skills.md).

2026-10-01 Stage_Battle 리소스 연결을 별도 프로젝트 복사본에서 검증했다. 컴파일·EditMode 12개·PlayMode 2개 통과. 원본 에디터는 열어 둔 채 파일/씬 해시 일치를 확인했으며, 직접 조작한 원본 에디터 검증과 구분한다. [리소스 연결 검토 기록](reviews/2026-10-01-stage-battle-resources.md).

2026-09-30 새 마키보 턴 기반의 Unity 컴파일·EditMode 12개·PlayMode 1개를 검증했다. 실제 검증 씬의 버튼과 턴 전환을 확인했다. 단, Sirenix/Odin의 에디터 초기화 예외는 별도 문제이며 플레이어 빌드는 미실행이다. [턴 기반 검토 기록](reviews/2026-09-30-turn-foundation.md), [이전 격리 기록](reviews/2026-09-30-carve-quarantine.md).

## 확인한 환경

| 항목 | 기준/확인 결과 |
| --- | --- |
| 저장소 | `C:\Fork\CounterAchive` · Windows / PowerShell |
| Unity | [ProjectVersion.txt](../ProjectSettings/ProjectVersion.txt): `6000.5.10f1` |
| 로컬 에디터 | `C:\Program Files\Unity\Hub\Editor\6000.5.10f1\Editor\Unity.exe` 존재 확인 |
| Unity Hub | `C:\Program Files\Unity Hub\Unity Hub.exe` 존재 확인 |
| 의존성 | [manifest.json](../Packages/manifest.json), [packages-lock.json](../Packages/packages-lock.json) |
| 주요 패키지 | Input System `1.20.0`, Unity Test Framework `1.7.0`, uGUI `2.5.0`, Timeline `1.8.13` (manifest 기준) |
| 입력 설정 | [ProjectSettings.asset](../ProjectSettings/ProjectSettings.asset)의 `activeInputHandler: 2` |
| 기존 제품명 | 같은 설정 파일의 `productName: CounterAchive` |
| 최신 실행 검증 | 슬롯/HP UI 포함 컴파일·PlayMode 11개 통과. EditMode 최근 실행은 SkillSlot 작업의 67개 통과이며 이번 UI 작업에서는 재실행하지 않음. 기존 에디터 초기화 예외 있음. 플레이어 빌드 미실행 |

다른 PC에서는 실제 에디터 경로를 확인한다. 의존성 목록은 패키지 복원 성공 증거가 아니며, 최초 Unity 실행 시 복원/임포트 결과를 확인해야 한다. 이번 작업에서는 에디터/패키지 설치나 전역 Codex 설정 변경이 필요하지 않았다.

## 프로젝트 열기

새 전투 진입점은 `Assets/Scenes/Stage/Battle/BattleType/Stage_Battle.unity` 또는 **마키보 → Stage_Battle 열기** 메뉴다. 기존 InGame 리소스를 사용하는 새 턴 화면이다. 별도의 `Assets/MaseiKivotos/Scenes/TurnSandbox.unity` / **마키보 → 턴 검증 씬 열기**도 유지한다. 카브의 LoadingScene/메뉴를 거치는 기존 진행 흐름은 연결하지 않았다. [Stage_Battle 검증 범위](reviews/2026-10-01-stage-battle-resources.md).

1. Unity Hub에 저장소 폴더를 등록하고 프로젝트 지정 버전으로 연다.
2. 패키지 복원과 임포트 완료 후 Console의 컴파일 오류부터 확인한다.
3. 실행할 씬과 필요한 초기화 진입점을 [현재 구현 현황](CURRENT_STATE.md)에서 찾는다. 씬을 직접 여는 경로와 메뉴/로딩을 통한 진입 경로를 구분한다.
4. 씬·프리팹·애니메이션 이벤트 연결이 필요한 기능은 Inspector와 Play Mode에서 확인한다.

에디터가 열려 있는 동일 프로젝트에 별도 배치 Unity를 동시에 실행하지 않는다. 자동 생성 `.csproj`/`.slnx`의 빌드만으로 씬 연결이나 게임 동작 검증을 대신하지 않는다.

## 기능 하나를 진행하는 순서

1. `git status --short`와 대상 diff로 작업 전 상태를 확인한다.
2. 요청에 필요한 규칙 원문 절과 관련 미정 항목을 찾고, 성공/실패/경계 사례를 정한다.
3. 기존 코드의 재사용 범위와 변경 범위를 판단하여 구현한다. 큰 전환은 동작을 검토할 수 있는 단위로 나눈다.
4. 변경에 맞는 검증을 수행하고, 수행하지 못한 검증은 이유와 재현 절차를 남긴다.
5. dayoff53에게 동작 변경, 검사 결과, 확인할 씬/조작/기대 결과를 제출한다.
6. 관련 [진행 상태](ROADMAP.md)와 결정/현황 문서를 갱신하고 검토 의견을 후속 수정에 반영한다.

## 검증 선택

### 코드 주석

마키보 스크립트에는 카브에서 사용하던 방식처럼 한국어 역할 설명을 작성한다. 클래스·함수·필드·프로퍼티·enum 항목에는 `/// <summary>`, 함수 매개변수에는 `/// <param>`을 사용한다. 내부 변수에는 용도를 설명하는 `//` 주석을 작성하고, 턴 경계·예약 잠금·비공개 정보·임시 데이터 등 구현 의도도 함께 남긴다. 구현하지 않은 기능을 지원하는 것처럼 설명하지 않으며, 동작을 수정할 때 관련 주석도 갱신한다.

2026-10-01 현재 마키보 C# 파일 11개에 적용했다. [주석 정비와 정적 검증 기록](reviews/2026-10-01-code-comments.md).

### 변경별 검증 범위

| 변경 | 적절한 검증 |
| --- | --- |
| 문서만 변경 | 링크/경로, 규칙 원문과의 일치, diff/누락 확인 |
| 피해·정렬·비용·상태 계산 | 재현 가능한 EditMode 등 규칙 테스트. 경계값/실패/동률을 포함하고 난수는 재현 가능하게 설계 |
| 전투 상태 전환 | 예약부터 실행·퇴각·종료까지 순서와 상태를 검증 |
| UI/씬/프리팹/애니메이션 | Unity 컴파일 및 대상 씬 Play Mode, Inspector 참조, 애니메이션 종료 이벤트 확인 |
| 저장/이름 변경 | 기존 데이터 로드, 직렬화 참조, 저장 경로·키 이행 확인 |
| 배포 관련 | 대상 플랫폼 빌드와 생성된 실행물 확인 |

현재 테스트는 `Assets/MaseiKivotos/Tests`에 있다. Unity를 닫고 `& ./Tools/Test-MakiboTurns.ps1`로 EditMode/PlayMode를 순서대로 실행한다. `-Platform EditMode` 또는 `-Platform PlayMode`로 나눌 수 있다. 아래는 일반적인 개별 Unity 테스트 호출 예시다.

```powershell
# PowerShell, 저장소 루트에서 실행. Unity 에디터가 이 프로젝트를 열고 있지 않은 상태.
$unityExe = 'C:\Program Files\Unity\Hub\Editor\6000.5.10f1\Editor\Unity.exe'
$projectPath = (Get-Location).Path
$testLogDir = Join-Path $projectPath 'Logs'
New-Item -ItemType Directory -Path $testLogDir -Force | Out-Null
& $unityExe -batchmode -projectPath $projectPath -runTests -testPlatform EditMode -testResults (Join-Path $testLogDir 'editmode-results.xml') -logFile (Join-Path $testLogDir 'editmode.log')
```

프로세스 종료 후 로그와 결과 XML에서 실패뿐 아니라 실제 실행한 테스트 수를 확인한다. 테스트가 0개면 규칙 검증 완료로 표시하지 않는다. PlayMode 검증이 필요하면 해당 테스트/씬을 별도로 실행한다.

## dayoff53에게 제출할 검토 기록

큰 기능은 `docs/reviews/YYYY-MM-DD-작업명.md`에 다음 내용을 남긴다. 작은 수정은 최종 응답과 관련 상태 갱신으로 충분하다.

```markdown
# 작업명
날짜 / 상태: 검토 대기 / 검토자: dayoff53

## 목적과 관련 규칙
- 사용자 요청, 원문 절, 적용한 확정 결정 ID

## 변경 결과
- 이전 동작 → 변경 동작, 주요 파일

## 검증
- 실제 실행한 검사와 결과
- 실행하지 못한 항목과 이유

## dayoff53 확인 순서
1. 열 씬 또는 진입 경로
2. 입력/조작
3. 기대 결과와 경계 사례

## 남은 사항
- 미정 결정 ID, 제약, 다음 작업

## 검토 결과
- dayoff53의 실제 피드백을 받은 뒤 기록
```

## Codex가 문서를 사용하는 방식

루트 [AGENTS.md](../AGENTS.md)에 프로젝트 지침과 목적별 문서 경로를 둔다. Codex는 작업 시작 시 프로젝트 경로의 `AGENTS.md`를 발견해 지침으로 읽는다. 이 구성은 [OpenAI 공식 AGENTS.md 안내](https://learn.chatgpt.com/docs/agent-configuration/agents-md)에 따른다. 규칙 전문은 필요할 때 관련 절을 읽도록 분리했다.

새 세션에서는 이 저장소를 작업 폴더로 열고 “이 프로젝트의 목표, 규칙 기준, 현재 상태를 요약해줘”로 적용을 확인할 수 있다. 이번 세션에서는 파일을 직접 구성·확인했으며, 별도 새 세션의 자동 로딩 시험은 하지 않았다. 채팅 전체의 자동 저장이나 다른 프로젝트로의 전역 적용을 의미하지 않는다.
