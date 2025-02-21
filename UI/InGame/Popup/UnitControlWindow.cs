using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public interface IUnitControlWindow
{
    GameObject SelectedUnit { get; set; }
    void UpdateUpgradeCostText(int cost);
    void UpdateDeleteCostText(int cost);
    void UpdateRepairCostText(int cost);
}

public class UnitControlWindow : UI_Popup, IUnitControlWindow
{
    #region Enums

    private enum Buttons
    {
        UnitUpgradeButton,
        UnitDeleteButton,
        UnitRepairButton,
        UnitSkillButton,
    }

    private enum Images
    {
        UnitUpgradePanel,
        UnitDeletePanel,
        UnitRepairPanel,
        UnitSkillPanel,
        
        UnitUpgradeButtonPanel,
        UnitDeleteButtonPanel,
        UnitRepairButtonPanel,
        UnitSkillButtonPanel,

        UnitUpgradeGoldImage,
        UnitDeleteGoldImage,
        UnitDeleteGoldPlusImage,
        UnitRepairGoldImage,
    }
    
    private enum Texts
    {
        UnitUpgradeGoldText,
        UnitDeleteGoldText,
        UnitRepairGoldText,
        HpText,
        MpText
    }

    #endregion
    
    private GameViewModel _gameVm;

    private int _hp;
    private int _maxHp;
    private int _mp;
    private int _maxMp;
    private CreatureController _cc;
    private GameObject _selectedUnit;
    private TextMeshProUGUI _hpText;
    private TextMeshProUGUI _mpText;
    private Camera _portraitCamera;
    
    public GameObject SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            _selectedUnit = value;
            _cc = _selectedUnit.GetComponent<CreatureController>();
            
