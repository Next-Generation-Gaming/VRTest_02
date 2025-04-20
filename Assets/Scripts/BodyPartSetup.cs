using UnityEngine;

public class BodyPartSetup : MonoBehaviour
{
    
    private XrayType xrayType = XrayType.None;
    private BodyPart bodyPart;
    private LegSelect legSelection;
    void Start()
    {

            xrayType = ScenarioDataLoader.Instance.scenarioData.xrayType;
            Debug.Log(xrayType);
            bodyPart = ScenarioDataLoader.Instance.scenarioData.bodyPart;
            Debug.Log(bodyPart);
            legSelection = ScenarioDataLoader.Instance.scenarioData.legSelection;
            Debug.Log(legSelection);
            
        // Find Body Part based on child index 
        // 0: Fetlock , 1: Foot , 2: Carpus , 3: Tarsus , 4: Stifle , 5: Head
        
        switch (bodyPart)
        {
            case BodyPart.Fetlock:
                ActivateBodyPart(legSelection,0,bodyPart);
                break;
            case BodyPart.Foot:
                ActivateBodyPart(legSelection,1,bodyPart);
                break;
            case BodyPart.Carpus:
                ActivateBodyPart(legSelection,2,bodyPart);
                break;
            case BodyPart.Tarsus:
                ActivateBodyPart(legSelection,3,bodyPart);
                break;
            case BodyPart.Stifle:
                ActivateBodyPart(legSelection,4,bodyPart);
                break;
            case BodyPart.Head:
                ActivateBodyPart(legSelection,5,bodyPart);
                break;
            // Add more cases as needed for other body parts
            default:
                Debug.LogError("Invalid body part");
                break;
        }
    }
    
    //Activate Body Part was made into a function as it was used multiple times above,
    // It functions by taking the leg selection and the index of the body part to activate
    // the correct body part and leg selection
    private void ActivateBodyPart(LegSelect legSelection, int index,BodyPart bodyPart)
    {
        transform.GetChild(index).gameObject.SetActive(true);
        Debug.Log(bodyPart + " activated");
        LegSelection(legSelection, transform.GetChild(index).gameObject);
    }

    private void LegSelection(LegSelect legSelection,GameObject bodyPartObject )
    {
        switch (legSelection)
        {
            case LegSelect.FrontLeft:
                ActivateLeg(bodyPartObject,0);
                break;
            case LegSelect.FrontRight:
                ActivateLeg(bodyPartObject,1);
                break;
            case LegSelect.HindLeft:
                ActivateLeg(bodyPartObject,2);
                break;
            case LegSelect.HindRight:
                ActivateLeg(bodyPartObject,3);
                break;
            default:
                Debug.LogError("Invalid leg selection");
                break;
        }
    }

    // Below was made into a function as it was used multiple times,
    // only one line, Not needed, but gives better readability above.
    private void ActivateLeg(GameObject bodyPartObject , int index)
    {
        Debug.Log("FrontLeft activated");
        bodyPartObject.transform.GetChild(index).gameObject.SetActive(true);
    }
}
