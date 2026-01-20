using UnityEngine;
using System.Collections.Generic;


public class GameFlags : MonoBehaviour
{
    public static GameFlags Instance { get; private set; }

    private Dictionary<string, bool> flags = new Dictionary<string, bool>();
    private Dictionary<string, int> intVariables = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
    }

    public bool GetFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName)) return false;
        return flags.ContainsKey(flagName) ? flags[flagName] : false;
    }

    public bool HasFlag(string flagName)
    {
        return flags.ContainsKey(flagName);
    }


    public void SetInt(string varName, int value)
    {
        if (string.IsNullOrEmpty(varName)) return;
        
        intVariables[varName] = value;
        Debug.Log($"[GameFlags] Set '{varName}' to {value}");
    }

    public int GetInt(string varName)
    {
        if (string.IsNullOrEmpty(varName)) return 0;
        return intVariables.ContainsKey(varName) ? intVariables[varName] : 0;
    }

    public void ModifyInt(string varName, int delta)
    {
        int current = GetInt(varName);
        SetInt(varName, current + delta);
    }

  
    public void ClearAllFlags()
    {
        flags.Clear();
        intVariables.Clear();
        Debug.Log("[GameFlags] All flags cleared");
    }

    public void DebugPrintFlags()
    {
        Debug.Log("=== Current Flags ===");
        foreach (var kvp in flags)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
        foreach (var kvp in intVariables)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
    }

  
    public void SaveToPlayerPrefs()
    {
        foreach (var kvp in flags)
        {
            PlayerPrefs.SetInt($"flag_{kvp.Key}", kvp.Value ? 1 : 0);
        }
        foreach (var kvp in intVariables)
        {
            PlayerPrefs.SetInt($"int_{kvp.Key}", kvp.Value);
        }
        PlayerPrefs.Save();
        Debug.Log("[GameFlags] Saved to PlayerPrefs");
    }

    public void LoadFromPlayerPrefs()
    {
        flags.Clear();
        intVariables.Clear();
        
        Debug.Log("[GameFlags] Loaded from PlayerPrefs");
    }
}