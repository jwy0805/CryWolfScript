using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ReinforceArrowController : MonoBehaviour
{
    private Image[] _arrows;

    private void Start()
    {
        _arrows = GetComponentsInChildren<Image>().Where(image => image.gameObject != gameObject).ToArray();
    }

    public void SetArrowColors(float percentage) // ex) 0.75 0.66 e.g.
    {
        var fullArrows = (int)(percentage * 4);
        var remainder = 0.2f + percentage * 0.62f;

        for (var i = 0; i < _arrows.Length; i++)
        {
            _arrows[i].color = percentage < 0.25f ? Color.red : percentage < 0.5 ? Color.yellow : Color.green;
            
            if (i < fullArrows)
            {
                _arrows[i].fillAmount = 1f;
            }
            else if (i == fullArrows)
            {
                _arrows[i].fillAmount = remainder;
            }
            else
            {
                _arrows[i].fillAmount = 0f;
            }
        }
    }
}
