using Google.Protobuf.Protocol;

public class MosquitoPesterController : MosquitoBugController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.MosquitoPester;
    }

    protected override void OnHitEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            AttackMethod = AttackMethod.ProjectileAttack,
            Effect = EffectId.None,
            Projectile = ProjectileId.MosquitoPesterProjectile
        });
    }
}
