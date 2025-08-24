using UnityEngine;

public class HoverAnimation : MonoBehaviour
{
    public float hoverHeight = 0.5f;
    public float hoverSpeed = 2f;

    public bool enableRotation = true;
    public float rotationSpeed = 30f;
    public Vector3 rotationAxis = Vector3.up;

    private Vector3 startPosition;
    private float timeOffset;

    void Start()
    {
        startPosition = transform.position;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }
    void Update()
    {
        float hoverOffset = Mathf.Sin((Time.time + timeOffset) * hoverSpeed) * hoverHeight;

        transform.position = startPosition + Vector3.up * hoverOffset;
    }
}
