using System;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Polls the Quest controllers for “A” (right hand) and “X” (left hand) button presses,
/// and invokes corresponding events exactly once per press.
/// </summary>
public class InputScript : MonoBehaviour
{

    public event Action OnAButtonPressed;


    public event Action OnXButtonPressed;

    [Header("XR Button Settings")]
    [Tooltip("Which XRNode to poll for A (primaryButton on the right hand).")]
    [SerializeField] private XRNode xrNodeA = XRNode.RightHand;

    [Tooltip("Which XRNode to poll for X (primaryButton on the left hand).")]
    [SerializeField] private XRNode xrNodeX = XRNode.LeftHand;

    private InputDevice _deviceA;
    private InputDevice _deviceX;


    private bool _prevAState = false;
    private bool _prevXState = false;

    private void Start()
    {

        _deviceA = InputDevices.GetDeviceAtXRNode(xrNodeA);
        _deviceX = InputDevices.GetDeviceAtXRNode(xrNodeX);
    }

    private void Update()
    {

        if (!_deviceA.isValid)
        {
            _deviceA = InputDevices.GetDeviceAtXRNode(xrNodeA);
            _prevAState = false;
        }

        if (!_deviceX.isValid)
        {
            _deviceX = InputDevices.GetDeviceAtXRNode(xrNodeX);
            _prevXState = false;
        }


        if (_deviceA.TryGetFeatureValue(CommonUsages.primaryButton, out bool aPressed))
        {

            if (aPressed && !_prevAState)
            {
                OnAButtonPressed?.Invoke();
            }
            _prevAState = aPressed;
        }

        if (_deviceX.TryGetFeatureValue(CommonUsages.primaryButton, out bool xPressed))
        {

            if (xPressed && !_prevXState)
            {
                OnXButtonPressed?.Invoke();
            }
            _prevXState = xPressed;
        }
    }
}