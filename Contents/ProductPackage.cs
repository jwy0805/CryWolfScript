using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProductPackage : GameProduct
{
    private void Start()
    {
        SetProductText();
    }

    public void SetProductText()
    {
        var titleText = Util.FindChild<TextMeshProUGUI>(gameObject, "TextName", true);
        if (ProductInfo == null) return;
        titleText.text = Managers.Localization.BindLocalizedText(titleText, ProductInfo.ProductCode);
    }
}
