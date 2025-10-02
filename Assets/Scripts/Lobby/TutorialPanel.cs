using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TutorialPanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.35f;

    [Header("Buttons")]
    [SerializeField] private Button btnOpen;
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnLeft;
    [SerializeField] private Button btnRight;

    [Header("Pages")]
    [Tooltip("Danh sách trang Tutorial (mỗi item là 1 GameObject). " +
             "Chỉ trang hiện tại sẽ được SetActive(true).")]
    [SerializeField] private GameObject[] contents;

    [Header("Behavior")]
    [Tooltip("Nếu bật, khi ở trang đầu nhấn Left sẽ nhảy sang trang cuối và ngược lại.")]
    [SerializeField] private bool loopPages = false;

    private int currentIndex = 0;
    private Coroutine fadeCo;

    private void Awake()
    {
        // Gán sự kiện nút
        if (btnOpen != null)
        {
            btnOpen.onClick.RemoveAllListeners();
            btnOpen.onClick.AddListener(Open);
        }
        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(Close);
        }
        if (btnLeft != null)
        {
            btnLeft.onClick.RemoveAllListeners();
            btnLeft.onClick.AddListener(PrevPage);
        }
        if (btnRight != null)
        {
            btnRight.onClick.RemoveAllListeners();
            btnRight.onClick.AddListener(NextPage);
        }

        // Start ẩn
        if (canvasGroup != null)
        {
            canvasGroup.gameObject.SetActive(false);
            SetCanvasGroup(0f, false);
        }

        // Ẩn toàn bộ pages ngay từ đầu
        RefreshPages();
    }

    private void Start()
    {
        // đảm bảo trang 0 hiển thị khi mở lần đầu
        SetPage(0);
    }

    // ===================== Open / Close =====================
    public void Open()
    {
        SettingPanel.PlaySFX(0);
        if (canvasGroup != null)
            canvasGroup.gameObject.SetActive(true);

        // luôn quay về trang 0 khi mở (tuỳ biến nếu muốn)
        SetPage(0);

        StartFade(1f, true);
    }

    public void Close()
    {
        SettingPanel.PlaySFX(0);
        StartFade(0f, false, () =>
        {
            if (canvasGroup != null)
                canvasGroup.gameObject.SetActive(false);
        });
    }

    // ===================== Paging =====================
    public void NextPage()
    {
        SettingPanel.PlaySFX(0);
        if (contents == null || contents.Length == 0) return;

        if (currentIndex >= contents.Length - 1)
        {
            if (loopPages) SetPage(0);
            return;
        }
        SetPage(currentIndex + 1);
    }

    public void PrevPage()
    {
        SettingPanel.PlaySFX(0);
        if (contents == null || contents.Length == 0) return;

        if (currentIndex <= 0)
        {
            if (loopPages) SetPage(contents.Length - 1);
            return;
        }
        SetPage(currentIndex - 1);
    }

    public void SetPage(int index)
    {
        if (contents == null || contents.Length == 0) return;
        index = Mathf.Clamp(index, 0, contents.Length - 1);
        currentIndex = index;
        RefreshPages();
        RefreshNavButtons();
    }

    private void RefreshPages()
    {
        if (contents == null) return;
        for (int i = 0; i < contents.Length; i++)
        {
            if (contents[i] != null)
                contents[i].SetActive(i == currentIndex);
        }
    }

    private void RefreshNavButtons()
    {
        bool atFirst = (currentIndex == 0);
        bool atLast = (contents != null && currentIndex == contents.Length - 1);

        if (btnLeft != null) btnLeft.interactable = loopPages || !atFirst;
        if (btnRight != null) btnRight.interactable = loopPages || !atLast;
    }

    // ===================== Fade helpers =====================
    private void StartFade(float to, bool enableInteract, System.Action onFinish = null)
    {
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeRoutine(to, enableInteract, onFinish));
    }

    private IEnumerator FadeRoutine(float to, bool enableInteractAtEnd, System.Action onFinish)
    {
        if (canvasGroup == null) yield break;

        float from = canvasGroup.alpha;
        float t = 0f;

        // trạng thái tương tác trong lúc fade
        canvasGroup.blocksRaycasts = true; // để không click xuyên nền
        canvasGroup.interactable = false; // khoá tương tác trong lúc fade

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(from, to, k);
            yield return null;
        }

        canvasGroup.alpha = to;
        bool visible = to > 0.99f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = enableInteractAtEnd && visible;

        onFinish?.Invoke();
    }

    private void SetCanvasGroup(float alpha, bool interactable)
    {
        canvasGroup.alpha = alpha;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = alpha > 0.99f;
    }
}
