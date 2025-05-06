using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using UnityEngine.XR.Interaction.Toolkit;


public class OperatorController : AbstractController
{
    public AudioSource audioSource;

    public AudioClip walkietalkieAudioClip;
    public AudioClip startButtonAudioClip;

    public static float GetTest()
    {
        return 1f;
    }
    public virtual string settingPath { get; set; } = "Settings/Controller/OperatorController";
    public virtual int curSettingVersion { get; set; } = 0;


    public List<OperatorInputSetting[]> operatorInputSettingsVersions { get; set; } = new List<OperatorInputSetting[]>();

    // Start is called before the first frame update
    protected virtual void Start()
    {
        string targetTypeStr = target.GetType().Name;
        if (targetTypeStr != targetType.ToString())
        {
            string mesg = "controller: target doesn't match target type";
            DebugHelper.Log(DebugHelper.Field.controller, mesg);
        }

        InitControllerSetting();

        if (PlayerPrefs.HasKey("curSettingVersion"))
        {
            curSettingVersion = PlayerPrefs.GetInt("curSettingVersion");
        }
        else
        {
            PlayerPrefs.SetInt("curSettingVersion", curSettingVersion);
            PlayerPrefs.Save();
        }
        Debug.Log($"SwitchOperatorInputSettingsVersion, curVersion = {curSettingVersion}");


    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SwitchOperatorInputSettingsVersion();
        }
        foreach (OperatorInputSetting operatorInputSetting in operatorInputSettingsVersions[curSettingVersion])
        {
            float parameter = operatorInputSetting.GetInput();
            target.GetInstruction(operatorInputSetting.GetInstructionType(), parameter);
            if (parameter != 0)
            {
                if (operatorInputSetting.GetInstructionType() == 6)
                {
                    audioSource.PlayOneShot(walkietalkieAudioClip);
                }
                if (operatorInputSetting.GetInstructionType() == 7)
                {
                    audioSource.PlayOneShot(startButtonAudioClip);
                }

                string mesg = $"{target.GetType()} get instruction, instruction type:{operatorInputSetting.GetInstructionType()}, parameter:{parameter}";
                DebugHelper.Log(DebugHelper.Field.controller, mesg);
            }
        }

    }

    public virtual void InitControllerSetting()
    {
        InitInputSetting();
    }

    public virtual void InitInputSetting()
    {
        for (int i = 0; File.Exists(GetSettingPath(i)); i++)
        {
            OperatorInputSetting[] operatorInputSettings = JsonHelper.ListFromJson<OperatorInputSetting>(GetSettingPath(i));
            for (int j = 0; j < operatorInputSettings.Length; j++)
            {
                operatorInputSettings[j].GetAxisValue = GetAxisValue;
            }
            operatorInputSettingsVersions.Add(operatorInputSettings);
        }

    }



    public virtual float GetAxisValue(string axisName)
    {
        return Input.GetAxis(axisName);
    }

    public virtual string GetSettingPath(int version)
    {
        string targetTypeStr = target.GetType().Name;
        return $"{Application.dataPath}/{settingPath}/{targetTypeStr}_{version}.json";
    }

    public void SwitchOperatorInputSettingsVersion()
    {
        curSettingVersion += 1;
        if (operatorInputSettingsVersions.Count < curSettingVersion + 1)
        {
            curSettingVersion = 0;
        }
        PlayerPrefs.SetInt("curSettingVersion", curSettingVersion);
        PlayerPrefs.Save();
        Debug.Log($"SwitchOperatorInputSettingsVersion, curVersion = {curSettingVersion}");
    }





    [System.Serializable]
    public class OperatorInputSetting
    {
        public Func<string, float> GetAxisValue;
        public int instructionType;
        public bool isContinous;

        public string[] axisNames;
        public bool isNeedActivated;
        public string[] aAxisNames;
        public bool lastSwitch = false;

        public float GetInput()
        {
            //both positive and negative is not pressed

            float parameter = 0;



            if (isNeedActivated)
            {
                if (!IsActivated())
                {
                    return parameter;
                }
            }

            foreach (string axisName in axisNames)
            {
                if (GetAxisValue(axisName) != 0)
                {
                    parameter = GetAxisValue(axisName);
                    string mesg = "get axis: " + axisName + parameter;
                    DebugHelper.Log(DebugHelper.Field.controller, mesg);
                }
            }

            if (!isContinous)
            {
                if (lastSwitch == (parameter != 0))
                {
                    parameter = 0;
                }
                else
                {
                    lastSwitch = (parameter != 0);
                }

            }

            return parameter;
        }

        bool IsActivated()
        {
            foreach (string axisName in aAxisNames)
            {
                if (GetAxisValue(axisName) != 0)
                {
                    string mesg = "active: " + axisName + GetAxisValue(axisName);
                    DebugHelper.Log(DebugHelper.Field.controller, mesg);
                    return true;
                }
            }

            return false;
        }

        public int GetInstructionType()
        {
            return instructionType;
        }
    }





}
