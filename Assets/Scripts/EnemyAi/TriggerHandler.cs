using UnityEngine;

public class TriggerHandler : MonoBehaviour
{
    public GameObject objectToAppear;
    public AudioClip[] audioClip;

    public AudioSource audioSource;

    public Animator ghostAnimator;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Safe ghost handling
        if (objectToAppear != null)
        {
            objectToAppear.SetActive(false);
            ghostAnimator = objectToAppear.GetComponent<Animator>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (objectToAppear != null)
            {
                objectToAppear.SetActive(true);

                // 👉 Play animation if available
                if (ghostAnimator != null)
                    ghostAnimator.SetTrigger("Appear");
            }

            PlayRandomAudio();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (objectToAppear != null)
            {
                objectToAppear.SetActive(false);
            }
        }
    }

    void PlayRandomAudio()
    {
        if (audioClip == null || audioClip.Length == 0) return;

        int randomIndex = Random.Range(0, audioClip.Length);
        audioSource.PlayOneShot(audioClip[randomIndex]);
    }
}
