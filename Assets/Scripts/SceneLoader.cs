using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    private GameEngine GE = GameEngine.getGE();
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
            case 0: GE.Difficulty = 3; break;
            case 1: GE.Difficulty = 10; break;
            case 2: GE.Difficulty = 20; break;
        }
    }

    private void SetScoring(string option)
    {
        if (option == "zero-sum")
        {
            GE.isZeroSum = true;
        }
        else
        {
            GE.isZeroSum = false;
        }
    }
}
