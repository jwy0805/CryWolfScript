using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Serialization;

public class ProjectileController : MonoBehaviour
{
    public int Id { get; set; }
    public float Speed { get; set; }
    public Vector3 DestPos { get; set; } = Vector3.zero;

    protected virtual void Start()
    {
        Init();
    }

    protected virtual void Init() { }
    
    protected virtual void FixedUpdate()
    {
        Vector3 dir = DestPos - transform.position;
        if (dir.sqrMagnitude < 0.01f)
        {
            Managers.Object.Remove(Id);
            Managers.Resource.Destroy(gameObject);
        }
        else
        {
            float moveDist = Mathf.Clamp(Speed * Time.deltaTime, 0, dir.magnitude);
            transform.position += dir.normalized * moveDist;
            transform.LookAt(transform.position + dir.normalized);
        }
    }
}