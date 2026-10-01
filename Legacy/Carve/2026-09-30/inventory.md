# 카브 스크립트 격리 목록

2026-09-30. 원래 파일과 .cs.meta를 함께 보존했다. GUID와 SHA-256은 [manifest.json](manifest.json)에 있다.

| 원래 경로 / 보관 코드 | 격리 이유 |
| --- | --- |
| [Assets/Scenes/Stage/0. Scene Template/SceneTemplatePipeline_Stage_Battle.cs](<Assets/Scenes/Stage/0. Scene Template/SceneTemplatePipeline_Stage_Battle.cs>) | 카브 전투 씬 템플릿의 빈 훅 또는 빈 스크립트. 새 구현의 실행 경로로 사용하지 않음. |
| [Assets/Scenes/Stage/Battle/BattleType/DDsLike/SceneTemplate/SceneTemplatePipeline_Stage_Battle.cs](<Assets/Scenes/Stage/Battle/BattleType/DDsLike/SceneTemplate/SceneTemplatePipeline_Stage_Battle.cs>) | 카브 전투 씬 템플릿의 빈 훅 또는 빈 스크립트. 새 구현의 실행 경로로 사용하지 않음. |
| [Assets/Script/Data/ScriptableObject/BattleStageUnitData.cs](<Assets/Script/Data/ScriptableObject/BattleStageUnitData.cs>) | 카브 능력치·전투 상태·스킬·편성·스테이지 데이터 구조. 새 데이터와 중복되지 않도록 격리. |
| [Assets/Script/Data/ScriptableObject/ColorState.cs](<Assets/Script/Data/ScriptableObject/ColorState.cs>) | 카브 능력치·전투 상태·스킬·편성·스테이지 데이터 구조. 새 데이터와 중복되지 않도록 격리. |
| [Assets/Script/Data/ScriptableObject/GroupUnitsData.cs](<Assets/Script/Data/ScriptableObject/GroupUnitsData.cs>) | 카브 능력치·전투 상태·스킬·편성·스테이지 데이터 구조. 새 데이터와 중복되지 않도록 격리. |
| [Assets/Script/Data/ScriptableObject/SceneKeyData.cs](<Assets/Script/Data/ScriptableObject/SceneKeyData.cs>) | 카브 능력치·전투 상태·스킬·편성·스테이지 데이터 구조. 새 데이터와 중복되지 않도록 격리. |
| [Assets/Script/Data/ScriptableObject/SkillData.cs](<Assets/Script/Data/ScriptableObject/SkillData.cs>) | 카브 능력치·전투 상태·스킬·편성·스테이지 데이터 구조. 새 데이터와 중복되지 않도록 격리. |
| [Assets/Script/Data/ScriptableObject/UnitData.cs](<Assets/Script/Data/ScriptableObject/UnitData.cs>) | 카브 능력치·전투 상태·스킬·편성·스테이지 데이터 구조. 새 데이터와 중복되지 않도록 격리. |
| [Assets/Script/Data/UnitStatus.cs](<Assets/Script/Data/UnitStatus.cs>) | 카브 능력치·전투 상태·스킬·편성·스테이지 데이터 구조. 새 데이터와 중복되지 않도록 격리. |
| [Assets/Script/InGame/ExploreStageScene/ExploreStageMaster.cs](<Assets/Script/InGame/ExploreStageScene/ExploreStageMaster.cs>) | DataManager 또는 ExploreStageMaster 의존성으로 함께 격리하는 탐험 진행·입력·상호작용 코드. |
| [Assets/Script/InGame/ExploreStageScene/InteractObjectController.cs](<Assets/Script/InGame/ExploreStageScene/InteractObjectController.cs>) | DataManager 또는 ExploreStageMaster 의존성으로 함께 격리하는 탐험 진행·입력·상호작용 코드. |
| [Assets/Script/InGame/ExploreStageScene/PlayerController.cs](<Assets/Script/InGame/ExploreStageScene/PlayerController.cs>) | DataManager 또는 ExploreStageMaster 의존성으로 함께 격리하는 탐험 진행·입력·상호작용 코드. |
| [Assets/Script/InGame/GroupSelectScene/FirstPartySelectSlotController.cs](<Assets/Script/InGame/GroupSelectScene/FirstPartySelectSlotController.cs>) | 구형 편성 데이터 의존 또는 미완성 편성 코드. 새 편성 연결에서 재작성. |
| [Assets/Script/InGame/GroupSelectScene/GroupSelectManager.cs](<Assets/Script/InGame/GroupSelectScene/GroupSelectManager.cs>) | 구형 편성 데이터 의존 또는 미완성 편성 코드. 새 편성 연결에서 재작성. |
| [Assets/Script/InGame/PlayStageScene/Skill/Damage.cs](<Assets/Script/InGame/PlayStageScene/Skill/Damage.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/InGame/PlayStageScene/Skill/RandomCircleGenerator.cs](<Assets/Script/InGame/PlayStageScene/Skill/RandomCircleGenerator.cs>) | 외부 호출 가능한 기능이 없는 미사용 전투 보조 코드. |
| [Assets/Script/InGame/PlayStageScene/Skill/SkillEffect.cs](<Assets/Script/InGame/PlayStageScene/Skill/SkillEffect.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/InGame/PlayStageScene/UI/SlotGroundSpriteController.cs](<Assets/Script/InGame/PlayStageScene/UI/SlotGroundSpriteController.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/InGame/PlayStageScene/UI/UnitCard.cs](<Assets/Script/InGame/PlayStageScene/UI/UnitCard.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/InGame/PlayStageScene/UI/UnitSetUIController.cs](<Assets/Script/InGame/PlayStageScene/UI/UnitSetUIController.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/InGame/PlayStageScene/UI/WinUIController.cs](<Assets/Script/InGame/PlayStageScene/UI/WinUIController.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/InGame/PlayStageScene/Unit/AnimatorEventObserver.cs](<Assets/Script/InGame/PlayStageScene/Unit/AnimatorEventObserver.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/InGame/PlayStageScene/Unit/UniSelectSlot/UnitSelectController.cs](<Assets/Script/InGame/PlayStageScene/Unit/UniSelectSlot/UnitSelectController.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/InGame/PlayStageScene/Unit/UniSelectSlot/UnitSelectSlotGroupController.cs](<Assets/Script/InGame/PlayStageScene/Unit/UniSelectSlot/UnitSelectSlotGroupController.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/InGame/PlayStageScene/Unit/UnitBase.cs](<Assets/Script/InGame/PlayStageScene/Unit/UnitBase.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/InGame/PlayStageScene/Unit/UnitCharacterController.cs](<Assets/Script/InGame/PlayStageScene/Unit/UnitCharacterController.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/InGame/PlayStageScene/Unit/UnitFieldController.cs](<Assets/Script/InGame/PlayStageScene/Unit/UnitFieldController.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/InGame/PlayStageScene/Unit/UnitSlotController.cs](<Assets/Script/InGame/PlayStageScene/Unit/UnitSlotController.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/InGame/PlayStageScene/Unit/UnitSlotGroupController.cs](<Assets/Script/InGame/PlayStageScene/Unit/UnitSlotGroupController.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/InGame/PlayStageScene/UnitProduction.cs](<Assets/Script/InGame/PlayStageScene/UnitProduction.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/Manager/DataManager.cs](<Assets/Script/Manager/DataManager.cs>) | BattleStageMaster 및 카브 유닛·저장 데이터에 직접 의존하는 초기화/저장 연결. |
| [Assets/Script/Manager/StageManager/BattleStageMaster.cs](<Assets/Script/Manager/StageManager/BattleStageMaster.cs>) | 카브 전투 진행·AP·COST·즉시 스킬 선택·슬롯 조작·전투 UI 제어를 폐기하고 새 규칙으로 작성. |
| [Assets/Script/Manager/StageManager/StageMaster_Skill.cs](<Assets/Script/Manager/StageManager/StageMaster_Skill.cs>) | 카브 전투 진행·AP·COST·즉시 스킬 선택·슬롯 조작·전투 UI 제어를 폐기하고 새 규칙으로 작성. |
| [Assets/Script/Manager/StageManager/StageMaster_Slot.cs](<Assets/Script/Manager/StageManager/StageMaster_Slot.cs>) | 카브 전투 진행·AP·COST·즉시 스킬 선택·슬롯 조작·전투 UI 제어를 폐기하고 새 규칙으로 작성. |
| [Assets/Script/Manager/StageManager/StageMaster_Turn.cs](<Assets/Script/Manager/StageManager/StageMaster_Turn.cs>) | 카브 전투 진행·AP·COST·즉시 스킬 선택·슬롯 조작·전투 UI 제어를 폐기하고 새 규칙으로 작성. |
| [Assets/Script/Manager/StageManager/StageMaster_UI.cs](<Assets/Script/Manager/StageManager/StageMaster_UI.cs>) | 카브 전투 진행·AP·COST·즉시 스킬 선택·슬롯 조작·전투 UI 제어를 폐기하고 새 규칙으로 작성. |
| [Assets/Script/SceneStarter.cs](<Assets/Script/SceneStarter.cs>) | BattleStageMaster 및 카브 유닛·저장 데이터에 직접 의존하는 초기화/저장 연결. |
| [Assets/Script/System/Custom Editor/SerializableDictionary.cs](<Assets/Script/System/Custom Editor/SerializableDictionary.cs>) | 카브 전투의 AP/슬롯 사전용 컨테이너. 남는 자체 코드에 사용처 없음. |
| [Assets/Script/System/SceneChanger/SceneChangeController.cs](<Assets/Script/System/SceneChanger/SceneChangeController.cs>) | SceneKeyData·DataManager·구형 전투 상태에 연결된 로딩/스테이지 흐름. |
| [Assets/Script/System/SceneChanger/SceneChangeManager.cs](<Assets/Script/System/SceneChanger/SceneChangeManager.cs>) | SceneKeyData·DataManager·구형 전투 상태에 연결된 로딩/스테이지 흐름. |
| [Assets/Script/UI/OptionController.cs](<Assets/Script/UI/OptionController.cs>) | 카브 DataManager.SaveGame을 호출하는 옵션/저장 결합 코드. |
| [Assets/Script/UI/PlayUIController.cs](<Assets/Script/UI/PlayUIController.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/UI/SkillRangeUIController.cs](<Assets/Script/UI/SkillRangeUIController.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/UI/SkillSlotUIController.cs](<Assets/Script/UI/SkillSlotUIController.cs>) | 카브 유닛·피해·행동·배치·결과 UI 또는 그 직접 의존 코드. 마키보 전투에서 재작성. |
| [Assets/Script/UI/StageSelectUI/StageLoadButtonController.cs](<Assets/Script/UI/StageSelectUI/StageLoadButtonController.cs>) | SceneKeyData·DataManager·구형 전투 상태에 연결된 로딩/스테이지 흐름. |
| [Assets/Script/UI/StageSelectUI/StageLoadButtonListController.cs](<Assets/Script/UI/StageSelectUI/StageLoadButtonListController.cs>) | SceneKeyData·DataManager·구형 전투 상태에 연결된 로딩/스테이지 흐름. |
| [Assets/Script/UI/StageSelectUI/StageMenuController.cs](<Assets/Script/UI/StageSelectUI/StageMenuController.cs>) | SceneKeyData·DataManager·구형 전투 상태에 연결된 로딩/스테이지 흐름. |
| [Assets/Script/UI/StageSelectUI/StageSelectController.cs](<Assets/Script/UI/StageSelectUI/StageSelectController.cs>) | SceneKeyData·DataManager·구형 전투 상태에 연결된 로딩/스테이지 흐름. |
| [Assets/StageSelectController.cs](<Assets/StageSelectController.cs>) | 카브 전투 씬 템플릿의 빈 훅 또는 빈 스크립트. 새 구현의 실행 경로로 사용하지 않음. |

