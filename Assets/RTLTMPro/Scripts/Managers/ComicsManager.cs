using UnityEngine;
using UnityEngine.SceneManagement;

public class ComicsManager : MonoBehaviour
{
    [Header("Comic Images")]
    [SerializeField] private GameObject comicImage1;
    [SerializeField] private GameObject comicImage2;
    [SerializeField] private GameObject comicImage3;
    [SerializeField] private GameObject comicImage4;
    [SerializeField] private GameObject comicImage5;

    [Header("Background Music")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Timing Settings")]
    [SerializeField] private float switchTime = 3f; 

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName; 

    private GameObject[] images;
    private int currentIndex = 0;
    private float timer = 0f;

    private void Start()
    {
        // تشغيل الموسيقى الخاصة بالكوميك
        
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

      
        images = new GameObject[] { comicImage1, comicImage2, comicImage3, comicImage4 };

       
        ShowOnly(currentIndex);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= switchTime)
        {
            timer = 0f;
            NextImage();
        }
    }

    private void NextImage()
    {
        currentIndex++;

       
        if (currentIndex >= images.Length)
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        ShowOnly(currentIndex);
    }

    private void ShowOnly(int index)
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].SetActive(i == index);
        }
    }
}
