using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Unity.VisualScripting;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

/* Last Modified : 24. 10. 08
   * Version : 1.013
   */

public interface ILoader<TKey, TValue>
{
    Dictionary<TKey, TValue> MakeDict();
}

public class DataManager
{
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
        { UnitId.Wolf, new List<Skill> { Skill.WolfLastHitDna } },
        { UnitId.Werewolf, new List<Skill> { Skill.WerewolfThunder } },
        { UnitId.Bomb, new List<Skill> { Skill.BombBomb } },
        { UnitId.SnowBomb, new List<Skill> { Skill.SnowBombAreaAttack } },
        { UnitId.PoisonBomb, new List<Skill> { Skill.PoisonBombSelfDestruct } },
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
        if (UnitDict.Count != 0 
            && ObjectDict.Count != 0 
            && SkillDict.Count != 0 
            && TutorialDict.Count != 0
            && LocalizationDict.Count != 0)
        {
            return;
        }
        
        Debug.Log("Admin Log: Loading contents...");
        
        var unitDictTask = LoadJsonAsync<Contents.UnitLoader, int, Contents.UnitData>("UnitData");
        var objectDictTask = LoadJsonAsync<Contents.ObjectLoader, int, Contents.ObjectData>("ObjectData");
        var skillDictTask = LoadJsonAsync<Contents.SkillLoader, int, Contents.SkillData>("SkillData");
        var tutorialDictTask = LoadJsonAsync<Contents.TutorialLoader, TutorialType, Contents.TutorialData>("TutorialData");
        var localizationDictTask = LoadJsonAsync<Dictionary<string, Dictionary<string, Contents.LocalizationEntry>>>("LanguageData");
        
        await Task.WhenAll(unitDictTask, objectDictTask, skillDictTask, tutorialDictTask, localizationDictTask);
        
        var loader = unitDictTask.Result!; // Contents.UnitLoader
        // 중복 Id 찾아서 로그 남기기
        var dupes = loader.Units
            .GroupBy(u => u.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (dupes.Any())
            Debug.LogError($"UnitData.json에 중복된 Id 발견! [{string.Join(", ", dupes)}]");
        
        UnitDict = unitDictTask.Result!.MakeDict();
        ObjectDict = objectDictTask.Result!.MakeDict();
        SkillDict = skillDictTask.Result!.MakeDict();
        TutorialDict = tutorialDictTask.Result!.MakeDict();
        LocalizationDict = localizationDictTask.Result;
    }

    private async Task<TLoader> LoadJsonAsync<TLoader, TKey, TValue>(string data) where TLoader : ILoader<TKey, TValue>
    {
        return await LoadJsonAsync<TLoader>(data);
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
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON deserialization error: {e.Message}");
            throw;
        }
    }

    private async Task<string> LoadJsonStringAsync(string data)
    {
        var filePath = Path.Combine(Application.streamingAssetsPath, $"{data}.json");
        
#if UNITY_ANDROID && !UNITY_EDITOR
        var url = filePath.StartsWith("jar:") ? filePath : $"jar:file://{filePath}";
        using var req = UnityWebRequest.Get(url);
        await req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Cannot load {data}.json: {req.error}");
            return null;
        }
        return req.downloadHandler.text;
#else
        if (File.Exists(filePath))
        {
            return await File.ReadAllTextAsync(filePath);
        }
        
        Debug.LogError($"Cannot find {data}.json");
        return null;
#endif
    }
}
