//using MotionSystems;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ForceSeatManager : MonoBehaviour
{

    #region SubClass
    [System.Serializable]
    public class AttributeRange
    {
        public float max;
        public float min;

        public AttributeRange(float min, float max)
        {
            if (min > max)
            {
                throw new ArgumentException("AttributeRange: min > max");
            }

            this.min = min;
            this.max = max;
        }
    }

    [System.Serializable]
    public class AttributeRanges
    {
        public AttributeRange rRoll;
        public AttributeRange rPitch;
        public AttributeRange rYaw;

        public AttributeRange rSurge;
        public AttributeRange rSway;
        public AttributeRange rHeave;

        public AttributeRange GetAttributeRange(Attribute attribute)
        {
            switch (attribute)
            {
                case Attribute.roll:
                    {
                        return rRoll;
                    }
                case Attribute.pitch:
                    {
                        return rPitch;
                    }
                case Attribute.heave:
                    {
                        return rHeave;
                    }
                case Attribute.yaw:
                    {
                        return rYaw;
                    }
                case Attribute.surge:
                    {
                        return rSurge;
                    }
                case Attribute.sway:
                    {
                        return rSway;
                    }
            }
            throw new ArgumentException("ShakeSetting.GetAttributeShakeSetting: unknown attribute");
        }
    }



    [System.Serializable]
    public class AttributeShakeSetting
    {
        public float step;
        public float max;
        public float min;

        public AttributeShakeSetting()
        {
            step = 0.01f;
            max = 0;
            min = 0;
        }
    }

    [System.Serializable]
    public class ShakeSetting
    {
        public AttributeShakeSetting ssRoll;
        public AttributeShakeSetting ssPitch;
        public AttributeShakeSetting ssYaw;

        public AttributeShakeSetting ssSurge;
        public AttributeShakeSetting ssSway;
        public AttributeShakeSetting ssHeave;

        public AttributeShakeSetting GetAttributeShakeSetting(Attribute attribute)
        {
            switch (attribute)
            {
                case Attribute.roll:
                    {
                        return ssRoll;
                    }
                case Attribute.pitch:
                    {
                        return ssPitch;
                    }
                case Attribute.heave:
                    {
                        return ssHeave;
                    }
                case Attribute.yaw:
                    {
                        return ssYaw;
                    }
                case Attribute.surge:
                    {
                        return ssSurge;
                    }
                case Attribute.sway:
                    {
                        return ssSway;
                    }
            }
            throw new ArgumentException("ShakeSetting.GetAttributeShakeSetting: unknown attribute");
        }

    }

    public enum Attribute { roll, pitch, yaw, surge, sway, heave };

    public class Shock
    {
        public float dur;
        public float curDur;
        public float step;
        public Attribute attribute;
        public bool isPositive;
        public Shock(Attribute attribute, bool isPositive, float step, float dur)
        {
            this.attribute = attribute;
            this.step = step;
            this.dur = dur;
            this.isPositive = isPositive;
            this.curDur = 0;
        }
    }
    #endregion


    #region Setting
    public AttributeRanges attributeRanges = new AttributeRanges
    {
        rRoll = new AttributeRange(-10, 10),
        rPitch = new AttributeRange(-10, 10),
        rYaw = new AttributeRange(-10, 10),
        rSurge = new AttributeRange(-10, 10),
        rSway = new AttributeRange(-10, 10),
        rHeave = new AttributeRange(-10, 10)
    };

    public ShakeSetting sIdle = new ShakeSetting
    {
        ssRoll = new AttributeShakeSetting(),
        ssPitch = new AttributeShakeSetting(),
        ssYaw = new AttributeShakeSetting(),
        ssSurge = new AttributeShakeSetting(),
        ssSway = new AttributeShakeSetting(),
        ssHeave = new AttributeShakeSetting()
    };
    public ShakeSetting ssWork = new ShakeSetting
    {
        ssRoll = new AttributeShakeSetting
        {
            step = 0.008f,
            max = 1f,
            min = -1f,
        },
        ssPitch = new AttributeShakeSetting
        {
            step = 0.008f,
            max = 1f,
            min = -1f,
        },
        ssYaw = new AttributeShakeSetting(),
        ssSurge = new AttributeShakeSetting(),
        ssSway = new AttributeShakeSetting(),
        ssHeave = new AttributeShakeSetting()
    };
    public ShakeSetting ssWind = new ShakeSetting
    {
        ssRoll = new AttributeShakeSetting
        {
            step = 0.016f,
            max = 1.5f,
            min = -1.5f,
        },
        ssPitch = new AttributeShakeSetting
        {
            step = 0.016f,
            max = 1.5f,
            min = -1.5f,
        },
        ssYaw = new AttributeShakeSetting(),
        ssSurge = new AttributeShakeSetting(),
        ssSway = new AttributeShakeSetting(),
        ssHeave = new AttributeShakeSetting()
    };
    public ShakeSetting ssAccident = new ShakeSetting
    {
        ssRoll = new AttributeShakeSetting
        {
            step = 0.05f,
            max = 5f,
            min = -5f,
        },
        ssPitch = new AttributeShakeSetting
        {
            step = 0.05f,
            max = 5f,
            min = -5f,
        },
        ssYaw = new AttributeShakeSetting(),
        ssSurge = new AttributeShakeSetting(),
        ssSway = new AttributeShakeSetting(),
        ssHeave = new AttributeShakeSetting()
    };
    #endregion

    public enum ForceSeatState { idle, work, wind, accident, shock };
    public ForceSeatState state = ForceSeatState.idle;

    private Shock curShock = null;

    public ForceSeatState stateBeforeShock;

    // Origin position of the shaft
    private Vector3 m_originPosition;

    // Origin rotation of the board
    private Vector3 m_originRotation;


    private float m_heave = 0;
    private float m_sway = 0;
    private float m_surge = 0;
    private float m_pitch = 0;
    private float m_roll = 0;
    private float m_yaw = 0;


    //// FSMI api
    //private ForceSeatMI m_fsmi;

    //// Position in physical coordinates that will be send to the platform
    //private FSMI_TopTablePositionPhysical m_platformPosition = new FSMI_TopTablePositionPhysical();

    public float testShockDur = 0.1f;
    public float testShockStep = 0.05f;

    public float testRotationStep = 0.01f;
    public float testTranslateStep = 0.01f;

    string testDebugLog = "";


    void Start()
    {
        //// Load ForceSeatMI library from ForceSeatPM installation directory
        //m_fsmi = new ForceSeatMI();

        //if (m_fsmi.IsLoaded())
        //{

        //    // Prepare data structure by clearing it and setting correct size
        //    m_platformPosition.mask = 0;
        //    m_platformPosition.structSize = (byte)Marshal.SizeOf(m_platformPosition);

        //    m_platformPosition.state = FSMI_State.NO_PAUSE;

        //    // Set fields that can be changed by demo application
        //    m_platformPosition.mask = FSMI_POS_BIT.STATE | FSMI_POS_BIT.POSITION;

        //    m_fsmi.SetAppID(""); // If you have dedicated app id, remove ActivateProfile calls from your code
        //    m_fsmi.ActivateProfile("SDK - Positioning");
        //    m_fsmi.BeginMotionControl();


        //    SendDataToPlatform();
        //}
        //else
        //{
        //    Debug.LogError("ForceSeatMI library has not been found!Please install ForceSeatPM.");
        //}



    }

    void Update()
    {
        //if (m_fsmi.IsLoaded())
        //{

        //    TestController();
        //}
    }

    void FixedUpdate()
    {

        Debug.Log(state);
        switch (state)
        {
            case ForceSeatState.idle:
                {
                    Shake(sIdle);
                    break;
                }
            case ForceSeatState.work:
                {
                    Shake(ssWork);
                    break;
                }
            case ForceSeatState.wind:
                {
                    Shake(ssWind);
                    break;
                }
            case ForceSeatState.accident:
                {
                    Shake(ssAccident);
                    break;
                }
            case ForceSeatState.shock:
                {
                    ActShock();
                    break;
                }
        }
        SendDataToPlatform();
    }

    void OnDestroy()
    {
        //if (m_fsmi.IsLoaded())
        //{
        //    m_fsmi.EndMotionControl();
        //    m_fsmi.Dispose();
        //}
    }

    private void AttributeShake(ref float attribute, float step, float min, float max)
    {
        if (attribute > max)
        {
            attribute -= step;
            if (attribute < min)
            {
                attribute = max;
            }
        }
        else if (attribute < min)
        {
            attribute += step;
            if (attribute > max)
            {
                attribute = min;
            }
        }
        else
        {
            float probPos = (max - attribute) / (max - min);
            float randomNum = UnityEngine.Random.Range(0f, 1f);
            if (randomNum < probPos)
            {
                attribute = Mathf.Clamp(attribute + step, min, max);
            }
            else if (randomNum > probPos)
            {
                attribute = Mathf.Clamp(attribute - step, min, max);
            }

            testDebugLog += ($", proPos: {probPos}, randonNum: {randomNum}, Attribute after Fixed Updated: {attribute}");
            Debug.Log(testDebugLog);
        }


    }

    private void AttributeShake(ref float attribute, AttributeShakeSetting attributeShakeSetting, AttributeRange attributeRange)
    {
        float rangedMax = Mathf.Min(attributeShakeSetting.max, attributeRange.max);
        float rangedMin = Mathf.Max(attributeShakeSetting.min, attributeRange.min);
        testDebugLog += ($" ,Cur Value: {attribute}, max: {rangedMax}, min: {rangedMin}, step: {attributeShakeSetting.step}");
        AttributeShake(ref attribute, attributeShakeSetting.step, rangedMin, rangedMax);

    }

    private void Shake(ShakeSetting shakeSetting)
    {

        foreach (Attribute attribute in Enum.GetValues(typeof(Attribute)))
        {
            AttributeShake(ref GetAttributeRef(attribute), shakeSetting.GetAttributeShakeSetting(attribute), attributeRanges.GetAttributeRange(attribute));
            testDebugLog = $"Attribute: {attribute}";
        }

        //AttributeShake(ref m_roll, shakeSetting.ssRoll, attributeRanges.rRoll);
        //AttributeShake(ref m_pitch, shakeSetting.ssPitch, attributeRanges.rPitch);
        //AttributeShake(ref m_yaw, shakeSetting.ssYaw, attributeRanges.rYaw);
        //AttributeShake(ref m_surge, shakeSetting.ssSurge, attributeRanges.rSurge);
        //AttributeShake(ref m_sway, shakeSetting.ssSway, attributeRanges.rSway);
        //AttributeShake(ref m_heave, shakeSetting.ssHeave, attributeRanges.rHeave);

    }


    private void SendDataToPlatform()
    {
        //// Convert parameters to logical units
        //m_platformPosition.state = FSMI_State.NO_PAUSE;
        //m_platformPosition.roll = Mathf.Deg2Rad * m_roll;
        //m_platformPosition.pitch = -Mathf.Deg2Rad * m_pitch;
        //m_platformPosition.yaw = Mathf.Deg2Rad * m_yaw;

        //m_platformPosition.heave = m_heave * 10;
        //m_platformPosition.surge = m_surge * 10;
        //m_platformPosition.sway = m_sway * 10;

        //// Send data to platform
        //m_fsmi.SendTopTablePosPhy(ref m_platformPosition);
    }

    public void StartShock(Attribute attribute, bool isPositive, float step, float dur)
    {

        Shock shock = new Shock(attribute, isPositive, step, dur);

        stateBeforeShock = state == ForceSeatState.shock ? stateBeforeShock : state;
        state = ForceSeatState.shock;

        curShock = shock;
    }

    private void ActShock()
    {
        Attribute attribute = curShock.attribute;

        ref float attributeRef = ref GetAttributeRef(attribute);

        float max = attributeRanges.GetAttributeRange(attribute).max;
        float min = attributeRanges.GetAttributeRange(attribute).min;

        float step = curShock.isPositive ? curShock.step : -curShock.step;

        attributeRef = Mathf.Clamp(attributeRef + step, min, max);

        curShock.curDur += Time.fixedDeltaTime;

        if (curShock.curDur > curShock.dur)
        {
            curShock = null;
            state = stateBeforeShock;
        }


    }

    private ref float GetAttributeRef(Attribute attribute)
    {
        switch (attribute)
        {
            case Attribute.roll:
                {
                    return ref m_roll;
                }
            case Attribute.pitch:
                {
                    return ref m_pitch;
                }
            case Attribute.heave:
                {
                    return ref m_heave;
                }
            case Attribute.yaw:
                {
                    return ref m_yaw;
                }
            case Attribute.surge:
                {
                    return ref m_surge;
                }
            case Attribute.sway:
                {
                    return ref m_sway;
                }
        }
        throw new ArgumentException("GetAttributeRef: unknown attribute");
    }


    private void TestController()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            state = ForceSeatState.idle;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            state = ForceSeatState.work;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            state = ForceSeatState.wind;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            state = ForceSeatState.accident;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            StartShock(Attribute.pitch, true, testShockStep, testShockDur);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            StartShock(Attribute.pitch, false, testShockStep, testShockDur);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            StartShock(Attribute.roll, false, testShockStep, testShockDur);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            StartShock(Attribute.roll, true, testShockStep, testShockDur);
        }

        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            StartShock(Attribute.yaw, false, testShockStep, testShockDur);
        }
        else if (Input.GetKeyDown(KeyCode.KeypadPeriod))
        {
            StartShock(Attribute.pitch, true, testShockStep, testShockDur);
        }

        if (Input.GetKey(KeyCode.W))
        {
            m_pitch = Mathf.Clamp(m_pitch + testRotationStep, attributeRanges.rPitch.min, attributeRanges.rPitch.max);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            m_pitch = Mathf.Clamp(m_pitch - testRotationStep, attributeRanges.rPitch.min, attributeRanges.rPitch.max);
        }

        if (Input.GetKey(KeyCode.A))
        {
            m_roll = Mathf.Clamp(m_roll - testRotationStep, attributeRanges.rRoll.min, attributeRanges.rRoll.max);

        }
        else if (Input.GetKey(KeyCode.D))
        {
            m_roll = Mathf.Clamp(m_roll + testRotationStep, attributeRanges.rRoll.min, attributeRanges.rRoll.max);
        }

        if (Input.GetKey(KeyCode.Z))
        {
            m_yaw = Mathf.Clamp(m_yaw - testRotationStep, attributeRanges.rYaw.min, attributeRanges.rYaw.max);

        }
        else if (Input.GetKey(KeyCode.C))
        {
            m_yaw = Mathf.Clamp(m_yaw + testRotationStep, attributeRanges.rYaw.min, attributeRanges.rYaw.max);
        }

        if (Input.GetKey(KeyCode.T))
        {
            m_surge = Mathf.Clamp(m_surge + testTranslateStep, attributeRanges.rSurge.min, attributeRanges.rSurge.max);
        }
        else if (Input.GetKey(KeyCode.G))
        {
            m_surge = Mathf.Clamp(m_surge - testTranslateStep, attributeRanges.rSurge.min, attributeRanges.rSurge.max);
        }

        if (Input.GetKey(KeyCode.H))
        {
            m_sway = Mathf.Clamp(m_sway + testTranslateStep, attributeRanges.rSway.min, attributeRanges.rSway.max);
        }
        else if (Input.GetKey(KeyCode.F))
        {
            m_sway = Mathf.Clamp(m_sway - testTranslateStep, attributeRanges.rSway.min, attributeRanges.rSway.max);
        }

        if (Input.GetKey(KeyCode.Y))
        {
            m_heave = Mathf.Clamp(m_heave + testTranslateStep, attributeRanges.rHeave.min, attributeRanges.rHeave.max);
        }
        else if (Input.GetKey(KeyCode.N))
        {
            m_heave = Mathf.Clamp(m_heave - testTranslateStep, attributeRanges.rHeave.min, attributeRanges.rHeave.max);
        }

    }

}
