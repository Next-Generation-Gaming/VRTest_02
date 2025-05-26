using System.Runtime.InteropServices;
using Aaron_25;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Danger : MonoBehaviour
{
   // public GameObject dangerZone1;
    //public GameObject dangerZone2;
    public AudioClip audioClip;
    // public Image dangerImage;
    //public Text dangerText;
    //public GameObject dangerPanel;
    private MeshRenderer meshRenderer;
    private AudioSource audioReference;
    public Side side;
    
    private bool hasEntered = false;

    void Start()
    {
        // grab the component reference so you don't have to do GetComponent<AudioSource>().PlayOneShot() all the time
        // but just  audioReference.PlayOneShot(audioClip);
        audioReference = GetComponent<AudioSource>();  

        // Get the MeshRenderer component and disable it
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
            //dangerPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            meshRenderer.enabled = true;
            GetComponent<AudioSource>().PlayOneShot(audioClip);
            audioReference.PlayOneShot(audioClip);
            //dangerPanel.SetActive(true);
        }
        
        if (other.tag == "Player" && side == Side.Front)
        {
            if (hasEntered)
            {
                // if has already entered, play more agressive sound
                AudioManager.Instance.PlayAudio(4, AudioLibraryType.Horse, true);
                return;
            }
            AudioManager.Instance.PlayAudio(3,AudioLibraryType.Horse,true);
        }
        else if (other.tag == "Player" && side == Side.Back)
        {
            if (hasEntered)
            {
                // if has already entered, play more agressive sound
                AudioManager.Instance.PlayAudio(6, AudioLibraryType.Horse, true);
                return;
            }
            AudioManager.Instance.PlayAudio(5,AudioLibraryType.Horse,true);
        }
        
        hasEntered = true;
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    GetComponent<AudioSource>().PlayOneShot(audioClip);
    //    meshRenderer.enabled = true;
    //    dangerPanel.SetActive(true);
    //}

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            meshRenderer.enabled = false;
           // dangerPanel.SetActive(false);
            audioReference.Stop();
        }
    }
    
    public enum Side
    {
        Front,
        Back
    }
}
