using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class CircleLineRenderer : SingleColorLineRenderer
{
    public int steps=64;

    public float radius = 3;



    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        DrawCircle(steps, radius);
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
    }

    private void DrawCircle(int steps, float radius)
    {
        lineRenderer.positionCount = steps;

        for (int i = 0; i < steps; i++)
        {

            float curRadian = (float)i / steps * 2 * Mathf.PI;

            float x = Mathf.Cos(curRadian) * radius;
            float z = Mathf.Sin(curRadian) * radius;

            Vector3 curPosition = new Vector3(x, 0, z);

            lineRenderer.SetPosition(i, curPosition);

        }
    }
}
