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
            
            if (value)
            {
                appliedLabel.gameObject.SetActive(true);
                groupPrice.gameObject.SetActive(false);
            }
            else
            {
                appliedLabel.gameObject.SetActive(false);
                groupPrice.gameObject.SetActive(true);
            }
            
            Debug.Log($"Ads Remover Applied set to {value}");
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
