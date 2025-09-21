using UnityEngine;

public class Billboard : MonoBehaviour
{
    [Tooltip("Camera sẽ nhìn về. Để trống thì tự lấy Camera.main")]
    public Camera targetCamera;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        // 👉 Bước 1: Reset local rotation để xóa xoay cha
        transform.localRotation = Quaternion.identity;

        // 👉 Bước 2: Xoay lại về camera bằng world rotation
        Vector3 direction = targetCamera.transform.position - transform.position;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }
}
