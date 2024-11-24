using UnityEngine;
using System.Collections.Generic;
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

     private List<WireEndpoints> activeWires = new List<WireEndpoints>();
    private HashSet<Transform> connectedPoints = new HashSet<Transform>();

    public class WireEndpoints
    {
        public GameObject startPointObject;
        public GameObject endPointObject;
        public WireController wire;
    }

    public void DeleteWire(WireEndpoints wireEndpoints)
    {
        if (wireEndpoints != null)
        {
            activeWires.Remove(wireEndpoints);
            if (wireEndpoints.startPointObject != null)
            {
                Destroy(wireEndpoints.startPointObject.transform.parent.gameObject);
            }
        }
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

    // Create the wire endpoints object
    WireEndpoints wireEndpoints = new WireEndpoints
    {
        startPointObject = startPoint,
        endPointObject = endPoint,
        wire = wire
    };

    // Add to tracking list
    activeWires.Add(wireEndpoints);

    return wireEndpoints;
}
     public bool AreAllWiresConnected()
    {
        // Clear the set of connected points
        connectedPoints.Clear();

        // Check each wire's endpoints
        foreach (var wire in activeWires)
        {
            if (wire.startPointObject == null || wire.endPointObject == null)
                return false;

            Transform startParent = wire.startPointObject.transform.parent;
            Transform endParent = wire.endPointObject.transform.parent;

            // If either endpoint isn't parented to a connection point, the circuit isn't complete
            if (startParent == null || endParent == null)
                return false;

            // Add the connection points to our set
            connectedPoints.Add(startParent);
            connectedPoints.Add(endParent);
        }

        // The circuit is complete if we have at least 2 wires and all connection points are used
        // (forming a closed loop)
        return activeWires.Count >= 2 && connectedPoints.Count >= 4;
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

}