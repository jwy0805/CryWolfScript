using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;

public class BasicProjectileController : ProjectileController
{
    private Rigidbody _rb;

    public float hitOffset = 0f;
    public bool useFirePointRotation;
    public Vector3 rotationOffset = new(0, 0, 0);
    public GameObject hit;
    public GameObject flash;
    public GameObject[] detached;

    private string[] _effects;
    
    protected override async void Init()
    {
        try
        {
            #region Effect
        
            _rb = GetComponent<Rigidbody>();

            flash = await Managers.Resource.LoadAsync<GameObject>($"Prefabs/Effects/Flashes/{gameObject.name}Flash", "prefab");
            if (flash != null)
            {
                //Instantiate flash effect on projectile position
                var flashInstance = Instantiate(flash, transform.position, Quaternion.identity);
                flashInstance.transform.forward = gameObject.transform.forward;
            
                //Destroy flash effect depending on particle Duration time
                var flashPs = flashInstance.GetComponent<ParticleSystem>();
                if (flashPs != null)
                {
                    Managers.Resource.Destroy(flashInstance, flashPs.main.duration);
                }
                else
                {
                    var flashPsParts = flashInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                    Managers.Resource.Destroy(flashInstance, flashPsParts.main.duration);
                }
            }

            _ = PlaySfx();

            #endregion
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    protected override void FixedUpdate()
    {
        Vector3 dir = DestPos - transform.position;
        
        if (detached.Length > 0)
        {
            foreach (var detachedPrefab in detached)
            {
                if (detachedPrefab == null || !detachedPrefab.TryGetComponent(out Renderer render)) continue;
                render.enabled = true;
            }
        }
        
        if (dir.sqrMagnitude < 0.01f)
        {
            _ = HitEffect();
        }
        else
        {
            float moveDist = Mathf.Clamp(Speed * Time.deltaTime, 0, dir.magnitude);
            transform.position += dir.normalized * moveDist;
            transform.LookAt(DestPos);
        }
    }
    
    private async Task HitEffect()
    {
        #region Effect
         
        //Lock all axes movement and rotation
        _rb.constraints = RigidbodyConstraints.FreezeAll;
        Speed = 0;
 
        Vector3 point = transform.position;
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, point.normalized);
        Vector3 pos = point + point.normalized * hitOffset;
 
        //Spawn hit effect on collision
        hit = await Managers.Resource.LoadAsync<GameObject>($"Prefabs/Effects/Hits/{gameObject.name}Hit", "prefab");
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
                Managers.Resource.Destroy(hitInstance, hitPs.main.duration);
            }
            else
            {
                var hitPsParts = hitInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                Managers.Resource.Destroy(hitInstance, hitPsParts.main.duration);
            }
        }

        _ = PlayHitSfx();

        //Removing trail from the projectile on collision enter or smooth removing. Detached elements must have "AutoDestroying script"
        foreach (var detachedPrefab in detached)
        {
            if (detachedPrefab != null)
            {
                detachedPrefab.transform.parent = null;
            }
        }
        
        //Destroy projectile on collision
        Managers.Object.Remove(Id);
        Managers.Resource.Destroy(gameObject);

        #endregion
    }

    private async Task PlaySfx()
    {
        if (Sound)
        {
            switch (gameObject.name)
            {
                case "BasicProjectile":
                    await Managers.Sound.PlaySfx3D("InGame/basic_projectile_1", transform.position);
                    break;
                default:
                    await Managers.Sound.PlaySfx3D($"InGame/{Util.ToSnakeCase(gameObject.name)}", transform.position);
                    break;
            }
        }
    }
    
    private async Task PlayHitSfx()
    {
        switch (gameObject.name)
        {
            case "BigPoison":
            case "FungiProjectile":
            case "MosquitoPesterProjectile":
            case "MosquitoStingerProjectile":
            case "MothCelestialPoison":
            case "ToadstoolProjectile":
            case "SmallPoison": 
                await Managers.Sound.PlaySfx3D("InGame/poison_projectile_hit", transform.position);
                break;
            case "HauntFire":
            case "SnakeFire":
            case "SnakeNagaFire": 
            case "Sprout3HitFire": 
            case "SproutFire":
                await Managers.Sound.PlaySfx3D("InGame/fire_projectile_hit", transform.position);
                break;
            default:
                await Managers.Sound.PlaySfx3D("InGame/projectile_hit", transform.position);
                break;
        }
    }
}
