using UnityEngine;
using System.Collections.Generic;
using System;

public class GameFlags : MonoBehaviour
{
    public static GameFlags Instance { get; private set; }

    public Dictionary<string, bool> flags = new Dictionary<string, bool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadFromPlayerPrefs();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetFlag(string flagName, bool value)
    {
        if (string.IsNullOrEmpty(flagName)) return;
        flags[flagName] = value;
        Debug.Log($"[GameFlags] Set '{flagName}' to {value}");
        SaveToPlayerPrefs(); // Auto-save on change (Optional)
    }

    public bool GetFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName)) return false;
        return flags.ContainsKey(flagName) ? flags[flagName] : false;
    }

    // --- SAVE SYSTEM FIX ---
    
    [Serializable]
    private class SaveData
    {
        public List<string> keys = new List<string>();
        public List<bool> values = new List<bool>();
    }

    public void SaveToPlayerPrefs()
    {
        SaveData data = new SaveData();
        foreach (var kvp in flags)
        {
            data.keys.Add(kvp.Key);
            data.values.Add(kvp.Value);
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("GameFlags_Save", json);
        PlayerPrefs.Save();
    }

    public void LoadFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("GameFlags_Save"))
        {
            string json = PlayerPrefs.GetString("GameFlags_Save");
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            flags.Clear();
            for (int i = 0; i < data.keys.Count; i++)
            {
                if (i < data.values.Count)
                {
                    flags[data.keys[i]] = data.values[i];
                }
            }
            Debug.Log("[GameFlags] Data Loaded Successfully");
        }
    }
}