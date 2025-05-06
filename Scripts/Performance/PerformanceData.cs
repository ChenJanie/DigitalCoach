using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PerformanceData
{
    public Message[] messages;
    public Action[] perfmMethods;



    public PerformanceData(Action[] perfmMethods, params Message[] messages)
    {
        this.messages = messages;
        this.perfmMethods = perfmMethods;
    }

    public PerformanceData(Action perfmMethod, params Message[] messages)
    {
        this.messages = messages;
        this.perfmMethods = new Action[] { perfmMethod};
    }

    public PerformanceData(Message[] messages, params Action[] perfmMethods)
    {
        this.messages = messages;
        this.perfmMethods = perfmMethods;
    }

    public PerformanceData(Message message, params Action[] perfmMethods)
    {
        this.messages = new Message[] { message };
        this.perfmMethods = perfmMethods;
    }

    public PerformanceData(params Message[] messages)
    {
        this.messages = messages;
        this.perfmMethods = null;
    }

    public PerformanceData(params Action[] perfmMethods)
    {
        this.messages = null;
        this.perfmMethods = perfmMethods;
    }
}
