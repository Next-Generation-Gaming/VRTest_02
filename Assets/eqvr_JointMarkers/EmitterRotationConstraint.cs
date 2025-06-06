// Assets/Scripts/EmitterRotationConstraint.cs
using UnityEngine;

/// <summary>
/// Locks the emitter's roll by freezing rotation around its forward axis using Rigidbody constraints.
/// Ensures the emitter can only pitch (up/down) and yaw (left/right).
/// Attach this to your Emitter GameObject (ensure it has a Rigidbody component).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class EmitterRotationConstraint : MonoBehaviour
{
    void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        // Ensure Rigidbody is non-kinematic so constraints apply
        rb.isKinematic = false;
        rb.useGravity = false;

        // Freeze rotation around forward (Z) axis to prevent roll
        // This assumes the emitter's local forward axis is aligned with its Transform.forward (Z-axis)
        rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
    }
}