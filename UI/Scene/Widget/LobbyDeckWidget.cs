using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;

public class LobbyDeckWidget
{
    private readonly IUserService _userService;
    private readonly ICardFactory _cardFactory;
    private readonly DeckViewModel _deckVm;
    
    private Dictionary<string, GameObject> _deckButtonDict;
    private Dictionary<string, GameObject> _lobbyDeckButtonDict;

    private Transform _deckParent;        // Images.Deck
    private Transform _lobbyDeckParent;   // Images.LobbyDeck
    private Transform _assetParent;       // Images.BattleSettingPanel
    
    private readonly Action<PointerEventData> _onCardClicked;
    private readonly Action<PointerEventData> _onDeckTabClicked;
    
    private bool _viewsBound = false;
    
    public LobbyDeckWidget(
        IUserService userService,
        ICardFactory cardFactory,
        DeckViewModel deckVm,
        Action<PointerEventData> onCardClicked,
        Action<PointerEventData> onDeckTabClicked)
    {
        _userService = userService;
        _cardFactory = cardFactory;
        _deckVm = deckVm;
        _onCardClicked = onCardClicked;
        _onDeckTabClicked = onDeckTabClicked;
         
        BindEvents();
    }

    private void BindEvents()
    {
        _deckVm.OnDeckInitialized += SetDeckUI;
        _deckVm.OnDeckSwitched += HandleSetDeckButtonUI;
        _deckVm.OnDeckSwitched += SetDeckUI;
    }
    
    public void BindViews(
        Dictionary<string, GameObject> deckButtonDict,
        Dictionary<string, GameObject> lobbyDeckButtonDict,
        Transform deckParent,
        Transform lobbyDeckParent,
        Transform assetParent)
    {
        _deckButtonDict = deckButtonDict;
        _lobbyDeckButtonDict = lobbyDeckButtonDict;
        _deckParent = deckParent;
        _lobbyDeckParent = lobbyDeckParent;
        _assetParent = assetParent;
        _viewsBound = true;
    }

    public async Task InitDeck()
    {
        await _deckVm.Initialize();
    }
    
    public void OnInitDeckButton(Faction faction)
    {
        SetDeckButtonUI(faction);
    }
    
    private Task HandleSetDeckButtonUI(Faction faction)
    {
        SetDeckButtonUI(faction);
        return Task.CompletedTask;
    }

    private void SetDeckButtonUI(Faction faction)
    {
        if (_deckButtonDict == null || _lobbyDeckButtonDict == null) return;

        var deckButtons = _deckButtonDict.Values.ToList();
        var lobbyDeckButtons = _lobbyDeckButtonDict.Values.ToList();
        var deckNumber = faction == Faction.Sheep 
            ? _userService.User.DeckSheep.DeckNumber 
            : _userService.User.DeckWolf.DeckNumber;
        
        for (var i = 0; i < deckButtons.Count; i++)
        {
            deckButtons[i].GetComponent<DeckButtonInfo>().IsSelected = false;
            lobbyDeckButtons[i].GetComponent<DeckButtonInfo>().IsSelected = false;
        }
        
        _deckButtonDict[$"DeckButton{deckNumber}"].GetComponent<DeckButtonInfo>().IsSelected = true;
        _lobbyDeckButtonDict[$"LobbyDeckButton{deckNumber}"].GetComponent<DeckButtonInfo>().IsSelected = true;
    }

    public async Task ResetDeckUI(Faction faction)
    {
        if (!_viewsBound) return;

        Util.DestroyAllChildren(_deckParent);
        Util.DestroyAllChildren(_lobbyDeckParent);
        await SetDeckUI(faction);
    }
    
    private async Task SetDeckUI(Faction faction)
    {
        if (!_viewsBound) return;

        // Set Deck - As using layout group, no need to set position
        var deck = _deckVm.GetDeck(faction);

        Util.DestroyAllChildren(_deckParent);
        Util.DestroyAllChildren(_lobbyDeckParent);
        
        foreach (var unit in deck.UnitsOnDeck)
        {
            await _cardFactory.GetCardResources<UnitId>(unit, _deckParent, _onCardClicked);
            await _cardFactory.GetCardResources<UnitId>(unit, _lobbyDeckParent, _onDeckTabClicked);
        }

        for (var i = 1; i <= _deckButtonDict.Count; i++)
        {
            _deckButtonDict[$"DeckButton{i}"].GetComponent<DeckButtonInfo>().DeckIndex = i;
            _lobbyDeckButtonDict[$"LobbyDeckButton{i}"].GetComponent<DeckButtonInfo>().DeckIndex = i;
        }
        
        Util.DestroyAllChildren(_assetParent);
        
        // Set Asset Frame
        IAsset asset = faction == Faction.Sheep 
            ? _userService.User.BattleSetting.SheepInfo 
            : _userService.User.BattleSetting.EnchantInfo;

        if (faction == Faction.Sheep)
        {
            await _cardFactory.GetCardResources<SheepId>(asset, _assetParent, _onCardClicked);
        }
        else
        {
            await _cardFactory.GetCardResources<EnchantId>(asset, _assetParent, _onCardClicked);
        }
        
        // Set Character Frame
        var character = _userService.User.BattleSetting.CharacterInfo;
        await _cardFactory.GetCardResources<CharacterId>(character, _assetParent, _onCardClicked);
    }
    
    public async Task OnDeckButtonClicked(PointerEventData data)
    {
        var buttonNumber = data.pointerPress.GetComponent<DeckButtonInfo>().DeckIndex;
        await _deckVm.SelectDeck(buttonNumber, Util.Faction);
    }
    
    public void Dispose()
    {
        _deckVm.OnDeckInitialized -= SetDeckUI;
        _deckVm.OnDeckSwitched -= HandleSetDeckButtonUI;
        _deckVm.OnDeckSwitched -= SetDeckUI;
    }
}