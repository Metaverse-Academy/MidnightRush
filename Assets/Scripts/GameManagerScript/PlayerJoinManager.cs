using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJoinManager : MonoBehaviour
{
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;
    [SerializeField] private Transform[] spawnPoints;

    // [SerializeField] private CinemachineTargetGroup cinemachineTargetGroup; 

    private int currnetPlayerIndex = 0;

    private void Start()
    {
        if (currnetPlayerIndex == 0)
        {
            PlayerInputManager.instance.playerPrefab = player1Prefab;
            currnetPlayerIndex++;
        }

        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoin;
    }

    private void OnPlayerJoin(PlayerInput input)
    {
        if (PlayerInputManager.instance.playerCount == 1)
        {
            PlayerInputManager.instance.playerPrefab = player2Prefab;
        }

        if (spawnPoints.Length > 0)
        {
            int spawnIndex = PlayerInputManager.instance.playerCount - 1;

            if (spawnIndex < spawnPoints.Length)
            {
                input.transform.position = spawnPoints[spawnIndex].position;
            }
        }

        if (PlayerInputManager.instance.playerCount == 2)
        {

        }

        if (PlayerInputManager.instance.playerCount == 2)
        {
            PlayerInputManager.instance.DisableJoining();
        }
    }
}