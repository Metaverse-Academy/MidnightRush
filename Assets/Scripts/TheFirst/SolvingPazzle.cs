using System.Collections;
using UnityEngine;

public class SolvingPazzle : Interactable
{
    [Header("Lamp Settings")]
    [SerializeField] private GameObject handPazzle;
    [SerializeField] private GameObject placePazzle;

    [Header("UI Feedback")]
    [SerializeField] private string placePazzlePrompt = "To place the Pazzle, press ☐ ";
    [SerializeField] private string noPazzlePrompt = "You need a letter Cube to Solve the Pazzle.";
    private bool hasPazzle = false;
    public void Interact(GameObject interactor)
    {
        PlayerInteraction player = interactor.GetComponent<PlayerInteraction>();

        if (player != null && player.IsHoldingPazzle && !hasPazzle)
        {
            PlacePazzle(player);
        }
    }
    public new string GetPrompt()
    {
        if (hasPazzle)
        {
            return "The Pazzle has a battery installed.";
        }
        else
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                PlayerInteraction playerInteraction = player.GetComponent<PlayerInteraction>();
                if (playerInteraction != null && playerInteraction.IsHoldingPazzle)
                {
                    return placePazzlePrompt;
                }
            }
            return noPazzlePrompt;
        }
    }
    private void PlacePazzle(PlayerInteraction player)
    {
        if (handPazzle != null) handPazzle.SetActive(false);

        if (placePazzle != null) placePazzle.SetActive(true);

        player.IsHoldingPazzle = false;
        hasPazzle = true;
    }
    public bool HasPazzle()
    {
        return hasPazzle;
    }
}