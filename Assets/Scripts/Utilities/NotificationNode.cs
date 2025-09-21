using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public enum NotificationType
{
    Success,
    Error,
    Info
}

public class NotificationNode : MonoBehaviour
{
    public static NotificationNode Instance { get; private set; }

    [Header("References")]
    public GameObject toastObject;
    public Image backgroundImage;
    public TextMeshProUGUI messageText;

    [Header("Animation Settings")]
    public float fadeDuration = 0.5f;
    public float moveUpDistance = 100f;
    public float displayDuration = 1.5f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        rectTransform = toastObject.GetComponent<RectTransform>();
        canvasGroup = toastObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = toastObject.AddComponent<CanvasGroup>();

        toastObject.SetActive(false);
    }

    public void ShowNotification(string message, NotificationType type = NotificationType.Info)
    {
        StopAllCoroutines();
        StartCoroutine(Show(message, type));
    }

    private IEnumerator Show(string message, NotificationType type)
    {
        toastObject.SetActive(true);
        messageText.text = message;

        switch (type)
        {
            case NotificationType.Success:

                
                backgroundImage.color = Color.green;
                break;
            case NotificationType.Error:

                
                backgroundImage.color = Color.red;
                break;
            case NotificationType.Info:
                backgroundImage.color = Color.white;
                break;
        }

        rectTransform.anchoredPosition = Vector2.zero;
        canvasGroup.alpha = 0f;

        // Fade in + move up
        float time = 0f;
        while (time < fadeDuration)
        {
            float t = time / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            rectTransform.anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(0, moveUpDistance * 0.2f), t);
            time += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = new Vector2(0, moveUpDistance * 0.2f);

        // Hold
        yield return new WaitForSeconds(displayDuration);

        // Fade out + move up more
        time = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, moveUpDistance);
        while (time < fadeDuration)
        {
            float t = time / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            time += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        toastObject.SetActive(false);
    }
}
