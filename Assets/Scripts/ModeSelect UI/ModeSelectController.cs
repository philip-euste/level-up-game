using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelectController : MonoBehaviour
{
    [SerializeField] private GameObject popupOverlay;
    [SerializeField] private TMP_Text statusMessage;
    [SerializeField] private TMP_Text popupMessage;

    void Awake()
    {
        if (popupOverlay != null)
        {
            popupOverlay.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Popup Overlay is not assigned!");
        }

        if (statusMessage != null)
        {
            statusMessage.gameObject.SetActive(false);
        }
    }

    public void LocalPlay()
    {
        SceneManager.LoadScene("BattleScene");
    }

    public void OnlinePlay()
    {
        ShowComingSoon();
    }

    public void Training()
    {
        ShowComingSoon();
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ClosePopup()
    {
        if (popupOverlay != null)
        {
            popupOverlay.SetActive(false);
        }

        if (statusMessage != null)
        {
            statusMessage.gameObject.SetActive(false);
        }
    }

    void ShowComingSoon()
    {
        if (popupOverlay == null || statusMessage == null || popupMessage == null)
        {
            Debug.LogWarning("Cannot show popup: assign the Popup Overlay, Status Message, and Popup Message.");
            return;
        }

        statusMessage.text = "COMING SOON";
        statusMessage.gameObject.SetActive(true);
        popupMessage.text = "This mode is not available yet.";
        popupOverlay.SetActive(true);
    }
}
