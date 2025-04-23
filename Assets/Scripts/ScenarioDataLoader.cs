using UnityEngine;

public class ScenarioDataLoader : MonoBehaviour
{
    public ScenarioData scenarioData = new ScenarioData();
    public static ScenarioDataLoader Instance { get; private set; }
    public void Start()
    {
        // Check if an instance of ScenarioDataLoader already exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Ensure this object persists across scene loads
        DontDestroyOnLoad(this.gameObject);
        
        
    }

    public void Awake()
    {
        // if main menu scene is loaded, reset the scenario data
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
        {
            Debug.Log("Main menu scene loaded, resetting scenario data.");
            ResetScenarioData();
        }
    }
    
    
    public void SaveScenarioData(TrainingMode trainingMode, TaskType taskType, XrayType xrayType, BodyPart bodyPart, LegSelect legSelection)
    {
        // Save the scenario data to the struct
        scenarioData.trainingMode = trainingMode;
        scenarioData.taskType = taskType;
        scenarioData.xrayType = xrayType;
        scenarioData.bodyPart = bodyPart;
        scenarioData.legSelection = legSelection;
    }

    private void ResetScenarioData()
    {
        // Reset the scenario data to default values
        scenarioData.trainingMode = TrainingMode.None;
        scenarioData.taskType = TaskType.None;
        scenarioData.xrayType = XrayType.None;
        scenarioData.bodyPart = BodyPart.None;
        scenarioData.legSelection = LegSelect.None;
    }
    
    
    [System.Serializable]
    public struct ScenarioData
    {
        public TrainingMode trainingMode;
        public TaskType taskType;
        public XrayType xrayType;
        public BodyPart bodyPart;
        public LegSelect legSelection;
    }
}
