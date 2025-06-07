using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(LineRenderer))]
public class ActivityManager : MonoBehaviour
{
    // Activity name for logging
    private string activityName = "Activity Manager";

    [Header("References to equipment")]
    public XRGrabInteractable plate;
    public XRGrabInteractable emitter;

    [Header("Plate Detection & Alignment")]
    [Tooltip("Marker detection radius for plate (meters)")]
    public float plateDetectionRadius = 0.2f;
    [Tooltip("Max angular error (deg) for plate vs. any allowed marker normal")]
    public float plateAngleThreshold = 5f;

    [Header("Emitter Detection & Alignment")]
    [Tooltip("Min allowed emitter distance from marker center (meters)")]
    public float emitterMinDistance = 0.3f;
    [Tooltip("Max allowed emitter distance from marker center (meters)")]
    public float emitterMaxDistance = 0.6f;
    [Tooltip("Max angular error (deg) for emitter vs. marker normal")]
    public float emitterAngleThreshold = 5f;
    [Tooltip("Max angular error (deg) between emitter and plate forward")]
    public float emitterPlateAngleThreshold = 5f;

    [Header("Input Manager Reference")]
    [Tooltip("Drag the GameObject that has InputScript attached here.")]
    public InputScript inputScript;

    // Internal state
    private List<Marker> _markers;
    private Marker _currentMarker;

    // Renderers for visual feedback
    private MeshRenderer _plateRenderer;
    private MeshRenderer _emitterRenderer;
    private Color _plateOrigColor;
    private Color _emitterOrigColor;

    // LineRenderers for visible debug lines
    private LineRenderer _plateLine;
    private LineRenderer _emitterLine;

    // Controls whether any debug coloring or lines are shown
    private bool _debugEnabled = true;

    private void Awake()
    {
        // Cache MeshRenderers & original colors
        if (plate != null)
        {
            _plateRenderer = plate.GetComponent<MeshRenderer>();
            if (_plateRenderer != null)
                _plateOrigColor = _plateRenderer.material.color;
        }

        if (emitter != null)
        {
            _emitterRenderer = emitter.GetComponent<MeshRenderer>();
            if (_emitterRenderer != null)
                _emitterOrigColor = _emitterRenderer.material.color;
        }

        // Set up debug LineRenderers
        _plateLine = new GameObject("PlateDebugLine").AddComponent<LineRenderer>();
        _plateLine.transform.SetParent(transform);
        ConfigureLineRenderer(_plateLine);

        _emitterLine = new GameObject("EmitterDebugLine").AddComponent<LineRenderer>();
        _emitterLine.transform.SetParent(transform);
        ConfigureLineRenderer(_emitterLine);
    }

