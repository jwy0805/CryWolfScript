using Google.Protobuf.Protocol;

public class PortalController : CreatureController
{
    protected override void Init()
    {
        ObjectType = GameObjectType.Portal;
    }
    
    protected override void Update() { }
}
