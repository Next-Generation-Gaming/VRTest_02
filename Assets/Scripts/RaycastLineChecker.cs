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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Awake()
    {
        xrayType = ScenarioDataLoader.Instance.scenarioData.xrayType;
        Debug.Log("X-ray type: " + xrayType);
        switch (xrayType)
        {
            case XrayType.DP_DPI:
                DPRaycastStart();
                isRaycastLineActive = true;
                break;
            case XrayType.DMPLO:
                DMPLORaycastStart();
                isRaycastLineActive = true;
                break;
            case XrayType.DLPMO:
                DLPMORaycastStart();
                isRaycastLineActive = true;
                break;
            case XrayType.LM:
                LMRaycastStart();
                isRaycastLineActive = true;
                break;
            default:
                Debug.LogError("Invalid X-ray type");
                break;
        }
        
        Activate(xrayType);
    }

    public void Activate(XrayType xrayType)
    {
        this.xrayType = xrayType;

        // Set the raycast line active
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

    // Update is called once per frame
    void Update()
    {
        if (isRaycastLineActive)
        {
            Vector3 direction = (backVector - frontVector).normalized;
            float distance = Vector3.Distance(frontVector, backVector);
            RaycastHit[] hits = Physics.RaycastAll(frontVector, direction, distance);

            if (isGuidedMode && !isLineActive)
            {
                CreateVisibleLine();
            }

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider != null)
                {
                    Debug.Log("Hit: " + hit.collider.name);
                }
            }
        }

    }

    private void CreateVisibleLine()
    {
        // Create a line renderer to visualize the raycast
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;

        // Set the positions of the line renderer
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, frontVector);
        lineRenderer.SetPosition(1, backVector);
        isLineActive = true;
    }

    private void DPRaycastStart()
    {
        // 10° downward pitch around local right axis
        Quaternion pitchDown = Quaternion.AngleAxis(-10f, transform.right);

        // Apply pitch to forward and backward directions
        Vector3 pitchedDirection = pitchDown * transform.forward;

        // Long incoming ray from the front (7.5x range)
       frontVector = transform.position + pitchedDirection * (0.15f * 7.5f);

        // Short outgoing ray to the back (1x range)
        backVector = transform.position - pitchedDirection * (0.25f);
    }

    private void DMPLORaycastStart()
    {
        Vector3 frontDirection;
        Vector3 backDirection;

        if (isLeft)
        {
            backDirection = Quaternion.AngleAxis(10, -transform.right) *
                             (Quaternion.AngleAxis(-45, Vector3.up) * transform.forward);
            frontDirection = Quaternion.AngleAxis(-10, transform.right) *
                            (Quaternion.AngleAxis(-45, Vector3.up) * -transform.forward);
        }
        else
        {
            backDirection = Quaternion.AngleAxis(-10, transform.right) *
                            (Quaternion.AngleAxis(-45, Vector3.up) * -transform.forward);
            frontDirection = Quaternion.AngleAxis(10, -transform.right) *
                             (Quaternion.AngleAxis(-45, Vector3.up) * transform.forward);
        }

        frontVector = transform.position + frontDirection.normalized * (0.15f * 7.5f);
        backVector = transform.position + backDirection.normalized * (0.25f);
    }


    private void DLPMORaycastStart()
    {
        Vector3 frontDirection;
        Vector3 backDirection;

        if (isLeft)
        {
            backDirection = Quaternion.AngleAxis(10, -transform.right) *
                             (Quaternion.AngleAxis(45, Vector3.up) * transform.forward);  // Front up
            frontDirection = Quaternion.AngleAxis(-10, transform.right) *
                            (Quaternion.AngleAxis(45, Vector3.up) * -transform.forward);  // Back down
        }
        else
        {
            backDirection = Quaternion.AngleAxis(-10, transform.right) *
                             (Quaternion.AngleAxis(45, Vector3.up) * -transform.forward); // Front up
            frontDirection = Quaternion.AngleAxis(10, -transform.right) *
                            (Quaternion.AngleAxis(45, Vector3.up) * transform.forward);   // Back down
        }

        frontVector = transform.position + frontDirection.normalized * (0.15f * 7.5f);
        backVector = transform.position + backDirection.normalized * 0.25f;
    }


    private void LMRaycastStart()
    {
        Vector3 leftDirection = -transform.right;
        Vector3 rightDirection = transform.right;
        
        // Outside lines should be longer
        
        if (isLeft)
        {
            frontVector = transform.position + leftDirection * (0.15f * 7.5f);
            backVector = transform.position - leftDirection * (0.25f);
        }
        else
        {
            frontVector = transform.position + rightDirection * (0.15f * 7.5f);
            backVector = transform.position - rightDirection * (0.25f);
        }
    }
}