## 유지한 자체 코드

- [Assets/Script/Manager/CameraManager.cs](<../../../Assets/Script/Manager/CameraManager.cs>) — 전투 규칙과 독립적인 연출/기반 기능.
- [Assets/Script/Manager/PoolManager.cs](<../../../Assets/Script/Manager/PoolManager.cs>) — 전투 규칙과 독립적인 연출/기반 기능.
- [Assets/Script/Production/FadeController.cs](<../../../Assets/Script/Production/FadeController.cs>) — 전투 규칙과 독립적인 연출/기반 기능.
- [Assets/Script/Production/FadeManager.cs](<../../../Assets/Script/Production/FadeManager.cs>) — 전투 규칙과 독립적인 연출/기반 기능.
- [Assets/Script/Production/PlayerCameraController.cs](<../../../Assets/Script/Production/PlayerCameraController.cs>) — 전투 규칙과 독립적인 연출/기반 기능.
- [Assets/Script/System/Singleton.cs](<../../../Assets/Script/System/Singleton.cs>) — 전투 규칙과 독립적인 연출/기반 기능.
- [Assets/Script/System/CoroutineManager.cs](<../../../Assets/Script/System/CoroutineManager.cs>) — 전투 규칙과 독립적인 연출/기반 기능.
- [Assets/Script/UI/KeyBoardIcon.cs](<../../../Assets/Script/UI/KeyBoardIcon.cs>) — 전투 규칙과 독립적인 연출/기반 기능.
- [Assets/Script/InGame/ExploreStageScene/OldLibraryController.cs](<../../../Assets/Script/InGame/ExploreStageScene/OldLibraryController.cs>) — 전투 규칙과 독립적인 연출/기반 기능.
- [Assets/Script/InGame/ExploreStageScene/RainGroundEffectController.cs](<../../../Assets/Script/InGame/ExploreStageScene/RainGroundEffectController.cs>) — 전투 규칙과 독립적인 연출/기반 기능.
- [Assets/Script/InGame/PlayStageScene/EffectController.cs](<../../../Assets/Script/InGame/PlayStageScene/EffectController.cs>) — 전투 규칙과 독립적인 연출/기반 기능.
- [Assets/Script/InGame/PlayStageScene/Skill/SkillProductionController.cs](<../../../Assets/Script/InGame/PlayStageScene/Skill/SkillProductionController.cs>) — 전투 규칙과 독립적인 연출/기반 기능.

- [Assets/PlayerInput.cs](../../../Assets/PlayerInput.cs) — 보존된 입력 액션의 자동 생성 바인딩. 현재 새 전투에 연결한 것은 아님.
