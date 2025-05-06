using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class MathHelper
{
    public static float GetAbsSum(float[] a)
    {
        float sum=0;
        foreach(float v in a)
        {
            sum += Math.Abs(v);
        }
        return sum;
    }
    public static float GetAbsSum(List<float> a)
    {
        float sum = 0;
        foreach (float v in a)
        {
            sum += Math.Abs(v);
        }
        return sum;
    }

}
