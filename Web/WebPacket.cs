using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
// ReSharper disable UnassignedField.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable CollectionNeverUpdated.Global

public class UserInfo
{
    public string UserName { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public int ExpToLevelUp { get; set; }
    public int RankPoint { get; set; }
    public int HighestRankPoint { get; set; }
    public int Victories { get; set; }
    public int WinRate { get; set; }
    public int Gold { get; set; }
    public int Spinel { get; set; }
    public bool BattleTutorialDone { get; set; }
    public bool CollectionTutorialDone { get; set; }
    public bool ReinforceTutorialDone { get; set; }
}

public class FriendUserInfo
{
    public string UserName { get; set; }
    public int Level { get; set; }
    public int RankPoint { get; set; }
    public UserAct Act { get; set; }
    public FriendStatus FriendStatus { get; set; }
}

public class MailInfo
{
    public int MailId { get; set; }
    public MailType Type { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int ProductId { get; set; }
    public ProductCategory ProductCategory { get; set; }
    public bool Claimed { get; set; }
    public string Message { get; set; }
    public string Sender { get; set; }
}

public class ProductInfo
{
    public int Id { get; set; }
    public List<CompositionInfo> Compositions { get; set; }
    public int Price { get; set; }
    public CurrencyType CurrencyType { get; set; }
    public ProductCategory Category { get; set; }
    public string ProductCode { get; set; }
}

public class CompositionInfo
{
    public int Id { get; set; }
    public int CompositionId { get; set; }
    public ProductType Type { get; set; }
    public int Count { get; set; }
    public int MinCount { get; set; }
    public int MaxCount { get; set; }
    public bool Guaranteed { get; set; }
    public bool IsSelectable { get; set; }
}

public class DailyProductInfo
{
    public int Id { get; set; }
    public int Price { get; set; }
    public CurrencyType CurrencyType { get; set; }
    public ProductCategory Category { get; set; }
    public bool AlreadyBought { get; set; }
}

public class UnitInfo : IAsset
{
    public int Id { get; set; }
    public UnitClass Class { get; set; }
    public int Level { get; set; }
    public int Species { get; set; }
    public Role Role { get; set; }
    public Faction Faction { get; set; }
    public UnitRegion Region { get; set; }
}

public class SheepInfo : IAsset
{
    public int Id { get; set; }
    public UnitClass Class { get; set; }
}

public class EnchantInfo : IAsset
{
    public int Id { get; set; }
    public UnitClass Class { get; set; }
}

public class CharacterInfo : IAsset
{
    public int Id { get; set; }
    public UnitClass Class { get; set; }
}

public class MaterialInfo : IAsset
{
    public int Id { get; set; }
    public UnitClass Class { get; set; }
}

public class ReinforcePointInfo
{
    public UnitClass Class { get; set; }
    public int Level { get; set; }
    public int Point { get; set; }
}

public class OwnedUnitInfo
{
    public UnitInfo UnitInfo { get; set; }
    public int Count { get; set; }
}

public class OwnedSheepInfo
{
    public SheepInfo SheepInfo { get; set; }
    public int Count { get; set; }
}

public class OwnedEnchantInfo
{
    public EnchantInfo EnchantInfo { get; set; }
    public int Count { get; set; }
}

public class OwnedCharacterInfo
{
    public CharacterInfo CharacterInfo { get; set; }
    public int Count { get; set; }
}

public class OwnedMaterialInfo
{
    public MaterialInfo MaterialInfo { get; set; }
    public int Count { get; set; }
}

public class BattleSettingInfo
{
    public SheepInfo SheepInfo { get; set; }
    public EnchantInfo EnchantInfo { get; set; }
    public CharacterInfo CharacterInfo { get; set; }
}

public class DeckInfo
{
    public int DeckId { get; set; }
    public UnitInfo[] UnitInfo { get; set; }
    public int DeckNumber { get; set; }
    public int Faction { get; set; }
    public bool LastPicked { get; set; }
}

public class UnitMaterialInfo
{
    public int UnitId { get; set; }
    public List<OwnedMaterialInfo> Materials { get; set; }
}

public class UserStageInfo
{
    public int UserId { get; set; }
    public int StageId { get; set; }
    public int StageLevel { get; set; }
    public int StageStar { get; set; }
    public bool IsCleared { get; set; }
    public bool IsAvailable { get; set; }
}

public class StageEnemyInfo
{
    public int StageId { get; set; }
    public List<UnitId> UnitIds { get; set; }
}

public class StageRewardInfo
{
    public int StageId { get; set; }
    public List<SingleRewardInfo> RewardProducts { get; set; }
}

public class SingleRewardInfo
{
    public int ItemId { get; set; }
    public ProductType ProductType { get; set; }
    public int Count { get; set; }
    public int Star { get; set; }
}

public class RewardInfo
{
    public int ItemId { get; set; }
    public ProductType ProductType { get; set; }
    public int Count { get; set; }
}

public class ValidateNewAccountPacketRequired
{
    public string UserAccount { get; set; }
    public string Password { get; set; }
}

public class ValidateNewAccountPacketResponse
{
    public bool ValidateOk { get; set; }
    public int ErrorCode { get; set; }
}

public class CreateUserAccountPacketRequired
{
    public string UserAccount { get; set; }
    public string Password { get; set; }
}

public class CreateUserAccountPacketResponse
{
    public bool CreateOk { get; set; }
    public string Message { get; set; }
}

public class LoginUserAccountPacketRequired
{
    public string UserAccount { get; set; }
    public string Password { get; set; }
}

public class LoginUserAccountPacketResponse
{
    public bool LoginOk { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class GetAppleTokenPacketRequired
{
    public string IdToken { get; set; }
}

public class GetAppleTokenPacketResponse
{
    public string CustomToken { get; set; }
}

public class LoginApplePacketRequired
{
    public string IdToken { get; set; }
}

public class LoginApplePacketResponse
{
    public bool LoginOk { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class LoginGooglePacketRequired
{
    public string IdToken { get; set; }
}

public class LoginGooglePacketResponse
{
    public bool LoginOk { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class ChangeActPacketRequired
{
    public string AccessToken { get; set; }
    public int SessionId { get; set; }
    public Faction Faction { get; set; }
    public int MapId { get; set; }
}

public class ChangeActPacketResponse
{
    public bool ChangeOk { get; set; }
}

public class ChangeActPacketSingleRequired
{
    public string AccessToken { get; set; }
    public int SessionId { get; set; }
    public int StageId { get; set; }
    public Faction Faction { get; set; }
}

public class ChangeActPacketSingleResponse
{
    public bool ChangeOk { get; set; }
}

public class ChangeActTestPacketRequired
{
    public string AccessToken { get; set; }
    public int SessionId { get; set; }
    public Faction Faction { get; set; }
    public int MapId { get; set; }
}

public class ChangeActTestPacketResponse
{
    public bool ChangeOk { get; set; }
}

public class CancelMatchPacketRequired
{
    public string AccessToken { get; set; }
}

public class CancelMatchPacketResponse
{
    public bool CancelOk { get; set; }
}

public class SurrenderPacketRequired
{
    public string AccessToken { get; set; }
}

public class SurrenderPacketResponse
{
    public bool SurrenderOk { get; set; }
}

public class LoadInfoPacketRequired
{
    public bool LoadInfo { get; set; }
}

public class LoadInfoPacketResponse
{
    public bool LoadInfoOk { get; set; }
    public List<UnitInfo> UnitInfos { get; set; }
    public List<SheepInfo> SheepInfos { get; set; }
    public List<EnchantInfo> EnchantInfos { get; set; }
    public List<CharacterInfo> CharacterInfos { get; set; }
    public List<MaterialInfo> MaterialInfos { get; set; }
    public List<ReinforcePointInfo> ReinforcePoints { get; set; }
    public List<UnitMaterialInfo> CraftingMaterials { get; set; }
}

public class LoadUserInfoPacketRequired
{
    public string AccessToken { get; set; }
}

public class LoadUserInfoPacketResponse
{
    public bool LoadUserInfoOk { get; set; }
    public UserInfo UserInfo { get; set; }
}

public class LoadTestUserPacketRequired
{
    public int UserId { get; set; }
}

public class LoadTestUserPacketResponse
{
    public bool LoadTestUserOk { get; set; }
    public UserInfo UserInfo { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class UpdateUserInfoPacketRequired
{
    public string AccessToken { get; set; }
    public UserInfo UserInfo { get; set; }
}

public class UpdateUserInfoPacketResponse
{
    public bool UpdateUserInfoOk { get; set; }
}

public class UpdateTutorialRequired
{
    public string AccessToken { get; set; }
    public TutorialType[] TutorialTypes { get; set; }
    public bool Done { get; set; }
}

public class UpdateTutorialResponse
{
    public bool UpdateTutorialOk { get; set; }
}

public class RefreshTokenRequired
{
    public string RefreshToken { get; set; }
}

public class RefreshTokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class InitCardsPacketRequired
{
    public string AccessToken { get; set; }
    public Env Environment { get; set; }
}

public class InitCardsPacketResponse
{
    public bool GetCardsOk { get; set; }
    public List<OwnedUnitInfo> OwnedCardList { get; set; }
    public List<UnitInfo> NotOwnedCardList { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class InitSheepPacketRequired
{
    public string AccessToken { get; set; }
    public Env Environment { get; set; }
}

public class InitSheepPacketResponse
{
    public bool GetSheepOk { get; set; }
    public List<OwnedSheepInfo> OwnedSheepList { get; set; }
    public List<SheepInfo> NotOwnedSheepList { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class InitEnchantPacketRequired
{
    public string AccessToken { get; set; }
    public Env Environment { get; set; }
}

public class InitEnchantPacketResponse
{
    public bool GetEnchantOk { get; set; }
    public List<OwnedEnchantInfo> OwnedEnchantList { get; set; }
    public List<EnchantInfo> NotOwnedEnchantList { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class InitCharacterPacketRequired
{
    public string AccessToken { get; set; }
    public Env Environment { get; set; }
}

public class InitCharacterPacketResponse
{
    public bool GetCharacterOk { get; set; }
    public List<OwnedCharacterInfo> OwnedCharacterList { get; set; }
    public List<CharacterInfo> NotOwnedCharacterList { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class InitMaterialPacketRequired
{
    public string AccessToken { get; set; }
    public Env Environment { get; set; }
}

public class InitMaterialPacketResponse
{
    public bool GetMaterialOk { get; set; }
    public List<OwnedMaterialInfo> OwnedMaterialList { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class GetInitDeckPacketRequired
{
    public string AccessToken { get; set; }
    public Env Environment { get; set; }
}

public class GetInitDeckPacketResponse
{
    public bool GetDeckOk { get; set; }
    public List<DeckInfo> DeckList { get; set; }
    public BattleSettingInfo BattleSetting { get; set; }
}

public class GetSelectedDeckRequired
{
    public string AccessToken { get; set; }
    public int Faction { get; set; }
    public int DeckNumber { get; set; }
}

public class GetSelectedDeckResponse
{
    public bool GetSelectedDeckOk { get; set; }
    public DeckInfo Deck { get; set; }
}

public class UpdateDeckPacketRequired
{
    public string AccessToken { get; set; }
    public int DeckId { get; set; }
    public UnitId UnitIdToBeDeleted { get; set; }
    public UnitId UnitIdToBeUpdated { get; set; }
}

public class UpdateDeckPacketResponse
{
    public int UpdateDeckOk { get; set; }
}

public class UpdateLastDeckPacketRequired
{
    public string AccessToken { get; set; }
    public Dictionary<int, bool> LastPickedInfo { get; set; }
}

public class UpdateLastDeckPacketResponse
{
    public bool UpdateLastDeckOk { get; set; }
}

public class UpdateBattleSettingPacketRequired
{
    public string AccessToken { get; set; }
    public BattleSettingInfo BattleSettingInfo { get; set; }
}

public class UpdateBattleSettingPacketResponse
{
    public bool UpdateBattleSettingOk { get; set; }   
}

public class LoadMaterialsPacketRequired
{
    public string AccessToken { get; set; }
    public UnitId UnitId { get; set; }
}

public class LoadMaterialsPacketResponse
{
    public List<OwnedMaterialInfo> CraftingMaterialList { get; set; }
    public bool LoadMaterialsOk { get; set; }
}

public class CraftCardPacketRequired
{
    public string AccessToken { get; set; }
    public List<OwnedMaterialInfo> Materials { get; set; }
    public UnitId UnitId { get; set; }
    public int Count { get; set; }
}

public class CraftCardPacketResponse
{
    // Error: 0 - Success, 1 - Not enough materials
    public bool CraftCardOk { get; set; }
    public int Error { get; set; }
}

public class ReinforceResultPacketRequired
{
    public string AccessToken { get; set; }
    public UnitInfo UnitInfo { get; set; }
    public List<UnitInfo> UnitList { get; set; }
}

public class ReinforceResultPacketResponse
{
    public bool ReinforceResultOk { get; set; }
    public bool IsSuccess { get; set; }
    public List<OwnedUnitInfo> OwnedUnitList { get; set; }
    public int Error { get; set; }
}

public class InitProductPacketRequired
{
    public string AccessToken { get; set; }
}

public class InitProductPacketResponse
{
    public bool GetProductOk { get; set; }
    public List<ProductInfo> SpecialPackages { get; set; }
    public List<ProductInfo> BeginnerPackages { get; set; }
    public List<ProductInfo> GoldPackages { get; set; }
    public List<ProductInfo> SpinelPackages { get; set; }
    public List<ProductInfo> GoldItems { get; set; }
    public List<ProductInfo> SpinelItems { get; set; }
    public List<ProductInfo> ReservedSales { get; set; }
    public List<DailyProductInfo> DailyDeals { get; set; }
}

public class FriendListPacketRequired
{
    public string AccessToken { get; set; }
}

public class FriendListPacketResponse
{
    public bool FriendListOk { get; set; }
    public List<FriendUserInfo> FriendList { get; set; }
}

public class SearchUsernamePacketRequired
{
    public string AccessToken { get; set; }
    public string Username { get; set; }
}

public class SearchUsernamePacketResponse
{
    public bool SearchUsernameOk { get; set; }
    public List<FriendUserInfo> FriendUserInfos { get; set; }
}

public class FriendRequestPacketRequired
{
    public string AccessToken { get; set; }
    public string FriendUsername { get; set; }
    public FriendStatus CurrentFriendStatus { get; set; }
}

public class FriendRequestPacketResponse
{
    public bool FriendRequestOk { get; set; }
    public FriendStatus FriendStatus { get; set; }
}

public class LoadPendingFriendPacketRequired
{
    public string AccessToken { get; set; }
}

public class LoadPendingFriendPacketResponse
{
    public bool LoadPendingFriendOk { get; set; }
    public List<FriendUserInfo> PendingFriendList { get; set; }
}

public class AcceptFriendPacketRequired
{
    public string AccessToken { get; set; }
    public string FriendUsername { get; set; }
    public bool Accept { get; set; }
}

public class AcceptFriendPacketResponse
{
    public bool AcceptFriendOk { get; set; }
    public bool Accept { get; set; }
}

public class LoadPendingMailPacketRequired
{
    public string AccessToken { get; set; }
}

public class LoadPendingMailPacketResponse
{
    public bool LoadPendingMailOk { get; set; }
    public List<MailInfo> PendingMailList { get; set; }
}

public class ClaimMailPacketRequired
{
    public string AccessToken { get; set; }
    public int MailId { get; set; }
}

public class ClaimMailPacketResponse
{
    public bool ClaimMailOk { get; set; }
}

public class InviteFriendlyMatchPacketRequired
{
    public string AccessToken { get; set; }
    public string InviterName { get; set; }
    public string InviteeName { get; set; }
    public Faction InviterFaction { get; set; }
    public Faction InviteeFaction { get; set; }
}

public class InviteFriendlyMatchPacketResponse
{
    public bool InviteOk { get; set; }
}

public class AcceptInvitationPacketRequired
{
    public string AccessToken { get; set; }
    public bool Accept { get; set; }
    public string InviteeName { get; set; }
}

public class AcceptInvitationPacketResponse
{
    public bool AcceptInvitationOk { get; set; }
    public Faction MyFaction { get; set; }
    public UserInfo EnemyInfo { get; set; }
    public DeckInfo EnemyDeckSheep { get; set; }
    public DeckInfo EnemyDeckWolf { get; set; }
}

public class VirtualPaymentPacketRequired
{
    public string AccessToken { get; set; }
    public string ProductCode { get; set; }
}

public class VirtualPaymentPacketResponse
{
    public bool PaymentOk { get; set; }
}

public class CashPaymentPacketRequired
{
    public string AccessToken { get; set; }
    public string Receipt { get; set; }
    public string ProductCode { get; set; }
}

public class CashPaymentPacketResponse
{
    public bool PaymentOk { get; set; }
}

public class LoadStageInfoPacketRequired
{
    public string AccessToken { get; set; }
}

public class LoadStageInfoPacketResponse
{
    public bool LoadStageInfoOk { get; set; }
    public List<UserStageInfo> UserStageInfos { get; set; }
    public List<StageEnemyInfo> StageEnemyInfos { get; set; }
    public List<StageRewardInfo> StageRewardInfos { get; set; }
}