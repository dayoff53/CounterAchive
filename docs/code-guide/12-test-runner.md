# 12. Unity 테스트 실행 도구 — Test-MakiboTurns

기준: 2026-10-01 · 커밋 f69ceba · 검토 대기

[소스 파일 열기](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Tools/Test-MakiboTurns.ps1)

이 PowerShell 스크립트는 Unity 테스트를 지정 어셈블리별로 실행하고 결과 XML을 검사한다. 턴 규칙 자체를 구현하지 않는다.

## 작동 순서

1. 저장소 루트를 계산하고 실행 중인 Unity 프로세스가 있는지 검사한다.
2. Unity 실행 파일과 TurnSandbox 씬이 있는지 검사한다.
3. Logs 폴더를 준비하고 All이면 EditMode → PlayMode 순서를 선택한다.
4. 실행마다 새 타임스탬프의 XML/로그 경로를 만든다.
5. Unity를 숨김 창으로 시작하고 프로세스 종료를 기다린다.
6. 이번 XML 존재, 종료 코드, 실행 건수, Passed 결과, 실패 건수를 모두 확인한다.

이전 실행의 통과 XML을 잘못 재사용하지 않도록 파일 이름을 매번 바꾼다. EditMode에는 -nographics를 사용하지만 PlayMode는 실제 UI를 촬영하므로 그래픽 장치를 유지한다. -quit은 넣지 않고 테스트 러너가 종료하도록 한다.

## 매개변수와 모든 이름 있는 변수

- Platform: EditMode / PlayMode / All 중 하나. 기본 All. 잘못된 문자열은 ValidateSet이 거부한다.
- UnityPath: 실행할 Unity.exe 경로. 기본값은 6000.5.10f1 설치 경로다.
- ErrorActionPreference: Stop. PowerShell 오류를 계속 무시하지 않고 중단한다.
- root: Tools 폴더의 상위를 절대 경로로 만든 저장소 루트.
- PSScriptRoot: PowerShell이 제공하는 현재 스크립트 폴더. 호출한 터미널의 현재 폴더와 구분한다.
- platforms: 실제 실행할 모드 배열. All이면 두 항목.
- mode: foreach에서 현재 실행 중인 모드.
- stamp: 밀리초까지 포함한 실행 시각 문자열.
- result: 이번 테스트의 XML 출력 경로.
- log: 이번 Unity 실행의 로그 경로.
- arguments: batchmode, projectPath, runTests, testPlatform, assemblyNames, testResults, logFile 인수 목록. 프로젝트/출력 경로는 공백을 고려해 따옴표로 감싼다.
- process: Start-Process가 반환한 프로세스 객체. PID, ExitCode와 WaitForExit에 사용한다.
- xml: 생성된 결과 파일을 XML로 읽은 객체.
- run: xml의 test-run 요소. result/total/passed/failed를 읽는다.
- null: New-Item의 반환 객체를 버리는 PowerShell 기본값. 오류를 무시하는 의미는 아니다.

## 함수와 검사

이 파일에는 자체 function 선언이 없다. 최상위 절차로 실행한다. Get-Process/Get-Content/Start-Process는 PowerShell 명령이고 WaitForExit는 프로세스 객체의 메서드다.

현재 Unity 프로세스 검사는 이 프로젝트만이 아니라 실행 중인 Unity 전체를 검사한다. 다른 프로젝트의 Unity도 열려 있으면 중단된다. 결과 파일이 없거나 실행 테스트가 0개면 성공으로 취급하지 않는다.

종료까지의 자체 제한 시간은 없다. Unity가 정체되면 스크립트도 기다린다. 이는 게임의 자동 턴 진행에 있는 remainingSteps와 별개다.

## 실행 예시

저장소에서 PowerShell로 `& ./Tools/Test-MakiboTurns.ps1 -Platform EditMode`를 실행하면 규칙 테스트만 실행한다. All 또는 매개변수 생략은 두 종류를 차례대로 실행한다. 이 안내를 작성하면서 스크립트를 재실행하지는 않았다.
