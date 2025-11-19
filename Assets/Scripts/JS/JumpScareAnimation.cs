using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.UIElements;

public class JumpScareAnimation : MonoBehaviour
{
    public Animator camera_shake;
    public UnityEngine.UI.Image puppet;
    private Animator animator;
    private AudioSource audioSource;
    private bool hasPlayed = false;

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    public void Play()
    {
        if (!hasPlayed)
        {
            StartCoroutine(PlaySequence());
        }
    }

    private IEnumerator PlaySequence()
    {
        puppet.gameObject.SetActive(true);
        hasPlayed = true;
        audioSource.Play();
        camera_shake.SetTrigger("Shake");
        animator.SetTrigger("JumpScare");

        yield return null; //Wait for the animator to be updated.

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        yield return new WaitForSeconds(info.length);

        hasPlayed = false;
        puppet.gameObject.SetActive(false);
    }
}