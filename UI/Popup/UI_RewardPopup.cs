using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_RewardPopup : UI_Popup
{
    private Transform _rewardPanel;
    
    public List<Reward> Rewards { get; set; }
    public bool FromRank { get; set; }
    
    private enum Images
    {
        RewardPanel,
    }
    
    private enum Buttons
    {
        PanelButton,
    }

    private enum Texts
    {
        
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();
    }

    protected override void BindObjects()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        _rewardPanel = GetImage((int)Images.RewardPanel).transform;
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.PanelButton).gameObject.BindEvent(OnPanelClicked);
    }

    protected override void InitUI()
    {
        foreach (var reward in Rewards)
        {
            if (reward.ProductType == Google.Protobuf.Protocol.ProductType.Material)
            {
                Managers.Data.MaterialInfoDict.TryGetValue(reward.ItemId, out var materialInfo);
                if (materialInfo == null) continue;
                var itemObject = Managers.Resource.GetMaterialResources(materialInfo, _rewardPanel);
            }
            else
            {
                var itemObject = Managers.Resource.Instantiate("UI/Deck/ItemFrameGold", _rewardPanel);
                var countText = Util.FindChild(itemObject, "CountText", true, true);
                countText.GetComponent<TextMeshProUGUI>().text = reward.Count.ToString();
            }
        }
    }
    
    private void OnPanelClicked(PointerEventData data)
    {
        Managers.Scene.LoadScene(FromRank ? Define.Scene.MainLobby : Define.Scene.SinglePlay);
        Managers.UI.ClosePopupUI();
    }
}
