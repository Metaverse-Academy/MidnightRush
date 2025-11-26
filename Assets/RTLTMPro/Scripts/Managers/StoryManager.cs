using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    [Header("Story Panels")]
    [SerializeField] private GameObject storyPanel1;
    [SerializeField] private GameObject storyPanel2;

    [Header("UI References")]
    [SerializeField] private GameObject StoryUI;
    [SerializeField] private GameObject ComicsUI;

    [Header("Display Durations")]
    [SerializeField] private float displayDuration1 = 3f;    
    [SerializeField] private float displayDurationComics = 3f; 
    [SerializeField] private float displayDuration2 = 3f;     

    private void Start()
    {
        StoryUI.SetActive(true);
        ComicsUI.SetActive(false);

        StartCoroutine(StorySequence());
    }

    private IEnumerator StorySequence()
    {
        storyPanel1.SetActive(true);
        yield return new WaitForSeconds(displayDuration1);
        storyPanel1.SetActive(false);

        ComicsUI.SetActive(true);
        yield return new WaitUntil(() => ComicsUI.activeSelf == false);

        storyPanel2.SetActive(true);
        yield return new WaitForSeconds(displayDuration2);
        
        SceneManager.LoadScene("Main Menu");

    }
    
}
