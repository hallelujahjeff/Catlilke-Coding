using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Color = System.Drawing.Color;

[RequireComponent(typeof(Rigidbody))]
public class CustomGravityRigidbody : MonoBehaviour
{
    private Rigidbody body;
    private float floatDelay;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
    }
    void Start()
    {
        
    }
    
    void FixedUpdate()
    {
        if (body.IsSleeping())
        {
            GetComponent<Renderer>().material.SetColor("_Color",UnityEngine.Color.gray);
            floatDelay = 0f;
            return;
        }
        GetComponent<Renderer>().material.SetColor("_Color",floatDelay < 1f ? UnityEngine.Color.red : UnityEngine.Color.yellow);
        if (body.velocity.sqrMagnitude < 0.0001f)
        {
            floatDelay += Time.deltaTime;
            if (floatDelay >= 1f)
                return;
        }
        else
        {
            floatDelay = 0f;
        }
        body.AddForce(CustomGravity.GetGravity(body.position),ForceMode.Acceleration);
    }
}
