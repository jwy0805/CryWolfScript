using NUnit.Framework.Constraints;
// ReSharper disable InconsistentNaming

public readonly struct PolicyPopupResult
{
    public readonly bool AgreedPolicy;
    public readonly bool AgreedTerms;
    public readonly bool IsUnder13;
    
    public PolicyPopupResult(bool agreedPolicy, bool agreedTerms, bool isUnder13)
    {
        AgreedPolicy = agreedPolicy;
        AgreedTerms = agreedTerms;
        IsUnder13 = isUnder13;
    }
}

public enum MaterialId
{
    None = 2000,
    Bristles = 2001,
    DownFeather = 2002,
    Feather = 2003,
    GuardHair = 2004,
    Hairball = 2005,
    CardPlatePeasant = 2006,
    CardPlateSquire = 2007,
    CardPlateKnight = 2008,
    CardPlateNobleKnight = 2009,
    CardPlateBaron = 2010,
    CardPlateEarl = 2011,
    CardPlateDuke = 2012,
    ClayDawn = 2013,
    ClayEarth = 2014,
    ClayFire = 2015,
    ClayForest = 2016,
    ClayRock = 2017,
    ClayWater = 2018,
    LeatherLowGrade = 2019,
    LeatherMidGrade = 2020,
    LeatherHighGrade = 2021,
    LeatherTopGrade = 2022,
    PigmentBlack = 2023,
    PigmentBlue = 2024,
    PigmentGreen = 2025,
    PigmentPurple = 2026,
    PigmentRed = 2027,
    PigmentYellow = 2028,
    SoulPowderBysscaligo = 2029,
    SoulPowderGrellude = 2030,
    SoulPowderIscreh = 2031,
    SoulPowderMistykile = 2032,
    SoulPowderSandibreeze = 2033,
    SoulPowderVoltenar = 2034,
    SoulPowderZumarigloom = 2035,
    RainbowEgg = 2036,
}

public enum ProductId
{
    None = 0,
    OverPower,
    BeginningOfTheLegend1,
    BeginningOfTheLegend2,
    BeginnersSpirit,
    BeginnersResolve,
    BeginnersLuck,
    BeginnersAmbition,
    RainbowEggPackage,
    GoldPile,
    GoldPouch,
    GoldBasket,
    GoldVault,
    SpinelPile,
    SpinelFistful,
    SpinelPouch,
    SpinelBasket,
    SpinelChest,
    SpinelVault,
    LowGradeMaterialBox,
    MidGradeMaterialBox,
    TopGradeMaterialBox,
    PeasantCardPlate,
    SquireCardPlate,
    KnightCardPlate,
    NobleKnightCardPlate,
    BaronCardPlate,
    PeasantCardPlate5,
    SquireCardPlate5,
    KnightCardPlate5,
    NobleKnightCardPlate5,
    BaronCardPlate5,
    SelectableKnight3BoxSheep,
    SelectableKnight3BoxWolf,
    RandomKnight3Box,
    SelectableKnight2BoxSheep,
    SelectableKnight2BoxWolf,
    RandomNobleKnight2Box,
    SelectableNobleKnight3BoxSheep,
    SelectableNobleKnight3BoxWolf,
    RandomPeasant1Box,
    RandomSquire1Box,
    RandomKnight1Box,
    RandomNobleKnight1Box,
    RandomBaron1Box,
    RandomPeasant2Box,
    RandomSquire2Box,
    RandomKnight2Box,
    RandomBaron2Box,
    RandomSheepBox,
    RandomEnchantBox,
    WoodenChest,
    GoldenChest,
    JeweledChest,
    RandomBaron1To2Box1,
    RandomPeasantMaterialBox,
    RandomSquireMaterialBox,
    RandomKnightMaterialBox,
    RandomNobleKnightMaterialBox,
    RandomBaronMaterialBox,
    GoldenPass,
    RandomBaron3Box,
    AdsRemover,
    PeasantCardPlate3,
    SquireCardPlate3,
    KnightCardPlate3,
    NobleKnightCardPlate3,
    BaronCardPlate3,
    Bunny3,
    Bunny2,
    Rabbit2,
    Rabbit,
    Hare,
    Mushroom3,
    Mushroom2,
    Fungi2,
    Fungi,
    Toadstool,
    Seed3,
    Seed2,
    Sprout2,
    Sprout,
    FlowerPot,
    Bud3,
    Bud2,
    Bloom2,
    Bloom,
    Blossom,
    PracticeDummy3,
    PracticeDummy2,
    TargetDummy2,
    TargetDummy,
    TrainingDummy,
    Shell3,
    Shell2,
    Spike2,
    Spike,
    Hermit,
    SunBlossom3,
    SunBlossom2,
    SunflowerFairy2,
    SunflowerFairy,
    SunfloraPixie,
    MothLuna3,
    MothLuna2,
    MothMoon2,
    MothMoon,
    MothCelestial,
    Soul3,
    Soul2,
    Haunt2,
    Haunt,
    SoulMage,
    DogPup3,
    DogPup2,
    DogBark2,
    DogBark,
    DogBowwow,
    Burrow3,
    Burrow2,
    MoleRat2,
    MoleRat,
    MoleRatKing,
    MosquitoBug3,
    MosquitoBug2,
    MosquitoPester2,
    MosquitoPester,
    MosquitoStinger,
    WolfPup3,
    WolfPup2,
    Wolf2,
    Wolf,
    Werewolf,
    Bomb3,
    Bomb2,
    SnowBomb2,
    SnowBomb,
    PoisonBomb,
    Cacti3,
    Cacti2,
    Cactus2,
    Cactus,
    CactusBoss,
    Snakelet3,
    Snakelet2,
    Snake2,
    Snake,
    SnakeNaga,
    Lurker3,
    Lurker2,
    Creeper2,
    Creeper,
    Horror,
    Skeleton3,
    Skeleton2,
    SkeletonGiant2,
    SkeletonGiant,
    SkeletonMage,
    Spinel10,
    Spinel20,
    Spinel30,
    Spinel50,
    Gold100,
    Gold200,
    Gold500,
    Gold1000,
    RandomPeasant3Box,
    RandomSquire3Box,
    RandomNobleKnight3Box,
}

