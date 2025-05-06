using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pivot : AbstractCompnent
{
    Transform tJib;

    #region Change
    public override float UpdateStatus(bool isWithInputChange = false, float inputChange = 0, bool isToBeRecorded = true)
    {
        float statusChange = base.UpdateStatus(isWithInputChange, inputChange, isToBeRecorded);
        tJib.localEulerAngles = new Vector3(0, status, 0);

        return statusChange;
    }

    public override float UpdateAcc(bool isWithInputChange = false, float inputChange = 0, bool isToBeRecorded = true)
    {
        float oriAcc = acc;
        float accChange = base.UpdateAcc(isWithInputChange, inputChange, isToBeRecorded);

        if (Mathf.Abs(oriAcc) <= maxAcc[gear]*0.5 && Mathf.Abs(oriAcc + accChange) > maxAcc[gear] * 0.5)
        {
            bool isPositive = accChange > 0 ? false : true;
            forceSeatManager.StartShock(ForceSeatManager.Attribute.yaw, isPositive, 0.2f, 0.1f);
        }

        if (Mathf.Abs(oriAcc) > maxAcc[gear] * 0.5 && Mathf.Abs(oriAcc + accChange) <= maxAcc[gear] * 0.5)
        {
            bool isPositive = accChange > 0 ? false : true;
            forceSeatManager.StartShock(ForceSeatManager.Attribute.yaw, isPositive, 0.2f, 0.1f);
        }

        return accChange;

    }
    #endregion


    #region Get
    public override float GetActualStatusChange(float change)
    {
        if (maxStatus == 180 && minStatus == -180)
        {
            return change;
        }
        else
        {
            return base.GetActualStatusChange(change);
        }
            
    }
    #endregion

    #region Init
    public override void Init(Transform[] workTransforms, AbstractEquipment.DriveMode driveMode)
    {
        base.Init(workTransforms, driveMode);

        notificationTemplates.Add("countDown", "{0} degree");
        notificationTemplates.Add("slow", "{0} degree slow");
        notificationTemplates.Add("start", "Swing {0} {1} degree");
        notificationTemplates.Add("end", "Swing {0} ends");
        notificationTemplates.Add("pAdv", "right");
        notificationTemplates.Add("nAdv", "left");
    }
    public override void InitWorkTransforms(Transform[] transforms)
    {
        tJib = transforms[0];
    }
    public override float InitOriStatus()
    {
        status = tJib.localEulerAngles.y;
        return status;

    }

    #endregion

    #region Exec
    #endregion
}
