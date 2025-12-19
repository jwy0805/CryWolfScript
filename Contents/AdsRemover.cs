using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class AdsRemover : ProductSimple
{
    public bool Applied
    {
        set
        {
            var appliedLabel = Util.FindChild(gameObject, "AppliedFlag", true, true);
            var groupPrice = Util.FindChild(gameObject, "GroupPrice", true);

            if (appliedLabel == null || groupPrice == null) return;
            if (value)
            {
                appliedLabel.gameObject.SetActive(true);
                appliedLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-145, -85);
                groupPrice.gameObject.SetActive(false);
            }
            else
            {
                appliedLabel.gameObject.SetActive(false);
                groupPrice.gameObject.SetActive(true);
            }
        }
    }
    
    private async void Start()
    {
        try
        {
            Init();

            var removerMessageKey = "AdsRemoverMessage";
            var removerApplyKey = "AdsRemoverAppliedText";
            var removerName = Util.FindChild<TextMeshProUGUI>(gameObject, "TextName", true, true);
            var removerMessage = Util.FindChild<TextMeshProUGUI>(gameObject, "AdsRemoverMessage", true, true);
            var removerApply = Util.FindChild<TextMeshProUGUI>(gameObject, "AdsRemoverAppliedText", true, true);
        
            await Managers.Localization.BindLocalizedText(removerName, "ads_remover_title");
            await Managers.Localization.BindLocalizedText(removerMessage, removerMessageKey);
            await Managers.Localization.BindLocalizedText(removerApply, removerApplyKey);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
}
