using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(LineRenderer))]
public class ActivityManager : MonoBehaviour
{
    // Activity name for logging
    private String activityName = "Activity Manager";
    
    [Header("References to equipment")]
    public XRGrabInteractable plate;
    public XRGrabInteractable emitter;

    [Header("Plate Detection & Alignment")]
    [Tooltip("Marker detection radius for plate (meters)")]
    public float plateDetectionRadius = 0.2f;
    [Tooltip("Max angular error (deg) for plate vs. marker normal")]
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

    void Awake()
    {
        // cache renderers
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

        // Setup plate line renderer
        _plateLine = new GameObject("PlateDebugLine").AddComponent<LineRenderer>();
        _plateLine.transform.SetParent(transform);
        ConfigureLineRenderer(_plateLine);

        // Setup emitter line renderer
        _emitterLine = new GameObject("EmitterDebugLine").AddComponent<LineRenderer>();
        _emitterLine.transform.SetParent(transform);
        ConfigureLineRenderer(_emitterLine);
    }

    void Start()
    {
        Debug.Log($"[{activityName}] Starting setup...");
        if (plate == null)
            Debug.LogError($"[{activityName}] Plate reference is missing in Inspector!");
        else
            Debug.Log($"[{activityName}] Plate assigned: " + plate.name);

        if (emitter == null)
            Debug.LogError($"[{activityName}] Emitter reference is missing in Inspector!");
        else
            Debug.Log($"[{activityName}] Emitter assigned: " + emitter.name);

        _markers = FindObjectsOfType<Marker>().ToList();
        Debug.Log($"[{activityName}] Found {_markers.Count} markers in scene.");
    }

    void Update()
    {
        if (plate == null || emitter == null || _markers == null)
            return;

        // Plate-marker detection
        Vector3 pPos = plate.transform.position;
        var nearest = _markers.Where(m => Vector3.Distance(m.transform.position, pPos) <= plateDetectionRadius)
                               .OrderBy(m => Vector3.Distance(m.transform.position, pPos))
                               .FirstOrDefault();

        if (nearest != _currentMarker)
        {
            _currentMarker = nearest;
            _plateLine.enabled = false;
            _emitterLine.enabled = false;

            if (_currentMarker != null)
                Debug.Log($"[{activityName}] Plate entered range of marker: " + _currentMarker.name);
            else
                Debug.Log($"[{activityName}] Plate left all marker ranges");
        }

        if (_currentMarker == null)
        {
            ResetPlateColor(); ResetEmitterColor();
            return;
        }

        // Plate alignment
        Vector3 markerNormal = _currentMarker.transform.TransformDirection(_currentMarker.forwardDirection.normalized);
        float plateError = Vector3.Angle(plate.transform.forward, markerNormal);
        bool plateAligned = plateError <= plateAngleThreshold;
        UpdatePlateColor(plateAligned);
        LogAndDraw(_plateLine, pPos, _currentMarker.transform.position, plateAligned);
        Debug.Log($"[{activityName}] Plate vs Marker error: {plateError:F1}° (Aligned: {plateAligned})");

        // Emitter alignment if plate OK
        if (plateAligned)
        {
            bool emOK = CheckEmitterAlignment(markerNormal);
            UpdateEmitterColor(emOK);
            // draw emitter forward ray if aligned check
            Vector3 ePos = emitter.transform.position;
            LogAndDraw(_emitterLine, ePos, ePos + emitter.transform.up * 0.5f, emOK);
            Debug.Log($"[{activityName}] Emitter alignment check: {emOK}");
        }
        else
        {
            ResetEmitterColor();
        }
    }

    private void ConfigureLineRenderer(LineRenderer lr)
    {
        lr.positionCount = 2;
        lr.startWidth = 0.005f;
        lr.endWidth   = 0.005f;
        lr.material    = new Material(Shader.Find("Sprites/Default"));
        lr.enabled     = false;
    }

    private void LogAndDraw(LineRenderer lr, Vector3 start, Vector3 end, bool ok)
    {
        lr.startColor = lr.endColor = ok ? Color.green : Color.red;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.enabled = true;
    }

    private bool CheckEmitterAlignment(Vector3 markerNormal)
    {
        var ePos = emitter.transform.position;
        float dist = Vector3.Distance(ePos, _currentMarker.transform.position);
        if (dist < emitterMinDistance || dist > emitterMaxDistance)
        {
            Debug.Log($"[{activityName}] Emitter dist {dist:F2}m out of range [{emitterMinDistance}-{emitterMaxDistance}]");
            return false;
        }
        float normErr = Vector3.Angle(emitter.transform.up, -markerNormal);
        if (normErr > emitterAngleThreshold)
        {
            Debug.Log($"[{activityName}] Emitter vs Marker normal error: {normErr:F1}°");
            return false;
        }
        float plateErr = Vector3.Angle(emitter.transform.up, -plate.transform.forward);
        if (plateErr > emitterPlateAngleThreshold)
        {
            Debug.Log($"[{activityName}] Emitter vs Plate forward error: {plateErr:F1}°");
            return false;
        }
        return true;
    }

    private void UpdatePlateColor(bool ok)
    {
        if (_plateRenderer) _plateRenderer.material.color = ok ? Color.green : Color.red;
    }
    private void ResetPlateColor()
    {
        if (_plateRenderer) _plateRenderer.material.color = _plateOrigColor;
        _plateLine.enabled = false;
    }
    private void UpdateEmitterColor(bool ok)
    {
        if (_emitterRenderer) _emitterRenderer.material.color = ok ? Color.green : Color.red;
    }
    private void ResetEmitterColor()
    {
        if (_emitterRenderer) _emitterRenderer.material.color = _emitterOrigColor;
        _emitterLine.enabled = false;
    }
}
