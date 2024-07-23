using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;

public partial class UI_MainLobby
{
    private Card _selectedCard;
    private UI_CardClickPopup _cardPopup;
    private Transform _deck;
    private Transform _collection;
    private Transform _noCollection;

    private UI_CardClickPopup CardPopup
    {
        get => _cardPopup;
        set
        {
            if (_cardPopup != null) Managers.UI.ClosePopupUI(_cardPopup);
            _cardPopup = value;
        }
    }

    private async void SetMainLobbyItemUI()
    {
        _deck = GetImage((int)Images.Deck).transform;
        _collection = GetImage((int)Images.HoldingCardPanel).transform;
        _noCollection = GetImage((int)Images.NotHoldingCardPanel).transform;

        GetButton((int)Buttons.DeckButton1).GetComponent<DeckButtonInfo>().DeckIndex = 1;
        GetButton((int)Buttons.DeckButton2).GetComponent<DeckButtonInfo>().DeckIndex = 2;
        GetButton((int)Buttons.DeckButton3).GetComponent<DeckButtonInfo>().DeckIndex = 3;
        GetButton((int)Buttons.DeckButton4).GetComponent<DeckButtonInfo>().DeckIndex = 4;
        GetButton((int)Buttons.DeckButton5).GetComponent<DeckButtonInfo>().DeckIndex = 5;
        
        await SetCards();
        Debug.Log("Set Cards");
        SetDeckUI(Util.Camp);
        SetCollection(Util.Camp);
        SetItemUI();
    }
    
    private async Task SetCards()
    {
        var cardPacket = new GetOwnedCardsPacketRequired
        {
            AccessToken = Managers.User.AccessToken,
            Environment = Managers.Web.Environment
        };
        
        var deckPacket = new GetInitDeckPacketRequired 
        {
            AccessToken = Managers.User.AccessToken,
            Environment = Managers.Web.Environment
        };
        
        var cardTask = Managers.Web.SendPostRequestAsync<GetOwnedCardsPacketResponse>("Collection/GetCards", cardPacket);
        var deckTask = Managers.Web.SendPostRequestAsync<GetInitDeckPacketResponse>("Collection/GetDecks", deckPacket);
        
        await Task.WhenAll(cardTask, deckTask);

        var cardResponse = cardTask.Result;
        var deckResponse = deckTask.Result;
        if (cardResponse.GetCardsOk == false || deckResponse.GetDeckOk == false) return;
        if (cardResponse.AccessToken != null)
        {   // Test 에서 뒤늦게 토큰을 받는 경우
            Managers.Token.SaveAccessToken(cardResponse.AccessToken);
            Managers.Token.SaveRefreshToken(cardResponse.RefreshToken);
        }
        
        foreach (var unitInfo in cardResponse.OwnedCardList) Managers.User.LoadOwnedUnit(unitInfo);
        foreach (var unitInfo in cardResponse.NotOwnedCardList) Managers.User.LoadNotOwnedUnit(unitInfo);
        foreach (var deckInfo in deckResponse.DeckList) Managers.User.LoadDeck(deckInfo);
        
        Managers.User.BindDeck();
        SetDeckButtonUI(Util.Camp);
    }
    
    private void SetDeckUI(Camp camp)
    {
        var deck = camp == Camp.Sheep ? Managers.User.DeckSheep : Managers.User.DeckWolf;
        var parent = Util.FindChild(gameObject, _deck.name, true, true).transform;
        foreach (var unit in deck.UnitsOnDeck) Util.GetCardResources(unit, parent, 0, OnCardClickedOnDeck);
    }
    
    private void SetCollection(Camp camp)
    {
        var ownedUnits = camp == Camp.Sheep 
            ? Managers.User.OwnedCardListSheep.OrderBy(unitInfo => unitInfo.Class).ToList() 
            : Managers.User.OwnedCardListWolf.OrderBy(unitInfo => unitInfo.Class).ToList() ;
        var notOwnedUnits = camp == Camp.Sheep 
            ? Managers.User.NotOwnedCardListSheep.OrderBy(unitInfo => unitInfo.Class).ToList()  
            : Managers.User.NotOwnedCardListWolf.OrderBy(unitInfo => unitInfo.Class).ToList() ;
        var ownedParent = Util
            .FindChild(gameObject, _collection.name, true, true).transform; 
        var notOwnedParent = Util
            .FindChild(gameObject, _noCollection.name, true, true).transform;
        
        Util.DestroyAllChildren(ownedParent);
        Util.DestroyAllChildren(notOwnedParent);

        foreach (var unit in ownedUnits)
        {
            Util.GetCardResources(unit, ownedParent, 0, OnCardClickedOnDeck);
        }

        foreach (var unit in notOwnedUnits)
        {
            var cardFrame = Util.GetCardResources(unit, notOwnedParent);
            var cardUnit = cardFrame.transform.Find("CardUnit").gameObject;
            cardUnit.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(
                $"Sprites/Portrait/{unit.Id.ToString()}_gray");
        }
    }

