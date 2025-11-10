using TMPro;
using UnityEngine;

public class HintUI : MonoBehaviour
{
    [SerializeField] private TMP_Text hintText;

    private void Start()
    {
        if (!hintText) hintText = GetComponentInChildren<TMP_Text>();
        PuzzleManager.Instance.OnPuzzleSolved += ShowHint;
    }
    private void OnDestroy()
    {
        if (PuzzleManager.Instance) PuzzleManager.Instance.OnPuzzleSolved -= ShowHint;
    }

    private void ShowHint(int index)
    {
        switch (index)
        {
            case 0: hintText.text = "You hear a latch in the living room…"; break;
            case 1: hintText.text = "A soft thud upstairs… the bedroom might be open."; break;
            case 2: hintText.text = "Cold air seeps from below… the basement calls."; break;
            case 3: hintText.text = "Power surges—light crawls across the house…"; break;
            default: hintText.text = ""; break;
        }
    }
}
