using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProductSimple : Product
{
    private void Start()
    {
        var titleText = Util.FindChild<TextMeshProUGUI>(gameObject, "TextName", true);
        titleText.text = Managers.Localization.GetLocalizedValue(titleText, ProductInfo.ProductCode);
    }
}
