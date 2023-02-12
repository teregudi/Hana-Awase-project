using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    Dropdown dropdown;
    ToggleGroup toggleGroup;

    public void LoadScene()
    {
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
            case 0: GameEngine.difficulty = 0; break;
            case 1: GameEngine.difficulty = 5; break;
            case 2: GameEngine.difficulty = 9; break;
        }
    }

    private void SetScoring(string option)
    {
        if (option == "zero-sum") GameEngine.isZeroSum = true;
        else GameEngine.isZeroSum = false;
    }
}
