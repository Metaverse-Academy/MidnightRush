using UnityEngine;

public class CubeHolder : Interactable
{
    [Header("Solution")]
    [SerializeField] private int correctCubeID = 1;
    [SerializeField] private GameObject placedCubePrefab; // مجسم المكعب الذي سيظهر بعد وضعه

    [Header("Feedback")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string successTrigger = "Open";

    private bool isSolved = false;

    public override string GetPrompt()
    {
        if (isSolved) return "Puzzle is already solved.";

        PlayerInteraction player = FindObjectOfType<PlayerInteraction>(); // ابحث عن اللاعب
        if (player != null && player.IsHoldingCube())
        {
            return "Press E to place cube";
        }
        return "You need a cube.";
    }

    public override void Interact(GameObject interactor)
    {
        if (isSolved) return;

        PlayerInteraction player = interactor.GetComponent<PlayerInteraction>();
        if (player != null && player.IsHoldingCube())
        {
            if (player.GetHeldCubeID() == correctCubeID)
            {
                // الحل الصحيح
                player.SetHeldCube(false, 0, null);
                if (placedCubePrefab != null) placedCubePrefab.SetActive(true);
                if (doorAnimator != null) doorAnimator.SetTrigger(successTrigger);
                isSolved = true;
                Debug.Log("Correct! Puzzle Solved.");
            }
            else
            {
                // الحل الخاطئ
                Debug.Log("Wrong Cube!");
                // يمكنك إضافة رسالة على الشاشة هنا
            }
        }
    }
}