using UnityEngine;

public class AdManager : MonoBehaviour
{
    public AdsInitializer adsInitializer;
    public InterstitialAd interstitialAd;
    [SerializeField] bool turnOffInterstitialAds = false;
    private bool firstAdShown = false;

    public static AdManager Instance { get; private set; }

    // Turpināsim...
    private void Awake()
    {
        
    }

}
