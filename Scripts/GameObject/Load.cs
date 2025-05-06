using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Load : MonoBehaviour
{
    bool arrived = false;
    bool collided = false;

    float arrivedDur = 0f;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("collision");
        switch (other.tag)
        {
            case ("Des"):
                {
                    arrived = true;
                    break;
                }
            default:
                {
                    collided = true; break;
                }
        }
    }

    private void Update()
    {
        if (arrived)
        {
            arrivedDur += Time.deltaTime;
        }
    }

    public bool IsArrived()
    {
        return arrived;
    }

    public float IsArrivedF()
    {
        return arrived ? 1 : 0;
    }

    public bool IsCollided()
    {
        return collided;
    }

    public float IsCollidedF()
    {
        return collided? 1 : 0;
    }
}
