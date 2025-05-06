using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrTask : AbstrackTask
{
    Dictionary<AbstrackTask, bool> taskToFailedDict;

    public override bool IsFinished()
    {
        foreach(AbstrackTask task in taskToFailedDict.Keys)
        {
            if (!taskToFailedDict[task] && task.IsFinished()) 
            {
                PDsToSend.AddRange(task.GetPerformanceData());
                ECsToSend.AddRange(task.GetExitCode());
                ECsToSend.Add(ExitCode.success);
                return true;
            } 
        }

        return false;
    }

    public override bool IsFailed()
    {
        duration += Time.deltaTime;
        foreach (AbstrackTask task in taskToFailedDict.Keys)
        {
            if (taskToFailedDict[task]) continue;
            taskToFailedDict[task] = task.IsFailed();
            if (taskToFailedDict.ContainsKey(task))
            {
                PDsToSend.AddRange(task.GetPerformanceData());
                ECsToSend.AddRange(task.GetExitCode());
            }
            if (!taskToFailedDict[task]) return false;
        }

        if(failExitCode!=ExitCode.none) {
            ECsToSend.Add(failExitCode);
        }
        
        return true;
    }

    public override List<PerformanceData> GetPerformanceData()
    {
        foreach (AbstrackTask abstrackTask in taskToFailedDict.Keys)
        {
            if (!taskToFailedDict[abstrackTask])
            {
                PDsToSend.AddRange(abstrackTask.GetPerformanceData());
            }
        }
        return null;
    }

    public OrTask(List<AbstrackTask> tasks, float finishWeight, float errorWeight, PerformanceData startPD = null, PerformanceData finishPD = null, PerformanceData failPD = null, ExitCode failExitCode=ExitCode.none) : base(finishWeight, errorWeight, startPD, finishPD, failPD, failExitCode)
    {
        taskToFailedDict = new Dictionary<AbstrackTask, bool>();
        foreach(AbstrackTask task in tasks)
        {
            taskToFailedDict.Add(task, false);
        }
    }
}
