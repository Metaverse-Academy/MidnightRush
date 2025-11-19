using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class JumpScareSequence : MonoBehaviour
{
    public RawImage scareImage;
    public Texture2D[] frames;
    public float frameRate = 30.0f;
    public int repeat = 1;
    public Animator camera_shake;

    private AudioSource audioSource;
    private bool hasPlayed = false;

    // Event for when jump scare completes
    public event Action OnJumpScareComplete;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play()
    {
        if (!hasPlayed)
        {
            hasPlayed = true;
            StartCoroutine(PlaySequence());
        }
    }

    private IEnumerator PlaySequence()
    {
        scareImage.gameObject.SetActive(true);
        scareImage.transform.SetAsLastSibling();
        float delay = 1.0f / frameRate;
        
        if (audioSource != null)
            audioSource.Play();
            
        if (camera_shake != null)
            camera_shake.SetTrigger("Shake");
            
        for (int i = 0; i < repeat; i++)
        {
            foreach (Texture2D frame in frames)
            {
                scareImage.texture = frame;
                yield return new WaitForSeconds(delay);
            }
        }

        hasPlayed = false;
        scareImage.gameObject.SetActive(false);

        // Notify that jump scare is complete
        OnJumpScareComplete?.Invoke();
    }

    // Helper method to get the total duration of the jump scare
    public float GetTotalDuration()
    {
        return (frames.Length * repeat) / frameRate;
    }
}