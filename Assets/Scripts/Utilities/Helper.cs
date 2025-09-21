using System.Collections;
using UnityEngine;

public class Helper : MonoBehaviour
{
    private static Helper _instance;

    public static Helper Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("[Helper]");
                _instance = go.AddComponent<Helper>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public static void TweenTransform(Transform target, Transform start, Transform end, float duration, System.Action onComplete = null)
    {
        if (target == null || start == null || end == null)
        {
            Debug.LogWarning("[TweenTransform] One or more transforms are null.");
            return;
        }

        Instance.StartCoroutine(Instance.TweenTransformRoutine(target, start, end, duration, onComplete));
    }

    private IEnumerator TweenTransformRoutine(Transform target, Transform start, Transform end, float duration, System.Action onComplete)
    {
        float time = 0f;

        Vector3 startPos = start.position;
        Vector3 endPos = end.position;

        Vector3 startScale = start.lossyScale; // For world scale
        Vector3 endScale = end.lossyScale;

        Quaternion startRot = start.rotation;
        Quaternion endRot = end.rotation;

        while (time < duration)
        {
            if (target == null)
            {
                Debug.LogWarning("[TweenTransform] Target transform destroyed during tween.");
                yield break;
            }

            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            // Easing: SmoothStep (ease-out)
            t = t * t * (3f - 2f * t);

            target.position = Vector3.Lerp(startPos, endPos, t);
            target.localScale = Vector3.Lerp(startScale, endScale, t);
            target.rotation = Quaternion.Lerp(startRot, endRot, t);

            yield return null;
        }

        if (target != null)
        {
            target.position = endPos;
            target.localScale = endScale;
            target.rotation = endRot;
        }

        onComplete?.Invoke();
    }

    public static string FormatSeconds(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }
}
