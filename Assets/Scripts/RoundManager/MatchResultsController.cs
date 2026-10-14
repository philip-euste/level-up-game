using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchResultsController : MonoBehaviour
{
    public TMP_Text winnerText;
    public TMP_Text player1WinsText;
    public TMP_Text player2WinsText;
    public TMP_Text matchDurationText;

    private static bool hasMatchResults;
    private static string matchWinner;
    private static int player1Wins;
    private static int player2Wins;
    private static float matchElapsedTime;

    public static void SetMatchResults(
        string winner,
        int player1RoundWins,
        int player2RoundWins,
        float elapsedTime
    )
    {
        matchWinner = winner;
        player1Wins = player1RoundWins;
        player2Wins = player2RoundWins;
        matchElapsedTime = elapsedTime;
        hasMatchResults = true;
    }

    void Start()
    {
        if (!hasMatchResults)
        {
            Debug.LogWarning(
                "No match results are available. Open this scene by completing a match."
            );
            return;
        }

        if (winnerText == null)
            Debug.LogError("Assign Winner Text on MatchResultsController.");
        else
            winnerText.text = matchWinner;

        if (player1WinsText == null)
            Debug.LogError("Assign Player 1 Wins Text on MatchResultsController.");
        else
            player1WinsText.text = $"PLAYER 1 ROUND WINS: {player1Wins}";

        if (player2WinsText == null)
            Debug.LogError("Assign Player 2 Wins Text on MatchResultsController.");
        else
            player2WinsText.text = $"PLAYER 2 ROUND WINS: {player2Wins}";

        if (matchDurationText == null)
            Debug.LogError("Assign Match Duration Text on MatchResultsController.");
        else
        {
            TimeSpan duration = TimeSpan.FromSeconds(matchElapsedTime);
            string format = duration.TotalHours >= 1
                ? @"h\:mm\:ss"
                : @"m\:ss";
            matchDurationText.text = $"TOTAL PLAY TIME: {duration.ToString(format)}";
        }
    }

    public void RetryMatch()
    {
        LoadScene("BattleScene");
    }

    public void ChangeCharacters()
    {
        LoadScene("CharacterSelectScene");
    }

    public void ReturnToMainMenu()
    {
        LoadScene("MainMenu");
    }

    private void LoadScene(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"Cannot load '{sceneName}': it is missing from the active Build Profile."
            );
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
