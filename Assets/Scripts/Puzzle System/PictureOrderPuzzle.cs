using UnityEngine;
using System.Collections.Generic;

public class PictureOrderPuzzle : PuzzleBase
{
    [Header("Pictures to Order")]
    [SerializeField] private List<GameObject> pictures; // assign all pictures in correct order in the inspector

    [Header("Target Slots (Final Positions)")]
    [SerializeField] private List<Transform> targetSlots; // same count as pictures

    [Header("Randomization")]
    [SerializeField] private bool randomizeAtStart = true;

    private List<GameObject> currentOrder = new List<GameObject>();

    private void Start()
    {
        if (pictures.Count != targetSlots.Count)
        {
            Debug.LogError("PictureOrderPuzzle: pictures and slots count mismatch!");
            return;
        }

        // store a copy for runtime
        currentOrder = new List<GameObject>(pictures);

        if (randomizeAtStart)
            RandomizePictures();
    }

    private void RandomizePictures()
    {
        for (int i = 0; i < currentOrder.Count; i++)
        {
            int randIndex = Random.Range(i, currentOrder.Count);
            GameObject temp = currentOrder[i];
            currentOrder[i] = currentOrder[randIndex];
            currentOrder[randIndex] = temp;
        }

        // reposition in world to randomized layout
        for (int i = 0; i < currentOrder.Count; i++)
        {
            currentOrder[i].transform.position = targetSlots[i].position;
            currentOrder[i].transform.rotation = targetSlots[i].rotation;
        }

        Debug.Log("Pictures randomized!");
    }

    // Called when player swaps or moves a picture
    public void OnPicturePlaced(GameObject picture, Transform newSlot)
    {
        int slotIndex = targetSlots.IndexOf(newSlot);
        if (slotIndex == -1)
        {
            Debug.LogWarning("Invalid slot for picture placement.");
            return;
        }

        // move picture in logical list
        currentOrder[slotIndex] = picture;

        CheckPuzzleSolved();
    }

    private void CheckPuzzleSolved()
    {
        for (int i = 0; i < pictures.Count; i++)
        {
            if (currentOrder[i] != pictures[i])
                return;
        }

        Debug.Log("✅ Picture order puzzle solved!");
        Solve(); // from PuzzleBase (marks solved and notifies PuzzleManager)
    }
}
