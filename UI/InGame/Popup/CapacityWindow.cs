using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Google.Protobuf.Protocol;
using NUnit.Framework;
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
    void UpdateUpgradeCostText(int cost);
    void UpdateDeleteCostText(int cost);
    void UpdateRepairCostText(int cost);
    int GetButtonIndex(GameObject button);
    GameObject GetDeleteImage();
    GameObjectType ObjectType { get; set; }
}

// Windows that show the plural units in the game
public class CapacityWindow : UI_Popup, ICapacityWindow
{
    #region Enums
    
    private enum Buttons
    {
        UnitUpgradeButton,
        UnitDeleteButton,
        UnitRepairButton,
        
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
        UnitUpgradePanel,
        UnitDeletePanel,
        UnitRepairPanel,
        
        UnitUpgradeButtonPanel,
        UnitDeleteButtonPanel,
        UnitRepairButtonPanel,
        
        UnitUpgradeGoldImage,
        UnitDeleteGoldImage,
        UnitDeleteGoldPlusImage,
        UnitRepairGoldImage,
        
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
        UnitUpgradeText,
        
        UnitUpgradeGoldText,
        UnitDeleteGoldText,
        UnitRepairGoldText,
    }
    
    #endregion
    
    private GameViewModel _gameVm;
    private TutorialViewModel _tutorialVm;
    
    private readonly Dictionary<string, GameObject> _buttonDict = new();
    private readonly Dictionary<string, GameObject> _imageDict = new();
    private readonly GameObject[] _buttonArray = new GameObject[12];
    private GameObject _deleteImage;

    public GameObjectType ObjectType { get; set; }
    
    [Inject]
    public void Construct(GameViewModel gameViewModel, TutorialViewModel tutorialViewModel)
    {
        _gameVm = gameViewModel;
        _tutorialVm = tutorialViewModel;
        
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
        InitEvents();
        
        // Tutorial
        if ((_tutorialVm.Step == 16 && Util.Faction == Faction.Wolf) ||
            (_tutorialVm.Step == 21 && Util.Faction == Faction.Sheep))
        {
            _tutorialVm.ShowTutorialPopup();
        }
    }

    protected override void BindObjects()
    {
        BindData<Button>(typeof(Buttons), _buttonDict);
        BindData<Image>(typeof(Images), _imageDict);
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        _deleteImage = _imageDict["DeleteImage"];
        SetObjectSize(_imageDict["DeleteImage"], 0.35f);
        _imageDict["DeleteImage"].SetActive(false);
        for (var i = 0; i < 12; i++)
        {
            _buttonArray[i] = _buttonDict[$"NorthUnitButton{i}"];
        }
    }
    
    protected override void InitUI()
    {
        var images = new List<Images>();
        switch (ObjectType)
        {
            case GameObjectType.Tower:
                images.AddRange(new[] { Images.UnitUpgradePanel, Images.UnitDeletePanel });
                break;
            case GameObjectType.Fence:
                images.AddRange(new[] { Images.UnitRepairPanel });
                break;
            case GameObjectType.MonsterStatue:
                images.AddRange(new[] 
                    { Images.UnitUpgradePanel, Images.UnitDeletePanel, Images.UnitRepairPanel });
                break;
        }
        
        BindControlButtons(images);
        SetObjectSize(_imageDict["UnitUpgradeButtonPanel"], 0.6f);
        SetObjectSize(_imageDict["UnitDeleteButtonPanel"], 0.6f);
        SetObjectSize(_imageDict["UnitRepairButtonPanel"], 0.6f);
        SetObjectSize(_imageDict["UnitUpgradeGoldImage"], 0.15f);
        SetObjectSize(_imageDict["UnitDeleteGoldImage"], 0.15f);
        SetObjectSize(_imageDict["UnitDeleteGoldPlusImage"], 0.1f);
        SetObjectSize(_imageDict["UnitRepairGoldImage"], 0.15f);
    }
    
