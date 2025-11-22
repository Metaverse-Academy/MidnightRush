using UnityEngine;

public class TriggerHandler : MonoBehaviour
{
    public GameObject objectToAppear;     // Assign in Inspector
    public AudioClip[] audioClip;         // Assign multiple audio clips in Inspector

    private AudioSource audioSource;      // Will store the AudioSource component

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            // If the GameObject has no AudioSource, add one automatically
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        objectToAppear.SetActive(false);
        Debug.Log("Ghost is Invisible");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("player entered the trigger");

        if (other.CompareTag("Player"))
        {
            if (objectToAppear != null)
            {
                objectToAppear.SetActive(true);
                Debug.Log("Ghost Appeared");
            }

            PlayRandomAudio();   // 🔥 Play a random sound
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

    // 🔥 NEW FUNCTION — Plays a random audio clip
    void PlayRandomAudio()
    {
        if (audioClip.Length == 0)
        {
            Debug.LogWarning("No audio clips assigned!");
            return;
        }

        int randomIndex = Random.Range(0, audioClip.Length);
        AudioClip clipToPlay = audioClip[randomIndex];

        audioSource.PlayOneShot(clipToPlay);
    }
}
