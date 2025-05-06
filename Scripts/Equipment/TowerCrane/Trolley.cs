using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trolley : AbstractCompnent
{
    Transform tTrolley;
    #region Change
    public override float UpdateAcc(bool isWithInputChange = false, float inputChange = 0, bool isToBeRecorded = true)
    {
        float oriAcc = acc;
        float accChange = base.UpdateAcc(isWithInputChange, inputChange, isToBeRecorded);

        if (Mathf.Abs(oriAcc) <= maxAcc[gear] * 0.5 && Mathf.Abs(oriAcc + accChange) > maxAcc[gear] * 0.5)
        {
            bool isPositive = accChange > 0 ? false : true;
            forceSeatManager.StartShock(ForceSeatManager.Attribute.pitch, isPositive, 0.2f, 0.1f);
        }

        if (Mathf.Abs(oriAcc) > maxAcc[gear] * 0.5 && Mathf.Abs(oriAcc + accChange) <= maxAcc[gear] * 0.5)
        {
            bool isPositive = accChange > 0 ? false : true;
            forceSeatManager.StartShock(ForceSeatManager.Attribute.pitch, isPositive, 0.2f, 0.1f);
        }

        return accChange;
    }
    public override float UpdateSpeed(bool isWithInputChange = false, float inputChange = 0, bool isToBeRecorded = true)
    {
        return base.UpdateSpeed(isWithInputChange, inputChange, isToBeRecorded);
    }
    public override float UpdateStatus(bool isWithInputChange = false, float inputChange = 0, bool isToBeRecorded = true)
    {
        float actualChange = base.UpdateStatus(isWithInputChange, inputChange, isToBeRecorded);
        tTrolley.localPosition = tTrolley.localPosition + new Vector3(-actualChange, 0, 0);
        return actualChange;
    }
    public override int UpdateGear(bool isWithInputChange = false, float inputChange = 0, bool isToBeRecorded = true)
    {
        return base.UpdateGear(isWithInputChange, inputChange, isToBeRecorded);
    }
    #endregion

    #region Set
    public override bool SetStatus(float status)
    {
        return base.SetStatus(status);
    }

    #endregion

    #region Get

    public override float GetAccChange_Phy_0(float para)
    {
        return base.GetAccChange_Phy_0(para);
    }
    public override float GetActualAccChange(float change)
    {
        return base.GetActualAccChange(change);
    }
    public override float GetActualSpeedChange(float change)
    {
        return base.GetActualSpeedChange(change);
    }
    public override float GetActualStatusChange(float change)
    {
        return base.GetActualStatusChange(change);
    }
    public override float GetActualGearChange(float change)
    {
        return base.GetActualGearChange(change);
    }

    #endregion

    #region Init
    public override void Init(Transform[] workTransforms, AbstractEquipment.DriveMode driveMode)
    {
        base.Init(workTransforms, driveMode);

        notificationTemplates.Add("countDown", "{0}m");
        notificationTemplates.Add("slow", "{0}m slow");
        notificationTemplates.Add("start", "Trolley {0} {1} m");
        notificationTemplates.Add("end", "Trolley {0} ends");
        notificationTemplates.Add("pAdv", "out");
        notificationTemplates.Add("nAdv", "in");
    }

    public override float InitOriStatus()
    {
        status = -tTrolley.localPosition.x;
        return status;

    }
    public override void InitBehavDict()
    {
        base.InitBehavDict();
    }
    public override void InitWorkTransforms(Transform[] transforms)
    {
        tTrolley = transforms[0];
    }

    public override void InitControlPara(AbstractEquipment.DriveMode driveMode)
    {
        base.InitControlPara(driveMode);
    }
    #endregion

    #region Exec
    public override float Work(AbstractEquipment.DriveMode driveMode)
    {
        return base.Work(driveMode);
    }
    #endregion

    public override void RecordBehav(string behavName, float parameter)
    {
        base.RecordBehav(behavName, parameter);
    }


}
