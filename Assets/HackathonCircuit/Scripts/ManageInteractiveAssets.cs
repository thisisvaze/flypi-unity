using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Networking;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using TMPro;
public class ManageInteractiveAssets : MonoBehaviour
{
    public static ManageInteractiveAssets Instance { get; private set; }
     [SerializeField]
    private float distanceFromCamera = 0.5f;
    [SerializeField]
    private float horizontalSpacing = 0.3f;
    private float currentHorizontalOffset = 0f;
    [SerializeField]
    private GameObject ModelWrapperPrefab;

    private string downloadedModelsDataPath;
    private Texture2D texture;

       // Add a HashSet to keep track of downloaded model URLs
    private HashSet<string> downloadedModelUrls = new HashSet<string>();
    private Dictionary<string, (string name, string category, string creator_name)> downloadedModelIdentifiers = new Dictionary<string, (string name, string category, string creator_name)>();
    private Dictionary<string, string> downloadedImageIdentifiers = new Dictionary<string, string>();




     private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        downloadedModelsDataPath = Application.persistentDataPath + "/Resources/";
    }

      // Add this new method to reset object positioning
    private void ResetObjectPositioning()
    {
        currentHorizontalOffset = 0f;
    }
    
 
public List<ModelResponseForAgent> GetCurrent3DModels()
{
    List<ModelResponseForAgent> currentModels = new List<ModelResponseForAgent>();
    var models = GameObject.FindObjectsOfType<GLTFast.GltfBoundsAsset>();

    Debug.Log($"Found {models.Length} models in the scene.");

    foreach (GLTFast.GltfBoundsAsset model in models)
    {
        Debug.Log($"Model found: {model.gameObject.name}");
        if (downloadedModelIdentifiers.TryGetValue(model.gameObject.name, out var modelInfo))
        {
            currentModels.Add(new ModelResponseForAgent
            {
                id = model.gameObject.name,
                name = modelInfo.name,
                category = modelInfo.category,
                url = "", // We don't store the URL, so leave it empty
                creator_name = modelInfo.creator_name
            });
        }
        else
        {
            // Fallback for models that weren't downloaded through our system
            currentModels.Add(new ModelResponseForAgent
            {
                id = model.gameObject.name,
                name = model.gameObject.name,
                category = "Unknown",
                url = ""
            });
        }
    }

    return currentModels;
}

public List<ImageResponseForAgent> GetCurrentImages()
{
    List<ImageResponseForAgent> currentImages = new List<ImageResponseForAgent>();
    var images = GameObject.FindObjectsOfType<Image>();

    Debug.Log($"Found {images.Length} images in the scene.");

    foreach (Image image in images)
    {
        Debug.Log($"Image found: {image.gameObject.name}");

        if (downloadedImageIdentifiers.TryGetValue(image.gameObject.name, out var imageName))
        {
            currentImages.Add(new ImageResponseForAgent
            {
                id = image.gameObject.name,
                name = imageName,
                url = "" // We don't store the URL, so leave it empty
            });
        }
        else
        {
            // Fallback for images that weren't downloaded through our system
            currentImages.Add(new ImageResponseForAgent
            {
                id = image.gameObject.name,
                name = image.gameObject.name,
                url = ""
            });
        }
    }

    return currentImages;
}
public void Create3DModels(List<ModelResponseForAgent> createModels)
{
    ResetObjectPositioning();
    foreach (var model in createModels)
    {
        if (!downloadedModelIdentifiers.ContainsKey(model.id))
        {
            try
            {
                StartCoroutine(ModelDownloader(model.url, model.id));
                downloadedModelIdentifiers[model.id] = (model.name, model.category, model.creator_name);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error downloading model: {e.Message}");
            }
        }
        else
        {
            Debug.Log("Model already downloaded: " + model.url);
            LoadDownloadedModel(model.id);
        }
    }
}

public void Delete3DModels(List<ModelResponseForAgent> deleteModels)
{
    foreach (var model in deleteModels)
    {
        DeleteModel(model.id);
    }
}

private void DeleteModel(string modelId)
{
    var models = GameObject.FindObjectsOfType<GLTFast.GltfAsset>();
    foreach (GLTFast.GltfAsset model in models)
    {
        if (model.gameObject.name == modelId)
        {
            Destroy(model.gameObject);
            downloadedModelIdentifiers.Remove(modelId);
            Debug.Log($"Deleted model with ID: {modelId}");
            break;
        }
    }
}

