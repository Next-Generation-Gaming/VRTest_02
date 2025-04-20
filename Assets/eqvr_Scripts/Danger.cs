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
}
