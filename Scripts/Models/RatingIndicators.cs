using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatingIndicators
{
    public float totFinishWeight;
    public float curFinishWeight;
    public float totTime;
    public float maxTime;


    public RatingIndicators(float totFinishWeight, float curFinishWeight, float totTime, float maxTime)
    {
        this.totFinishWeight = totFinishWeight;
        this.curFinishWeight = curFinishWeight;
        this.totTime = totTime;
        this.maxTime = maxTime; 
    }
}
