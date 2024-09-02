using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class WebService : IWebService
{
    private const string LocalPort = "7270";
    private const string DevPort = "499";
    private const string StagePort = "";
    
    public Env Environment { get; set; } = Env.Local;

    private string BaseUrl
    {
        get
        {
            return Environment switch
            {
                Env.Local => $"https://localhost:{LocalPort}/api",
                Env.Dev => $"https://localhost:{DevPort}/api",
                Env.Stage => $"https://localhost:{StagePort}/api",
                Env.Prod => $"https://localhost:{StagePort}/api",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
    
    public virtual async Task<T> SendWebRequestAsync<T>(string url, string method, object obj)
    {
        var tcs = new TaskCompletionSource<T>();
        await SendWebRequest<T>(url, method, obj, response => tcs.SetResult(response));
        return await tcs.Task;
    }
    
    public virtual async Task SendWebRequest<T>(string url, string method, object obj, Action<T> responseAction)
    {
        var sendUrl = $"{BaseUrl}/{url}";
        byte[] jsonBytes = null;
        
        if (obj != null)
        {
            var jsonStr = JsonConvert.SerializeObject(obj);
            jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
        }

        using var uwr = new UnityWebRequest(sendUrl, method);
        uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        var operation = uwr.SendWebRequest();

        while (operation.isDone == false)
        {
            await Task.Yield();
        }
        
        if (uwr.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"{uwr.error} : {uwr.downloadHandler.text}");
        }
        else
        {
            var resObj = JsonConvert.DeserializeObject<T>(uwr.downloadHandler.text);
            responseAction?.Invoke(resObj);
        }
    }
}
