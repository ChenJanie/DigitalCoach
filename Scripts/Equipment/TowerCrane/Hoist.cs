using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Hoist : AbstractCompnent
{
    private DistanceConstrain distanceConstrain;

    private Transform tHook;
    private Transform tTrolley;

    private float[] SwingAngles = new float[] { 0, 0 };

    public float[] maxLoadFormula;


    #region Change
    public override float UpdateSpeed(bool isWithInputChange = false, float inputChange = 0, bool isToBeRecorded = true)
    {
        float actualChange =base.UpdateSpeed(isWithInputChange, inputChange, isToBeRecorded);
        distanceConstrain.SetLimitChangeSpeed(speed);
        return actualChange;
    }
    public override float UpdateStatus(bool isWithInputChange = false, float inputChange = 0, bool isToBeRecorded = true)
    {
        float actualChange = base.UpdateStatus(isWithInputChange, inputChange, isToBeRecorded);
        distanceConstrain.ChangeLimit(actualChange);

        if (isWithInputChange)
        {
            tHook.localPosition =tTrolley.position+ new Vector3(0, -distanceConstrain.limit, 0);
        }
        return actualChange;
    }
    public override float UpdateAcc(bool isWithInputChange = false, float inputChange = 0, bool isToBeRecorded = true)
    {
        float oriAcc = acc;
        float accChange = base.UpdateAcc(isWithInputChange, inputChange, isToBeRecorded);

        if(Mathf.Abs(oriAcc)<= maxAcc[gear-1] * 0.5 && Mathf.Abs(oriAcc+accChange)> maxAcc[gear-1] * 0.5)
        {
            bool isPositive = accChange > 0 ? false : true;
            forceSeatManager.StartShock(ForceSeatManager.Attribute.pitch, isPositive, 0.2f, 0.1f);
        }

        if(Mathf.Abs(oriAcc) > maxAcc[gear-1] * 0.5 && Mathf.Abs(oriAcc+accChange) <= maxAcc[gear-1] * 0.5)
        {
            bool isPositive = accChange > 0 ? false : true;
            forceSeatManager.StartShock(ForceSeatManager.Attribute.pitch, isPositive, 0.2f, 0.1f);
        }
       
        return accChange;
    }
    public Vector3 UpdateHookSwing(bool isWithInputChange = false, Vector3 inputChange=default, bool isToBeRecorded = true)
    {
        AttributeControl control = attributeControlDict["load"];
        Vector3 posChange;
        if (isWithInputChange)
        {
            posChange = inputChange;
        }
        else
        {
            posChange = GetHookPosSwingChange();
        }

        if (posChange != default && isToBeRecorded)
        {

            string mesg = $"{this.GetType()}: curHookPos={tHook.localPosition.ToString()}, change={posChange.ToString()}, nextPos={tHook.localPosition+posChange}";
            DebugHelper.Log(DebugHelper.Field.component, mesg);
        }

        if (control.throwaway)
        {
            ClearControlParas(ref control);
        }

        return posChange;
    }

    #endregion

    #region Set
    public override bool SetStatus(float status)
    {
        return base.SetStatus(status);
    }

    public void SetSwingAngle(string panel, float angle)
    {
        if (panel == "XY")
        {
            SwingAngles[0] = angle;
        }
        else if (panel == "YZ")
        {
            SwingAngles[1] = angle;
        }
    }
    #endregion

    #region Get
    public Vector3 GetHookPosSwingChange()
    {
        AttributeControl controlX = attributeControlDict["hookSwingX"];
        AttributeControl controlY = attributeControlDict["hookSwingY"];
        AttributeControl controlZ = attributeControlDict["hookSwingZ"];
        Vector3 posChange = default;
        switch (controlX.controlMode)
        {
            case (AttributeControlMode.ignore):
                {
                    break;
                }
            case (AttributeControlMode.passive):
                {
                    break;
                }
            case (AttributeControlMode.proactive):
                {
                    float posChangeX = controlX.getChangeMethod(controlX.para);
                    float posChangeY = controlX.getChangeMethod(controlY.para);
                    float posChangeZ = controlX.getChangeMethod(controlZ.para);
                    posChange =new Vector3(posChangeX, posChangeY, posChangeZ);
                    break;
                }
        }
        return posChange;
    }
    public float GetMaxLoadForce(float trolleyDistance, float factor=1f)
    {
        float maxLoad=0;
        for(int i = 0; i < maxLoadFormula.Length; i++)
        {
            maxLoad += (float) Math.Pow(trolleyDistance, i) * maxLoadFormula[i];
        }
        return Math.Abs(maxLoad * 1000 * Physics.gravity.y)*factor;
    }
    public float GetLoadForce()
    {
        return distanceConstrain.tension;
    }
    #endregion

    #region Init
    public override void Init(Transform[] workTransforms, AbstractEquipment.DriveMode driveMode)
    {
        base.Init(workTransforms, driveMode);


        switch (attributeControlDict["hookSwingX"].controlMode)
        {
            case AbstractCompnent.AttributeControlMode.passive:
                {
                    distanceConstrain = this.tHook.GetComponents<DistanceConstrain>()[0];
                    distanceConstrain.Init(tHook, tTrolley, speed);
                    distanceConstrain.SetLimit(status);

                    break;
                }
            case AbstractCompnent.AttributeControlMode.proactive:
                {

                    break;
                }
        }  


        notificationTemplates.Add("countDown", "{0}m");
        notificationTemplates.Add("slow", "{0}m slow");
        notificationTemplates.Add("start", "Hoist {0} {1} m");
        notificationTemplates.Add("end", "Hoist {0} ends");
        notificationTemplates.Add("pAdv", "down");
        notificationTemplates.Add("nAdv", "up");
    }
    public override void InitWorkTransforms(Transform[] transforms)
    {
        tHook = transforms[0];
        tTrolley = transforms[1];
    }

    public override float InitOriStatus()
    {
        status = tTrolley.position.y - tHook.position.y;
        return status;
    }
    public override void InitControlPara(AbstractEquipment.DriveMode driveMode)
    {
        base.InitControlPara(driveMode);

        switch (driveMode)
        {
            case (AbstractEquipment.DriveMode.demo_0):
                {
                    attributeControlDict.Add("hookSwingX", new AttributeControl(AttributeControlMode.proactive, false, (float x) => { return x; }));
                    attributeControlDict.Add("hookSwingY", new AttributeControl(AttributeControlMode.proactive, false, (float x) => { return x; }));
                    attributeControlDict.Add("hookSwingZ", new AttributeControl(AttributeControlMode.proactive, false, (float x) => { return x; }));
                    break;
                }
            case (AbstractEquipment.DriveMode.interact_0):
                {
                    attributeControlDict.Add("hookSwingX", new AttributeControl(AttributeControlMode.passive, false, null));
                    attributeControlDict.Add("hookSwingY", new AttributeControl(AttributeControlMode.passive, false, null));
                    attributeControlDict.Add("hookSwingZ", new AttributeControl(AttributeControlMode.passive, false, null));
                    break;
                }
        }
    }

    #endregion

    #region Exec
    public override float Work(AbstractEquipment.DriveMode driveMode)
    {
        float result= base.Work(driveMode);

        tHook.localEulerAngles = tTrolley.parent.localEulerAngles;

        return result;
    }


    #endregion

}
