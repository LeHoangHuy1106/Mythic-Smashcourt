using UnityEngine;
using System.Collections.Generic;
using System;

public class DataGame : MonoBehaviour
{
    public static DataGame Instance { get; private set; }

    public Action<int> OnCoinChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ---------------- LEVEL ----------------
    private const string KEY_LEVEL = "DATA_LEVEL";

    public int Level
    {
        get => DataUser.GetInt(KEY_LEVEL, 1);  // mặc định level 1
        set => DataUser.SetInt(KEY_LEVEL, Mathf.Max(1, value));
    }

    public void NextLevel()
    {
        Level++;
        Debug.Log($"[DataGame] Level up: {Level}");
    }

    public void ResetLevel()
    {
        Level = 1;
        Debug.Log("[DataGame] Reset Level về 1");
    }

    // ---------------- SUB LEVELS ----------------
    private const string KEY_SUB_LEVELS = "DATA_SUB_LEVELS";

    /// <summary>
    /// List 3 phần tử: [0]=Speed, [1]=Power, [2]=Time
    /// </summary>
    public List<int> SubLevels
    {
        get
        {
            var list = DataUser.GetList<int>(KEY_SUB_LEVELS);
            if (list == null || list.Count != 3)
            {
                list = new List<int> { 1, 1, 1 }; // mặc định
                DataUser.SetList(KEY_SUB_LEVELS, list);
            }
            return list;
        }
        set
        {
            if (value == null || value.Count != 3)
                throw new ArgumentException("SubLevels phải có đúng 3 phần tử (Speed, Power, Time)");

            DataUser.SetList(KEY_SUB_LEVELS, value);
        }
    }

    public int GetSubLevel(int index)
    {
        var list = SubLevels;
        return list[Mathf.Clamp(index, 0, 2)];
    }

    public void SetSubLevel(int index, int value)
    {
        var list = SubLevels;
        list[Mathf.Clamp(index, 0, 2)] = Mathf.Max(1, value);
        SubLevels = list; // ghi lại
    }

    // ---------------- COIN ----------------
    private const string KEY_COIN = "DATA_COIN";

    public int Coin
    {
        get => DataUser.GetInt(KEY_COIN, 0); // mặc định 0 coin
        set
        {
            int safeValue = Mathf.Max(0, value);
            DataUser.SetInt(KEY_COIN, safeValue);
            OnCoinChanged?.Invoke(safeValue);
        }
    }

    public void AddCoin(int amount)
    {
        if (amount <= 0) return;
        Coin += amount;
        Debug.Log($"[DataGame] AddCoin: +{amount}, total={Coin}");
    }

    public bool SpendCoin(int amount)
    {
        if (amount <= 0) return false;
        if (Coin < amount)
        {
            Debug.LogWarning("[DataGame] Not enough coin!");
            return false;
        }

        Coin -= amount;
        Debug.Log($"[DataGame] SpendCoin: -{amount}, left={Coin}");
        return true;
    }
}
