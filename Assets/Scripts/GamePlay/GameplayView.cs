using UnityEngine;
using TMPro;

public class GameplayView : MonoBehaviour
{
    public static GameplayView Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;   // 👈 Text hiển thị Level hiện tại

    [Header("Summary Panel Ref")]
    [SerializeField] private SummaryPanel summaryPanel;

    [Header("Config Texts")]
    [SerializeField] private TextMeshProUGUI playerSpeedText;
    [SerializeField] private TextMeshProUGUI playerTimeText;
    [SerializeField] private TextMeshProUGUI playerPowerText;
    [SerializeField] private TextMeshProUGUI enemySpeedText;
    [SerializeField] private TextMeshProUGUI enemyTimeText;
    [SerializeField] private TextMeshProUGUI enemyPowerText;

    [Header("Character Refs")]
    [SerializeField] private Character player;
    [SerializeField] private Character enemy;

    [Header("Scores")]
    public int playerScore = 0;
    public int enemyScore = 0;

    [Header("Statues")]
    public int totalStatue = 1;
    private int count = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        UpdateLevelText();
    }

    public void SetTotalStatue(int total)
    {
        totalStatue = Mathf.Max(1, total);
        ResetScores();
    }

    public void ResetScores()
    {
        playerScore = 0;
        enemyScore = 0;
        count = 0;
        UpdateText();
    }

    public void IncreasePlayerScore(int delta = 1)
    {
        playerScore += delta;
        count += 1;
        UpdateText();
        CheckEndGame();
    }

    public void IncreaseEnemyScore(int delta = 1)
    {
        enemyScore += delta;
        count += 1;
        UpdateText();
        CheckEndGame();
    }

    private void UpdateText()
    {
        if (scoreText != null)
            scoreText.text = $"{enemyScore} - {playerScore}";
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            int currentLevel = (DataGame.Instance != null) ? Mathf.Max(1, DataGame.Instance.Level) : 1;
            levelText.text = $"Level {currentLevel}";
        }
    }

    private void CheckEndGame()
    {
        if (count >= totalStatue)
        {
            // disable 2 nhân vật khi end game
            if (player != null) Destroy(player.gameObject);
            if (enemy != null) Destroy (enemy.gameObject);

            if (playerScore == enemyScore)
            {
                Debug.Log("[GameplayView] Tie! Continue playing...");
            }
            else if (playerScore > enemyScore)
            {
                Debug.Log("[GameplayView] Player wins!");
                if (summaryPanel != null)
                    summaryPanel.ShowPanelSummaryPlayerWin(playerScore, enemyScore);
            }
            else
            {
                Debug.Log("[GameplayView] Enemy wins!");
                if (summaryPanel != null)
                    summaryPanel.ShowPanelSummaryEnemyWin(playerScore, enemyScore);
            }
        }
    }

    public void SetConfigValues(
      float playerSpeed, float playerTime, int playerPower,
      float enemySpeed, float enemyTime, int enemyPower)
    {
        if (playerSpeedText) playerSpeedText.text = $"{playerSpeed:F1}";
        if (playerTimeText) playerTimeText.text = $"{playerTime:F2}";
        if (playerPowerText) playerPowerText.text = $"{playerPower}";

        if (enemySpeedText) enemySpeedText.text = $"{enemySpeed:F1}";
        if (enemyTimeText) enemyTimeText.text = $"{enemyTime:F2}";
        if (enemyPowerText) enemyPowerText.text = $"{enemyPower}";
    }
}
