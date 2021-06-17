using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using Color = System.Drawing.Color;

public class GravityPlane : GravitySource
{
    [SerializeField] private float gravity = 9.81f;
    [SerializeField, Min(0f)] private float range = 1f;
    
    void OnDrawGizmos ()
    {
        Vector3 center = new Vector3(10f, 0f, 10f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Vector3 size = new Vector3(20f, 0f, 20f);
        Gizmos.color = UnityEngine.Color.yellow;
        Gizmos.DrawWireCube(center, size);
        Gizmos.color = UnityEngine.Color.cyan;
        Gizmos.DrawWireCube(center + new Vector3(0f,range,0f), size);
    }

    public override Vector3 GetGravity(Vector3 position)
    {
        Vector3 up = transform.up;
        float distance = Vector3.Dot(up, position - transform.position);
        if (distance > range)
            return Vector3.zero;
        float g = -gravity;
        if (distance > 0f)
            g *= 1f - distance / range;
        return g * up;
    }   
}
    