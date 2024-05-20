using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckButtonInfo : MonoBehaviour
{
    private bool _isSelected;
    
    public int DeckIndex { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            var buttonImage = GetComponent<Image>();
            buttonImage.color = _isSelected 
                ? new Color(171f/255f, 140f/255f, 64f/255f) 
                : new Color(248f/255f, 211f/255f, 123f/255f);
        }
    }
}
