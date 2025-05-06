using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Diagnostics;

public class ExpRecorder : MonoBehaviour
{
    
    public string expName;
    private string subjectPath;
    private string expPath;
    private string eyeTrackPath;

    private AbstractEquipment[] equipments;


    private Dictionary<AbstractCompnent, StreamWriter> compBehavSWDict = new Dictionary<AbstractCompnent, StreamWriter>();
    private Dictionary<AbstractCompnent, StreamWriter> compAttriSWDict = new Dictionary<AbstractCompnent, StreamWriter>();

    private StreamWriter taskSF;
    private StreamWriter leftEyeSF;
    private StreamWriter rightEyeSF;

    public TaskManager.ExpMode expMode = TaskManager.ExpMode.rTrain;

    private string subjectID="test";

    private int fixedUpdateCounter = 0;

    

    // Start is called before the first frame update
    void Start()
    {
        if (GlobalVariables.ContainKey("subjectID"))
        {
            subjectID = GlobalVariables.Get<String>("subjectID");
        }

        if (GlobalVariables.ContainKey("expMode"))
        {
            expMode = GlobalVariables.Get<TaskManager.ExpMode>("expMode");
        }

        subjectPath = $"{Application.dataPath}/Record/Exp/{subjectID}";
        expPath = $"{subjectPath}/{expName}_{DateTime.Now.ToString("HH-mm-ss_yyyy-MM-dd")}_{expMode}";

        GlobalVariables.Set("expPath", expPath);
        if (!Directory.Exists(subjectPath))
        {
            Directory.CreateDirectory(subjectPath);
        }
        if (Directory.Exists(expPath))
        {
            Directory.Delete(expPath, true);
        }
        Directory.CreateDirectory(expPath);

        eyeTrackPath = $"{expPath}/eyeTracking";

        Directory.CreateDirectory(eyeTrackPath);



        equipments = GameObject.FindObjectsOfType<AbstractEquipment>();

        InitFileStreamWriter();

    }

    // Update is called once per frame
    void Update()
    {
        foreach(AbstractEquipment equipment in equipments)
        {
            foreach(AbstractCompnent compnent in equipment.compnents)
            {
                RecordBehav(compnent);
                RecordAttri(compnent);
            }
        }
    }

    private void FixedUpdate()
    {
        fixedUpdateCounter++;
    }

    #region Init
    private void InitFileStreamWriter()
    {
        foreach(AbstractEquipment equipment in equipments)
        {
            string equipPath = $"{expPath}/{equipment.name}";
            Directory.CreateDirectory(equipPath);
            for(int i=0; i < equipment.compnents.Count; i++)
            {
                AbstractCompnent compnent = equipment.compnents[i];

                string compPath = $"{equipPath}/{i}_{compnent.GetType()}";
                Directory.CreateDirectory(compPath);

                string behavSavePath = $"{compPath}/behaviors.csv";
                string attriSavePath = $"{compPath}/attributes.csv";
                
                StreamWriter behavSw = File.CreateText(behavSavePath);
                StreamWriter attriSw = File.CreateText(attriSavePath);

                behavSw.WriteLine("behavName, time, value");
                attriSw.WriteLine("attriName, time, value");


                compBehavSWDict.Add(compnent, behavSw);
                compAttriSWDict.Add(compnent, attriSw);

            }
        }

        string taskPath= $"{expPath}/tasks.csv";
        taskSF = File.CreateText(taskPath);

        string lEyePath = $"{eyeTrackPath}/leftEye.csv";
        leftEyeSF= File.CreateText(lEyePath);
        string rEyePath = $"{eyeTrackPath}/rightEye.csv";
        rightEyeSF= File.CreateText(rEyePath);



    }
    #endregion

    #region Exec
    private void RecordBehav(AbstractCompnent compnent)
    {
        List<string> row=new List<string>();
        foreach(string behavName in compnent.recordBehavNames)
        {
            foreach (Pair<float, float> tuple in compnent.behavRecordDict[behavName])
            {
                float time = tuple.Item1;
                float value = tuple.Item2;
                string record = $"{behavName},{time},{value}";

                compBehavSWDict[compnent].WriteLine(record);

            }

            compnent.behavRecordDict[behavName].Clear();
        }

    }

    private void RecordAttri(AbstractCompnent compnent)
    {
        List<string> row = new List<string>();
        foreach (string attriName in compnent.recordAttriNames)
        {
            foreach (Pair<float, float> tuple in compnent.attriRecordDict[attriName])
            {
                float time = tuple.Item1;
                float value = tuple.Item2;
                string record = $"{attriName},{time},{value}";

                compAttriSWDict[compnent].WriteLine(record);

            }

            compnent.attriRecordDict[attriName].Clear();
        }
    }

    public void RecordTask(string taskDescription, bool success)
    {
        string record = $"{taskDescription},{success},{Time.fixedDeltaTime * fixedUpdateCounter}";

        taskSF.WriteLine(record);
    }

    public void RecordEye(int groupIndex, int itemIndex0, int itemIndex1, bool isLeft)
    {
        string record = $"{groupIndex},{itemIndex0},{itemIndex1},{Time.fixedDeltaTime*fixedUpdateCounter}";

        if (isLeft)
        {
            leftEyeSF.WriteLine(record);
        }
        else
        {
            rightEyeSF.WriteLine(record);
        }
    }
    #endregion

    private void OnDestroy()
    {
        foreach(AbstractCompnent component in compAttriSWDict.Keys)
        {
            compAttriSWDict[component].Close();
        }

        foreach (AbstractCompnent component in compBehavSWDict.Keys)
        {
            compBehavSWDict[component].Close();
        }

        taskSF.Close();
        leftEyeSF.Close();
        rightEyeSF.Close();

        //if (expMode == TaskManager.ExpMode.test)
        //{
        //    string cmd = Application.dataPath + "/Scripts/Analyse/analyseSingleExp.py";

        //    UnityEngine.Debug.Log(cmd);
        //    run_cmd(cmd, expPath);
        //}

        string cmd = Application.dataPath + "/Scripts/Analyse/analyseSingleExp.py";
        UnityEngine.Debug.Log(cmd);
        run_cmd(cmd, expPath);
    }

    private void run_cmd(string cmd, string args)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "C:\\Users\\Freeman\\AppData\\Local\\Programs\\Python\\Python311\\python.exe";
        start.Arguments = string.Format("{0} {1}", cmd, args);
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.CreateNoWindow = true;
        using (Process process = Process.Start(start))
        {
            //using (StreamReader reader = process.StandardOutput)
            //{
            //    string result = reader.ReadToEnd();
            //    UnityEngine.Debug.Log("result");
            //    UnityEngine.Debug.Log(result);
            //}
        }
    }


}
