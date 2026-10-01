# Stage_Battle 리소스와 마키보 턴 연결

2026-10-01 · 상태: 검토 대기 · 검토자: dayoff53

## 목적과 적용 범위

사용자가 지정한 [Stage_Battle.unity](../../Assets/Scenes/Stage/Battle/BattleType/Stage_Battle.unity)의 `InGame` 리소스로 턴 진행을 확인한다. 원문 1절의 9슬롯/시작 2명, 4–8절의 턴 단계/Speed/예약/COST를 적용한다. 전투 규칙을 추가 확정하지 않았다.

## 구현

- 씬에 `Makibo Battle`을 추가했다. [StageBattleView](../../Assets/MaseiKivotos/Unity/StageBattleView.cs)가 기존 `InGame/UnitSlots` 9개의 위치와 바닥 스프라이트를 사용한다. 왼쪽부터 1–9번이며 원래 오브젝트 이름 `UnitSlot_0`–`UnitSlot_8`은 유지한다.
- 기존 슬롯의 `UnitBase/UnitCharactorSprite` 렌더러에 프로젝트의 시로코·호시노·라이플맨 스프라이트를 연결한다. PNG와 Aseprite의 이름 충돌을 피하도록 정확한 PNG 프레임을 씬의 Sprite 필드로 지정했다. 표시 유닛은 여전히 임시 A/B/X/Y이며 능력치/편성은 밸런스 데이터가 아니다. 애니메이션 이벤트와 공격 동작은 연결하지 않았다.
- 기존 `TurnEndButton`, `ActionPointSkipNutton`, COST 바를 재사용한다. Play 중 해당 인스턴스만 새 HUD로 옮기고 이벤트를 새 컨트롤러에 연결한다. 배경/바닥/슬롯 배치는 유지하며 신규 텍스트·초기화/동률 버튼을 보완했다.
- 기존 미연결 메뉴와 AP/HP 표시 UI는 Play 중 숨긴다. 저장/옵션/공격/이동/승리 메뉴가 동작하는 것처럼 보이지 않게 한다. 프리팹 자산이나 스프라이트를 삭제하지 않았다.
- [TurnBattleView](../../Assets/MaseiKivotos/Unity/TurnBattleView.cs)를 통해 기존 검증 씬과 Stage_Battle이 동일한 턴 컨트롤러와 Core를 사용한다. 격리된 카브 코드는 재연결하지 않았다.
- 원본 `DDBattleInGame.prefab`은 작업 직전 파일과 SHA-256이 동일하다. 씬은 새 루트/컴포넌트만 추가했다. 기존 사용자 씬 이동과 프리팹 변경을 되돌리지 않았다. 자동 생성 솔루션 파일은 직접 편집하지 않았다.

## 검증

열린 원본 Unity의 편집 상태와 분리하기 위해 `Logs/StageValidation`에 Assets/Packages/ProjectSettings를 복사해 **Unity 6000.5.10f1**로 실행했다. 복사본의 Library는 새로 생성했다. 원본 에디터의 화면 조작 성공을 뜻하지 않는다.

- **컴파일:** 성공. 변경된 코드에서 CS 컴파일 오류 없음.
- **EditMode:** 12/12 통과. `Logs/stage-editmode.xml`.
- **PlayMode:** 2/2 통과. Stage_Battle 연결 검사와 TurnSandbox 회귀 검사. 실제 Canvas raycast/클릭으로 슬롯 예약·취소, 개별 차례, 중복 진행 차단, TURN 01→02, COST 바 0.3→0.5, 처리 중 초기화를 검증했다.
- **화면:** 실제 씬을 렌더링한 [1턴](evidence/2026-10-01-stage-turn-01.png), [2턴](evidence/2026-10-01-stage-turn-02.png). 캐릭터 프레임과 버튼/슬롯 글자 대비를 확인하고 수정했다.
- **보존:** 격리 스크립트 49개와 메타의 해시/원래 경로 재등장 검사 통과. 기존 InGame 프리팹과 ProjectSettings.asset은 작업 시작 백업과 동일하다. 스크립트 외 리소스 폐기 없음.
- 원본과 검증 복사본의 수정 대상 코드/씬이 일치하는지 해시로 확인했다. 요약 증거: [검증 JSON](evidence/2026-10-01-stage-tests.json).
- 보존된 Missing Script 경고와 기존 Sirenix/Odin `GUITimeHelper.Init` / `ImguiElementUtils.Init` 초기화 예외는 남는다. 테스트 성공을 에디터 전체 무오류 또는 플레이어 빌드 성공으로 해석하지 않는다.

재현: 원본 Unity를 닫은 상태에서 `Tools/Test-MakiboTurns.ps1`를 실행하거나, Unity Test Runner에서 `MaseiKivotos.EditModeTests`와 `MaseiKivotos.PlayModeTests` 어셈블리를 실행한다. PlayMode 테스트는 실제 Stage_Battle과 검증 씬을 차례로 로드한다.

## dayoff53 확인 순서

1. Unity가 외부 파일 변경을 감지하면 변경된 씬을 다시 로드한다. 작업 전 씬의 오래된 내용을 덮어 저장하지 않는다. `마키보 → Stage_Battle 열기` 또는 지정 씬을 직접 연다.
2. Play. 기존 배경과 가로 9칸, 아군 1·2번/적군 8·9번, `TURN 01`, 양측 COST 3을 확인한다.
3. 아군 슬롯 클릭으로 턴 종료 예약/취소를 확인한다. 적군 예약은 비공개다.
4. `예약 확정 · 차례 생략` 후 `다음 단계 / 차례`를 눌러 A(150) → X(120) → B(80) → Y(50)의 차례를 확인한다. 초록색 슬롯과 현재 차례 문구가 함께 바뀐다.
5. `턴 넘기기`를 누르면 남은 차례와 다음 초기화를 진행한 뒤 `TURN 02`, 양측 COST 5, 예약 단계에서 멈춘다. 처리 중 중복 클릭은 차단한다.
6. 처리 도중 `처음부터`를 눌러 TURN 01/COST 3으로 돌아오는지 확인한다. `동률 예제`는 전원 Speed 100으로 다시 시작한다.

## 남은 사항

- 턴 종료만 구현된 개발용 전투다. 스킬·피해·이동·배치·상태·승패 연출/메뉴·콘텐츠 데이터는 후속 범위다.
- 원본의 Missing Script 참조는 보존되어 경고가 남는다. 기존 씬을 일괄 정리하거나 공유 프리팹에서 컴포넌트를 제거하지 않았다.
- UI 연결은 Play 시 구성된다. 편집 모드의 원래 InGame 배치는 그대로 보인다.
- 플레이어 빌드는 미실행. dayoff53 검토 전이다.
