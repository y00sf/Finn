using System;
using UnityEngine;
public interface IMiniGame
{
    string GameID { get; }
    
    void BeginGame(Action<bool> onEndCallback);
    
    void ForceCleanup();
}

public interface IDifficultyScaler
{
    void SetDifficultyMultiplier(float multiplier);
}
public abstract class MiniGameBase : MonoBehaviour, IMiniGame
{
    [Header("Base Settings")]
    [SerializeField] private string gameID;
    
   
    protected Action<bool> _onGameEnd;
    protected bool _isRunning = false;

    public string GameID => gameID;
    
    public void BeginGame(Action<bool> onEndCallback)
    {
        _onGameEnd = onEndCallback;
        _isRunning = true;
        
      
        gameObject.SetActive(true);
        OnStart();
    }

  
    public void ForceCleanup()
    {
        _isRunning = false;
        OnCleanup();
        gameObject.SetActive(false);
    }

    
    protected void EndGame(bool isWin)
    {
        if (!_isRunning) return;
        
        _isRunning = false;
        OnCleanup(); 
        
        
        _onGameEnd?.Invoke(isWin);
        
       
        gameObject.SetActive(false);
    }

  
    protected abstract void OnStart(); 
    protected abstract void OnCleanup();
}
