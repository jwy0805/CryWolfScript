using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Serialization;

public class ProjectileController : MonoBehaviour
{
    public int Id { get; set; }
    public float Speed { get; set; }
    public Vector3 DestPos { get; set; } = Vector3.zero;
    public bool Sound { get; set; }

    protected virtual void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        _ = PlaySfx();
    }
    
    protected virtual void FixedUpdate()
    {
        Vector3 dir = DestPos - transform.position;
        if (dir.sqrMagnitude < 0.01f)
        {
            _ = PlayHitSfx();
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

    private async Task PlaySfx()
    {
        if (Sound)
        {
            var path = $"InGame/{Util.ToSnakeCase(gameObject.name)}";
            await Managers.Sound.PlaySfx3D(path, transform.position);   
        }
    }

    private async Task PlayHitSfx()
    {
        await Managers.Sound.PlaySfx3D("InGame/projectile_hit", transform.position);
    }
}