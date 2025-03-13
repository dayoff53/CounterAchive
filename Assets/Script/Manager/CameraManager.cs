    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// 카메라 관리를 위한 매니저 클래스
    /// </summary>
    public class CameraManager : Singleton<CameraManager>
    {
        [SerializeField]
        private Camera _mainCamera;
        public Camera mainCamera
        {
            get { return _mainCamera; }
            set 
            { 
                _mainCamera = value;
                Debug.Log($"mainCamera가 변경되었습니다: {_mainCamera}");
            }
        }

        private float defaultZoom;
        private Vector3 defaultPosition;
        private float cameraWidth = 12.8f;
        private float cameraHeight = 7.2f;

        // 카메라 이동 제한 범위
        private Vector2 minPosition = new Vector2(-12.8f, -7.2f);
        private Vector2 maxPosition = new Vector2(12.8f, 7.2f);

        // 카메라 줌 제한
        private float minZoom = 1f; // 기본 orthographicSize
        private float maxZoom = 7.2f;

        /// <summary>
        /// 초기화 함수. 카메라의 기본 설정값을 지정합니다.
        /// </summary>
        private void Start()
        {
            Init();
        }

    public void Init()
    {
            mainCamera = Camera.main;
            Debug.Log($"mainCamera: {mainCamera}");
            defaultZoom = mainCamera.orthographicSize;
            defaultPosition = mainCamera.transform.position;

            cameraWidth = Camera.main.orthographicSize * Camera.main.aspect;
            cameraHeight = Camera.main.orthographicSize;
            minPosition = new Vector2(-cameraWidth, -cameraHeight);
            maxPosition = new Vector2(cameraWidth, cameraHeight);

            // 줌 제한을 기본 줌으로 설정
            maxZoom = defaultZoom;
    }

    /// <summary>
    /// 카메라의 위치를 설정하는 함수입니다.
    /// </summary>
    /// <param name="position">이동할 목표 위치</param>
    public void SetCameraPosition(Vector3 position)
        {
            cameraWidth = Camera.main.orthographicSize * Camera.main.aspect;
            cameraHeight = Camera.main.orthographicSize;
            Vector3 clampedPosition = new Vector3(
                Mathf.Clamp(position.x, minPosition.x + cameraWidth, maxPosition.x - cameraWidth),
                Mathf.Clamp(position.y, minPosition.y + cameraHeight, maxPosition.y - cameraHeight),
                position.z = mainCamera.transform.position.z
            );
            mainCamera.transform.position = clampedPosition;
        }

        /// <summary>
        /// 특정 대상을 향해 카메라를 줌인/아웃하는 함수입니다.
        /// </summary>
        /// <param name="target">줌의 대상이 되는 Transform</param>
        /// <param name="targetZoom">목표 줌 크기</param>
        /// <param name="duration">줌 동작 시간</param>
        public void ZoomToTarget(Transform target, float targetZoom, float duration)
        {
            StopAllCoroutines();
            float clampedZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            StartCoroutine(ZoomCoroutine(target.position, clampedZoom, duration));
        }

        /// <summary>
        /// 카메라를 초기 상태로 되돌리는 함수입니다.
        /// </summary>
        /// <param name="duration">리셋 동작 시간</param>
        public void ResetCamera(float duration)
        {
            StopAllCoroutines();
            StartCoroutine(ZoomCoroutine(defaultPosition, defaultZoom, duration));
        }

        private Vector3 targetPosition;
        /// <summary>
        /// 카메라 줌 동작을 처리하는 코루틴입니다.
        /// </summary>
        /// <param name="targetPosition">목표 위치</param>
        /// <param name="targetZoom">목표 줌 크기</param>
        /// <param name="duration">동작 시간</param>
        private IEnumerator ZoomCoroutine(Vector3 targetPosition, float targetZoom, float duration)
        {
            this.targetPosition = targetPosition;
            Vector3 startPosition = mainCamera.transform.position;
            float startZoom = mainCamera.orthographicSize;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;

                SetCameraPosition(Vector3.Lerp(startPosition, targetPosition, t));
                // 카메라 줌 조절
                float newZoom = Mathf.Lerp(startZoom, targetZoom, t);
                mainCamera.orthographicSize = Mathf.Clamp(newZoom, minZoom, maxZoom);

                yield return null;
            }

            // 최종 위치와 줌 값 설정
            SetCameraPosition(targetPosition);
            mainCamera.orthographicSize = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

    }
