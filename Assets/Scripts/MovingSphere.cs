using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    [SerializeField,Range(0f,100f)]
    float maxSpeed = 10f;
    [SerializeField,Range(0f,100f)]
    float maxAcceleration = 50f;
    
    Vector3 velocity;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        // if(velocity.x < desiredVelocity.x)
        //     velocity.x = Mathf.Min(velocity.x + maxSpeedChange,desiredVelocity.x);
        velocity.x = Mathf.MoveTowards(velocity.x,desiredVelocity.x,maxSpeedChange);    //与上方等价
        // else if(velocity.x > desiredVelocity.x)
        //     velocity.x = Mathf.Max(velocity.x - maxSpeedChange,desiredVelocity.x);
        velocity.z = Mathf.MoveTowards(velocity.z,desiredVelocity.z,maxSpeedChange);
        Vector3 displacement = velocity * Time.deltaTime;
        Vector3 newPosition = transform.localPosition + displacement;
        
        transform.localPosition = newPosition;
    }
}
