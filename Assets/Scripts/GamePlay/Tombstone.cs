using UnityEngine;
using System.Collections;

public class Tombstone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform model;  // gán model ở Inspector
    [SerializeField] private Collider checkCollider; // collider bao ngoài (IsTrigger = true)

    [Header("Animation Config")]
    [SerializeField] private float startY = -0.388f;  // vị trí bắt đầu (chìm dưới đất)
    [SerializeField] private float endY = 0f;         // vị trí kết thúc (trồi lên)
    [SerializeField] private float moveDuration = 0.5f; // thời gian di chuyển

    private bool overlapStatue = false;

    private void OnEnable()
    {
        if (model == null) return;

        // Ẩn model ban đầu
        model.gameObject.SetActive(false);

        // Reset vị trí và rotation
        Vector3 pos = model.localPosition;
        pos.y = startY;
        model.localPosition = pos;

        model.localRotation = Quaternion.Euler(
            -90f,
            Random.Range(0f, 360f),
            90f
        );

        // Reset flag
        overlapStatue = false;

        // Bắt đầu kiểm tra trong 0.2s
        StartCoroutine(CheckBeforeAppear());
    }

    private IEnumerator CheckBeforeAppear()
    {
        yield return new WaitForSeconds(0.2f);

        if (overlapStatue)
        {
            // Nếu đang chạm statue thì hủy
            Destroy(gameObject);
        }
        else
        {
            // Nếu không, bật model và chạy animation
            model.gameObject.SetActive(true);
            StartCoroutine(MoveToTarget(model, endY, moveDuration));
        }
    }

    private IEnumerator MoveToTarget(Transform target, float targetY, float duration)
    {
        if (target == null) yield break;

        Vector3 start = target.localPosition;
        Vector3 end = new Vector3(start.x, targetY, start.z);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            target.localPosition = Vector3.Lerp(start, end, k);
            yield return null;
        }

        target.localPosition = end;
    }

    // Kiểm tra va chạm với statue qua trigger
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("statue"))
        {
            overlapStatue = true;
        }
    }
}
