using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VirtualWalkieTalkie : VirtualButton
{




    protected override void InitAttriDict()
    {
        attriDict.Add("select", 0);
    }

    protected override void PressDownPerformance()
    {
        if (activateSound != null)
        {
            buttonAudioSource.PlayOneShot(activateSound);
        }
        
    }
    protected override void PressUpPerformance()
    {

    }


}
