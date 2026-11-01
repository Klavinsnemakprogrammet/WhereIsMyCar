using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Advertisements;

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

    private bool firstAdShown;
    private bool adsLoaded;


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
        if (!firstAdShown)
        {
            Debug.Log("Showing interstitial ad for the first time!");
            firstAdShown = true;
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
        if (interstitialAd != null)
        {
            Button interstitialButton = FindButton("InterstitialAdButton");
            if (interstitialButton != null)
            {
                interstitialAd.SetButton(interstitialButton);
            }
        }

        rewardedAds ??= FindFirstObjectByType<RewardedAds>();
        if (rewardedAds != null)
        {
            Button rewardedAdButton = FindButton("RewardedAdButton");
            if (rewardedAdButton != null)
            {
                rewardedAds.SetButton(rewardedAdButton);
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

    private static Button FindButton(string tag)
    {
        GameObject buttonObject = GameObject.FindGameObjectWithTag(tag);
        return buttonObject != null ? buttonObject.GetComponent<Button>() : null;
    }
}
