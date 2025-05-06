using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class SingleColorLineRenderer : MonoBehaviour
{
    public enum PresetColor { Red, Green, Blue };
    public PresetColor presetColor;
    public float width=0.1f;
    public List<float> curveKeyXs = new List<float> { 1 };
    public List<float> curveKeyYs = new List<float> { 1 };

    public bool useCurve = true;
    protected LineRenderer lineRenderer;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.colorGradient = GetPresetGradient(presetColor);

        if (curveKeyXs.Count != curveKeyYs.Count)
        {
            throw new ArgumentException("curveKeyXs.Count != curveKeyYs.Count");
        }
        AnimationCurve curve = new AnimationCurve();
        for (int i = 0; i < curveKeyXs.Count; i++)
        {
            curve.AddKey(curveKeyXs[i], curveKeyYs[i]);
        }

        
        lineRenderer.widthCurve = curve;
        lineRenderer.widthMultiplier = width;


    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    public Gradient GetPresetGradient(PresetColor presetColor)
    {
        Gradient gradient = new Gradient();
        switch (presetColor)
        {
            case PresetColor.Red:
            {
                gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.red, 1.0f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
                );
                break;
            }
            case PresetColor.Green:
            {
                gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.green, 0.0f), new GradientColorKey(Color.green, 1.0f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
                );
                break;
            }
            case PresetColor.Blue:
            {
                gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.blue, 1.0f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
                );
                break;
            }
        }
        
        return gradient;
    }
}
