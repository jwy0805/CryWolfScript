using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;

public class UI_HealthCircle : MonoBehaviour
{
    [SerializeField] public Slider _slider; 
    [SerializeField] private Image _sliderFill;
    private CreatureController _cc;
    private RectTransform _rectTransform;
    
    private void Start()
    {
        _cc = gameObject.GetComponent<CreatureController>();
        _rectTransform = _slider.GetComponent<RectTransform>();
        float multiplier = (float)(0.5 + _cc.Stat.SizeX * 0.25);
        _rectTransform.localScale = new Vector3(multiplier, multiplier, 1);
    }

    private void Update()
    {
        float ratio = (_cc.Hp / (float)_cc.MaxHp) * 100;
        _slider.value = ratio;
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
