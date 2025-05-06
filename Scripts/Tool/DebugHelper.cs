using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugHelper
{
    public enum Field { controller, equipment, component, physics, eyeTrack};
    public static Dictionary<Field, bool> fieldActiveDict = new Dictionary<Field, bool>
    {
        [Field.controller]=true,
        [Field.equipment]=false,
        [Field.component]=false,
        [Field.physics]=false,
        [Field.eyeTrack]=false
    };

    public static void Log(Field field, string mesg)
    {
        if (fieldActiveDict[field])
        {
            Debug.Log(mesg);
        }
    }
}
