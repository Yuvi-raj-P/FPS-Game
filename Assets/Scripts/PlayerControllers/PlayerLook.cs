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

    [Header("Sensitivity Settings")]
    public float minSensitivity = 5f;
    public float maxSensitivity = 100f;

    void Awake()
    {
#if UNITY_EDITOR
        xSensitivity = 200f;
        ySensitivity = 200f;
#else
            xSensitivity = 50f;
            ySensitivity = 50f;
#endif
        currentTargetFov = defaultFov;

        LoadSensitivitySettings();
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
    public void SetXSensitivity(float sensitivity)
    {
        xSensitivity = Mathf.Lerp(minSensitivity, maxSensitivity, sensitivity);
        SaveSensitivitySettings();
    }
    public void SetYSensitivity(float sensitivity)
    {
        ySensitivity = Mathf.Lerp(minSensitivity, maxSensitivity, sensitivity);
        SaveSensitivitySettings();
    }
    public float GetNormalizedXSensitivity()
    {
        return Mathf.InverseLerp(minSensitivity, maxSensitivity, xSensitivity);
    }
    public float GetNormalizedYSensitivity()
    {
        return Mathf.InverseLerp(minSensitivity, maxSensitivity, ySensitivity);
    }
    private void SaveSensitivitySettings()
    {
        PlayerPrefs.SetFloat("MouseXSensitivity", xSensitivity);
        PlayerPrefs.SetFloat("MouseYSensitivity", ySensitivity);
        PlayerPrefs.Save();
    }
    private void LoadSensitivitySettings()
    {
        if (PlayerPrefs.HasKey("MouseXSensitivity"))
        {
            xSensitivity = PlayerPrefs.GetFloat("MouseXSensitivity");
        }
        if (PlayerPrefs.HasKey("MouseYSensitivity"))
        {
            ySensitivity = PlayerPrefs.GetFloat("MouseYSensitivity");
        }
    }
}