            // set selected portrait 
            _gameVm.SetPortraitFromFieldUnit(_cc.UnitId);
        }
    }
    
    [Inject]
    public void Construct(GameViewModel gameViewModel)
    {
        _gameVm = gameViewModel;
    }

    protected override void Init()
    {
        base.Init();
        _gameVm.UnitControlWindow = this;
        
        BindObjects();
        InitUI();
        InitButtonEvents();
        InitCamera();
    }

    protected void FixedUpdate()
    {
        if (_cc == null) return;
        
        var offset = _cc.transform.forward * _cc.Stat.SizeX;
        _portraitCamera.transform.position = _cc.transform.position + offset + Vector3.up * _cc.Stat.SizeY;
        
        if (_hp == _cc.Hp && _maxHp == _cc.MaxHp && _mp == _cc.Mp && _maxMp == _cc.MaxMp) return;
        _hpText.text = $"{_cc.Hp} / {_cc.MaxHp}";
        _mpText.text = _maxMp is 0 or 1 ? "- / -" : $"{_cc.Mp} / {_cc.MaxMp}";
        _hp = _cc.Hp;
        _maxHp = _cc.MaxHp;
        _mp = _cc.Mp;
        _maxMp = _cc.MaxMp;
        
        var hpRatio = _cc.Hp / (float)_cc.MaxHp * 100;
        _hpText.color = hpRatio switch
        {
            > 70.0f => Color.green,
            < 30.0f => Color.red,
            _ => Color.yellow
        };
    }

    private void InitCamera()
    {
        _portraitCamera = GameObject.FindGameObjectsWithTag("Camera")
            .FirstOrDefault(obj => obj.name == "PortraitCam")?.GetComponent<Camera>();
        
        if (_portraitCamera != null && _cc != null)
        {
            var unitPos = _cc.transform.position;
            var offset = _cc.transform.forward * _cc.Stat.SizeX;
            _portraitCamera.transform.position = unitPos + offset + Vector3.up * _cc.Stat.SizeY;
            _portraitCamera.transform.LookAt(unitPos);
        }
    }
    
    protected override void InitUI()
    {
        var images = new List<Images>();
        switch (_cc.ObjectType)
        {
            case GameObjectType.Tower:
                if (_gameVm.GetLevelFromUiObject(_cc.UnitId) < 3)
                {
                    images.AddRange(new[] 
                        { Images.UnitUpgradePanel, Images.UnitDeletePanel, Images.UnitSkillPanel });
                }
                else
                {
                    images.AddRange(new[] 
                        { Images.UnitDeletePanel, Images.UnitSkillPanel });
                }
                break;
            case GameObjectType.Fence:
                images.AddRange(new[] { Images.UnitRepairPanel });
                break;
            case GameObjectType.Sheep:
                images.AddRange(new[] { Images.UnitSkillPanel });
                break;
            case GameObjectType.Monster:
                images.AddRange(new[] { Images.UnitSkillPanel });
                break;
            case GameObjectType.MonsterStatue:
                if (_gameVm.GetLevelFromUiObject(_cc.UnitId) < 3)
                {
                    images.AddRange(new[]
                    {
                        Images.UnitUpgradePanel,
                        Images.UnitDeletePanel,
                        Images.UnitRepairPanel,
                        Images.UnitSkillPanel
                    });
                }
                else
                {
                    images.AddRange(new[]
                    {
                        Images.UnitDeletePanel,
                        Images.UnitRepairPanel,
                        Images.UnitSkillPanel
                    });
                }
                break;
        }
        
        BindControlButtons(images);
        SetObjectSize(GetImage((int)Images.UnitUpgradeButtonPanel).gameObject, 0.7f);
        SetObjectSize(GetImage((int)Images.UnitDeleteButtonPanel).gameObject, 0.7f);
        SetObjectSize(GetImage((int)Images.UnitRepairButtonPanel).gameObject, 0.7f);
        SetObjectSize(GetImage((int)Images.UnitSkillButtonPanel).gameObject, 0.7f);
        SetObjectSize(GetImage((int)Images.UnitUpgradeGoldImage).gameObject, 0.15f);
        SetObjectSize(GetImage((int)Images.UnitDeleteGoldImage).gameObject, 0.15f);
        SetObjectSize(GetImage((int)Images.UnitDeleteGoldPlusImage).gameObject, 0.1f);
        SetObjectSize(GetImage((int)Images.UnitRepairGoldImage).gameObject, 0.15f);
    }

    private void BindControlButtons(List<Images> images)
    {
        var allImages = new List<Images>
        {
            Images.UnitUpgradePanel,
            Images.UnitDeletePanel,
            Images.UnitRepairPanel,
            Images.UnitSkillPanel
        };
        
        var imagesToBeHidden = allImages.Except(images).ToList();
        foreach (var hiddenImage in imagesToBeHidden)
        {
            GetImage((int)hiddenImage).gameObject.SetActive(false);
        }
        
        // Set the position of the images by the number of images (The number of images is different for each object type)
        Image image;
        float increment;
        switch (images.Count)
        {
            case 1:
                image = GetImage((int)images[0]);
                image.GetComponent<RectTransform>().anchorMin = new Vector2(0.3f, 0f);
                image.GetComponent<RectTransform>().anchorMax = new Vector2(0.7f, 1f);
                break;
            case 2:
                increment = 0.4f;
                for (var i = 0; i < images.Count; i++)
                {
                    image = GetImage((int)images[i]);
                    image.GetComponent<RectTransform>().anchorMin = new Vector2(0.1f + increment * i, 0f);
                    image.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f + increment * i, 1f);
                }
                break;
            case 3:
                increment = 0.33f;
                for (var i = 0; i < images.Count; i++)
                {
                    image = GetImage((int)images[i]);
                    image.GetComponent<RectTransform>().anchorMin = new Vector2(0f + increment * i, 0f);
                    image.GetComponent<RectTransform>().anchorMax = new Vector2(0.33f + increment * i, 1f);
                }
                break;
            case 4:
                increment = 0.25f;
                for (var i = 0; i < images.Count; i++)
                {
                    image = GetImage((int)images[i]);
                    image.GetComponent<RectTransform>().anchorMin = new Vector2(0f + increment * i, 0f);
                    image.GetComponent<RectTransform>().anchorMax = new Vector2(0.25f + increment * i, 1f);
                }
                break;
        }
        
        _gameVm.UpdateUnitUpgradeCostRequired(new []{ _cc.Id });
        _gameVm.UpdateUnitDeleteCostRequired(new []{ _cc.Id });
        _gameVm.UpdateUnitRepairCostRequired(new []{ _cc.Id });
    }
    
    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        _hpText = GetText((int)Texts.HpText);
        _mpText = GetText((int)Texts.MpText);
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.UnitUpgradeButton).gameObject.BindEvent(OnUpgradeClicked);
        GetButton((int)Buttons.UnitDeleteButton).gameObject.BindEvent(OnDeleteClicked);
        GetButton((int)Buttons.UnitRepairButton).gameObject.BindEvent(OnRepairClicked);
        GetButton((int)Buttons.UnitSkillButton).gameObject.BindEvent(OnSkillClicked);
    }
    
    public void UpdateUpgradeCostText(int cost)
    {
        GetText((int)Texts.UnitUpgradeGoldText).text = cost.ToString();
    }
    
    public void UpdateDeleteCostText(int cost)
    {
        GetText((int)Texts.UnitDeleteGoldText).text = cost.ToString();
    }
    
    public void UpdateRepairCostText(int cost)
    {
        GetText((int)Texts.UnitRepairGoldText).text = cost.ToString();
    }
    
    private void OnUpgradeClicked(PointerEventData data)
    {
        if (_cc == null) return;
        _gameVm.OnUnitUpgradeClicked(new[] { _cc.Id });
    }
    
    private void OnDeleteClicked(PointerEventData data)
    {
        if (_cc == null) return;
        _gameVm.OnUnitDeleteClicked(new[] { _cc.Id });
    }
    
    private void OnRepairClicked(PointerEventData data)
    {
        if (_cc == null) return;
        _gameVm.OnUnitRepairClicked(new[] { _cc.Id });
    }

    private void OnSkillClicked(PointerEventData data)
    {
        _gameVm.OnUnitSkillClicked();
    }

    private void OnDestroy()
    {
        _gameVm.UnitControlWindow = null;
    }
}
