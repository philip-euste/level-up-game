
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    public FighterHealth player1;
    public FighterHealth player2;
    public RoundManager roundManager;

    public bool MatchEnded =>
        roundManager != null && roundManager.MatchFinished;

    void Awake()
    {
        if (roundManager == null)
            roundManager = FindAnyObjectByType<RoundManager>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            RestartMatch();

        if (Input.GetKeyDown(KeyCode.Escape))
            ReturnToMainMenu();
    }

    public void RestartMatch()
    {
        SceneManager.LoadScene("BattleScene");
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
