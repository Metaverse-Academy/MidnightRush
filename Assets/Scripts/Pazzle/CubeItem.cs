using UnityEngine;
public class CubeItem : MonoBehaviour, IInteractable
{
    public string cubeLetter;
    public Transform groundPosition;
    public enum CubeState { Ground, InHand, InHolder }
    public CubeState state = CubeState.Ground;
    public PlayerHand currentPlayer = null;
    public string GetPrompt()
    {
        if (state == CubeState.Ground) return "press E to pick up the cube";
        if (state == CubeState.InHand) return $"press E to drop the cube (player: {currentPlayer.name})";
        return "cube placed";
    }

    public void Interact(GameObject interactor)
    {
        var playerHand = interactor.GetComponent<PlayerHand>();
        if (!playerHand) return;

        if (state == CubeState.Ground)
        {
            //player picks up the cube
            transform.position = playerHand.transform.position;
            transform.SetParent(playerHand.transform);
            state = CubeState.InHand;
            currentPlayer = playerHand;
            playerHand.heldCube = this;
        }
        else if (state == CubeState.InHand && currentPlayer == playerHand)
        {
            //player drops the cube
            transform.position = groundPosition.position;
            transform.SetParent(null);
            state = CubeState.Ground;
            currentPlayer.heldCube = null;
            currentPlayer = null;
        }
    }
}