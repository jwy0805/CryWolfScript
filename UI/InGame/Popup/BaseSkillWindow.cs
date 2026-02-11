using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    private TutorialViewModel _tutorialVm;

    private readonly Dictionary<string, GameObject> _buttonDict = new();
    
    [Inject]
    public void Construct(GameViewModel gameViewModel, TutorialViewModel tutorialViewModel)
    {
        _gameVm = gameViewModel;
        _tutorialVm = tutorialViewModel;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
            _gameVm.BaseSkillWindow = this;
            _gameVm.UpdateBaseSkillCostRequired();

            BindObjects();
            await InitUIAsync();
            InitButtonEvents();
        
            // Tutorial
            if (_tutorialVm.NextTag.Contains("BaseSkillWindow"))
            {
                _tutorialVm.StepTutorialByClickingUI();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    protected override async Task InitUIAsync()
    {
        var baseUpgradeString = Util.Faction == Faction.Sheep ? "SheepBase" : "WolfPortal";
        var resourceString = Util.Faction == Faction.Sheep ? "SheepResource" : "WolfResource";
        var assetString = Util.Faction == Faction.Sheep ? "SheepAsset" : "WolfAsset";
        
        var baseUpgradeButtonImage = _buttonDict["BaseUpgradeButton"].GetComponent<Image>();
        var repairButtonImage = _buttonDict["RepairButton"].GetComponent<Image>();
        var resourceButtonImage = _buttonDict["ResourceButton"].GetComponent<Image>();
        var assetButtonImage = _buttonDict["AssetButton"].GetComponent<Image>();
        
        var baseUpgradeText = GetText((int)Texts.BaseUpgradeText);
        var repairText = GetText((int)Texts.RepairText);
        var resourceText = GetText((int)Texts.ResourceText);
        var assetText = GetText((int)Texts.AssetText);
        
        var baseUpgradeIconPath = $"Sprites/Icons/SkillIcons/{baseUpgradeString}";
        var repairIconPath = $"Sprites/Icons/SkillIcons/RepairAll";
        var resourceIconPath = $"Sprites/Icons/SkillIcons/{resourceString}";
        var assetIconPath = $"Sprites/Icons/SkillIcons/{assetString}";
        var goldIconPath = Util.Faction == Faction.Sheep ? "Sprites/UIIcons/icon_coin" : "Sprites/UIIcons/icon_dna";
        
        var baseUpgradeKey = Util.Faction == Faction.Sheep ? "upgrade_base" : "upgrade_portal";
        var repairKey = Util.Faction == Faction.Sheep ? "repair_all_fences" : "repair_all_statues";
        var resourceKey = Util.Faction == Faction.Sheep ? "increase_gold" : "increase_dna";
        var assetKey = Util.Faction == Faction.Sheep ? "create_sheep" : "upgrade_enchant";
        
        baseUpgradeButtonImage.sprite = await Managers.Resource.LoadAsync<Sprite>(baseUpgradeIconPath);
        repairButtonImage.sprite = await Managers.Resource.LoadAsync<Sprite>(repairIconPath);
        resourceButtonImage.sprite = await Managers.Resource.LoadAsync<Sprite>(resourceIconPath);
        assetButtonImage.sprite = await Managers.Resource.LoadAsync<Sprite>(assetIconPath);
        
        GetImage((int)Images.BaseUpgradeGoldImage).sprite = await Managers.Resource.LoadAsync<Sprite>(goldIconPath);
        GetImage((int)Images.RepairGoldImage).sprite = await Managers.Resource.LoadAsync<Sprite>(goldIconPath);
        GetImage((int)Images.ResourceGoldImage).sprite = await Managers.Resource.LoadAsync<Sprite>(goldIconPath);
        GetImage((int)Images.AssetGoldImage).sprite = await Managers.Resource.LoadAsync<Sprite>(goldIconPath);

        await Managers.Localization.BindLocalizedText(baseUpgradeText, baseUpgradeKey);
        await Managers.Localization.BindLocalizedText(repairText, repairKey);
        await Managers.Localization.BindLocalizedText(resourceText, resourceKey);
        await Managers.Localization.BindLocalizedText(assetText, assetKey);
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
    
    private async Task OnSkillButtonClicked(PointerEventData data)
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
        await _gameVm.ShowUpgradePopupNoCost(skillNameCamp);
    }

    public void UpdateBaseSkillCost(S_SetBaseSkillCost packet)
    {
        GetText((int)Texts.BaseUpgradeGoldText).text = packet.UpgradeCost.ToString();
        GetText((int)Texts.RepairGoldText).text = packet.RepairCost.ToString();
        GetText((int)Texts.ResourceGoldText).text = packet.ResourceCost.ToString();
        GetText((int)Texts.AssetGoldText).text = packet.AssetCost.ToString();
    }
}