public enum ProductType
{
    Container = 0, // Other Product
    Unit = 1,
    Material = 2,
    Enchant = 3,
    Sheep = 4,
    Character = 5,
    Gold = 6,
    Spinel = 7,
    Exp = 8,
    Subscription = 9,
}

public enum TutorialType
{
    Main = 0,
    BattleWolf = 1,
    BattleSheep = 2,
    ChangeFaction = 3,
    Collection = 4,
    Crafting = 5,
    InGameTooltip = 6,
}

public enum ProductCategory
{
    None = 0,
    SpecialPackage = 1,
    BeginnerPackage = 2,
    GoldStore = 3,
    SpinelStore = 4,
    GoldPackage = 5,
    SpinelPackage = 6,
    ReservedSale = 7,
    DailyDeal = 8,
    Pass = 9,
    Other = 100,
}   

public enum MailType
{
    None,
    Notice,
    Invite,
    Product,
    Reward
}

public enum SubscriptionType
{
    None = 0, 
    SeasonPass = 1,
    AdsRemover = 2,
}

public enum CurrencyType
{
    None,
    Cash,
    Gold,
    Spinel
}

public enum CurrencyId
{
    None,
    Gold = 4001,
    Spinel = 4002
}

public enum CashCurrencyType
{
    None,
    KRW,
    USD,
    JPY,
    CNY,
    EUR,
    GBP, // Great Britain Pound
    CAD, // Canadian Dollar
    AUD, // Australian Dollar
    NZD, // New Zealand Dollar
    CHF, // Swiss Franc
    SEK, // Swedish Krona
    DKK, // Danish Krone
    NOK, // Norwegian Krone
    ZAR, // South African Rand
    RUB, // Russian Ruble
    BRZ, // Brazilian Real
    MXN, // Mexican Peso
    INR, // Indian Rupee
    IDR, // Indonesian Rupiah
    VNĐ, // Vietnamese Dong
    THB, // Thai Baht
}

public enum Countries
{
    None,
    KR, // South Korea
    US, // United States
    JP, // Japan
    CN, // China
    EU, // European Union
    BR, // Brazil
    ID, // Indonesia
    VN, // Vietnam
    TH, // Thailand
    TW, // Taiwan
}

public enum TransactionStatus
{
    None,
    Completed,
    Refunded,
    Cancelled,
    Failed,
    Pending,
    Expired,
    PartiallyRefunded,
}

public enum ConsentStatus
{
    None,
    AllFinished,
    PolicyFinished,
    RegulationFinished,
}

public enum FontType
{
    None,
    Bold,
    BlackLined,
    BlueLined,
    RedLined,
}

public enum EventRepeatType
{
    None,
    Daily,
    Weekly,
    Monthly,
}

public enum EventConditionType
{
    None,
    Counter,
}

public enum EventCounterKey
{
    friendly_match,
    first_purchase,
    single_play_win,
}