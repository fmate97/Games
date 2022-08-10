using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIPanelController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject startPanel;
    [SerializeField] GameObject ingamePanel;
    [SerializeField] GameObject ingamePausePanel;
    [SerializeField] GameObject victoryPanel;
    [SerializeField] GameObject defeatPanel;
    [Header("Panels Level Name")]
    [SerializeField] string levelName;
    [SerializeField] TextMeshProUGUI startPanelLevelName;
    [SerializeField] TextMeshProUGUI ingamePausePanelLevelName;
    [SerializeField] TextMeshProUGUI victoryPanelLevelName;
    [SerializeField] TextMeshProUGUI defeatPanelLevelName;
    [Header("Level Controller")]
    [SerializeField] GameObject levelControllerGameObject;
    [Header("Button click sound")]
    [SerializeField] AudioSource clickSound;

    private string _continuePrefKey = "nextLevel";
    private LevelController _levelControllerScript;

    void Start()
    {
        Time.timeScale = 1;

        AllPanelDeactive();
        startPanel.SetActive(true);

        startPanelLevelName.text = ingamePausePanelLevelName.text = victoryPanelLevelName.text = 
            defeatPanelLevelName.text = levelName;

        _levelControllerScript = levelControllerGameObject.GetComponent<LevelController>();

        PlayerPrefs.SetString(_continuePrefKey, SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
    }

    void LateUpdate()
    {
        if (_levelControllerScript._gamePause)
        {
            Time.timeScale = 0;
            ingamePausePanel.SetActive(true);
        }

        if (_levelControllerScript._gameOver)
        {
            Time.timeScale = 0;
            AllPanelDeactive();
            defeatPanel.SetActive(true);
        }
        else if (_levelControllerScript._gameVictory)
        {
            Time.timeScale = 0;
            AllPanelDeactive();
            victoryPanel.SetActive(true);
        }
    }

    private void AllPanelDeactive()
    {
        startPanel.SetActive(false);
        ingamePanel.SetActive(false);
        ingamePausePanel.SetActive(false);
        victoryPanel.SetActive(false);
        defeatPanel.SetActive(false);
    }

    public void PlayButtonClick()
    {
        clickSound.Play();
        AllPanelDeactive();
        ingamePanel.SetActive(true);
        levelControllerGameObject.SetActive(true);
    }

    public void ContinueButtonClick()
    {
        clickSound.Play();
        _levelControllerScript._gamePause = false;
        Time.timeScale = 1;
        ingamePausePanel.SetActive(false);
    }

    public void AgainButtonClick()
    {
        clickSound.Play();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void NextLevel()
    {
        clickSound.Play();
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (SceneManager.sceneCount > nextSceneIndex)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    public void MenuButtonClick()
    {
        clickSound.Play();
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitButtonClick()
    {
        clickSound.Play();
        Application.Quit();
    }
}