    private void SetDeckButtonUI(Camp camp)
    {
        var deckNumber = camp == Camp.Sheep ? Managers.User.DeckSheep.DeckNumber : Managers.User.DeckWolf.DeckNumber;
        var deckButtons = new List<Button>
        {
            GetButton((int)Buttons.DeckButton1),
            GetButton((int)Buttons.DeckButton2),
            GetButton((int)Buttons.DeckButton3),
            GetButton((int)Buttons.DeckButton4),
            GetButton((int)Buttons.DeckButton5)
        };
        
        foreach (var deckButton in deckButtons)
        {
            deckButton.GetComponent<DeckButtonInfo>().IsSelected = false;
        }
        
        deckButtons[deckNumber - 1].GetComponent<DeckButtonInfo>().IsSelected = true;
    }
    
    private void SetItemUI()
    {
        Util.SetAlpha(GetButton((int)Buttons.CollectionTabButton).GetComponent<Image>(), 0.4f);
        GetImage((int)Images.CollectionScrollView).gameObject.SetActive(false);
    }

    private void SetCardPopupUI(Card card)
    {
        _selectedCard = card;
        CardPopup = Managers.UI.ShowPopupUI<UI_CardClickPopup>();
        CardPopup.SelectedCard = _selectedCard;
        CardPopup.CardPosition = _selectedCard.transform.position;
        CardPopup.FromDeck = card.gameObject.transform.parent == GetImage((int)Images.Deck).transform;
        
        // TODO: Set Size By Various Resolution
    }
    
    public void ResetDeckUI(Camp camp)
    {
        var parent = GetImage((int)Images.Deck).transform;
        foreach (Transform child in parent) Destroy(child.gameObject);
        SetDeckUI(camp);
    }

    public void UpdateDeck(Card oldCard, Card newCard)
    {
        var deck = Util.Camp == Camp.Sheep ? Managers.User.DeckSheep : Managers.User.DeckWolf;
        var index = Array.FindIndex(deck.UnitsOnDeck, unitInfo => unitInfo.Id == oldCard.UnitInfo.Id);
        if (index == -1) return;
        
        var unitIdToBeDeleted = deck.UnitsOnDeck[index].Id;
        var unitIdToBeUpdated = newCard.UnitInfo.Id;
        deck.UnitsOnDeck[index] = newCard.UnitInfo;
        if (Util.Camp == Camp.Sheep) Managers.User.DeckSheep = deck;
        else Managers.User.DeckWolf = deck;

        var updateDeckPacket = new UpdateDeckPacketRequired
        {
            AccessToken = Managers.User.AccessToken,
            DeckId = deck.DeckId,
            UnitIdToBeDeleted = unitIdToBeDeleted,
            UnitIdToBeUpdated = unitIdToBeUpdated
        };
        
        Managers.Web.SendPutRequest<UpdateDeckPacketResponse>(
            "Collection/UpdateDeck", updateDeckPacket, response => { });
    }
    
    // Event Methods
    private void OnDeckTabClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
        Util.SetAlpha(GetButton((int)Buttons.CollectionTabButton).GetComponent<Image>(), 0.4f);
        Util.SetAlpha(GetButton((int)Buttons.DeckTabButton).GetComponent<Image>(), 1f);
        GetImage((int)Images.CollectionScrollView).gameObject.SetActive(false);
        GetImage((int)Images.DeckScrollView).gameObject.SetActive(true);
    }

    public void OnDeckButtonClicked(PointerEventData data)
    {
        var buttonNumber = data.pointerPress.GetComponent<DeckButtonInfo>().DeckIndex;
        var deckList = Util.Camp == Camp.Sheep ? Managers.User.AllDeckSheep : Managers.User.AllDeckWolf;
        var deck = deckList.First(deck => deck.DeckNumber == buttonNumber);
        
        foreach (var d in deckList) d.LastPicked = false;
        deck.LastPicked = true;
        
        var packetDict = deckList.ToDictionary(d => d.DeckId, d => d.LastPicked);
        var updateLastDeckPacket = new UpdateLastDeckPacketRequired
        {
            AccessToken = Managers.User.AccessToken,
            LastPickedInfo = packetDict
        };
        Managers.Web.SendPutRequest<UpdateLastDeckPacketResponse>(
            "Collection/UpdateLastDeck", updateLastDeckPacket, response => { });

        if (Util.Camp == Camp.Sheep) Managers.User.DeckSheep = deck;
        else Managers.User.DeckWolf = deck;
        ResetDeckUI(Util.Camp);
        SetDeckButtonUI(Util.Camp);
    }

    private void OnCollectionTabClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
        Util.SetAlpha(GetButton((int)Buttons.DeckTabButton).GetComponent<Image>(), 0.4f);
        Util.SetAlpha(GetButton((int)Buttons.CollectionTabButton).GetComponent<Image>(), 1f);
        GetImage((int)Images.DeckScrollView).gameObject.SetActive(false);
        GetImage((int)Images.CollectionScrollView).gameObject.SetActive(true);
    }
    
    private void OnCardClickedOnDeck(PointerEventData data)
    {
        if (data.pointerPress.TryGetComponent(out Card card) == false) return;
        SetCardPopupUI(card);
    }
}
