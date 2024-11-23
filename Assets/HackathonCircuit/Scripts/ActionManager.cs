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
            // Get screenshot texture reference
            Texture2D screenTexture = DisplayCaptureManager.Instance.ScreenCaptureTexture;
            string base64String = ProcessScreenshot(screenTexture);
            
            // Update the context with the screenshot
            if (state.current_state.sketch_context == null)
            {
                state.current_state.sketch_context = new SketchContext();
            }
            
            // If we got an empty string or null, use sample moon
            base64String = string.IsNullOrEmpty(base64String) ? SampleImageB64.Sample : base64String;
            state.current_state.sketch_context.screenshot = base64String;
            Debug.Log("Screenshot added to context");
            
            // Now that we have the screenshot, proceed with sending the request
            //SendRequestToServer();
        }
        catch(Exception e) {
            Debug.LogError("Error adding screenshot to context: " + e.Message);
            
            // Handle error case with sample moon image
            if (state.current_state.sketch_context == null)
            {
                state.current_state.sketch_context = new SketchContext();
            }
            state.current_state.sketch_context.screenshot = SampleImageB64.Sample;
            Debug.Log("Using sample moon image as fallback");
        }
    }

    private string ProcessScreenshot(Texture2D texture)
    {
        if (texture == null) return null;
        
        try {
            byte[] bytes = texture.EncodeToPNG();
            return System.Convert.ToBase64String(bytes);
        }
        catch (Exception e) {
            Debug.LogError("Error processing screenshot: " + e.Message);
            return null;
        }
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