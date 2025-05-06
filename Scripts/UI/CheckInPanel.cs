using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class CheckInPanel : InformationPanel
{
    string subjectProfilePath = $"{Application.dataPath}/SubjectID.txt";
    // Start is called before the first frame update
    void Start()
    {
        GlobalVariables.Set("expMode", 0);

        string subjectID = "test";
        string[] lines = File.ReadAllLines(subjectProfilePath);
        if (lines.Length > 0)
        {
            subjectID = lines[lines.Length - 1];
        }
        

        GlobalVariables.Set("subjectID", subjectID);

        TMPro.TMP_Text txtSubjectID = this.transform.Find("SubjectID").GetComponent<TMPro.TMP_Text>();
        if (txtSubjectID != null)
        {
            txtSubjectID.text = subjectID;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetExpMode(int modeIndex)
    {
        GlobalVariables.Set("expMode", modeIndex);
    }
}
