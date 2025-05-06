using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AbstrackTask;

public class WorkerPeroformance : MonoBehaviour
{
    public Transform PPEStart;
    public Transform PPEEnd;

    private GameObject highlightDangerSparkGO;

    private Animator animator;

    private GameObject PPEGO;


    // Start is called before the first frame update
    void Start()
    {
        animator = this.transform.GetComponent<Animator>();

        highlightDangerSparkGO = this.transform.Find("HighlightDangerSpark").gameObject;


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StopWalk()
    {
        animator.SetBool("isWalking", false);
    }
    public void StartWalkForward()
    {
        animator.SetBool("isWalking", true);
        animator.SetBool("isForward", true);
    }

    public void StartWalkBackward()
    {
        animator.SetBool("isWalking", true);
        animator.SetBool("isForward", false);
    }

    public void GetCrashed()
    {
        animator.SetBool("isCrashed", true);
    }

    public void WearPPE()
    {
        string cablePath = "Prefabs/Cable";
        GameObject cableGO = Instantiate(Resources.Load(cablePath, typeof(GameObject)), this.transform) as GameObject;

        Cable cable = cableGO.GetComponent<Cable>();
        cable.endPoint0=PPEEnd; cable.endPoint1=PPEStart;

        PPEGO = cableGO;

    }

    public void TakeOffPPE()
    {
        Destroy(PPEGO);
    }

    private void OnTriggerEnter(Collider other)
    {
        switch(other.tag)
        {
            case ("WorkerDes"):
                {
                    StopWalk();
                    break;
                }
            case ("Load"):
                {
                    GetCrashed();
                    break;
                }
        }


    }

    public void HighlightDanger()
    {
        highlightDangerSparkGO.SetActive(true);
    }
    public void StopHighlightDanger()
    {
        highlightDangerSparkGO.SetActive(false);
    }


}
