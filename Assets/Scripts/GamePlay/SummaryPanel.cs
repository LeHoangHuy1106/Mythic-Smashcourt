using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class SummaryPanel : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject darkBg;       // "dark bg"
    [SerializeField] private GameObject contentRoot;  // "Content"

    [Header("Panels")]
    [SerializeField] private GameObject panelEnemy;   // Content/Enemy
    [SerializeField] private GameObject panelPlayer;  // Content/Player

    [Header("Scores UI")]
    [SerializeField] private TextMeshProUGUI playerPanelScoreText; // tỉ số panel Player
    [SerializeField] private TextMeshProUGUI enemyPanelScoreText;  // tỉ số panel Enemy

    [Header("Player Win UI")]
    [SerializeField] private TextMeshProUGUI coinRewardText;  // coin reward (panel Player)

    [Header("Buttons")]
    [SerializeField] private Button btnHome;    // panel Enemy
    [SerializeField] private Button btnReplay;  // panel Enemy
    [SerializeField] private Button btnNext;    // panel Player
    [SerializeField] private Button btnHome2;   // panel Player

    [Header("Transition Ref")]
    [SerializeField] private Transition transition;   // tham chiếu Transition trong scene

    [Header("Config")]
    [SerializeField] private string mainSceneName = "MainScene";   // nút Home
    [SerializeField] private string lobbySceneName = "LobbyScene";  // Replay/Next
    [SerializeField] private float animDuration = 0.25f;
    [SerializeField] private float countDuration = 0.8f;

    private Coroutine scaleCo;
    private Coroutine countCo;

    void Awake()
    {
        HideAll();

        if (btnHome)
        {
            btnHome.onClick.RemoveAllListeners();
            btnHome.onClick.AddListener(OnClickHome);
        }
        if (btnReplay)
        {
            btnReplay.onClick.RemoveAllListeners();
            btnReplay.onClick.AddListener(OnClickReplay);
        }
        if (btnNext)
        {
            btnNext.onClick.RemoveAllListeners();
            btnNext.onClick.AddListener(OnClickNext);
        }
        if (btnHome2)
        {
            btnHome2.onClick.RemoveAllListeners();
            btnHome2.onClick.AddListener(OnClickHome);
        }
    }

    // ================== Public API ==================

    public void ShowPanelSummaryEnemyWin(int playerScore, int enemyScore)
    {
        if (enemyPanelScoreText) enemyPanelScoreText.text = $"{playerScore} - {enemyScore}";
        SettingPanel.Instance.PlaySound(2);
        SetActiveSafe(darkBg, true);
        SetActiveSafe(contentRoot, true);

        SetActiveSafe(panelPlayer, false);
        AnimatePanel(panelEnemy);
    }

    public void ShowPanelSummaryPlayerWin(int playerScore, int enemyScore)
    {
        int coinReward = playerScore * 2;

        if (DataGame.Instance != null)
        {
            DataGame.Instance.NextLevel();
            DataGame.Instance.AddCoin(coinReward);
        }
        SettingPanel.Instance.PlaySound(10);

        if (playerPanelScoreText) playerPanelScoreText.text = $"{playerScore} - {enemyScore}";

        if (coinRewardText) coinRewardText.text = "0";
        if (btnHome2) btnHome2.interactable = false;
        if (btnNext) btnNext.interactable = false;

        SetActiveSafe(darkBg, true);
        SetActiveSafe(contentRoot, true);

        SetActiveSafe(panelEnemy, false);
        AnimatePanel(panelPlayer);

        if (countCo != null) StopCoroutine(countCo);
        countCo = StartCoroutine(CountCoinThenFinish(coinReward));
    }

    public void HideAll()
    {
        if (scaleCo != null) { StopCoroutine(scaleCo); scaleCo = null; }
        if (countCo != null) { StopCoroutine(countCo); countCo = null; }

        SetActiveSafe(panelEnemy, false);
        SetActiveSafe(panelPlayer, false);
        SetActiveSafe(contentRoot, false);
        SetActiveSafe(darkBg, false);
    }

    // ================== Buttons ==================

    private void OnClickHome()
    {
        if (transition != null)
            transition.LoadingScene(mainSceneName);
        else
            SceneManager.LoadScene(mainSceneName);
    }

    private void OnClickReplay()
    {
        if (transition != null)
            transition.LoadingScene(lobbySceneName);
        else
            SceneManager.LoadScene(lobbySceneName);
    }

    private void OnClickNext()
    {
        if (transition != null)
            transition.LoadingScene(lobbySceneName);
        else
            SceneManager.LoadScene(lobbySceneName);
    }

    // ================== Animation ==================

    private void AnimatePanel(GameObject panel)
    {
        if (!panel) return;

        panel.SetActive(true);
        panel.transform.localScale = Vector3.one * 0.4f;

        if (scaleCo != null) StopCoroutine(scaleCo);
        scaleCo = StartCoroutine(ScaleUp(panel.transform, Vector3.one, animDuration));
    }

    private IEnumerator ScaleUp(Transform target, Vector3 endScale, float duration)
    {
        Vector3 start = target.localScale;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            float e = Mathf.SmoothStep(0f, 1f, k);
            target.localScale = Vector3.Lerp(start, endScale, e);
            yield return null;
        }
        target.localScale = endScale;
        scaleCo = null;
    }

    // ================== Coin Counter ==================

    private IEnumerator CountCoinThenFinish(int targetCoin)
    {
        float elapsed = 0f;
        int displayed = 0;

        targetCoin = Mathf.Max(0, targetCoin);

        while (elapsed < countDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(elapsed / countDuration);
            float e = 1f - Mathf.Pow(1f - k, 3f);
            displayed = Mathf.RoundToInt(Mathf.Lerp(0, targetCoin, e));

            if (coinRewardText)
                coinRewardText.text = displayed.ToString();

            yield return null;
        }

        if (coinRewardText)
            coinRewardText.text = targetCoin.ToString();


        if (btnHome2) btnHome2.interactable = true;
        if (btnNext) btnNext.interactable = true;

        countCo = null;
    }

    // ================== Utils ==================

    private static void SetActiveSafe(GameObject go, bool active)
    {
        if (go && go.activeSelf != active) go.SetActive(active);
    }
}
