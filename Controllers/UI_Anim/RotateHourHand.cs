using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateHourHand : MonoBehaviour
{
    private Quaternion _rotation;
    private readonly float _rotationSpeed = -18.5f;
    
    public bool RotationStart { get; set; }
    
    private void Start()
    {
        RotationStart = false;
        _rotation = Quaternion.identity;
    }

    private void Update()
    {
        if (RotationStart == false) return;
        
        float rotationAmount = _rotationSpeed * Time.deltaTime;
        Quaternion deltaRotation = Quaternion.Euler(0f, 0f, rotationAmount);
        _rotation *= deltaRotation;
        
        transform.rotation = Quaternion.Slerp(transform.rotation, _rotation, Time.deltaTime);
    }

    public void InitializingRotation()
    {
        transform.rotation = Quaternion.identity;
        _rotation = Quaternion.identity;
    }
}
