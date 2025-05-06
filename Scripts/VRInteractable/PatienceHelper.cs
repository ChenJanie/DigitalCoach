using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatienceHelper : MonoBehaviour
{
    AudioSource audioSource;
    bool lastIsPlaying;
    public VirtualController controller;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (lastIsPlaying==false && audioSource.isPlaying) 
        //{ 
        //    controller.enabled = false;
        //}
        //if (lastIsPlaying == true && !audioSource.isPlaying)
        //{
        //    controller.enabled = true;
        //}

        //lastIsPlaying=audioSource.isPlaying;
    }
}
