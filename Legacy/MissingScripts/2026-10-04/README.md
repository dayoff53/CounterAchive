# Missing Script 정리 전 백업

2026-10-04 사용자 요청: Inspector에서 Mono Script 연결이 없는 컴포넌트를 제거한다.

- `before/Assets/`: 정리 직전 현재 작업 파일 32개와 각각의 `.meta`. Git HEAD가 아니라 사용자의 기존 편집을 포함한 디스크 상태다. Unity 임포트 대상인 프로젝트 `Assets` 밖에 보관한다.
- [manifest.json](manifest.json): 원본 경로, 백업 경로, 정리 전후 SHA-256, meta SHA-256, 직접 제거한 컴포넌트 수.
- [unity-report.json](unity-report.json): Unity에서 실제 확인한 GameObject 경로와 정리 전후 누락 개수.
- [검토 기록](../../../docs/reviews/2026-10-04-missing-script-cleanup.md): 검사·적용 범위와 회귀 검증.

이 폴더의 `.gitattributes`는 `before/`의 자동 줄바꿈 변환을 끈다. Git 업로드/체크아웃으로 백업의 원본 바이트와 manifest 해시가 바뀌지 않도록 한다.

231개 씬/프리팹을 검사했다. 실제 수정은 프리팹 9개와 씬 23개이며, 167개 컴포넌트를 직접 제거했다. 정리 전 989개라는 값은 중첩 프리팹 인스턴스마다 보이던 누락을 중복 집계한 수치다. 원본 프리팹에서 먼저 제거하면 여러 씬에서 함께 사라지므로 167과 989는 같은 기준의 개수가 아니다. 정리 후 전수 검사에서 누락 컴포넌트는 0개다.

정상 GameObject·컴포넌트 유형/개수·활성 상태·태그·레이어·로컬 위치/회전/크기·SpriteRenderer/UI Image 스프라이트·Animator 컨트롤러 참조의 지문이 정리 전후 일치했다. Unity 저장 과정에서 직렬화 버전/기본 필드가 갱신되거나 더 이상 필요 없는 `stripped` 참조 블록이 정리된 부분은 실제 GameObject 삭제와 구분한다. 모든 리소스 파일 및 GUID는 유지한다.

구형 ScriptableObject `.asset`과 격리된 카브 `.cs/.meta`는 변경하지 않는다. 기존 `Legacy/Carve/2026-09-30/manifest.json`도 당시의 보존 기록으로 유지한다. 그 기록을 검사하는 `Verify-CarveQuarantine.ps1`은 이후 의도적으로 편집한 씬/프리팹의 해시 차이를 보고할 수 있으며, 그것을 감추기 위해 과거 해시를 갱신하지 않는다.

복구가 필요하면 Unity를 닫고 manifest의 경로를 대조한다. 현재 파일이 기록된 `after` 해시와 다르면 후속 변경이 있다는 뜻이므로 백업으로 덮어쓰지 말고 차이를 먼저 병합한다. 전체 복구가 아니라 필요한 파일만 검토해 복구하며, 이 백업의 구형 스크립트 참조가 다시 Missing Script를 만들 수 있다는 점을 함께 확인한다.
