using Google.Protobuf.Protocol;

public class WerewolfController : WolfController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Werewolf;
    }
    
    protected override void OnSkillEvent()
    {
        Managers.Network.Send(new C_Attack
        {
            ObjectId = Id, AttackMethod = AttackMethod.EffectAttack, Effect = EffectId.LightningStrike
        });
    }
}
