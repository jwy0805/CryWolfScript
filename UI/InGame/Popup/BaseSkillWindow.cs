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
        _gameVm.SubResourceWindow = this;
        
        BindObjects();
        InitUI();
        InitButtonEvents();
    }

    protected override void InitUI()
    {
        var resourceString = Util.Camp == Camp.Sheep ? "IncreaseSheepResource" : "IncreaseWolfResource";
        var assetString = Util.Camp == Camp.Sheep ? "CreateSheep" : "Enchant";
        var resourceButtonImage = _buttonDict["ResourceButton"].GetComponent<Image>();
        var assetButtonImage = _buttonDict["AssetButton"].GetComponent<Image>();
        var baseUpgradeText = GetText((int)Texts.BaseUpgradeText);
        var repairText = GetText((int)Texts.RepairText);
        var resourceText = GetText((int)Texts.ResourceText);
        var assetText = GetText((int)Texts.AssetText);
        
        resourceButtonImage.sprite = Managers.Resource.Load<Sprite>($"Sprites/Icons/icon_base_skill_{resourceString}");
        assetButtonImage.sprite = Managers.Resource.Load<Sprite>($"Sprites/Icons/icon_base_skill_{assetString}");

        baseUpgradeText.text = Util.Camp == Camp.Sheep ? "기지\n업그레이드" : "포탈\n업그레이드";
        repairText.text = Util.Camp == Camp.Sheep ? "모든 울타리\n수리" : "모든 석상\n수리";
        resourceText.text = Util.Camp == Camp.Sheep ? "획득 골드\n증가" : "획득 DNA\n증가";
        assetText.text = Util.Camp == Camp.Sheep ? "양 생성" : "주술 강화";
        
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
        }
    }
    
    private void OnSkillButtonClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI<UI_UpgradePopup>();
        if (_gameVm.CurrentSelectedSkillButton != null)
        {
            var oldImage = GetImageFromButton(_gameVm.CurrentSelectedSkillButton);
            oldImage.color = Color.green;
        }
        
        var selectedSkillButton = data.pointerPress.GetComponent<ISkillButton>();
        if (selectedSkillButton == null) return;
        
        _gameVm.CurrentSelectedSkillButton = selectedSkillButton;
        var newImage = GetImageFromButton(_gameVm.CurrentSelectedSkillButton);
        if (newImage != null)
        {
            newImage.color = Color.blue;
        }
        
        Managers.UI.ShowPopupUiInGame<UI_UpgradePopup>();
        var skillName = _gameVm.CurrentSelectedSkillButton.Name.Replace("Button", "");
        var camp = Util.Camp.ToString();
        var skillNameCamp = $"{skillName}{camp}";
        if (Enum.TryParse(skillNameCamp, out Skill skill))
        {
            Managers.Network.Send(new C_SetUpgradePopup { SkillId = (int)skill });
        }
    }
    
    private Image GetImageFromButton(ISkillButton button)
    {
        if (button is not MonoBehaviour mono) return null;
        var go = mono.gameObject;
        return go.transform.parent.parent.GetChild(1).GetComponent<Image>();
    }
}
