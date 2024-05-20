using Google.Protobuf.Protocol;

public class BloomController : BudController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Bloom;
        CurrentAttack = ProjectileId.SeedProjectile;
    }

    protected override void OnHitEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            AttackMethod = AttackMethod.ProjectileAttack,
            Effect = EffectId.None,
            Projectile = CurrentAttack
        });
    }

    protected override void OnSkillEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            AttackMethod = AttackMethod.ProjectileAttack,
            Effect = EffectId.None,
            Projectile = CurrentAttack
        });
    }
}
