using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewPoint : MonoBehaviour
{
    public float VisibilityDistance = 6;
    [Range(0.0f, 1.0f)]
    public float CenterAlpha = 1;
    [Range(0.0f, 1.0f)]
    public float RangeAlpha = 0.8f;
    public float LimitDistance = 4;
    [Range(0.0f, 1.0f)]
    public float LimitAlpha = 0.2f;
    [Range(0.0f, 360.0f)]
    public float VisibilityAngle = 140;
    [Range(0.0f, 1.0f)]
    public float AngleAlpha = 0.2f;
    public float OutlineInternal = 0.05f;
    public float OutlineExternal = 0.05f;
    public Color OutlineColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    [Range(0.0f, 1.0f)]
    public float OutlineAlpha = 1;
}
