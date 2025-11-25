using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    [Header("Story Panels")]
    [SerializeField] private GameObject storyPanel1;
    [SerializeField] private GameObject storyPanel2;
    [SerializeField] private GameObject storyPanel3;

    [Header("UI References")]
    [SerializeField] private GameObject StoryUI;
    [SerializeField] private GameObject ComicsUI;

    [Header("Display Durations")]
    [SerializeField] private float displayDuration1 = 3f;
    [SerializeField] private float displayDuration2 = 3f;
    [SerializeField] private float displayDuration3 = 3f;

    [Header("Background Music")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip backgroundMusic;


    private void Start()
    {
        // تفعيل واجهة الستوري
        StoryUI.SetActive(true);
        ComicsUI.SetActive(false);

        // تشغيل موسيقى الخلفية
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

        StartCoroutine(PlayStorySequence());
    }

    private IEnumerator PlayStorySequence()
    {
        // Panel 1
        storyPanel1.SetActive(true);
        yield return new WaitForSeconds(displayDuration1);
        storyPanel1.SetActive(false);

        // Panel 2
        storyPanel2.SetActive(true);
        yield return new WaitForSeconds(displayDuration2);
        storyPanel2.SetActive(false);

        // Panel 3
        storyPanel3.SetActive(true);
        yield return new WaitForSeconds(displayDuration3);
        storyPanel3.SetActive(false);

        // إخفاء الستوري → إظهار الكوميك
        StoryUI.SetActive(false);
        ComicsUI.SetActive(true);

    }

}
