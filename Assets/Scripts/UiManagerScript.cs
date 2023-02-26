using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManagerScript : MonoBehaviour
{
    private GameEngine GE;
    private GameObject pauseMenuObject;
    private GameObject gameOverObject;

    public GameObject pauseMenuPrefab;
    public GameObject gameOverPrefab;
    Dropdown dropdown;
    ToggleGroup toggleGroup;

    public void Start()
    {
        GE = GameEngine.GetGameEngine();
    }

    public void Update()
    {
        if (GE.currentPhase == Phase.GAME_OVER)
        {
            GE.currentPhase = Phase.PLAYER_MOVE_BLOCKED;
            GameObject canvas = GameObject.Find("TT Canvas");
            gameOverObject = Instantiate(gameOverPrefab);
            gameOverObject.transform.SetParent(canvas.transform, false);
            gameOverObject.SetActive(true);
            Text[] texts = gameOverObject.GetComponentsInChildren<Text>();
            Text winText = texts.First();
            winText.text = GE.currentState.GetPlayerScore() > GE.currentState.GetAiScore() ? "YOU WIN" : "YOU LOSE";
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenuObject == null)
            {
                GameObject canvas = GameObject.Find("TT Canvas");
                pauseMenuObject = Instantiate(pauseMenuPrefab);
                pauseMenuObject.transform.SetParent(canvas.transform, false);
            }
            else
            {
                GameObject.Destroy(pauseMenuObject);
                pauseMenuObject = null;
            }
        }
    }

    public void LoadScene()
    {
        GameEngine.GetGameEngine().Reset();

        dropdown = GameObject.Find("Dropdown").GetComponent<Dropdown>();
        SetDifficulty(dropdown.value);

        toggleGroup = GameObject.Find("ScoringOptions").GetComponent<ToggleGroup>();
        SetScoring(toggleGroup.ActiveToggles().FirstOrDefault().name);

        SceneManager.LoadScene("MainScene");
    }

    private void SetDifficulty(int dropdownValue)
    {
        switch (dropdownValue)
        {
            case 0:
                GameEngine.difficulty = 0;
                break;
            case 1:
                GameEngine.difficulty = 4;
                break;
            case 2:
                GameEngine.difficulty = 8;
                break;
        }
    }

    private void SetScoring(string option)
    {
        if (option == "zero-sum")
            GameEngine.isZeroSum = true;
        else
            GameEngine.isZeroSum = false;
    }

    public void Restart()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
