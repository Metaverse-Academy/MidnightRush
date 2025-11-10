using System.Collections.Generic;
using UnityEngine;

public class SwitchSequencePuzzle : PuzzleBase
{
    [SerializeField] private List<int> correctOrder = new() { 2, 4, 1, 3 };
    private List<int> input = new();

    public void PressSwitch(int id)
    {
        if (isSolved) return;

        input.Add(id);

        // early mismatch check
        for (int i = 0; i < input.Count; i++)
        {
            if (input[i] != correctOrder[i])
            {
                input.Clear();
                // play error SFX
                return;
            }
        }

        // full match?
        if (input.Count == correctOrder.Count) {
            Solve();
            // turn on house lights here or in a separate listener
        }
    }
}
