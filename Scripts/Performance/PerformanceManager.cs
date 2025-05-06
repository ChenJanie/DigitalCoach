using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GoogleTextToSpeech.Scripts;
using System.IO;

public class PerformanceManager : MonoBehaviour
{
    public MainUI mainUI;

    public SignallerPerformance signallerPerformance;

    public List<WorkerPeroformance> workerPeroformances;

    public List<WorkerPeroformance> badWorkerPerformances;

    public List<WorkerPeroformance> allWorkerPerformances;

    public GameObject virtualWalkieTalkieHLGO;
    public GameObject virtualLeftJoystickHLGO;
    public GameObject virtualRightJoystickHLGO;
    public GameObject warningCircleGO;

    public TaskManager taskManager;
    //public ForceSeatManager forceSeatManager;
    public ExpRecorder expRecorder;
    


    public GameObject arrow;

    

    [SerializeField] private TextToSpeech textToSpeech;

    [SerializeField] private AudioSource audioSource;

    private List<Pair<AudioClip, string>> audioFilePathPairs = new List<Pair<AudioClip,string>>();

    private void Update()
    {
        if (!audioSource.isPlaying && audioFilePathPairs.Count>0)
        {
            audioSource.PlayOneShot(audioFilePathPairs[0].Item1);
            string fileName = Path.GetFileNameWithoutExtension(audioFilePathPairs[0].Item2);
            string newFileName = fileName.Replace(fileName, fileName + $"_{Time.timeSinceLevelLoad+audioFilePathPairs[0].Item1.length}");
            if (File.Exists(audioFilePathPairs[0].Item2))
            {
                System.IO.File.Move(audioFilePathPairs[0].Item2, TextToSpeech.GetAudioPath(newFileName + ".mp3"));

            }
            audioFilePathPairs.RemoveAt(0);

        }
    }

    private void Start()
    {
        //textToSpeech.PlayTextToSpeech("this is a test", Message.Source.Hint);
    }



    public void Perform(PerformanceData performanceData)
    {
        if (performanceData == null) return;
        if (performanceData.messages != null)
        {
            mainUI.GetNewMessage(performanceData.messages);
            foreach(Message message in performanceData.messages)
            {
                textToSpeech.PlayTextToSpeech(message);
            }
            
        }
        if (performanceData.perfmMethods != null)
        {
            foreach(Action perfmMethod in performanceData.perfmMethods)
            {
                perfmMethod();
            }
        }
    }

    public void Perform(List<PerformanceData> performanceDatas) {
        if (performanceDatas == null) return;
        foreach(PerformanceData performanceData in performanceDatas)
        {
            Perform(performanceData);
        }
    }

    public void WorkerStruckByCraneRig()
    {

    }

    public void WorkerWithPPEKeepAway()
    {
        foreach(WorkerPeroformance workerPeroformance in workerPeroformances) {
            workerPeroformance.WearPPE();
            workerPeroformance.StartWalkBackward();
        }
    }
    public void WorkerWithoutPPEWearPPEAndKeepAway()
    {
        foreach (WorkerPeroformance workerPeroformance in badWorkerPerformances)
        {
            workerPeroformance.WearPPE();
            workerPeroformance.StartWalkBackward();
        }
    }

    public void AllWorkerTakeOffPPE()
    {
        foreach (WorkerPeroformance workerPeroformance in allWorkerPerformances)
        {
            workerPeroformance.TakeOffPPE();
        }
    }

    public Action GetComponentStatusChangeGestureMethod(AbstractCompnent compnent, float statusChange)
    {
        switch (compnent)
        {
            case Hoist hoist:
                {
                    if (statusChange > 0)
                    {
                        return HoistDownGesture;
                    }
                    else
                    {
                        return HoistUpGesture;
                    }
                }
            case Trolley trolley:
                {
                    if (statusChange > 0)
                    {
                        return TrolleyOutGesture;
                    }
                    else
                    {
                        return TrolleyInGesture;
                    }
                }
            case Pivot pivot:
                {
                    if (statusChange > 0)
                    {
                        return PivotRightGesture;
                    }
                    else
                    {
                        return PivotLeftGesture;
                    }
                }
        }

        return null;
    }

    public void HoistUpGesture()
    {
        signallerPerformance.HoistUpGesture();
    }

    public void HoistDownGesture()
    {
        signallerPerformance.HoistDownGesture();
    }

    public void TrolleyInGesture()
    {
        signallerPerformance.TrolleyInGesture();
    }

    public void TrolleyOutGesture()
    {
        signallerPerformance.TrolleyOutGesture();
    }

    public void PivotLeftGesture()
    {
        signallerPerformance.PivotLeftGesture();
    }

    public void PivotRightGesture()
    {
        signallerPerformance.PivotRightGesture();
    }
    public void StopGesture()
    {
        signallerPerformance.StopGuesture();
    }

    public void ReceiveAudioClip(AudioClip audioClip, string filePath)
    {
        audioFilePathPairs.Add(new Pair<AudioClip, string>(audioClip, filePath));
    }

    public void AddUnloadGuide()
    {
        taskManager.tasks.Add(taskManager.MakeUnloadTask(0, 0));
    }

    public void HighlightWalkietalkie()
    {
        virtualWalkieTalkieHLGO.SetActive(true);
    }
    public void StopHighlightWalkietalkie()
    {
        virtualWalkieTalkieHLGO.SetActive(false);
    }
    public void HighlightLeftJoystick()
    {
        virtualLeftJoystickHLGO.SetActive(true);
    }
    public void StopHighlightLeftJoystick()
    {
        virtualLeftJoystickHLGO.SetActive(false);
    }
    public void HighlightRightJoystick()
    {
        virtualRightJoystickHLGO.SetActive(true);
    }
    public void StopHighlightRightJoystick()
    {
        virtualRightJoystickHLGO.SetActive(false);
    }
    public void ActivateDistanceWarning()
    {
        warningCircleGO.SetActive(true);
    }
    public void DeactivateDistanceWarning()
    {
        warningCircleGO.SetActive(false);
    }
    public void HighlightDangerousWorker()
    {
        foreach(WorkerPeroformance workerPeroformance in badWorkerPerformances)
        {
            workerPeroformance.HighlightDanger();
        }
    }

    public void StopHighlightDangerousWorker()
    {
        foreach (WorkerPeroformance workerPeroformance in badWorkerPerformances)
        {
            workerPeroformance.StopHighlightDanger();
        }
    }

    public void EnableSignallerViewPanel()
    {
        mainUI.EnableSignallerViewPanel();
    }

    public void ForceSeatWork()
    {
        //if (forceSeatManager.stateBeforeShock != ForceSeatManager.ForceSeatState.work) forceSeatManager.stateBeforeShock = ForceSeatManager.ForceSeatState.work;
        //if (forceSeatManager.state != ForceSeatManager.ForceSeatState.shock) forceSeatManager.state = ForceSeatManager.ForceSeatState.work;
    }

    public void ForceSeatWind()
    {
        //if (forceSeatManager.stateBeforeShock != ForceSeatManager.ForceSeatState.work) forceSeatManager.stateBeforeShock = ForceSeatManager.ForceSeatState.wind;
        //if (forceSeatManager.state != ForceSeatManager.ForceSeatState.shock) forceSeatManager.state = ForceSeatManager.ForceSeatState.wind;
    }


    public void RecordTask(string task, bool success)
    {
        expRecorder.RecordTask(task, success);
    }
    



}