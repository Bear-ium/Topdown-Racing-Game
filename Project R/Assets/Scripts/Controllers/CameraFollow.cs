using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Assign your car (player) GameObject here
    public float smoothSpeed = 0.125f; // Adjust for smoothness
    public Vector3 offset; // Customize the offset to get the right view

    void LateUpdate()
    {
        if (target == null) return;

        // Desired position of the camera with offset
        Vector3 desiredPosition = target.position + offset;

        // Smoothly move the camera to the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
