using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;
public class ContextSender : MonoBehaviour
{

    [SerializeField]
    Toggle captureImageToggle;

    [SerializeField]
    Toggle toggleCamera;

    [SerializeField]
    Image renderImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        SetupListeners();

    }

    void SetupListeners(){

        toggleCamera.onValueChanged.AddListener(ToggleCamera);
        captureImageToggle.onValueChanged.AddListener(CaptureImage);

    }

    void ToggleCamera(bool value){

        Debug.Log("ToggleCamera: " + value);
        renderImage.enabled = value;

    }

    void CaptureImage(bool value){

        Debug.Log("CaptureImage: " + value);
        EventManager.TriggerEvent(EventList.OnCameraCaptureRequest, "test");

    }

    [Button("Send Context")]
    void SendContext(){

        EventManager.TriggerEvent(EventList.OnCameraCaptureRequest, "test");

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
