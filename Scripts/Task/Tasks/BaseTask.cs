using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.XR.CoreUtils;

public class BaseTask : AbstrackTask
{

    public Func<float> getParaMethod;
    public float targetPara;
    public bool isIncreasing;
    public bool isClosed;
    public bool isWithNotification;
    public List<Tuple<float, string>> paraMessagePairs;

    public PerformanceManager performanceManager;


    public Dictionary<string, Func<float>> builtInGetMethodDict;

    

    public override bool IsFinished()
    {

        duration += Time.deltaTime;
        float curPara=getParaMethod();

        while (isWithNotification && paraMessagePairs.Count > 0 && ShouldSendMessage())
        {
            PDsToSend.Add( 
                new PerformanceData( 
                    new Message(Message.Source.Signaller, paraMessagePairs[0].Item2, TaskManager.ActivityPeriod.a6)
                    )
                );
            paraMessagePairs.RemoveAt(0);
        }



        bool finished = false;
        if (isClosed)
        {
            if (isIncreasing) finished= curPara >= targetPara;
            else finished = curPara <= targetPara;
        }
        else
        {
            if (isIncreasing) finished = curPara > targetPara;
            else finished = curPara < targetPara;
        }
        if (finished)
        {
            if (finishPerformance != null)
            {
                PDsToSend.Add(finishPerformance);
            }
            ECsToSend.Add(ExitCode.success);
        }
        return finished;
    }

    public bool ShouldSendMessage()
    {
        float curPara = getParaMethod();
        bool sendMessage = false;
        if (isClosed)
        {
            if (isIncreasing) sendMessage = curPara >= paraMessagePairs[0].Item1;
            else sendMessage = curPara <= paraMessagePairs[0].Item1;
        }
        else
        {
            if (isIncreasing) sendMessage = curPara > paraMessagePairs[0].Item1;
            else sendMessage = curPara < paraMessagePairs[0].Item1;
        }
        return sendMessage;
    }

    public override bool IsFailed()
    {
        duration += Time.deltaTime;
        return false;
    }

    public BaseTask(Func<float> getParaMethod, float targetPara, bool isIncreasing, bool isClosed, bool isWithNotification, float finishWeight, float errorWeight, PerformanceData startPD = null, PerformanceData finishPD = null, PerformanceData failPD = null, Dictionary<string,string> notificationTemplates=null, ExitCode failExitCode=ExitCode.none) : base(finishWeight, errorWeight, startPD, finishPD, failPD, failExitCode)
    {
        this.getParaMethod = getParaMethod;
        this.targetPara = targetPara;
        this.isIncreasing = isIncreasing;
        this.isWithNotification = isWithNotification;

        if (isWithNotification)
        {
            paraMessagePairs = new List<Tuple<float, string>>();

            float curPara = getParaMethod();

            float distance = targetPara - curPara;

            float interval;

            float threshold0 = distance > 0 ? 10 : -10;
            float threshold1 = distance > 0 ? 5 : -5;
            if (Math.Abs(distance) >= Math.Abs(threshold0))
            {
                interval = distance > 0 ? 5 : -5;
                List<Tuple<float, string>> tmp = new List<Tuple<float, string>>();
                float para = threshold0;
                while (Math.Abs(para)<Math.Abs(distance))
                {
                    
                    string message = string.Format(notificationTemplates["countDown"], (int)Math.Abs(para));
                    tmp.Add(new Tuple<float, string>(targetPara - para, message));
                    para += interval;
                }

                tmp.Reverse();

                paraMessagePairs.AddRange(tmp);
            }
            if (Math.Abs(distance) > Math.Abs(threshold1))
            {
                interval = distance > 0 ? 2 : -2;
                List<Tuple<float, string>> tmp = new List<Tuple<float, string>>();
                float para = threshold1;
                while (Math.Abs(para) < Math.Min(Math.Abs(threshold0), Math.Abs(distance)))
                {
                    
                    string message = string.Format(notificationTemplates["countDown"], (int)Math.Abs(para));
                    tmp.Add(new Tuple<float, string>(targetPara-para, message));
                    para += interval;
                }

                tmp.Reverse();

                paraMessagePairs.AddRange(tmp);
            }

            {
                interval = distance > 0 ? 1 : -1;
                List<Tuple<float, string>> tmp = new List<Tuple<float, string>>();
                float para = distance > 0 ? 1 : -1;
                while (Math.Abs(para) < Math.Min(Math.Abs(threshold1), Math.Abs(distance)))
                {
                    
                    string message = string.Format(notificationTemplates["slow"], (int)Math.Abs(para));
                    tmp.Add(new Tuple<float, string>(targetPara - para, message));
                    para += interval;
                }

                tmp.Reverse();

                paraMessagePairs.AddRange(tmp);
            }

            

            Debug.Log(paraMessagePairs);
        }



    }





}
