using UnityEngine;

public class CircuitStateManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Generate3DCircuitModel circuitGenerator;
    
    private GameObject ledBulb;
    private GameObject circuitSwitch;
    private ToggleOnHandCollision switchToggle;
    private WireManager wireManager;

    private void Start()
    {
        // Get references
        wireManager = circuitGenerator.GetComponent<WireManager>();
        UpdateCircuitState();
    }

public void Initialize(GameObject led, GameObject switchObj)
{
    ledBulb = led;
    circuitSwitch = switchObj;
    
    // Only try to get the component if the switch exists
    if (switchObj != null)
    {
        switchToggle = switchObj.GetComponent<ToggleOnHandCollision>();
        if (switchToggle == null)
        {
            Debug.LogWarning("ToggleOnHandCollision component not found on switch object!");
        }
    }
}

private void UpdateCircuitState()
{
    if (ledBulb == null || circuitSwitch == null || switchToggle == null) return;

    bool isCircuitComplete = IsCircuitComplete();
    bool isSwitchClosed = !switchToggle.IsOpen;
    
    // Circuit is powered if it's complete AND switch is closed
    bool isPowered = isCircuitComplete && isSwitchClosed;
    
    // Update light material color
    Transform lightChild = ledBulb.transform.Find("light");
    if (lightChild != null)
    {
        Material lightMaterial = lightChild.GetComponent<MeshRenderer>().material;
        lightMaterial.SetColor("_BaseColor", isPowered ? Color.yellow : Color.black);
    }
}


    private void Update()
    {
        UpdateCircuitState();
    }


private bool IsCircuitComplete()
{
    // Check if all required wires are present and connected
        return wireManager.AreAllWiresConnected();
    }
}