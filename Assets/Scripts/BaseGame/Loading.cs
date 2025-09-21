using UnityEngine;
using TMPro;
using System.Collections;

public class Loading : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TMP_Text loadingText;       // Text hiển thị % loading
    [SerializeField] private Transition transition;      // Script Transition

    [Header("Config")]
    [SerializeField] private string targetScene = "MainScene"; // Tên scene đích
    [SerializeField] private float maxDuration = 4f;           // tối đa 4 giây

    private void Start()
    {
        if (loadingText != null)
        {
            loadingText.text = "Loading... 0%";
        }
        StartCoroutine(LoadingRoutine());
    }

    private IEnumerator LoadingRoutine()
    {
        float elapsed = 0f;
        int progress = 0;

        while (progress < 100)
        {
            // Bước tiến ngẫu nhiên (1–10%)
            int step = Random.Range(1, 10);
            progress += step;
            if (progress > 100) progress = 100;

            // Cập nhật text
            if (loadingText != null)
                loadingText.text = $"{progress}%";

            // Thời gian chờ ngẫu nhiên nhưng không vượt quá 4s
            float remaining = maxDuration - elapsed;
            float wait = Random.Range(0.05f, 0.4f);
            if (elapsed + wait > maxDuration) wait = remaining;

            elapsed += wait;
            yield return new WaitForSeconds(wait);
        }

        // Đảm bảo hiển thị 100% trước khi chuyển
        if (loadingText != null)
            loadingText.text = "100%";

        yield return new WaitForSeconds(0.3f);

        // Gọi Transition để đóng scene và load MainScene
        if (transition != null)
        {
            transition.LoadingScene(targetScene);
        }
        else
        {
            Debug.LogWarning("Transition chưa được gán trong Loading.cs");
        }
    }
}
