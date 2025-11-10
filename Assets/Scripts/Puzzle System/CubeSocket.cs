using UnityEngine;

public class CubeSocket : MonoBehaviour
{
    [SerializeField] private CubePuzzle puzzle; // reference to your CubePuzzle script
    [SerializeField] private int slotIndex; // 0 or 1

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cube")) // tag your letter cubes as "Cube"
        {
            puzzle.PlaceCube(slotIndex, other.gameObject);
            // Optionally snap cube position to socket
            other.transform.position = transform.position;
            other.transform.rotation = transform.rotation;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Cube"))
        {
            puzzle.RemoveCube(other.gameObject);
        }
    }
}
