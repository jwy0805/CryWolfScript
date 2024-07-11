using UnityEngine;
using UnityEngine.UI;

public class UI_HealthCircle : MonoBehaviour
{
    private Slider _hpSlider;
    private Slider _shieldSlider;
    private Image _hpSliderFill;
    private Image _shieldSliderFill;
    private CreatureController _cc;
    private RectTransform _hpRect;
    private RectTransform _shieldRect;
    
    private void Start()
    {
        _hpSlider = Util.FindChild<Slider>(gameObject, "HpSlider", true, true);
        _shieldSlider = Util.FindChild<Slider>(gameObject, "ShieldSlider", true, true);
        _hpSliderFill = Util.FindChild<Image>(_hpSlider.gameObject, "Fill", true, true);
        _shieldSliderFill = Util.FindChild<Image>(_shieldSlider.gameObject, "Fill", true, true);
        
        _cc = gameObject.GetComponent<CreatureController>();
        var radius = gameObject.GetComponent<CapsuleCollider>().radius;
        _hpRect = _hpSlider.GetComponent<RectTransform>();
        _shieldRect = _shieldSlider.GetComponent<RectTransform>();
        _hpRect.localScale = new Vector3(radius * 2f, radius * 2f, 1);
        _shieldRect.localScale = new Vector3(radius * 2f, radius * 2f, 1);
    }
    
    private void Update()
    {
        if (_cc.ShieldAdd <= 0)
        {
            _shieldSlider.value = 0;
            _shieldSlider.gameObject.SetActive(false);
        }
        else
        {
            _shieldSlider.gameObject.SetActive(true);
            float shieldRatio = _cc.ShieldRemain / (float)_cc.ShieldAdd * 100;
            _shieldSlider.value = shieldRatio;
            _shieldSliderFill.color = Color.blue;
        }
        
        float ratio = _cc.Hp / (float)_cc.MaxHp * 100;
        _hpSlider.value = ratio;
        switch (ratio)
        {
            case > 70.0f:
                _hpSliderFill.color = Color.green;
                break;
            case < 30.0f:
                _hpSliderFill.color = Color.red;
                break;
            default:
                _hpSliderFill.color = Color.yellow;
                break;
        }
    }
}
