using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    [SerializeField] private GameObject storyPanel1;
    [SerializeField] private GameObject storyPanel2;
    [SerializeField] private GameObject storyPanel3;

    [SerializeField] private GameObject StoryUI;
    [SerializeField] private GameObject ComicsUI;

    [SerializeField] private float displayDuration1 = 3f;
    [SerializeField] private float displayDuration2 = 3f;
    [SerializeField] private float displayDuration3 = 3f;

    private void Start()
    {
        StoryUI.SetActive(true);
        ComicsUI.SetActive(false);
        StartCoroutine(PlayStorySequence());
    }

    private IEnumerator PlayStorySequence()
    {
        storyPanel1.SetActive(true);
        yield return new WaitForSeconds(displayDuration1);
        storyPanel1.SetActive(false);

        storyPanel2.SetActive(true);
        yield return new WaitForSeconds(displayDuration2);
        storyPanel2.SetActive(false);

        storyPanel3.SetActive(true);
        yield return new WaitForSeconds(displayDuration3);
        storyPanel3.SetActive(false);

        StoryUI.SetActive(false);
        ComicsUI.SetActive(true);

       
    }
}
