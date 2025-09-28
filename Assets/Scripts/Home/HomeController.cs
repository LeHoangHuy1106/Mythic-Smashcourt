using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class HomeController : MonoBehaviour
{
    [Header("UI References")]
    public Animator uiAnimator;                        // Animator điều khiển panel
    public Button btnGo;                               // Nút GO
    [SerializeField] private TextMeshProUGUI textCoin;
    [Header("Scroll View")]
    [SerializeField] private ScrollRect scrollView;    // ScrollRect (bắt buộc với cách này)
    [SerializeField] private RectTransform viewport;   // Viewport của ScrollView
    [SerializeField] private RectTransform content;    // Content của ScrollView

    [Header("Level Items")]
    [SerializeField] private ItemLevel itemLevelPrefab; // Prefab 1 ô level (có ItemLevel)
    [SerializeField, Min(1)] private int totalLevels = 40;

    [Header("Transition")]
    [SerializeField] private Transition transition;    // Tham chiếu Transition (Inspector)

    [Header("Runtime")]
    public List<ItemLevel> listItemLevel = new List<ItemLevel>();

    private bool builtOnce = false;

    private void Awake()
    {
        if (btnGo) btnGo.onClick.AddListener(OnClickGo);

        if (!viewport && scrollView) viewport = scrollView.viewport;
        if (!content && scrollView) content = scrollView.content;
        if (scrollView && content) scrollView.content = content;
        SetCoin(DataGame.Instance.Coin);
    }
    public void SetCoin(int coin)
    {
        if (textCoin != null)
            textCoin.text = coin.ToString();
    }
    private void OnEnable()
    {
        if (builtOnce) return;

        BuildLevels(totalLevels);
        SetupLevelsFromData();
        StartCoroutine(ScrollToCurrentLevelAfterLayout());

        builtOnce = true;
    }

    // ---------------- Build & Setup ----------------

    private void BuildLevels(int count)
    {
        if (!itemLevelPrefab || !content)
        {
            Debug.LogError("[HomeController] Thiếu itemLevelPrefab hoặc content/viewport!");
            return;
        }

        foreach (Transform t in content) Destroy(t.gameObject);
        listItemLevel.Clear();

        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(itemLevelPrefab.gameObject, content, false);
            go.name = $"Level_{i + 1}";
            go.SetActive(true);
            listItemLevel.Add(go.GetComponent<ItemLevel>());
        }
    }

    private void SetupLevelsFromData()
    {
        if (listItemLevel.Count == 0) return;

        int levelCurrent = (DataGame.Instance != null) ? Mathf.Max(1, DataGame.Instance.Level) : 1;
        levelCurrent = Mathf.Clamp(levelCurrent, 1, listItemLevel.Count);
        int openIndex = levelCurrent - 1;

        for (int i = 0; i < listItemLevel.Count; i++)
        {
            // state: 0 = block, 1 = current, 2 = done
            int state = 0;
            if (i == openIndex) state = 1;
            else if (i < openIndex) state = 2;

            bool isUnlocked = (state != 0);
            listItemLevel[i].Init(i, isUnlocked, OnClickLevel, state);
        }
    }

    // ---------------- UI Events ----------------

    private void OnClickGo()
    {
        if (uiAnimator) uiAnimator.SetTrigger("Hide");
        SettingPanel.Instance.PlaySound(0);
        foreach (var item in listItemLevel)
            if (item && !item.gameObject.activeSelf) item.gameObject.SetActive(true);

        StartCoroutine(ScrollToCurrentLevelAfterLayout());
    }

    private void OnClickLevel(int levelIndex)
    {
        int selectLevel = levelIndex + 1;
        if (DataGame.Instance != null)
            DataGame.Instance.Level = Mathf.Clamp(selectLevel, 1, listItemLevel.Count);
        SettingPanel.Instance.PlaySound(0);
        if (transition != null)
            transition.LoadingScene("LobbyScene");
        else
            SceneManager.LoadScene("LobbyScene");
    }

    // ---------------- Scroll Helpers ----------------

    private IEnumerator ScrollToCurrentLevelAfterLayout()
    {
        if (!scrollView || !viewport || !content || listItemLevel.Count == 0) yield break;

        // Đợi layout tính xong kích thước
        yield return null;
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        int levelCurrent = (DataGame.Instance != null) ? Mathf.Max(1, DataGame.Instance.Level) : 1;
        int index = Mathf.Clamp(levelCurrent - 1, 0, listItemLevel.Count - 1);

        // Cuộn NGANG nếu scrollView.horizontal; nếu vertical thì cuộn DỌC
        if (scrollView.horizontal) ScrollToIndexHorizontal(index, true, 0.25f);
        else ScrollToIndexVertical(index, true, 0.25f);
    }

    /// <summary>Cuộn NGANG đưa item index vào giữa viewport (dùng horizontalNormalizedPosition).</summary>
    private void ScrollToIndexHorizontal(int index, bool smooth, float duration = 0.25f)
    {
        if (!scrollView || !content || !viewport) return;

        float contentW = content.rect.width;
        float viewportW = viewport.rect.width;

        // Không cuộn được nếu content không dài hơn viewport
        if (contentW <= viewportW)
        {
            scrollView.horizontalNormalizedPosition = 0f;
            return;
        }

        RectTransform itemRT = listItemLevel[index].GetComponent<RectTransform>();
        // tâm item trong toạ độ content (gốc tại pivot của content)
        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(content, itemRT);
        float itemCenterX = bounds.center.x;

        // toạ độ của mép trái content trong hệ trục với gốc tại pivot
        float leftEdgeX = -contentW * content.pivot.x;

        // khoảng cách từ mép trái tới tâm item
        float centerFromLeft = itemCenterX - leftEdgeX;

        // muốn đặt tâm item vào giữa viewport:
        float targetPixels = Mathf.Clamp(centerFromLeft - viewportW * 0.5f, 0f, contentW - viewportW);

        float targetNormalized = targetPixels / (contentW - viewportW);

        if (!smooth) { scrollView.horizontalNormalizedPosition = targetNormalized; return; }

        StartCoroutine(SmoothSetHorizontalNormalized(targetNormalized, duration));
    }

    /// <summary>Cuộn DỌC đưa item index vào giữa viewport (dùng verticalNormalizedPosition).</summary>
    private void ScrollToIndexVertical(int index, bool smooth, float duration = 0.25f)
    {
        if (!scrollView || !content || !viewport) return;

        float contentH = content.rect.height;
        float viewportH = viewport.rect.height;

        if (contentH <= viewportH)
        {
            scrollView.verticalNormalizedPosition = 1f; // top
            return;
        }

        RectTransform itemRT = listItemLevel[index].GetComponent<RectTransform>();
        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(content, itemRT);
        float itemCenterY = bounds.center.y;

        // mép TOP trong toạ độ pivot content (y dương hướng lên)
        float topEdgeY = (1f - content.pivot.y) * contentH;

        // khoảng cách từ TOP xuống tâm item
        float centerFromTop = topEdgeY - itemCenterY;

        float targetPixels = Mathf.Clamp(centerFromTop - viewportH * 0.5f, 0f, contentH - viewportH);

        // verticalNormalizedPosition: 1 = top, 0 = bottom
        float targetNormalized = 1f - (targetPixels / (contentH - viewportH));

        if (!smooth) { scrollView.verticalNormalizedPosition = targetNormalized; return; }

        StartCoroutine(SmoothSetVerticalNormalized(targetNormalized, duration));
    }

    private IEnumerator SmoothSetHorizontalNormalized(float target, float duration)
    {
        scrollView.StopMovement();
        float start = scrollView.horizontalNormalizedPosition;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            float e = 1f - Mathf.Pow(1f - k, 3f);
            scrollView.horizontalNormalizedPosition = Mathf.Lerp(start, target, e);
            yield return null;
        }
        scrollView.horizontalNormalizedPosition = target;
    }

    private IEnumerator SmoothSetVerticalNormalized(float target, float duration)
    {
        scrollView.StopMovement();
        float start = scrollView.verticalNormalizedPosition;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            float e = 1f - Mathf.Pow(1f - k, 3f);
            scrollView.verticalNormalizedPosition = Mathf.Lerp(start, target, e);
            yield return null;
        }
        scrollView.verticalNormalizedPosition = target;
    }
}
