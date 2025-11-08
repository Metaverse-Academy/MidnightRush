using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;
    [SerializeField] private Transform[] spawnPoints;
    private int currentPlayerIndex = 0;
    private void Start()
    {
        if (PlayerInputManager.instance == null)
        {
            Debug.LogError("PlayerInputManager is not found in the scene!");
            return;
        }

        if (currentPlayerIndex == 0)
        {
            PlayerInputManager.instance.playerPrefab = player1Prefab;
        }
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoin;
    }
    private void OnPlayerJoin(PlayerInput input)
    {
        if (currentPlayerIndex < spawnPoints.Length)
        {
            input.transform.position = spawnPoints[currentPlayerIndex].position;
        }
        else
        {
            Debug.LogWarning("there are not enough spawn points for the players!");
            input.transform.position = spawnPoints[0].position;
        }

        Camera playerCamera = input.GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            Rect rect = new Rect();

            if (currentPlayerIndex == 0)
            {
                rect.x = 0f;
                rect.y = 0f;
                rect.width = 0.5f;
                rect.height = 1f;
            }
            else if (currentPlayerIndex == 1)
            {
                rect.x = 0.5f;
                rect.y = 0f;
                rect.width = 0.5f;
                rect.height = 1f;
            }

            playerCamera.rect = rect;
        }
        else
        {
            Debug.LogError("the player camera is not found!");
        }
        currentPlayerIndex++;

        if (currentPlayerIndex == 1)
        {
            PlayerInputManager.instance.playerPrefab = player2Prefab;
        }

        if (PlayerInputManager.instance.playerCount == 2)
        {
            PlayerInputManager.instance.DisableJoining();
        }
    }
}