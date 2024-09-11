using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public Dictionary<int, Contents.UnitData> UnitDict { get; private set; } = new();
    public Dictionary<int, Contents.ObjectData> ObjectDict { get; private set; } = new();
    public Dictionary<int, Contents.SkillData> SkillDict { get; private set; } = new();

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
