using UnityEngine;

public class WireController : MonoBehaviour
{
    private Transform startPoint;
    private Transform endPoint;
    private GameObject[] segments;
    private float wireWidth;
    private float curvature;
    private float stretchFactor;
    private Vector3 lastStartPos;
    private Vector3 lastEndPos;
    private float velocityDamping = 0.1f;
    private Vector3 controlPointVelocity;

    public void Initialize(GameObject[] wireSegments, float width, float curveAmount, float stretching)
    {
        segments = wireSegments;
        wireWidth = width;
        curvature = curveAmount;
        stretchFactor = stretching;
        controlPointVelocity = Vector3.zero;
    }

    public void SetEndpoints(Transform start, Transform end)
    {
        startPoint = start;
        endPoint = end;
        lastStartPos = start.position;
        lastEndPos = end.position;
        UpdateWire();
    }

    private void Update()
    {
        if (startPoint != null && endPoint != null)
        {
            UpdateWire();
            lastStartPos = startPoint.position;
            lastEndPos = endPoint.position;
        }
    }
private void UpdateWire()
{
    if (segments == null || segments.Length == 0) return;

    Vector3 startPos = startPoint.position;
    Vector3 endPos = endPoint.position;
    float distance = Vector3.Distance(startPos, endPos);

    // Calculate middle point and add some physics-based movement
    Vector3 midPoint = (startPos + endPos) / 2f;
    Vector3 movement = ((startPos - lastStartPos) + (endPos - lastEndPos)) * 0.5f;
    
    // Calculate control point with physics-based adjustment
    float sag = distance * stretchFactor;
    Vector3 baseControlPoint = midPoint + (Vector3.up * -sag);
    Vector3 targetControlPoint = baseControlPoint + (movement * distance);
    
    // Smooth the control point movement
    controlPointVelocity = Vector3.Lerp(controlPointVelocity, 
        (targetControlPoint - baseControlPoint) * 2f, velocityDamping);
    Vector3 controlPoint = baseControlPoint + controlPointVelocity;

    for (int i = 0; i < segments.Length; i++)
    {
        float t = i / (float)(segments.Length - 1);
        
        // Calculate current and next positions
        Vector3 position = CalculateBezierPoint(t, startPos, controlPoint, endPos);
        Vector3 nextPosition = CalculateBezierPoint(
            Mathf.Min(1f, (i + 1) / (float)(segments.Length - 1)),
            startPos, controlPoint, endPos
        );

        // Calculate segment direction and length
        Vector3 direction = (nextPosition - position).normalized;
        float segmentLength = Vector3.Distance(position, nextPosition);

        // Set segment transform
        segments[i].transform.up = direction;
        segments[i].transform.localScale = new Vector3(wireWidth, segmentLength * 0.5f, wireWidth);
        
        // Position the segment
        if (i == 0)
        {
            // First segment starts at startPos
            segments[i].transform.position = startPos + (direction * (segmentLength * 0.25f));
        }
        else if (i == segments.Length - 1)
        {
            // Last segment ends at endPos
            segments[i].transform.position = endPos - (direction * (segmentLength * 0.25f));
        }
        else
        {
            // Middle segments
            segments[i].transform.position = position;
        }
    }
}
    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        
        return (uu * p0) + (2 * u * t * p1) + (tt * p2);
    }
}