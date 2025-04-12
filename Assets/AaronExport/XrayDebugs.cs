using UnityEngine;

public class XrayDebugs : MonoBehaviour
{
    // Made private as adjusted them in the inspector caused it to not appear, they can be adjusted in the AssignDefaultColorsIfNeeded method
    private Color dpDpiDebugColor = Color.red;
    private Color lmDebugColor = Color.green;
    private Color dlpmoDebugColor = Color.blue;
    private Color dmploDebugColor = Color.yellow;

    // Debug mode flag
    public bool debugMode = false;
    private bool debugModeActive = false;

    // Range for ray lengths
    public float range = 0.15f;

    public Limb limb;

    // Initialize the script
    public void Start()
    {
        // Ensure colors are set, defaults provided at initialization
        AssignDefaultColorsIfNeeded();
    }

    // Update is called once per frame
    public void Update()
    {
        HandleDebugModeState();
        if (debugModeActive)
        {
            StartDebugMode(); // Draw debug rays every frame
        }
    }

    // Ensure default colors are assigned if not set
    private void AssignDefaultColorsIfNeeded()
    {
        dpDpiDebugColor = dpDpiDebugColor == default ? Color.red : dpDpiDebugColor; // DP/DPI color
        lmDebugColor = lmDebugColor == default ? Color.green : lmDebugColor; // LM color
        dlpmoDebugColor = dlpmoDebugColor == default ? Color.blue : dlpmoDebugColor; // DLPMO color
        dmploDebugColor = dmploDebugColor == default ? Color.yellow : dmploDebugColor; // DMPLO color
    }

    // Handle entering and exiting debug mode
    private void HandleDebugModeState()
    {
        if (debugMode && !debugModeActive)
        {
            debugModeActive = true;
            Debug.Log("Debug mode is active");
        }
        else if (!debugMode && debugModeActive)
        {
            debugModeActive = false;
            Debug.Log("Debug mode is inactive");
        }
    }

    // Draw debug rays based on current settings
    private void StartDebugMode()
    {
        Debug.Log("Drawing debug lines");

        DPDebug();

        // Draw LM side-to-side rays at 0 degrees
        DrawSideToSideRays(lmDebugColor);

        DLPMODebug();

        DMPLODebug();
    }

    private void DPDebug()
    {
        // 10° downward pitch around local right axis
        Quaternion pitchDown = Quaternion.AngleAxis(-10f, transform.right);

        // Apply pitch to forward and backward directions
        Vector3 pitchedDirection = pitchDown * transform.forward;

        // Long incoming ray from the front (7.5x range)
        Vector3 startFront = transform.position - pitchedDirection * range;
        Debug.DrawLine(startFront, transform.position, dpDpiDebugColor);

        // Short outgoing ray to the back (1x range)
        Vector3 endBack = transform.position + pitchedDirection * (range * 7.5f);
        Debug.DrawLine(transform.position, endBack, dpDpiDebugColor);
    }




    private void DMPLODebug()
    {
        if (limb == Limb.L_Front || limb == Limb.L_Hind)
        {
            Vector3 dmploForward = Quaternion.AngleAxis(10, -transform.right) *
                                   (Quaternion.AngleAxis(-45, Vector3.up) * transform.forward); // Front up
            Vector3 dmploBackward = Quaternion.AngleAxis(-10, transform.right) *
                                    (Quaternion.AngleAxis(-45, Vector3.up) * -transform.forward); // Back down

            Debug.DrawLine(transform.position, transform.position + dmploForward * (range * 7.5f),
                dmploDebugColor); // Incoming ray (front, up 10°)
            Debug.DrawLine(transform.position, transform.position + dmploBackward * range,
                dmploDebugColor); // Outgoing ray (back, down 10°)
        }
        else if (limb == Limb.R_Front || limb == Limb.R_Hind)
        {
            Vector3 dmploForward = Quaternion.AngleAxis(-10, transform.right) *
                                   (Quaternion.AngleAxis(-45, Vector3.up) * -transform.forward); // Front up
            Vector3 dmploBackward = Quaternion.AngleAxis(10, -transform.right) *
                                    (Quaternion.AngleAxis(-45, Vector3.up) * transform.forward); // Back down

            Debug.DrawLine(transform.position, transform.position + dmploForward * range,
                dmploDebugColor); // Incoming ray (front, up 10°)
            Debug.DrawLine(transform.position, transform.position + dmploBackward * (range * 7.5f),
                dmploDebugColor); // Outgoing ray (back, down 10°)
        }
        else
        {
            Debug.LogWarning("Invalid limb type for DMPLO rays");
        }
    }

