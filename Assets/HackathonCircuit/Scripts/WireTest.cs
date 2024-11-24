using UnityEngine;


public class WireTest : MonoBehaviour
{
    public WireManager wireManager;

    void Start()
    {
        // Create a wire with endpoints
        WireManager.WireEndpoints wire = wireManager.CreateWireWithEndpoints(
            new Vector3(0, 0, 0),    // Start position
            new Vector3(2, 0, 0)     // End position
        );

        // Now you can move the endpoints:
        wire.startPointObject.transform.position = new Vector3(-0.3f, 0.8f, 0.6f);
        wire.endPointObject.transform.position = new Vector3(0.1f, 0.8f, 0.6f);

        // Or delete the wire when needed:
        // wireManager.DeleteWire(wire);
    }
}