using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawn : MonoBehaviour
{
    public GameObject PlayerPrefab;

    public Transform[] spawnPoints;

    private int currnetPlayerIndex = 0;
    private Transform defaultSpawnPoint;
    private Transform spawnLocation;
   


    private void Awake()
    {
        
    }
private void Start()
{
    int playerIndex = 0; // spawn at spawnPoints[0]
    SetSpawnLocation(playerIndex);
    SpawnPlayer();
}


    private void SpawnPlayer()
{
    if (PlayerPrefab != null && spawnLocation != null)
    {
        Instantiate(PlayerPrefab, spawnLocation.position, spawnLocation.rotation);
    }
    else
    {
        Debug.LogError("Player prefab or spawn location is not assigned.");
    }
}


    private void Instantiate(object playerPrefab, Vector3 position, Quaternion rotation)
    {
        throw new NotImplementedException();
    }

    private void Instantiate(GameObject playerPrefab, Vector3 position, Quaternion rotation)
    {
        UnityEngine.Object.Instantiate(playerPrefab, position, rotation);
    }


    public void SetSpawnLocation(int index)
    {
        if (index >= 0 && index < spawnPoints.Length)
        {
            spawnLocation = spawnPoints[index];
        }
        else
        {
            Debug.LogError("Invalid spawn point index.");
        }
    }
}

