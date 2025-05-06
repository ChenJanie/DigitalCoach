using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalTask : AbstrackTask
{
    AbstrackTask forbiddenTask;

    AbstrackTask task;

    public override bool IsFinished()
    {
        bool finished= task.IsFinished();
        if (finished)
        {
            PDsToSend.AddRange(task.GetPerformanceData());
            if(finishPerformance!=null)
            {
                PDsToSend.Add(finishPerformance);
            }
            ECsToSend.AddRange(task.GetExitCode());
            ECsToSend.Add(ExitCode.success);
        }
        return finished;
    }

    public override bool IsFailed()
    {
        duration += Time.deltaTime;
        bool failed=false;
        if (forbiddenTask.IsFinished())
        {
            PDsToSend.AddRange(forbiddenTask.GetPerformanceData());
            ECsToSend.Add(ExitCode.fSuccess);
            failed = true;
        }
        if (task.IsFailed())
        {
            PDsToSend.AddRange(task.GetPerformanceData());
            ECsToSend.AddRange(task.GetExitCode());
            failed = true;
        }
        if (failed)
        {
            if (failPerformance != null)
            {
                PDsToSend.Add(failPerformance);
            }
            
            if (failExitCode != ExitCode.none)
            {
                ECsToSend.Add(failExitCode);
            }      
        }



        return failed;
    }

    public ConditionalTask(AbstrackTask task, AbstrackTask forbiddenTask,float finishWeight, float errorWeight, PerformanceData startPD = null, PerformanceData finishPD = null, PerformanceData failPD = null, ExitCode failExitCode=ExitCode.none) : base(finishWeight,errorWeight, startPD, finishPD, failPD, failExitCode)
    {
        
        this.task = task;
        this.forbiddenTask = forbiddenTask;

    }

    public override List<PerformanceData> GetPerformanceData()
    {
        PDsToSend.AddRange(task.GetPerformanceData());
        return base.GetPerformanceData();
    }


}
