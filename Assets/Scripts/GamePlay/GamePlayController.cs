using UnityEngine;
using System.Collections.Generic;

// Dùng struct để chứa config player
public struct PlayerConfig
{
    public int power;
    public float ballSpeed;
    public float freezeTime;
}

public class GamePlayController : MonoBehaviour
{
    [Header("References")]
    public WallSetupTopDown setupWalls;
    public GameObject statuePrefab;
    public Transform parentStatue;

    [Header("Characters")]
    public Character player;
    public Character enemy;

    [Header("Models")]
    public GameplayModel gameplayModel;

    [Header("Runtime")]
    public List<GameObject> statues = new List<GameObject>();

    private void Start()
    {
        SettingPanel.Instance.PlayGameplayMusic();
        int currentLevel = DataGame.Instance != null ? DataGame.Instance.Level : 1;
        LevelData cfg = gameplayModel.GetLevelData(currentLevel);

        // ✅ Lấy config Player từ SubLevels
        PlayerConfig playerCfg = GetPlayerConfig();

        // ✅ Tính balanceFactor
        float balanceFactor = CalculateBalanceFactor(playerCfg.power, Mathf.Abs(cfg.enemyBallPower),currentLevel);

        // Spawn tượng
        SpawnStatue(cfg.amountStatue, balanceFactor);

        if (GameplayView.Instance != null)
            GameplayView.Instance.SetTotalStatue(cfg.amountStatue);

        // Player setup
        if (player != null)
        {
            player.ConfigurePlayer(
                playerCfg.ballSpeed,   // speed ball
                1f,                    // spawnCooldown tạm fix
                0.15f,                 // respawnDelay tạm fix
                2f,                    // rotateTime tạm fix
                playerCfg.power        // power
            );
            player.SetFreezeTime(playerCfg.freezeTime); // scale theo sublevel
            player.StartGame();
            player.PrepareBall();
        }

        // Enemy setup (theo GameplayModel)
        if (enemy != null)
        {
            enemy.ConfigureEnemy(
                cfg.enemyBallSpeed,
                cfg.enemySpawnCooldown,
                cfg.enemyRespawnDelay,
                2f, // rotateTime fix
                cfg.enemyBallPower,
                1.2f,
                3f
            );
            enemy.StartGame();
            enemy.PrepareBall();
        }

        // ✅ Gọi hàm set text config UI
        if (GameplayView.Instance != null)
        {
            GameplayView.Instance.SetConfigValues(
                playerCfg.ballSpeed,
                playerCfg.freezeTime,
                playerCfg.power,
                cfg.enemyBallSpeed,
                cfg.enemyRespawnDelay,
                Mathf.Abs(cfg.enemyBallPower)
            );
        }
    }


    // ✅ Hàm lấy config Player dựa vào SubLevels
    private PlayerConfig GetPlayerConfig()
    {
        var subs = DataGame.Instance.SubLevels;
        int speedLevel = subs[0];
        int powerLevel = subs[1];
        int timeLevel = subs[2];

        PlayerConfig cfg = new PlayerConfig();

        cfg.power = powerLevel;
        cfg.ballSpeed = speedLevel;

        // FreezeTime = 1 * 0.95^(timeLevel-1)
        cfg.freezeTime = 2f * Mathf.Pow(0.95f, timeLevel - 1);

        return cfg;
    }

    // ✅ Tính balanceFactor = (min/max)/100
    private float CalculateBalanceFactor(int playerPower, int enemyPower, int level)
    {
        int max = Mathf.Max(Mathf.Abs(playerPower), Mathf.Abs(enemyPower));
        int min = Mathf.Min(Mathf.Abs(playerPower), Mathf.Abs(enemyPower));

        if (max == 0) return 0.01f;

        // Nếu level < 10 → chia 50, ngược lại chia 100
        int divisor = (level < 10) ? 50 : 100;
        return (float)min / max / divisor;
    }
    private void Update()
    {
        if (player != null && Input.GetMouseButtonDown(0))
            player.ShootBall();

        if (player != null && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            player.ShootBall();
    }

    public void SpawnStatue(int amount, float balanceFactor)
    {
        if (statuePrefab == null || setupWalls == null || parentStatue == null || amount <= 0)
        {
            Debug.LogWarning("Setup thiếu tham chiếu hoặc amount <= 0");
            return;
        }

        var limits = setupWalls.GetLimits();
        float minX = limits[0];
        float maxX = limits[1];

        foreach (var s in statues)
            if (s != null) Destroy(s);
        statues.Clear();

        float offset = 0.5f;
        float spawnMinX = minX + offset;
        float spawnMaxX = maxX - offset;

        if (amount == 1)
        {
            float centerX = (spawnMinX + spawnMaxX) * 0.5f;
            Vector3 pos = new Vector3(centerX, 0f, 0f);
            GameObject statue = Instantiate(statuePrefab, pos, Quaternion.identity, parentStatue);

            statue.GetComponent<Statue>()?.SetBalanceFactor(balanceFactor);

            statue.transform.localScale = Vector3.one;
            statues.Add(statue);
        }
        else
        {
            float step = (spawnMaxX - spawnMinX) / (amount - 1);
            for (int i = 0; i < amount; i++)
            {
                float xPos = spawnMinX + step * i;
                Vector3 pos = new Vector3(xPos, 0f, 0f);

                GameObject statue = Instantiate(statuePrefab, pos, Quaternion.identity, parentStatue);
                statue.GetComponent<Statue>()?.SetBalanceFactor(balanceFactor);

                statue.transform.localScale = Vector3.one;
                statues.Add(statue);
            }
        }
    }
}
