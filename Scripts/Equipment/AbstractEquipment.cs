using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractEquipment : MonoBehaviour
{

    public AbstractController controller;
    public List<Instruction> instructions=new List<Instruction>();

    public List<AbstractCompnent> compnents = new List<AbstractCompnent>();
    public int walkietalkieVersion;
    public Walkietalkie walkietalkie;

    public Dictionary<string, float> behavRecord = new Dictionary<string, float>();

    


    public static readonly string dataSheetPath = "Settings/DataSheets";

    //demo_0: direct set status in each update
    //interact_0: set acc according to para in each update
    public enum DriveMode { demo_0, interact_0};

    public DriveMode driveMode = DriveMode.interact_0;

    public string recordSavePath;

    public class Instruction
    {
        public int instructionTypeInt;
        public float parameter;

        public Instruction(int instructionTypeInt, float parameter)
        {
            this.instructionTypeInt = instructionTypeInt;
            this.parameter = parameter;
        }
    }

    public virtual void GetInstruction(int instructionTypeInt, float parameter)
    {
        instructions.Add(new Instruction(instructionTypeInt, parameter));
    }
    public virtual float PerformInstruction(Instruction instruction)
    {
        return 0;
    }

    public virtual void InitWorkTransforms()
    {
    }

    public virtual void InitComponents()
    {
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        controller = this.GetComponent<AbstractController>();

        InitWorkTransforms();
        InitComponents();

        Debug.Log("Equipment Init");

    }

    // Update is called once per frame
    public virtual void Update()
    {
        foreach(Instruction instruction in instructions)
        {
            string mesg = $"{this.GetType()}: perform {instruction.instructionTypeInt}, parameter={instruction.parameter}";
            DebugHelper.Log(DebugHelper.Field.equipment, mesg);
            PerformInstruction(instruction);
        }

        instructions.Clear();
    }

    public virtual void FixedUpdate()
    {
        foreach(AbstractCompnent compnent in compnents)
        {
            compnent.Work(driveMode);
        }
    }

    public static string GetComponentJsonPath(string componentName, int componentVersion)
    {
        return $"{Application.dataPath}/{dataSheetPath}/{componentName}_{componentVersion}.json";
    }

    public float checkBehavRecord(string componentName, string behavName)
    {
        return 0;
    }

    
}
