using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
public class ResponsiveCanvas : MonoBehaviour
{
    [SerializeField] Vector2 referenceResolution = new Vector2(1366, 768);
    [SerializeField, Range(0f, 1f)] float matchWidthOrHeight = 0.5f;
    [SerializeField] RectTransform background;
    [SerializeField] RectTransform logo;
    [SerializeField] RectTransform driftCar;
    [SerializeField] RectTransform startButton;
    [SerializeField] RectTransform exitButton;
    [SerializeField] RectTransform Hanoja;

    RectTransform canvasRect;
    Vector2 lastCanvasSize;
    bool lastPortrait;

    void Awake()
    {
        GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = matchWidthOrHeight;

        canvasRect = GetComponent<RectTransform>();
        FindMissingReferences();

        if (background == null)
        {
            Debug.LogError("ResponsiveCanvas needs a child named 'background' with an Image component.", this);
            enabled = false;
            return;
        }

        Image backgroundImage = background.GetComponent<Image>();
        if (backgroundImage == null || backgroundImage.sprite == null)
        {
            Debug.LogError("The background object needs an Image with a sprite assigned.", background);
            enabled = false;
            return;
        }

        backgroundImage.raycastTarget = false;
        background.SetAsFirstSibling();
        FitBackground();
        ArrangeMenu();
    }

    void LateUpdate()
    {
        bool portrait = Screen.width < Screen.height;
        if (canvasRect.rect.size == lastCanvasSize && portrait == lastPortrait)
        {
            return;
        }

        FitBackground();
        ArrangeMenu();
    }

    void FindMissingReferences()
    {
        background ??= FindChildRect("background");
        logo ??= FindChildRect("Image");
        driftCar ??= FindChildRect("DriftCar");
        startButton ??= FindChildRect("StartButton");
        exitButton ??= FindChildRect("ExitButton");
        Hanoja ??= FindChildRect("Hanoja");
    }

    RectTransform FindChildRect(string objectName)
    {
        Transform child = transform.Find(objectName);
        return child != null ? child.GetComponent<RectTransform>() : null;
    }

    void FitBackground()
    {
        RectTransform parent = background.parent as RectTransform;
        Image image = background.GetComponent<Image>();
        if (parent == null || image.sprite == null)
        {
            return;
        }

        Vector2 parentSize = parent.rect.size;
        if (parentSize.x <= 0f || parentSize.y <= 0f)
        {
            return;
        }

        float imageAspect = image.sprite.rect.width / image.sprite.rect.height;
        float parentAspect = parentSize.x / parentSize.y;

        background.anchorMin = new Vector2(0.5f, 0.5f);
        background.anchorMax = new Vector2(0.5f, 0.5f);
        background.pivot = new Vector2(0.5f, 0.5f);
        background.anchoredPosition = Vector2.zero;
        background.localScale = Vector3.one;

        if (imageAspect > parentAspect)
        {
            background.sizeDelta = new Vector2(parentSize.y * imageAspect, parentSize.y);
        }
        else
        {
            background.sizeDelta = new Vector2(parentSize.x, parentSize.x / imageAspect);
        }

        lastCanvasSize = canvasRect.rect.size;
    }

    void ArrangeMenu()
    {
        Vector2 canvasSize = canvasRect.rect.size;
        if (canvasSize.x <= 0f || canvasSize.y <= 0f)
        {
            return;
        }

        lastPortrait = Screen.width < Screen.height;
        if (lastPortrait)
        {
            SetLayout(logo, new Vector2(0.5f, 0.87f), new Vector2(canvasSize.x * 0.7f, canvasSize.y * 0.2f));
            SetLayout(driftCar, new Vector2(0.5f, 0.55f), new Vector2(canvasSize.x * 0.72f, canvasSize.y * 0.34f));
            SetLayout(startButton, new Vector2(0.5f, 0.23f), new Vector2(canvasSize.x * 0.68f, canvasSize.y * 0.1f));
            SetLayout(Hanoja, new Vector2(0.5f, 0.34f), new Vector2(canvasSize.x * 0.68f, canvasSize.y * 0.1f));
            SetLayout(exitButton, new Vector2(0.5f, 0.11f), new Vector2(canvasSize.x * 0.68f, canvasSize.y * 0.1f));
        }
        else
        {
            SetLayout(logo, new Vector2(0.2f, 0.75f), new Vector2(canvasSize.x * 0.28f, canvasSize.y * 0.55f));
            SetLayout(driftCar, new Vector2(0.43f, 0.52f), new Vector2(canvasSize.x * 0.32f, canvasSize.y * 0.5f));
            SetLayout(startButton, new Vector2(0.82f, 0.43f), new Vector2(canvasSize.x * 0.23f, canvasSize.y * 0.13f));
            SetLayout(exitButton, new Vector2(0.82f, 0.28f), new Vector2(canvasSize.x * 0.23f, canvasSize.y * 0.13f));
            SetLayout(Hanoja, new Vector2(0.82f, 0.58f), new Vector2(canvasSize.x * 0.23f, canvasSize.y * 0.13f));
        }
    }

    static void SetLayout(RectTransform element, Vector2 anchor, Vector2 size)
    {
        if (element == null)
        {
            return;
        }

        element.anchorMin = anchor;
        element.anchorMax = anchor;
        element.anchoredPosition = Vector2.zero;
        element.sizeDelta = size;
        element.localScale = Vector3.one;
    }
}
