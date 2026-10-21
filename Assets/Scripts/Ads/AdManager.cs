using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAds = false;

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

    }
    // velak papildinas prieks parejam reklamam
    private void HandleInterstitialReady()
    {
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

            // velak papildinas ar parejam pogam 

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
