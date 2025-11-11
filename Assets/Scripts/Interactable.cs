using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt = "Press [E] to interact";

    public virtual string GetPrompt() => prompt;

    public virtual void Interact(GameObject interactor)
    {
        Debug.Log($"{name} was interacted with by {interactor.name}");
    }
}
