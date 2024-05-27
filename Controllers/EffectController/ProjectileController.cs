using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Serialization;

public class ProjectileController : MonoBehaviour
{
    public Vector3 destPos = Vector3.zero;
    protected float Speed = 10f;
    
    public int Id { get; set; }

    protected virtual void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        Managers.Network.Send(new C_SetDest { ObjectId = Id });
    }
    
    protected virtual void FixedUpdate()
    {
        if (destPos == Vector3.zero) return;
        Vector3 dir = destPos - transform.position;
        if (dir.magnitude < 0.1f)
        {
            Managers.Network.Send(new C_Attack
            {
                ObjectId = Id, AttackMethod = AttackMethod.NormalAttack, Projectile = ProjectileId.BasicProjectile
            });
            
            Managers.Object.Remove(Id);
            Managers.Network.Send(new C_Leave { ObjectId = Id });
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
