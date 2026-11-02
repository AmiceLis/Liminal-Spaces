using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public float delayInSeconds = 2f;

    void Start()
    {
        audioSource.PlayDelayed(delayInSeconds); // <-- This line starts the delayed playback
    }

    void Update()
    {
        // Not needed unless you want to trigger audio from player input, etc.
    }
}
