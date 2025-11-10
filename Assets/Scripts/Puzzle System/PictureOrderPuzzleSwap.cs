using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PictureOrderPuzzleSwap : PuzzleBase
{
    [Header("Pictures in CORRECT order (drag in left→right final order)")]
    [SerializeField] private List<PictureFrame> pictures = new();   // each has a GameObject in scene

    [Header("Mount points in CORRECT order (left→right final positions)")]
    [SerializeField] private List<Transform> mounts = new();        // empty transforms for positions

    [Header("Randomize")]
    [SerializeField] private bool randomizeAtStart = true;

    [Header("Swap Animation")]
    [SerializeField] private float slideTime = 0.35f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0,0,1,1);

    // runtime
    private PictureFrame selected;
    private Dictionary<PictureFrame, int> currentIndexOf = new();   // where each picture currently is
    private PictureFrame[] pictureAtIndex;                          // which picture currently sits at each index

    void Start()
    {
        if (pictures.Count == 0 || mounts.Count == 0 || pictures.Count != mounts.Count)
        {
            Debug.LogError("PictureOrderPuzzleSwap: pictures and mounts must be same non-zero size.");
            enabled = false;
            return;
        }

        // hook back to controller
        foreach (var p in pictures)
            p.BindController(this);

        // Build initial index map in CORRECT order
        pictureAtIndex = new PictureFrame[pictures.Count];
        for (int i = 0; i < pictures.Count; i++)
        {
            pictureAtIndex[i] = pictures[i];
            currentIndexOf[pictures[i]] = i;
        }

        // Place all pictures at their correct mounts (then shuffle if needed)
        for (int i = 0; i < pictures.Count; i++)
            SnapPictureToMount(pictures[i], mounts[i]);

        if (randomizeAtStart)
            Shuffle();
    }

    private void Shuffle()
    {
        // Fisher-Yates on indices
        for (int i = 0; i < pictureAtIndex.Length; i++)
        {
            int j = Random.Range(i, pictureAtIndex.Length);
            (pictureAtIndex[i], pictureAtIndex[j]) = (pictureAtIndex[j], pictureAtIndex[i]);
        }

        // Update transforms + dictionary
        for (int i = 0; i < pictureAtIndex.Length; i++)
        {
            var pic = pictureAtIndex[i];
            currentIndexOf[pic] = i;
            SnapPictureToMount(pic, mounts[i]);
        }
    }

    private void SnapPictureToMount(PictureFrame pic, Transform mount)
    {
        var t = pic.transform;
        t.position = mount.position;
        t.rotation = mount.rotation;
    }

    // Called by PictureFrame when the player interacts
    public void OnFrameClicked(PictureFrame frame)
    {
        if (isSolved) return;

        if (selected == null)
        {
            selected = frame;
            selected.SetHighlight(true);
            return;
        }

        if (selected == frame)
        {
            // deselect
            selected.SetHighlight(false);
            selected = null;
            return;
        }

        // we have two frames → swap them
        var a = selected;
        var b = frame;
        selected.SetHighlight(false);
        selected = null;

        StartCoroutine(SwapFramesRoutine(a, b));
    }

    private IEnumerator SwapFramesRoutine(PictureFrame a, PictureFrame b)
    {
        int ia = currentIndexOf[a];
        int ib = currentIndexOf[b];

        Transform mountA = mounts[ia];
        Transform mountB = mounts[ib];

        // Animate slide to each other's mount
        yield return StartCoroutine(SlideTo(a.transform, mountB.position, mountB.rotation));
        yield return StartCoroutine(SlideTo(b.transform, mountA.position, mountA.rotation));

        // Update bookkeeping
        pictureAtIndex[ia] = b;
        pictureAtIndex[ib] = a;
        currentIndexOf[a] = ib;
        currentIndexOf[b] = ia;

        // Check solved
        CheckSolved();
    }

    private IEnumerator SlideTo(Transform t, Vector3 pos, Quaternion rot)
    {
        Vector3 startP = t.position;
        Quaternion startR = t.rotation;
        float tmr = 0f;
        while (tmr < slideTime)
        {
            tmr += Time.deltaTime;
            float u = slideCurve.Evaluate(Mathf.Clamp01(tmr / slideTime));
            t.position = Vector3.Lerp(startP, pos, u);
            t.rotation = Quaternion.Slerp(startR, rot, u);
            yield return null;
        }
        t.position = pos;
        t.rotation = rot;
    }

    private void CheckSolved()
    {
        // pictures list is the CORRECT order; pictureAtIndex is current order
        for (int i = 0; i < pictures.Count; i++)
        {
            if (pictureAtIndex[i] != pictures[i])
                return; // not solved yet
        }

        Debug.Log("✅ Picture order puzzle solved!");
        Solve(); // from PuzzleBase → notifies PuzzleManager
    }
}
