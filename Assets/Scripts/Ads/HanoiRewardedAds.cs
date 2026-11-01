using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class HanoiRewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] private string androidAdUnitId = "Rewarded_Android";
    [SerializeField] private Button rewardedAdButton;
    [SerializeField] private HanoiTower hanoiTower;
    [SerializeField, Min(1)] private int movesToSubtract = 2;
    [SerializeField, Min(1f)] private float retryDelay = 10f;

    private bool adLoaded;
    private bool loadingAd;
    private bool showingAd;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        hanoiTower ??= FindFirstObjectByType<HanoiTower>();
        if (hanoiTower == null)
            Debug.LogError("HanoiRewardedAds could not find a HanoiTower in the scene.", this);

        if (rewardedAdButton != null)
            SetButton(rewardedAdButton);
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Cannot load the Hanoi rewarded ad before Unity Ads is initialized.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(androidAdUnitId))
        {
            Debug.LogError("HanoiRewardedAds needs a valid ad unit ID.", this);
            return;
        }

        if (loadingAd || adLoaded || showingAd)
            return;

        loadingAd = true;
        adLoaded = false;
        if (rewardedAdButton != null)
            rewardedAdButton.interactable = false;

        Advertisement.Load(androidAdUnitId, this);
    }

    public void ShowAd()
    {
        if (!adLoaded || showingAd)
        {
            Debug.LogWarning("The Hanoi rewarded ad is not ready to show.", this);
            return;
        }

        showingAd = true;
        loadingAd = false;
        adLoaded = false;
        previousTimeScale = Time.timeScale;
        if (rewardedAdButton != null)
            rewardedAdButton.interactable = false;

        Advertisement.Show(androidAdUnitId, this);
    }

    public void SetButton(Button button)
    {
        if (button == null)
        {
            Debug.LogError("HanoiRewardedAds needs a rewarded-ad Button.", this);
            return;
        }

        if (rewardedAdButton != null)
            rewardedAdButton.onClick.RemoveListener(ShowAd);

        rewardedAdButton = button;
        rewardedAdButton.onClick.AddListener(ShowAd);
        rewardedAdButton.interactable = adLoaded;
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId != androidAdUnitId)
            return;

        loadingAd = false;
        adLoaded = true;
        if (rewardedAdButton != null)
            rewardedAdButton.interactable = true;
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        if (placementId != androidAdUnitId)
            return;

        loadingAd = false;
        adLoaded = false;
        Debug.LogError($"Failed to load Hanoi rewarded ad: {error} - {message}", this);
        StartCoroutine(RetryLoadAd());
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        if (placementId != androidAdUnitId)
            return;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowClick(string placementId)
    {
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        if (placementId != androidAdUnitId)
            return;

        Debug.LogError($"Failed to show Hanoi rewarded ad: {error} - {message}", this);
        FinishShowingAd();
        StartCoroutine(RetryLoadAd());
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId != androidAdUnitId)
            return;

        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            if (hanoiTower == null)
            {
                Debug.LogError("Cannot grant the Hanoi ad reward because HanoiTower is missing.", this);
            }
            else
            {
                hanoiTower.ApplyRewardedMoveDiscount(movesToSubtract);
            }
        }

        FinishShowingAd();
        StartCoroutine(RetryLoadAd());
    }

    private void FinishShowingAd()
    {
        Time.timeScale = previousTimeScale;
        showingAd = false;
        if (rewardedAdButton != null)
            rewardedAdButton.interactable = false;
    }

    private IEnumerator RetryLoadAd()
    {
        yield return new WaitForSecondsRealtime(retryDelay);
        LoadAd();
    }

    private void OnDestroy()
    {
        if (showingAd)
            Time.timeScale = previousTimeScale;
        if (rewardedAdButton != null)
            rewardedAdButton.onClick.RemoveListener(ShowAd);
    }
}
