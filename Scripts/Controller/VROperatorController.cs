using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

public class VROperatorController : OperatorController
{

    public override string settingPath { get; set; } = "Settings/Controller/VROperatorController";
    public int settingVersion { get; set; } = 0;

    public Dictionary<string, float> virtualAxes;


    // Start is called before the first frame update
    protected override void Start()
    {
        virtualAxes = new Dictionary<string, float>();
        base.Start();

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void InitControllerSetting()
    {
        base.InitControllerSetting();
        JObject jObject = JObject.Parse(File.ReadAllText(GetSettingPath(0)));

        JArray jArray = (JArray)jObject["axisNames"];

        string[] axisNames = jArray.ToObject<string[]>();
        foreach (string axisName in axisNames)
        {
            virtualAxes.Add(axisName, 0);
        }
    }

    public override float GetAxisValue(string axisName)
    {
        return virtualAxes[axisName];
    }


}
