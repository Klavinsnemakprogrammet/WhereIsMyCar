using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using TMPro;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAds = false;
    public RewardedAds rewardedAds;
    [SerializeField] bool turnOffRewardedAds = false;
    public BannerAd bannerAd;
    [SerializeField] bool turnOffBannerAds = false;

    public static AdManager Instance { get; private set; }

    private bool adsLoaded;
    private bool hanoiInterstitialPending;
    private HanoiTower hanoiRewardTower;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false;
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        adsInitializer ??= GetComponent<AdsInitializer>();
        interstitialAd ??= GetComponent<InterstitialAd>();
        rewardedAds ??= GetComponent<RewardedAds>();
        bannerAd ??= GetComponent<BannerAd>();

        if (adsInitializer != null)
        {
            adsInitializer.OnAdsInitialized += HandleAdsInitialized;
        }

        if (interstitialAd != null)
        {
            interstitialAd.OnInterstitialAdReady += HandleInterstitialReady;
        }

    }

    private void Start()
    {
        if (Advertisement.isInitialized)
        {
            HandleAdsInitialized();
        }
    }

    private void HandleAdsInitialized()
    {
        if (adsLoaded)
        {
            return;
        }

        adsLoaded = true;

        if (!turnOffInterstitialAds && interstitialAd != null)
        {
            interstitialAd.LoadAd();
        }

        if (!turnOffRewardedAds && rewardedAds != null)
        {
            rewardedAds.LoadAd();
        }

        if (!turnOffBannerAds && bannerAd != null)
        {
            bannerAd.LoadBanner();
        }
    }

    private void HandleInterstitialReady()
    {
        if (hanoiInterstitialPending)
        {
            hanoiInterstitialPending = false;
            if (SceneManager.GetActiveScene().name == "HanoiScene")
                interstitialAd.ShowAd();
        }

        Debug.Log("Interstitial ad is ready to be shown!");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hanoiInterstitialPending = false;
        if (scene.name == "HanoiScene")
            ShowInterstitialOnHanoiLaunch();

        if (interstitialAd != null)
        {
            Button interstitialButton = FindButton("InterstitialAdButton");
            if (interstitialButton != null)
            {
                interstitialAd.SetButton(interstitialButton);
            }
        }

        rewardedAds = FindSceneRewardedAds(scene) ?? rewardedAds;
        rewardedAds ??= FindFirstObjectByType<RewardedAds>();
        if (rewardedAds != null)
        {
            Button rewardedAdButton = scene.name == "HanoiScene" && !turnOffRewardedAds
                ? CreateHanoiRewardButton(scene)
                : FindButton("RewardedAdButton");
            if (rewardedAdButton != null)
            {
                rewardedAds.SetButton(rewardedAdButton);
            }

            if (scene.name == "HanoiScene")
            {
                hanoiRewardTower = FindFirstObjectByType<HanoiTower>();
                if (hanoiRewardTower != null)
                    rewardedAds.OnRewardedAdCompleted += ApplyHanoiReward;
                else
                    Debug.LogError("Could not find HanoiTower for the rewarded ad move discount.", this);
            }

            if (adsLoaded && !turnOffRewardedAds)
            {
                rewardedAds.LoadAd();
            }
        }

        if (bannerAd != null)
        {
            Button bannerAdButton = FindButton("BannerAdButton");
            if (bannerAdButton != null)
            {
                bannerAd.SetButton(bannerAdButton);
            }
        }
    }

    private void ShowInterstitialOnHanoiLaunch()
    {
        if (turnOffInterstitialAds || interstitialAd == null)
            return;

        if (interstitialAd.isReady)
        {
            interstitialAd.ShowAd();
            return;
        }

        hanoiInterstitialPending = true;
        interstitialAd.LoadAd();
    }

    private void ApplyHanoiReward()
    {
        if (hanoiRewardTower != null)
            hanoiRewardTower.ApplyRewardedMoveDiscount(2);
    }

    private static RewardedAds FindSceneRewardedAds(Scene scene)
    {
        RewardedAds[] sceneAds = FindObjectsByType<RewardedAds>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (RewardedAds sceneAd in sceneAds)
        {
            if (sceneAd.gameObject.scene == scene)
                return sceneAd;
        }

        return null;
    }

    private Button CreateHanoiRewardButton(Scene scene)
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Canvas sceneCanvas = null;
        foreach (Canvas candidate in canvases)
        {
            if (candidate.gameObject.scene == scene && candidate.rootCanvas == candidate)
            {
                sceneCanvas = candidate;
                break;
            }
        }

        if (sceneCanvas == null)
        {
            Debug.LogError("Could not find a root Canvas for the Hanoi rewarded-ad button.", this);
            return null;
        }

        GameObject buttonObject = new GameObject(
            "HanoiRewardedAdButton",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.SetParent(sceneCanvas.transform, false);
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0f);
        buttonRect.anchoredPosition = new Vector2(0f, 120f);
        buttonRect.sizeDelta = new Vector2(340f, 76f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.72f, 0.1f, 0.04f, 0.96f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonImage;
        button.transition = Selectable.Transition.None;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        button.interactable = false;

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font == null)
        {
            Debug.LogError("Could not load the TextMeshPro font for the Hanoi rewarded-ad button.", this);
            Destroy(buttonObject);
            return null;
        }

        GameObject labelObject = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(buttonRect, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8f, 4f);
        labelRect.offsetMax = new Vector2(-8f, -4f);

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.font = font;
        label.text = "WATCH AD - REMOVE 2 MOVES";
        label.fontSize = 26f;
        label.enableAutoSizing = true;
        label.fontSizeMin = 18f;
        label.fontSizeMax = 26f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        return button;
    }

    private static Button FindButton(string tag)
    {
        GameObject buttonObject = GameObject.FindGameObjectWithTag(tag);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }
}
