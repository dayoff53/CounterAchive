# 13. 카브 격리 검증 도구 — Verify-CarveQuarantine

기준: 2026-10-01 · 커밋 f69ceba · 검토 대기

[소스 파일 열기](https://github.com/dayoff53/CounterAchive/blob/f69ceba8208cc6b17680aa0e5997cc7477fcb30a/Tools/Verify-CarveQuarantine.ps1)

이 스크립트는 카브를 Assets 밖에 보관한 기록이 유지되는지 검사한다. 게임이 실행될 때 호출되는 코드는 아니다. Legacy/Carve의 코드 49개를 현재 전투에 연결하지 않는다.

## 검사 순서

1. manifest.json을 읽고 실패 목록을 준비한다.
2. 격리 스크립트와 메타의 SHA-256을 검사한다.
3. 원래 Assets 경로에 격리 코드/메타가 다시 생겼는지 검사한다.
4. 격리 당시 보존한 에셋 목록의 경로와 해시를 검사한다.
5. Assets의 cs.meta에서 퇴역 GUID가 재사용되었는지 확인한다.
6. Assets의 C# 소스에서 격리 타입 이름을 보수적으로 검색한다.
7. 실패가 하나라도 있으면 전부 출력한 뒤 예외로 종료한다. 없으면 검사 결과 객체를 출력한다.

## 입력과 공유 변수

- ManifestPath: 저장소 상대 격리 기록 경로. 기본 Legacy/Carve/2026-09-30/manifest.json.
- ErrorActionPreference: Stop. 처리 오류 발생 시 중단한다.
- PSScriptRoot: PowerShell 기본 변수. 도구가 있는 폴더다.
- root: 저장소 루트의 절대 경로.
- manifest: JSON을 객체로 변환한 원본 보존 기록.
- failures: 실패 설명 문자열을 누적하는 List.
- entry: 각 반복에서 처리하는 격리 항목 또는 보존 에셋 항목. 반복문에 따라 archivePath/guid 또는 path를 읽는다.
- path: 원래 스크립트/메타 재등장 검사에서 사용할 상대 경로. 아래 Assert-Hash 안의 같은 이름 변수는 함수 내부의 절대 경로다.
- retiredGuids: 격리 GUID를 키로 보관하는 해시 테이블. 존재 여부를 빠르게 확인한다.
- meta: 현재 검사 중인 Assets의 cs.meta 파일.
- match: 메타에서 guid 32자리 문자열을 찾은 정규식 결과.
- retiredTypes: manifest에 기록된 declaredTypes를 모아 중복 제거한 이름 목록.
- pattern: 타입 이름을 정규식으로 이스케이프하고 단어 경계로 둘러싼 검색식.
- source: 현재 검사 중인 Assets의 C# 파일.
- code: 읽은 소스 문자열. 주석 제거 후 타입 이름 검색에 사용한다.
- hits: 해당 소스에서 검출한 퇴역 타입 이름을 중복 제거한 배열.
- $_: ForEach-Object 등 파이프라인이 처리 중인 항목. 문맥에 따라 manifest 항목, 타입 이름 문자열, 정규식 일치 항목, 실패 문자열이다.
- true: retiredGuids에 키가 있다는 표시로 저장하는 PowerShell 기본 논리값.

## Assert-Hash 함수

입력 relativePath는 저장소 기준 파일 경로, expectedHash는 manifest에 남긴 SHA-256이다. 함수 내부 path는 root와 relativePath를 합친 절대 경로다.

파일이 없으면 Missing file, 해시가 다르면 Changed file을 failures에 추가한다. 검사마다 즉시 예외를 내지 않으므로 한 번 실행해 여러 문제를 볼 수 있다. 반환값으로 참/거짓을 주는 함수가 아니라 공유 실패 목록을 갱신하는 함수다.

## 출력 필드

QuarantinedScripts는 기록된 격리 스크립트 수, PreservedAssetFiles는 기록된 보존 파일 수다. ArchiveHashes/PreservedAssetHashes는 모든 관련 검사를 통과했을 때 PASS로 출력한다. RetiredSourceReferences는 정적 검색 결과이며 UnityCompilation은 이 스크립트가 컴파일을 검사하지 않았음을 명시한다.

## 역사 기록과 현재 상태의 차이

이 도구는 격리 당시 스냅샷에 대한 보존 검사다. 이후 정상적인 씬 이동이나 에셋 수정도 경로/해시 불일치로 보고한다. 현재의 의도적인 Stage_Battle 연결 변경까지 '코드 손상'으로 단정하면 안 된다. 원래 manifest를 조용히 새 기준으로 덮어쓰면 보존 증거가 사라진다.

주석 제거와 타입 이름 검색은 정규식이므로 C# 컴파일러나 완전한 의존성 분석기가 아니다. 문자열 내부 타입 이름도 보고될 수 있고 주석처럼 보이는 문자열 처리에도 한계가 있다. 결과는 실제 참조를 확인해 해석해야 한다.

이 문서는 기존 도구를 설명한다. 이번에 전체 보존 검사가 모두 통과했다고 주장하지 않는다.
