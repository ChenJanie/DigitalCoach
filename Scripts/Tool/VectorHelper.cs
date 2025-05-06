using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VectorHelper
{
    public static Vector3 GetDistance(Transform a, Transform b, Transform parent=null)
    {
        return GetDistance(a.position, b.position, parent);
    }

    public static Vector3 GetDistance(Vector3 a, Transform b, Transform parent = null)
    {
        return GetDistance(a, b.position, parent);
    }

    public static Vector3 GetDistance(Transform a, Vector3 b, Transform parent = null)
    {
        return GetDistance(a.position, b, parent);
    }

    public static Vector3 GetDistance(Vector3 a, Vector3 b, Transform parent = null)
    {
        Vector3 distance = a - b;
        if (parent != null)
        {
            Quaternion inverseRotation = Quaternion.Inverse(parent.transform.rotation);
            distance = inverseRotation * distance;
        }

        return distance;
    }

    public static Vector3[] OrthogonalDecomposition(Vector3 vector, Vector3 subDirection)
    {
        
        Vector3 subVector2 = Vector3.ProjectOnPlane(vector, subDirection);
        Vector3 subVector1 = vector - subVector2;
        return new Vector3[] { subVector1, subVector2 };

    }
    public static Vector3 ArrayToV3(float[] a)
    {
        if (a.Length != 3)
        {
            throw new ArgumentException();
        }

        return new Vector3(a[0], a[1], a[2]);
    }
    public static Vector3 V3AddArray(Vector3 v3, float[] a)
    {
        if (a.Length != 3)
        {
            throw new ArgumentException();
        }
        return v3 + ArrayToV3(a);
    }
}
