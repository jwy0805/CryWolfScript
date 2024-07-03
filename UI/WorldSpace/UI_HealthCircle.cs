using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UI_HealthCircle : MonoBehaviour
{
    [SerializeField] public Slider slider; 
    [SerializeField] private Image sliderFill;
    private CreatureController _cc;
    private RectTransform _rectTransform;
    
    private void Start()
    {
        _cc = gameObject.GetComponent<CreatureController>();
        _rectTransform = slider.GetComponent<RectTransform>();
        float multiplier = (float)(0.5 + _cc.Stat.SizeX * 0.25);
        _rectTransform.localScale = new Vector3(multiplier, multiplier, 1);
    }

    private void Update()
    {
        if (_cc.Shield > 0)
        {
            float ratio = _cc.Shield / (float)_cc.ShieldMax * 100;
            slider.value = ratio;
            sliderFill.color = Color.cyan;
        }
        else
        {
            float ratio = _cc.Hp / (float)_cc.MaxHp * 100;
            slider.value = ratio;
            switch (ratio)
            {
                case > 70.0f:
                    sliderFill.color = Color.green;
                    break;
                case < 30.0f:
                    sliderFill.color = Color.red;
                    break;
                default:
                    sliderFill.color = Color.yellow;
                    break;
            }
        }
    }
}
