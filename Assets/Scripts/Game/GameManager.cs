using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private PlayerController currentPlayer;
    public PlayerController CurrentPlayer => currentPlayer;
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void RegisterPlayer(PlayerController player)
    {
        currentPlayer = player;
    }
    public void UnregisterPlayer()
    {
        currentPlayer = null;
    }
}
