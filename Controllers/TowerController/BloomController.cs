using Google.Protobuf.Protocol;

public class BloomController : BudController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Bloom;
        AttackAnimValue = 2 / 3f;
    }
}
