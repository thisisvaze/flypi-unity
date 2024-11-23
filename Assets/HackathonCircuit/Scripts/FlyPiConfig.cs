using UnityEngine;
using UnityEngine.Android;
using System.Collections;
using Oculus.Interaction.Input;
using Oculus.Voice.Dictation;
using Meta.WitAi;

public class FlyPiConfig : MonoBehaviour
{
    public static FlyPiConfig Instance { get; private set; }

    public bool onboarding_completed = false;


    public bool is_processing_request = false;

    public string api_url = "http://localhost:8000/";

    private const string SKETCH_TO_3D_ENDPOINT = "api/v1/retrieve-circuit-schema-test";

    public string SKETCH_TO_3D_URL => api_url + SKETCH_TO_3D_ENDPOINT;


    public GameObject onboardingPanel;


    private AppDictationExperience _appDictationExperience;
    public AppDictationExperience appDictationExperience
    {
        get
        {
            if (_appDictationExperience == null)
            {
                _appDictationExperience = FindObjectOfType<AppDictationExperience>();
            }
            return _appDictationExperience;
        }
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

       private void Start()
    {   
        StartCoroutine(ShowOnboardingPanelAfterDelay());
    }

    private IEnumerator ShowOnboardingPanelAfterDelay()
    {
        
        yield return new WaitForSeconds(2);
        if(!onboarding_completed){
            ShowOnboardingPanel();
        }
    }

    private void ShowOnboardingPanel()
{
    // Calculate the initial position
    Vector3 panelPosition = Camera.main.transform.position + Camera.main.transform.forward * 0.4f;
    
    // Ensure the y-position is at least 0.6 units above the camera
    panelPosition.y = Mathf.Max(panelPosition.y,  0.6f);
    
     // Set the panel's position
    onboardingPanel.transform.position = panelPosition;
    
    // Make the panel face the camera, rotating only around the y-axis
    Vector3 directionToCamera = Camera.main.transform.position - onboardingPanel.transform.position;
    directionToCamera.y = 0; // Ignore vertical difference
    onboardingPanel.transform.rotation = Quaternion.LookRotation(-directionToCamera);
    
        onboardingPanel.SetActive(true);
    }

}