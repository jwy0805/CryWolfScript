using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
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
    }

    #endregion
    
    private GameViewModel _gameVm;
    private TutorialViewModel _tutorialVm;
    
    private Camera _tutorialCamera;
    private GameObject _log;
    private bool _isTutorial;
    private readonly List<GameObject> _selectedObjects = new();
    private readonly Dictionary<string, GameObject> _dictPortrait = new();
    
    public MyPlayerController Player { get; set; }
    public Faction Faction { get; set; }
    
    [Inject]
    public void Construct(GameViewModel gameViewModel, TutorialViewModel tutorialViewModel)
    {
        _gameVm = gameViewModel;
        _tutorialVm = tutorialViewModel;
    }

    private void Awake()
    {
        _gameVm.SetPortraitFromFieldUnitEvent += SetPortraitFromFieldUnit;
        _gameVm.OnPortraitClickedEvent += ShowPortraitSelectEffect;
        _gameVm.TurnOnSelectRingCoroutineEvent += SelectRingAsync;
        _gameVm.TurnOffOneSelectRingEvent += TurnOffOneSelectRing;
        _gameVm.TurnOffSelectRingEvent += TurnOffSelectRing;
        _gameVm.SelectedObjectIds.CollectionChanged += OnSlotIdChanged;
        _tutorialVm.OnInitTutorialCamera1 += InitTutorialBattleCamera;
        _tutorialVm.OnGetTankerIndex += GetTankerIndex;
        _tutorialVm.OnGetRangerIndex += GetRangerIndex;
    }
    
    protected override void Init()
    {
        base.Init();
        Faction = Util.Faction;
        
        if (Managers.Game.IsTutorial)
        {
            _isTutorial = true;
            Managers.Game.IsTutorial = false;
        }
        
        BindObjects();
        InitButtonEvents();
        InitUI();
    }
    
    protected override async void BindObjects()
    {
        try
        {
            _dictPortrait.Clear();
            _selectedObjects.Clear();
        
            BindData<Image>(typeof(Images), _dictPortrait);
            Bind<Button>(typeof(Buttons));
            Bind<TextMeshProUGUI>(typeof(Texts));
        
            _log = await SetLog();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.MenuButton).gameObject.BindEvent(OnMenuClicked);
        GetButton((int)Buttons.CapacityButton).gameObject.BindEvent(OnCapacityClicked);
        GetButton((int)Buttons.ResourceButton).gameObject.BindEvent(OnResourceClicked);
    }
    
    protected override void InitUI()
    {
        GetText((int)Texts.ResourceText).text = "0";

        if (_isTutorial)
        {
            _ = SetTutorialUI();
        }
    }
    
    // Set the selected units in the main lobby to the log bar
    private async Task<GameObject> SetLog()
    {   
        var deck = Util.Faction == Faction.Sheep ? User.Instance.DeckSheep : User.Instance.DeckWolf;
        for (var i = 0; i < deck.UnitsOnDeck.Length ; i++)
        {
            var parent = _dictPortrait[$"UnitPanel{i}"].transform;
            var prefab = await Managers.Resource.InstantiateAsyncFromContainer(
                "UI/InGame/SkillPanel/Portrait", parent);
            var costText = Util.FindChild(parent.gameObject, "UnitCostText", true);
            var level = deck.UnitsOnDeck[i].Level;
            var initPortraitId = deck.UnitsOnDeck[i].Id - (level - 1);
            var portrait = prefab.GetComponent<UI_Portrait>();

            portrait.UnitId = (UnitId)initPortraitId;
            costText.GetComponent<TextMeshProUGUI>().text =
                Managers.Data.UnitDict[initPortraitId].Stat.RequiredResources.ToString();
            prefab.GetComponent<Image>().sprite = 
                await Managers.Resource.LoadAsync<Sprite>($"Sprites/Portrait/{portrait.UnitId}");
            prefab.BindEvent(OnPortraitClicked);
        }
        
        return _dictPortrait["UnitPanel0"].transform.parent.gameObject;
    }

    private IPortrait SetPortraitFromFieldUnit(UnitId unitId)
    {
        var portraits = _log.GetComponentsInChildren<UI_Portrait>();
        var unitInfo = Managers.Data.UnitInfoDict[(int)unitId];
        
        foreach (var p in portraits)
        {
            var portraitUnitInfo = Managers.Data.UnitInfoDict[(int)p.UnitId];
            if (unitInfo.Species == portraitUnitInfo.Species)
            {
                IPortrait portrait = p;
                return portrait;
            }
        }
        
        return null;
    }
    
    // Show portrait select effect
    private void ShowPortraitSelectEffect(IPortrait portrait, bool on)
    {
        if (portrait is not MonoBehaviour mono) return;
        var go = mono.gameObject;
        var parent = go.transform.parent;
        var glows = parent.GetComponentsInChildren<GlowCycle>();
        
        foreach (var glowCycle in glows)
        {
            glowCycle.Selected = false;
        }
        
        var glowObject = go.transform.parent.GetChild(1).gameObject;
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
    
    public async Task UpgradePortrait(string newUnitName)
    {
        // Update the portrait image - unit name that is upgraded
        var portrait = _gameVm.CurrentSelectedPortrait;
        if (portrait is not MonoBehaviour mono) return;
        var go = mono.gameObject;
        var portraitKey = $"Sprites/Portrait/{newUnitName}";
        go.GetComponent<Image>().sprite = await Managers.Resource.LoadAsync<Sprite>(portraitKey);
        
        // Update Skill panel to match the new unit
        _gameVm.UpdateSkillPanel(portrait);
        
        // Update the cost of the unit
        var costText = Util.FindChild(go.transform.parent.gameObject, "UnitCostText", true);
        Managers.Data.UnitDict.TryGetValue((int)portrait.UnitId, out var unit);
        
        if (unit == null)
        {
            Debug.LogWarning($"Unit with ID {(int)portrait.UnitId} not found in UnitDict.");
            return;
        }
        
        costText.GetComponent<TextMeshProUGUI>().text = unit.Stat.RequiredResources.ToString();
        
        // Tutorial
        if ((_tutorialVm.Step == 9 && Util.Faction == Faction.Wolf) ||
            (_tutorialVm.Step == 11 && Util.Faction == Faction.Sheep))
        {
            StepTutorial();
            _tutorialVm.SendHoldPacket(true);
        }
    }

    private async Task TurnOnSelectRing(int id)
    {   
        var go = Managers.Object.FindById(id);
        if (go == null) return; 
        if (go.transform.Find("SelectRing") != null)
        {
            return;
        }
        
        var selectRing = await Managers.Resource.Instantiate("WorldObjects/SelectRing", go.transform);
        if (go.TryGetComponent(out Collider anyCollider) == false) return;
        var size = anyCollider.bounds.size.x;
        selectRing.transform.localPosition = new Vector3(0, 0.01f, 0);
        selectRing.transform.localScale = new Vector3(size, size, size) * 0.75f;
        _gameVm.SelectedObjectIds.Add(id);
    }
    
    private async Task SelectRingAsync(int id)
    {
        await TurnOnSelectRing(id);
    }

    private void TurnOffOneSelectRing(int id)
    {
        var go = Managers.Object.FindById(id);
        if (go == null) return;
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
                _gameVm.CapacityWindow.InitSlotAsync(args.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Remove:
                for (var i = 0; i < _gameVm.SelectedObjectIds.Count; i++)
                {
                    _gameVm.CapacityWindow.DeleteAllSlots();
                    _gameVm.CapacityWindow.InitSlotAsync(i);
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

    private async Task OnPortraitClicked(PointerEventData data)
    {
        if (data.pointerPress.TryGetComponent(out UI_Portrait portrait) == false) { return; }
        await _gameVm.OnPortraitClicked(portrait);
    }
    
    private async Task OnResourceClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
        await Managers.UI.ShowPopupUiInGame<BaseSkillWindow>();
    }
    
    private async Task OnCapacityClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
        await Managers.UI.ShowPopupUiInGame<BaseSkillWindow>();
    }

    private async Task OnMenuClicked(PointerEventData data)
    {
        await Managers.UI.ShowPopupUI<UI_GameMenuPopup>();
    }
    
    #endregion

    #region tutorial
    private void StepTutorial()
    {
        _tutorialVm.StepTutorial();
    }
    
    private async Task SetTutorialUI()
    {
        if (Faction == Faction.Sheep)
        {
            await Managers.UI.ShowPopupUI<UI_TutorialBattleSheepPopup>();
        }
        else
        {
            await Managers.UI.ShowPopupUI<UI_TutorialBattleWolfPopup>();
        }
        
        _tutorialVm.ClearDictionary();
    }
    
    private void InitTutorialBattleCamera(Vector3 npcPos, Vector3 cameraPos)
    {
        var cameraObjects = GameObject.FindGameObjectsWithTag("Camera");
        var cameraObject = cameraObjects.FirstOrDefault(go => go.name == "TutorialCamera");
        if (cameraObject == null) return;
        _tutorialCamera = cameraObject.GetComponent<Camera>();
        _tutorialCamera.transform.position = cameraPos;
        _tutorialCamera.transform.LookAt(npcPos);
    }
    
    private int GetTankerIndex()
    {
        var portraits = _log.GetComponentsInChildren<UI_Portrait>();
        var index = int.MinValue;
        foreach (var portrait in portraits)
        {
            if (Managers.Data.UnitInfoDict.TryGetValue((int)portrait.UnitId, out var unitInfo))
            {
                if (unitInfo.Role is Role.Tanker or Role.Warrior)
                {
                    var parent = portrait.transform.parent;
                    for (var i = 0; i < 6; i++)
                    {
                        if (parent.name == $"UnitPanel{i}")
                        {
                            index = i;
                            break;
                        }
                    }
                }
            }
        }
        
        return index;
    }
    
    private int GetRangerIndex()
    {
        var portraits = _log.GetComponentsInChildren<UI_Portrait>();
        var index = int.MinValue;
        foreach (var portrait in portraits)
        {
            if (Managers.Data.UnitInfoDict.TryGetValue((int)portrait.UnitId, out var unitInfo))
            {
                if (unitInfo.Role is Role.Ranger or Role.Mage)
                {
                    var parent = portrait.transform.parent;
                    for (var i = 0; i < 6; i++)
                    {
                        if (parent.name == $"UnitPanel{i}")
                        {
                            index = i;
                            break;
                        }
                    }
                }
            }
        }
        
        return index;
    }
    
    #endregion
    
    private void OnDestroy()
    {
        _gameVm.OnPortraitClickedEvent -= ShowPortraitSelectEffect;
        _gameVm.TurnOnSelectRingCoroutineEvent -= SelectRingAsync;
        _gameVm.TurnOffOneSelectRingEvent -= TurnOffOneSelectRing;
        _gameVm.TurnOffSelectRingEvent -= TurnOffSelectRing;
        _gameVm.SelectedObjectIds.CollectionChanged -= OnSlotIdChanged;
        _gameVm.Dispose();
        _gameVm = null;
        
        _tutorialVm.OnInitTutorialCamera1 -= InitTutorialBattleCamera;
        _tutorialVm.OnGetTankerIndex -= GetTankerIndex;
        _tutorialVm.OnGetRangerIndex -= GetRangerIndex;
        _tutorialVm.Dispose();
        _tutorialVm = null;
    }
}
