using UnityEngine;

public class CubePuzzle : MonoBehaviour
{
    [Header("Correct cubes (exact scene instances)")]
    [SerializeField] private GameObject correctA;
    [SerializeField] private GameObject correctB;

    [Header("Slots")]
    [SerializeField] private PlacePoint slot0;
    [SerializeField] private PlacePoint slot1;

    [Header("Optional feedback")]
    [SerializeField] private DoorController doorToOpen; // CHANGED TO DoorController

    private GameObject placed0, placed1;
    private bool solved;
    

    private void OnEnable()
    {
        if (slot0){ slot0.OnPlaced += OnPlaced0; slot0.OnRemoved += OnRemoved0; }
        if (slot1){ slot1.OnPlaced += OnPlaced1; slot1.OnRemoved += OnRemoved1; }
    }

    private void OnDisable()
    {
        if (slot0){ slot0.OnPlaced -= OnPlaced0; slot0.OnRemoved -= OnRemoved0; }
        if (slot1){ slot1.OnPlaced -= OnPlaced1; slot1.OnRemoved -= OnRemoved1; }
    }

    private void OnPlaced0(PlacePoint p, GameObject go){ 
        placed0 = go; 
        Check(); 
    }
    
    private void OnRemoved0(PlacePoint p, GameObject go){ 
        if (placed0 == go) placed0 = null; 
    }
    
    private void OnPlaced1(PlacePoint p, GameObject go){ 
        placed1 = go; 
        Check(); 
    }
    
    private void OnRemoved1(PlacePoint p, GameObject go){ 
        if (placed1 == go) placed1 = null; 
    }

    private void Check()
    {
        if (solved) return;
        if (!placed0 || !placed1) return;


        bool ok = (placed0 == correctA && placed1 == correctB) ||
                  (placed0 == correctB && placed1 == correctA);

        if (ok)
        {
            solved = true;
            
            // Lock the cubes in place
            if (slot0) slot0.AllowTakeBack = false;
            if (slot1) slot1.AllowTakeBack = false;
            
            // Open the door using DoorController
            if (doorToOpen != null)
            {
                doorToOpen.OpenDoor();
            }
            else
            {
            }
        }
        else
        {
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (slot0) 
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(slot0.transform.position, Vector3.one * 0.15f);
        }
        if (slot1) 
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(slot1.transform.position, Vector3.one * 0.15f);
        }
    }
}