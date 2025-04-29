using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewMainMenuFunctions : MonoBehaviour
{
    //TODO: Make the GameObjects private and assign them in Start
    [Header("Menu Objects")]
    public GameObject initialMenu;
    public GameObject GuidedMode_Menu1;
    public GameObject GuidedMode_Menu2;
    public GameObject AssessmentMode_Menu1;
    public GameObject XrayDropdown;
    public GameObject BodyPartDropdown;
    public GameObject ConfirmationScreen;
    public GameObject LegSelectionDropdown;
    
    [Header("Training Setup Data for debugging only, *Do not set in inspector*")]
    [SerializeField] private TrainingMode trainingMode;
    [SerializeField] private TaskType taskType;
    [SerializeField] private XrayType xrayType;
    [SerializeField] private BodyPart bodyPart;
    [SerializeField] private LegSelect legSelection;
    
    private List<XrayType> xrayTypes = new List<XrayType>();
    private List<BodyPart> bodyParts = new List<BodyPart>();
    
    
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitalMenuSetup();

        // fill xray types List for randomisation
        xrayTypes.Add(XrayType.DP_DPI);
        xrayTypes.Add(XrayType.LM);
        xrayTypes.Add(XrayType.DLPMO);
        xrayTypes.Add(XrayType.DMPLO);
        
        // fill body parts List for randomisation
        bodyParts.Add(BodyPart.Fetlock);
        bodyParts.Add(BodyPart.Foot);
        bodyParts.Add(BodyPart.Carpus);
        bodyParts.Add(BodyPart.Tarsus);
        bodyParts.Add(BodyPart.Stifle);
        bodyParts.Add(BodyPart.Head);
    }

    private void InitalMenuSetup()
    {
        // Ensure only intital menu is active
        initialMenu.SetActive(true);
        // loop through children , starting at index 1 and set to false
        for (int i = 1; i < initialMenu.transform.childCount; i++)
        {
            initialMenu.transform.parent.GetChild(i).gameObject.SetActive(false);
        }
    }

    //Used For Button Unity Events

    //Mode Buttons:
    public void OnGuidedModeButtonPressed()
    {
        initialMenu.SetActive(false);
        GuidedMode_Menu1.SetActive(true);
        trainingMode = TrainingMode.GuidedMode;
        Debug.Log("Guided Mode button pressed");
    }
    public void OnAssessmentButtonPressed()
    {
        initialMenu.SetActive(false);
        AssessmentMode_Menu1.SetActive(true);
        trainingMode = TrainingMode.AssessmentMode;
        Debug.Log("Assessment Mode button pressed");
    }
    
    
    //Task Select (Emitter, Plate or Both)
    public void OnTaskSelection(int selection)
    {
        switch (selection)
        {
            case 0:
                taskType = TaskType.Emitter;
                break;
            case 1:
                taskType = TaskType.Plate;
                break;
            case 2:
                taskType = TaskType.Both;
                break;
            default:
                taskType = TaskType.None;
                break;
        }
        
        Debug.Log("Task Type selected: " + taskType);
        
        GuidedMode_Menu1.SetActive(false);
        var taskSelectedText = GuidedMode_Menu2.transform.GetChild(1).GetComponentInChildren<TMP_Text>();
        GuidedMode_Menu2.SetActive(true);
        taskSelectedText.text = "Task Type: " + taskType;
    }
    
    
    //Dropdown toggles
    public void OnXrayDropdownButtonPressed()
    {
        XrayDropdown.SetActive(!XrayDropdown.activeSelf);
        Debug.Log("Xray dropdown button pressed");
    }
    public void OnBodyPartDropdownButtonPressed()
    {
        BodyPartDropdown.SetActive(!BodyPartDropdown.activeSelf);
        Debug.Log("Body Part Dropdown button pressed");
    }
    public void OnLegSelectionDropdownButtonPressed()
    {
        LegSelectionDropdown.SetActive(!LegSelectionDropdown.activeSelf);
        Debug.Log("Leg Selection Dropdown button pressed");
    }
    
    
    //Dropdown Choice Selection
    public void OnXrayTypeSelected(int selection)
    {
        switch (selection)
        {
            case 0:
                xrayType = XrayType.DP_DPI;
                break;
            case 1:
                xrayType = XrayType.LM;
                break;
            case 2:
                xrayType = XrayType.DLPMO;
                break;
            case 3:
                xrayType = XrayType.DMPLO;
                break;
            default:
                xrayType = XrayType.None;
                break;
        }
        
        Debug.Log("X-ray Type selected: " + xrayType);
        XrayDropdown.SetActive(false);
        var xrayTypeSelectedText = XrayDropdown.transform.parent.GetChild(0).GetComponentInChildren<TMP_Text>();
        xrayTypeSelectedText.text = "X-ray Type: " + xrayType;
        ContinueCheck();
    }
    public void OnBodyPartSelected(int selection)
    {
        switch (selection)
        {
            case 0:
                bodyPart = BodyPart.Fetlock;
                break;
            case 1:
                bodyPart = BodyPart.Foot;
                break;
            case 2:
                bodyPart = BodyPart.Carpus;
                break;
            case 3:
                bodyPart = BodyPart.Tarsus;
                break;
            case 4:
                bodyPart = BodyPart.Stifle;
                break;
            case 5:
                bodyPart = BodyPart.Head;
                break;
            default:
                bodyPart = BodyPart.None;
                break;
        }
        
        BodyPartDropdown.SetActive(false);
        var bodyPartSelectedText = BodyPartDropdown.transform.parent.GetChild(0).GetComponentInChildren<TMP_Text>();
        bodyPartSelectedText.text = "Body Part: " + bodyPart;
        Debug.Log("Body Part selected: " + bodyPart);
        
        ContinueCheck();
    }
    public void OnLegSelection(int selection)
    {
        switch (selection)
        {
            case 0:
                legSelection = LegSelect.FrontLeft;
                break;
            case 1:
                legSelection = LegSelect.FrontRight;
                break;
            case 2:
                legSelection = LegSelect.HindLeft;
                break;
            case 3:
                legSelection = LegSelect.HindRight;
                break;
            default:
                legSelection = LegSelect.None;
                break;
        }
        LegSelectionDropdown.SetActive(false);
        var legSelectionText = LegSelectionDropdown.transform.parent.GetChild(0).GetComponentInChildren<TMP_Text>();
        legSelectionText.text = "Leg Selection: " + legSelection;
        Debug.Log("Leg Selection selected: " + legSelection);
        ContinueCheck();
    }
    // End of Button Unity Events
    private void ContinueCheck()
    {
        // if all options have been selected , show confirmation screen
        if (trainingMode != TrainingMode.None && taskType != TaskType.None && xrayType != XrayType.None && bodyPart != BodyPart.None && legSelection != LegSelect.None)
        {
            ShowConfirmationScreen();
        }
        else
        {
            Debug.LogWarning("Please select all options before continuing");
        }
    }
    
    private void ShowConfirmationScreen()
    {
        AssessmentMode_Menu1.SetActive(false);
        GuidedMode_Menu2.SetActive(false);
        var selectRoleText = ConfirmationScreen.transform.GetChild(3).GetComponentInChildren<TMP_Text>();
        var xrayTypeText = ConfirmationScreen.transform.GetChild(4).GetComponentInChildren<TMP_Text>();
        var bodyPartText = ConfirmationScreen.transform.GetChild(5).GetComponentInChildren<TMP_Text>();
        
        bodyPartText.text = "Body Part: " + bodyPart;
        xrayTypeText.text = "X-ray Type: " + xrayType;
        selectRoleText.text = "Training Mode: " + trainingMode;
        ConfirmationScreen.SetActive(true);
        
        //Waits for confirmation button press or restart button Press
    }
    
    //Also a Unity Button Event
    public void OnConfirmationButtonPressed()
    {
        ScenarioDataLoader.Instance.SaveScenarioData(trainingMode, taskType, xrayType, bodyPart, legSelection);
    
        bool isDataValid = CheckScenarioData();
        if (!isDataValid)
        {
            Debug.LogError("Scenario data is not set correctly");
            // Resetting logic goes here
            ResetSelections();
            InitalMenuSetup();
        }
    }
    
    private bool CheckScenarioData()
    {
        // check if any data is missing or set to default/none
        if (trainingMode == TrainingMode.None)
        {
            Debug.LogWarning("Training Mode is not set");
            return false;
        }
        
        if (xrayType == XrayType.None)
        {
            Debug.LogWarning("X-ray Type is not set");
            return false;
        }
        
        if (bodyPart == BodyPart.None)
        {
            Debug.LogWarning("Body Part is not set");
            return false;
        }
        
        if (trainingMode != TrainingMode.None && xrayType != XrayType.None && bodyPart != BodyPart.None)
        {
            Debug.Log("All data is set");
        }

        return true;
    }
    
    public void ResetSelections()
    {
        // Set all data to none
        trainingMode = TrainingMode.None;
        xrayType = XrayType.None;
        bodyPart = BodyPart.None;
        taskType = TaskType.None;
        xrayType = XrayType.None;
        bodyPart = BodyPart.None;
        Debug.Log("All data has been reset");
        
        // Get the text components for the dropdowns and reset them
        var bodyPartSelectedText = BodyPartDropdown.transform.parent.GetChild(0).GetComponentInChildren<TMP_Text>();
        var xrayTypeSelectedText = XrayDropdown.transform.parent.GetChild(0).GetComponentInChildren<TMP_Text>();
        var selectRoleText = ConfirmationScreen.transform.GetChild(3).GetComponentInChildren<TMP_Text>();
        var taskSelectedText = GuidedMode_Menu2.transform.GetChild(1).GetComponentInChildren<TMP_Text>();
        var legSelectionText = LegSelectionDropdown.transform.parent.GetChild(0).GetComponentInChildren<TMP_Text>();
        
        // Reset the text to default
        bodyPartSelectedText.text = "Select Body Part:";
        xrayTypeSelectedText.text = "Select Xray Type:";
        taskSelectedText.text = "Select Task Type:";
        selectRoleText.text = "Select Training Mode:";
        legSelectionText.text = "Select Leg:";
        // Return to starting screen
        initialMenu.SetActive(true);
        ConfirmationScreen.SetActive(false);
    }
    
    public void LoadMainScene()
    {
        // Load the main scene
        Debug.Log("Loading main scene");
        SceneManager.LoadScene(1);
    }

    /// Assessment Mode Setup
    public void AssessmentModeSetup(int taskID)
    {
        switch (taskID)
        {
            case 0:
                // Emitter task
                Debug.Log("Emitter Task Assessment Setup initiated");
                taskType = TaskType.Emitter;
                RandomiseTypeAndPart();
                break;
            case 1:
                // Plate task
                Debug.Log("Plate Task Assessment Setup initiated");
                taskType = TaskType.Plate;
                RandomiseTypeAndPart();
                break;
            case 2:
                // Both task
                Debug.Log("Both Task Assessment Setup initiated");
                taskType = TaskType.Both;
                RandomiseTypeAndPart();
                break;
            default:
                Debug.LogWarning("Task ID not found");
                break;
        }
        
        // Set the training setup data
        trainingMode = TrainingMode.AssessmentMode;
        
        CheckScenarioData();
        ShowConfirmationScreen();
    }
    private void RandomiseTypeAndPart()
    {
        // Randomise the x-ray type and body part
        int randomXrayType = Random.Range(0, xrayTypes.Count);
        int randomBodyPart = Random.Range(0, bodyParts.Count);
        
        xrayType = xrayTypes[randomXrayType];
        bodyPart = bodyParts[randomBodyPart];
    }
}

//Enums for different selections the player can make, used across the project.
public enum LegSelect
{
    None,
    FrontLeft,
    FrontRight,
    HindLeft,
    HindRight
}

public enum TaskType
{
    None,
    Emitter,
    Plate,
    Both
}

/// Enums for the training setup
public enum TrainingMode
{
    None,
    GuidedMode,
    AssessmentMode
}

public enum XrayType
{
    None,
    DP_DPI,
    LM,
    DLPMO,
    DMPLO
}

public enum BodyPart
{
    None,
    Fetlock,
    Foot,
    Carpus,
    Tarsus,
    Stifle,
    Head,
    Default
    
}
