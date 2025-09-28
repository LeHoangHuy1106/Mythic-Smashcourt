using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelData
{
    public int level;

    // Enemy config
    public float enemyBallSpeed;      // L1=1  → L40=10  (tăng đều)
    public float enemySpawnCooldown;  // L1=3  → L40=0.5 (giảm đều)
    public float enemyRespawnDelay;   // L1=2  → L40=0.15 (giảm đều)
    public int enemyBallPower;      // L1=-1 → L40=-10 (giảm đều, int floor)

    // Gameplay
    public int amountStatue;          // L1-2:1, L3-4:2, L5-8:3, L9-10:4, >10:5
}

public class GameplayModel : MonoBehaviour
{
    [Header("Auto-generated level table (1..40)")]
    public List<LevelData> dataLevels = new List<LevelData>(40);

    private void Reset() { RecomputeLevels(); }
    private void OnValidate() { RecomputeLevels(); }
    private void Awake()
    {
        if (dataLevels == null || dataLevels.Count != 40)
            RecomputeLevels();
    }

    /// <summary>
    /// Lấy dữ liệu level (clamp 1..40)
    /// </summary>
    public LevelData GetLevelData(int level)
    {
        int lv = Mathf.Clamp(level, 1, 40);
        if (dataLevels == null || dataLevels.Count != 40) RecomputeLevels();
        return dataLevels[lv - 1];
    }

    /// <summary>
    /// Tính lại toàn bộ bảng 40 level theo công thức
    /// </summary>
    public void RecomputeLevels()
    {
        if (dataLevels == null) dataLevels = new List<LevelData>(40);
        dataLevels.Clear();

        for (int lv = 1; lv <= 40; lv++)
        {
            float t = T(lv); // 0 ở level 1 → 1 ở level 40

            var row = new LevelData
            {
                level = lv,
                enemyBallSpeed = Mathf.Lerp(1f, 10f, t),       // ↑ 1 → 10
                enemySpawnCooldown = Mathf.Lerp(3f, 0.5f, t),      // ↓ 3 → 0.5
                enemyRespawnDelay = Mathf.Lerp(2f, 0.15f, t),     // ↓ 2 → 0.15
                enemyBallPower = Mathf.FloorToInt(Mathf.Lerp(-1f, -10f, t)), // ↓ -1 → -10
                amountStatue = AmountByLevel(lv)
            };

            dataLevels.Add(row);
        }
    }

    // t = 0..1 cho level 1..40 (39 khoảng)
    private static float T(int level)
    {
        return (Mathf.Clamp(level, 1, 40) - 1) / 39f;
    }

    private static int AmountByLevel(int level)
    {
        if (level <= 2) return 1;
        if (level <= 4) return 2;
        if (level <= 8) return 3;
        if (level <= 10) return 4;
        return 5;
    }
}
