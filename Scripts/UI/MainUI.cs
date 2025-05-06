using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;


public class MainUI : MonoBehaviour
{
    public TowerCrane towerCrane;
    public int maxMessageNum;

    public GameObject pOperatorMessage;
    public GameObject pSignallerMessage;
    public GameObject pHintMessage;
    public GameObject pDefaultMessage;

    private Transform generalComponentPanel;
    private Transform safetyPanel;
    private Transform statusPanel;
    private Transform messagePanel;
    private Transform timePanel;
    private Transform signallerViewPanel;

    public float maxMessageDuration;
    private float curMessageDuration;
    private bool isShowingMessage;
    private List<Trio<string,RectTransform, float>> messageRtfDurTrios;


    // Start is called before the first frame update
    void Start()
    {
        curMessageDuration = 0;
        maxMessageDuration = 10;
        isShowingMessage = false;
        messageRtfDurTrios = new List<Trio<string,RectTransform, float>>();
        generalComponentPanel = this.transform.Find("GeneralComponentPanel");
        safetyPanel = this.transform.Find("SafetyPanel");
        statusPanel = this.transform.Find("StatusPanel");
        messagePanel = this.transform.Find("MessagePanel");
        messagePanel.gameObject.SetActive(isShowingMessage);
        signallerViewPanel = this.transform.Find("SignallerViewPanel");

        timePanel = this.transform.Find("TimePanel");

    }


    void UpdateGeneralComponentPanel()
    {
        UpdateComponentPanel(towerCrane.hoist, "Hoist");
        UpdateComponentPanel(towerCrane.trolley, "Trolley");
        UpdateComponentPanel(towerCrane.pivot, "Pivot");
    }

    void UpdateComponentPanel(AbstractCompnent compnent, string componentName)
    {
        Transform componentPanel =generalComponentPanel.Find(componentName);

        Transform gearPanel = componentPanel.Find("Gear");
        gearPanel.Find("MAX").gameObject.GetComponent<Text>().text = "MAX: "+compnent.maxGear.ToString();
        gearPanel.Find("MIN").gameObject.GetComponent<Text>().text = "MIN: " + 1.ToString();
        gearPanel.Find("Cur").gameObject.GetComponent<Text>().text = "Cur: " + compnent.gear.ToString("0.0");

        Transform speedPanel = componentPanel.Find("Speed");
        speedPanel.Find("MAX").gameObject.GetComponent<Text>().text = "MAX: " + compnent.maxSpeed[compnent.gear-1].ToString();
        speedPanel.Find("MIN").gameObject.GetComponent<Text>().text = "MIN: " + 0.ToString();
        speedPanel.Find("Cur").gameObject.GetComponent<Text>().text = "Cur: " + compnent.speed.ToString("0.0");

        Transform statusPanel = componentPanel.Find("Status");
        statusPanel.Find("MAX").gameObject.GetComponent<Text>().text = "MAX: " + compnent.maxStatus.ToString();
        statusPanel.Find("MIN").gameObject.GetComponent<Text>().text = "MIN: " + compnent.minStatus.ToString();
        statusPanel.Find("Cur").gameObject.GetComponent<Text>().text = "Cur: " + compnent.status.ToString("0.0");

    }

    void UpdateSafetyPanel()
    {
        float rate = towerCrane.GetLoadRate();
        if (rate > 1) rate = 1;

        DebugHelper.Log(DebugHelper.Field.physics, "load rate"+rate.ToString());
        RectTransform rtIn = safetyPanel.Find("TensionBarIn").GetComponent<RectTransform>();
        RectTransform rtOut = safetyPanel.Find("TensionBarOut").GetComponent<RectTransform>();
        rtIn.sizeDelta = new Vector2(rate*rtOut.sizeDelta.x, rtOut.sizeDelta.y);

        DebugHelper.Log(DebugHelper.Field.physics, rtIn.sizeDelta.ToString());

        if (rate < 0.3)
        {
            safetyPanel.Find("TensionBarIn").GetComponent<Image>().color= new Color32(124, 252, 0, 100);
        }
        else if (rate < 0.6)
        {
            safetyPanel.Find("TensionBarIn").GetComponent<Image>().color = new Color32(253, 218, 13, 100);
        }
        else
        {
            safetyPanel.Find("TensionBarIn").GetComponent<Image>().color = new Color32(255, 0, 0, 100);

        }
    }

    private void UpdateMessagePanel()
    {
        messagePanel.gameObject.SetActive(isShowingMessage);

        if (isShowingMessage)
        {
            while (messageRtfDurTrios.Count > maxMessageNum)
            {
                Destroy(messageRtfDurTrios[0].Item2.gameObject);
                messageRtfDurTrios.RemoveAt(0);
            }

            for(int i = 0; i< messageRtfDurTrios.Count; i++)
            {
                if (messageRtfDurTrios[i].Item3 > maxMessageDuration)
                {
                    Destroy(messageRtfDurTrios[i].Item2.gameObject);
                    messageRtfDurTrios.RemoveAt(i);
                    i--;
                }
            }

            float curPosY = 0;
            foreach (Trio<string, RectTransform, float> trio in messageRtfDurTrios)
            {
                string message = trio.Item1;
                RectTransform rtfMessage = trio.Item2;

                rtfMessage.anchoredPosition = new Vector3(0, curPosY, 0);

                curPosY -= rtfMessage.sizeDelta.y;

                trio.Item3 += Time.deltaTime;
            }
        }

        if (messageRtfDurTrios.Count == 0)
        {
            isShowingMessage = false;
            messagePanel.gameObject.SetActive(isShowingMessage);
        }
        
    }


    public void GetNewMessage(Message[] messages)
    {
        foreach(Message message in messages)
        {
            GameObject goMessage;

            switch (message.source)
            {
                case (Message.Source.Operator):
                    {
                        goMessage = (GameObject)Instantiate(pOperatorMessage, messagePanel);
                        break;
                    }
                case (Message.Source.Signaller):
                    {
                        goMessage = (GameObject)Instantiate( pSignallerMessage, messagePanel);
                        break;
                    }
                case (Message.Source.Hint):
                    {
                        goMessage = (GameObject)Instantiate(pHintMessage, messagePanel);
                        break;
                    }
                default:
                    {
                        goMessage = (GameObject)Instantiate(pDefaultMessage, messagePanel);
                        break;
                    }
            }

            RectTransform rtfMessage = goMessage.GetComponent<RectTransform>();
            Text text = goMessage.GetComponent<Text>();
            text.text = message.content;
            messageRtfDurTrios.Add(new Trio<string, RectTransform, float>(message.content, rtfMessage, 0));
        }

        
        
        isShowingMessage = true;

        messagePanel.gameObject.SetActive(isShowingMessage);
    }

    private void UpdateTimePanel()
    {
        TMP_Text timeText =timePanel.Find("TimeText").gameObject.GetComponent < TMP_Text >();
        if (timeText != null)
        {
            timeText.text=Time.timeSinceLevelLoad.ToString("0.0")+" s";
        }
    }

    public void EnableSignallerViewPanel()
    {
        signallerViewPanel.gameObject.SetActive(true);
    }


    // Update is called once per frame
    void Update()
    {
        UpdateGeneralComponentPanel();
        UpdateMessagePanel();
        UpdateSafetyPanel();
        UpdateTimePanel();
    }
}
