using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static AbstrackTask;

public class InformationPanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Continue()
    {
        Destroy(gameObject);
    }

    public void CheckReference(string pageName)
    {
        string path = $"Prefabs/UI/CheckOut/ReferencePanels/{pageName}";

        GameObject refPanel = Instantiate(Resources.Load(path, typeof(GameObject)), this.transform) as GameObject;
    }

    public void LoadScene(string name)
    {
        //if(GlobalVariables.ContainKey("expMode")&& GlobalVariables.Get<int>("expMode")==4)
        //{
        //    SceneManager.LoadScene("sc1 1");
        //}
        SceneManager.LoadScene(name);
    }
}
