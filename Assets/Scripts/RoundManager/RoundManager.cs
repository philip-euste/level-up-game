using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundManager : MonoBehaviour
{
    private enum RoundWinner
    {
        Player1,
        Player2,
        Draw
    }

    [Header("Fighters")]
    public FighterHealth player1;
    public FighterHealth player2;

    [Header("Round Settings")]
    public float roundDuration = 99f;
    public int roundsToWin = 3;

    [Header("UI")]
    public GameObject roundEndPanel;
    public TMP_Text resultText;
    public TMP_Text timerText;

    private float timeRemaining;
    private bool roundEnded;
    private int player1Wins;
    private int player2Wins;
    public bool MatchFinished { get; private set; }

    void Start()
    {
        if (roundEndPanel != null)
            roundEndPanel.SetActive(false);

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(roundDuration).ToString();

        StartRound();
    }

    void Update()
    {
        if (MatchFinished || roundEnded)
            return;

        timeRemaining -= Time.deltaTime;

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(
                Mathf.Max(timeRemaining, 0f)
            ).ToString();

        if (player1 == null || player2 == null)
            return;

        if (player1.IsDefeated)
        {
            EndRound(RoundWinner.Player2);
            return;
        }

        if (player2.IsDefeated)
        {
            EndRound(RoundWinner.Player1);
            return;
        }

        if (timeRemaining <= 0f)
        {
            if (player1.currentHealth > player2.currentHealth)
                EndRound(RoundWinner.Player1);
            else if (player2.currentHealth > player1.currentHealth)
                EndRound(RoundWinner.Player2);
            else
                EndRound(RoundWinner.Draw);
        }
    }

    public void StartRound()
    {
        if (MatchFinished)
            return;

        roundEnded = false;
        timeRemaining = roundDuration;

        if (roundEndPanel != null)
            roundEndPanel.SetActive(false);

        if (resultText != null)
            resultText.text = "";

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();

        if (player1 != null)
            ResetFighter(player1);

        if (player2 != null)
            ResetFighter(player2);

        Debug.Log("Round started!");
    }

    void ResetFighter(FighterHealth fighter)
    {
        if (fighter == null)
            return;

        fighter.ResetForRound();

        FighterController fighterController =
            fighter.GetComponent<FighterController>();

        if (fighterController != null)
            fighterController.ResetForRound();

        Rigidbody2D rb = fighter.GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    void EndRound(RoundWinner winner)
    {
        if (roundEnded || MatchFinished)
            return;

        roundEnded = true;

        string message;
        bool roundWonByPlayer1 = false;
        bool roundWonByPlayer2 = false;

        if (winner == RoundWinner.Player1)
        {
            player1Wins++;
            roundWonByPlayer1 = true;
            message = "Player 1 wins the round!";
        }
        else if (winner == RoundWinner.Player2)
        {
            player2Wins++;
            roundWonByPlayer2 = true;
            message = "Player 2 wins the round!";
        }
        else
        {
            message = "Round draw!";
        }

        if (player1Wins >= roundsToWin || player2Wins >= roundsToWin)
        {
            MatchFinished = true;

            if (player1Wins >= roundsToWin)
            {
                message = "PLAYER 1 WINS THE MATCH!";
                Debug.Log("Player 1 wins the match!");
            }
            else
            {
                message = "PLAYER 2 WINS THE MATCH!";
                Debug.Log("Player 2 wins the match!");
            }
        }
        else
        {
            Debug.Log(message);
        }

        if (resultText != null)
            resultText.text = message;

        if (roundEndPanel != null)
            roundEndPanel.SetActive(true);

        if (player1 != null)
        {
            FighterController controller =
                player1.GetComponent<FighterController>();

            if (controller != null)
                controller.SetControlsEnabled(false);
        }

        if (player2 != null)
        {
            FighterController controller =
                player2.GetComponent<FighterController>();

            if (controller != null)
                controller.SetControlsEnabled(false);
        }
    }

    public void NextRound()
    {
        if (MatchFinished)
        {
            Debug.Log("Match finished! Return to the menu or restart.");
            return;
        }

        StartRound();
    }
}