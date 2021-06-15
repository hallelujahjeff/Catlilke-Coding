using System;
using UnityEngine;
[RequireComponent(typeof(Camera))]

public class OrbitCamera : MonoBehaviour
{
    [SerializeField]
    Transform focus = default;
    [SerializeField, Range(1f, 20f)] 
    private float distance = 5f;
    [SerializeField, Min(0f)]
    float focusRadius = 1f;
    [SerializeField, Range(0f, 1f)]
    float focusCentering = 0.5f;
    [SerializeField, Range(1f, 360f)]
    float rotationSpeed = 180f;  //相机环绕旋转的速度
    [SerializeField, Range(-89f, 89f)]
    float minVerticalAngle = 30f, maxVerticalAngle = 60f;
    [SerializeField, Min(0f)]
    float alignDelay = 5f;  //相机自动对齐的延迟时间
    [SerializeField, Range(0f, 90f)]
    float alignSmoothRange = 45f;
    [SerializeField] 
    private LayerMask layermask = default;


    private Vector3 focusPoint, prevFocusPoint;
    
    [SerializeField]
    private Vector2 orbitAngles = new Vector2(45f, 0f);
    public Quaternion gravityAlignment = Quaternion.identity; //控制相机基于重力坐标系的旋转
    private Quaternion orbitRotation;
    
    private float lastMnualRotationTime; //最后一次手动旋转相机的时刻
    [SerializeField]
    private Camera regularCamera;

    Vector3 CameraHalfExtends {
        get {
            Vector3 halfExtends;
            halfExtends.y =
                regularCamera.nearClipPlane *
                Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
            halfExtends.x = halfExtends.y * regularCamera.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }

    private void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle)
        {
            maxVerticalAngle = minVerticalAngle;
        }
    }

    private void Awake()
    {
        regularCamera = GetComponent<Camera>();
        focusPoint = focus.position;
        transform.localRotation = orbitRotation = Quaternion.Euler(orbitAngles);
    }
    private void LateUpdate()   //移动相机的逻辑
    {
        //更新相机基于重力的旋转量
        gravityAlignment =
            Quaternion.FromToRotation(
                gravityAlignment * Vector3.up, CustomGravity.getUpAxis(focusPoint)
            ) * gravityAlignment;
        
        UpdateFocusPoint();
        if (ManualRotation() || AutomaticRotation())
        {
            ConstrainAngle();
            orbitRotation = Quaternion.Euler(orbitAngles);
        }

        Quaternion lookRotation = gravityAlignment * orbitRotation;
        
        Vector3 lookDirection = lookRotation * Vector3.forward; //相机的方向
        Vector3 lookPosition = focusPoint - lookDirection * distance;   //相机的位置
    
        //从焦点向相机位置发出射线，检测第一个障碍物，将相机移动到障碍物前方位置。
        if (Physics.BoxCast(focusPoint,
            CameraHalfExtends,
            -lookDirection,
            out RaycastHit hit,
            lookRotation,
            distance - regularCamera.nearClipPlane,
            layermask))
        {
            lookPosition = focusPoint -
                           lookDirection * (hit.distance + regularCamera.nearClipPlane);
        }
        transform.SetPositionAndRotation(lookPosition,lookRotation);
    }

    void UpdateFocusPoint()
    {
        prevFocusPoint = focusPoint;
        Vector3 targetPoint = focus.position;
        if (focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, focusPoint);
            float t = 1f;
            if (distance > 0.01f && focusCentering > 0f)
                t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
            if (distance > focusRadius)
            {
                t = Mathf.Min(t, focusRadius / distance);
            }
            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }
        else
        {
            focusPoint = targetPoint;
        }
        //Debug.LogFormat("{0}{1}",focusPoint,prevFocusPoint);
    }

    bool ManualRotation()
    {
        Vector2 input = new Vector2(
            UnityEngine.Input.GetAxis("Camera Vertical"),
            UnityEngine.Input.GetAxis("Camera Horizontal")
        );
        const float e = 0.001f;
        if (input.x < -e || input.x > e || input.y < -e || input.y > e)
        {
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
            lastMnualRotationTime = Time.unscaledTime;
            return true;    //相机有变动
        }
        return false;
    }

    bool AutomaticRotation()
    {
        if (Time.unscaledTime - lastMnualRotationTime < alignDelay)
            return false;
        //Debug.LogFormat("{0}{1}",focusPoint,prevFocusPoint);
        
        Vector3 alignedDelta = Quaternion.Inverse(gravityAlignment) * (focusPoint - prevFocusPoint);
        Vector2 movement = new Vector2(alignedDelta.x, alignedDelta.z);
        
        float movementDeltaSqr = movement.sqrMagnitude; //sqrMagnitude指长度的平方
        if (movementDeltaSqr < 0.000001f)
            return false;

        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr)); //传入一个单位向量
        float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
        //float rotationChange = rotationSpeed * Time.unscaledDeltaTime;
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
        if (deltaAbs < alignSmoothRange) 
        {
            rotationChange *= deltaAbs / alignSmoothRange;
        }
        else if (180f - deltaAbs < alignSmoothRange) 
        {
            rotationChange *= (180f - deltaAbs) / alignSmoothRange;
        }
        
        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
        //orbitAngles.y = headingAngle;
        return true;
    }
    

    void ConstrainAngle()
    {
        orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);
        if (orbitAngles.y < 0f)
            orbitAngles.y += 360f;
        else if (orbitAngles.y >= 360f)
            orbitAngles.y -= 360f;
    }
    
    static float GetAngle (Vector2 direction) { //根据二维坐标系y返回对应角度值
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle;
    }
    
}