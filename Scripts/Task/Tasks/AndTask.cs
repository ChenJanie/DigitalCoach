using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndTask : AbstrackTask
{

    public List<Pair<AbstrackTask, bool>> taskFinishedPairs;

    public override bool IsFinished()
    {
        for (int i = 0; i < taskFinishedPairs.Count; i++)
        {
            Pair<AbstrackTask, bool> pair = taskFinishedPairs[i];
            if (pair.Item2) continue;
            pair.Item2 = pair.Item1.IsFinished();
            if (pair.Item2)
            {
                PDsToSend.AddRange(pair.Item1.GetPerformanceData());
                ECsToSend.AddRange(pair.Item1.GetExitCode());
            }
            if (pair.Item2) return false;
        }

        ECsToSend.Add(ExitCode.success);
        return true;
    }

    public override bool IsFailed()
    {
        duration += Time.deltaTime;
        for (int i = 0; i<taskFinishedPairs.Count;i++)
        {
            Pair<AbstrackTask, bool> pair = taskFinishedPairs[i];

            if (!pair.Item2 && pair.Item1.IsFailed())
            {
                PDsToSend.AddRange(pair.Item1.GetPerformanceData());
                ECsToSend.AddRange(pair.Item1.GetExitCode());
                if (failExitCode != ExitCode.none)
                {
                    ECsToSend.Add(failExitCode);
                }
                return true;
            }
        }
            return false;
    }

    public AndTask(List<AbstrackTask> tasks, float finishWeight, float errorWeight, PerformanceData startPD = null, PerformanceData finishPD = null, PerformanceData failPD = null, ExitCode failExitCode = ExitCode.none) : base(finishWeight, errorWeight, startPD, finishPD, failPD, failExitCode)
    {
        taskFinishedPairs = new List<Pair<AbstrackTask, bool>>();
        foreach(AbstrackTask task in tasks)
        {
            taskFinishedPairs.Add(new Pair<AbstrackTask, bool>(task, false));
        }
    }

    public override List<PerformanceData> GetPerformanceData()
    {
        for (int i = 0; i < taskFinishedPairs.Count; i++)
        {
            Pair<AbstrackTask, bool> pair = taskFinishedPairs[i];
            if (!pair.Item2)
            {
                PDsToSend.AddRange(pair.Item1.GetPerformanceData());
            }
        }
        return base.GetPerformanceData();
    }
}
