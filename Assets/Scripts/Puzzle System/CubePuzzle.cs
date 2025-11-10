using UnityEngine;

public class CubePuzzle : PuzzleBase
{
    [Header("Correct Cubes")]
    [SerializeField] private GameObject correctCube1;
    [SerializeField] private GameObject correctCube2;

    [Header("Socket Slots")]
    [SerializeField] private Transform slot1;
    [SerializeField] private Transform slot2;

    private GameObject placedCube1;
    private GameObject placedCube2;

    public void PlaceCube(int slotIndex, GameObject cube)
    {
        if (isSolved) return;

        if (slotIndex == 0)
            placedCube1 = cube;
        else if (slotIndex == 1)
            placedCube2 = cube;

        CheckPuzzleSolved();
    }

    private void CheckPuzzleSolved()
    {
        if (placedCube1 == null || placedCube2 == null) return;

        bool correct =
            (placedCube1 == correctCube1 && placedCube2 == correctCube2) ||
            (placedCube1 == correctCube2 && placedCube2 == correctCube1);

        if (correct)
        {
            Debug.Log("✅ Cube puzzle solved!");
            Solve(); // From PuzzleBase
        }
        else
        {
            Debug.Log("❌ Wrong cubes, try again.");
        }
    }

    public void RemoveCube(GameObject cube)
    {
        if (cube == placedCube1) placedCube1 = null;
        else if (cube == placedCube2) placedCube2 = null;
    }
}
