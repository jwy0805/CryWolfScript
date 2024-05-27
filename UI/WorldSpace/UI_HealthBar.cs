using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;

public class UI_HealthBar : MonoBehaviour
{
    private Slider _slider; 
    private Image _sliderFill;
    private StatInfo _stat;
    private Transform _healthSlider;
    private Camera _camera;
    
    void Start()
    {
        GameObject go = gameObject;
        _stat = go.GetComponent<CreatureController>().Stat;
        _healthSlider = Util.FindChild(go, "HealthSlider", false, true).transform;
        _slider = Util.FindChild(_healthSlider.gameObject, "Slider", true, true).GetComponent<Slider>();
        _sliderFill = _slider.fillRect.gameObject.GetComponent<Image>();
        _camera = Camera.main;
    }

    void Update()
    {
        float ratio = (_stat.Hp / (float)_stat.MaxHp) * 100;
        _slider.value = ratio;
        _healthSlider.rotation = _camera.transform.rotation;
        _healthSlider.gameObject.SetActive(!(ratio >= 99.8f));

        switch (ratio)
        {
            case > 70.0f:
                _sliderFill.color = Color.green;
                break;
            case < 30.0f:
                _sliderFill.color = Color.red;
                break;
            default:
                _sliderFill.color = Color.yellow;
                break;
        }
    }
}
