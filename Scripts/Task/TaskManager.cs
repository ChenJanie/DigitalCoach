using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using static AbstrackTask;
using Unity.VisualScripting;

public class TaskManager : MonoBehaviour
{
    private System.Random random = new System.Random();
    public enum ExpMode { rTrain, cTrain, practice, test, debug};
    public enum ActivityPeriod { discuss, a1, a2, a3, a4, a5, a6, a7 };


    public ExpMode expMode=ExpMode.rTrain;

    public float maxDuration=5*60+20;

    public float maxErrorWeight = 100;
    public float unitErrorWeight = 1;
    public float curErrorWeight = 0;
    public float totFinishWeight = 7;
    public float curFinishWeight = 0;

    public float totWindDur = 15;
    public float beforeWindDur = 5;
    public float curWindDur = 0;
    public float maxWindForce = 500;
    public Vector3 windDir = new Vector3(0,0,0);

    public bool isWithWind = true;
    public bool isWithHint = true;

    public float commonErrorWeight = 1f;



    public AbstractEquipment target;

    public Load load;
    public Transform tHook;

    public List<AbstrackTask> tasks;

    public Dictionary<AbstrackTask, PerformanceData[]> taskToPerformance;

    public PerformanceManager performanceManager;

    public ExpRecorder recorder;

    public float timeLimitConfusing = 20f;

    public Camera topViewCamera;

    public ExpRecorder expRecorder;




    private List<AbstrackTask.ExitCode> exitCodes= new List<AbstrackTask.ExitCode>();


    private bool isNearTarget=false;

    private SerialTask unloadGuide=null;
    private SerialTask prepareTask=null;


    private List<AbstrackTask.ExitCode> ecToRecord=new List<AbstrackTask.ExitCode>() { AbstrackTask.ExitCode.SC_1, AbstrackTask.ExitCode.SC_2, AbstrackTask.ExitCode.SC_3, AbstrackTask.ExitCode.SC_4, AbstrackTask.ExitCode.SC_5, AbstrackTask.ExitCode.SC_6, AbstrackTask.ExitCode.SC_7, AbstrackTask.ExitCode.SC_8, AbstrackTask.ExitCode.SC_9 };


    public enum TaskReturn { processing, finish, fail, timeOut};

    private void Update()
    {
        for(int i=0; i < tasks.Count; i++)
        {
            if (tasks[i].IsFailed())
            {
                List<PerformanceData> failPD = tasks[i].GetPerformanceData();

                List<AbstrackTask.ExitCode> ecs= tasks[i].GetExitCode();
                foreach (AbstrackTask.ExitCode exitCode in ecs)
                {
                    if (ecToRecord.Contains(exitCode))
                    {
                        expRecorder.RecordTask(exitCode.ToString(), false);
                    }
                }
                exitCodes.AddRange(ecs);

                performanceManager.Perform(failPD);

                curErrorWeight += tasks[i].errorWeight;
                curFinishWeight += tasks[i].finishWeight;

                

                RemoveTaskAt(ref i);
                continue;

            }

            if (tasks[i].IsFinished())
            {
                List<PerformanceData> finishPD = tasks[i].GetPerformanceData();
                performanceManager.Perform(finishPD);

                curFinishWeight += tasks[i].finishWeight;

                RemoveTaskAt(ref i);
                continue;
            }

            List<PerformanceData> performanceData = tasks[i].GetPerformanceData();
            if (performanceData != null&&performanceData.Count!=0)
            {
                performanceManager.Perform(performanceData);
            }

            if (curErrorWeight >= maxErrorWeight || curFinishWeight >= totFinishWeight)
            {
                if (curFinishWeight >= totFinishWeight)
                {
                    expRecorder.RecordTask("success", true);
                }
                else if (curFinishWeight >= maxErrorWeight)
                {
                    expRecorder.RecordTask("fail", true);
                }
                CheckOut();
            }
        }

        if(target is TowerCrane)
        {
            TowerCrane towerCrane = (TowerCrane)target;

            if ( target.walkietalkie.GetStatus_throwAway() > 0 && isNearTarget)
            {
                if (unloadGuide != null)
                {
                    tasks.Remove(unloadGuide);
                }
                float finishWeight= totFinishWeight==3? 1f : 0f;
                unloadGuide = MakeConfusingUnloadTask(timeLimitConfusing, finishWeight, commonErrorWeight,isWithHint:isWithHint);
                tasks.Add(unloadGuide);
            }

            if (!tasks.Contains(prepareTask)&& curWindDur<totWindDur+beforeWindDur && isWithWind){
                curWindDur += Time.deltaTime;

                
                if(curWindDur > beforeWindDur) {
                    float windForce = (float)random.NextDouble() * (float)random.NextDouble() * maxWindForce;
                    Debug.Log("having wind");
                    tHook.gameObject.GetComponent<Rigidbody>().AddForce(windDir*windForce);

                    performanceManager.ForceSeatWind();
                }

                if(curWindDur >= totWindDur + beforeWindDur)
                {
                    performanceManager.ForceSeatWork();
                }

            }

            if (towerCrane.IsNearLoadDes() && !isNearTarget)
            {
                isNearTarget = true;
                if(isWithHint) performanceManager.HighlightWalkietalkie();
                expRecorder.RecordTask("a4", true);
            }
        }




    }

