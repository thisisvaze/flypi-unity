using UnityEngine;

public class ToggleOnHandCollision : MonoBehaviour
{
    // Reference angles for toggling
    private readonly Quaternion closedRotation = Quaternion.Euler(0f, 0f, -40f);
    private readonly Quaternion openRotation = Quaternion.Euler(0f, 0f, 0f);
    public bool IsOpen => isOpen;
    
    // Track current state
    private bool isOpen = true;

    // Called when another collider enters this object's trigger
    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is an index finger
        if (other.gameObject.name.Contains("Index"))
        {
            // Toggle between rotations
            if (isOpen)
            {
                transform.rotation = closedRotation;
            }
            else
            {
                transform.rotation = openRotation;
            }
            isOpen = !isOpen;
        }
    }
}