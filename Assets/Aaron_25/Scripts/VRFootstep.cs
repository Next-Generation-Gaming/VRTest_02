using System;
using Aaron_25;
using UnityEngine;

public class VRFootstep : MonoBehaviour
{
    public float stepDistance = 1.5f;
    private int[] footstepIndices = { 0, 1, 2, 3, 4, 5, 6, 7 };
    
    private Vector3 lastPosition;
    private float distanceMoved;
    
    private void Start()
    {
        lastPosition = transform.position;
        distanceMoved = 0f;
    }

    private void LateUpdate()
    {
        if (lastPosition == transform.position) return;
        distanceMoved += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;
        if (distanceMoved >= stepDistance)
        {
            Debug.Log("Footstep reset");
            PlayFootstepSound();
            distanceMoved = 0f; // Reset distance after playing footstep
        }
    }

    private void PlayFootstepSound()
    {
        var index = footstepIndices[UnityEngine.Random.Range(0, footstepIndices.Length)];
        if (index == null)
        {
            Debug.LogError("Invalid footstep index");
            return;
        }
        
        AudioManager.Instance.PlayAudio(index,AudioLibraryType.Player);
    }
}
