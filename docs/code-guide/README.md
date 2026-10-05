# 마키보 코드 읽기 — 구조부터 변수·함수까지

[현재 코드 3분 요약](QUICK_READ.md) · [Notion에서 짧게 읽기](https://app.notion.com/p/3ed4ef43abba8129b1b0cfea23154903?pvs=204)

[Notion에서 읽기](https://app.notion.com/p/3ec4ef43abba8112a788ea623561b6fd?pvs=204) · [모바일 HTML](mobile.html)

기준일: 2026-10-01 · 코드 기준: f69ceba · 작성: Codex · 상태: 검토 대기

이 문서는 dayoff53이 휴대폰으로 조금씩 읽으며 현재 마키보 코드를 이해하도록 만든 해설이다. 설명의 양을 줄이기보다 전체 흐름 → 파일의 책임 → 변수와 함수 순서로 나누었다. 코드의 실제 동작을 기준으로 작성했으며 미구현 기능을 완성된 규칙처럼 설명하지 않는다.

## 먼저 읽을 순서

- 처음 5~10분: 이 안내의 '전체 구조'와 '한 턴이 넘어가는 과정'.
- 다음 읽기: 01 데이터 → 02 턴 규칙 → 03 컨트롤러.
- 화면 이해: 04 공통 화면 → 05 Stage 화면 → 06 독립 검증 화면.
- Unity 메뉴 확인: 07·08.
- 검증 이해: 09 규칙 테스트 → 10·11 씬 테스트.
- 개발 도구 확인: 12 테스트 실행 → 13 격리 검증 → 14 어셈블리·씬 연결.

각 장은 먼저 설명과 작동 순서를 읽고, 아래의 '변수·함수 상세'를 사전처럼 찾아볼 수 있게 했다. 같은 이름의 지역 변수도 함수가 다르면 별도 설명한다. 긴 가로 표는 사용하지 않았다.

## 현재 어디까지 구현했는가

현재 완성 범위는 **턴 진행을 확인하는 개발용 전투 기반**이다. 9칸 필드에 양측 두 명을 놓고 Speed 순서를 정한다. 종료 예약을 켜거나 끄고, 예약을 확정한 뒤 개별 차례 또는 전체 턴을 진행할 수 있다. 다음 턴 번호와 COST 변화가 화면에 나타난다.

예약할 수 있는 행동은 턴 종료뿐이다. 일반 스킬·피해·HP 변화·상태 효과·이동·추가 배치·교체·EX·아이템·서포트·턴 도중 Speed 변화·콘텐츠 데이터·승패 연출은 연결하지 않았다. Core에는 퇴각 결과를 받아 승패를 판단하는 입구가 있으나 UI에서 공격하거나 퇴각시키는 기능은 없다.

## 전체 구조

**데이터를 준비한다.** BattleRoster.cs가 기본 능력치, 유닛 정의, 세력 편성, 전투 중 유닛 상태와 난수 공급기를 정의한다.

**전투 상태를 계산한다.** TurnBattle.cs가 단계·순서·예약·완료·COST·승패·진행 기록을 관리한다. Unity 화면에 의존하지 않는다.

**버튼을 규칙 호출로 바꾼다.** TurnSandboxController.cs가 임시 편성을 만들고 슬롯 클릭, 단계 진행, 턴 넘기기, 초기화를 처리한다.

**화면에 보여 준다.** TurnBattleView.cs가 공통 약속이다. StageBattleView.cs는 기존 InGame 리소스를 연결하고, TurnSandboxView.cs는 독립 검증 화면을 생성한다.

**개발과 검증을 돕는다.** Editor의 두 파일은 씬 열기 메뉴를 제공한다. Tests의 세 파일과 Tools의 두 스크립트는 규칙·씬·기존 자산 보존을 검증한다.

호출 방향은 대체로 '사용자 → 화면 버튼 → Controller → TurnBattle → Controller → View.Refresh'다. Core는 화면의 버튼이나 Text를 모른다. View는 전투 값을 직접 계산하거나 변경하지 않고 Controller를 호출한다.

## 헷갈리기 쉬운 단어

**전체 턴 / TurnNumber:** 모든 필드 유닛의 유효한 차례를 처리하는 한 묶음. 모두 끝나야 번호가 1 증가한다.

**차례 / ActiveUnitId:** 지금 실행할 유닛 한 명의 구간. 첫 진행으로 시작, 다음 진행으로 종료한다.

**Phase:** 전체 턴 안의 단계. TurnStart, Initialization, Order, Planning, Execution 다섯 가지와 종료 상태 Ended가 있다.

**예약:** 지금 행동을 즉시 실행하는 것이 아니라 실행 단계에서 무엇을 할지 저장하는 것. 현재 저장 내용은 '턴 종료' 여부뿐이다.

**Definition과 BattleUnit:** Definition은 기본 정보, BattleUnit은 이번 전투에서 바뀌는 배치와 남은 횟수다.

**Field / Reserve / Retreated:** 출전 중 / 대기 중 / 퇴각. Reserve는 '죽음'과 같은 뜻이 아니다.

**COST와 행동 횟수:** COST는 세력 공유 자원이며 상한 10까지 유지·증가한다. 메인/서브 횟수는 유닛마다 초기화되며 이월되지 않는다.

**StepDelay와 Speed:** StepDelay는 자동 진행을 눈으로 보기 위한 초 단위 대기. Speed는 행동 순서를 정하는 정수 능력치다.

## 한 턴이 넘어가는 과정

기본 예제에는 A(150)·B(80)·X(120)·Y(50)가 필드에 있다. C/Z는 대기하므로 순서에 들어오지 않는다.

1. Play 직후 Awake가 화면을 만들고 ResetBattle을 호출한다.
2. ResetBattle이 TurnBattle을 만들고 초기 단계를 Planning까지 진행한다.
3. 화면에는 TURN 01, 아군/적군 COST 3, 순서 A → X → B → Y가 보인다.
4. 아군 슬롯을 클릭하면 종료 예약이 켜지고 다시 클릭하면 꺼진다. 이 조작만으로 차례가 시작되지는 않는다.
5. '예약 확정'을 누르면 Step → AdvanceOne이 호출된다. 개발용 처리로 미예약자를 포함한 모든 필드 유닛에 종료 예약을 채우고 Execution으로 이동한다.
6. '다음 단계 / 차례'를 누르면 A 차례가 시작되고 ActiveUnitId가 "A"가 된다.
7. 다시 누르면 A가 완료되고 ActiveUnitId가 null이 된다. 아직 TurnNumber는 1이다.
8. 같은 방식으로 X, B, Y를 처리한다. 마지막 Y가 끝나면 FinishRound가 TurnNumber를 2로 바꾸고 TurnStart로 돌아간다.
9. 이어 턴 시작 검사, 행동 횟수 초기화와 COST 획득, 순서 결정을 진행하면 2턴 Planning에 도착한다.
10. '턴 넘기기'는 이 과정을 코루틴으로 자동 반복한다. 2턴 Planning에 도착하면 멈추므로 TURN 02와 양측 COST 5가 보인다.

'턴 넘기기'는 번호만 올리는 버튼이 아니다. 남아 있는 차례와 다음 턴 초기화를 실제 Core 호출로 처리한다. 실행 중 중복 호출은 IsAdvancing으로 막는다.

## 값이 언제 바뀌는가

**TurnNumber:** 생성 시 1. FinishRound에서만 증가한다. ResetBattle은 새 객체를 만들므로 다시 1이다.

**남은 메인/서브:** Initialization에서 필드 유닛에 1/기본 서브 횟수를 지급한다. 차례 종료 또는 퇴각 시 0이다. 대기에는 지급하지 않는다.

**COST:** 생성 시 세력마다 3. 첫 초기화에서는 증가하지 않는다. 2턴 이후 현재 필드 인원만큼 증가하며 최대 10이다. 아직 비용을 쓰는 행동은 없다.

**order/reserved/completed:** 순서 결정 또는 턴 종료 경계에서 비운다. 이벤트 기록은 턴을 넘어 유지되지만 최근 256개만 보관한다.

**화면 문구:** View.Refresh가 위 값을 읽어 반영한다. 버튼을 눌렀다고 텍스트가 독립적으로 턴 번호를 계산하지 않는다.

## 예외와 보호 장치

Core는 잘못된 단계의 예약 수정, 미예약 확정, 잘못된 편성과 ID를 예외로 알린다. Controller는 자동 진행 중 추가 입력과 종료 후 진행을 먼저 무시한다. 두 종류의 보호는 역할이 다르다.

퇴각 묶음은 전체 ID를 먼저 검사한 뒤 반영한다. UI는 상대 예약 기록을 걸러 표시한다. 읽기 전용 목록은 외부에서 컬렉션을 수정하지 못하게 하지만 매번 새 사본을 만드는 것은 아니다.

Missing Script 경고가 남는 것은 기존 리소스에서 격리한 카브 스크립트 참조를 보존했기 때문이다. 새 코드가 이 스크립트를 다시 연결해 사용하는 것은 아니다.

## 검증 상태와 이 문서의 기준

2026-10-01 별도 Unity 프로젝트 복사본에서 컴파일, EditMode 12/12, PlayMode 2/2 통과 기록이 있다. 이후 주석 정비에서는 C# 11개 파일의 코드 토큰이 변경 전후 동일함을 확인했다. 이번 문서 작업은 코드를 바꾸거나 Unity 테스트를 새로 실행하지 않는다.

기존 Sirenix/Odin 초기화 예외와 Missing Script 경고는 남아 있으며 플레이어 빌드는 미실행이다. dayoff53의 검토 완료를 의미하지 않는다.

이 해설은 f69ceba의 소스에 고정되어 있다. 원본 링크도 커밋 기준이라 이후 브랜치 코드가 바뀌어도 지금 설명하던 코드를 열 수 있다. 새 기능을 구현하면 해설 갱신이 필요하다.

## C# 표기를 읽는 작은 사전

- get만 있는 속성: 값을 조회하는 창구. private set은 해당 클래스가, internal set은 같은 어셈블리가 바꿀 수 있다.
- readonly 필드: 필드가 가리키는 대상을 다시 대입하지 못하게 한다. List 내부 내용까지 불변이라는 뜻은 아니다.
- int?: 정수 또는 null을 담는다. Slot에서 필드 밖 상태를 표현한다.
- IEnumerable: 순회할 수 있는 입력. ToArray/ToList는 해당 시점의 목록을 만든다.
- HashSet: 중복 없는 집합. 예약 여부/완료 여부를 빠르게 조회한다.
- Dictionary: 키별 값. 여기서는 세력 ID로 COST를 찾는다.
- Where/Select/Any: 조건에 맞는 항목 고르기 / 값 변환 / 하나라도 존재하는지 검사다.
- 람다의 u 또는 e: 그 함수식이 검사 중인 유닛/이벤트 한 개를 잠시 부르는 이름이다.
- IEnumerator와 yield return: Unity가 프레임이나 대기 시간에 걸쳐 이어 실행하는 코루틴이다.
- SerializeField: 비공개 필드를 Unity Inspector/직렬화로 설정할 수 있게 한다.
- abstract/override: 공통 약속과 그 구체적인 구현을 뜻한다.
- Assert: 테스트의 기대값 검사다. 실패하면 해당 테스트가 실패한다.

## 문서 범위와 누락 검사

마키보 C# 11개 파일을 소스 구문 트리와 대조했다. 선언 222곳(복수 필드를 한 줄에 선언한 경우 한 선언으로 집계), 함수 매개변수 135개, 지역·반복문·람다 이름 184곳을 각 파일의 해설에 포함했다. 같은 변수 이름이 여러 함수에 있으면 사용 위치별로 따로 설명한다.

Tools의 PowerShell 2개와 asmdef 5개, 씬·메타 연결도 별도 장으로 설명한다. 소스 코드는 변경하지 않았다. Notion 15개 페이지를 저장 후 다시 읽어 선언/매개변수/지역 변수 위치와 하위 페이지 연결을 확인했다.

## 근거 문서

- [설명 기준 커밋](https://github.com/dayoff53/CounterAchive/commit/f69ceba8208cc6b17680aa0e5997cc7477fcb30a)
- [보존된 전투 규칙 원문](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/docs/reference/battle-rules-2026-09-23.txt)
- [확정 결정과 미정 사항](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/docs/DECISIONS.md)
- [Stage_Battle 구현·실행 검증](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/docs/reviews/2026-10-01-stage-battle-resources.md)
- [한국어 주석 정비·코드 동일성 검사](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/docs/reviews/2026-10-01-code-comments.md)

관련 Notion 안내: [Notion 전투 규칙 안내](https://app.notion.com/p/3ea4ef43abba80d688aace0ec963c097). 규칙 판단은 해당 요약만으로 하지 않고 보존 원문과 결정 기록을 함께 확인한다.

## 장별 상세 해설

아래 하위 페이지를 순서대로 읽거나 관심 파일부터 열면 된다.

- [01. 전투 데이터를 읽는 법 — BattleRoster](01-data.md)
- [02. 턴 규칙의 중심 — TurnBattle](02-turn-battle.md)
- [03. 버튼에서 다음 턴까지 — TurnSandboxController](03-controller.md)
- [04. 두 화면의 공통 약속 — TurnBattleView](04-view-contract.md)
- [05. 기존 InGame 리소스를 연결하는 법 — StageBattleView](05-stage-view.md)
- [06. 독립 검증 화면 — TurnSandboxView](06-sandbox-view.md)
- [07. 실제 전투 씬 열기 — StageBattleScene](07-stage-menu.md)
- [08. 검증 씬 생성과 열기 — TurnSandboxScene](08-sandbox-menu.md)
- [09. 규칙 검증을 읽는 법 — TurnBattleTests](09-core-tests.md)
- [10. 실제 씬 연결 검증 — StageBattleTests](10-stage-tests.md)
- [11. 검증 화면의 회귀 검사 — TurnSandboxTests](11-sandbox-tests.md)
- [12. Unity 테스트 실행 도구 — Test-MakiboTurns](12-test-runner.md)
- [13. 카브 격리 검증 도구 — Verify-CarveQuarantine](13-quarantine.md)
- [14. 어셈블리·씬·메타·검증 기록의 연결](14-connections.md)
