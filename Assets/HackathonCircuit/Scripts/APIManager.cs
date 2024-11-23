using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class APIManager
{
    private string API_URL => FlyPiConfig.Instance.SKETCH_TO_3D_URL;
    private const float REQUEST_TIMEOUT = 10f;

    public IEnumerator SendSketchTo3DRequest(ConversationState input, System.Action<SketchTo3DResponse> callback)
    {
        if (FlyPiConfig.Instance.is_processing_request)
        {
            Debug.LogWarning("Already processing a request. Skipping this request.");
            yield break;
        }
        FlyPiConfig.Instance.is_processing_request = true;
       

        try
        {

        string jsonInput = JsonUtility.ToJson(input);
        Debug.Log($"Sending request with data: {jsonInput}");

        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(API_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonInput);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            float elapsedTime = 0f;
            bool isTimeout = false;
            AsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= REQUEST_TIMEOUT)
                {
                    isTimeout = true;
                    request.Abort();
                    break;
                }
                yield return null;
            }

            if (isTimeout)
            {
                Debug.LogError("Request timed out");
                callback(null);
            }
            else if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
                Debug.Log($"Received raw response: {responseJson}");

                SketchTo3DResponse response = JsonUtility.FromJson<SketchTo3DResponse>(responseJson);
                Debug.Log($"Parsed SketchTo3DResponse: {JsonUtility.ToJson(response)}");

                callback(response);
            }
             else
                {
                    Debug.LogError($"Error: {request.error}");
                    Debug.LogError($"Response Code: {request.responseCode}");
                    Debug.LogError($"Response Body: {request.downloadHandler.text}");
                    Debug.LogError($"Request URL: {API_URL}");
                    Debug.LogError($"Request Headers: {request.GetRequestHeader("Content-Type")}, {request.GetRequestHeader("Authentication")}");
                    callback(null);
                }
            }
        }
        finally
        {
            FlyPiConfig.Instance.is_processing_request = false;
        }
    }

}