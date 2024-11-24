using UnityEngine;
using System.Collections.Generic;

class Generate3DCircuitModel : MonoBehaviour
{
    [System.Serializable]
    public class Component
    {
        public string id;
        public string type;
    }

    [System.Serializable]
    public class Connection
    {
        public string component;
        public string[] connections;
    }

    [System.Serializable]
    public class CircuitData
    {
        public Component[] components;
        public Connection[] connections;
    }

[Header("Prefab References")]
[SerializeField] private GameObject batteryPrefab;
[SerializeField] private GameObject resistorPrefab;
[SerializeField] private GameObject ledPrefab;
[SerializeField] private GameObject switchPrefab;
[SerializeField] private WireManager wireManager;

    private Dictionary<string, GameObject> componentInstances = new Dictionary<string, GameObject>();


private Vector3[] cornerPositions = new Vector3[4] {
    new Vector3(0, 0, 0),  // Bottom Left
    new Vector3(0, 0, 1),  // Top Left
    new Vector3(1, 0, 0),  // Bottom Right
    new Vector3(1, 0, 1)   // Top Right
};

private int currentCornerIndex = 0;
    
private void Start()
{
    string testJson = @"{
        ""components"": [
            {""id"": ""comp1"", ""type"": ""battery""},
            {""id"": ""comp2"", ""type"": ""resistor""},
            {""id"": ""comp3"", ""type"": ""switch""}
        ]
    }";

    GenerateCircuit(testJson);
}
public void GenerateCircuit(string jsonData)
{
    Debug.Log($"Attempting to parse JSON: {jsonData}");
    currentCornerIndex = 0;
    
    // Clear existing components
    foreach (var component in componentInstances.Values)
    {
        if (component != null)
            Destroy(component);
    }
    componentInstances.Clear();
    
    CircuitData circuitData = JsonUtility.FromJson<CircuitData>(jsonData);
    
    if (circuitData == null || circuitData.components == null)
    {
        Debug.LogError("Failed to parse circuit data!");
        return;
    }
    
    Debug.Log($"Successfully parsed {circuitData.components.Length} components");
    
    // First pass: Create all components at corner positions
    for (int i = 0; i < circuitData.components.Length; i++)
    {
        Component component = circuitData.components[i];
        if (i >= cornerPositions.Length)
        {
            Debug.LogError("Too many components for available positions!");
            break;
        }

        GameObject prefab = GetPrefabForType(component.type);
        if (prefab == null)
        {
            Debug.LogError($"No prefab found for component type: {component.type}");
            continue;
        }
        
        GameObject instance = Instantiate(prefab, transform.position + cornerPositions[i], Quaternion.identity, transform);
        instance.name = component.id;
        componentInstances.Add(component.id, instance);
        Debug.Log($"Created component {component.id} of type {component.type} at corner {i}");
    }

    // Create closed loop connections
    int componentCount = circuitData.components.Length;
    for (int i = 0; i < componentCount; i++)
    {
        GameObject currentComponent = componentInstances[circuitData.components[i].id];
        GameObject nextComponent = componentInstances[circuitData.components[(i + 1) % componentCount].id];
        
        CreateWireConnection(currentComponent, nextComponent);
    }
}
private void CreateWireConnection(GameObject from, GameObject to)
{
    Debug.Log($"Attempting to create wire from {from.name} to {to.name}");
    
    if (wireManager == null)
    {
        Debug.LogError("Wire Manager is not assigned!");
        return;
    }

     // Get all possible connection points
    Transform fromEnd1 = from.transform.Find("End1");
    Transform fromEnd2 = from.transform.Find("End2");
    Transform toEnd1 = to.transform.Find("End1");
    Transform toEnd2 = to.transform.Find("End2");

    if (fromEnd1 == null || fromEnd2 == null || toEnd1 == null || toEnd2 == null)
    {
        Debug.LogWarning($"Connection points not found for {from.name} or {to.name}");
        return;
    }

    // Calculate distances between all possible combinations
    float dist11 = Vector3.Distance(fromEnd1.position, toEnd1.position);
    float dist12 = Vector3.Distance(fromEnd1.position, toEnd2.position);
    float dist21 = Vector3.Distance(fromEnd2.position, toEnd1.position);
    float dist22 = Vector3.Distance(fromEnd2.position, toEnd2.position);

    // Find the shortest connection
    Transform selectedFromEnd;
    Transform selectedToEnd;
    
    if (dist11 <= dist12 && dist11 <= dist21 && dist11 <= dist22)
    {
        selectedFromEnd = fromEnd1;
        selectedToEnd = toEnd1;
    }
    else if (dist12 <= dist21 && dist12 <= dist22)
    {
        selectedFromEnd = fromEnd1;
        selectedToEnd = toEnd2;
    }
    else if (dist21 <= dist22)
    {
        selectedFromEnd = fromEnd2;
        selectedToEnd = toEnd1;
    }
    else
    {
        selectedFromEnd = fromEnd2;
        selectedToEnd = toEnd2;
    }

    // Create wire using WireManager with the closest endpoints
    WireManager.WireEndpoints wire = wireManager.CreateWireWithEndpoints(
        selectedFromEnd.position,
        selectedToEnd.position
    );

    // Parent the wire endpoints to the selected connection points
    wire.startPointObject.transform.SetParent(selectedFromEnd);
    wire.endPointObject.transform.SetParent(selectedToEnd);

    Debug.Log($"Successfully created wire from {from.name} to {to.name}");
}

private GameObject GetPrefabForType(string type)
{
    return type switch
    {
        "battery" => batteryPrefab,
        "resistor" => resistorPrefab,
        "led" => ledPrefab,
        "switch" => switchPrefab,  // Add switch case
        _ => null
    };
}

    private Vector3 GetRandomPosition()
    {
        // Generate a random position within a reasonable range
        return new Vector3(
            Random.Range(-5f, 5f),
            Random.Range(-5f, 5f),
            Random.Range(-5f, 5f)
        );
    }


}

