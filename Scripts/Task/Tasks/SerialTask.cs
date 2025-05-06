using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialTask : AbstrackTask
{
    List<AbstrackTask> tasks;

    public SerialTask(List<AbstrackTask> tasks, float finishWeight, float errorWeight, PerformanceData startPD = null, PerformanceData finishPD = null, PerformanceData failPD = null, ExitCode failExitCode=ExitCode.none) : base(finishWeight, errorWeight, startPD, finishPD, failPD, failExitCode)
    {
        this.tasks = tasks;
    }

    public override bool IsFailed()
    {
        if (tasks.Count == 0)
        {
            return false;
        }
        if (tasks[0].IsFailed())
        {
            PDsToSend.AddRange(tasks[0].GetPerformanceData());
            if (finishPerformance != null)
            {
                PDsToSend.Add(failPerformance);
            }
            ECsToSend.AddRange(tasks[0].GetExitCode());
            if (failExitCode!=ExitCode.none)
            {
                ECsToSend.Add(failExitCode);
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public override bool IsFinished()
    {
        duration += Time.deltaTime;
        while (tasks.Count>0 && tasks[0].IsFinished())
        {
            PDsToSend.AddRange(tasks[0].GetPerformanceData());
            ECsToSend.AddRange(tasks[0].GetExitCode());
            finishWeight += tasks[0].finishWeight;
            tasks.RemoveAt(0);
        }
        if (tasks.Count == 0)
        {
            return true;
        }

        return false;
    }

    public override List<PerformanceData> GetPerformanceData()
    {
        if(tasks.Count != 0)
        {
            PDsToSend.AddRange(tasks[0].GetPerformanceData());
        }

        return base.GetPerformanceData();
    }
    public override List<ExitCode> GetExitCode()
    {
        if (tasks.Count!=0)
        {
            ECsToSend.AddRange(tasks[0].GetExitCode());
        }

        return base.GetExitCode();
    }


}
