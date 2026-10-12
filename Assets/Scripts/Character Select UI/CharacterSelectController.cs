using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class CharacterSelectController : MonoBehaviour
{
    [Serializable]
    private sealed class CharacterEntry
    {
        public string characterId;
        public string displayName;
        public bool isPlayable;
        public Sprite previewSprite;
        public RuntimeAnimatorController previewAnimatorController;
    }

    private sealed class PlayerSelection
    {
        public int index;
        public bool isReady;
        public string status;
        public CharacterSelectionPreview preview;
        public TMP_Text nameLabel;
        public TMP_Text availabilityLabel;
        public TMP_Text statusLabel;
        public TMP_Text placeholderLabel;
    }

    [Serializable]
    private sealed class LayoutSettings
    {
        public Vector2 referenceResolution = new Vector2(1920f, 1080f);
        public Color backgroundColor = new Color(0.035f, 0.055f, 0.09f, 1f);
        public Color panelColor = new Color(0.08f, 0.13f, 0.2f, 0.78f);
        public Color accentColor = new Color(0.95f, 0.68f, 0.25f, 1f);
        public Color textColor = Color.white;
        public Color readyColor = new Color(0.4f, 0.9f, 0.55f, 1f);
        public Vector2 player1PanelMin = new Vector2(0.03f, 0.13f);
        public Vector2 player1PanelMax = new Vector2(0.49f, 0.87f);
        public Vector2 player2PanelMin = new Vector2(0.51f, 0.13f);
        public Vector2 player2PanelMax = new Vector2(0.97f, 0.87f);
        public Vector2 titleAnchor = new Vector2(0.5f, 0.95f);
        public Vector2 titleSize = new Vector2(900f, 64f);
        public Vector2 sectionTitleAnchor = new Vector2(0.5f, 0.89f);
        public Vector2 sectionTitleSize = new Vector2(760f, 60f);
        public Vector2 previewPlaceholderAnchor = new Vector2(0.5f, 0.55f);
        public Vector2 previewPlaceholderSize = new Vector2(400f, 300f);
        public Vector2 nameAnchor = new Vector2(0.5f, 0.28f);
        public Vector2 nameSize = new Vector2(760f, 65f);
        public Vector2 availabilityAnchor = new Vector2(0.5f, 0.22f);
        public Vector2 availabilitySize = new Vector2(760f, 50f);
        public Vector2 statusAnchor = new Vector2(0.5f, 0.16f);
        public Vector2 statusSize = new Vector2(760f, 65f);
        public Vector2 controlsAnchor = new Vector2(0.5f, 0.065f);
        public Vector2 controlsSize = new Vector2(800f, 85f);
        public Vector2 backButtonAnchor = new Vector2(0.5f, 0.035f);
        public Vector2 backButtonSize = new Vector2(240f, 64f);
        public float previewScale = 0.65f;
        public float cameraOrthographicSize = 5.8f;
        public float titleFontSize = 40f;
        public float sectionTitleFontSize = 34f;
        public float nameFontSize = 40f;
        public float availabilityFontSize = 28f;
        public float statusFontSize = 25f;
        public float controlsFontSize = 20f;
        public float placeholderFontSize = 100f;
        public float backFontSize = 26f;
    }

    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_FontAsset uiFont;
    [SerializeField] private LayoutSettings layout = new LayoutSettings();
    [SerializeField] private List<CharacterEntry> characterRoster = new List<CharacterEntry>();
    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private Vector3 player1PreviewPosition = new Vector3(-3.2f, -1f, 0f);
    [SerializeField] private Vector3 player2PreviewPosition = new Vector3(3.2f, -1f, 0f);

    private PlayerSelection player1;
    private PlayerSelection player2;
    private Material previewMaterial;
    private bool transitionStarted;

    private void Awake()
    {
        CharacterSelectionSession.Reset();

        if (!ValidateSetup())
        {
            enabled = false;
            return;
        }

        ConfigureCanvas();
        EnsureEventSystem();
        ConfigureCameraAndPreviewMaterial();

        player1 = CreatePlayerSection(
            "Player 1",
            "A: PREVIOUS     D: NEXT\nENTER: CONFIRM / UNLOCK",
            player1PreviewPosition,
            false,
            layout.player1PanelMin,
            layout.player1PanelMax);
        player2 = CreatePlayerSection(
            "Player 2",
            "LEFT ARROW: PREVIOUS     RIGHT ARROW: NEXT\nRIGHT SHIFT: CONFIRM / UNLOCK",
            player2PreviewPosition,
            true,
            layout.player2PanelMin,
            layout.player2PanelMax);

        RefreshPlayer(player1, 1);
        RefreshPlayer(player2, 2);
        CreateBackButton();
    }

    private void Update()
    {
        if (transitionStarted)
        {
            return;
        }

        HandlePlayerInput(
            player1,
            KeyCode.A,
            KeyCode.D,
            KeyCode.Return,
            true,
            1);
        HandlePlayerInput(
            player2,
            KeyCode.LeftArrow,
            KeyCode.RightArrow,
            KeyCode.RightShift,
            false,
            2);

        if (player1.isReady && player2.isReady)
        {
            StartMatch();
        }
    }

    private void OnDestroy()
    {
        if (previewMaterial != null)
        {
            Destroy(previewMaterial);
        }
    }

    private bool ValidateSetup()
    {
        if (canvas == null || layout == null)
        {
            Debug.LogError("CharacterSelectController requires a Canvas and layout configuration.");
            return false;
        }

        if (layout.referenceResolution.x <= 0f || layout.referenceResolution.y <= 0f ||
            layout.previewScale <= 0f || layout.cameraOrthographicSize <= 0f)
        {
            Debug.LogError("CharacterSelectController layout dimensions must be positive.");
            return false;
        }

        if (characterRoster == null || characterRoster.Count == 0)
        {
            Debug.LogError("CharacterSelectController requires at least one roster entry.");
            return false;
        }

        HashSet<string> characterIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (CharacterEntry entry in characterRoster)
        {
            if (entry == null ||
                string.IsNullOrWhiteSpace(entry.characterId) ||
                string.IsNullOrWhiteSpace(entry.displayName) ||
                !characterIds.Add(entry.characterId))
            {
                Debug.LogError("Every roster entry needs a unique ID and a display name.");
                return false;
            }
        }

        if (characterRoster[0].previewSprite == null &&
            characterRoster[0].previewAnimatorController == null)
        {
            Debug.LogWarning(
                "The first roster entry has no preview art or Animator Controller assigned.");
        }

        if (uiFont == null)
        {
            uiFont = TMP_Settings.defaultFontAsset;
        }

        if (uiFont == null)
        {
            Debug.LogError("CharacterSelectController could not find a TextMeshPro font asset.");
            return false;
        }

        return true;
    }

    private void ConfigureCanvas()
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = layout.referenceResolution;
        scaler.matchWidthOrHeight = 0.5f;

        if (canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        CreateImage("Background", canvas.transform, layout.backgroundColor, Vector2.zero,
            Vector2.zero, Vector2.one, Vector2.zero);
        CreateText(
            "Character Select Title",
            "CHOOSE YOUR FIGHTER",
            canvas.transform,
            layout.accentColor,
            layout.titleFontSize,
            TextAlignmentOptions.Center,
            layout.titleAnchor,
            layout.titleSize);
    }

    private void EnsureEventSystem()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
        }

        if (eventSystem.GetComponent<BaseInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
    }

    private void ConfigureCameraAndPreviewMaterial()
    {
        Camera previewCamera = Camera.main;
        if (previewCamera == null)
        {
            GameObject cameraObject = new GameObject("Character Select Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            previewCamera = cameraObject.AddComponent<Camera>();
        }

        previewCamera.orthographic = true;
        previewCamera.orthographicSize = layout.cameraOrthographicSize;
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = layout.backgroundColor;

        Shader spriteShader =
            Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (spriteShader == null)
        {
            spriteShader = Shader.Find("Sprites/Default");
        }

        if (spriteShader != null)
        {
            previewMaterial = new Material(spriteShader);
        }
        else
        {
            Debug.LogWarning("No sprite shader was found for character previews.");
        }
    }

    private PlayerSelection CreatePlayerSection(
        string playerName,
        string controls,
        Vector3 previewPosition,
        bool facesLeft,
        Vector2 panelAnchorMin,
        Vector2 panelAnchorMax)
    {
        GameObject panel = CreateImage(
            playerName + " Section",
            canvas.transform,
            layout.panelColor,
            Vector2.zero,
            Vector2.zero,
            panelAnchorMin,
            panelAnchorMax);

        CreateText(playerName.ToUpperInvariant(), playerName.ToUpperInvariant(),
            panel.transform, layout.accentColor, layout.sectionTitleFontSize,
            TextAlignmentOptions.Center, layout.sectionTitleAnchor, layout.sectionTitleSize);
        TMP_Text nameLabel = CreateText("Selected Character", string.Empty,
            panel.transform, layout.textColor, layout.nameFontSize,
            TextAlignmentOptions.Center, layout.nameAnchor, layout.nameSize);
        TMP_Text availabilityLabel = CreateText("Availability", string.Empty,
            panel.transform, layout.accentColor, layout.availabilityFontSize,
            TextAlignmentOptions.Center, layout.availabilityAnchor, layout.availabilitySize);
        TMP_Text statusLabel = CreateText("Ready Status", string.Empty,
            panel.transform, layout.textColor, layout.statusFontSize,
            TextAlignmentOptions.Center, layout.statusAnchor, layout.statusSize);
        CreateText("Controls", controls, panel.transform, layout.textColor,
            layout.controlsFontSize, TextAlignmentOptions.Center,
            layout.controlsAnchor, layout.controlsSize);
        TMP_Text placeholderLabel = CreateText("Preview Placeholder", "?",
            panel.transform, layout.accentColor, layout.placeholderFontSize,
            TextAlignmentOptions.Center,
            layout.previewPlaceholderAnchor,
            layout.previewPlaceholderSize);

        CharacterSelectionPreview preview = CharacterSelectionPreview.Create(
            playerName + " Character Preview",
            previewPosition,
            facesLeft,
            previewMaterial,
            layout.previewScale);

        return new PlayerSelection
        {
            index = 0,
            isReady = false,
            status = playerName == "Player 1"
                ? "PRESS ENTER TO CONFIRM"
                : "PRESS RIGHT SHIFT TO CONFIRM",
            preview = preview,
            nameLabel = nameLabel,
            availabilityLabel = availabilityLabel,
            statusLabel = statusLabel,
            placeholderLabel = placeholderLabel
        };
    }

    private void HandlePlayerInput(
        PlayerSelection player,
        KeyCode previousKey,
        KeyCode nextKey,
        KeyCode confirmKey,
        bool allowKeypadEnter,
        int playerNumber)
    {
        if (player == null)
        {
            return;
        }

        if (Input.GetKeyDown(previousKey))
        {
            CycleCharacter(player, -1, playerNumber);
        }

        if (Input.GetKeyDown(nextKey))
        {
            CycleCharacter(player, 1, playerNumber);
        }

        bool confirmPressed = Input.GetKeyDown(confirmKey);
        if (allowKeypadEnter)
        {
            confirmPressed |= Input.GetKeyDown(KeyCode.KeypadEnter);
        }

        if (confirmPressed)
        {
            ToggleReady(player, playerNumber);
        }
    }

    private void CycleCharacter(PlayerSelection player, int direction, int playerNumber)
    {
        if (player.isReady)
        {
            return;
        }

        player.index = (player.index + direction + characterRoster.Count)
            % characterRoster.Count;
        player.status = playerNumber == 1
            ? "PRESS ENTER TO CONFIRM"
            : "PRESS RIGHT SHIFT TO CONFIRM";
        RefreshPlayer(player, playerNumber);
    }

    private void ToggleReady(PlayerSelection player, int playerNumber)
    {
        if (player.isReady)
        {
            player.isReady = false;
            player.status = playerNumber == 1
                ? "PRESS ENTER TO CONFIRM"
                : "PRESS RIGHT SHIFT TO CONFIRM";
            RefreshPlayer(player, playerNumber);
            return;
        }

        CharacterEntry selectedCharacter = characterRoster[player.index];
        if (!selectedCharacter.isPlayable)
        {
            player.status = "CHARACTER COMING SOON — SELECT AN AVAILABLE CHARACTER";
            RefreshPlayer(player, playerNumber);
            return;
        }

        player.isReady = true;
        player.status = "READY!";
        RefreshPlayer(player, playerNumber);
    }

    private void RefreshPlayer(PlayerSelection player, int playerNumber)
    {
        CharacterEntry selectedCharacter = characterRoster[player.index];
        player.nameLabel.text = selectedCharacter.displayName;
        player.availabilityLabel.text =
            selectedCharacter.isPlayable ? "AVAILABLE" : "COMING SOON";
        player.availabilityLabel.color =
            selectedCharacter.isPlayable ? layout.readyColor : layout.accentColor;
        player.statusLabel.text = player.status;
        player.statusLabel.color =
            player.isReady ? layout.readyColor : layout.textColor;
        player.placeholderLabel.gameObject.SetActive(
            selectedCharacter.previewSprite == null &&
            selectedCharacter.previewAnimatorController == null);
        player.preview.Display(
            selectedCharacter.previewSprite,
            selectedCharacter.previewAnimatorController);
    }

    private void StartMatch()
    {
        CharacterEntry player1Character = characterRoster[player1.index];
        CharacterEntry player2Character = characterRoster[player2.index];
        if (!player1.isReady || !player2.isReady ||
            !player1Character.isPlayable || !player2Character.isPlayable)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(battleSceneName))
        {
            Debug.LogError("Battle scene is missing from the Build Profile: " + battleSceneName);
            return;
        }

        transitionStarted = true;
        CharacterSelectionSession.SetConfirmedCharacters(
            player1Character.characterId,
            player2Character.characterId);
        SceneManager.LoadScene(battleSceneName);
    }

    private void CreateBackButton()
    {
        GameObject buttonObject = new GameObject(
            "Back Button",
            typeof(RectTransform),
            typeof(Image),
            typeof(Button));
        buttonObject.transform.SetParent(canvas.transform, false);

        RectTransform rect = (RectTransform)buttonObject.transform;
        rect.anchorMin = layout.backButtonAnchor;
        rect.anchorMax = layout.backButtonAnchor;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = layout.backButtonSize;

        Image image = buttonObject.GetComponent<Image>();
        image.color = layout.accentColor;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(ReturnToMainMenu);

        CreateText("Back Label", "BACK", buttonObject.transform,
            layout.backgroundColor, layout.backFontSize,
            TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), Vector2.zero)
            .rectTransform.anchorMin = Vector2.zero;
        RectTransform labelRect =
            buttonObject.transform.GetChild(0).GetComponent<RectTransform>();
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;
    }

    private void ReturnToMainMenu()
    {
        CharacterSelectionSession.Reset();
        LoadScene(mainMenuSceneName);
    }

    private void LoadScene(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("Scene is missing from the Build Profile: " + sceneName);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private GameObject CreateImage(
        string objectName,
        Transform parent,
        Color color,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rect = (RectTransform)imageObject.transform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return imageObject;
    }

    private TMP_Text CreateText(
        string objectName,
        string value,
        Transform parent,
        Color color,
        float fontSize,
        TextAlignmentOptions alignment,
        Vector2 anchor,
        Vector2 size)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = (RectTransform)textObject.transform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.font = uiFont;
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(14f, fontSize - 8f);
        text.fontSizeMax = fontSize;
        text.textWrappingMode = TMPro.TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }
}

public static class CharacterSelectionSession
{
    public static bool HasConfirmedCharacters { get; private set; }
    public static string Player1CharacterId { get; private set; }
    public static string Player2CharacterId { get; private set; }

    public static void SetConfirmedCharacters(string player1Id, string player2Id)
    {
        Player1CharacterId = player1Id;
        Player2CharacterId = player2Id;
        HasConfirmedCharacters = true;
    }

    public static void Reset()
    {
        Player1CharacterId = null;
        Player2CharacterId = null;
        HasConfirmedCharacters = false;
    }
}
