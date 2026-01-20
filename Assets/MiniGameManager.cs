using UnityEngine;
using UnityEngine.Events;

public class MiniGameManager : MonoBehaviour
{
    [Header("External Communication")]
   
    public UnityEvent<string> OnGameStarted; 
    public UnityEvent<string> OnGameWon;
    public UnityEvent<string> OnGameLost;

    [Header("Active State")]
    private IMiniGame _activeGame;

    
    public void LaunchMiniGame(MiniGameBase gameToLaunch)
    {
        if (_activeGame != null)
        {
            Debug.LogWarning("A game is already running!");
            return;
        }

        _activeGame = gameToLaunch;
        
       
        OnGameStarted.Invoke(gameToLaunch.GameID);

      
        gameToLaunch.BeginGame(HandleGameResult);
    }

  
    private void HandleGameResult(bool isWin)
    {
        string id = _activeGame.GameID;
        _activeGame = null; 

        if (isWin)
        {
            Debug.Log($"MiniGame {id} WON");
            OnGameWon.Invoke(id);
        }
        else
        {
            Debug.Log($"MiniGame {id} LOST!");
            OnGameLost.Invoke(id);
        }
    }
    
    public void ForceStopCurrentGame()
    {
        if (_activeGame != null)
        {
            _activeGame.ForceCleanup();
            _activeGame = null;
        }
    }
}
