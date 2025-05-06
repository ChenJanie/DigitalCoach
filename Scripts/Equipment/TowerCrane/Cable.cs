using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cable : MonoBehaviour
{
    public Transform endPoint0;
    public Transform endPoint1;
    public float diameter;
    private float length;

    void UpdateCable()
    {

        Vector3 distanceVector = VectorHelper.GetDistance(endPoint0, endPoint1);
        length = distanceVector.magnitude;
        Quaternion rotation = Quaternion.LookRotation(distanceVector, new Vector3(0,1,0));
        this.transform.rotation = rotation;
        this.transform.localScale = new Vector3(diameter, diameter, length);
        this.transform.position = (endPoint0.position + endPoint1.position) * 0.5f;
    }
    // Start is called before the first frame update
    void Start()
    {
        UpdateCable();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCable();
    }
}
