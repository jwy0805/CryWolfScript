using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

public interface ICapacityWindow
{
    void InitSlot(int index);
    void DeleteAllSlots();
    GameObjectType ObjectType { get; set; }
}

public class CapacityWindow : UI_Popup, ICapacityWindow
{
    #region Enums
    
    private enum Buttons
    {
        UnitUpgradeButton,
        UnitDeleteButton,
        
        NorthUnitButton0,
        NorthUnitButton1,
        NorthUnitButton2,
        NorthUnitButton3,
        NorthUnitButton4,
        NorthUnitButton5,
        NorthUnitButton6,
        NorthUnitButton7,
        NorthUnitButton8,
        NorthUnitButton9,
        NorthUnitButton10,
        NorthUnitButton11,
    }

    private enum Images
    {
        UnitUpgradeButtonPanel,
        UnitDeleteButtonPanel,
        UnitUpgradeGoldImage,
        UnitDeleteGoldImage,
        UnitDeleteGoldPlusImage,
        DeleteImage,
        
        NorthUnitPanel0,
        NorthUnitPanel1,
        NorthUnitPanel2,
        NorthUnitPanel3,
        NorthUnitPanel4,
        NorthUnitPanel5,
        NorthUnitPanel6,
        NorthUnitPanel7,
        NorthUnitPanel8,
        NorthUnitPanel9,
        NorthUnitPanel10,
        NorthUnitPanel11,
    }
    
    private enum Texts
    {
        UnitUpgradeGoldText,
        UnitDeleteGoldText,
    }
    
    #endregion
    
    private GameViewModel _gameVm;
    
    private readonly Dictionary<string, GameObject> _unitSlotButton = new();
    private readonly Dictionary<string, GameObject> _unitSlotPanel = new();
    private readonly GameObject[] _buttonArray = new GameObject[12];
    private GameObject _deleteImage;

    public GameObjectType ObjectType { get; set; }
    
    [Inject]
    public void Construct(GameViewModel gameViewModel)
    {
        _gameVm = gameViewModel;
        InjectDependenciesToChildren();
    }

    private void InjectDependenciesToChildren()
    {
        var childComponents = GetComponentsInChildren<IRequiresGameViewModel>();
        foreach (var component in childComponents)
        {
            component.SetGameViewModel(_gameVm);
        }
    }

    protected override void Init()
    {
        base.Init();
        _gameVm.CapacityWindow = this;
        
        BindObjects();
        InitUI();
        InitButtonEvents();
        InitSlot(0);
        SubscribeEvents();
    }

    public void DeleteAllSlots()
    {
        for (var i = 0; i < 12 ; i ++)
        {
            SetObjectSize(_unitSlotPanel[$"NorthUnitPanel{i}"], 0);
        }
    }
    
    public void InitSlot(int index)
    {
        var id = _gameVm.SelectedObjectIds[index];
        var unitId = Managers.Object.FindById(id).GetComponent<CreatureController>().UnitId;
        var slotPanel = _unitSlotPanel[$"NorthUnitPanel{index}"];
        var slotButtonImage = _unitSlotButton[$"NorthUnitButton{index}"];
        var path = ObjectType switch
        {
            GameObjectType.Tower => $"Sprites/Portrait/{unitId.ToString()}",
            GameObjectType.MonsterStatue => $"Sprites/Portrait/{unitId.ToString()}Statue",
            GameObjectType.Fence => $"Sprites/Portrait/{unitId.ToString()}",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var image = slotButtonImage.GetComponent<Image>();
        image.sprite = Managers.Resource.Load<Sprite>(path);
        SetObjectSize(slotPanel, 0.25f);   
    }

    protected override void BindObjects()
    {
        BindData<Button>(typeof(Buttons), _unitSlotButton);
        BindData<Image>(typeof(Images), _unitSlotPanel);
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        _deleteImage = _unitSlotPanel["DeleteImage"];
        SetObjectSize(_unitSlotPanel["DeleteImage"], 0.35f);
        _unitSlotPanel["DeleteImage"].SetActive(false);
        for (var i = 0; i < 12; i++)
        {
            _buttonArray[i] = _unitSlotButton[$"NorthUnitButton{i}"];
        }
    }

    protected override void InitUI()
    {
        SetObjectSize(_unitSlotPanel["UnitUpgradeButtonPanel"], 0.6f);
        SetObjectSize(_unitSlotPanel["UnitDeleteButtonPanel"], 0.6f);
        SetObjectSize(_unitSlotPanel["UnitUpgradeGoldImage"], 0.15f);
        SetObjectSize(_unitSlotPanel["UnitDeleteGoldImage"], 0.15f);
        SetObjectSize(_unitSlotPanel["UnitDeleteGoldPlusImage"], 0.1f);
    }

    protected override void InitButtonEvents()
    {
        foreach (var slot in _unitSlotButton.Values)
        {
            slot.BindEvent(OnSlotClicked);
        }
    }

    private void SubscribeEvents()
    {
        _gameVm.SetDeleteImageOnWindowEvent -= SetDeleteImage;
        _gameVm.SetDeleteImageOnWindowEvent += SetDeleteImage;
        _gameVm.HighlightDeleteImageOnWindowEvent -= HighlightDeleteImage;
        _gameVm.HighlightDeleteImageOnWindowEvent += HighlightDeleteImage;
        _gameVm.RestoreDeleteImageOnWindowEvent -= RestoreDeleteImage;
        _gameVm.RestoreDeleteImageOnWindowEvent += RestoreDeleteImage;
        _gameVm.DisappearDeleteImageOnWindowEvent -= DisappearDeleteImage;
        _gameVm.DisappearDeleteImageOnWindowEvent += DisappearDeleteImage;
        _gameVm.GetButtonIndexEvent -= GetButtonIndex;
        _gameVm.GetButtonIndexEvent += GetButtonIndex;
        _gameVm.GetDeleteImageEvent -= GetDeleteImage;
        _gameVm.GetDeleteImageEvent += GetDeleteImage;
    }

    private void SetDeleteImage()
    {
        _deleteImage.SetActive(true);
    }

    private void HighlightDeleteImage()
    {
        SetObjectSize(_deleteImage, 0.4f);
        _deleteImage.GetComponent<Image>().color = Color.red;
    }

    private void RestoreDeleteImage()
    {
        SetObjectSize(_deleteImage, 0.3f);
        _deleteImage.GetComponent<Image>().color = Color.white;
    }
    
    private void DisappearDeleteImage()
    {
        _deleteImage.SetActive(false);
    }

    private GameObject GetDeleteImage()
    {
        return _deleteImage;
    }
    
    private void OnSlotClicked(PointerEventData data)
    {
        var index = GetButtonIndex(data.pointerPress.gameObject);
        _gameVm.OnSlotClicked(index);
    }
    
    private int GetButtonIndex(GameObject button)
    {
        return Array.IndexOf(_buttonArray, button);
    }
    
    private void OnDestroy()
    {
        if (_gameVm != null && (CapacityWindow)_gameVm.CapacityWindow == this)
        {
            _gameVm.SetDeleteImageOnWindowEvent -= SetDeleteImage;
            _gameVm.HighlightDeleteImageOnWindowEvent -= HighlightDeleteImage;
            _gameVm.RestoreDeleteImageOnWindowEvent -= RestoreDeleteImage;
            _gameVm.DisappearDeleteImageOnWindowEvent -= DisappearDeleteImage;
            _gameVm.GetButtonIndexEvent -= GetButtonIndex;
            _gameVm.GetDeleteImageEvent -= GetDeleteImage;
            _gameVm.CapacityWindow = null;
        }
    }
}
