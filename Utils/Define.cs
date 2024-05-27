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
        Lobby,
        Game,
        Result,
        Store,
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
}

