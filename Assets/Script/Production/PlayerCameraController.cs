using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField]
    [InfoBox("카메라 타겟")]
    GameObject cameraTarget;
    

    [SerializeField]
    [InfoBox("카메라 위치")]
    Vector3 cameraPosition = new Vector3(0, 0, -10);

    [SerializeField]
    [InfoBox("카메라 이동 속도")]
    float cameraMoveSpeed = 10f;

    [SerializeField]
    [InfoBox("카메라 줌 제한")]
    float minZoom = 1f;
    float maxZoom = 7.2f;
    Vector2 currentSize = new Vector2(0, 0);
    float cameraZoomSpeed = 10f;
    


    [SerializeField]
    [InfoBox("맵의 크기")]
    Vector2 mapSize;    

    void Start()
    {
        currentSize = new Vector2(Camera.main.orthographicSize * Screen.width / Screen.height, Camera.main.orthographicSize);
    }

    void FixedUpdate()
    {
        CameraMove();
    }

    void CameraMove()
    {
        transform.position = Vector3.Lerp(transform.position, cameraTarget.transform.position + cameraPosition, 
                                  Time.deltaTime * cameraMoveSpeed);

        float clampX = Mathf.Clamp(transform.position.x, -mapSize.x + currentSize.x, mapSize.x - currentSize.x);
        
        float clampY = Mathf.Clamp(transform.position.y, -mapSize.y + currentSize.y, mapSize.y - currentSize.y);

        transform.position = new Vector3(clampX, clampY, cameraPosition.z);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(currentSize.x, currentSize.y, cameraPosition.z) * 2);
    }
}
