using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleIconSizeController : MonoBehaviour
{
    public float WidthRatio { get; set; } = 0.22f;
    private readonly float _childWidthRatio = 0.6f;
    
    private void Start()
    {
        StartCoroutine(DelayedAdjustSize());
    }
    
    private IEnumerator DelayedAdjustSize()
    {
        yield return new WaitForEndOfFrame();
        AdjustSize();
    }

    private void AdjustSize()
    {
        var parent = transform.parent;
        var parentRect = parent.GetComponent<RectTransform>();
        var rect = GetComponent<RectTransform>();
        var childRect = Util.FindChild(gameObject, "RoleIcon").GetComponent<RectTransform>();
        var parentRectWidth = parentRect.rect.width;
        
        rect.sizeDelta = new Vector2(parentRectWidth * WidthRatio, parentRectWidth * WidthRatio);
        rect.anchoredPosition = new Vector2(parentRectWidth * 0.2f, -parentRectWidth * 0.2f);
        childRect.sizeDelta = new Vector2(rect.rect.width * _childWidthRatio, rect.rect.width * _childWidthRatio);
    }

    private void OnRectTransformDimensionsChange()
    {
        AdjustSize();
    }
}
