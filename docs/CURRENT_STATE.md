# 현재 구현 현황

## 2026-10-04 Missing Script 컴포넌트 정리

- 사용자 요청에 따라 Unity에서 231개 씬/프리팹을 전수 검사하고 32개 파일의 누락 컴포넌트 167개를 직접 제거했다. 상속 인스턴스도 함께 정리되어 재검사 결과는 0개다. 정상 계층/컴포넌트/위치/시각 참조와 모든 리소스 파일/meta를 보존했다.
- **검토 대기:** 사본 컴파일·PlayMode **11/11** 통과. 원본 적용 후 Assets 4,040개 해시 대조에서 의도한 32개 외 변경/삭제 없음. 구형 ScriptableObject는 유지하며, 콘솔의 출처 미확인 Unknown 경고 24건과 Odin 예외는 별도로 남아 있다. [제거 목록·백업·검증](reviews/2026-10-04-missing-script-cleanup.md).

## 2026-10-04 슬롯 바닥·HP UI

- `Stage_Battle`의 `SlotGroundSprite` 스프라이트를 UI Image로 연결하고, 기존 `UnitBase/UnitCanvas/HpBarBackground`와 `HpBar`를 런타임 HUD로 옮겨 현재/최대 HP와 게이지를 표시한다. 이동 미리보기·실제 교환·피해·퇴각·초기화를 따라가며 빈 슬롯에서는 HP를 숨긴다.
- 원본 Unit/UnitSlot 프리팹·meta·씬 해시 보존 확인. **검토 대기:** Unity 사본 컴파일·PlayMode **11/11** 통과. Core 변경 없이 UI만 수정해 EditMode는 이번에 재실행하지 않았다. [화면·검증·확인 순서](reviews/2026-10-04-unit-slot-hp-ui.md).

## 2026-10-04 SkillSlot 프리팹 표시

- 메인 스킬 선택 창은 기존 `Assets/Resources/prefab/UI/SkillSlot.prefab` 복제본을 사용한다. 이름·효과 설명·현재/최대 BP·사거리·범위·아이콘·타입·속성을 연결했다. 타입 3종/속성 11종은 사용자 지정 색 계열로 표시한다.
- `SkillSlotView`가 표시를 담당하며 사용자의 프리팹 원본과 GUID는 보존한다. 아이콘은 미정이며 `Resources/SkillIcons/<스킬 ID>`의 Sprite로 추후 연결한다. 변화 타입은 표시만 지원하고 전투 효과는 미구현이다.
- **검토 대기:** 사본 컴파일·EditMode **67/67**·PlayMode **10/10** 통과. 데이터 필드/색상/아이콘/네 장 배치와 실제 BP 갱신을 확인했다. [프리팹 연결 검토 기록](reviews/2026-10-04-skill-slot.md).

## 2026-10-04 이동 금지 중 재시도 변경

- D-009에 따라 이미 이동 불가/외부 이동 금지 상태에서 실패한 시도는 디버프를 새로 부여하거나 갱신하지 않는다. 서브 소비와 자기 종료의 1회 감소는 유지한다. 예: 잔여 1회 → 재시도 1회 유지 → 자기 종료 0회.
- **검토 대기:** 사본 컴파일, EditMode **67/67**, PlayMode **8/8** 통과. [변경 범위와 확인 순서](reviews/2026-10-04-movement-lock-retry.md).

## 2026-10-04 이동 교환 후 상태 재검증

- 교환만 당한 B는 이동 불가 0회를 유지하며 같은 턴/다음 턴에 자기 이동이 가능함을 재현했다. 사용자가 재확인 후 오해였다고 답변해 제보를 종결했다. 이 조사에서는 런타임 로직을 변경하지 않고 회귀 테스트 3개를 추가했다.
- 사본에서 컴파일·EditMode **65/65**, PlayMode **7/7** 통과. [조사 결과·재현 조건·화면 증거](reviews/2026-10-04-movement-passenger-check.md). 상태: **검토 완료(제보 확인 범위)**.

## 2026-10-02 현재 상태 — 서브 이동과 예약 미리보기

