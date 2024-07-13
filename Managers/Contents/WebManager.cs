using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class WebManager
{
    private const string LocalPort = "7270";
    private const string DevPort = "499";
    private const string StagePort = "";

    private string BaseUrl { get; } = $"https://localhost:{LocalPort}/api";
    
    public void SendPostRequest<T>(string url, object obj, Action<T> responseAction)
    {
        Managers.Instance.StartCoroutine(CoSendWebRequest(url, UnityWebRequest.kHttpVerbPOST, obj, responseAction));
    }
    
    public void SendPutRequest<T>(string url, object obj, Action<T> responseAction)
    {
        Managers.Instance.StartCoroutine(CoSendWebRequest(url, UnityWebRequest.kHttpVerbPUT, obj, responseAction));
    }
    
    public void SendGetRequest<T>(string url, object obj, Action<T> responseAction)
    {
        Managers.Instance.StartCoroutine(CoSendWebRequest(url, UnityWebRequest.kHttpVerbGET, obj, responseAction));
    }

    public Task<T> SendPostRequestAsync<T>(string url, object obj)
    {
        var tcs = new TaskCompletionSource<T>();
        SendPostRequest<T>(url, obj, response => tcs.SetResult(response));
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
