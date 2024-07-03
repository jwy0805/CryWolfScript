using Google.Protobuf.Protocol;

public class SnakeNagaController : SnakeController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.SnakeNaga;
        AttackAnimValue = 5 / 6f;
    }
}
