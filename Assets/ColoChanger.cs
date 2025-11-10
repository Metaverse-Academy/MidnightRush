using UnityEngine;

public class ColoChanger : MonoBehaviour, IInteractable
{
    private MeshRenderer meshRenderer;

    public string GetPrompt()
    {
        return "Change Color";
    }

    public void Interact(GameObject interactor)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = new Color(Random.value, Random.value, Random.value);
        }
    }

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }


}
