using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;

public class UI_HealthBar : MonoBehaviour
{
    private Transform _slider;
    private RectTransform _sliderRect;
    private Slider _hpSlider;
    private Slider _shieldSlider;
    private Image _hpSliderFill;
    private Image _shieldSliderFill;
    private CreatureController _cc;
    private Camera _camera;
    
    void Start()
    {
        _slider = Util.FindChild(gameObject, "HealthSlider(Clone)", true, true).transform;
        _hpSlider = Util.FindChild<Slider>(_slider.gameObject, "HpSlider", true, true);
        _shieldSlider = Util.FindChild<Slider>(_slider.gameObject, "ShieldSlider", true, true);
        _hpSliderFill = Util.FindChild<Image>(_hpSlider.gameObject, "Fill", true, true);
        _shieldSliderFill = Util.FindChild<Image>(_shieldSlider.gameObject, "Fill", true, true);
        
        _cc = gameObject.GetComponent<CreatureController>();
        _sliderRect = _slider.GetComponent<RectTransform>();
        _camera = Camera.main;

        var type = _cc.ObjectType;
        switch (type)
        {
            case GameObjectType.Fence:
                var sizeX = gameObject.GetComponent<BoxCollider>().size.x;
                _sliderRect.localScale = new Vector3(0.005f * sizeX, 0.01f, 0.01f);
                break;
            default:
                var radius = gameObject.GetComponent<CapsuleCollider>().radius;
                _sliderRect.localScale = new Vector3(0.005f * radius, 0.01f, 0.01f);
                break;
        }
    }

    void Update()
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
        
        _slider.rotation = _camera.transform.rotation;
        float ratio = _cc.Hp / (float)_cc.MaxHp * 100;
        _hpSlider.value = ratio;
        _slider.gameObject.SetActive(ratio <= 99.8f || _shieldSlider.value > 0);
        
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
