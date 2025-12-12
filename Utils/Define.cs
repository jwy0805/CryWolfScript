public class Define
{
    public enum MouseEvent
    {
        Press,
        PointerDown,
        PointerUp,
        Click,
        Drag,
    }
    
    public enum Scene
    {
        Unknown,
        Start,
        Login,
        MainLobby,
        Game,
        MatchMaking,
        FriendlyMatch,
        SinglePlay,
        Loading,
    }
    
    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount,
    }

    public enum UIEvent
    {
        Click,
        Drag,
    }
    
    public enum WorldObject
    {
        Unknown,
        Fence,
        PlayerSheep,
        PlayerWolf,
        Monster,
        Sheep,
        Tower,
        Ground,
        Item,
    }

    public enum PopupType
    {
        UpgradePopup
    }

    public enum ServerType
    {
        Api,
        MatchMaking,
        Socket
    }
    
    public enum GameMode
    {
        RankGame = 0,
        FriendlyMatch = 1,
        SinglePlay = 2,
    }
    
    public enum SelectMode
    {
        Normal,
        CraftingPanel,
        Craft,
        Reinforce,
        Recycle
    }
    
    public enum ArrangeMode
    {
        All,
        Summary,
        Class,
        Count
    }
}

