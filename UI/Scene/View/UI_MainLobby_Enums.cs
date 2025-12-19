using System.Collections.Generic;
using UnityEngine;

public partial class UI_MainLobby
{
    private enum Buttons
    {
        ShopButton,
        ShopButtonFocus,
        ItemButton,
        ItemButtonFocus,
        GameButton,
        GameButtonFocus,
        EventButton,
        EventButtonFocus,
        ClanButton,
        ClanButtonFocus,
        
        FactionButton,
        GoldButton,
        SpinelButton,
        
        ProfilePanelButton,
        SettingsButton,
        FriendsButton,
        MailButton,
        
        PlayButton,
        ModeSelectButtonLeft,
        ModeSelectButtonRight,
        
        DeckTabButton,
        CollectionTabButton,
        CraftingTabButton,

        LobbyDeckButton1,
        LobbyDeckButton2,
        LobbyDeckButton3,
        LobbyDeckButton4,
        LobbyDeckButton5,
        
        DeckButton1,
        DeckButton2,
        DeckButton3,
        DeckButton4,
        DeckButton5,
        
        
        ArrangeAllButton,
        ArrangeSummaryButton,
        ArrangeClassButton,
        ArrangeCountButton,
        
        CraftingBackButton,
        CraftingButton,
        CraftUpperArrowButton,
        CraftLowerArrowButton,
        CraftButton,
        
        ReinforcingButton,
        ReinforceButton,
        
        RecyclingButton,
        
        DailyProductsRefreshButton,
        AdsRemover,
        RestorePurchaseButton,
    }

    private enum Texts
    {
        GoldText,
        SpinelText,
        LevelText,
        ExpText,
        
        UsernameText,
        RankText,
        
        MainSettingsButtonText,
        MainFriendsButtonText,
        MainMailButtonText,
        MainMissionButtonText,
        MainGiftButtonText,
        PlayButtonText,
        FriendlyMatchPanelText,
        RankGamePanelText,
        SinglePlayPanelText,
        MainShopButtonText,
        MainItemButtonText,
        MainBattleButtonText,
        MainEventButtonText,
        MainClanButtonText,
        
        MainDeckLabelText,
        MainBattleSettingLabelText,
        MainCraftingButtonText,
        MainReinforcingButtonText,
        MainRecyclingButtonText,
        MainCraftText,
        CraftCountText,
        MainTempText,
        ReinforceCardSelectText,
        ReinforceCardNumberText,
        SuccessRateText,
        SuccessText,
        
        HoldingLabelText,
        NotHoldingLabelText,
        AssetHoldingLabelText,
        AssetNotHoldingLabelText,
        CharacterHoldingLabelText,
        CharacterNotHoldingLabelText,
        MaterialHoldingLabelText,
        
        SpecialDealText,
        SpecialPackageLabelText,
        BeginnerPackageLabelText,
        ReservedSaleLabelText,
        DailyDealLabelText,
        SpinelStoreLabelText,
        GoldStoreLabelText,
        GoldPackageLabelText,
        SpinelPackageLabelText,
        
        RestorePurchaseText,
        RefreshText,
        DailyProductsRefreshButtonTimeText
    }

    private enum Images
    {
        TopPanel,
        GamePanelBackground,
        GameImageGlow,
        LobbyDeck,
        
        FactionButtonIcon,
        ExpSliderBackground,
        
        FriendAlertIcon,
        MailAlertIcon,
        
        ModeSelectButtonLeftFrame,
        ModeSelectButtonRightFrame,
        
        RankGamePanel,
        FriendlyMatchPanel,
        SinglePlayPanel,
        
        ShopBackground,
        ItemBackground,
        EventBackground,
        ClanBackground,
        
        DeckScrollView,
        CollectionScrollView,
        Deck,
        
        ShopPanel,
        UnitHoldingCardPanel,
        UnitNotHoldingCardPanel,
        AssetHoldingCardPanel,
        AssetNotHoldingCardPanel,
        CharacterHoldingCardPanel,
        CharacterNotHoldingCardPanel,
        MaterialHoldingPanel,
        
        UnitHoldingLabelPanel,
        UnitNotHoldingLabelPanel,
        AssetHoldingLabelPanel,
        AssetNotHoldingLabelPanel,
        CharacterHoldingLabelPanel,
        CharacterNotHoldingLabelPanel,
        MaterialHoldingLabelPanel,
        
        BattleSettingPanel,
        CraftingPanel,
        
        CraftingCardPanel,
        CardPedestal,
        
        CraftingBackButtonFakePanel,
        CraftingSelectPanel,
        CraftingCraftPanel,
        CraftCardPanel,
        
        CraftingReinforcePanel,
        ReinforceCardPanel,
        ArrowPanel,
        ReinforceResultPanel,
        
        CraftingRecyclePanel,
        
        MaterialScrollView,
        MaterialPanel,
        
        ShopScrollbar,
        BeginnerPackagePanel,
        SpecialPackagePanel,
        ReservedSalePanel,
        DailyDealPanel,
        DailyDealProductPanel,
        SpinelStorePanel,
        GoldStorePanel,
        GoldPackagePanel,
        SpinelPackagePanel,
        
        NoticeScrollView,
        
        LoadingPanel,
        LoadingMarkImage,
    }

