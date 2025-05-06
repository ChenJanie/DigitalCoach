using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Data.Common;

public class AbstractCompnent
{
    protected ForceSeatManager forceSeatManager;

    public int gear=1;
    public int maxGear;

    public float status;
    public float maxStatus;
    public float minStatus;

    public float speed;
    public float[] maxSpeed;

    public float acc;
    public float[] maxAcc;

    public float oriSpeed = 0;

    public Dictionary<string, string> notificationTemplates = new Dictionary<string, string>();
    public Pair<float, float> pair;
    public VROperatorController controller;


    private int fixedUpdateCounter = 0;



    public enum AttributeControlMode { ignore , passive, proactive };

    public class AttributeControl
    {
        public AttributeControlMode controlMode;
        public bool throwaway;
        public float para;
        public Func<float,float> getChangeMethod;


        public AttributeControl(AttributeControlMode controlMode, bool throwaway, Func<float,float> getChangeMethod, float initPara=0)
        {
            this.controlMode = controlMode;
            this.throwaway = throwaway;
            this.para = initPara;
            this.getChangeMethod = getChangeMethod;
        }

    }
    //attribute, throwaway, 
    public Dictionary<string, AttributeControl> attributeControlDict=new Dictionary<string, AttributeControl>();


    public string[] recordBehavNames;
    public string[] recordAttriNames;

    private float t = Time.fixedDeltaTime;

    //behavName:(time, value)
    public Dictionary<string, List<Pair<float,float>>> behavRecordDict =new Dictionary<string, List<Pair<float, float>>>();
    public Dictionary<string, List<Pair<float, float>>> attriRecordDict =new Dictionary<string, List<Pair<float, float>>>();
    //public Dictionary<String, float> behavIntervalDict = new Dictionary<string, float>();


    #region Change

    public virtual int UpdateGear(bool isWithInputChange = false, float inputChange = 0, bool isToBeRecorded = true)
    {
        AttributeControl control = attributeControlDict["gear"];
        float gearChange = 0;
        if (isWithInputChange)
        {
            gearChange = GetActualGearChange(inputChange);
        }
        else
        {
            gearChange = GetGearChange();
        }


        if (isToBeRecorded)
        {
            if(gearChange != 0)
            {
                string mesg = $"{this.GetType()}: curGear={gear}, change={gearChange}, nextGear={gear + gearChange}";
                DebugHelper.Log(DebugHelper.Field.component, mesg);
            }
            RecordBehav(GetCurMethodName(), gearChange);
            RecordAttri("Gear", gear + gearChange);
        }

        gear += (int)gearChange;


        if (control.throwaway)
        {
            ClearControlParas(ref control);
        }

        return (int)gearChange;
    }

    public virtual float UpdateAcc(bool isWithInputChange=false, float inputChange=0, bool isToBeRecorded = true)
    {
        AttributeControl control = attributeControlDict["acc"];
        float accChange=0;
        if (isWithInputChange)
        {
            accChange = GetActualAccChange(inputChange);
        }
        else
        {
            accChange = GetAccChange();
        }
        if (isToBeRecorded)
        {
            if(accChange != 0)
            {
                string mesg = $"{this.GetType()}: curAcc={acc}, accChange={accChange}, nextAcc={acc + accChange}";
                DebugHelper.Log(DebugHelper.Field.component, mesg);
            }
            RecordBehav(GetCurMethodName(), accChange);
            RecordAttri("Acc", acc + accChange);
        }
        acc += accChange;

        if (control.throwaway)
        {
            ClearControlParas(ref control);
        }

        return accChange;
    }
    public virtual float UpdateSpeed(bool isWithInputChange = false, float inputChange = 0, bool isToBeRecorded = true)
    {
        AttributeControl control = attributeControlDict["speed"];
        float speedChange = 0;
        if (isWithInputChange)
        {
            speedChange = GetActualSpeedChange(inputChange);
        }
        else
        {
            speedChange = GetSpeedChange();
        }

        //make speed == 0 if min or max status
        if (status == maxStatus && speedChange > 0)
        {
            speedChange = -speed;
        }

        if (status == minStatus && speedChange < 0)
        {
            speedChange = -speed;
        }

        if ( isToBeRecorded)
        {
            if (speedChange != 0)
            {
                string mesg = $"{this.GetType()}: curSpeed={speed}, accChange={speedChange}, nextAcc={speed + speedChange}";
                DebugHelper.Log(DebugHelper.Field.component, mesg);
            }

            RecordBehav(GetCurMethodName(), speedChange);
            RecordAttri("Speed", speed + speedChange);
        }
        speed += speedChange;

        if (control.throwaway)
        {
            ClearControlParas(ref control);
        }

        return speedChange;
    }

