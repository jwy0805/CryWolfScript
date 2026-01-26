using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ProductSimple : GameProduct
{
    private void Start()
    {
        _ = SetProductText();
    }

    public async Task SetProductText()
    {
        var titleText = Util.FindChild<TextMeshProUGUI>(gameObject, "TextName", true);
        titleText.text = await Managers.Localization.BindLocalizedText(titleText, ProductInfo.ProductCode);
    }
}
