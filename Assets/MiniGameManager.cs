using UnityEngine;
using UnityEngine.Events;

public class MiniGameManager : MonoBehaviour
{
    public static bool IsMiniGameActive = false;

    [Header("External Communication")]
    public UnityEvent<string> OnGameStarted; 
    public UnityEvent<string> OnGameWon;
    public UnityEvent<string> OnGameLost;

    [Header("Active State")]
    private IMiniGame _activeGame;
    
    [SerializeField] private Rigidbody rb;
    
    public void LaunchMiniGame(MiniGameBase gameToLaunch)
    {
        if (_activeGame != null)
        {
            Debug.LogWarning("A game is already running!");
            return;
        }

        IsMiniGameActive = true;

        _activeGame = gameToLaunch;

        OnGameStarted.Invoke(gameToLaunch.GameID);
      
        gameToLaunch.BeginGame(HandleGameResult);
    }

    private void HandleGameResult(bool isWin)
    {
        string id = _activeGame.GameID;
        _activeGame = null; 

        IsMiniGameActive = false;

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
            IsMiniGameActive = false;
        }
    }
}