    private void RemoveTaskAt(ref int i)
    {
        tasks.RemoveAt(i);
        i--;
    }

    private void Start()
    {
        if (GlobalVariables.ContainKey("expMode"))
        {
            expMode = GlobalVariables.Get<ExpMode>("expMode");
        }
        
        Debug.Log("cur expMode:"+expMode.ToString());

        tasks = new List<AbstrackTask>();


        if (windDir.magnitude == 0)
        {
            windDir = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()).normalized;
        }


        switch (expMode)
        {
            
            case(ExpMode.practice):
                {
                    isWithHint = false;
                    InitPracticeMode();
                    break;
                }
            case(ExpMode.rTrain):
                {
                    isWithHint = true;
                    InitRTrainMode();
                    break;
                }
            case(ExpMode.test):
                {
                    isWithHint = false;
                    InitTestMode();
                    break;
                }
            case (ExpMode.cTrain):
                {
                    isWithHint = true;
                    InitCTrainMode();
                    break;
                }
            default:
                {
                    break;
                }

        }

    }

    private void TestMakeConfusingUnloadTask()
    {
        SerialTask confusingTask = MakeConfusingUnloadTask(5f, 0, commonErrorWeight,0);
        tasks.Add(confusingTask);
    }

    private void InitRTrainMode()
    {
        TowerCrane towerCrane = (TowerCrane)target;

        commonErrorWeight = 1;

        BaseTask maxDurTask = new BaseTask(() => Time.timeSinceLevelLoad, maxDuration, true, true, false, 0, commonErrorWeight);

        //start make prepare task
        PerformanceData sc1_2FailPD = new PerformanceData(performanceManager.WorkerStruckByCraneRig);


        PerformanceData preparetask_1_call_startPD = new PerformanceData(
            new Message(Message.Source.Hint, "Use the walkie-talkie to notify workers to stay away from the load"),
            performanceManager.HighlightWalkietalkie
            );
        PerformanceData preparetask_1_call_finishPD = new PerformanceData(
            new Action[]
            {
                performanceManager.StopHighlightWalkietalkie,
                performanceManager.ForceSeatWork,
                () =>
                {
                    performanceManager.RecordTask("SC_1",true);
                }
            },
            
            new Message(Message.Source.Operator, "The crane operation is about to start."),
            new Message(Message.Source.Signaller, "Okay, I will use walkietalkie to give you lifting instructions. ")
            );

        ConditionalTask preparetask_1_call = new ConditionalTask(MakeCallSignallerTask_towercrane(), MakeAnyMoveTask(new int[] { 1, 2, 3 }), 1, commonErrorWeight, preparetask_1_call_startPD, preparetask_1_call_finishPD, sc1_2FailPD, AbstrackTask.ExitCode.SC_1);



        PerformanceData prepareTask_2_keep3mAway_startPD = new PerformanceData(
            new Action[]
            {
                performanceManager.WorkerWithPPEKeepAway,
                performanceManager.ActivateDistanceWarning,
                performanceManager.HighlightDangerousWorker,
                performanceManager.HighlightWalkietalkie,
                () =>
                {
                    performanceManager.RecordTask("SC_2", true);
                }

            },
            new Message(Message.Source.Signaller, "Keep 3m away from the loads being lifted."),
            new Message(Message.Source.Hint, "Someone is not wearing PPE and is too close to the load, use the walkie-talkie to notify him")
            );
        PerformanceData prepareTask_2_keep3mAway_finishPD = new PerformanceData(
            new Action[]
            {
                performanceManager.WorkerWithoutPPEWearPPEAndKeepAway,   
                performanceManager.DeactivateDistanceWarning,
                performanceManager.StopHighlightDangerousWorker,
                performanceManager.StopHighlightWalkietalkie,


            },
            new Message(Message.Source.Operator, "Someone is not wearing PPE"),
            new Message(Message.Source.Signaller, "Get it")
            );

        ConditionalTask prepareTask_2_keep3mAway = new ConditionalTask(MakeCallSignallerTask_towercrane(), MakeAnyMoveTask(new int[] { 1, 2, 3 }), 1, commonErrorWeight, prepareTask_2_keep3mAway_startPD, prepareTask_2_keep3mAway_finishPD, sc1_2FailPD, AbstrackTask.ExitCode.SC_2);



        BaseTask lift3dmTask = MakeComponentStatusChangeForTask(-0.3f, towerCrane.hoist, false, 0, commonErrorWeight);
        PerformanceData prepareTask_3_lift3dm_startPD = new PerformanceData(
            performanceManager.HighlightRightJoystick,
             new Message(Message.Source.Signaller, "Lift up the loads 300mm from ground")
            );
        PerformanceData prepareTask_3_lift3dm_finishPD = new PerformanceData(
            performanceManager.StopHighlightRightJoystick,
            new Message(Message.Source.Signaller, "Lifting up ends")
            );
        ConditionalTask prepareTask_3_lift3dm = new ConditionalTask(lift3dmTask, MakeAnyMoveTask(new int[] { 2, 3 }), 0, commonErrorWeight, prepareTask_3_lift3dm_startPD, prepareTask_3_lift3dm_finishPD, sc1_2FailPD, AbstrackTask.ExitCode.SC_3);



        PerformanceData prepareTask_4_wait3s_startPD = new PerformanceData(
            new Message(Message.Source.Signaller, "Wait for 3 seconds for stabilizing the loads")
            );
        PerformanceData prepareTask_4_wait3s_finishPD = new PerformanceData(            
            new Message(Message.Source.Signaller, "Stably control the load and navigate it to the landing location. Keep an eye on the load"),
            () =>
            {
                performanceManager.RecordTask("SC_3", true);
            },
            performanceManager.AllWorkerTakeOffPPE
            );
        AbstrackTask prepareTask_4_wait3s = MakeWaitTask(3f, 1, commonErrorWeight, prepareTask_4_wait3s_startPD, prepareTask_4_wait3s_finishPD, sc1_2FailPD, AbstrackTask.ExitCode.SC_3);




        prepareTask = new SerialTask(new List<AbstrackTask> { preparetask_1_call, prepareTask_2_keep3mAway, prepareTask_3_lift3dm, prepareTask_4_wait3s }, 0, commonErrorWeight);




        BaseTask unstableTask = new BaseTask(towerCrane.GetLoadRate, towerCrane.maxWarrningLoadRate, true, true, false, 0, commonErrorWeight);
        ConditionalTask keepStableTask = new ConditionalTask(new BlankTask(0, commonErrorWeight), unstableTask, 1, commonErrorWeight, failExitCode: AbstrackTask.ExitCode.SC_4);


        BaseTask unloadTask = new BaseTask(load.IsArrivedF, 0.5f, true, true, false, 1, commonErrorWeight);



        BaseTask makeCollisionTask = new BaseTask(load.IsCollidedF, 0.5f, true, true, false, 0, 0);
        ConditionalTask avoidCollisionTask = new ConditionalTask(new BlankTask(0, 0), makeCollisionTask, 0, maxErrorWeight, failExitCode: ExitCode.SC_4);


        BaseTask closeToDesTask = new BaseTask(towerCrane.GetLoadDesDist, 3f, false, true, false, 0, 0);
        BaseTask haveGuidanceTask = new BaseTask(() => { return unloadGuide == null ? 0f : 1f; }, 0.5f, true, false, false, 0, 0);
        PerformanceData askForGuidanceFinishPD = new PerformanceData(() => { performanceManager.RecordTask("SC_5", true); });
        ConditionalTask askForGuidanceTask = new ConditionalTask(haveGuidanceTask, closeToDesTask, 1, commonErrorWeight, failExitCode: ExitCode.SC_5, finishPD: askForGuidanceFinishPD);


        tasks.Add(prepareTask);
        tasks.Add(keepStableTask);
        tasks.Add(unloadTask);
        tasks.Add(avoidCollisionTask);
        tasks.Add(askForGuidanceTask);

        Debug.Log(tasks.Count);

    }

    private void InitCTrainMode()
    {
        TowerCrane towerCrane = (TowerCrane)target;

        commonErrorWeight = 0;

        BaseTask maxDurTask = new BaseTask(() => Time.timeSinceLevelLoad, maxDuration, true, true, false, 0, commonErrorWeight);

        //start make prepare task
        PerformanceData sc1_2FailPD = new PerformanceData(performanceManager.WorkerStruckByCraneRig);


        PerformanceData preparetask_1_call_startPD = new PerformanceData(
            new Message(Message.Source.Hint, "Use the walkie-talkie to notify workers to stay away from the load"),
            performanceManager.HighlightWalkietalkie
            );
        PerformanceData preparetask_1_call_finishPD = new PerformanceData(
            new Action[]
            {
                performanceManager.StopHighlightWalkietalkie,
                performanceManager.ForceSeatWork,
                 () =>
                {
                    performanceManager.RecordTask("SC_1",true);
                }
            },
            new Message(Message.Source.Operator, "The crane operation is about to start."),
            new Message(Message.Source.Signaller, "Okay, I will use walkietalkie to give you lifting instructions. ")
            );

        ConditionalTask preparetask_1_call = new ConditionalTask(MakeCallSignallerTask_towercrane(), new BlankTask(0, commonErrorWeight), 1, commonErrorWeight, preparetask_1_call_startPD, preparetask_1_call_finishPD, sc1_2FailPD, AbstrackTask.ExitCode.SC_1);



        PerformanceData prepareTask_2_keep3mAway_startPD = new PerformanceData(
            new Action[]
            {
                performanceManager.WorkerWithPPEKeepAway,
                performanceManager.ActivateDistanceWarning,
                performanceManager.HighlightDangerousWorker,
                performanceManager.HighlightWalkietalkie,
                () =>
                {
                    performanceManager.RecordTask("SC_2", true);
                }

            },
            new Message(Message.Source.Signaller, "Keep 3m away from the loads being lifted."),
            new Message(Message.Source.Hint, "Someone is not wearing PPE and is too close to the load, use the walkie-talkie to notify him")
            );
        PerformanceData prepareTask_2_keep3mAway_finishPD = new PerformanceData(
            new Action[]
            {
                performanceManager.WorkerWithoutPPEWearPPEAndKeepAway,
                performanceManager.DeactivateDistanceWarning,
                performanceManager.StopHighlightDangerousWorker,
                performanceManager.StopHighlightWalkietalkie

            },
            new Message(Message.Source.Operator, "Someone is not wearing PPE"),
            new Message(Message.Source.Signaller, "Get it")
            );

        ConditionalTask prepareTask_2_keep3mAway = new ConditionalTask(MakeCallSignallerTask_towercrane(), new BlankTask(0, commonErrorWeight), 1, commonErrorWeight, prepareTask_2_keep3mAway_startPD, prepareTask_2_keep3mAway_finishPD, sc1_2FailPD, AbstrackTask.ExitCode.SC_2);



        BaseTask lift3dmTask = MakeComponentStatusChangeForTask(-0.3f, towerCrane.hoist, false, 0, commonErrorWeight);
        PerformanceData prepareTask_3_lift3dm_startPD = new PerformanceData(
            performanceManager.HighlightRightJoystick,
             new Message(Message.Source.Signaller, "Lift up the loads 300mm from ground")
            );
        PerformanceData prepareTask_3_lift3dm_finishPD = new PerformanceData(
            performanceManager.StopHighlightRightJoystick,
            new Message(Message.Source.Signaller, "Lifting up ends")
            );
        ConditionalTask prepareTask_3_lift3dm = new ConditionalTask(lift3dmTask, new BlankTask(0, commonErrorWeight), 0, commonErrorWeight, prepareTask_3_lift3dm_startPD, prepareTask_3_lift3dm_finishPD, sc1_2FailPD, AbstrackTask.ExitCode.SC_3);



        PerformanceData prepareTask_4_wait3s_startPD = new PerformanceData(
            new Message(Message.Source.Signaller, "Wait for 3 seconds for stabilizing the loads")
            );
        PerformanceData prepareTask_4_wait3s_finishPD = new PerformanceData(
            new Message(Message.Source.Signaller, "Stably control the load and navigate it to the landing location. Keep an eye on the load"),
            () =>
            {
                performanceManager.RecordTask("SC_3", true);
            },
            performanceManager.AllWorkerTakeOffPPE
            );
        BaseTask prepareTask_4_wait3s = MakeTimerTask(3f, 1, commonErrorWeight, prepareTask_4_wait3s_startPD, prepareTask_4_wait3s_finishPD, sc1_2FailPD, AbstrackTask.ExitCode.SC_3);



        prepareTask = new SerialTask(new List<AbstrackTask> { preparetask_1_call, prepareTask_2_keep3mAway, prepareTask_3_lift3dm, prepareTask_4_wait3s }, 0, commonErrorWeight);




        BaseTask unstableTask = new BaseTask(towerCrane.GetLoadRate, towerCrane.maxWarrningLoadRate, true, true, false, 0, commonErrorWeight);
        ConditionalTask keepStableTask = new ConditionalTask(new BlankTask(0, commonErrorWeight), unstableTask, 1, commonErrorWeight, failExitCode: AbstrackTask.ExitCode.SC_4);



        BaseTask unloadTask = new BaseTask(load.IsArrivedF, 0.5f, true, true, false, 1, commonErrorWeight);



        BaseTask makeCollisionTask = new BaseTask(load.IsCollidedF, 0.5f, true, true, false, 0, 0);
        ConditionalTask avoidCollisionTask = new ConditionalTask(new BlankTask(0, 0), makeCollisionTask, 0, maxErrorWeight, failExitCode: ExitCode.SC_4);


        BaseTask closeToDesTask = new BaseTask(towerCrane.GetLoadDesDist, 3f, false, true, false, 0, 0);
        BaseTask haveGuidanceTask = new BaseTask(() => { return unloadGuide == null ? 0f : 1f; }, 0.5f, true, false, false, 0, 0);
        PerformanceData askForGuidanceFinishPD = new PerformanceData(() => { performanceManager.RecordTask("SC_5", true); });
        ConditionalTask askForGuidanceTask = new ConditionalTask(haveGuidanceTask, new BlankTask(0, commonErrorWeight), 1, commonErrorWeight, failExitCode: ExitCode.SC_5, finishPD: askForGuidanceFinishPD);


        tasks.Add(prepareTask);
        tasks.Add(keepStableTask);
        tasks.Add(unloadTask);
        tasks.Add(avoidCollisionTask);
        tasks.Add(askForGuidanceTask);

        Debug.Log(tasks.Count);

    }

    private void InitTestMode()
    {
        TowerCrane towerCrane = (TowerCrane)target;

        commonErrorWeight = 1;

        BaseTask maxDurTask = new BaseTask(() => Time.timeSinceLevelLoad, maxDuration, true, true, false, 0, commonErrorWeight);

        //start make prepare task
        PerformanceData sc1_2FailPD = new PerformanceData(performanceManager.WorkerStruckByCraneRig);


        PerformanceData preparetask_1_call_finishPD = new PerformanceData(
            new Action[]
            {
                performanceManager.StopHighlightWalkietalkie,
                performanceManager.ForceSeatWork,
                () =>
                {
                    performanceManager.RecordTask("SC_1",true);
                }
            },
            new Message(Message.Source.Operator, "The crane operation is about to start.", ActivityPeriod.a2),
            new Message(Message.Source.Signaller, "Okay, I will use walkietalkie to give you lifting instructions.",ActivityPeriod.a2)
            );

        ConditionalTask preparetask_1_call = new ConditionalTask(MakeCallSignallerTask_towercrane(), MakeAnyMoveTask(new int[] { 1, 2, 3 }), 1, commonErrorWeight, null, preparetask_1_call_finishPD, sc1_2FailPD, AbstrackTask.ExitCode.SC_1);



        PerformanceData prepareTask_2_keep3mAway_startPD = new PerformanceData(
            new Action[]
            {
                performanceManager.WorkerWithPPEKeepAway,

            },
            new Message(Message.Source.Signaller, "Keep 3m away from the loads being lifted.", ActivityPeriod.a3)
            );
        PerformanceData prepareTask_2_keep3mAway_finishPD = new PerformanceData(
            new Action[]
            {
                performanceManager.WorkerWithoutPPEWearPPEAndKeepAway,
                () =>
                {
                    performanceManager.RecordTask("SC_2",true);
                }

            },
            new Message(Message.Source.Operator, "Someone is not wearing PPE", ActivityPeriod.a3),
            new Message(Message.Source.Signaller, "Get it", ActivityPeriod.a3)
            );

        ConditionalTask prepareTask_2_keep3mAway = new ConditionalTask(MakeCallSignallerTask_towercrane(), MakeAnyMoveTask(new int[] { 1, 2, 3 }), 1, commonErrorWeight, prepareTask_2_keep3mAway_startPD, prepareTask_2_keep3mAway_finishPD, sc1_2FailPD, AbstrackTask.ExitCode.SC_2);



        BaseTask lift3dmTask = MakeComponentStatusChangeForTask(-0.3f, towerCrane.hoist, false, 0, commonErrorWeight);
        PerformanceData prepareTask_3_lift3dm_startPD = new PerformanceData(
             new Message(Message.Source.Signaller, "Lift up the loads 300mm from ground", ActivityPeriod.a3)
            );
        PerformanceData prepareTask_3_lift3dm_finishPD = new PerformanceData(
            new Message(Message.Source.Signaller, "Lifting up ends", ActivityPeriod.a3)
            );
        ConditionalTask prepareTask_3_lift3dm = new ConditionalTask(lift3dmTask, MakeAnyMoveTask(new int[] { 2, 3 }), 0, commonErrorWeight, prepareTask_3_lift3dm_startPD, prepareTask_3_lift3dm_finishPD, sc1_2FailPD, AbstrackTask.ExitCode.SC_3);



        PerformanceData prepareTask_4_wait3s_startPD = new PerformanceData(
            new Message(Message.Source.Signaller, "Wait for 3 seconds for stabilizing the loads", ActivityPeriod.a3)
            );
        PerformanceData prepareTask_4_wait3s_finishPD = new PerformanceData(
            new Action[]
            {
                performanceManager.AllWorkerTakeOffPPE,
                () =>
                {
                    performanceManager.RecordTask("SC_3",true);
                }
            },
            
            new Message(Message.Source.Signaller, "Stably control the load and navigate it to the landing location. Keep an eye on the load", ActivityPeriod.a4)
            );
        AbstrackTask prepareTask_4_wait3s = MakeWaitTask(3f, 1, commonErrorWeight, prepareTask_4_wait3s_startPD, prepareTask_4_wait3s_finishPD, sc1_2FailPD, AbstrackTask.ExitCode.SC_3);




        prepareTask = new SerialTask(new List<AbstrackTask> { preparetask_1_call, prepareTask_2_keep3mAway, prepareTask_3_lift3dm, prepareTask_4_wait3s }, 0, commonErrorWeight);




        BaseTask unstableTask = new BaseTask(towerCrane.GetLoadRate, towerCrane.maxWarrningLoadRate, true, true, false, 0, commonErrorWeight);
        ConditionalTask keepStableTask = new ConditionalTask(new BlankTask(0, commonErrorWeight), unstableTask, 1, commonErrorWeight, failExitCode: AbstrackTask.ExitCode.SC_4);



        BaseTask unloadTask = new BaseTask(load.IsArrivedF, 0.5f, true, true, false, 1, commonErrorWeight);



        BaseTask makeCollisionTask = new BaseTask(load.IsCollidedF, 0.5f, true, true, false, 0, 0);
        ConditionalTask avoidCollisionTask = new ConditionalTask(new BlankTask(0, 0), makeCollisionTask, 0, maxErrorWeight, failExitCode: ExitCode.SC_4);


        BaseTask closeToDesTask = new BaseTask(towerCrane.GetLoadDesDist, 3f, false, true, false, 0, 0);
        BaseTask haveGuidanceTask = new BaseTask(() => { return unloadGuide == null ? 0f : 1f; }, 0.5f, true, false, false, 0, 0);
        PerformanceData askForGuidanceFinishPD = new PerformanceData(() => { performanceManager.RecordTask("SC_5", true); });
        ConditionalTask askForGuidanceTask = new ConditionalTask(haveGuidanceTask, closeToDesTask, 1, commonErrorWeight, failExitCode: ExitCode.SC_5, finishPD: askForGuidanceFinishPD);


        tasks.Add(prepareTask);
        tasks.Add(keepStableTask);
        tasks.Add(unloadTask);
        tasks.Add(avoidCollisionTask);
        tasks.Add(askForGuidanceTask);

        Debug.Log(tasks.Count);

        LayerMask layerMask = topViewCamera.cullingMask;
        layerMask &= ~(1 << LayerMask.NameToLayer("DesMark"));
        topViewCamera.cullingMask = layerMask;
    }

    private void InitPracticeMode()
    {
        performanceManager.ForceSeatWork();
        TowerCrane towerCrane = (TowerCrane)target;

        commonErrorWeight = 0;

        BaseTask maxDurTask = new BaseTask(() => Time.timeSinceLevelLoad, maxDuration, true, true, false, 0, commonErrorWeight);



        BaseTask unstableTask = new BaseTask(towerCrane.GetLoadRate, towerCrane.maxWarrningLoadRate, true, true, false, 0, commonErrorWeight);
        ConditionalTask keepStableTask = new ConditionalTask(new BlankTask(0, commonErrorWeight), unstableTask, 1, commonErrorWeight, failExitCode: AbstrackTask.ExitCode.SC_4);



        BaseTask unloadTask = new BaseTask(load.IsArrivedF, 0.5f, true, true, false, 1, commonErrorWeight);



        BaseTask makeCollisionTask = new BaseTask(load.IsCollidedF, 0.5f, true, true, false, 0, 0);
        ConditionalTask avoidCollisionTask = new ConditionalTask(new BlankTask(0, 0), makeCollisionTask, 0, maxErrorWeight, failExitCode: ExitCode.SC_4);

        tasks.Add(keepStableTask);
        tasks.Add(unloadTask);
        tasks.Add(avoidCollisionTask);

        Debug.Log(tasks.Count);
    }

    private BaseTask MakeCallSignallerTask_towercrane(float errorWeight=0, float finishWeight=0, PerformanceData startPD = null, PerformanceData finishPD = null, PerformanceData failPD = null, ExitCode failExitCode = ExitCode.none)
    {
        TowerCrane towerCrane=(TowerCrane) target;
        BaseTask task = new BaseTask(towerCrane.walkietalkie.GetStatus_throwAway, 0, true, false,false, 0,errorWeight,startPD,finishPD,failPD);
        return task;
    }

    private BaseTask MakeAnyMoveTask(int[] compnentIndices, float finishWeight=0, float errorWeight=0)
    {


        Func<float> getParaMethod = () =>
        {
            float totalChange = 0;
            foreach (int compnentIndex in compnentIndices)
            {
                AbstractCompnent compnent = target.compnents[compnentIndex];
                foreach (Pair<float, float> pair in compnent.behavRecordDict["UpdateStatus"])
                {
                    totalChange += Math.Abs(pair.Item2*10000);
                }
            }
            return totalChange;
        };

        BaseTask task = new BaseTask(getParaMethod, 0, true, false,false, finishWeight, errorWeight);
        return task;
    }

    private BaseTask MakeComponentStatusChangeForTask(float change, AbstractCompnent compnent, bool isWithNotification, float finishWeight, float errorWeight)
    {
        float targetPara = compnent.GetStatus() + change;
        bool isIncreasing = change > 0;

        return new BaseTask(compnent.GetStatus, targetPara, isIncreasing, true, isWithNotification, finishWeight, errorWeight);
    }

    private ConditionalTask MakeWaitTask(float duration, float finishWeight, float errorWeight = 0, PerformanceData startPD = null, PerformanceData finishPD = null, PerformanceData failPD = null, AbstrackTask.ExitCode failExitCode=AbstrackTask.ExitCode.none)
    {
        //float targetPara = duration;
        //BaseTask timerTask = new BaseTask(()=> { return 0; }, targetPara, true, true,false, 0, errorWeight);
        //timerTask.getParaMethod = timerTask.GetDuration;

        BaseTask timerTask = MakeTimerTask(duration, finishWeight, errorWeight);    

        ConditionalTask waitTask = new ConditionalTask(timerTask, MakeAnyMoveTask(new int[] {  2, 3 }), finishWeight,0, startPD, finishPD, failPD, failExitCode);

        return waitTask;

    }

    private BaseTask MakeTimerTask(float duration, float finishWeight=0, float errorWeight = 0, PerformanceData startPD = null, PerformanceData finishPD = null, PerformanceData failPD = null, AbstrackTask.ExitCode failExitCode = AbstrackTask.ExitCode.none)
    {
        BaseTask timerTask = new BaseTask(null, duration, true, true, false, finishWeight, errorWeight, startPD, finishPD, failPD);
        timerTask.getParaMethod = timerTask.GetDuration;


        return timerTask;
    }

    public SerialTask MakeUnloadTask(float finishWeight, float errorWeight)
    {
        List<AbstrackTask> subTasks= new List<AbstrackTask>();
        TowerCrane towerCrane = (TowerCrane)target;
        List<Tuple<AbstractCompnent, float>> loadPath = towerCrane.FindLoadPath(towerCrane.tDes);
        foreach(Tuple<AbstractCompnent, float> tuple in loadPath)
        {
            AbstractCompnent compnent = tuple.Item1;
            float statusChange = tuple.Item2;
            if (Math.Abs(statusChange)<0.1) continue;
            Dictionary<string, string> notificationTemplate = compnent.notificationTemplates;
            float targetPara = compnent.GetStatus() + statusChange;
            bool isIncreasing = (statusChange > 0);

            string adv = statusChange > 0 ? notificationTemplate["pAdv"] : notificationTemplate["nAdv"];
            string startMessage = String.Format(compnent.notificationTemplates["start"], adv, Math.Abs(statusChange).ToString("0"));
            PerformanceData startPD = new PerformanceData(
                new Message(Message.Source.Signaller, startMessage),
                performanceManager.GetComponentStatusChangeGestureMethod(compnent, statusChange)
                );

            string finishMessage = String.Format(compnent.notificationTemplates["end"], adv);
            PerformanceData finishPD = new PerformanceData(
                new Message(Message.Source.Signaller, finishMessage)
                );

            BaseTask subTask = new BaseTask(compnent.GetStatus, targetPara, isIncreasing, true, true, 0, 0,startPD, finishPD, null, notificationTemplates: compnent.notificationTemplates);
            subTasks.Add(subTask);
        }
        return new SerialTask(subTasks, finishWeight, errorWeight);
    }

    private SerialTask MakeConfusingUnloadTask(float timeLimit, float finishWeight, float errorWeight, int confusingIndex=-1, bool isWithHint=false)
    {
        List<AbstrackTask> subTasks = new List<AbstrackTask>();
        TowerCrane towerCrane = (TowerCrane)target;
        List<Tuple<AbstractCompnent, float>> loadPath = towerCrane.FindLoadPath(towerCrane.tDes);

        confusingIndex=confusingIndex==-1?random.Next(loadPath.Count) : confusingIndex;

        for(int i = 0; i < loadPath.Count; i++)
        {
            Tuple<AbstractCompnent, float> tuple = loadPath[i];
            AbstractCompnent compnent = tuple.Item1;
            float statusChange = tuple.Item2;
            if (Math.Abs(statusChange) < 0.1) continue;
            Dictionary<string, string> notificationTemplate = compnent.notificationTemplates;
            float targetPara = compnent.GetStatus() + statusChange;
            bool isIncreasing = (statusChange > 0);

            string adv = statusChange > 0 ? notificationTemplate["pAdv"] : notificationTemplate["nAdv"];
            string confusingAdv = statusChange < 0 ? notificationTemplate["pAdv"] : notificationTemplate["nAdv"];
            string startMessage = String.Format(compnent.notificationTemplates["start"], adv, Math.Abs(statusChange).ToString("0"));
            if (i == confusingIndex)
            {
                
                PerformanceData startPD;
                if (isWithHint)
                {
                    startPD = new PerformanceData(
                    performanceManager.GetComponentStatusChangeGestureMethod(compnent, -statusChange),
                    new Message(Message.Source.Signaller, startMessage),
                    new Message(Message.Source.Hint, "The signaller's gesture doesn't match his instruction, use the walkie-talkie to inform him")
                    );
                }
                else
                {
                    startPD = new PerformanceData(
                    performanceManager.GetComponentStatusChangeGestureMethod(compnent, -statusChange),
                    new Message(Message.Source.Signaller, startMessage, ActivityPeriod.a6)
                    );
                }


                string finishMessage = String.Format(compnent.notificationTemplates["end"], adv);
                PerformanceData finishPD = new PerformanceData(
                    new Message(Message.Source.Signaller, finishMessage, ActivityPeriod.a6)
                    );


                PerformanceData callFinishPD = new PerformanceData(
                    () => { performanceManager.RecordTask("SC_6", true); },
                    new Message(Message.Source.Operator, "The gesture is confusing. I cannot get it.", ActivityPeriod.a6),

                    new Message(Message.Source.Signaller, "Sorry, I made the wrong gesture.", ActivityPeriod.a6)
                    );
                BaseTask callTask = MakeCallSignallerTask_towercrane(finishPD: callFinishPD);


                BaseTask timerTask = MakeTimerTask(timeLimit, 0, 0);

                ConditionalTask subTask = new ConditionalTask(callTask, timerTask, 0, 0, startPD, failExitCode: ExitCode.SC_6);

                subTasks.Add(subTask);


                PerformanceData guideStartPD = new PerformanceData(
                    new Message(Message.Source.Signaller, startMessage, ActivityPeriod.a6),
                    performanceManager.GetComponentStatusChangeGestureMethod(compnent, statusChange)
                    );
                BaseTask guideTask = new BaseTask(compnent.GetStatus, targetPara, isIncreasing, true, true, 0, 0, guideStartPD, finishPD, null, notificationTemplates: compnent.notificationTemplates);

                subTasks.Add(guideTask);


                BaseTask wait3sTask = MakeTimerTask(5);
                float tolerance = isIncreasing ? 1f : -1f;
                ExitCode stopFailEC = ExitCode.none;
                string taskName = "";
                switch (compnent)
                {
                    case Hoist hoist:
                        {
                            stopFailEC = ExitCode.SC_8;
                            taskName = "SC_8";
                            tolerance *= 4;
                            break;
                        }
                    case Trolley trolley:
                        {
                            stopFailEC = ExitCode.SC_9;
                            taskName = "SC_9";
                            tolerance *= 3;
                            break;
                        }
                    case Pivot pivot:
                        {
                            stopFailEC = ExitCode.SC_7;
                            taskName = "SC_7";
                            tolerance *= 6;
                            break;
                        }
                }
                BaseTask overTask = new BaseTask(compnent.GetStatus, targetPara + tolerance, isIncreasing, true, false, 0, 0);

                PerformanceData stopFinishPD = new PerformanceData(()=> { performanceManager.RecordTask(taskName, true); });
                ConditionalTask stopTask = new(wait3sTask, overTask, 0, 0, failExitCode: stopFailEC, finishPD: stopFinishPD);

                subTasks.Add(stopTask);
            }
            else
            {
                PerformanceData startPD = new PerformanceData(
                    new Message(Message.Source.Signaller, startMessage,ActivityPeriod.a6),
                    performanceManager.GetComponentStatusChangeGestureMethod(compnent, statusChange)
                    );

                string finishMessage = String.Format(compnent.notificationTemplates["end"], adv);
                PerformanceData finishPD = new PerformanceData(
                    new Message(Message.Source.Signaller, finishMessage, ActivityPeriod.a6)
                    );

                BaseTask subTask = new BaseTask(compnent.GetStatus, targetPara, isIncreasing, true, true, 0, 0, startPD, finishPD, null, notificationTemplates: compnent.notificationTemplates);
                subTasks.Add(subTask);

                BaseTask wait3sTask = MakeTimerTask(5);
                float tolerance = isIncreasing? 1f: -1f;
                
                ExitCode stopFailEC = ExitCode.none;
                string taskName = "";
                switch (compnent)
                {
                    case Hoist hoist:
                        {
                            stopFailEC = ExitCode.SC_8;
                            taskName = "SC_8";
                            tolerance *= 4;
                            break;
                        }
                    case Trolley trolley:
                        {
                            stopFailEC = ExitCode.SC_9;
                            taskName = "SC_9";
                            tolerance *= 3;
                            break;
                        }
                    case Pivot pivot:
                        {
                            stopFailEC = ExitCode.SC_7;
                            taskName = "SC_7";
                            tolerance *= 6;
                            break;
                        }
                }
                BaseTask overTask = new BaseTask(compnent.GetStatus, targetPara + tolerance, isIncreasing, true, false, 0, 0);
                PerformanceData stopFinishPD = new PerformanceData(() => { performanceManager.RecordTask(taskName, true); });
                ConditionalTask stopTask = new(wait3sTask, overTask, 0, 0, failExitCode: stopFailEC, finishPD: stopFinishPD);
                subTasks.Add(stopTask);
            }


        }

        PerformanceData performanceData = new PerformanceData(performanceManager.StopHighlightWalkietalkie, performanceManager.EnableSignallerViewPanel);
        PerformanceData endPD = new PerformanceData(performanceManager.StopGesture);
        return new SerialTask(subTasks, finishWeight, errorWeight, startPD: performanceData, finishPD:endPD);
    }

    private void CheckOut()
    {
        
        GlobalVariables.Set("exitCodes", exitCodes);
        GlobalVariables.Set("totFinishWeight", totFinishWeight);
        GlobalVariables.Set("curFinishWeight", curFinishWeight>=totFinishWeight?curFinishWeight-totFinishWeight+1: curFinishWeight);
        GlobalVariables.Set("totTime", Time.timeSinceLevelLoad);
        GlobalVariables.Set("maxTime", maxDuration);
        Debug.Log("CheckOut");
        SceneManager.LoadScene("CheckOut");
    }







}
