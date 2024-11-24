using UnityEngine;

public class AttachSocket : MonoBehaviour
{
    public string wireEndTag = "WireEnd";
    private GameObject attachedWireEnd = null;
    public bool isOccupied => attachedWireEnd != null;

    // Optional: Reference to visual indicator for valid connection point
    [SerializeField] private GameObject connectionIndicator;
    
    // Optional: Events or delegates could be added here
    // public event System.Action<GameObject> OnWireConnected;
    // public event System.Action<GameObject> OnWireDisconnected;

    private void Start()
    {
        // Initialize any visual indicators
        if (connectionIndicator != null)
        {
            connectionIndicator.SetActive(false);
        }
    }

private void OnTriggerEnter(Collider other)
{
    // Check if socket is empty and the incoming object is a wire end
    if (!isOccupied && other.CompareTag(wireEndTag))
    {
        if (connectionIndicator != null)
        {
            connectionIndicator.SetActive(true);
        }
        AttachWireEnd(other.gameObject);
    }
}

private void AttachWireEnd(GameObject wireEnd)
{
    if (attachedWireEnd != null) return;

    attachedWireEnd = wireEnd;
    
    // Snap the wire end to the socket's position
    attachedWireEnd.transform.position = transform.position;
    
    // Disable physics if present
    Rigidbody wireRb = attachedWireEnd.GetComponent<Rigidbody>();
    if (wireRb != null)
    {
        wireRb.isKinematic = true;
    }

    // Make the wire end a child of the socket to maintain position
    attachedWireEnd.transform.SetParent(transform);
}

    private void OnTriggerExit(Collider other)
    {
        if (attachedWireEnd != null && other.CompareTag(wireEndTag))
        {
            DetachWireEnd();
        }

        // Hide visual indicator
        if (connectionIndicator != null)
        {
            connectionIndicator.SetActive(false);
        }
    }

    public void DetachWireEnd()
    {
        if (attachedWireEnd == null) return;

        // Re-enable physics if present
        Rigidbody wireRb = attachedWireEnd.GetComponent<Rigidbody>();
        if (wireRb != null)
        {
            wireRb.isKinematic = false;
        }
        
        // Optional: Notify any listeners
        // OnWireDisconnected?.Invoke(attachedWireEnd);
        
        attachedWireEnd = null;
    }

    // Public method to check if a specific wire end is attached to this socket
    public bool IsWireEndAttached(GameObject wireEnd)
    {
        return attachedWireEnd == wireEnd;
    }

    // Public method to get the currently attached wire end
    public GameObject GetAttachedWireEnd()
    {
        return attachedWireEnd;
    }

    // Optional: Method to force detachment (useful for cleanup or reset)
    public void ForceDetach()
    {
        DetachWireEnd();
        if (connectionIndicator != null)
        {
            connectionIndicator.SetActive(false);
        }
    }
}