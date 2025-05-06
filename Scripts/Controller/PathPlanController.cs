using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPlanController : AbstractController
{
    // Start is called before the first frame update
    void Start()
    {
        
    }


    private float[] PathFind()
    {
        return new float[] { };
    }

    // Update is called once per frame
    void Update()
    {
        float[] paras = PathFind();
        //instructions: { hoist, trolley, pivot, setXYSwingAngle, setZYSwingAngle }

        
        target.GetInstruction(0, paras[0]);
        target.GetInstruction(1, paras[1]);

    }
}