    private void BindDictionary()
    {
        
        _craftingUiDict = new Dictionary<string, GameObject>
        {
            { "CraftingBackButtonFakePanel", GetImage((int)Images.CraftingBackButtonFakePanel).gameObject },
            { "CraftingSelectPanel", GetImage((int)Images.CraftingSelectPanel).gameObject },
            { "CraftingCraftPanel", GetImage((int)Images.CraftingCraftPanel).gameObject },
            { "CraftingReinforcePanel", GetImage((int)Images.CraftingReinforcePanel).gameObject },
            { "CraftingRecyclePanel", GetImage((int)Images.CraftingRecyclePanel).gameObject },
            { "MaterialScrollView", GetImage((int)Images.MaterialScrollView).gameObject }
        };

        _collectionUiDict = new Dictionary<string, GameObject>
        {
            { "UnitHoldingCardPanel", GetImage((int)Images.UnitHoldingCardPanel).gameObject },
            { "UnitNotHoldingCardPanel", GetImage((int)Images.UnitNotHoldingCardPanel).gameObject },
            { "AssetHoldingCardPanel", GetImage((int)Images.AssetHoldingCardPanel).gameObject },
            { "AssetNotHoldingCardPanel", GetImage((int)Images.AssetNotHoldingCardPanel).gameObject },
            { "CharacterHoldingCardPanel", GetImage((int)Images.CharacterHoldingCardPanel).gameObject },
            { "CharacterNotHoldingCardPanel", GetImage((int)Images.CharacterNotHoldingCardPanel).gameObject },
            { "MaterialHoldingPanel", GetImage((int)Images.MaterialHoldingPanel).gameObject },
            { "UnitHoldingLabelPanel", GetImage((int)Images.UnitHoldingLabelPanel).gameObject },
            { "UnitNotHoldingLabelPanel", GetImage((int)Images.UnitNotHoldingLabelPanel).gameObject },
            { "AssetHoldingLabelPanel", GetImage((int)Images.AssetHoldingLabelPanel).gameObject },
            { "AssetNotHoldingLabelPanel", GetImage((int)Images.AssetNotHoldingLabelPanel).gameObject },
            { "CharacterHoldingLabelPanel", GetImage((int)Images.CharacterHoldingLabelPanel).gameObject },
            { "CharacterNotHoldingLabelPanel", GetImage((int)Images.CharacterNotHoldingLabelPanel).gameObject },
            { "MaterialHoldingLabelPanel", GetImage((int)Images.MaterialHoldingLabelPanel).gameObject }
        };
        
        _arrangeButtonDict = new Dictionary<string, GameObject>
        {
            { "ArrangeAllButton", GetButton((int)Buttons.ArrangeAllButton).gameObject },
            { "ArrangeSummaryButton", GetButton((int)Buttons.ArrangeSummaryButton).gameObject },
            { "ArrangeClassButton", GetButton((int)Buttons.ArrangeClassButton).gameObject },
            { "ArrangeCountButton", GetButton((int)Buttons.ArrangeCountButton).gameObject }
        };

        _bottomButtonDict = new Dictionary<string, GameObject>
        {
            { "ShopButton", GetButton((int)Buttons.ShopButton).gameObject },
            { "ItemButton", GetButton((int)Buttons.ItemButton).gameObject },
            { "GameButton", GetButton((int)Buttons.GameButton).gameObject },
            { "EventButton", GetButton((int)Buttons.EventButton).gameObject },
            { "ClanButton", GetButton((int)Buttons.ClanButton).gameObject },
        };
        
        _bottomButtonFocusDict = new Dictionary<string, GameObject>
        {
            { "ShopButtonFocus", GetButton((int)Buttons.ShopButtonFocus).gameObject },
            { "ItemButtonFocus", GetButton((int)Buttons.ItemButtonFocus).gameObject },
            { "GameButtonFocus", GetButton((int)Buttons.GameButtonFocus).gameObject },
            { "EventButtonFocus", GetButton((int)Buttons.EventButtonFocus).gameObject },
            { "ClanButtonFocus", GetButton((int)Buttons.ClanButtonFocus).gameObject },
        };
        
        _tabButtonDict = new Dictionary<string, GameObject>
        {
            { "DeckTabButton", GetButton((int)Buttons.DeckTabButton).gameObject },
            { "CollectionTabButton", GetButton((int)Buttons.CollectionTabButton).gameObject },
            { "CraftingTabButton", GetButton((int)Buttons.CraftingTabButton).gameObject }
        };
        
        _deckButtonDict = new Dictionary<string, GameObject>
        {
            { "DeckButton1", GetButton((int)Buttons.DeckButton1).gameObject },
            { "DeckButton2", GetButton((int)Buttons.DeckButton2).gameObject },
            { "DeckButton3", GetButton((int)Buttons.DeckButton3).gameObject },
            { "DeckButton4", GetButton((int)Buttons.DeckButton4).gameObject },
            { "DeckButton5", GetButton((int)Buttons.DeckButton5).gameObject }
        };
        
        _lobbyDeckButtonDict = new Dictionary<string, GameObject>
        {
            { "LobbyDeckButton1", GetButton((int)Buttons.LobbyDeckButton1).gameObject },
            { "LobbyDeckButton2", GetButton((int)Buttons.LobbyDeckButton2).gameObject },
            { "LobbyDeckButton3", GetButton((int)Buttons.LobbyDeckButton3).gameObject },
            { "LobbyDeckButton4", GetButton((int)Buttons.LobbyDeckButton4).gameObject },
            { "LobbyDeckButton5", GetButton((int)Buttons.LobbyDeckButton5).gameObject }
        };
    }
}