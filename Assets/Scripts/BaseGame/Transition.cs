using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Transition : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image bgBlack;
    [SerializeField] private Transform iconHat;

    [Header("Timings")]
    [SerializeField] private float openDurationHat = 0.6f;     // thời gian scale nón
    [SerializeField] private float openDurationBG = 0.6f;      // thời gian fade background
    [SerializeField] private float closeDuration = 0.6f;

    [Header("Config")]
    [SerializeField] private bool isOpen = true; // True thì mới chạy OpenScene khi Start

    private const float TargetAlpha = 255f / 255f;
    private bool isTransitioning = false;

    private void Start()
    {
        // Set trạng thái đầu
       

        // Chỉ gọi OpenScene nếu bật cờ isOpen
        if (isOpen)
        {
            if (bgBlack != null)
            {
                SetImageAlpha(bgBlack, TargetAlpha);
                bgBlack.gameObject.SetActive(true);
            }

            if (iconHat != null)
            {
                iconHat.localScale = Vector3.one;
                iconHat.gameObject.SetActive(true);
            }
            StartCoroutine(OpenScene());

        }
    }

    public IEnumerator OpenScene()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        // B1: scale icon từ 1 -> 0
        float t = 0f;
        while (t < openDurationHat)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / openDurationHat);
            float ease = Mathf.SmoothStep(0f, 1f, k);

            if (iconHat) iconHat.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, ease);
            yield return null;
        }
        if (iconHat) iconHat.localScale = Vector3.zero;
        if (iconHat) iconHat.gameObject.SetActive(false);

        // B2: fade background từ 160 -> 0
        t = 0f;
        while (t < openDurationBG)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / openDurationBG);
            float ease = Mathf.SmoothStep(0f, 1f, k);

            if (bgBlack) SetImageAlpha(bgBlack, Mathf.Lerp(TargetAlpha, 0f, ease));
            yield return null;
        }
        if (bgBlack) SetImageAlpha(bgBlack, 0f);
        if (bgBlack) bgBlack.gameObject.SetActive(false);

        isTransitioning = false;
    }

    public void LoadingScene(string sceneName)
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(CloseSceneAndLoad(sceneName));
    }

    private IEnumerator CloseSceneAndLoad(string sceneName)
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        if (bgBlack)
        {
            bgBlack.gameObject.SetActive(true);
            SetImageAlpha(bgBlack, 0f);
        }
        if (iconHat)
        {
            iconHat.gameObject.SetActive(true);
            iconHat.localScale = Vector3.zero;
        }

        float t = 0f;
        while (t < closeDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / closeDuration);
            float ease = Mathf.SmoothStep(0f, 1f, k);

            if (bgBlack) SetImageAlpha(bgBlack, Mathf.Lerp(0f, TargetAlpha, ease));
            if (iconHat) iconHat.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, ease);

            yield return null;
        }

        if (bgBlack) SetImageAlpha(bgBlack, TargetAlpha);
        if (iconHat) iconHat.localScale = Vector3.one;

        SceneManager.LoadScene(sceneName);
    }

    private void SetImageAlpha(Image img, float a)
    {
        if (!img) return;
        var c = img.color;
        c.a = a;
        img.color = c;
    }
}
