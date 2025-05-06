using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceConstrain : MonoBehaviour
{
    ConfigurableJoint configurableJoint;

    public Rigidbody rbConstrained;
    public Rigidbody rbConnectTo;

    public Vector3 lastConnectedToPos;

    Transform tfConstrained;
    Transform tfConnectedTo;

    Vector3 oriPosition;
    Vector3 curPosition;

    public float tension;

    public float t;
    public float limit;

    public float curDistance;

    private Vector3 lastRbConstrainedVel;

    public float limitChangeSpeed;

    public bool flag = false;

    public void SetLimitChangeSpeed(float limitChangeSpeed)
    {
        this.limitChangeSpeed = limitChangeSpeed;
    }

    public void Init(Transform tfConstrained, Transform tfConnectedTo,  float oriLimitChangeSpeed)
    {
        this.tfConstrained = tfConstrained;
        this.tfConnectedTo = tfConnectedTo;

        this.rbConstrained = tfConstrained.GetComponent<Rigidbody>();
        this.rbConnectTo = tfConnectedTo.GetComponent<Rigidbody>();

        this.configurableJoint = tfConstrained.GetComponent<ConfigurableJoint>();

        this.lastRbConstrainedVel = rbConstrained.velocity;
        lastConnectedToPos = rbConnectTo.gameObject.transform.position;

        this.limit = configurableJoint.linearLimit.limit;
        this.curDistance = VectorHelper.GetDistance(tfConnectedTo,tfConstrained).magnitude;

        this.limitChangeSpeed = oriLimitChangeSpeed;
    }

    public void ChangeLimit(float change)
    {
        if (change == 0) return;
        SoftJointLimit softJointLimit = new SoftJointLimit();
        softJointLimit.limit = configurableJoint.linearLimit.limit + change;
        limit = softJointLimit.limit;
        this.GetComponent<ConfigurableJoint>().linearLimit = softJointLimit; ;

        this.GetComponent<Rigidbody>().WakeUp();
    }

    public void SetLimit(float limit)
    {
        
        SoftJointLimit softJointLimit = new SoftJointLimit();
        softJointLimit.limit = limit;
        limit = softJointLimit.limit;
        this.GetComponent<ConfigurableJoint>().linearLimit = softJointLimit; ;

        this.GetComponent<Rigidbody>().WakeUp();
    }

    private void FixedUpdate()
    {
        //if (limitChangeSpeed != 0)
        //{
        //    flag = true;
        //}
        //if (!flag) return;

        Vector3 connectedToVel = (tfConnectedTo.position - lastConnectedToPos)/t;



        Vector3 normalDirection = VectorHelper.GetDistance(tfConstrained, tfConnectedTo).normalized;

        Vector3[] subVels = VectorHelper.OrthogonalDecomposition(rbConstrained.velocity-connectedToVel, normalDirection);

        Vector3 normalVel = subVels[0];
        Vector3 tangentialVel = subVels[1];

        normalVel = normalDirection * limitChangeSpeed;
        rbConstrained.velocity = tangentialVel + normalVel+connectedToVel;
        DebugHelper.Log(DebugHelper.Field.physics, rbConstrained.velocity.ToString());

        Vector3 velChange = rbConstrained.velocity - lastRbConstrainedVel;
        Vector3 momentumChange = (velChange * rbConstrained.mass - Physics.gravity * rbConstrained.mass * t);
        Vector3[] subMomentumChange = VectorHelper.OrthogonalDecomposition(momentumChange, normalDirection);

        tension = subMomentumChange[0].magnitude / t;

        lastRbConstrainedVel = rbConstrained.velocity;
        lastConnectedToPos = tfConnectedTo.position;

    }



    // Start is called before the first frame update
    void Start()
    {
        t = Time.fixedDeltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
