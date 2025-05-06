using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VirtualButton : VirtualController
{
    public float activateCD = 0.3f;
    public AudioClip activateSound;
    protected float lastActivate = 0;
    public bool isPositive=true;


    protected Transform buttonMeshTF;
    protected AudioSource buttonAudioSource;
    protected override void InitAttriDict()
    {
        attriDict.Add("select", 0);
    }

    protected override void Start()
    {
        base.Start();

        buttonMeshTF = this.transform.Find("Mesh");
        XRSimpleInteractable simpleInteractable = buttonMeshTF.GetComponent<XRSimpleInteractable>();
        buttonAudioSource = buttonMeshTF.GetComponent<AudioSource>();
        simpleInteractable.selectEntered.AddListener(SelectEnter);
        simpleInteractable.selectExited.AddListener(SelectExit);
    }
    protected override void Update()
    {
        base.Update();

        lastActivate+=Time.deltaTime;
    }

    protected virtual void PressDownPerformance()
    {
        buttonMeshTF.localScale = new Vector3(1, 0.6f, 1);

        if(activateSound!= null)
        {
            buttonAudioSource.PlayOneShot(activateSound);
        }
        
    }

    protected virtual void PressUpPerformance()
    {
        buttonMeshTF.localScale = new Vector3(1, 1, 1);
    }


    protected override void SelectEnter(SelectEnterEventArgs arg)
    {
        if (lastActivate > activateCD)
        {
            attriDict["select"] = isPositive ? 1 : -1;
            PressDownPerformance();
        }

    }

    protected override void SelectExit(SelectExitEventArgs arg)
    {
        attriDict["select"] = 0;
        PressUpPerformance();
    }

}
