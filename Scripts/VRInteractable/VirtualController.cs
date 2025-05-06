using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.XR.Interaction.Toolkit;

public class VirtualController : MonoBehaviour
{
    public string settingPath = "Settings/VRInteractable/Controller";
    public string controllerName;

    public VROperatorController targetVRController;

    public Dictionary<string, float> attriDict = new Dictionary<string, float>();
    public Dictionary<string, string> attriToAxisDict;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        InitAttriDict();
        InitAttriToAxisDict();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        UpdateAttris();
        Input(targetVRController);
    }

    protected virtual void InitAttriDict()
    {
    }

    protected virtual void InitAttriToAxisDict()
    {
        string jsonStr=File.ReadAllText(GetSettingPath());
        attriToAxisDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);
    }
    protected virtual void UpdateAttris()
    {

    }

    protected virtual void Input(VROperatorController targetVROperatorController)
    {
        foreach(string key in attriToAxisDict.Keys)
        {
            string axisName = attriToAxisDict[key];
            targetVRController.virtualAxes[axisName] = attriDict[key];
        }
    }

    public virtual string GetSettingPath()
    {
        return $"{Application.dataPath}/{settingPath}/{controllerName}.json";
    }

    protected virtual void Activate(ActivateEventArgs arg)
    {
        attriDict["active"] = 1;
    }

    protected virtual void Deactivate(DeactivateEventArgs arg)
    {
        attriDict["active"] = 0;
    }

    protected virtual void SelectEnter(SelectEnterEventArgs arg)
    {
        attriDict["select"] = 1;
    }

    protected virtual void SelectExit(SelectExitEventArgs arg)
    {
        attriDict["select"] = 1;
    }
}
