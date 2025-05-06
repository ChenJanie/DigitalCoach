using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walkietalkie : AbstractCompnent
{

    string message="";


    public override void InitControlPara(AbstractEquipment.DriveMode driveMode)
    {
        switch (driveMode)
        {
            case (AbstractEquipment.DriveMode.demo_0):
                {
                    attributeControlDict.Add("acc", new AttributeControl(AttributeControlMode.ignore, false, null));
                    attributeControlDict.Add("speed", new AttributeControl(AttributeControlMode.ignore, false, null));
                    attributeControlDict.Add("status", new AttributeControl(AttributeControlMode.ignore, false, null));
                    attributeControlDict.Add("gear", new AttributeControl(AttributeControlMode.ignore, false, null));
                    break;
                }
            case (AbstractEquipment.DriveMode.interact_0):
                {
                    attributeControlDict.Add("acc", new AttributeControl(AttributeControlMode.ignore, false, null));
                    attributeControlDict.Add("speed", new AttributeControl(AttributeControlMode.ignore, false, null));
                    attributeControlDict.Add("status", new AttributeControl(AttributeControlMode.proactive, true, GetActualStatusChange));
                    attributeControlDict.Add("gear", new AttributeControl(AttributeControlMode.ignore, false, null));
                    break;
                }
        }
    }

    public void SetMessage(string message)
    {
        this.message = message;
    }

    public float GetStatus_throwAway()
    {
        float temp = status;
        status = 0;
        return temp;
    }
}