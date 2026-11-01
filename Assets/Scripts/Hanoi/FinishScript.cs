using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinishScript : MonoBehaviour
{
    [SerializeField] private HanoiTower hanoiTower;
    [SerializeField] private Canvas canvas;
    [Tooltip("Assign the one-star, two-star, and three-star victory panels in that order.")]
    [SerializeField] private Sprite[] victoryScreens = new Sprite[3];
    [SerializeField] private Sprite homeButtonSprite;
    [SerializeField] private Sprite restartButtonSprite;

    private GameObject overlay;
    private Image victoryImage;
    private TMP_Text elapsedTimeText;
    private TMP_FontAsset elapsedTimeFont;
    private RectTransform homeButtonRect;
    private RectTransform restartButtonRect;
    private float startTime;

    private void Awake()
    {
        startTime = Time.time;

        hanoiTower ??= GetComponent<HanoiTower>();
        if (hanoiTower == null)
        {
            Debug.LogError("FinishScript needs a HanoiTower reference.", this);
            enabled = false;
            return;
        }

        if (canvas == null || canvas.GetComponent<GraphicRaycaster>() == null)
        {
            Debug.LogError("FinishScript needs a Canvas with a GraphicRaycaster.", this);
            enabled = false;
            return;
        }

        if (victoryScreens == null || victoryScreens.Length != 3)
        {
            Debug.LogError("FinishScript needs exactly three victory panel sprites, ordered from one to three stars.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < victoryScreens.Length; i++)
        {
            if (victoryScreens[i] == null)
            {
                Debug.LogError($"FinishScript victory panel sprite {i} is not assigned.", this);
                enabled = false;
                return;
            }
        }

        if (homeButtonSprite == null || restartButtonSprite == null)
        {
            Debug.LogError("FinishScript needs home and restart button sprites assigned.", this);
            enabled = false;
            return;
        }

        elapsedTimeFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (elapsedTimeFont == null)
        {
            Debug.LogError("FinishScript could not load the LiberationSans SDF TextMeshPro font.", this);
            enabled = false;
            return;
        }

        CreateOverlay();
        hanoiTower.onWin.AddListener(ShowVictory);
    }

    private void OnDestroy()
    {
        if (hanoiTower != null && hanoiTower.onWin != null)
            hanoiTower.onWin.RemoveListener(ShowVictory);
    }

    private void CreateOverlay()
    {
        overlay = new GameObject("HanoiVictoryOverlay", typeof(RectTransform), typeof(Image));
        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.SetParent(canvas.transform, false);
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlay.GetComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.6f);
        overlayImage.raycastTarget = true;

        GameObject panel = new GameObject("VictoryPanel", typeof(RectTransform), typeof(Image));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.SetParent(overlayRect, false);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);

        victoryImage = panel.GetComponent<Image>();
        victoryImage.preserveAspect = true;
        victoryImage.raycastTarget = true;

        GameObject timeObject = new GameObject(
            "ElapsedTime",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        RectTransform timeRect = timeObject.GetComponent<RectTransform>();
        timeRect.SetParent(panelRect, false);
        timeRect.anchorMin = new Vector2(0.46f, 0.12f);
        timeRect.anchorMax = new Vector2(0.96f, 0.28f);
        timeRect.offsetMin = Vector2.zero;
        timeRect.offsetMax = Vector2.zero;

        elapsedTimeText = timeObject.GetComponent<TextMeshProUGUI>();
        elapsedTimeText.font = elapsedTimeFont;
        elapsedTimeText.fontSize = 42f;
        elapsedTimeText.enableAutoSizing = true;
        elapsedTimeText.fontSizeMin = 24f;
        elapsedTimeText.fontSizeMax = 42f;
        elapsedTimeText.alignment = TextAlignmentOptions.MidlineLeft;
        elapsedTimeText.color = Color.white;
        elapsedTimeText.outlineColor = new Color(0.82f, 0.55f, 0.19f);
        elapsedTimeText.outlineWidth = 0.6f;
        Material timeMaterial = elapsedTimeText.fontMaterial;
        timeMaterial.SetColor(ShaderUtilities.ID_FaceColor, Color.white);
        timeMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0.82f, 0.55f, 0.19f));
        elapsedTimeText.fontStyle = FontStyles.Bold;
        elapsedTimeText.raycastTarget = false;

        homeButtonRect = CreateButton(panelRect, "HomeButton", homeButtonSprite, new Vector2(0.12f, 0.1f), GoHome);
        restartButtonRect = CreateButton(panelRect, "RestartButton", restartButtonSprite, new Vector2(0.88f, 0.1f), Restart);
        overlay.SetActive(false);
    }

    private RectTransform CreateButton(Transform parent, string objectName, Sprite sprite, Vector2 anchor, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.SetParent(parent, false);
        buttonRect.anchorMin = anchor;
        buttonRect.anchorMax = anchor;
        buttonRect.sizeDelta = new Vector2(52f, 52f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.sprite = sprite;
        buttonImage.preserveAspect = true;
        buttonImage.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonImage;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(onClick);
        return buttonRect;
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private static void GoHome()
    {
        SceneManager.LoadScene("StartScene");
    }

    private void ShowVictory()
    {
        int minimumMoves = (1 << hanoiTower.DiscCount) - 1;
        int stars = hanoiTower.Moves == minimumMoves
            ? 3
            : hanoiTower.Moves <= minimumMoves * 2
                ? 2
                : 1;

        Sprite screen = victoryScreens[stars - 1];
        victoryImage.sprite = screen;
        elapsedTimeText.text = FormatTime(Time.time - startTime);

        RectTransform panelRect = victoryImage.rectTransform;
        RectTransform canvasRect = canvas.transform as RectTransform;
        float width = screen.rect.width;
        float height = screen.rect.height;
        float fit = Mathf.Min(
            canvasRect.rect.width * 0.9f / width,
            canvasRect.rect.height * 0.95f / height);
        panelRect.sizeDelta = new Vector2(width * fit, height * fit);
        homeButtonRect.sizeDelta = new Vector2(52f * fit, 52f * fit);
        restartButtonRect.sizeDelta = new Vector2(52f * fit, 52f * fit);
        elapsedTimeText.fontSize = 42f * fit;
        elapsedTimeText.fontSizeMin = 24f * fit;
        elapsedTimeText.fontSizeMax = 42f * fit;

        overlay.transform.SetAsLastSibling();
        overlay.SetActive(true);
    }

    private static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.FloorToInt(seconds);
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;
        return $"{minutes:00}:{remainingSeconds:00}";
    }
}
