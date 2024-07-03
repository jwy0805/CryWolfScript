using Google.Protobuf.Protocol;

public class WerewolfController : WolfController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Werewolf;
        AttackAnimValue = 5 / 6f;
    }
}
