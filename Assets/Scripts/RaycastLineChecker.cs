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
    
    private float acceptableAngleEmitter = 10f;
    private float acceptableAnglePlate;
    public float angleTolerance = 5f;

    public float minimumEmitterDistance;
    public float maximumEmitterDistance;
    public float minimumPlateDistance;
    public float maximumPlateDistance;

    public void Awake()
    {
        isLeft = gameObject.name.StartsWith("L") || gameObject.name.StartsWith("l");
        
        // if Scenario Data Loader is not found, get xray type from parent of parent in BodyPartSetup
        if (ScenarioDataLoader.Instance == null)
        {
            Debug.LogWarning("ScenarioDataLoader instance not found. Assuming xray type from Debug.");
            xrayType = gameObject.transform.parent.parent.GetComponent<BodyPartSetup>().debugXrayType;
        }
        else
        {
            xrayType = ScenarioDataLoader.Instance.scenarioData.xrayType;
        }
        Debug.Log("X-ray type: " + xrayType);

        switch (xrayType)
        {
            case XrayType.DP_DPI:
                DPRaycastStart(); // 180f is straight vertical , 170f is a slight upwards rotation
                acceptableAngleEmitter = 170f;
                acceptableAnglePlate = 180f; 
                // emitter angle is opposite to plate angle
                break;
            case XrayType.DMPLO:
                DMPLOCorrectedRaycastStart();
                acceptableAngleEmitter = 170f;
                acceptableAnglePlate = 180f; 
                break;
            case XrayType.DLPMO:
                DLPMORaycastStart();
                acceptableAngleEmitter = 170f;
                acceptableAnglePlate = 180f; 
                break;
            case XrayType.LM:
                LMRaycastStart();
                acceptableAngleEmitter = 180f;
                acceptableAnglePlate = 180f;
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
    
    private void CreateVisibleLine(Vector3 endPoint)
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
        lineRenderer.SetPosition(1, endPoint); // <-- using world position correctly

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



    private void UpdateRayLine(Ray ray, string expectedTag, GameObject lineObject, float distance)
    {
        if (lineObject == null) return;

        Color targetColor = Color.red;

        if (Physics.Raycast(ray, out RaycastHit hit, distance))
        {
            if (hit.collider.CompareTag(expectedTag))
            {
                Debug.Log("Hit " + expectedTag + ": " + hit.collider.name);
                targetColor = PositionAndRotationCheck(hit,expectedTag);
            }
        }

        ChangeLineColour(lineObject.GetComponent<LineRenderer>(), targetColor);
    }
    
    private Color PositionAndRotationCheck(RaycastHit hit, string expectedTag)
    {
        // Distance Check
        float distanceToHit = Vector3.Distance(transform.position, hit.point);
        Debug.Log("Distance to hit: " + distanceToHit);

        if (expectedTag == "Emitter")
        {
            //Emitter Rotation and Distance Check
            if (distanceToHit < minimumEmitterDistance || distanceToHit > maximumEmitterDistance)
                return Color.red;

            // Signed angle between forward and hit surface normal
            float signedAngle = Vector3.SignedAngle(transform.forward, -hit.normal, transform.right);
            Debug.Log("Signed Angle to surface: " + signedAngle);

            // Handle wraparound correctly
            if (Mathf.Abs(Mathf.DeltaAngle(signedAngle, acceptableAngleEmitter)) <= angleTolerance)
                return Color.green;

            return Color.red;
        }
        
        if (expectedTag == "Plate")
        {
            Debug.Log("Found Plate");
            if (distanceToHit < minimumPlateDistance || distanceToHit > maximumPlateDistance)
                return Color.red;

            // Signed angle between forward and hit surface normal
            float signedAngle = Vector3.SignedAngle(transform.forward, -hit.normal, transform.right);
            Debug.Log("Signed Angle to surface: " + signedAngle);

            // Handle wraparound correctly
            if (Mathf.Abs(Mathf.DeltaAngle(signedAngle, acceptableAnglePlate)) <= angleTolerance)
                return Color.green;

            return Color.red;
        }
        Debug.LogError("Invalid tag: " + expectedTag);
        return Color.red;
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
        frontVector = transform.position + pitchedDirection * 2f;
        backVector = transform.position - transform.forward.normalized * (0.25f);
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

        frontVector = transform.position + frontDirection.normalized * (2f);
        backVector = transform.position - transform.forward.normalized * (0.25f);
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

        frontVector = transform.position + frontDirection.normalized * (2f);
        backVector = transform.position - transform.forward.normalized * (0.25f);
    }

    private void LMRaycastStart()
    {
        Vector3 direction = isLeft ? -transform.right : transform.right;
        frontVector = transform.position + direction * (2f);
        backVector = transform.position - transform.forward.normalized * (0.25f);
    }
}
