# 마세이 키보토스

**마세이 키보토스(약칭: 마키보)**는 기존 `CounterAchive` Unity 프로젝트를 새로운 전투 규칙에 맞게 전환하여 완성하는 게임 프로젝트입니다.

Codex가 기능 설계·구현·검증을 주도하고, **dayoff53**이 결과를 검토합니다. 기존 카운터 아카이브는 **카브**라고 부릅니다. 카브 코드는 격리했으며, 새 마키보의 **5단계 턴 진행·턴 넘기기 검증 씬**을 구현했습니다. 스킬을 포함한 전체 전투는 개발 중입니다.

기존 씬·프리팹·데이터 에셋은 보존했지만 격리된 스크립트 참조는 Missing Script로 남습니다. 카브의 기존 플레이 흐름은 현재 동작하지 않습니다. [격리 결과와 검토 순서](docs/reviews/2026-09-30-carve-quarantine.md), [복구용 코드와 목록](Legacy/Carve/2026-09-30/README.md)을 참고하세요.

## 시작하기

Unity Hub에서 이 저장소를 열고 [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt)의 에디터 버전을 사용합니다. 현재 기준은 **Unity 6000.5.10f1**입니다. 설치·검증 범위와 작업 절차는 [개발 절차](docs/DEVELOPMENT.md)를 참고합니다.

**새 턴 기능 실행:** [Stage_Battle.unity](Assets/Scenes/Stage/Battle/BattleType/Stage_Battle.unity)를 열고 Play → **턴 넘기기**를 누릅니다. 기존 `InGame`의 배경·9칸 슬롯·버튼·COST 바를 사용하며 `TURN 01 / COST 3`에서 `TURN 02 / COST 5`로 바뀝니다. `마키보 → Stage_Battle 열기` 메뉴도 사용할 수 있습니다. [리소스 연결·검증·확인 순서](docs/reviews/2026-10-01-stage-battle-resources.md).

독립적인 [TurnSandbox.unity](Assets/MaseiKivotos/Scenes/TurnSandbox.unity)도 같은 턴 로직을 사용합니다. [턴 기반 기능 범위](docs/reviews/2026-09-30-turn-foundation.md).

**일반 스킬 실행:** Play → **메인 스킬 → 아군 → 스킬 → 적 대상 → 예약 적용 → 닫기 → 턴 넘기기**. 기본 사격과 신비탄으로 HP/BP/COST 변화를 확인합니다. 확정·실행 시 모두 BP 0이면 헤일로 이상으로 대체되어 자신과 양옆 피아에 피해를 주고 최대 HP 55%를 소모합니다. [스킬 기능 범위·확인 순서](docs/reviews/2026-10-02-main-skills.md).

**이동 실행:** Play → **서브 이동 → 아군 A → 4번 → 닫기**. 4단계에서 파란 예시 배치를 확인합니다. **예약 확정** 시 원래 배치로 돌아온 뒤 **다음 단계 / 행동**으로 한 칸씩 이동하거나 **턴 넘기기**로 자동 실행합니다. [이동 규칙·화면·확인 순서](docs/reviews/2026-10-02-movement.md).

| 문서 | 용도 |
| --- | --- |
| [스킬 카드 표시](docs/reviews/2026-10-04-skill-slot.md) | 기존 SkillSlot 프리팹의 데이터·BP·타입/속성 색상과 아이콘 연결 |
| [AGENTS.md](AGENTS.md) | Codex의 프로젝트 작업 지침 |
| [프로젝트 기준](docs/PROJECT.md) | 목표, 역할, 범위, 완료 판단 |
| [전투 규칙 안내](docs/BATTLE_RULES.md) | 확정 규칙 요약과 원문 절 안내 |
| [전투 규칙 원문](docs/reference/battle-rules-2026-09-23.txt) | 제공받은 2026-09-23 문서의 보존본 |
| [현재 구현 현황](docs/CURRENT_STATE.md) | 기존 코드 구조와 전환 시 확인할 차이 |
| [마키보 코드 읽기](docs/code-guide/README.md) | 모바일용 구조·작동 순서·변수·함수 해설 및 Notion 목차 |
| [개발 절차](docs/DEVELOPMENT.md) | 환경, 검증, dayoff53 검토 절차 |
| [결정 기록](docs/DECISIONS.md) | 확정 변경, 미정 규칙, 보류 기능 |
| [전환 계획과 진행 상태](docs/ROADMAP.md) | 단계별 목표와 다음 작업 |
| [이번 환경 구성 결과](docs/reviews/2026-09-29-context-setup.md) | 구성 범위와 검증 결과 |

게임 규칙은 원문과 이후 dayoff53이 확정한 결정을 기준으로 합니다. 기존 코드의 동작은 구현 현황이며, 새 규칙의 근거가 아닙니다.