- 두 전투 화면의 **서브 이동** 창에서 아군·목적지를 선택한다. 4단계는 실제 좌표와 자원을 바꾸지 않는 예상 배치이며, 5단계 시작 시 예시 이미지를 없애고 실제 위치에서 한 칸씩 실행한다(D-008).
- 서브 1회 소비, 아군 순차 교환, 적 통과 거부/실행 시 부분 이동, 시도 종료의 이동 불가 2회와 자기 종료의 1회 감소를 연결했다. 이동 후 메인 스킬과 종료를 이어서 처리한다.
- `TurnBattle.Movement.cs`가 예약/실행/복사본 예측, `MovementPanel.cs`가 선택 창, `StageBattleView.cs`가 별도 예시 렌더러를 담당한다. 예제 이동 거리는 임시 3칸이다.
- **검토 대기:** 사본에서 컴파일, EditMode **63/63**, PlayMode **6/6** 통과. 사용자 씬/SceneTemplate 편집은 작업 시작 해시와 일치한다. [동작·화면 증거·확인 순서](reviews/2026-10-02-movement.md).
- 강제 이동·해제 스킬·걷기 애니메이션, 적 이동 AI, 배치/교체·아이템·서포트·EX·일반 상태 시스템은 남는다. 기존 Odin 초기화 예외/Missing Script 경고도 남으며 플레이어 빌드는 미실행이다.

## 2026-10-02 메인 스킬 1차 구현 (이력)

- `Stage_Battle`과 `TurnSandbox`에 공통 **메인 스킬** 예약 창을 추가했다. 아군·소유 스킬·적 대상을 선택하고, 예약 적용/수정/취소 후 단계 진행 또는 턴 넘기기로 실행한다.
- 일반 단일 태크/신비 공격에 HP, 스킬별 BP, 공유 COST 확보/소비, 사거리·대상 재검사, 명중 실패, 11속성표·복합 상성 제한·자속·최종 올림, 퇴각과 승패를 연결했다. 턴 넘기기는 스킬 예약을 유지한다.
- `MainSkill.cs`는 정의와 피해 계산, `TurnBattle.Skills.cs`는 예약/실행, `MainSkillPanel.cs`는 선택 UI를 담당한다. 함수·변수의 한국어 역할 주석을 포함한다.
- BP 0 대체의 Q-001/Q-002/C-008은 **D-007로 확정**되었다. 헤일로 이상은 사용자 제한 검사 후 자신 중심 범위 피해 → 최대 HP 55% 올림 소모/자체 BP 완충 → 승패 순서로 실행한다. 임시 중단 경계는 제거했다. [범위·재현·검증 기록](reviews/2026-10-02-main-skills.md).
- **검토 대기:** 헤일로 이상 연결 후 컴파일, EditMode **45/45**, 실제 씬 PlayMode **4/4** 통과. 화면과 소스/씬/프리팹 해시 일치를 확인했다. 기존 Odin 예외와 Missing Script 경고는 남으며 플레이어 빌드는 미실행이다.
- 공격 애니메이션, 적의 스킬 선택 AI, 변화/상태/일반 광역/다단/EX/아이템/이동/교체는 미구현이다. 스킬 수치와 편성은 임시 데이터이며 기존 리소스 수정·폐기는 없다.

## 2026-10-01 Stage_Battle 리소스 연결 (이력)

- 실행 기준은 [Stage_Battle.unity](../Assets/Scenes/Stage/Battle/BattleType/Stage_Battle.unity)다. `InGame`의 배경·9칸 슬롯·유닛 렌더러·턴 종료/SKIP 버튼·COST 바에 새 턴 표시를 연결했다.
- [StageBattleView](../Assets/MaseiKivotos/Unity/StageBattleView.cs)는 기존 화면 리소스를 Play 중 재배치하고, [TurnBattleView](../Assets/MaseiKivotos/Unity/TurnBattleView.cs)를 통해 검증 씬과 턴 컨트롤러/Core를 공유한다. 구형 미연결 UI는 실행 중 숨기며 프리팹 원본은 유지했다.
- 상태와 검증 증거, 사용법은 [리소스 연결 검토 기록](reviews/2026-10-01-stage-battle-resources.md)을 따른다. 임시 편성으로 턴 종료만 실행하는 범위이며 스킬·피해·애니메이션 이벤트는 후속 작업이다.
- **검토 대기:** 별도 복사본에서 컴파일, EditMode 12개, PlayMode 2개 통과. 수정 코드/씬과 검증 복사본의 해시 일치 및 실제 렌더링 화면을 확인했다. 기존 Missing Script 경고/Odin 예외는 남고 플레이어 빌드는 미실행이다.

## 2026-09-30 새 턴 기반 (이력)

