    // Start of Selection
    using System.Collections;
    using UnityEngine;

    public class CameraManager : Singleton<CameraManager>
    {
        [SerializeField]
        private Camera mainCamera;

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

        void Start()
        {
            mainCamera = Camera.main;
            defaultZoom = mainCamera.orthographicSize;
            defaultPosition = mainCamera.transform.position;

            cameraWidth = Camera.main.orthographicSize * Camera.main.aspect;
            cameraHeight = Camera.main.orthographicSize;
            minPosition = new Vector2(-cameraWidth, -cameraHeight);
            maxPosition = new Vector2(cameraWidth, cameraHeight);

            // 줌 제한을 기본 줌으로 설정
            maxZoom = defaultZoom;
        }

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

        public void ZoomToTarget(Transform target, float targetZoom, float duration)
        {
            StopAllCoroutines();
            float clampedZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            StartCoroutine(ZoomCoroutine(target.position, clampedZoom, duration));
        }

        public void ResetCamera(float duration)
        {
            StopAllCoroutines();
            StartCoroutine(ZoomCoroutine(defaultPosition, defaultZoom, duration));
        }

        private Vector3 targetPosition;
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
