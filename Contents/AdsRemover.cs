using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class AdsRemover : ProductSimple
{
    private async void Start()
    {
        try
        {
            Init();

            var appliedLabel = Util.FindChild(gameObject, "AppliedLabel", true, true);
            var removerMessageKey = "AdsRemoverMessage";
            var removerApplyKey = "AdsRemoverAppliedText";
            var removerName = Util.FindChild<TextMeshProUGUI>(gameObject, "TextName", true);
            var removerMessage = Util.FindChild<TextMeshProUGUI>(gameObject, removerMessageKey, true);
            var removerApply = Util.FindChild<TextMeshProUGUI>(gameObject, removerApplyKey, true, true);
            var groupPrice = Util.FindChild(gameObject, "GroupPrice", true);
        
            await Managers.Localization.BindLocalizedText(removerName, "ads_remover_title");
            await Managers.Localization.BindLocalizedText(removerMessage, removerMessageKey);
            await Managers.Localization.BindLocalizedText(removerApply, removerApplyKey);

            if (User.Instance.SubscribeAdsRemover == false)
            {
                appliedLabel.SetActive(false);
            }
            else
            {
                groupPrice.SetActive(false);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
}
