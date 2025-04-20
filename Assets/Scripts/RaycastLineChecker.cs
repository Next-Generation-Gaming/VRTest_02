using UnityEngine;

public class RaycastLineChecker : MonoBehaviour
{
    public bool isGuidedMode = false;
    public bool isRaycastLineActive = false;
    public bool isLeft = false;
    private XrayType xrayType = XrayType.None;
    private Vector3 frontVector = Vector3.zero;
    private Vector3 backVector = Vector3.zero;
    private bool isLineActive;
    private GameObject line1;
    private GameObject line2;
    private bool isLine1Active;

    public float ObjectRotationRange;
    private Quaternion minimumRotation;
    private Quaternion maximumRotation;

    public float minimumEmitterDistance;
    public float maximumEmitterDistance;

    public void Awake()
    {
        isLeft = gameObject.name.StartsWith("L") || gameObject.name.StartsWith("l");

        xrayType = ScenarioDataLoader.Instance.scenarioData.xrayType;
        Debug.Log("X-ray type: " + xrayType);

        switch (xrayType)
        {
            case XrayType.DP_DPI:
                DPRaycastStart();
                minimumRotation = Quaternion.Euler(-85, -10, -180);
                maximumRotation = Quaternion.Euler(-75, 10, -170);
                break;
            case XrayType.DMPLO:
                DMPLOCorrectedRaycastStart();
                break;
            case XrayType.DLPMO:
                DLPMORaycastStart();
                break;
            case XrayType.LM:
                LMRaycastStart();
                break;
            default:
                Debug.LogError("Invalid X-ray type");
                break;
        }

        isRaycastLineActive = true;
        Activate(xrayType);
    }

    public void Activate(XrayType xrayType)
    {
        this.xrayType = xrayType;
        isRaycastLineActive = true;
    }

    public void Deactivate()
    {
        xrayType = XrayType.None;
        isRaycastLineActive = false;
    }

    public void SetLeft(bool left)
    {
        isLeft = left;
    }

    void Update()
    {
        if (!isRaycastLineActive || frontVector == Vector3.zero || backVector == Vector3.zero) return;

        Vector3 forwardDirection = (frontVector - transform.position).normalized;
        Vector3 backwardDirection = (backVector - transform.position).normalized;

        float frontDistance = Vector3.Distance(transform.position, frontVector);
        float backDistance = Vector3.Distance(transform.position, backVector);

        Ray frontRay = new Ray(transform.position, forwardDirection);
        Ray backRay = new Ray(transform.position, backwardDirection);

        Debug.DrawRay(frontRay.origin, frontRay.direction * frontDistance, Color.blue);
        Debug.DrawRay(backRay.origin, backRay.direction * backDistance, Color.blue);

        if (isGuidedMode && !isLineActive)
        {
            CreateVisibleLine(transform.position + forwardDirection * frontDistance);
            CreateVisibleLine(transform.position + backwardDirection * backDistance);
        }

        UpdateRayLine(frontRay, "Emitter", line1, frontDistance);
        UpdateRayLine(backRay, "Plate", line2, backDistance);
    }


    private void UpdateRayLine(Ray ray, string expectedTag, GameObject lineObject, float distance)
    {
        if (lineObject == null) return;

        Color targetColor = Color.red;

        if (Physics.Raycast(ray, out RaycastHit hit, distance))
        {
            if (hit.collider.CompareTag(expectedTag))
            {
                targetColor = PositionAndRotationCheck(hit);
            }
            else
            {
                Debug.LogWarning($"Hit object with tag '{hit.collider.tag}', expected '{expectedTag}'");
            }
        }

        ChangeLineColour(lineObject.GetComponent<LineRenderer>(), targetColor);
    }

    private Color PositionAndRotationCheck(RaycastHit hit)
    {
        // Distance Check
        float distanceToHit = Vector3.Distance(transform.position, hit.point);
        if (distanceToHit < minimumEmitterDistance || distanceToHit > maximumEmitterDistance)
            return Color.red;

        // Rotation Check: Compare surface normal to object's up direction
        float angle = Vector3.Angle(hit.normal, transform.up);
        if (angle <= ObjectRotationRange)
            return Color.green;

        return Color.red;
    }



    private void CreateVisibleLine(Vector3 direction)
    {
        GameObject lineObject = new GameObject(isLine1Active ? "Line2" : "Line1");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, direction);

        if (!isLine1Active)
        {
            line1 = lineObject;
            isLine1Active = true;
        }
        else
        {
            line2 = lineObject;
        }

        isLineActive = true;
    }

    private void ChangeLineColour(LineRenderer lineRenderer, Color color)
    {
        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
    }

    private void DPRaycastStart()
    {
        Quaternion pitchDown = Quaternion.AngleAxis(-10f, transform.right);
        Vector3 pitchedDirection = pitchDown * transform.forward;
        frontVector = transform.position + pitchedDirection * (0.15f * 7.5f);
        backVector = transform.position - pitchedDirection * (0.25f);
    }

    private void DMPLOCorrectedRaycastStart()
    {
        Vector3 frontDirection;
        Vector3 backDirection;

        if (isLeft)
        {
            frontDirection = Quaternion.AngleAxis(10, -transform.right) *
                             (Quaternion.AngleAxis(-45, Vector3.up) * transform.forward);
            backDirection = Quaternion.AngleAxis(-10, transform.right) *
                            (Quaternion.AngleAxis(-45, Vector3.up) * -transform.forward);
        }
        else
        {
            frontDirection = Quaternion.AngleAxis(10, -transform.right) *
                             (Quaternion.AngleAxis(-45, Vector3.up) * transform.forward);
            backDirection = Quaternion.AngleAxis(-10, transform.right) *
                            (Quaternion.AngleAxis(-45, Vector3.up) * -transform.forward);
        }

        frontVector = transform.position + frontDirection.normalized * (0.15f * 7.5f);
        backVector = transform.position + backDirection.normalized * 0.25f;
    }

    private void DLPMORaycastStart()
    {
        Vector3 frontDirection;
        Vector3 backDirection;

        if (isLeft)
        {
            frontDirection = Quaternion.AngleAxis(10, -transform.right) *
                             (Quaternion.AngleAxis(45, Vector3.up) * transform.forward);
            backDirection = Quaternion.AngleAxis(-10, transform.right) *
                            (Quaternion.AngleAxis(45, Vector3.up) * -transform.forward);
        }
        else
        {
            frontDirection = Quaternion.AngleAxis(10, -transform.right) *
                             (Quaternion.AngleAxis(45, Vector3.up) * transform.forward);
            backDirection = Quaternion.AngleAxis(-10, transform.right) *
                            (Quaternion.AngleAxis(45, Vector3.up) * -transform.forward);
        }

        frontVector = transform.position + frontDirection.normalized * (0.15f * 7.5f);
        backVector = transform.position + backDirection.normalized * 0.25f;
    }

    private void LMRaycastStart()
    {
        Vector3 direction = isLeft ? -transform.right : transform.right;
        frontVector = transform.position + direction * (0.15f * 7.5f);
        backVector = transform.position - direction * 0.25f;
    }
}
