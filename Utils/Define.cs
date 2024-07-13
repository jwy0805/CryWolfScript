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
        Result,
        Store,
        MatchMaking
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
}

