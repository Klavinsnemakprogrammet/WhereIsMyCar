using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Advertisements;

public class BannerAd : MonoBehaviour
{
    [SerializeField] string _AndroidAdUnitId = "Banner_Android";
    string _adUnitId;
    [SerializeField] Button _bannerButton;
    public bool isBannerVisable = false;

    [SerializeField] BannerPosition bannerPosition = BannerPosition.BOTTOM_CENTER;
    void Awake()
    {
        _adUnitId = _AndroidAdUnitId;
        Advertisement.Banner.SetPosition(bannerPosition);

    }
    public void LoadBanner()
    {
        if(!Advertisement.isInitialized)
        {
            Debug.Log("Tried to load banner ad before Unity ads was initialized");
            return;
        }

        BannerLoadOptions bannerOptions = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };
        Advertisement.Banner.Load(_adUnitId, bannerOptions);
    }
    void OnBannerLoaded()
    {
        Debug.Log("Banner ad loaded successfully.");
        CancelInvoke(nameof(LoadBanner));
        if (_bannerButton != null)
        {
            _bannerButton.interactable = true;
        }

        if (AdManager.Instance == null ||
            AdManager.Instance.interstitialAd == null ||
            !AdManager.Instance.interstitialAd.isShowing)
        {
            ShowBannerAd();
        }
    }
    void OnBannerError(string message)
    {
        Debug.LogWarning($"Banner ad failed to load: {message}");
        CancelInvoke(nameof(LoadBanner));
        Invoke(nameof(LoadBanner), 5f);
    }
    public void ShowBannerAd()
    {
        if (isBannerVisable)
        {
            HideBannerAd();
        }

        else
        {
            BannerOptions bannerOptions = new BannerOptions
            {
                clickCallback = OnBannerClicked,
                hideCallback = OnBannerHidden,
                showCallback = OnBannerShown
            };
            Advertisement.Banner.Show(_adUnitId, bannerOptions);
        }
    }
    void OnBannerClicked()
    {
        Debug.Log("Banner ad clicked."+_adUnitId);
    }
    void OnBannerHidden()
    {
        Debug.Log("Banner ad hidden."+_adUnitId);
        isBannerVisable = false;
    }
    void OnBannerShown()
    {
        Debug.Log("Banner ad shown."+_adUnitId);
        isBannerVisable = true;
    }
    public void HideBannerAd()
    {
        Advertisement.Banner.Hide();
        isBannerVisable = false;
    }

    public void SetButton(Button button)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowBannerAd);
        _bannerButton = button;
        _bannerButton.interactable = false;
    }

}