// Update ModelDownloader to use model ID instead of name
IEnumerator ModelDownloader(string model_url, string model_id)
{
    var uwr = new UnityWebRequest(model_url, UnityWebRequest.kHttpVerbGET);
    string path = Path.Combine(downloadedModelsDataPath, model_id + ".glb");
    uwr.downloadHandler = new DownloadHandlerFile(path);
    yield return uwr.SendWebRequest();
    if (uwr.result != UnityWebRequest.Result.Success)
        Debug.LogError(uwr.error);
    else
    {
        LoadDownloadedModel(model_id);
        Debug.Log("File successfully downloaded and saved to " + path);
    }
}

  // Update LoadDownloadedModel method
    async void LoadDownloadedModel(string model_id)
    {
        Vector3 position = GetNextPosition();
        var x = Instantiate(ModelWrapperPrefab, position, Camera.main.transform.rotation);
        x.name = model_id;
        var success = await x.GetComponentInChildren<GLTFast.GltfAsset>().Load(downloadedModelsDataPath + model_id + ".glb");
        Render3DModelWithScale(x);
    }



    // Add this new method
private Vector3 GetNextPosition()
{
    Vector3 cameraPosition = Camera.main.transform.position;
    Vector3 cameraForward = Camera.main.transform.forward;
    Vector3 cameraRight = Camera.main.transform.right;

    // Calculate the center position in front of the user
    Vector3 centerPosition = cameraPosition + cameraForward * distanceFromCamera;

    // Calculate the offset from the center
    float offset = currentHorizontalOffset - (horizontalSpacing * 0.5f);

    // Calculate the final position
    Vector3 position = centerPosition + cameraRight * offset;

    // Update the horizontal offset for the next object
    currentHorizontalOffset += horizontalSpacing;

    return position;
}


      private string ExtractModelIdentifier(string modelUrl)
    {
        var uri = new Uri(modelUrl);
        var segments = uri.Segments;
        // Assuming the identifier is always before the ".glb" segment
        return segments.FirstOrDefault(segment => segment.EndsWith(".glb"))?.Trim('/');
    }


private void Render3DModelWithScale(GameObject y)
{
    GameObject x = y.transform.Find("Sketchfab_model").gameObject;
    x.transform.localScale = Vector3.one;
    x.transform.rotation = Quaternion.identity;

    // Calculate combined bounds
    Renderer[] renderers = x.GetComponentsInChildren<Renderer>();
    if (renderers.Length == 0)
    {
        Debug.LogWarning("No renderers found in the model.");
        return;
    }

    Bounds combinedBounds = new Bounds(renderers[0].bounds.center, Vector3.zero);
    foreach (Renderer renderer in renderers)
    {
        combinedBounds.Encapsulate(renderer.bounds);
    }

    var hgi = y.GetComponent<SketchfabModelWrapper>().handGrabInteractable;
    hgi.transform.SetParent(x.transform);

    Vector3 size = combinedBounds.size;
    Vector3 center = combinedBounds.center;
    Debug.Log("Rendered size: " + size);

    // Calculate scale
    float dim_limit = 100f;
    float max_dim = Mathf.Max(size.x, size.y, size.z);
    float s = 0.001f * dim_limit / max_dim;

    // Add Rigidbody
    var rb = x.AddComponent<Rigidbody>();
    rb.useGravity = false;
    rb.isKinematic = true;
    rb.angularDamping = 0.0f;
    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

    // Add BoxCollider
    var bc = x.AddComponent<BoxCollider>();
    bc.center = center - x.transform.position;
    bc.size = size;
    bc.isTrigger = true;

    // connect surface collider for rays
    // var sc = x.AddComponent<ColliderSurface>();
    // sc.InjectCollider(bc);

    // var rayInteractableSetup = y.transform.Find("RayInteractableSetup").gameObject;
    // rayInteractableSetup.transform.SetParent(x.transform);
    // var rayInteractable = rayInteractableSetup.GetComponent<RayInteractable>();
    // rayInteractable.InjectSurface(sc);
    // rayInteractableSetup.SetActive(true);


    Debug.Log("Ideal scale: " + s);

    // Apply initial scale
    x.transform.localScale = new Vector3(s/100, s/100, s/100);
    hgi.gameObject.SetActive(true);
    hgi.GetComponent<HandGrabInteractable>().enabled = true;

    // Animate scale
    LeanTween.scale(x, new Vector3(s, s, s), 0.4f).setEase(LeanTweenType.easeInOutQuad);
     // Play animation if available
    PlayAnimationIfAvailable(x);
}

private void PlayAnimationIfAvailable(GameObject modelObject)
{
    var animator = modelObject.GetComponentInChildren<Animator>();
    if (animator != null)
    {
        animator.enabled = true;
        if (animator.runtimeAnimatorController != null)
        {
            animator.Play(0);
        }
    }
}



}

public class BypassCertificate : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}
