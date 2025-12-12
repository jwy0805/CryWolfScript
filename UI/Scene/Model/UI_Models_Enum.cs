public enum UserRole
{
    User,
    Admin
}

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
    Open = 3,
    OpenResult = 4,
    Subscription = 5, // subscription reward popup
}

public enum VirtualPaymentCode
{
    None,
    Product,
    Subscription
}

public enum CashPaymentErrorCode
{
    None = 0,
    InvalidReceipt = 1,     // 위조/만료/포맷 오류 - 재시도x
    Unauthorized = 2,       // 토큰 만료/권한 오류 - 재로그인
    AlreadyProcessed = 3,   // 이미 처리된 영수증 - 멱등성 처리용
    InternalError = 4,      // 서버 내부 오류 - 재시도
}

public enum NoticeType
{
    None,
    Notice,
    Event,
    Emergency
}