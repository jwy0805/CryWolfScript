public enum MaterialId
{
    None = 0,
    Bristles = 1,
    DownFeather = 2,
    Feather = 3,
    GuardHair = 4,
    Hairball = 5,
    CardPlatePeasant = 6,
    CardPlateSquire = 7,
    CardPlateKnight = 8,
    CardPlateNobleKnight = 9,
    CardPlateBaron = 10,
    CardPlateEarl = 11,
    CardPlateDuke = 12,
    ClayDawn = 13,
    ClayEarth = 14,
    ClayFire = 15,
    ClayForest = 16,
    ClayRock = 17,
    ClayWater = 18,
    LeatherLowGrade = 19,
    LeatherMidGrade = 20,
    LeatherHighGrade = 21,
    LeatherTopGrade = 22,
    PigmentBlack = 23,
    PigmentBlue = 24,
    PigmentGreen = 25,
    PigmentPurple = 26,
    PigmentRed = 27,
    PigmentYellow = 28,
    SoulPowderBysscaligo = 29,
    SoulPowderGrellude = 30,
    SoulPowderIscreh = 31,
    SoulPowderMistykile = 32,
    SoulPowderSandibreeze = 33,
    SoulPowderVoltenar = 34,
    SoulPowderZumarigloom = 35,
    RainbowEgg = 36,
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
}

public enum ProductType
{
    None = 0, // Other Product
    Unit,
    Material,
    Enchant,
    Sheep,
    Character,
    Gold,
    Spinel,
}

public enum TutorialType
{
    Main = 0,
    Battle = 1,
    Collection = 2,
    Reinforce = 3
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