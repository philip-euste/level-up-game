using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("Round Win Circles")]
    [Header("Player 1")]
    public Image player1RoundCircle1;
    public Image player1RoundCircle2;
    public Image player1RoundCircle3;

    [Header("Player 2")]
    public Image player2RoundCircle1;
    public Image player2RoundCircle2;
    public Image player2RoundCircle3;

    private float timeRemaining;
    private bool roundEnded;
    private int player1Wins;
    private int player2Wins;
    private float matchElapsedTime;
    private bool roundTransitionInProgress;
    public bool MatchFinished { get; private set; }
    public int Player1Wins => player1Wins;
    public int Player2Wins => player2Wins;

    void Start()
    {
        UpdateRoundWinCircles();
        StartRound();
    }

    void Update()
    {
        if (!MatchFinished)
            matchElapsedTime += Time.deltaTime;

        if (MatchFinished || roundEnded || roundTransitionInProgress)
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
        if (MatchFinished || roundTransitionInProgress)
            return;

        roundTransitionInProgress = true;
        roundEnded = true;
        timeRemaining = roundDuration;

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();

        if (player1 != null)
            ResetFighter(player1);

        if (player2 != null)
            ResetFighter(player2);

        if (roundEndPanel != null)
            roundEndPanel.SetActive(true);

        StartCoroutine(CountdownToRound());
    }

    void ResetFighter(FighterHealth fighter)
    {
        if (fighter == null)
            return;

        FighterController fighterController =
            fighter.GetComponent<FighterController>();

        if (fighterController != null)
            fighterController.ResetForRound();
        else
            fighter.ResetForRound();

        Rigidbody2D rb = fighter.GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    IEnumerator CountdownToRound()
    {
        if (resultText != null)
            resultText.text = "3";
        yield return new WaitForSeconds(1f);

        if (resultText != null)
            resultText.text = "2";
        yield return new WaitForSeconds(1f);

        if (resultText != null)
            resultText.text = "1";
        yield return new WaitForSeconds(1f);

        if (resultText != null)
            resultText.text = "LABAN!";
        yield return new WaitForSeconds(1f);

        if (roundEndPanel != null)
            roundEndPanel.SetActive(false);

        SetFighterControls(true);
        roundEnded = false;
        roundTransitionInProgress = false;
        Debug.Log("Round started!");
    }

    void EndRound(RoundWinner winner)
    {
        if (roundEnded || roundTransitionInProgress || MatchFinished)
            return;

        roundEnded = true;
        roundTransitionInProgress = true;

        string message;

        if (winner == RoundWinner.Player1)
        {
            player1Wins++;
            message = "Player 1 wins the round!";
        }
        else if (winner == RoundWinner.Player2)
        {
            player2Wins++;
            message = "Player 2 wins the round!";
        }
        else
        {
            message = "Round draw!";
        }

        UpdateRoundWinCircles();

        Debug.Log(message);

        if (resultText != null)
            resultText.text = message;

        if (roundEndPanel != null)
            roundEndPanel.SetActive(true);

        SetFighterControls(false);
        StartCoroutine(ContinueAfterRoundResult());
    }

    IEnumerator ContinueAfterRoundResult()
    {
        yield return new WaitForSeconds(2f);

        if (player1Wins >= roundsToWin || player2Wins >= roundsToWin)
        {
            MatchFinished = true;

            string matchMessage = player1Wins >= roundsToWin
                ? "PLAYER 1 WINS THE MATCH!"
                : "PLAYER 2 WINS THE MATCH!";

            if (resultText != null)
                resultText.text = matchMessage;

            Debug.Log(matchMessage);
            MatchResultsController.SetMatchResults(
                matchMessage,
                player1Wins,
                player2Wins,
                matchElapsedTime
            );
            SceneManager.LoadScene("MatchResultsScene");
            roundTransitionInProgress = false;
            yield break;
        }

        roundTransitionInProgress = false;
        StartRound();
    }

    void SetFighterControls(bool enabled)
    {
        if (player1 != null)
        {
            FighterController controller =
                player1.GetComponent<FighterController>();

            if (controller != null)
                controller.SetControlsEnabled(enabled);
        }

        if (player2 != null)
        {
            FighterController controller =
                player2.GetComponent<FighterController>();

            if (controller != null)
                controller.SetControlsEnabled(enabled);
        }
    }

    void UpdateRoundWinCircles()
    {
        Color unearnedColor = Color.gray;
        Color player1EarnedColor = Color.green;
        Color player2EarnedColor = Color.red;

        SetRoundCircleColor(player1RoundCircle1,
            player1Wins >= 1 ? player1EarnedColor : unearnedColor);
        SetRoundCircleColor(player1RoundCircle2,
            player1Wins >= 2 ? player1EarnedColor : unearnedColor);
        SetRoundCircleColor(player1RoundCircle3,
            player1Wins >= 3 ? player1EarnedColor : unearnedColor);

        SetRoundCircleColor(player2RoundCircle1,
            player2Wins >= 1 ? player2EarnedColor : unearnedColor);
        SetRoundCircleColor(player2RoundCircle2,
            player2Wins >= 2 ? player2EarnedColor : unearnedColor);
        SetRoundCircleColor(player2RoundCircle3,
            player2Wins >= 3 ? player2EarnedColor : unearnedColor);
    }

    void SetRoundCircleColor(Image circle, Color color)
    {
        if (circle != null)
            circle.color = color;
    }

    public void NextRound()
    {
        Debug.Log("Rounds start automatically.");
    }
}