- [TurnSandbox.unity](../Assets/MaseiKivotos/Scenes/TurnSandbox.unity)를 직접 열고 Play하면 턴 예약 화면에서 시작한다. **턴 넘기기**로 다음 턴에 도달하고 **다음 단계 / 차례**로 유닛별 차례를 확인한다.
- [MaseiKivotos.Core](../Assets/MaseiKivotos/Core/TurnBattle.cs)는 Unity 독립 5단계 턴·Speed 순서/동률 추첨·COST·턴 종료 예약을 처리한다. [Unity 연결부](../Assets/MaseiKivotos/Unity/TurnSandboxController.cs)와 [화면](../Assets/MaseiKivotos/Unity/TurnSandboxView.cs)은 별도 어셈블리다.
- 2세력 PvE 임시 유닛으로 시작 2명/대기 1명씩 배치한다. A(150) → X(120) → B(80) → Y(50) 순서. COST는 1턴 3, 2턴 5, 상한 10이다. 턴 도중 Speed 변경과 스킬·상태·교체는 아직 연결하지 않았다.
- 검증: Unity 컴파일, EditMode 12개, 실제 씬 PlayMode 1개 통과. UI raycast/클릭으로 TURN 01→02, COST 3→5, 중복 진행 방지/리셋을 확인하고 화면을 캡처했다. 플레이어 빌드는 미실행.
- 상태: **턴 기반 검토 대기**. [상세 범위·재현 절차](reviews/2026-09-30-turn-foundation.md). Odin/Sirenix 초기화 예외는 별도 문제로 남는다.

## 2026-09-30 카브 격리 직후 상태 (이력)

- 카운터 아카이브(카브)의 전투 및 강한 의존 스크립트 49개와 각 `.cs.meta`를 [Legacy/Carve/2026-09-30](../Legacy/Carve/2026-09-30/README.md)에 격리했다. 원본 코드와 GUID를 보존하며 Unity는 이 폴더를 임포트하지 않는다.
- 기존 AP/COST/스킬/유닛/배치 및 카브 데이터·저장·편성·스테이지 진행과 연결된 탐험 코드가 현재 실행 대상에서 빠졌다. 새 마키보 전투는 **구현 전**이다.
- 독립적인 연출/기반 코드 12개, 자동 생성 입력 바인딩, 서드파티 코드와 스크립트 외 리소스는 유지했다. 남긴 Assets 파일 3,978개의 SHA-256 보존 검사를 통과했다.
- 리소스 56개에 격리된 GUID 직접 참조 211건이 남는다. Missing Script와 구형 데이터 참조는 의도적으로 보존하며 기존 씬은 플레이 가능한 마키보 씬이 아니다.
- 상태: **격리 검토 대기**. 컴파일 결과와 dayoff53 확인 순서는 [검토 기록](reviews/2026-09-30-carve-quarantine.md)에 있다.
- Unity 스크립트 재컴파일은 통과했다. 단, 보존한 Odin/Sirenix 플러그인에서 에디터 초기화 예외 2건이 있어 에디터 전체 정상 동작은 확인되지 않았다. Play Mode·플레이어 빌드는 실행하지 않았다.
- 다음 작업은 별도 규칙 로직·Unity 연결부·새 검증 씬을 구성하는 M1이다. 카브 코드를 복원하여 새 전투와 혼용하지 않는다.


## 2026-09-29 격리 이전 카브 조사 (역사 기록)

아래는 격리 이전 코드의 정적 조사 결과다. 코드 링크는 보관소를 가리키며 현재 Assets에서 실행되는 기능을 뜻하지 않는다.
Unity 컴파일, Play Mode, 빌드 실행을 통과했다는 의미가 아니다. 코드가 바뀌면 해당 설명도 다시 확인한다.

목표와 역할은 [PROJECT.md](PROJECT.md), 새 전투 규칙은 [BATTLE_RULES.md](BATTLE_RULES.md)를 따른다.
아래의 기존 구현은 전환 작업의 출발점이며, 마세이 키보토스의 규칙을 정의하지 않는다.

## 저장소와 실행 환경

- 프로젝트 경로: `C:\Fork\CounterAchive`. 폴더명은 기존 저장소 식별자다.
- Unity 버전: `6000.5.10f1` — [ProjectVersion.txt](../ProjectSettings/ProjectVersion.txt).
- Unity 제품명은 아직 `CounterAchive`, 회사명은 `DefaultCompany`다 — [ProjectSettings.asset](../ProjectSettings/ProjectSettings.asset), 15–16행.
- 패키지 선언: Input System `1.20.0`, Test Framework `1.7.0`, UGUI `2.5.0`, Timeline `1.8.13` — [manifest.json](../Packages/manifest.json).
- 주요 자체 코드는 [Assets/Script](../Assets/Script), 데이터 에셋은 [Resources/ScriptableObject](../Assets/Resources/ScriptableObject), 공용 프리팹은 [Resources/prefab](../Assets/Resources/prefab)에 있다.
- `Assets/Plugins`, `Assets/AssetStore`, TextMesh Pro 예제는 자체 전투 로직과 구분해서 조사한다.

