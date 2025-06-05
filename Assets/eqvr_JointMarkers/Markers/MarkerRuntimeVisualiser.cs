using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Draws in-game debug lines for each allowed angle when showAngleGizmos is enabled.
/// Requires a simple unlit material assigned to lineMaterial.
/// Attach this to the same GameObject as Marker.
/// </summary>
[RequireComponent(typeof(Marker))]
public class MarkerRuntimeVisualizer : MonoBehaviour
{
    [Tooltip("Material used for drawing debug lines (e.g. Sprites/Default)")]
    public Material lineMaterial;
    [Tooltip("Color for debug lines")]
    public Color    debugColor = Color.yellow;

    private Marker _marker;
    private List<LineRenderer> _lines = new List<LineRenderer>();

    void Awake()
    {
        _marker = GetComponent<Marker>();
        // Create a LineRenderer for each allowed angle
        foreach (float angle in _marker.allowedAngles)
        {
            var go = new GameObject($"AngleLine_{angle}");
            go.transform.SetParent(transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material      = lineMaterial;
            lr.startWidth    = lr.endWidth = 0.005f;
            lr.startColor    = lr.endColor  = debugColor;
            lr.enabled       = false;
            _lines.Add(lr);
        }
    }

    void Update()
    {
        bool show = _marker.showAngleGizmos;
        for (int i = 0; i < _lines.Count; i++)
        {
            var lr    = _lines[i];
            lr.enabled = show;
            if (!show) continue;

            float angle       = _marker.allowedAngles[i];
            Quaternion rot    = Quaternion.AngleAxis(angle, transform.up);
            Vector3 angleDir  = transform.TransformDirection(rot * _marker.forwardDirection.normalized) * 0.2f;
            Vector3 start     = transform.position;
            lr.SetPosition(0, start);
            lr.SetPosition(1, start + angleDir);
        }
    }
}