    public virtual float UpdateStatus(bool isWithInputChange = false, float inputChange = 0, bool isToBeRecorded=true)
    {
        AttributeControl control = attributeControlDict["status"];
        float statusChange = 0;
        if (isWithInputChange)
        {
            statusChange = GetActualStatusChange(inputChange);
        }
        else
        {
            statusChange = GetStatusChange();
        }

        if ( isToBeRecorded)
        {
            if(statusChange != 0)
            {
                string mesg = $"{this.GetType()}: curStatus={status}, change={statusChange}, nextStatus={status + statusChange}";
                DebugHelper.Log(DebugHelper.Field.component, mesg);
            }

            RecordBehav(GetCurMethodName(), statusChange);
            RecordAttri("Status", status + statusChange);
        }
        status += statusChange;

        if(isToBeRecorded)

        if (control.throwaway)
        {
            ClearControlParas(ref control);
        }

        return statusChange;
    }

    #endregion

    #region Set
    public virtual bool SetControlPara(string attribute, float para)
    {
        attributeControlDict[attribute].para = para;


        return false;
    }

    public virtual bool SetStatus(float status)
    {
        UpdateStatus(true, status - this.status);
        if(status != this.status)
        {
            Debug.Log("Set Status Failed");
        }

        return status==this.status;
    }

    public virtual void ClearControlParas(ref AttributeControl control)
    {
        control.para = 0;
    }
    #endregion

    #region Get

    public float GetGearChange()
    {
        AttributeControl control = attributeControlDict["gear"];
        float gearChange = 0;
        switch (control.controlMode)
        {
            case (AttributeControlMode.ignore):
                {
                    gearChange = 0;
                    break;
                }
            case (AttributeControlMode.passive):
                {
                    gearChange = GetActualGearChange(0);
                    break;
                }
            case (AttributeControlMode.proactive):
                {
                    gearChange = (int)control.getChangeMethod(control.para);
                    break;
                }
        }

        return gearChange;

    }

    public float GetAccChange()
    {
        AttributeControl control = attributeControlDict["acc"];
        float accChange = 0;
        switch (control.controlMode)
        {
            case (AttributeControlMode.ignore):
                {
                    accChange = 0;
                    break;
                }
            case (AttributeControlMode.passive):
                {
                    accChange = GetActualAccChange(0);
                    break;
                }
            case (AttributeControlMode.proactive):
                {
                    accChange = control.getChangeMethod(control.para);
                    break;
                }
        }
        return accChange;
    }

    public float GetSpeedChange()
    {
        AttributeControl control = attributeControlDict["speed"];
        float speedChange = 0;
        switch (control.controlMode)
        {
            case (AttributeControlMode.ignore):
                {
                    speedChange = 0;
                    break;
                }
            case (AttributeControlMode.passive):
                {
                    speedChange = GetActualSpeedChange(acc * t);
                    break;
                }
            case (AttributeControlMode.proactive):
                {
                    speedChange = control.getChangeMethod(control.para);
                    break;
                }
        }
        return speedChange;
    }

    public float GetStatusChange()
    {
        AttributeControl control = attributeControlDict["status"];
        float statusChange = 0;
        switch (control.controlMode)
        {
            case (AttributeControlMode.ignore):
                {
                    statusChange = 0;
                    break;
                }
            case (AttributeControlMode.passive):
                {
                    statusChange = GetActualStatusChange((speed + oriSpeed) * t / 2);
                    break;
                }
            case (AttributeControlMode.proactive):
                {
                    statusChange = control.getChangeMethod(control.para);
                    break;
                }
        }
        return statusChange;
    }



    public virtual float GetActualGearChange(float change)
    {
        if (change == 0f)
        {
            return 0f;
        }
        else if (change > 0)
        {
            change = 1;
        }
        else if (change < 0)
        {
            change = -1;
        }
        int nextGear = gear + (int)change;
        if (nextGear > maxGear)
        {
            nextGear = maxGear;
        }
        else if (nextGear <= 0)
        {
            nextGear = 1;
        }

        int actualChange = nextGear - gear;
        return actualChange;
    }

    public virtual float GetActualAccChange(float change)
    {
        float nextAcc = acc + change;
        if (Math.Abs(nextAcc) > maxAcc[gear - 1])
        {
            nextAcc = nextAcc > 0 ? maxAcc[gear - 1] : -maxAcc[gear - 1];
        }
        return nextAcc - acc;
    }

    public virtual float GetActualSpeedChange(float change)
    {
        float nextSpeed = speed + change;


        if (Math.Abs(nextSpeed) > maxSpeed[gear - 1])
        {
            nextSpeed = nextSpeed > 0 ? maxSpeed[gear - 1] : -maxSpeed[gear - 1];
        }

        float actualChange = nextSpeed - speed;
        return actualChange;
    }
    public virtual float GetActualStatusChange(float change)
    {
        float nextStatus = status + change;

        if (nextStatus > maxStatus)
        {
            nextStatus = maxStatus;
        }
        else if (nextStatus < minStatus)
        {
            nextStatus = minStatus;
        }
        float actualChange = nextStatus - status;
        return actualChange;
    }



