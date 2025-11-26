using UnityEngine;
using UnityEngine.SceneManagement;

public class ComicsManager : MonoBehaviour
{
    [Header("Comic Images")]
    [SerializeField] private GameObject comicImage1;
    [SerializeField] private GameObject comicImage2;
    [SerializeField] private GameObject comicImage3;
    [SerializeField] private GameObject comicImage4;

    [Header("Timing Settings")]
    [SerializeField] private float switchTime = 3f;

    // 🔥 الإضافة المطلوبة: مدة كل صورة
    [SerializeField] private float[] imageDurations;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName; 

    private GameObject[] images;
    private int currentIndex = 0;
    private float timer = 0f;

    private void Start()
    {
        images = new GameObject[] { comicImage1, comicImage2, comicImage3, comicImage4 };

        ShowOnly(currentIndex);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // 🔥 الإضافة المطلوبة: استخدم مدة الصورة الحالية بدل switchTime
        if (timer >= imageDurations[currentIndex])
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
            gameObject.transform.parent.gameObject.SetActive(false);
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
