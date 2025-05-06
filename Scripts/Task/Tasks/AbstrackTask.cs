using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstrackTask
{
    public float errorWeight;
    public float finishWeight;
    public ExitCode failExitCode;
    public PerformanceData startPerformance;
    public PerformanceData finishPerformance;
    public PerformanceData failPerformance;

    public enum ExitCode { none, SC_1, SC_2, SC_3, SC_4, SC_5, SC_6, SC_7, SC_8, SC_9, success, timeOut, fSuccess };


    public float duration = 0f;


    public List<PerformanceData> PDsToSend=new List<PerformanceData>();
    public List<ExitCode> ECsToSend=new List<ExitCode>();

    private bool isFirstPerform = true;

    public virtual bool IsFinished()
    {
        return false;
    }

    public virtual bool IsFailed()
    {
        duration += Time.deltaTime;
        return false;
    }

    public virtual List<PerformanceData> GetPerformanceData()
    {
        List<PerformanceData> tmp = new List<PerformanceData>(PDsToSend);
        PDsToSend.Clear();
        return tmp;
    }

    public virtual List<ExitCode> GetExitCode()
    {
        List<ExitCode> tmp = new List<ExitCode>(ECsToSend);
        ECsToSend.Clear();
        return tmp;
    }

    public AbstrackTask(float finishWeight, float errorWeight, PerformanceData startPD=null, PerformanceData finishPD=null, PerformanceData failPD=null, ExitCode failExitCode=ExitCode.none)
    {
        this.finishWeight = finishWeight;
        this.errorWeight = errorWeight;
        this.startPerformance = startPD;
        this.finishPerformance = finishPD;
        this.failPerformance = failPD;
        this.failExitCode = failExitCode;

        if (startPD != null)
        {
            this.PDsToSend.Add(startPerformance);
        }  
    }

    public float GetDuration()
    {
        return duration;
    }
}
