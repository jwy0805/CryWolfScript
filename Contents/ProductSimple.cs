using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProductSimple : GameProduct
{
    private void Start()
    {
        SetProductText();
    }

    public void SetProductText()
    {
        var titleText = Util.FindChild<TextMeshProUGUI>(gameObject, "TextName", true);
        titleText.text = Managers.Localization.BindLocalizedText(titleText, ProductInfo.ProductCode);
    }
}
