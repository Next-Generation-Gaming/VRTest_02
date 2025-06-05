using System;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Polls the Quest controllers for “A” (right hand) and “X” (left hand) button presses,
/// and invokes corresponding events exactly once per press.
/// </summary>
public class InputScript : MonoBehaviour
{
    /// <summary>
    /// Fired exactly once when the Quest “A” button is pressed
    /// </summary>
    public event Action OnAButtonPressed;

    /// <summary>
    /// Fired exactly once when the Quest “X” button is pressed
    /// </summary>
    public event Action OnXButtonPressed;

    [Header("XR Button Settings")]
    [Tooltip("Which XRNode to poll for A (primaryButton on the right hand).")]
    [SerializeField] private XRNode xrNodeA = XRNode.RightHand;

    [Tooltip("Which XRNode to poll for X (primaryButton on the left hand).")]
    [SerializeField] private XRNode xrNodeX = XRNode.LeftHand;

    private InputDevice _deviceA;
    private InputDevice _deviceX;

    // Track previous frame states to detect rising edges.
    private bool _prevAState = false;
    private bool _prevXState = false;

    private void Start()
    {
        // Grab both controllers at start.
        _deviceA = InputDevices.GetDeviceAtXRNode(xrNodeA);
        _deviceX = InputDevices.GetDeviceAtXRNode(xrNodeX);
    }

    private void Update()
    {
        // Re-acquire the right-hand device if it’s no longer valid.
        if (!_deviceA.isValid)
        {
            _deviceA = InputDevices.GetDeviceAtXRNode(xrNodeA);
            _prevAState = false; // Reset to avoid phantom press when reconnecting.
        }

        // Re-acquire the left-hand device if it’s no longer valid.
        if (!_deviceX.isValid)
        {
            _deviceX = InputDevices.GetDeviceAtXRNode(xrNodeX);
            _prevXState = false; // Reset to avoid phantom press when reconnecting.
        }

        // Check the right-hand “A” button (primaryButton).
        if (_deviceA.TryGetFeatureValue(CommonUsages.primaryButton, out bool aPressed))
        {
            // Rising edge: was false last frame, now true this frame.
            if (aPressed && !_prevAState)
            {
                OnAButtonPressed?.Invoke();
            }
            _prevAState = aPressed;
        }

        // Check the left-hand “X” button (primaryButton).
        if (_deviceX.TryGetFeatureValue(CommonUsages.primaryButton, out bool xPressed))
        {
            // Rising edge: was false last frame, now true this frame.
            if (xPressed && !_prevXState)
            {
                OnXButtonPressed?.Invoke();
            }
            _prevXState = xPressed;
        }
    }
}
