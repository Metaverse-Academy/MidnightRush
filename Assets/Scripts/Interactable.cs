using UnityEngine;


public class Interactable : MonoBehaviour, IInteractable
{

    [SerializeField] private string prompt = "";

    public virtual string GetPrompt() => prompt;

    public virtual void Interact(GameObject interactor)
    {
        Debug.Log($"{name} was interacted with by {interactor.name}");
    }
}
