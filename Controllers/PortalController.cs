using Google.Protobuf.Protocol;
using UnityEngine;

public class PortalController : CreatureController
{
    private readonly float _destroySpeed = 2f;
    private GameObject _rune2;
    private GameObject _rune3;
    private GameObject _rune5;
    private GameObject _rune6;
    private GameObject _rune8;
    private GameObject _trails;

    protected override void Init()
    {
        ObjectType = GameObjectType.Portal;
        Managers.Resource.Instantiate("WorldObjects/HealthSlider", transform);
        Util.GetOrAddComponent<UI_HealthBar>(gameObject);
        
        _rune2 = transform.Find("Rune2").gameObject;
        _rune3 = transform.Find("Rune3").gameObject;
        _rune5 = transform.Find("Rune5").gameObject;
        _rune6 = transform.Find("Rune6").gameObject;
        _rune8 = transform.Find("Rune8").gameObject;
        _trails = transform.Find("Trails").gameObject;
        
        _rune2.SetActive(false);
        _rune3.SetActive(false);
        _rune5.SetActive(false);
        _rune6.SetActive(false);
        _rune8.SetActive(false);
        _trails.SetActive(false);
    }

    public void UpgradePortal(int level)
    {
        if (level == 2)
        {
            _rune2.SetActive(true);
            _rune3.SetActive(true);
            _rune5.SetActive(true);
            _rune6.SetActive(true);
            _rune8.SetActive(true);
        }
        else if (level == 3)
        {
            _trails.SetActive(true);
        }
    }
    
    protected override void UpdateDie()
    {
        transform.position += Vector3.down * (_destroySpeed * Time.deltaTime);
    }
}
