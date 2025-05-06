using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignallerPerformance : MonoBehaviour
{

    public AbstractEquipment target;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        FaceToTarget();
        animator = this.transform.GetComponent<Animator>();
    }


    public void HoistUpGesture()
    {

    }

    public void HoistDownGesture()
    {
        animator.SetBool("isWithGesture", true);
        animator.SetInteger("gestureType", 1);
        animator.SetBool("positiveDirection", true);
    }

    public void TrolleyInGesture()
    {
        animator.SetBool("isWithGesture", true);
        animator.SetInteger("gestureType", 2);
        animator.SetBool("positiveDirection", false);
    }

    public void TrolleyOutGesture()
    {
        animator.SetBool("isWithGesture", true);
        animator.SetInteger("gestureType", 2);
        animator.SetBool("positiveDirection", true);
    }

    public void PivotLeftGesture()
    {
        animator.SetBool("isWithGesture", true);
        animator.SetInteger("gestureType", 3);
        animator.SetBool("positiveDirection", false);
    }

    public void PivotRightGesture()
    {
        animator.SetBool("isWithGesture", true);
        animator.SetInteger("gestureType", 3);
        animator.SetBool("positiveDirection", true);
    }
    public void StopGuesture()
    {
        animator.SetInteger("gestureType", 0);
    }

    public void FaceToTarget()
    {
        Vector3 lookAtTarget=new Vector3(target.transform.position.x, this.transform.position.y, target.transform.position.z);

        this.transform.LookAt(lookAtTarget);
        
    }

}
