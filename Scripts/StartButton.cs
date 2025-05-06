using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class StartButton : MonoBehaviour
{

    public ExpRecorder expRecorder;
    public TaskManager taskManager;
    public AudioClip pushSound;
    
    public List<VirtualController> virtualControllers = new List<VirtualController>();

    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        XRSimpleInteractable simpleInteractable = GetComponent<XRSimpleInteractable>();
        simpleInteractable.selectEntered.AddListener(StartTask);
        simpleInteractable.selectEntered.AddListener(EndTask);
        simpleInteractable.selectEntered.AddListener(RecordTouch);
        simpleInteractable.selectEntered.AddListener(EnableVirtualCOntrollers);
        simpleInteractable.selectEntered.AddListener(PlayPushSound);


        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

        XRSimpleInteractable simpleInteractable = GetComponent<XRSimpleInteractable>();

        simpleInteractable.selectEntered.Invoke(null);
    }

    public void RecordTouch(SelectEnterEventArgs arg)
    {
        expRecorder.RecordTask("SC_0", true);
    }
    public void EnableVirtualCOntrollers(SelectEnterEventArgs arg)
    {
        foreach (var controller in virtualControllers)
        {
            controller.enabled = true;
        }
    }

    public void EndTask(SelectEnterEventArgs arg)
    {
        if (virtualControllers[0].enabled==true)
        {
            taskManager.curFinishWeight += taskManager.totFinishWeight;
        }
    }

    public void StartTask(SelectEnterEventArgs arg)
    {
        if (virtualControllers[0].enabled == false)
        {
            taskManager.curFinishWeight += 1;
        }
    }

    public void PlayPushSound(SelectEnterEventArgs arg)
    {
        audioSource.PlayOneShot(pushSound);
    }


}