    /// <summary>
    /// acc = (para - curSpeed/maxSpeed) * maxAcc
    /// </summary>
    /// <returns>acc</returns>
    /// <param name="para">axis value from input</param>
    public virtual float GetAccChange_Phy_0(float para)
    {
        float targetSpeed = para * maxSpeed[gear - 1];
        float nextAcc = (targetSpeed - speed) / maxSpeed[gear - 1] * maxAcc[gear - 1];
        float accChange = nextAcc - acc;
        if (nextAcc != 0)
        {
            string mesg = $"{this.GetType()}: accChange={accChange} according to parameter={para}";
            DebugHelper.Log(DebugHelper.Field.component, mesg);
        }
        return accChange;
    }






    public virtual string GetCurMethodName()
    {
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        return stackTrace.GetFrame(1).GetMethod().Name;
    }


    public int GetGear()
    {
        return gear;
    }

    public float GetGearF()
    {
        return (float)gear;
    }

    public float GetAcc()
    {
        return acc;
    }

    public float GetSpeed()
    {
        return speed;
    }

    public float GetStatus()
    {
        return status;
    }


    //public float GetStatusAbsChange()
    //{
    //    float change = 0;
    //    foreach (Tuple<float, float> tuple in behavRecordDict["UpdateStatus"])
    //    {
    //        change += Math.Abs(tuple.Item1);
    //    }
    //    return change;

    //}

    #endregion

    #region Init
    public virtual void Init(Transform[] workTransforms, AbstractEquipment.DriveMode driveMode)
    {
        InitWorkTransforms(workTransforms);
        status = InitOriStatus();
        InitBehavDict();
        InitAttriDict();
        InitControlPara(driveMode);
        forceSeatManager = GameObject.Find("ForceSeatManager").GetComponent<ForceSeatManager>();
    }
    public virtual void InitBehavDict()
    {
        foreach(string behavName in recordBehavNames)
        {
            behavRecordDict.Add(behavName, new List<Pair<float, float>>());
            //behavIntervalDict.Add(behavName, 0);
        } 
    }
    public virtual void InitAttriDict()
    {
        foreach (string attriName in recordAttriNames)
        {
            attriRecordDict.Add(attriName, new List<Pair<float, float>>());
        }
    }

    public virtual void InitWorkTransforms(Transform[] transforms)
    {

    }
    public virtual float InitOriStatus()
    {
        return 0;
    }
    public virtual void InitControlPara(AbstractEquipment.DriveMode driveMode)
    {
        switch (driveMode)
        {
            case (AbstractEquipment.DriveMode.demo_0):
                {
                    attributeControlDict.Add("acc", new AttributeControl(AttributeControlMode.ignore, false, null));
                    attributeControlDict.Add("speed", new AttributeControl(AttributeControlMode.ignore, false, null));
                    attributeControlDict.Add("status", new AttributeControl(AttributeControlMode.proactive, true, GetActualStatusChange));
                    attributeControlDict.Add("gear", new AttributeControl(AttributeControlMode.ignore, false, null));
                    break;
                }
            case (AbstractEquipment.DriveMode.interact_0):
                {
                    attributeControlDict.Add("acc", new AttributeControl(AttributeControlMode.proactive, false, GetAccChange_Phy_0));
                    attributeControlDict.Add("speed", new AttributeControl(AttributeControlMode.passive, false, null));
                    attributeControlDict.Add("status", new AttributeControl(AttributeControlMode.passive, false, null));
                    attributeControlDict.Add("gear", new AttributeControl(AttributeControlMode.proactive, true, GetActualGearChange));
                    break;
                }
        }
    }
    #endregion


    #region Exec
    public virtual float Work(AbstractEquipment.DriveMode driveMode)
    {
        //foreach(string behavName in behavNames)
        //{
        //    behavIntervalDict[behavName] += t;
        //}
        UpdateGear();
        UpdateAcc();
        UpdateSpeed();
        UpdateStatus();
        oriSpeed = speed;

        fixedUpdateCounter++;

        return 0;
    }
    #endregion

    public virtual void RecordBehav(string behavName, float value)
    {
        //float interval = behavIntervalDict[behavName];
        //behavRecordDict[behavName].Add(new Tuple<float, float>(parameter,interval));
        //behavIntervalDict[behavName] = 0;

        behavRecordDict[behavName].Add(new Pair<float, float>(Time.fixedDeltaTime*fixedUpdateCounter, value));
    }

    public virtual void RecordAttri(string attriName, float value) 
    {
        attriRecordDict[attriName].Add(new Pair<float, float>(Time.fixedDeltaTime * fixedUpdateCounter, value));
    }



}
