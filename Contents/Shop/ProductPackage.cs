using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ProductPackage : GameProduct
{
    private async void Start()
    {
        try
        {
            await SetProductText();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    public async Task SetProductText()
    {
        var titleText = Util.FindChild<TextMeshProUGUI>(gameObject, "TextName", true);
        if (ProductInfo == null) return;
        titleText.text = await Managers.Localization.BindLocalizedText(titleText, ProductInfo.ProductCode);
    }
}
