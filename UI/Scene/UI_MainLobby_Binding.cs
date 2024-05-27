using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public partial class UI_MainLobby
{
    private enum Buttons
    {
        CampButton,
        RankButton,
        SingleButton,
        MultiButton,
        ChatButton,
        ClanButton,
        NewsButton,
        
        DeckTabButton,
        DeckButton1,
        DeckButton2,
        DeckButton3,
        DeckButton4,
        DeckButton5,
        
        CollectionTabButton,
    }

    private enum Texts
    {
        GoldText,
        GemText,
        CloverText,
        CloverTimeText,
        
        RankScoreText,
        RankingText,
        RankNameText,
    }

    private enum Images
    {
        GamePanelBackground,
        
        GoldImage,
        GemImage,
        CloverImage,
        CloverTimeImage,
        RankStar,
        RankTextIcon,
        SingleButtonImage,
        MultiButtonImage,
        ChatImageIcon,
        ClanIcon,
        NoticeIcon,
        
        ButtonSelectFrame,
        ShopButtonIcon,
        ShopButtonEffect,
        ItemButtonIcon,
        ItemButtonEffect,
        GameButtonIcon,
        GameButtonEffect,
        EventButtonIcon,
        EventButtonEffect,
        ClanButtonIcon,
        ClanButtonEffect,
        
        DeckScrollView,
        CollectionScrollView,
        Deck,
        HoldingCardPanel,
        NotHoldingCardPanel
    }
    
    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
    }

    protected override void SetButtonEvents()
    {
        GetButton((int)Buttons.CampButton).gameObject.BindEvent(OnCampButtonClicked);
        GetButton((int)Buttons.SingleButton).gameObject.BindEvent(OnSingleClicked);
        GetButton((int)Buttons.MultiButton).gameObject.BindEvent(OnMultiClicked);
        
        GetButton((int)Buttons.DeckTabButton).gameObject.BindEvent(OnDeckTabClicked);
        GetButton((int)Buttons.DeckButton1).gameObject.BindEvent(OnDeckButtonClicked);
        GetButton((int)Buttons.DeckButton2).gameObject.BindEvent(OnDeckButtonClicked);
        GetButton((int)Buttons.DeckButton3).gameObject.BindEvent(OnDeckButtonClicked);
        GetButton((int)Buttons.DeckButton4).gameObject.BindEvent(OnDeckButtonClicked);
        GetButton((int)Buttons.DeckButton5).gameObject.BindEvent(OnDeckButtonClicked);
        
        GetButton((int)Buttons.CollectionTabButton).gameObject.BindEvent(OnCollectionTabClicked);
    }
    
    protected override void SetUI()
    {
        SetObjectSize(GetButton((int)Buttons.CampButton).gameObject, 1.0f);
        SetObjectSize(GetImage((int)Images.GoldImage).gameObject, 1.2f);
        SetObjectSize(GetImage((int)Images.GemImage).gameObject, 1.2f);
        SetObjectSize(GetImage((int)Images.CloverImage).gameObject, 0.9f);
        SetObjectSize(GetImage((int)Images.CloverTimeImage).gameObject, 0.9f);

        // 이미지 크기 조정을 위해 캔버스 강제 업데이트
        Canvas.ForceUpdateCanvases();
        SetObjectSize(GetImage((int)Images.RankStar).gameObject, 0.9f);
        SetObjectSize(GetImage((int)Images.RankTextIcon).gameObject, 0.25f);
        SetObjectSize(GetImage((int)Images.SingleButtonImage).gameObject, 0.7f);
        SetObjectSize(GetImage((int)Images.MultiButtonImage).gameObject, 0.7f);
        
        SetObjectSize(GetImage((int)Images.ChatImageIcon).gameObject, 0.9f);
        SetObjectSize(GetImage((int)Images.ClanIcon).gameObject, 0.6f);
        SetObjectSize(GetImage((int)Images.NoticeIcon).gameObject, 0.6f);
        
        SetObjectSize(GetImage((int)Images.ItemButtonIcon).gameObject, 0.8f);
        SetObjectSize(GetImage((int)Images.GameButtonIcon).gameObject, 0.8f);
        SetObjectSize(GetImage((int)Images.ShopButtonIcon).gameObject, 0.8f);
        SetObjectSize(GetImage((int)Images.EventButtonIcon).gameObject, 0.8f);
        SetObjectSize(GetImage((int)Images.ClanButtonIcon).gameObject, 0.8f);
        
        // MainLobby_Item Setting
        SetMainLobbyItemUI();
    }

    private void SwitchLobbyUI(Camp camp)
    {
        var gamePanelImage = GetImage((int)Images.GamePanelBackground);
        var campButtonImage = GetButton((int)Buttons.CampButton).GetComponent<Image>();
        var singleImage = GetImage((int)Images.SingleButtonImage);
        var multiImage = GetImage((int)Images.MultiButtonImage);
        
        switch (camp)
        {
            case Camp.Sheep:
                gamePanelImage.sprite = Resources.Load<Sprite>("Sprites/MainLobbySheep");
                campButtonImage.sprite = Resources.Load<Sprite>("Sprites/SheepButton");
                singleImage.sprite = Resources.Load<Sprite>("Sprites/SheepButton");
                multiImage.sprite = Resources.Load<Sprite>("Sprites/SheepMultiButton");
                break;
            case Camp.Wolf:
                gamePanelImage.sprite = Resources.Load<Sprite>("Sprites/MainLobbyWolf");
                campButtonImage.sprite = Resources.Load<Sprite>("Sprites/WolfButton");
                singleImage.sprite = Resources.Load<Sprite>("Sprites/WolfButton");
                multiImage.sprite = Resources.Load<Sprite>("Sprites/WolfMultiButton");
                break;
            default:
                break;
        }
    }

    private void SwitchDeck(Camp camp)
    {
        var deckList = Util.Camp == Camp.Sheep ? Managers.User.AllDeckSheep : Managers.User.AllDeckWolf;
        var deck = deckList.FirstOrDefault(d => d.LastPicked) 
                   ?? deckList.First(d => d.DeckNumber == 1);
        
        if (camp == Camp.Sheep)
        {
            Managers.User.DeckSheep = deck;
        }
        else
        {
            Managers.User.DeckWolf = deck;
        }
        
        ResetDeckUI(camp);
    }

    private void SwitchCollection(Camp camp)
    {
        SetCollection(camp);
    }
}
