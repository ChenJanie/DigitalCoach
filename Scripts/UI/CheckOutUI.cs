using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using System.Linq;
using static AbstrackTask;
using TMPro;
using System.Diagnostics;
using System.IO;

public class CheckOutUI : MonoBehaviour
{
    public Texture star;
    public Texture halfStar;

    Dictionary<AbstrackTask.ExitCode, List<string>> exitCodeToSVPanelNameDict = new Dictionary<AbstrackTask.ExitCode, List<string>>()
    {
        [AbstrackTask.ExitCode.SC_1] = new List<string>(){"SC1"},
        [AbstrackTask.ExitCode.SC_2] = new List<string>() { "SC2"},
        [AbstrackTask.ExitCode.SC_3] = new List<string>() { "SC3" },
        [AbstrackTask.ExitCode.SC_4] = new List<string>() { "SC4" },
        [AbstrackTask.ExitCode.SC_5] = new List<string>() { "SC5"    },
        [AbstrackTask.ExitCode.SC_6] = new List<string>() { "SC6"    },
        [AbstrackTask.ExitCode.SC_7] = new List<string>() { "SC7"    },
        [AbstrackTask.ExitCode.SC_8] = new List<string>() { "SC8" },
        [AbstrackTask.ExitCode.SC_9] = new List<string>() { "SC9" },
        [AbstrackTask.ExitCode.timeOut] = new List<string>() { "TimeOut" }
    };

    const string resultPanelPath = "Prefabs/UI/CheckOut/ResultPanel";
    const string SVPanelPath = "Prefabs/UI/CheckOut/SafetyViolationPanel";
    const string ratingPanelPath = "Prefabs/UI/CheckOut/RatingPanel/RatingPanel";

    // Start is called before the first frame update
    void Start()
    {
        float totFinishWeight=GlobalVariables.Get<float>("totFinishWeight");
        float curFinishWeight = GlobalVariables.Get<float>("curFinishWeight");
        float totTime = GlobalVariables.Get<float>("totTime");
        float maxTime = GlobalVariables.Get<float>("maxTime");
        RatingIndicators ratingIndicators = new RatingIndicators(totFinishWeight, curFinishWeight, totTime , maxTime);
        InitRatingPanels(ratingIndicators);


        List<AbstrackTask.ExitCode> exitCodes = GlobalVariables.Get<List<AbstrackTask.ExitCode>>("exitCodes");

        InitSafetyViolantionPanels(exitCodes);



    }


    void InitSafetyViolantionPanels(List<AbstrackTask.ExitCode> exitCodes)
    {
        bool success = true;
        if (exitCodes == null) return;
        for(int i=exitCodes.Count-1; i>=0; i--)
        {
            if (exitCodeToSVPanelNameDict.Keys.Contains(exitCodes[i])){
                InitSafetyViolantionPanel(exitCodes[i]);
                success = false;
            }
            
        }

        string path;

        if (success)
        {
            path= $"{resultPanelPath}/success";
        }
        else
        {
            path = $"{resultPanelPath}/fail";            
        }

        GameObject SVPanel = Instantiate(Resources.Load(path, typeof(GameObject)), this.transform) as GameObject;

    }
    void InitRatingPanels(RatingIndicators ratingIndicators)
    {

        float curFinishWeight = ratingIndicators.curFinishWeight;
        float totFinishWeight = ratingIndicators.totFinishWeight;
        float totTime = ratingIndicators.totTime;
        float maxTime = ratingIndicators.maxTime;

        float completionRate=curFinishWeight/totFinishWeight;

        int compStarNum = (int)(completionRate * 10 + 0.5);
        int prodStarNum = (int)(completionRate / (totTime / maxTime) * 10 + 0.5);
        int safetyStarNum = compStarNum;


        GameObject ratingPanelGO= Instantiate(Resources.Load(ratingPanelPath, typeof(GameObject)), this.transform) as GameObject;

        InitStars(compStarNum, ratingPanelGO.transform.Find("CompStars"));
        InitStars(prodStarNum, ratingPanelGO.transform.Find("ProdStars"));
        InitStars(safetyStarNum, ratingPanelGO.transform.Find("SafetyStars"));

        TMP_Text text = ratingPanelGO.transform.Find("Success").GetComponent<TMP_Text>();
        if (compStarNum <10) {
            
            text.text = "Fail...";
        }
        else
        {
            text.text = "Success!!!";
        }




    }

    void InitSafetyViolantionPanel(AbstrackTask.ExitCode exitCode)
    {
        foreach(string panelName in exitCodeToSVPanelNameDict[exitCode])
        {
            GameObject SVPanel = Instantiate(Resources.Load(GetSVPanelPath(panelName), typeof(GameObject)), this.transform) as GameObject;
        }
        
    }

    void InitStars(int starNum, Transform panel)
    {
        if (starNum > 10) starNum = 10;
        int i = 0;
        while(starNum >= 2)
        { 
            RawImage image=panel.GetChild(i).GetComponent<RawImage>();
            image.texture = star;
            starNum-=2; i++;
        }

        while(i<5)
        {
            RawImage image = panel.GetChild(i).GetComponent<RawImage>();
            image.color = Color.clear;
            i++;
        }
    }

    string GetSVPanelPath(AbstrackTask.ExitCode exitCode)
    {
        return $"{SVPanelPath}/{exitCodeToSVPanelNameDict[exitCode]}";
    }

    string GetSVPanelPath(string panelName)
    {
        return $"{SVPanelPath}/{panelName}";
    }

    void Test()
    {
        GlobalVariables.Set("exitCodes", new List<AbstrackTask.ExitCode> { AbstrackTask.ExitCode.SC_1, AbstrackTask.ExitCode.SC_2, AbstrackTask.ExitCode.SC_4 });
    }


    // Update is called once per frame
    void Update()
    {
        if(transform.childCount == 0)
        {
            GlobalVariables.Clear();
            SceneManager.LoadScene("CheckIn");
        }
    }

    private void run_cmd(string cmd, string args)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "C:/Users/Freeman/AppData/Local/Programs/Python/Python311/python.exe";
        start.Arguments = string.Format("{0} {1}", cmd, args);
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        using (Process process = Process.Start(start))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                Console.Write(result);
            }
        }
    }
}
