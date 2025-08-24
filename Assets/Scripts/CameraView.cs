using UnityEngine;
using UnityEngine.UI;

public class CameraView : MonoBehaviour
{
    void Update()
    {
        transform.LookAt(Camera.main.transform);
    }
}
