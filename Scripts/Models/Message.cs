using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Message
{
    public enum Source { Operator , Signaller , Hint}

    public Source source;

    public string content;

    public TaskManager.ActivityPeriod activityPeriod;

    public Message(Source source, string content, TaskManager.ActivityPeriod activityPeriod=TaskManager.ActivityPeriod.discuss) 
    {
        this.source = source;
        this.content = content;
        this.activityPeriod = activityPeriod;
    }
}
