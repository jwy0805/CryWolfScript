using Google.Protobuf.Protocol;
using UnityEngine;

public class PortalController : CreatureController
{
    private readonly float _destroySpeed = 2f;

    protected override void Init()
    {
        ObjectType = GameObjectType.Portal;
        Managers.Resource.Instantiate("WorldObjects/HealthSlider", transform);
        Util.GetOrAddComponent<UI_HealthBar>(gameObject);
    }

    protected override void UpdateDie()
    {
        transform.position += Vector3.down * (_destroySpeed * Time.deltaTime);
    }
}
