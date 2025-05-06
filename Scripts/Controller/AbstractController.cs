using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractController : MonoBehaviour
{
    public TargetType targetType;
    public AbstractEquipment target;

    public enum TargetType { TowerCrane , TaskManager};

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
