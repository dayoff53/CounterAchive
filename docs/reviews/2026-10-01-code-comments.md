# 마키보 스크립트 한국어 주석 정비

2026-10-01 · 상태: 검토 대기 · 검토자: dayoff53

사용자 요청에 따라 기존 카브의 XML 문서 주석 형식을 참고해 `Assets/MaseiKivotos`의 C# 파일 11개를 정비했다.

- 클래스·함수·필드·프로퍼티·enum 선언 222곳에 역할 설명을 작성했다.
- 함수 매개변수 135개와 내부 변수 선언 115곳에 설명을 작성했다.
- 턴 번호와 유닛 차례의 차이, 예약 확정 시점, COST 초기화, 비공개 예약, 임시 편성, 미구현 기능을 구분했다.
- 기존 영문 설명은 한국어로 정리했고, enum과 난수 인터페이스는 항목별 주석을 붙이도록 줄바꿈만 정리했다.

## 확인 순서

1. [BattleRoster.cs](../../Assets/MaseiKivotos/Core/BattleRoster.cs): 능력치·편성·배치 상태와 각 값의 의미.
2. [TurnBattle.cs](../../Assets/MaseiKivotos/Core/TurnBattle.cs): 단계별 진행과 예약·정렬·퇴각 처리.
3. [TurnSandboxController.cs](../../Assets/MaseiKivotos/Unity/TurnSandboxController.cs): 입력·코루틴·초기화 및 현재 개발용 동작.
4. [StageBattleView.cs](../../Assets/MaseiKivotos/Unity/StageBattleView.cs): 기존 리소스와 화면 변수·버튼 연결.

## 검증

Roslyn으로 변경 전후를 비교해 **11개 파일의 코드 토큰이 동일함**을 확인했다. `UNITY_EDITOR`를 켠 분기와 끈 분기를 각각 비교했으며, 문자열·식별자·연산자·직렬화 속성은 유지되었다. XML 주석의 형식과 실제 매개변수명 일치, 선언별 주석 누락도 검사했다. [검증 증거](evidence/2026-10-01-code-comments.json).

주석·공백만 변경한 작업이므로 Unity 컴파일·Play Mode·기존 규칙 테스트는 이번에 다시 실행하지 않았다. 이전 실행 검증 기록의 소스 해시는 당시 파일 기준으로 보존한다. 씬·프리팹·리소스·격리 코드와 기존 사용자 변경은 수정 대상에서 제외했다.
