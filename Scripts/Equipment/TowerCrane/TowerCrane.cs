using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class TowerCrane : AbstractEquipment
{
    public enum InstructionType { hoist, trolley, pivot,gearHoist,gearTrolley,gearPivot, walkietalkieCall,startButtonPush, setXYSwingAngle, setZYSwingAngle, hookSwingX, hookSwingY, hookSwingZ };

    public ExpRecorder expRecorder;
    public TaskManager taskManager;

    public int hoistVersion = 0;
    public int trolleyVersion = 0;
    public int pivotVersion = 0;

    public Hoist hoist;
    public Trolley trolley;
    public Pivot pivot;

    public bool isPlayerControlled;
    public Transform tStart;
    public Transform tDes;


    Transform tJib;
    Transform tTrolley;
    Transform tHook;
    Transform tLoad;

    Transform tMainUI;
    Transform tSideViewCamera;
    Transform tTopViewCamera;

    public float maxLoadForce = float.MaxValue;
    public float maxLoadForceFactor = 1.0f;
    public float curLoadForce;
    

    public float maxSafeLoadRate = 0.3f;
    public float maxWarrningLoadRate = 0.5f;

    public bool isForCave;
    private bool hasInitedForCave = false;

    private float hookLoadBottomDist;

    private bool started = false;
    

    public override float PerformInstruction(Instruction instruction)
    {
        //TODO
        switch ((InstructionType)instruction.instructionTypeInt)
        {
            case InstructionType.hoist:
                if(started) hoist.SetControlPara("acc",instruction.parameter);
                break;
            case InstructionType.trolley:
                if (started )trolley.SetControlPara("acc", instruction.parameter);
                break;
            case InstructionType.pivot:
                if (started) pivot.SetControlPara("acc", instruction.parameter);
                break;
            case InstructionType.gearHoist:
                if (started) hoist.SetControlPara("gear", instruction.parameter);
                break;
            case InstructionType.gearTrolley:
                if (started) trolley.SetControlPara("gear", instruction.parameter);
                break;
            case InstructionType.gearPivot:
                if (started) pivot.SetControlPara("gear", instruction.parameter);
                break;
            case InstructionType.walkietalkieCall:
                if (started) walkietalkie.SetControlPara("status", instruction.parameter);
                break;
            case InstructionType.startButtonPush:
                if (instruction.parameter > 0)
                {
                    if (!started)
                    {
                        started = true;
                        expRecorder.RecordTask("SC_0", true);
                    }
                    else
                    {
                        taskManager.curFinishWeight += taskManager.totFinishWeight;
                    }
                }
                break;
            case InstructionType.hookSwingX:
                hoist.SetControlPara("hookSwingX", instruction.parameter);
                break;
            case InstructionType.hookSwingY:
                hoist.SetControlPara("hookSwingY", instruction.parameter);
                break;
            case InstructionType.hookSwingZ:
                hoist.SetControlPara("hookSwingZ", instruction.parameter);
                break;
        }
        return 0;
    }

    // Start is called before the first frame update
    public override void Start()
    {
        
        base.Start();
        
        

    }

    public override void Update()
    {
        base.Update();

        maxLoadForce = hoist.GetMaxLoadForce(trolley.status, maxLoadForceFactor);
        curLoadForce = hoist.GetLoadForce();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    //public void InitForCave()
    //{
    //    Transform tUser = tJib.Find("Frame").Find("User");
    //    tMainUI.parent = tUser;
    //    tMainUI.localEulerAngles = new Vector3(0, 0, 0);
    //    tMainUI.localPosition = new Vector3(0, 1.2f, 0.9f);

    //    tSideViewCamera.gameObject.SetActive(true);
    //    tTopViewCamera.gameObject.SetActive(true);
    //}

    public override void InitWorkTransforms()
    {
        tJib = this.transform.Find("Jib");
        tTrolley = tJib.Find("Trolley");
        tHook = this.transform.Find("Hook");
        tLoad = tHook.Find("Load");

        tMainUI = this.transform.Find("MainUICanvas");
        tSideViewCamera = tJib.Find("TCSideViewCamera");
        tTopViewCamera = tTrolley.Find("HookTopViewCamera");

        Transform tloadBottom = tLoad.Find("LoadBottom");

        hookLoadBottomDist = tHook.position.y - tloadBottom.position.y;
    }
    public override void InitComponents()
    {

        walkietalkie = JsonHelper.ObjFromJson<Walkietalkie>(GetComponentJsonPath("walkietalkie", walkietalkieVersion));
        hoist = JsonHelper.ObjFromJson<Hoist>(GetComponentJsonPath("hoist",hoistVersion));
        trolley = JsonHelper.ObjFromJson<Trolley>(GetComponentJsonPath("trolley", trolleyVersion));
        pivot = JsonHelper.ObjFromJson<Pivot>(GetComponentJsonPath("pivot", pivotVersion));

        walkietalkie.Init(new Transform[] { }, driveMode);
        hoist.Init(new Transform[] { tHook, tTrolley}, driveMode);
        trolley.Init(new Transform[] { tTrolley }, driveMode);
        pivot.Init(new Transform[] { tJib }, driveMode);

        compnents.Add(walkietalkie);
        compnents.Add(hoist);
        compnents.Add(trolley);
        compnents.Add(pivot);



        if (isPlayerControlled)
        {
            List<Tuple<AbstractCompnent, float>> pathToStartPos = FindLoadPath(tStart);


            foreach (Tuple<AbstractCompnent, float> tuple in pathToStartPos)
            {
                AbstractCompnent compnent = tuple.Item1;
                float statusChange = tuple.Item2;
                compnent.UpdateStatus(true, statusChange, isToBeRecorded:false);
            }
            
        }

        List<Tuple<AbstractCompnent, float>> pathToDesPos = FindLoadPath(tDes);
        print(pathToDesPos);


    }

    public List<Tuple<AbstractCompnent, float>> FindLoadPath(Transform tDestination, float minSafeHeight=float.MinValue)
    {
        List<Tuple<AbstractCompnent, float>> loadPath= new List<Tuple<AbstractCompnent, float>>();
        float hoistStatusChange = 0;
        float trolleyStatusChange = 0;
        float pivotStatusChange = 0;

        Transform tTowerCrane = this.transform;

        Vector3 desRelDist = VectorHelper.GetDistance(tDestination, tTowerCrane, tTowerCrane);
       
        Vector3 oriRelDist = VectorHelper.GetDistance(tTrolley.position - new Vector3(0, hoist.status + hookLoadBottomDist, 0), tTowerCrane, tTowerCrane);

        Vector2 desRelLevelDist = new Vector2(desRelDist.x, desRelDist.z);
        Vector2 oriRelLevelDist = new Vector2(oriRelDist.x, oriRelDist.z);

        if (oriRelDist.y < minSafeHeight)
        {

        }

        if(desRelLevelDist.normalized != oriRelLevelDist.normalized)
        {
            pivotStatusChange = Vector2.SignedAngle(desRelLevelDist, oriRelLevelDist);
            loadPath.Add(new Tuple<AbstractCompnent, float>(pivot, pivotStatusChange));
        }

        float desRelLevelRadius = desRelLevelDist.magnitude;
        float oriRelLevelRadius = oriRelLevelDist.magnitude;

        if (desRelLevelRadius != oriRelLevelRadius)
        {
            trolleyStatusChange = desRelLevelRadius - oriRelLevelRadius;
            loadPath.Add(new Tuple<AbstractCompnent, float>(trolley, trolleyStatusChange));
        }

        if (desRelDist.y != oriRelDist.y)
        {
            hoistStatusChange = -(desRelDist.y - oriRelDist.y);
            loadPath.Add(new Tuple<AbstractCompnent, float>(hoist, hoistStatusChange));
        }



        return loadPath;
    }



    public float GetTotalStatusAbsChange()
    {
        float totalChange = 0;

        foreach(AbstractCompnent compnent in new AbstractCompnent[] {hoist, pivot, trolley })
        {
            foreach(Pair<float, float> pair in compnent.behavRecordDict["UpdateStatus"])
            {
                totalChange += pair.Item2;
            }
        }
        return totalChange;
    }

    public float GetLoadRate()
    {
        return Mathf.Min(curLoadForce / maxLoadForce, maxWarrningLoadRate - 0.01f);
    }


    public bool IsNearLoadDes()
    {
        Vector3 dist = VectorHelper.GetDistance(tTrolley, tDes);

        float length = (new Vector2(dist.x, dist.z)).magnitude;

        return length < 10;

        //return true;
    }

    public float IsNearLoadDesF()
    {
        if (IsNearLoadDes()) return 1f;
        else return 0;
    }

    public float GetLoadDesDist()
    {
        return (tLoad.position - tDes.position).magnitude;
    }



}
