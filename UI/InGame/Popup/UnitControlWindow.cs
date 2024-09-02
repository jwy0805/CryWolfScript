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
    
}

public class UnitControlWindow : UI_Popup, IUnitControlWindow
{
    #region Enums

    private enum Buttons
    {
        UnitUpgradeButton,
        UnitDeleteButton,
        UnitSkillButton,
    }

    private enum Images
    {
        UnitUpgradeButtonPanel,
        UnitDeleteButtonPanel,
        UnitUpgradeGoldImage,
        UnitDeleteGoldImage,
        UnitDeleteGoldPlusImage,
        UnitSkillButtonPanel,
    }
    
    private enum Texts
    {
        UnitUpgradeGoldText,
        UnitDeleteGoldText,
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

    public GameObject SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            _selectedUnit = value;
            _cc = _selectedUnit.GetComponent<CreatureController>();
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
        
        BindObjects();
        InitUI();
        InitButtonEvents();
        InitCamera();
    }

    protected void FixedUpdate()
    {
        if (_cc == null) return;
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
        var portraitCamera = GameObject.FindGameObjectsWithTag("Camera")
            .FirstOrDefault(obj => obj.name == "PortraitCam")?.GetComponent<Camera>();
        
        if (portraitCamera != null && _cc != null)
        {
            var unitPos = _cc.transform.position;
            var offset = _cc.transform.forward * _cc.Stat.SizeX;
            portraitCamera.transform.position = unitPos + offset + Vector3.up * _cc.Stat.SizeY;
            portraitCamera.transform.LookAt(unitPos);
        }
    }
    
    protected override void InitUI()
    {
        SetObjectSize(GetImage((int)Images.UnitUpgradeButtonPanel).gameObject, 0.75f);
        SetObjectSize(GetImage((int)Images.UnitDeleteButtonPanel).gameObject, 0.75f);
        SetObjectSize(GetImage((int)Images.UnitSkillButtonPanel).gameObject, 0.75f);
        SetObjectSize(GetImage((int)Images.UnitUpgradeGoldImage).gameObject, 0.15f);
        SetObjectSize(GetImage((int)Images.UnitDeleteGoldImage).gameObject, 0.15f);
        SetObjectSize(GetImage((int)Images.UnitDeleteGoldPlusImage).gameObject, 0.1f);
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
        GetButton((int)Buttons.UnitSkillButton).gameObject.BindEvent(OnSkillClicked);
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

    private void OnSkillClicked(PointerEventData data)
    {
        _gameVm.OnUnitSkillClicked();
    }
}
