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


}
