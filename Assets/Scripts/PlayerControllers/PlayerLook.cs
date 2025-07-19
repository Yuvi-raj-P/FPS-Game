using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Camera cam;
    private float xRotation = 0f;
    public float xSensitivity = 30f;
    public float ySensitivity = 30f;

    [Header("Zoom")]
    public float defaultFov = 48.6f;
    public float zoomedFov = 41f;
    public float zoomSpeed = 10f;

    private float currentTargetFov;

    void Awake()
    {
        #if UNITY_EDITOR
            xSensitivity = 300f;
            ySensitivity = 300f;
        #else
            xSensitivity = 50f;
            ySensitivity = 50f;
        #endif
        currentTargetFov = defaultFov;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        if (cam != null)
        {
            cam.fieldOfView = defaultFov;
        }
    }
    void Update()
    {
        if (cam != null)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, currentTargetFov, Time.deltaTime * zoomSpeed);
        }
    }
    public void SetZoom(bool isZooming)
    {
        currentTargetFov = isZooming ? zoomedFov : defaultFov;
    }


    public void ProcessLook(Vector2 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= (mouseY * Time.deltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime * xSensitivity));

    }
}
