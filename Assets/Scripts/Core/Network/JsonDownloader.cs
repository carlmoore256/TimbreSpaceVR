using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Threading.Tasks;

public class JsonDownloader : MonoBehaviour
{
    public static async Task<T> Download<T>(string url)
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(url);
        var operation = webRequest.SendWebRequest();

        while (!operation.isDone)
        {
            Debug.Log($"[*] Downloading JSON from {url}");
            await System.Threading.Tasks.Task.Delay(100);
        }

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error downloading JSON: " + webRequest.error);
            // return default(T);
            return default;
        }

        string json = webRequest.downloadHandler.text;
        T result = JsonHelper.FromJson<T>(json);
        return result;
    }
}