## 핵심 코드 지도

| 관심 영역 | 조사 출발점 | 확인한 역할 |
| --- | --- | --- |
| 전투 시작과 승리 | [BattleStageMaster.cs](../Legacy/Carve/2026-09-30/Assets/Script/Manager/StageManager/BattleStageMaster.cs) | 초기화, 유닛 배치, AP 초기화, 승리 조건 |
| AP·턴·Cost | [StageMaster_Turn.cs](../Legacy/Carve/2026-09-30/Assets/Script/Manager/StageManager/StageMaster_Turn.cs) | AP 누적, 행동 시작/종료, Cost 충전 |
| 스킬 선택과 실행 | [StageMaster_Skill.cs](../Legacy/Carve/2026-09-30/Assets/Script/Manager/StageManager/StageMaster_Skill.cs) | 사거리, 대상 범위, 명중 판정, 연출, 효과 호출 |
| 슬롯·전투 UI | [StageMaster_Slot.cs](../Legacy/Carve/2026-09-30/Assets/Script/Manager/StageManager/StageMaster_Slot.cs), [StageMaster_UI.cs](../Legacy/Carve/2026-09-30/Assets/Script/Manager/StageManager/StageMaster_UI.cs) | `BattleStageMaster`의 partial 구현 |
| 유닛 실행 상태 | [UnitBase.cs](../Legacy/Carve/2026-09-30/Assets/Script/InGame/PlayStageScene/Unit/UnitBase.cs) | HP/AP, 애니메이션, 피해, 사망 처리 |
| 유닛 원본·저장 상태 | [UnitData.cs](../Legacy/Carve/2026-09-30/Assets/Script/Data/ScriptableObject/UnitData.cs), [UnitStatus.cs](../Legacy/Carve/2026-09-30/Assets/Script/Data/UnitStatus.cs) | ScriptableObject 원본, 실행/저장용 상태 |
| 스킬 데이터·효과 | [SkillData.cs](../Legacy/Carve/2026-09-30/Assets/Script/Data/ScriptableObject/SkillData.cs), [SkillEffect.cs](../Legacy/Carve/2026-09-30/Assets/Script/InGame/PlayStageScene/Skill/SkillEffect.cs), [Damage.cs](../Legacy/Carve/2026-09-30/Assets/Script/InGame/PlayStageScene/Skill/Damage.cs) | 비용, 범위, 효과 목록, 공격력 배율 피해 |
| 스테이지 데이터 | [SceneKeyData.cs](../Legacy/Carve/2026-09-30/Assets/Script/Data/ScriptableObject/SceneKeyData.cs), [BattleStageUnitData.cs](../Legacy/Carve/2026-09-30/Assets/Script/Data/ScriptableObject/BattleStageUnitData.cs) | 씬 키, 단계/종류, 승리 조건, 배치 데이터 |
| 저장과 리소스 로드 | [DataManager.cs](../Legacy/Carve/2026-09-30/Assets/Script/Manager/DataManager.cs) | Resources 로드, JSON 저장/복원 |
| 씬 전환과 다음 단계 | [SceneChangeManager.cs](../Legacy/Carve/2026-09-30/Assets/Script/System/SceneChanger/SceneChangeManager.cs), [StageLoadButtonController.cs](../Legacy/Carve/2026-09-30/Assets/Script/UI/StageSelectUI/StageLoadButtonController.cs) | 로딩 씬 경유, 종류/레벨별 씬 선택 |

## 현재 전투 흐름

