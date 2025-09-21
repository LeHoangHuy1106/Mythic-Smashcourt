using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Header("Rotation Config")]
    public bool rotateX = true;
    public bool rotateY = true;
    public bool rotateZ = true;

    [Tooltip("Số giây để quay 1 vòng trên trục X")]
    public float secondsPerRotationX = 5f;

    [Tooltip("Số giây để quay 1 vòng trên trục Y")]
    public float secondsPerRotationY = 5f;

    [Tooltip("Số giây để quay 1 vòng trên trục Z")]
    public float secondsPerRotationZ = 5f;

    void Update()
    {
        Vector3 rotation = Vector3.zero;

        if (rotateX && secondsPerRotationX > 0f)
        {
            float degreesPerSecondX = 360f / secondsPerRotationX;
            rotation.x = degreesPerSecondX * Time.deltaTime;
        }

        if (rotateY && secondsPerRotationY > 0f)
        {
            float degreesPerSecondY = 360f / secondsPerRotationY;
            rotation.y = degreesPerSecondY * Time.deltaTime;
        }

        if (rotateZ && secondsPerRotationZ > 0f)
        {
            float degreesPerSecondZ = 360f / secondsPerRotationZ;
            rotation.z = degreesPerSecondZ * Time.deltaTime;
        }

        transform.Rotate(rotation, Space.Self);
    }
}
