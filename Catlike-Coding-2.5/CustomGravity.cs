using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGravity : MonoBehaviour
{
    public static Vector3 getUpAxis(Vector3 position)
    {
        return position.normalized;
    }

    public static Vector3 GetGravity(Vector3 position)
    {
        return position.normalized * Physics.gravity.y;
    }
    public static Vector3 GetGravity(Vector3 position,out Vector3 upAxis)   //返回相机的up向量
    {
        upAxis = position.normalized;   //返回相机的up向量
        return upAxis * Physics.gravity.y;
    }
}