1. `BattleStageMaster.Start()`가 매니저를 연결하고 `InitGame()`을 호출한다(114–138행).
2. `PlaceUnitSlot()`이 아군을 앞쪽, 적을 뒤쪽 슬롯에 배치한다(154–172행).
3. `UnitSetGame()`은 배치 가능한 슬롯이 있으면 유닛 선택 상태를 거치고, 없으면 바로 `StartGame()`으로 진입한다(177–222행).
4. `ActionPointsInit()`은 `Stay` 상태를 설정하고, 코루틴이 AP를 누적한다(227–244행).
5. `StageMaster_Turn.ActionPointAccumulation()`은 `Stay`에서만 AP를 올린다(43–60행).
6. `ActionPointUpper()`는 `speed × 경과 시간`을 누적하고 `maxAp` 이상이면 `ExecuteTurn()`을 호출한다(67–95행).
7. `ExecuteTurn()`은 `UnitPlay`로 전환하고 해당 유닛의 스킬 UI를 설정한 뒤 누적 AP를 0으로 만든다(169–190행).
8. `SkillTypeSelect()` → `SkillTargetSelect()` → `SkillStart()`로 대상 선택과 명중 판정을 진행한다(`StageMaster_Skill`, 101–200행).
9. `SkillHit` 애니메이션 이벤트는 피격 연출을, `SkillEnd`는 효과 적용 후 턴 종료를 호출한다.
10. `TurnEnd()`가 턴 수를 증가시키고 `Stay`로 돌아가 승리 조건을 갱신한다(`StageMaster_Turn`, 196–210행).

애니메이션 이벤트 연결은 [AnimatorEventObserver.cs](../Legacy/Carve/2026-09-30/Assets/Script/InGame/PlayStageScene/Unit/AnimatorEventObserver.cs)의 15–32행을 확인한다.
이 파일의 클래스명은 `AnimationEventObserver`다. 피해 효과의 실제 적용은 `SkillEndPlay()`에 있다(`StageMaster_Skill`, 259–268행).
스킬 진행이 멈추면 상태 전환뿐 아니라 해당 Animator/AnimationClip의 `SkillHit`·`SkillEnd` 연결을 함께 확인해야 한다.

## 전환 시 구분할 기존 의미

- **AP:** 현재 코드는 시간에 따라 쌓이는 행동 게이지다. `UnitData.ap`는 실행 유닛의 최대 AP와 연결되는 기존 값이다. 새 규칙은 이 AP 누적을 사용하지 않고 Speed 순서로 차례를 결정한다.
- **Cost:** `BattleStageMaster`에 공용 `float cost` 하나가 있다. 팀 1 유닛마다 `(speed / 10) × 경과 시간`으로 충전하고 10을 상한으로 처리한다(`StageMaster_Turn`, 102–117행).
- **스킬 비용:** 대상 선택 시 비용을 검사하고, `SkillStart()`에서 `skillCost`를 차감한다(`StageMaster_Skill`, 118행·182행). 기본 행동/특수 행동별 규칙 적용 여부를 새 명세와 대조한다.
- **능력치:** `UnitData`에는 HP, AP, 공격력, 방어력, 속도, 명중, 회피와 태그·스킬 목록이 있다(84–92행). `UnitTag`는 성별/종족/연령 분류다.
- **스킬 구조:** `SkillData`에는 Cost, 명중, `skillRange`, `skillArea`, 효과 목록과 연출 설정이 있다(38–80행). 새 규칙에서 필요한 분류·속성은 별도로 설계한다.
- **효과 구현:** 자체 코드에서 확인된 `SkillEffect` 파생 클래스는 `Damage`다. 다른 효과가 완성되어 있다고 가정하지 않는다.
- **승리 조건:** 적 전멸, 특정 적 처치, 지정 턴 생존이 정의되어 있다(`SceneKeyData`, 27–32행). `BattleStageMaster.UpdateStageClearCondition()`이 처리한다(249–296행).

확정 규칙은 [BATTLE_RULES.md](BATTLE_RULES.md), 미정 사항과 판단 기록은 [DECISIONS.md](DECISIONS.md)를 확인한다.

## 데이터와 저장 경계

`DataManager.DataInit()`은 `Resources/ScriptableObject/UnitData`, `SkillData`, `SceneKeyData`를 로드한다(92–101행).
`UnitStatus`는 능력치와 `skillNumberList`를 저장하고 `UnitData`/`UnitBase`에서 상태를 복사한다(14–26행·46–97행).
`SaveData`는 유닛 상태, 이전/현재 스테이지 번호, 스테이지 큐 목록 필드를 가진다(`DataManager`, 14–35행).
저장 위치는 `Application.persistentDataPath/saveData.json`이며 `JsonUtility`로 저장/복원한다(98행·119–182행).
제품명·회사명을 바꾸거나 저장 데이터 구조를 바꿀 때는 기존 세이브 경로와 복원 방식을 먼저 확인한다.
현재 소스에 저장 버전/명시적 마이그레이션 구조는 보이지 않는다. 실제 저장 파일의 왕복 복원은 검증하지 않았다.

