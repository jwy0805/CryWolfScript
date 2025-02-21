using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProductPackage : GameProduct
{
    private void Start()
    {
        var titleText = Util.FindChild<TextMeshProUGUI>(gameObject, "TextName", true);
        if (ProductInfo == null) return;
        titleText.text = Managers.Localization.GetLocalizedValue(titleText, ProductInfo.ProductCode);
    }
}
