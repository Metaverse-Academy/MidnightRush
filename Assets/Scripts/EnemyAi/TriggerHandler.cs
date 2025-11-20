using UnityEngine;
using UnityEngine.UI; // Required if you are working with UI elements

public class TriggerHandler : MonoBehaviour
{
    public GameObject objectToAppear; // Assign this in the Inspector

    public void Awake()
    {
        objectToAppear.SetActive(false); // Ensure the object is initially invisible
        Debug.Log("Ghost is Invisible");
    }
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("player entered the trigger");
        // Check if the entering object has a specific tag (optional, but recommended)
        if (other.CompareTag("Player")) 
        {
            if (objectToAppear != null)
            {
                objectToAppear.SetActive(true); // Make the object visible
                Debug.Log("Ghost Appeared");

            }
        }


    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("player exited the trigger");
        if (other.CompareTag("Player"))
        {
            if (objectToAppear != null)
            {
                objectToAppear.SetActive(false); // Make the object invisible
                Debug.Log("Ghost is Invisible");
            }
        }
    }
}