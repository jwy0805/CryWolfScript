using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

/* Last Modified : 24. 09. 12
 * Version : 0.02
 */

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public Dictionary<int, Contents.UnitData> UnitDict { get; private set; } = new();
    public Dictionary<int, Contents.ObjectData> ObjectDict { get; private set; } = new();
    public Dictionary<int, Contents.SkillData> SkillDict { get; private set; } = new();

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
    // Dictionaries for DB caching only before introducing REDIS

    public async Task InitAsync()
    {
        UnitDict = (await LoadJsonAsync<Contents.UnitLoader, int, Contents.UnitData>("UnitData"))!.MakeDict();
        ObjectDict = (await LoadJsonAsync<Contents.ObjectLoader, int, Contents.ObjectData>("ObjectData"))!.MakeDict();
        SkillDict = (await LoadJsonAsync<Contents.SkillLoader, int, Contents.SkillData>("SkillData"))!.MakeDict();
    }
    
    private async Task<TLoader> LoadJsonAsync<TLoader, TKey, TValue>(string data) where TLoader : ILoader<TKey, TValue>
    {
        var filePath = Path.Combine(Application.streamingAssetsPath, $"{data}.json");
        string jsonContent;

        if (Application.platform == RuntimePlatform.Android)
        {
            // Android
            using var www = UnityWebRequest.Get(filePath);
            var operation = www.SendWebRequest();

            while (operation.isDone == false)
            {
                await Task.Yield();
            }
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load {data}.json");
                return default;
            }

            jsonContent = www.downloadHandler.text;
        }
        else
        {
            // Other platforms
            if (File.Exists(filePath))
            {
                jsonContent = await File.ReadAllTextAsync(filePath);
            }
            else
            {
                Debug.LogError($"Cannot find {data}.json");
                return default;
            }
        }

        return JsonConvert.DeserializeObject<TLoader>(jsonContent);
    }
}
