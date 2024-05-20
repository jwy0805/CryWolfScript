using Google.Protobuf.Protocol;

public class SnakeNagaController : SnakeController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.SnakeNaga;
        OriginalAttack = ProjectileId.SmallFire;
        UpgradedAttack = ProjectileId.BigFire;
        CurrentAttack = OriginalAttack;
    }
    
    protected override void OnSkillEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id,
            AttackMethod = AttackMethod.EffectAttack,
            Effect = EffectId.Meteor,
            Projectile = ProjectileId.None
        });
    }
    
    public override void OnSkillUpdated(int id, GameObjectType type, SkillType skillType, int step)
    {
        if (id == (int)UnitId.SnakeNaga && type == GameObjectType.Monster) CurrentAttack = UpgradedAttack;
    }
}
