using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class DataUser
{
    // ----------- INT ----------
    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    // ----------- FLOAT ----------
    public static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }
    // ----------- BOOL ----------
    public static void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool GetBool(string key, bool defaultValue = false)
    {
        int defaultInt = defaultValue ? 1 : 0;
        return PlayerPrefs.GetInt(key, defaultInt) == 1;
    }

    // ----------- STRING ----------
    public static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
    }

    public static string GetString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }
    // ----------- LIST<T> ----------
    public static void SetList<T>(string key, List<T> list)
    {
        string json = JsonUtility.ToJson(new Wrapper<T> { items = list });
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
    }

    public static List<T> GetList<T>(string key)
    {
        string json = PlayerPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(json))
            return new List<T>();

        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.items;
    }

    // Wrapper để serialize danh sách
    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> items;
    }
}
