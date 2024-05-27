using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class BombSkillController : ProjectileController
{
    public float hitOffset = 0f;
    public bool useFirePointRotation;
    public Vector3 rotationOffset = new(0, 0, 0);
    public GameObject hit;
    public GameObject flash;
    public GameObject[] detached;

    private string[] _effects;
    
    protected override void Init()
    {
        Speed = 10f;
        Managers.Network.Send(new C_SetDest { ObjectId = Id });
    }
    
    protected override void FixedUpdate ()
    {
        if (destPos == Vector3.zero) return;
        Vector3 dir = destPos - transform.position;
        if (dir.magnitude < 0.2f)
        {
            Managers.Network.Send(new C_Attack
            {
                ObjectId = Id, AttackMethod = AttackMethod.NormalAttack, Projectile = ProjectileId.BombSkill
            });
            HitEffect();
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
    
    public void HitEffect()
    {
        #region Effect
         
                //Lock all axes movement and rotation
                Speed = 0;
         
                Vector3 point = transform.position;
                Quaternion rot = Quaternion.FromToRotation(Vector3.up, point.normalized);
                Vector3 pos = point + point.normalized * hitOffset;
         
                //Spawn hit effect on collision
                hit = Resources.Load<GameObject>($"Prefabs/Effects/Hits/{gameObject.name}Hit");
                if (hit != null)
                {
                    var hitInstance = Instantiate(hit, pos, rot);
                    if (useFirePointRotation) { hitInstance.transform.rotation = gameObject.transform.rotation * Quaternion.Euler(0, 180f, 0); }
                    else if (rotationOffset != Vector3.zero) { hitInstance.transform.rotation = Quaternion.Euler(rotationOffset); }
                    else { hitInstance.transform.LookAt(point + point.normalized); }
         
                    //Destroy hit effects depending on particle Duration time
                    var hitPs = hitInstance.GetComponent<ParticleSystem>();
                    if (hitPs != null)
                    {
                        Destroy(hitInstance, hitPs.main.duration);
                    }
                    else
                    {
                        var hitPsParts = hitInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                        Destroy(hitInstance, hitPsParts.main.duration);
                    }
                }
         
                //Removing trail from the projectile on collision enter or smooth removing. Detached elements must have "AutoDestroying script"
                foreach (var detachedPrefab in detached)
                {
                    if (detachedPrefab != null)
                    {
                        detachedPrefab.transform.parent = null;
                    }
                }
                //Destroy projectile on collision
                Managers.Resource.Destroy(gameObject);

                #endregion
    }
}
