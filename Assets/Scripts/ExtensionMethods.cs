using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{

    public static Vector3 TriangleNormal(this Vector3 v0, Vector3 v1, Vector3 v2)
    {

        Vector3 a = v0 - v1;
        Vector3 b = v0 - v2;
        return Vector3.Cross(a, b);
    }
    public static Quaternion GetInverse(this Vector3 p1, Vector3 p2, Vector3 forward)
    {
        return Quaternion.Inverse(Quaternion.LookRotation(p1 - p2, forward));
    }

    public static void DrawNormal(Vector3 origin,Vector3 normal ,float distance = 1000)
    {
        
        Debug.DrawRay(origin,normal * distance,Color.blue);
    }
}
