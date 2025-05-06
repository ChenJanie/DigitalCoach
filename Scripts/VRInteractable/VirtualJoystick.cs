using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR.Interaction.Toolkit;

public class NewBehaviourScript : VirtualController
{
    public float xAxis;
    public float yAxis;
    public float maxAngular=45;
    public float xDead = 0.3f;
    public float yDead = 0.3f;


    private Transform stick;

    protected override void InitAttriDict()
    {

        attriDict.Add("x", 0);
        attriDict.Add("y", 0);
        attriDict.Add("active", 0);
    }

    protected override void UpdateAttris()
    {
        float max=Mathf.Sin(maxAngular * Mathf.Deg2Rad);
        float xRaw = Mathf.Sin(stick.localEulerAngles.x * Mathf.Deg2Rad) / max;
        float yRaw = Mathf.Sin(stick.localEulerAngles.z * Mathf.Deg2Rad) / max;
        attriDict["x"] = Mathf.Abs(xRaw)>=xDead ? xRaw : 0;
        attriDict["y"] = Mathf.Abs(yRaw) >=yDead ? yRaw : 0;
        if(xRaw!=0||yRaw!=0)
        {
            Debug.Log($"xRaw: {xRaw}, yRaw: {yRaw}");
        }
        //Debug.Log($"x angle: {stick.localEulerAngles.x}, x axis: {xAxis}, y angle: {stick.localEulerAngles.y}, y axis: {yAxis}, max: {max}");
        //Debug.Log(yAxis);
    }
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();


        stick = transform.Find("Stick");

        ConfigurableJoint joint = stick.GetComponent<ConfigurableJoint>();

        SoftJointLimit lowAngularXLimit = new SoftJointLimit();
        lowAngularXLimit.limit = -maxAngular;
        joint.lowAngularXLimit = lowAngularXLimit;

        SoftJointLimit highAngularXLimit = new SoftJointLimit();
        highAngularXLimit.limit = maxAngular;
        joint.highAngularXLimit = highAngularXLimit;

        SoftJointLimit angularZLimit = new SoftJointLimit();
        angularZLimit.limit = maxAngular;
        joint.angularZLimit = angularZLimit;

        Rigidbody rb = stick.GetComponent<Rigidbody>();
        rb.WakeUp();



        XRGrabInteractable grabbable = stick.GetComponent<XRGrabInteractable>();
        grabbable.activated.AddListener(Activate);
        grabbable.deactivated.AddListener(Deactivate);


    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }


}
