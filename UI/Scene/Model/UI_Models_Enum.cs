public enum FriendStatus
{
    None,
    Pending,
    Accepted,
    Blocked
}

public enum AcquisitionPath
{
    None = 0,
    Shop = 1,
    Reward = 2,
    Rank = 3,
    Single = 4,
    Mission = 5,
    Tutorial = 6,
    Event = 7,
    Open = 8,
}

public enum RewardPopupType
{
    None = 0, // from result popup, to main lobby
    Item = 1, // all item popup
    Select = 2, // select popup
    Open = 3, // random open popup 
}