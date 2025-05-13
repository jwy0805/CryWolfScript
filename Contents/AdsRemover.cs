using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class AdsRemover : GameProduct
{
    private void Start()
    {
        var appliedLabel = Util.FindChild(gameObject, "AppliedLabel", true);
        var removerTitleName = "AdsRemoverTitle";
        var removerMessageName = "AdsRemoverMessage";
        var removerApplyName = "AdsRemoverAppliedText";
        var removerTitle = Util.FindChild(gameObject, removerTitleName, true);
        var removerMessage = Util.FindChild(gameObject, removerMessageName, true);
        var removerApply = Util.FindChild(gameObject, removerApplyName, true);
        var groupPrice = Util.FindChild(gameObject, "GroupPrice", true);
        
        Managers.Localization.GetLocalizedValue(removerTitle.GetComponent<TextMeshProUGUI>(), removerTitleName);
        Managers.Localization.GetLocalizedValue(removerMessage.GetComponent<TextMeshProUGUI>(), removerMessageName);
        Managers.Localization.GetLocalizedValue(removerApply.GetComponent<TextMeshProUGUI>(), removerApplyName);

        if (User.Instance.SubscribeAdsRemover == false)
        {
            appliedLabel.SetActive(false);
        }
        else
        {
            groupPrice.SetActive(false);
        }
    }
}
