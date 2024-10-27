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
                ? Color.white
                : new Color(93/255f, 51/255f, 34/255f);
        }
    }
}
