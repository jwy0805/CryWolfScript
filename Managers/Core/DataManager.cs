using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Unity.VisualScripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

/* Last Modified : 24. 10. 08
   * Version : 1.013
   */

public interface ILoader<TKey, TValue>
{
    Dictionary<TKey, TValue> MakeDict();
}

public class DataManager
{
    private bool _isInitialized = false;
    public Dictionary<int, Contents.UnitData> UnitDict { get; private set; } = new();
    public Dictionary<int, Contents.ObjectData> ObjectDict { get; private set; } = new();
    public Dictionary<int, Contents.SkillData> SkillDict { get; private set; } = new();
    public Dictionary<TutorialType, Contents.TutorialData> TutorialDict { get; private set; } = new();
    public Dictionary<string, Dictionary<string, Contents.LocalizationEntry>> LocalizationDict { get; set; } = new();

    public Dictionary<UnitId, List<Skill>> MainSkillDict { get; } = new()
    {
        { UnitId.Bunny, new List<Skill>() },
        { UnitId.Rabbit, new List<Skill> { Skill.RabbitAggro } },
        { UnitId.Hare, new List<Skill> { Skill.HarePunch } },
        { UnitId.Mushroom, new List<Skill>() },
        { UnitId.Fungi, new List<Skill> { Skill.FungiClosestHeal } },
        { UnitId.Toadstool, new List<Skill> { Skill.ToadstoolPoisonCloud } },
        { UnitId.Seed, new List<Skill>() },
        { UnitId.Sprout, new List<Skill> { Skill.SproutFireAttack } },
        { UnitId.FlowerPot, new List<Skill> { Skill.FlowerPotDoubleTargets } },
        { UnitId.Bud, new List<Skill>() },
        { UnitId.Bloom, new List<Skill> { Skill.Bloom3Combo } },
        { UnitId.Blossom, new List<Skill> { Skill.BlossomDeath } },
        { UnitId.PracticeDummy, new List<Skill>() },
        { UnitId.TargetDummy, new List<Skill> { Skill.TargetDummyAggro } },
        { UnitId.TrainingDummy, new List<Skill> { Skill.TrainingDummyFaintAttack } },
        { UnitId.Shell, new List<Skill>() },
        { UnitId.Spike, new List<Skill> { Skill.SpikeReflection } },
        { UnitId.Hermit, new List<Skill> { Skill.HermitShield } },
        { UnitId.SunBlossom, new List<Skill> { Skill.SunBlossomDefence } },
        { UnitId.SunflowerFairy, new List<Skill> { Skill.SunflowerFairyFenceHeal } },
        { UnitId.SunfloraPixie, new List<Skill> { Skill.SunfloraPixieInvincible } },
        { UnitId.MothLuna, new List<Skill>() },
        { UnitId.MothMoon, new List<Skill> { Skill.MothMoonSheepHeal } },
        { UnitId.MothCelestial, new List<Skill> { Skill.MothCelestialBreed } },
        { UnitId.Soul, new List<Skill>() },
        { UnitId.Haunt, new List<Skill> { Skill.HauntFire } },
        { UnitId.SoulMage, new List<Skill> { Skill.SoulMageMagicPortal } },
        { UnitId.DogPup, new List<Skill>() },
        { UnitId.DogBark, new List<Skill> { Skill.DogBarkAdjacentAttackSpeed } },
        { UnitId.DogBowwow, new List<Skill> { Skill.DogBowwowSmash } },
        { UnitId.Burrow, new List<Skill> { Skill.BurrowHalfBurrow } },
        { UnitId.MoleRat, new List<Skill> { Skill.MoleRatBurrowEvasion } },
        { UnitId.MoleRatKing, new List<Skill> { Skill.MoleRatKingBurrow } },
        { UnitId.MosquitoBug, new List<Skill>() },
        { UnitId.MosquitoPester, new List<Skill> { Skill.MosquitoPesterWoolRate } },
        { UnitId.MosquitoStinger, new List<Skill> { Skill.MosquitoStingerSheepDeath } },
        { UnitId.WolfPup, new List<Skill>() },
        { UnitId.Wolf, new List<Skill> { Skill.WolfMagicalAttack } },
        { UnitId.Werewolf, new List<Skill> { Skill.WerewolfThunder } },
        { UnitId.Bomb, new List<Skill> { Skill.BombBomb } },
        { UnitId.SnowBomb, new List<Skill> { Skill.SnowBombAreaAttack } },
        { UnitId.PoisonBomb, new List<Skill> { Skill.PoisonBombRecoverPoison } },
        { UnitId.Cacti, new List<Skill>() },
        { UnitId.Cactus, new List<Skill> { Skill.CactusReflection } },
        { UnitId.CactusBoss, new List<Skill> { Skill.CactusBossRush } },
        { UnitId.Snakelet, new List<Skill>() },
        { UnitId.Snake, new List<Skill> { Skill.SnakeFire } },
        { UnitId.SnakeNaga, new List<Skill> { Skill.SnakeNagaMeteor } },
        { UnitId.Lurker, new List<Skill>() },
        { UnitId.Creeper, new List<Skill> { Skill.CreeperRoll } },
        { UnitId.Horror, new List<Skill> { Skill.HorrorDivision } },
        { UnitId.Skeleton, new List<Skill>() },
        { UnitId.SkeletonGiant, new List<Skill> { Skill.SkeletonGiantRevive } },
        { UnitId.SkeletonMage, new List<Skill> { Skill.SkeletonMageCurse } }
    };

