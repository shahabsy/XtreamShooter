using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    public GameObject CurrentPlayer { get; private set; }

    public void SpawnPlayer()
    {
        if(CurrentPlayer != null)
            Destroy(CurrentPlayer);

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;

        CurrentPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        CurrentPlayer.name = "Player";

        GameManager.Instance.RegisterPlayer(CurrentPlayer.GetComponent<PlayerController>());
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnPlayer();
    }
}
