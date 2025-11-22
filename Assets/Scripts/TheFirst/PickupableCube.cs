using UnityEngine;

public class PickupableCube : Interactable
{
    [SerializeField] private int cubeID = 1;
    [SerializeField] private string promptMessage = "Pick up Cube";
    public override string GetPrompt()
    {
        PlayerInteraction player = FindObjectOfType<PlayerInteraction>();
        if (player != null && player.IsHoldingCube())
        {
            return "Press E to swap cubes";
        }
        return promptMessage;
    }

    public override void Interact(GameObject interactor)
    {
        PlayerInteraction player = interactor.GetComponent<PlayerInteraction>();
        if (player == null) return;

        PickupableCube oldCube = player.GetHeldCubeReference();

        player.SetHeldCube(true, this.cubeID, this);

        this.gameObject.SetActive(false);

        if (oldCube != null)
        {
            oldCube.transform.position = this.transform.position;
            oldCube.transform.rotation = this.transform.rotation;

            oldCube.gameObject.SetActive(true);
        }
    }
}