    // Dictionaries for DB caching only before introducing REDIS
    public Dictionary<int, UnitInfo> UnitInfoDict { get; set;  } = new();
    public Dictionary<int, SheepInfo> SheepInfoDict { get; set;  } = new();
    public Dictionary<int, EnchantInfo> EnchantInfoDict { get; set; } = new();
    public Dictionary<int, CharacterInfo> CharacterInfoDict { get; set; } = new();
    public Dictionary<int, MaterialInfo> MaterialInfoDict { get; set; } = new();
    public Dictionary<Tuple<UnitClass, int>, ReinforcePointInfo> ReinforcePointDict { get; set; } = new();
    public Dictionary<int, UnitMaterialInfo> CraftingMaterialDict { get; set; } = new();
    // Dictionaries for DB caching only before introducing REDIS

    public async Task InitAsync()
    {
        if (_isInitialized) return;
        if (UnitDict.Count > 0 || ObjectDict.Count > 0 || SkillDict.Count > 0 || TutorialDict.Count > 0 ||
            LocalizationDict.Count > 0) return;
        
        var unitLoaderTask = LoadJsonAsync<Contents.UnitLoader>("UnitData");
        var objectLoaderTask = LoadJsonAsync<Contents.ObjectLoader>("ObjectData");
        var skillLoaderTask = LoadJsonAsync<Contents.SkillLoader>("SkillData");
        var tutorialLoaderTask = LoadJsonAsync<Contents.TutorialLoader>("TutorialData");
        var localeDictTask = LoadJsonAsync<Dictionary<string, Dictionary<string, Contents.LocalizationEntry>>>("LanguageData");
        
        await Task.WhenAll(unitLoaderTask, objectLoaderTask, skillLoaderTask, tutorialLoaderTask, localeDictTask);
        
        UnitDict = unitLoaderTask.Result!.MakeDict();
        ObjectDict = objectLoaderTask.Result!.MakeDict();
        SkillDict = skillLoaderTask.Result!.MakeDict();
        TutorialDict = tutorialLoaderTask.Result!.MakeDict();
        LocalizationDict = localeDictTask.Result;
        
        _isInitialized = true;
    }

    private async Task<T> LoadJsonAsync<T>(string data)
    {
        var json = await LoadJsonStringAsync(data);
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError($"Cannot load {data}.json");
            return default;
        }

        try
        {
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter(),
                    new StringEnumConverter(),
                },
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return JsonConvert.DeserializeObject<T>(json, settings);
        }
        catch (Exception e)
        {
            Debug.LogError($"DataManager : JSON deserialization error for {data}.json: {e.Message}");
            throw;
        }
    }
    
    private async Task<string> LoadJsonStringAsync(string data)
    {
        if (Managers.Network.UseAddressables)
        {
            var handle = Addressables.LoadAssetAsync<TextAsset>($"Data/{data}.json");

            try
            {
                await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                    return handle.Result.text;

                Debug.LogError($"[DataManager] Addressables key miss: {data}.json");
                return null;
            }
            finally
            {
                Addressables.Release(handle);
            }
        }

        var filePath = Path.Combine(Application.dataPath, "Data", $"{data}.json");
        if (File.Exists(filePath))
        {
            return await File.ReadAllTextAsync(filePath);
        }
        
        Debug.LogError($"Data Manager : Cannot find {data}.json");
        return null;
    }
}