    private void BindControlButtons(List<Images> images)
    {
        var allImages = new List<Images>
        {
            Images.UnitUpgradePanel,
            Images.UnitDeletePanel,
            Images.UnitRepairPanel,
        };
        
        var imagesToBeHidden = allImages.Except(images).ToList();
        foreach (var hiddenImage in imagesToBeHidden)
        {
            _imageDict[hiddenImage.ToString()].SetActive(false);
        }

        Image image;
        float increment;
        switch (images.Count)
        {
            case 1:
                image = _imageDict[images[0].ToString()].GetComponent<Image>();
                image.GetComponent<RectTransform>().anchorMin = new Vector2(0.3f, 0f);
                image.GetComponent<RectTransform>().anchorMax = new Vector2(0.7f, 1f);
                break;
            case 2:
                increment = 0.4f;
                for (var i = 0; i < images.Count; i++)
                {
                    image = _imageDict[images[i].ToString()].GetComponent<Image>();
                    image.GetComponent<RectTransform>().anchorMin = new Vector2(0.1f + increment * i, 0f);
                    image.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f + increment * i, 1f);
                }

                break;
            case 3:
                increment = 0.33f;
                for (var i = 0; i < images.Count; i++)
                {
                    image = _imageDict[images[i].ToString()].GetComponent<Image>();
                    image.GetComponent<RectTransform>().anchorMin = new Vector2(0f + increment * i, 0f);
                    image.GetComponent<RectTransform>().anchorMax = new Vector2(0.33f + increment * i, 1f);
                }

                break;
        }
    }

    protected override void InitButtonEvents()
    {
        for (var i = 0; i < 12 ; i++)
        {
            _buttonDict[$"NorthUnitButton{i}"].gameObject.BindEvent(OnSlotClicked);
        }
        
        _buttonDict["UnitUpgradeButton"].BindEvent(OnUpgradeClicked);
        _buttonDict["UnitDeleteButton"].BindEvent(OnDeleteClicked);
        _buttonDict["UnitRepairButton"].BindEvent(OnRepairClicked);
    }
    
    public void InitSlot(int index)
    {
        _gameVm.UpdateUnitUpgradeCostRequired(_gameVm.SelectedObjectIds.ToArray());
        _gameVm.UpdateUnitDeleteCostRequired(_gameVm.SelectedObjectIds.ToArray());
        _gameVm.UpdateUnitRepairCostRequired(_gameVm.SelectedObjectIds.ToArray());
        
        var id = _gameVm.SelectedObjectIds[index];
        var go = Managers.Object.FindById(id);
        var slotPanel = _imageDict[$"NorthUnitPanel{index}"];
        var slotButtonImage = _buttonDict[$"NorthUnitButton{index}"];

        if (go == null)
        {
            SetObjectSize(slotPanel, 0);
            return;    
        }
        
        var unitId = go.GetComponent<CreatureController>().UnitId;
        var image = slotButtonImage.GetComponent<Image>();
        var path = ObjectType switch
        {
            GameObjectType.Tower => $"Sprites/Portrait/{unitId.ToString()}",
            GameObjectType.Fence => $"Sprites/Portrait/{unitId.ToString()}",
            GameObjectType.MonsterStatue => $"Sprites/Portrait/{unitId.ToString()}Statue",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        image.sprite = Managers.Resource.Load<Sprite>(path);
        SetObjectSize(slotPanel, 0.25f);   
    }
    
    private void InitEvents()
    {
        _gameVm.SetDeleteImageOnWindowEvent -= SetDeleteImage;
        _gameVm.SetDeleteImageOnWindowEvent += SetDeleteImage;
        _gameVm.HighlightDeleteImageOnWindowEvent -= HighlightDeleteImage;
        _gameVm.HighlightDeleteImageOnWindowEvent += HighlightDeleteImage;
        _gameVm.RestoreDeleteImageOnWindowEvent -= RestoreDeleteImage;
        _gameVm.RestoreDeleteImageOnWindowEvent += RestoreDeleteImage;
        _gameVm.DisappearDeleteImageOnWindowEvent -= DisappearDeleteImage;
        _gameVm.DisappearDeleteImageOnWindowEvent += DisappearDeleteImage;
    }

    public void DeleteAllSlots()
    {
        for (var i = 0; i < 12 ; i ++)
        {
            SetObjectSize(_imageDict[$"NorthUnitPanel{i}"], 0);
        }
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

    public GameObject GetDeleteImage()
    {
        return _deleteImage;
    }
    
    public int GetButtonIndex(GameObject button)
    {
        return Array.IndexOf(_buttonArray, button);
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
        _gameVm.OnUnitUpgradeClicked(_gameVm.SelectedObjectIds);
    }
    
    private void OnDeleteClicked(PointerEventData data)
    {
        _gameVm.OnUnitDeleteClicked(_gameVm.SelectedObjectIds);
    }
    
    private void OnRepairClicked(PointerEventData data)
    {
        _gameVm.OnUnitRepairClicked(_gameVm.SelectedObjectIds);
    }
    
    private void OnSlotClicked(PointerEventData data)
    {
        var index = GetButtonIndex(data.pointerPress.gameObject);
        _gameVm.OnSlotClicked(index);
    }
    
    private void OnDestroy()
    {
        if (_gameVm != null && (CapacityWindow)_gameVm.CapacityWindow == this)
        {
            _gameVm.SetDeleteImageOnWindowEvent -= SetDeleteImage;
            _gameVm.HighlightDeleteImageOnWindowEvent -= HighlightDeleteImage;
            _gameVm.RestoreDeleteImageOnWindowEvent -= RestoreDeleteImage;
            _gameVm.DisappearDeleteImageOnWindowEvent -= DisappearDeleteImage;
            _gameVm.CapacityWindow = null;
        }
    }
}
