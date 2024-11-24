using UnityEngine;

public class WireManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float endpointSize = 0.01f;
    [SerializeField] private Color startPointColor = Color.green;
    [SerializeField] private Color endPointColor = Color.red;
    [SerializeField] private Color wireColor = Color.red;
    [SerializeField] private float wireWidth = 0.02f;
    [SerializeField] private int segmentCount = 30;
    [SerializeField] private float curvature = 2f;
    [SerializeField] private float stretchFactor = 0.3f;

    public class WireEndpoints
    {
        public GameObject startPointObject;
        public GameObject endPointObject;
        public WireController wire;
    }

    public WireEndpoints CreateWireWithEndpoints(Vector3 startPosition, Vector3 endPosition)
    {
        // Create a container for this wire group
        GameObject wireGroup = new GameObject("Wire_Group");
        wireGroup.transform.SetParent(transform);

        // Create start point
        GameObject startPoint = CreateEndpoint("StartPoint", startPosition, startPointColor);
        startPoint.transform.SetParent(wireGroup.transform);

        // Create end point
        GameObject endPoint = CreateEndpoint("EndPoint", endPosition, endPointColor);
        endPoint.transform.SetParent(wireGroup.transform);

        // Create wire segments parent
        GameObject wireParent = new GameObject("Wire_Segments");
        wireParent.transform.SetParent(wireGroup.transform);

        // Create wire segments
        GameObject[] segments = new GameObject[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            segments[i] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            segments[i].name = $"Wire_Segment_{i}";
            segments[i].transform.SetParent(wireParent.transform);

            // Set up the segment's material
            Renderer wireRenderer = segments[i].GetComponent<Renderer>();
            Material wireMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            wireMaterial.color = wireColor;
            wireMaterial.EnableKeyword("_EMISSION");
            wireMaterial.SetColor("_EmissionColor", wireColor * 2f);
            wireRenderer.material = wireMaterial;
            wireRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            wireRenderer.receiveShadows = false;
        }

        // Add and initialize WireController
        WireController wire = wireParent.AddComponent<WireController>();
        wire.Initialize(segments, wireWidth, curvature, stretchFactor);
        wire.SetEndpoints(startPoint.transform, endPoint.transform);

        return new WireEndpoints
        {
            startPointObject = startPoint,
            endPointObject = endPoint,
            wire = wire
        };
    }
private GameObject CreateEndpoint(string name, Vector3 position, Color color)
{
    GameObject endpoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    endpoint.name = name;
    endpoint.transform.position = position;
    endpoint.transform.localScale = Vector3.one * endpointSize;

    endpoint.tag = "WireEnd";

    // Add Rigidbody to the endpoint
    Rigidbody rb = endpoint.AddComponent<Rigidbody>();
    rb.useGravity = false;
    rb.linearDamping = 5f;
    
    // Make sure the collider is a trigger
    SphereCollider collider = endpoint.GetComponent<SphereCollider>();
    collider.isTrigger = true;

    // Add HandGrabModule prefab as child
    GameObject handGrabPrefab = Resources.Load<GameObject>("HandGrabModule");
    if (handGrabPrefab != null)
    {
        GameObject handGrabInstance = Instantiate(handGrabPrefab, endpoint.transform);
        handGrabInstance.transform.localPosition = Vector3.zero;
        handGrabInstance.transform.localRotation = Quaternion.identity;

        // Get the interactable components and assign the rigidbody
        var grabbable = handGrabInstance.GetComponent<Oculus.Interaction.Grabbable>();
        if (grabbable != null)
        {
            grabbable.InjectOptionalTargetTransform(endpoint.transform);
        }
        
        // var handGrabInteractable = handGrabInstance.GetComponent<Oculus.Interaction.HandGrab.HandGrabInteractable>();
        // var grabInteractable = handGrabInstance.GetComponent<Oculus.Interaction.GrabInteractable>();

        // if (handGrabInteractable != null)
        // {
        //     handGrabInteractable.Rigidbody = rb;
        // }

        // if (grabInteractable != null)
        // {
        //     grabInteractable.Rigidbody = rb;
        // }
    }
    else
    {
        Debug.LogError("HandGrabModule prefab not found! Make sure it's in a Resources folder.");
    }

    // Set up renderer and material
    Renderer renderer = endpoint.GetComponent<Renderer>();
    Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    material.color = color;
    material.EnableKeyword("_EMISSION");
    material.SetColor("_EmissionColor", color * 2f);
    renderer.material = material;

        return endpoint;
    }

    public WireEndpoints CreateWireAtPosition(Vector3 position)
    {
        return CreateWireWithEndpoints(
            position,
            position + Vector3.right
        );
    }

    public void DeleteWire(WireEndpoints wireEndpoints)
    {
        if (wireEndpoints != null && wireEndpoints.startPointObject != null)
        {
            Destroy(wireEndpoints.startPointObject.transform.parent.gameObject);
        }
    }
}