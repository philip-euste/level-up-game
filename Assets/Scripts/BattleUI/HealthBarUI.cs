using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("Fighters")]
    public FighterHealth player1;
    public FighterHealth player2;

    [Header("Health Bar Fills")]
    public Image player1HealthFill;
    public Image player2HealthFill;

    void Start()
    {
        UpdateHealthBars();
    }

    void Update()
    {
        UpdateHealthBars();
    }

    void UpdateHealthBars()
    {
        UpdateHealthBar(player1, player1HealthFill);
        UpdateHealthBar(player2, player2HealthFill);
    }

    void UpdateHealthBar(FighterHealth fighter, Image fill)
    {
        if (fighter == null || fill == null)
            return;

        fill.fillAmount = fighter.maxHealth > 0f
            ? Mathf.Clamp01(fighter.currentHealth / fighter.maxHealth)
            : 0f;
    }
}
