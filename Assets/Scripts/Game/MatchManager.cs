
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    public FighterHealth player1;
    public FighterHealth player2;

    private bool matchEnded = false;
    public bool MatchEnded => matchEnded;

    void Update()
    {
        if (matchEnded)
        {
            if (Input.GetKeyDown(KeyCode.R))
                RestartMatch();

            return;
        }

        if (player1 == null || player2 == null)
            return;

        if (player1.IsDefeated)
            EndMatch("Player 2 wins!");
        else if (player2.IsDefeated)
            EndMatch("Player 1 wins!");
    }

    void EndMatch(string result)
    {
        matchEnded = true;
        Debug.Log(result);
    }

    public void RestartMatch()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
