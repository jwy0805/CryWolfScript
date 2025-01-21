using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public interface IBaseSkillWindow
{
    void UpdateBaseSkillCost(S_SetBaseSkillCost packet);
}

public class BaseSkillWindow : UI_Popup, IBaseSkillWindow
{
    #region Enums
    
    private enum Buttons
    {
        BaseUpgradeButton,
        RepairButton,
        ResourceButton,
        AssetButton,
    }
    
    private enum Images
    {
        BaseSkillWindowPanel,
        
        BaseUpgradeButtonPanel,
        RepairButtonPanel,
        ResourceButtonPanel,
        AssetButtonPanel,
        
        BaseUpgradeGoldImage,
        RepairGoldImage,
        ResourceGoldImage,
        AssetGoldImage,
    }

    private enum Texts
    {
        BaseUpgradeText,
        RepairText,
        ResourceText,
        AssetText,
        
        BaseUpgradeGoldText,
        RepairGoldText,
        ResourceGoldText,
        AssetGoldText,
    }
    
    #endregion
    
    private GameViewModel _gameVm;

    private readonly Dictionary<string, GameObject> _buttonDict = new();
    
    [Inject]
    public void Construct(GameViewModel gameViewModel)
    {
        _gameVm = gameViewModel;
    }
    
    protected override void Init()
    {
        base.Init();
        _gameVm.BaseSkillWindow = this;
        _gameVm.SetBaseSkillCostRequired();

        BindObjects();
        InitUI();
        InitButtonEvents();
    }

    protected override void InitUI()
    {
        var resourceString = Util.Faction == Faction.Sheep ? "IncreaseSheepResource" : "IncreaseWolfResource";
        var assetString = Util.Faction == Faction.Sheep ? "CreateSheep" : "Enchant";
        var resourceButtonImage = _buttonDict["ResourceButton"].GetComponent<Image>();
        var assetButtonImage = _buttonDict["AssetButton"].GetComponent<Image>();
        var baseUpgradeText = GetText((int)Texts.BaseUpgradeText);
        var repairText = GetText((int)Texts.RepairText);
        var resourceText = GetText((int)Texts.ResourceText);
        var assetText = GetText((int)Texts.AssetText);
        
        resourceButtonImage.sprite = Managers.Resource.Load<Sprite>($"Sprites/Icons/icon_base_skill_{resourceString}");
        assetButtonImage.sprite = Managers.Resource.Load<Sprite>($"Sprites/Icons/icon_base_skill_{assetString}");

        baseUpgradeText.text = Util.Faction == Faction.Sheep ? "기지\n업그레이드" : "포탈\n업그레이드";
        repairText.text = Util.Faction == Faction.Sheep ? "모든 울타리\n수리" : "모든 석상\n수리";
        resourceText.text = Util.Faction == Faction.Sheep ? "획득 골드\n증가" : "획득 DNA\n증가";
        assetText.text = Util.Faction == Faction.Sheep ? "양 생성" : "주술 강화";
        
        SetObjectSize(GetImage((int)Images.BaseUpgradeButtonPanel).gameObject, 0.65f, 0.65f);
        SetObjectSize(GetImage((int)Images.RepairButtonPanel).gameObject, 0.65f, 0.65f);
        SetObjectSize(GetImage((int)Images.ResourceButtonPanel).gameObject, 0.65f, 0.65f);
        SetObjectSize(GetImage((int)Images.AssetButtonPanel).gameObject, 0.65f, 0.65f);
        SetObjectSize(GetImage((int)Images.BaseUpgradeGoldImage).gameObject, 0.15f, 0.15f);
        SetObjectSize(GetImage((int)Images.RepairGoldImage).gameObject, 0.15f, 0.15f);
        SetObjectSize(GetImage((int)Images.ResourceGoldImage).gameObject, 0.15f, 0.15f);
        SetObjectSize(GetImage((int)Images.AssetGoldImage).gameObject, 0.15f, 0.15f);
    }
    
    protected override void BindObjects()
    {
        BindData<Button>(typeof(Buttons), _buttonDict);
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    protected override void InitButtonEvents()
    {
        foreach (var button in _buttonDict.Values)
        {
            button.BindEvent(OnSkillButtonClicked);
            button.AddComponent<UI_Skill>();
        }
    }
    
    private void OnSkillButtonClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI<UI_UpgradePopupNoCost>();
        foreach (var skillButton in _buttonDict.Values)
        {
            Managers.Resource.GetFrameFromCardButton(skillButton.GetComponent<UI_Skill>()).color = Color.green;
        }

        var selectedSkillButton = data.pointerPress.GetComponent<UI_Skill>();
        if (selectedSkillButton == null) return;
        
        _gameVm.CurrentSelectedSkillButton = selectedSkillButton;
        Managers.Resource.GetFrameFromCardButton(_gameVm.CurrentSelectedSkillButton).color = Color.cyan;
        
        var skillName = _gameVm.CurrentSelectedSkillButton.Name.Replace("Button", "");
        var camp = Util.Faction.ToString();
        var skillNameCamp = $"{skillName}{camp}";
        _gameVm.ShowUpgradePopupNoCost(skillNameCamp);
    }

    public void UpdateBaseSkillCost(S_SetBaseSkillCost packet)
    {
        GetText((int)Texts.BaseUpgradeGoldText).text = packet.UpgradeCost.ToString();
        GetText((int)Texts.RepairGoldText).text = packet.RepairCost.ToString();
        GetText((int)Texts.ResourceGoldText).text = packet.ResourceCost.ToString();
        GetText((int)Texts.AssetGoldText).text = packet.AssetCost.ToString();
    }
}
