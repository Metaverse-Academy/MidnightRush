using UnityEngine;

public class ComicsManager : MonoBehaviour
{
    [Header("Comic Images")]
    [SerializeField] private GameObject comicImage1;
    [SerializeField] private GameObject comicImage2;
    [SerializeField] private GameObject comicImage3;
    [SerializeField] private GameObject comicImage4;

    [Header("Background Music")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip backgroundMusic;

    private void Start()
    {
        // تشغيل الموسيقى الخاصة بالكوميك
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
