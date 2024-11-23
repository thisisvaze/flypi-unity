using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using System.Linq;
using System;
using Anaglyph.DisplayCapture;
using System.Collections;
public class ActionManager : MonoBehaviour
{
    private APIManager apiManager;
    private ConversationState conversationState;
    private ConversationState testState;

    void Start()
    {
        apiManager = new APIManager();
        conversationState = new ConversationState();
        testState = new ConversationState(true);
    }



    [Button("Reset Conversation")]
    void ResetConversation()
    {
        testState = new ConversationState(true);
    }

    [Button("Send Test Request")]
    void SendTestRequestToServer()
    {

        AddScreenshotToContext(testState);
        StartCoroutine(apiManager.SendSketchTo3DRequest(testState.current_state, 
            sketchTo3DResponse => {
                if (sketchTo3DResponse != null)
                {
                    Debug.Log("Received combined response");
                    OnSketchTo3DResponseReceived(sketchTo3DResponse.actions);
                }
                else
                {
                    Debug.LogWarning("Received null combined response");
                }
            }
        ));
    }
void AddScreenshotToContext(ConversationState state)
{
    try {
        // Get screenshot texture reference (this is fast)
        Texture2D screenTexture = DisplayCaptureManager.Instance.ScreenCaptureTexture;
        
        // Start a coroutine to handle the expensive operations
        StartCoroutine(ProcessScreenshotAsync(screenTexture, base64String => {
            // Update the context once we have the base64 string
            if (state.current_state.sketch_context == null)
            {
                state.current_state.sketch_context = new SketchContext();
            }
            state.current_state.sketch_context.screenshot = base64String;
            Debug.Log("Screenshot added to context: " + base64String);
            
            // Now that we have the screenshot, proceed with sending the request
            SendRequestToServer();
        }));
    }
    catch(Exception e) {
        string screenshotBase64 = SampleImageB64.SampleMoon;  // Initialize the variable
        Debug.LogError("Error adding screenshot to context: " + e.Message);
        
        // Handle error case synchronously
        if (state.current_state.sketch_context == null)
        {
            state.current_state.sketch_context = new SketchContext();
        }
        state.current_state.sketch_context.screenshot = screenshotBase64;
        Debug.Log("Screenshot added to context: " + screenshotBase64);
    }
}

private IEnumerator ProcessScreenshotAsync(Texture2D texture, Action<string> onComplete)
{
    // Move to end of frame to not block main thread
    yield return new WaitForEndOfFrame();
    
    // Do expensive operations
    byte[] bytes = texture.EncodeToPNG();
    string base64String = System.Convert.ToBase64String(bytes);
    
    // Call the completion handler with the result
    onComplete(base64String);
} 

    void SendRequestToServer()
    {
        AddScreenshotToContext(conversationState);
        StartCoroutine(apiManager.SendSketchTo3DRequest(conversationState.current_state, 
            sketchTo3DResponse => {
                if (sketchTo3DResponse != null)
                {
                    Debug.Log("Received combined response");
                    OnSketchTo3DResponseReceived(sketchTo3DResponse.actions);
                }
                else
                {
                    Debug.LogWarning("Received null combined response");
                }
            }
        ));
    }


    void OnEnable()
    {
        EventManager.StartListening(EventList.OnCameraCaptureRequest, OnCameraCaptureRequest);
    }

    void OnDisable()
    {
        EventManager.StopListening(EventList.OnCameraCaptureRequest, OnCameraCaptureRequest);
    }

void OnCameraCaptureRequest(string message)
{
   // Debug.Log("OnCameraCaptureRequest: " + message[:10]);
    SendRequestToServer();
}



private void OnSketchTo3DResponseReceived(SketchTo3DResponseAction actions)
{
    if (actions != null)
    { 

        if (actions.CREATE_3D_MODEL.Count > 0)
        {
            ManageInteractiveAssets.Instance.Create3DModels(actions.CREATE_3D_MODEL);
        }
        
        
    }
}
}