    private void DLPMODebug()
    {
        if (limb == Limb.L_Front || limb == Limb.L_Hind)
        {
            Vector3 dlpmoForward = Quaternion.AngleAxis(10, -transform.right) * (Quaternion.AngleAxis(45, Vector3.up) * transform.forward); // Front up
            Vector3 dlpmoBackward = Quaternion.AngleAxis(-10, transform.right) * (Quaternion.AngleAxis(45, Vector3.up) * -transform.forward); // Back down

            Debug.DrawLine(transform.position, transform.position + dlpmoForward * (range * 7.5f), dlpmoDebugColor);   // Incoming ray (front, up 10°)
            Debug.DrawLine(transform.position, transform.position + dlpmoBackward * range, dlpmoDebugColor);  // Outgoing ray (back, down 10°)
        }
        else if (limb == Limb.R_Front || limb == Limb.R_Hind)
        {
            Vector3 dlpmoForward = Quaternion.AngleAxis(-10, transform.right) * (Quaternion.AngleAxis(45, Vector3.up) * -transform.forward); // Front up
            Vector3 dlpmoBackward = Quaternion.AngleAxis(10, -transform.right) * (Quaternion.AngleAxis(45, Vector3.up) * transform.forward); // Back down

            Debug.DrawLine(transform.position, transform.position + dlpmoForward * range, dlpmoDebugColor);   // Incoming ray (front, up 10°)
            Debug.DrawLine(transform.position, transform.position + dlpmoBackward * (range * 7.5f), dlpmoDebugColor);  // Outgoing ray (back, down 10°)
        }
        else
        {
            Debug.LogWarning("Invalid limb type for DLPMO rays");
        }
        // DLPMO: 45° horizontal, ±10° vertical
        
        Debug.LogWarning("DLPMO Debug drawn with ±10° vertical angles");
    }

    // Draw left and right side-to-side rays used for LM Debug
    private void DrawSideToSideRays(Color color)
    {
        Vector3 leftDirection = -transform.right;
        Vector3 rightDirection = transform.right;

        // Outside lines should be double the length of the inside lines
        if (limb == Limb.L_Front || limb == Limb.L_Hind)
        {
            Debug.DrawLine(transform.position, transform.position + leftDirection * (range * 7.5f), color); // Left side
            Debug.DrawLine(transform.position, transform.position + rightDirection * range, color); // Right side
        }
        else if (limb == Limb.R_Front || limb == Limb.R_Hind)
        {
            Debug.DrawLine(transform.position, transform.position + leftDirection * range, color); // Left side
            Debug.DrawLine(transform.position, transform.position + rightDirection * (range * 7.5f), color); // Right side
        }
        else
        {
            Debug.LogWarning("Invalid limb type for side-to-side rays");
        }
    }
    
    private void DrawDiagonalRays(Color color)
    {
        Vector3 leftDirection = Quaternion.AngleAxis(-45, transform.right) * -transform.forward;
        Vector3 rightDirection = Quaternion.AngleAxis(45, transform.right) * transform.forward;

        Debug.DrawLine(transform.position, transform.position + leftDirection * range, color); // Left side
        Debug.DrawLine(transform.position, transform.position + rightDirection * range, color); // Right side
    }
}

public enum Limb
{
    L_Front,
    R_Front,
    L_Hind,
    R_Hind,
}
