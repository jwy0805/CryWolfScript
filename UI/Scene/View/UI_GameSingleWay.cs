using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_GameSingleWay : UI_Game
{
    #region Enums

    private enum Buttons
    {
        CapacityButton,
        ResourceButton,
        SubResourceButton,
        MenuButton,
    }
    
    private enum Images
    {
        UnitPanel0,
        UnitPanel1,
        UnitPanel2,
        UnitPanel3,
        UnitPanel4,
        UnitPanel5,
    }
    
    private enum Texts
    {
        ResourceText,
        SubResourceText,
    }

    #endregion
    
    private GameViewModel _gameVm;

    private readonly List<GameObject> _selectedObjects = new();
    private readonly Dictionary<string, GameObject> _dictPortrait = new();
    
    public MyPlayerController Player { get; set; }
    public Camp Camp { get; set; }
    
    [Inject]
    public void Construct(GameViewModel gameViewModel)
    {
        _gameVm = gameViewModel;
    }

    protected override void Init()
    {
        base.Init();
        Camp = Util.Camp;
        
        BindObjects();
        InitButtonEvents();
        InitUI();
    }
    
    private void Awake()
    {
        _gameVm.OnPortraitClickedEvent -= ShowPortraitSelectEffect;
        _gameVm.OnPortraitClickedEvent += ShowPortraitSelectEffect;
        _gameVm.TurnOnSelectRingCoroutineEvent -= SelectRingCoroutine;
        _gameVm.TurnOnSelectRingCoroutineEvent += SelectRingCoroutine;
        _gameVm.TurnOffOneSelectRingEvent -= TurnOffOneSelectRing;
        _gameVm.TurnOffOneSelectRingEvent += TurnOffOneSelectRing;
        _gameVm.TurnOffSelectRingEvent -= TurnOffSelectRing;
        _gameVm.TurnOffSelectRingEvent += TurnOffSelectRing;
        _gameVm.SelectedObjectIds.CollectionChanged -= OnSlotIdChanged;
        _gameVm.SelectedObjectIds.CollectionChanged += OnSlotIdChanged;
    }
    
    // Set the selected units in the main lobby to the log bar
    private void SetLog()
    {   
        for (var i = 0; i < Util.Deck.UnitsOnDeck.Length; i++)
        {
            var prefab = Managers.Resource.InstantiateFromContainer(
                "UI/InGame/SkillPanel/Portrait", _dictPortrait[$"UnitPanel{i}"].transform);
            var level = Util.Deck.UnitsOnDeck[i].Level;
            var initPortraitId = (int)Util.Deck.UnitsOnDeck[i].Id - (level - 1);
            var portrait = prefab.GetComponent<UI_Portrait>();
            portrait.UnitId = (UnitId)initPortraitId;
            
            prefab.GetComponent<Image>().sprite = 
                Managers.Resource.Load<Sprite>($"Sprites/Portrait/{portrait.UnitId}");
            prefab.BindEvent(OnPortraitClicked);
        }
    }
    
    // Show portrait select effect
    private void ShowPortraitSelectEffect(IPortrait portrait, bool on)
    {
        if (portrait is not MonoBehaviour mono) return;
        var go = mono.gameObject;
        var glowObject = go.transform.parent.transform.GetChild(1).gameObject;
        glowObject.TryGetComponent(out GlowCycle glow);
        if (go.TryGetComponent(out ButtonBounce bounce) == false) return;
        glow.Selected = on;
        bounce.Selected = on;
    }

    // Update skill panel - status of skill buttons
    public void UpgradeSkill(Skill skill)
    {
        _gameVm.OnSkillUpgraded(skill);
    }

    public void UpdateUpgradeCost(int cost)
    {
        _gameVm.UpdateUpgradeCostResponse(cost);
    }
    
    public void UpgradePortrait(string newUnitName)
    {
        // Update the portrait image - unit name that is upgraded
        var portrait = _gameVm.CurrentSelectedPortrait;
        if (portrait is not MonoBehaviour mono) return;
        var go = mono.gameObject;
        go.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>($"Sprites/Portrait/{newUnitName}");
        
        // Update Skill panel to match the new unit
        _gameVm.UpdateSkillPanel(portrait);
    }

    private void TurnOnSelectRing(int id)
    {   
        var go = Managers.Object.FindById(id);
        if (go.transform.Find("SelectRing") != null)
        {
            return;
        }
        
        var selectRing = Managers.Resource.Instantiate("WorldObjects/SelectRing", go.transform);
        if (go.TryGetComponent(out Collider anyCollider) == false) return;
        var size = anyCollider.bounds.size.x;
        selectRing.transform.localPosition = new Vector3(0, 0.01f, 0);
        selectRing.transform.localScale = new Vector3(size, size, size) * 0.75f;
        _gameVm.SelectedObjectIds.Add(id);
    }
    
    private void SelectRingCoroutine(int id)
    {
        StartCoroutine(TurnOnSelectRingCoroutine(id));
    }
    
    private IEnumerator TurnOnSelectRingCoroutine(int id)
    {
        yield return null;
        TurnOnSelectRing(id);
    }

    private void TurnOffOneSelectRing(int id)
    {
        var go = Managers.Object.FindById(id);
        var selectRing = go.transform.Find("SelectRing");
        if (selectRing != null)
        {
            Managers.Resource.Destroy(selectRing.gameObject);
        }
    }
    
    private void TurnOffSelectRing()
    {
        foreach (var id in _gameVm.SelectedObjectIds)
        {
            TurnOffOneSelectRing(id);
        }
    }

    private void OnSlotIdChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
        if (_gameVm.CapacityWindow == null) return;

        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                _gameVm.CapacityWindow.InitSlot(args.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Remove:
                for (var i = 0; i < _gameVm.SelectedObjectIds.Count; i++)
                {
                    _gameVm.CapacityWindow.DeleteAllSlots();
                    _gameVm.CapacityWindow.InitSlot(i);
                }
                break;
            case NotifyCollectionChangedAction.Reset: 
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
            default:
                break;
        }
    }
    
    #region Button Events

    private void OnPortraitClicked(PointerEventData data)
    {
        if (data.pointerPress.TryGetComponent(out UI_Portrait portrait) == false) { return; }
        _gameVm.OnPortraitClicked(portrait);
    }
    
    private void OnSubResourceButtonClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
        Managers.UI.ShowPopupUiInGame<SubResourceWindow>();
    }

    #endregion
    
    protected override void BindObjects()
    {
        BindData<Image>(typeof(Images), _dictPortrait);
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        SetLog();
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.SubResourceButton).gameObject.BindEvent(OnSubResourceButtonClicked);
    }

    protected override void InitUI()
    {
        GetText((int)Texts.ResourceText).text = "0";
    }
}
