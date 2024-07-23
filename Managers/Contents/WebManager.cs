using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public enum Env
{
    Local,
    Dev,
    Stage,
    Prod
}

public class WebManager
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
    
    public void SendPostRequest<T>(string url, object obj, Action<T> responseAction)
    {
        Managers.Instance.StartCoroutine(
            CoSendWebRequest(url, UnityWebRequest.kHttpVerbPOST, obj, responseAction));
    }
    
    public void SendPutRequest<T>(string url, object obj, Action<T> responseAction)
    {
        Managers.Instance.StartCoroutine(
            CoSendWebRequest(url, UnityWebRequest.kHttpVerbPUT, obj, responseAction));
    }
    
    public void SendGetRequest<T>(string url, object obj, Action<T> responseAction)
    {
        Managers.Instance.StartCoroutine(
            CoSendWebRequest(url, UnityWebRequest.kHttpVerbGET, obj, responseAction));
    }

    public Task<T> SendPostRequestAsync<T>(string url, object obj)
    {
        var tcs = new TaskCompletionSource<T>();
        SendPostRequest<T>(url, obj, response => tcs.SetResult(response));
        return tcs.Task;
    }
    
    public Task<T> SendPutRequestAsync<T>(string url, object obj)
    {
        var tcs = new TaskCompletionSource<T>();
        SendPutRequest<T>(url, obj, response => tcs.SetResult(response));
        return tcs.Task;
    }
    
    private IEnumerator CoSendWebRequest<T>(string url, string method, object obj, Action<T> responseAction)
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
        
        yield return uwr.SendWebRequest();
        
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