    private void Start()
    {
        Debug.Log($"[{activityName}] Starting setup...");

        if (plate == null)
            Debug.LogError($"[{activityName}] Plate reference is missing in Inspector!");
        else
            Debug.Log($"[{activityName}] Plate assigned: {plate.name}");

        if (emitter == null)
            Debug.LogError($"[{activityName}] Emitter reference is missing in Inspector!");
        else
            Debug.Log($"[{activityName}] Emitter assigned: {emitter.name}");

        _markers = FindObjectsOfType<Marker>().ToList();
        Debug.Log($"[{activityName}] Found {_markers.Count} markers in scene.");

        // Subscribe to input events
        if (inputScript == null)
        {
            Debug.LogError($"[{activityName}] No InputScript assigned! Drag the InputScript GameObject into the inspector.");
        }
        else
        {
            inputScript.OnAButtonPressed += PerformAlignmentCheck;
            inputScript.OnXButtonPressed += ToggleAllDebug;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid null-reference or memory leaks
        if (inputScript != null)
        {
            inputScript.OnAButtonPressed -= PerformAlignmentCheck;
            inputScript.OnXButtonPressed -= ToggleAllDebug;
        }
    }

    private void Update()
    {
        // Intentionally left empty: alignment and debug toggling are driven by input events.
    }

    private void PerformAlignmentCheck()
    {
        Debug.Log($"[{activityName}] A-button pressed → running alignment check (debug {( _debugEnabled ? "ON" : "OFF" )})...");

        if (plate == null || emitter == null || _markers == null)
        {
            Debug.LogError($"[{activityName}] Missing reference: plate, emitter, or markers list is null.");
            return;
        }

        // 1) Find nearest marker within plateDetectionRadius
        Vector3 pPos = plate.transform.position;
        Marker nearest = _markers
            .Where(m => Vector3.Distance(m.transform.position, pPos) <= plateDetectionRadius)
            .OrderBy(m => Vector3.Distance(m.transform.position, pPos))
            .FirstOrDefault();

        if (nearest != _currentMarker)
        {
            _currentMarker = nearest;
            DisableLines(); // hide old lines when marker changes

            if (_currentMarker != null)
                Debug.Log($"[{activityName}] Plate entered range of marker: {_currentMarker.name}");
            else
                Debug.Log($"[{activityName}] Plate left all marker ranges");
        }

        if (_currentMarker == null)
        {
            ResetPlateVisual();
            ResetEmitterVisual();
            Debug.Log($"[{activityName}] No marker in range → overall: NOT aligned");
            return;
        }

        // 2) Check plate vs. any allowed marker normal
        Vector3 bestMarkerNormal = Vector3.zero;
        float bestPlateError = float.MaxValue;

        // Compute each candidate normal from allowedAngles
        foreach (float angle in _currentMarker.allowedAngles)
        {
            // Rotate forwardDirection by 'angle' around marker's up axis
            Quaternion rot = Quaternion.AngleAxis(angle, _currentMarker.transform.up);
            Vector3 candidateLocal = rot * _currentMarker.forwardDirection.normalized;
            Vector3 candidateWorld = _currentMarker.transform.TransformDirection(candidateLocal).normalized;

            float error = Vector3.Angle(plate.transform.forward, candidateWorld);
            if (error < bestPlateError)
            {
                bestPlateError = error;
                bestMarkerNormal = candidateWorld;
            }
        }

        bool plateAligned = bestPlateError <= plateAngleThreshold;
        if (_debugEnabled)
        {
            UpdatePlateColor(plateAligned);
            LogAndDraw(_plateLine, pPos, _currentMarker.transform.position, plateAligned);
        }
        else
        {
            ResetPlateVisual();
        }

        Debug.Log($"[{activityName}] Plate vs Marker best‐angle error: {bestPlateError:F1}° (Aligned: {plateAligned})");

        if (!plateAligned)
        {
            ResetEmitterVisual();
            Debug.Log($"[{activityName}] Overall: NOT aligned (plate failed)");
            return;
        }

        // 3) Since plate is aligned, check emitter vs. marker & plate
        bool emitterOk = CheckEmitterAlignment(bestMarkerNormal);
        if (_debugEnabled)
        {
            UpdateEmitterColor(emitterOk);

            Vector3 ePos = emitter.transform.position;
            // Draw emitter up‐vector ray
            LogAndDraw(_emitterLine, ePos, ePos + emitter.transform.up * 0.5f, emitterOk);
        }
        else
        {
            ResetEmitterVisual();
        }

        Debug.Log($"[{activityName}] Emitter alignment check: {emitterOk}");

        // 4) Final summary
        if (emitterOk)
            Debug.Log($"[{activityName}] Overall: FULLY aligned (plate & emitter OK)");
        else
            Debug.Log($"[{activityName}] Overall: NOT aligned (emitter failed)");
    }

    private void ToggleAllDebug()
    {
        _debugEnabled = !_debugEnabled;

        // Toggle the marker gizmos as well:
        if (_markers != null && _markers.Count > 0)
        {
            foreach (var m in _markers)
                m.showAngleGizmos = _debugEnabled;
        }

        // If turning debug off, immediately clear any colors/lines:
        if (!_debugEnabled)
        {
            ResetPlateVisual();
            ResetEmitterVisual();
            DisableLines();
        }

        Debug.Log($"[{activityName}] Toggled ALL debug visuals → now: {(_debugEnabled ? "ON" : "OFF")}");
    }

    private bool CheckEmitterAlignment(Vector3 markerNormal)
    {
        Vector3 ePos = emitter.transform.position;
        float dist = Vector3.Distance(ePos, _currentMarker.transform.position);
        if (dist < emitterMinDistance || dist > emitterMaxDistance)
        {
            Debug.Log($"[{activityName}] Emitter dist {dist:F2}m out of range [{emitterMinDistance}-{emitterMaxDistance}]");
            return false;
        }

        // Emitter up vs. –markerNormal
        float normErr = Vector3.Angle(emitter.transform.up, -markerNormal);
        if (normErr > emitterAngleThreshold)
        {
            Debug.Log($"[{activityName}] Emitter vs Marker normal error: {normErr:F1}°");
            return false;
        }

        // Emitter up vs. –plate.forward
        float plateErr = Vector3.Angle(emitter.transform.up, -plate.transform.forward);
        if (plateErr > emitterPlateAngleThreshold)
        {
            Debug.Log($"[{activityName}] Emitter vs Plate forward error: {plateErr:F1}°");
            return false;
        }

        return true;
    }

    private void ConfigureLineRenderer(LineRenderer lr)
    {
        lr.positionCount = 2;
        lr.startWidth = 0.005f;
        lr.endWidth = 0.005f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.enabled = false;
    }

    private void LogAndDraw(LineRenderer lr, Vector3 start, Vector3 end, bool ok)
    {
        // If debug is off, just disable
        if (!_debugEnabled)
        {
            lr.enabled = false;
            return;
        }

        lr.startColor = lr.endColor = ok ? Color.green : Color.red;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.enabled = true;
    }

    private void UpdatePlateColor(bool ok)
    {
        if (_plateRenderer != null)
            _plateRenderer.material.color = ok ? Color.green : Color.red;
    }

    private void ResetPlateVisual()
    {
        if (_plateRenderer != null)
            _plateRenderer.material.color = _plateOrigColor;
        _plateLine.enabled = false;
    }

    private void UpdateEmitterColor(bool ok)
    {
        if (_emitterRenderer != null)
            _emitterRenderer.material.color = ok ? Color.green : Color.red;
    }

    private void ResetEmitterVisual()
    {
        if (_emitterRenderer != null)
            _emitterRenderer.material.color = _emitterOrigColor;
        _emitterLine.enabled = false;
    }

    private void DisableLines()
    {
        _plateLine.enabled = false;
        _emitterLine.enabled = false;
    }
}
