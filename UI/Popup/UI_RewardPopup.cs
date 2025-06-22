using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public bool FromTutorial { get; set; }
    
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

    protected override async Task InitUIAsync()
    {
        foreach (var reward in Rewards)
        {
            switch (reward.ProductType)
            {
                case Google.Protobuf.Protocol.ProductType.Material:
                {
                    Managers.Data.MaterialInfoDict.TryGetValue(reward.ItemId, out var materialInfo);
                    if (materialInfo == null) continue;
                    var itemObject = await Managers.Resource.GetMaterialResources(materialInfo, _rewardPanel);
                    break;
                }
                case Google.Protobuf.Protocol.ProductType.Unit:
                {
                    // Unit
                    Managers.Data.UnitInfoDict.TryGetValue(reward.ItemId, out var unitInfo);
                    if (unitInfo == null) continue;
                    var itemObject = await Managers.Resource.GetCardResources<UnitId>(unitInfo, _rewardPanel);
                    break;
                }
                default:
                {
                    var itemObject = await Managers.Resource.Instantiate("UI/Deck/ItemFrameGold", _rewardPanel);
                    var countText = Util.FindChild(itemObject, "CountText", true, true);
                    countText.GetComponent<TextMeshProUGUI>().text = reward.Count.ToString();
                    break;
                }
            }
        }
    }
    
    private void OnPanelClicked(PointerEventData data)
    {
        var scene = FromRank ? Define.Scene.MainLobby : FromTutorial ? Define.Scene.MainLobby : Define.Scene.SinglePlay;
        Managers.Scene.LoadScene(scene);
        Managers.UI.ClosePopupUI();
    }
}
