using UnityEngine;

public class TriggerHandler : MonoBehaviour
{
    public GameObject objectToAppear;     // Optional now
    public AudioClip[] audioClip;         // Assign multiple audio clips in Inspector

    public AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Only disable if assigned
        if (objectToAppear != null)
        {
            objectToAppear.SetActive(false);
            Debug.Log("Ghost is Invisible");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("player entered the trigger");

        if (other.CompareTag("Player"))
        {
            // Only show if the object exists
            if (objectToAppear != null)
            {
                objectToAppear.SetActive(true);
                Debug.Log("Ghost Appeared");
            }

            PlayRandomAudio();
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("player exited the trigger");

        if (other.CompareTag("Player"))
        {
            if (objectToAppear != null)
            {
                objectToAppear.SetActive(false);
                Debug.Log("Ghost is Invisible");
            }
        }
    }

    void PlayRandomAudio()
    {
        if (audioClip == null || audioClip.Length == 0)
        {
            Debug.LogWarning("No audio clips assigned!");
            return;
        }

        int randomIndex = Random.Range(0, audioClip.Length);
        AudioClip clipToPlay = audioClip[randomIndex];

        audioSource.PlayOneShot(clipToPlay);
    }
}