## 씬과 정적 조사에서 발견한 주의점

[EditorBuildSettings.asset](../ProjectSettings/EditorBuildSettings.asset)의 첫 활성 씬은 [LoadingScene](../Assets/Scenes/LoadingScene.unity)이다(8–10행).
그 뒤 `Menu_Title`, `Menu_GroupSelect`, `Stage_DD_Battle`, `Helmet_101`, `Helmet_102`, `Helmet_103`이 등록되어 있다.
이는 빌드 등록 순서이며, 자동으로 이 순서대로 진행됨을 뜻하지 않는다.

- `Helmet_102`와 `Helmet_103`의 등록 경로(24·27행)는 `Assets/Scenes/Stage/Battle/Helmet/...`이며 파일이 없다.
- 당시 경로는 `DDsLike/Helmet/Level_1`이었다. 현재 위치는 [Helmet/Level_1](../Assets/Scenes/Stage/Battle/BattleType/Helmet/Level_1)이며, 빌드 전 경로와 GUID 해석을 Unity에서 확인한다.
- `Stage_DD_Battle_2.0.unity`는 조사 시작 시 추적되지 않은 사용자 파일이며 이 BuildSettings 목록에 없다. 새 기준 씬으로 확정하지 않았다.
- `SceneChangeManager.SceneLoad()`는 `LoadingScene`을 거쳐 `SceneKeyData.sceneName`을 로드한다(24–33행·61–73행).
- `LoadStageData(BattleStageUnitData)`는 정의되어 있으나 `Assets/Script`의 C# 호출부 검색에서는 다른 호출이 발견되지 않았다. 씬·Inspector 이벤트 및 초기화 순서 확인이 필요하다.
- `UnitBase.ComputeDamage()`는 방어/관통 계산 뒤 327행에서 `computedDamage = Mathf.Max(damage)`로 덮어쓴다. 방어 계산을 적용했다고 간주하면 안 된다.
- `ExecuteTurn()`에서 팀별 AI 분기가 보이지 않으며 자체 코드 검색에서도 적 행동 선택 구현을 확인하지 못했다. 실제 적 턴 동작을 검증해야 한다.
- 승리 처리와 별개의 아군 전멸/패배 흐름은 조사한 전투 코드에서 확인되지 않았다.

## 검증 상태와 다음 조사

| 항목 | 현재 확인 수준 |
| --- | --- |
| 핵심 코드·패키지·빌드 등록 | 파일 정적 확인 |
| 자체 테스트 | `Assets/Script`에 NUnit/UnityTest 테스트와 자체 asmdef가 발견되지 않음. `Assets/Tests`, `.github` 디렉터리 없음 |
| Unity 컴파일 | 이번 환경 구성 작업에서 실행하지 않음 |
| Play Mode / 전투 완료 / 저장 복원 | 미검증 |
| 실행 파일 빌드 | 미검증. 씬 경로 불일치 우선 확인 필요 |

후속 작업은 [DEVELOPMENT.md](DEVELOPMENT.md)의 절차와 [ROADMAP.md](ROADMAP.md)를 따른다.
첫 실행 검증에서는 부트스트랩 씬, 매니저 초기화 순서, 스테이지 데이터 연결, 한 번의 스킬 완료, 적 턴, 승리/패배, 다음 씬 진입을 확인한다.
전투 규칙 계산은 연출·Inspector 연결과 구분하여 검증하고, 런타임 확인 전에는 정적 분석 결과로 표시한다.

## 환경 구성 시작 시 기존 사용자 변경

아래 파일은 이번 문서 구성 전부터 변경 또는 미추적 상태였으며 이번 구성 작업에서 보존한다.

- 수정: [DDBattleInGame.prefab](../Assets/Resources/prefab/DDBattleInGame.prefab)
- 수정: [SkillSlot.prefab](../Assets/Resources/prefab/UI/SkillSlot.prefab)
- 수정: [LoadingScene.unity](../Assets/Scenes/LoadingScene.unity)
- 당시 미추적: `Stage_DD_Battle_2.0.unity`와 해당 `.meta`. 이 항목은 조사 당시 기록이며 현재 실행 씬은 상단의 `Stage_Battle`이다.

이 목록은 2026-09-29 작업 시작 시점 기록이다. 이후 작업자는 `git status`로 현재 변경을 다시 확인한다.
