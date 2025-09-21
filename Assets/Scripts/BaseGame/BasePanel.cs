using System.Collections;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(CanvasGroup))]
public abstract class BasePanel : MonoBehaviour
{
    // Panel dùng chung để show/ẩn
    public GameObject panel;

    // Nút Back
    public Button btnBack;

    private CanvasGroup canvasGroup;

    protected virtual void Start()
    {
        Init();
    }

    public virtual void Init()
    {
        if (panel != null)
        {
            panel.SetActive(false);
            canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panel.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
        }

        if (btnBack != null)
        {
            btnBack.onClick.RemoveAllListeners();
            btnBack.onClick.AddListener(OnBackClick);
        }
    }

    public virtual void Show()
    {
        if (panel == null) return;

        SettingPanel.Instance.PlaySound(6);
        panel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(FadePanel(0f, 1f)); // Fade in
    }

    public virtual void Hide()
    {
        if (panel == null) return;

        StopAllCoroutines();
        StartCoroutine(FadePanel(1f, 0f)); // Fade out
    }

    private IEnumerator FadePanel(float from, float to)
    {
        float duration = 0.25f;
        float time = 0f;

        if (panel == null)
        {
            Debug.LogWarning("⚠️ FadePanel: Panel đã bị huỷ!");
            yield break;
        }

        if (canvasGroup == null)
        {
            canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            Debug.LogWarning("⚠️ FadePanel: CanvasGroup missing!");
            yield break;
        }

        canvasGroup.alpha = from;

        while (time < duration)
        {
            if (panel == null)
            {
                Debug.LogWarning("⚠️ FadePanel: Panel bị hủy trong khi tween!");
                yield break;
            }

            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        canvasGroup.alpha = to;

        if (Mathf.Approximately(to, 0f) && panel != null)
            panel.SetActive(false);
    }

    protected virtual void OnBackClick()
    {
        Hide();
    }
}
