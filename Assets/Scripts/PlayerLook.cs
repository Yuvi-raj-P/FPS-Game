using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Camera cam;
    private float xRotation = 0f;
    public float xSensitivity = 30f;
    public float ySensitivity = 30f;
    void Awake()
    {
        #if UNITY_EDITOR
            xSensitivity = 300f;
            ySensitivity = 300f;
        #else
            xSensitivity = 50f;
            ySensitivity = 50f;
        #endif
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
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
