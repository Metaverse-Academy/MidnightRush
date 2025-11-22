using UnityEngine;

public class CubeHolder : MonoBehaviour, IInteractable
{
    public string correctLetter;
    public Transform holderPosition;
    private bool isFilled = false;

    public string GetPrompt() => isFilled ? "The spot is filled" : "Place the cube here";

    public void Interact(GameObject interactor)
    {
        var playerHand = interactor.GetComponent<PlayerHand>();
        if (!playerHand) return;

        if (playerHand.heldCube != null && !isFilled)
        {
            CubeItem cube = playerHand.heldCube;

            // Check if the cube letter matches the correct letter
            if (cube.cubeLetter == correctLetter)
            {
                cube.transform.position = holderPosition.position;
                cube.transform.SetParent(holderPosition);
                cube.state = CubeItem.CubeState.InHolder;
                cube.currentPlayer = null;
                playerHand.heldCube = null;
                isFilled = true;

                Debug.Log($"✔ {interactor.name} placed the correct cube!");
            }
            else
            {
                Debug.Log($"❌ {interactor.name} placed the incorrect cube!");
            }
        }
    }
}