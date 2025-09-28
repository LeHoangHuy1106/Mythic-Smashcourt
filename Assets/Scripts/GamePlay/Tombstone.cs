using UnityEngine;
using System.Collections;

public class Tombstone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform model;  // gán model ở Inspector

    [Header("Animation Config")]
    [SerializeField] private float startY = -0.388f;  // vị trí bắt đầu (chìm dưới đất)
    [SerializeField] private float endY = 0f;         // vị trí kết thúc (trồi lên)
    [SerializeField] private float moveDuration = 0.5f; // thời gian di chuyển

    private void OnEnable()
    {
        if (model == null) return;

        // Reset vị trí và rotation khi spawn
        Vector3 pos = model.localPosition;
        pos.y = startY;              // bắt đầu ở dưới đất
        model.localPosition = pos;

        model.localRotation = Quaternion.Euler(
            -90f,
            Random.Range(0f, 360f),  // random quanh trục Y
            90f
        );

        // chạy animation move (từ startY -> endY)
        StartCoroutine(MoveToTarget(model, endY, moveDuration));
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
}
