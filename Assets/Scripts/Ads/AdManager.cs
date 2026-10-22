using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAds = false;
    private bool firstAdShown = false;
    public RewardedAds rewardedAds;
    [SerializeField] bool turnOffRewardedAds = false;
    public BannerAd bannerAd;
    [SerializeField] bool turnOffBannerAds = false;

    public static AdManager Instance { get; private set; }

    private bool firstSceneLoaded = false;


    // Turpināsim...
    private void Awake()
    {
        if(adsInitializer == null)
        {
            adsInitializer = FindObjectOfType<AdsInitializer>();
        }
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        adsInitializer.OnAdsInitialized += HandleAdsInitialized; 


       
        
    }
    private void HandleAdsInitialized()
    {
        if (!turnOffInterstitialAds)
        {
            interstitialAd.OnInterstitialAdReady += HandleInterstitialReady;
            interstitialAd.LoadAd();
        }
        if(!turnOffRewardedAds)
        {
            rewardedAds.LoadAd();
        }
        if(!turnOffBannerAds)
        {
            bannerAd.LoadBanner();
        }

    }
    // velak papildinas prieks parejam reklamam
    private void HandleInterstitialReady()
    {
        if(!firstAdShown)
        {
            Debug.Log("Showing interstitial ad for the first time!");
            interstitialAd.ShowAd();
            firstAdShown = true;
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
        if (interstitialAd == null)
            interstitialAd = FindFirstObjectByType<InterstitialAd>();

            Button interstitalButton = GameObject.FindGameObjectWithTag("InterstitialAdButton").GetComponent<Button>();
            if(interstitalButton != null && interstitalButton != null)
            {
                interstitialAd.SetButton(interstitalButton);
            }

            if(rewardedAds == null)
                rewardedAds = FindFirstObjectByType<RewardedAds>();

            Button rewardedAdButton = GameObject.FindGameObjectWithTag("RewardedAdButton").GetComponent<Button>();    
            if(rewardedAdButton != null && rewardedAds != null)
            {
                rewardedAds.SetButton(rewardedAdButton);
            }
            Button bannerAdButton = GameObject.FindGameObjectWithTag("BannerAdButton").GetComponent<Button>();    
            if(bannerAdButton != null && bannerAd != null)
            {
                bannerAd.SetButton(bannerAdButton);
            }
            

            if(!firstSceneLoaded)
            {
                firstSceneLoaded = true;
                Debug.Log("First scene loaded!");
                return;
            }

            Debug.Log("Scene relaoded");
            HandleAdsInitialized();
        
    }


}
